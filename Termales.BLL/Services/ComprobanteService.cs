using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs.Comprobante;

using Termales.Common.Settings;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Enums;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class ComprobanteService : IComprobanteService
{
    private readonly IUnitOfWork _uow;
    private readonly HttpClient  _nubefactHttp;
    private readonly NubefactSettings _cfg;
    private readonly IHttpContextAccessor _accessor;
    private readonly ISolicitudAnulacionService _solicitudes;
    private readonly IReciboPrinterService _reciboPrinter;

    public ComprobanteService(
        IUnitOfWork uow,
        IHttpClientFactory httpFactory,
        IOptions<NubefactSettings> cfg,
        IHttpContextAccessor accessor,
        ISolicitudAnulacionService solicitudes,
        IReciboPrinterService reciboPrinter)
    {
        _uow          = uow;
        _nubefactHttp = httpFactory.CreateClient("Nubefact");
        _cfg          = cfg.Value;
        _accessor     = accessor;
        _solicitudes  = solicitudes;
        _reciboPrinter = reciboPrinter;
    }

    private string ObtenerCajero() =>
        _accessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Name)?.Value ?? "---";

    // ── Comedor ───────────────────────────────────────────────────────
    public async Task<ApiResponse<ComprobanteResultadoDto>> GenerarComprobanteComedor(int ordenId, GenerarComprobanteComedorDto dto)
    {
        var orden = await _uow.Ordenes.ObtenerConDetallesAsync(ordenId);
        if (orden is null)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Orden no encontrada");
        if (orden.Estado != EstadoOrden.ParaCobrar)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("La orden no está lista para cobrar");
        if (dto.OrdenDetalleIds is null || dto.OrdenDetalleIds.Count == 0)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Debe seleccionar al menos un plato a cobrar");

        var idsUnicos = dto.OrdenDetalleIds.Distinct().ToList();
        var detallesACobrar = orden.Detalles.Where(d => idsUnicos.Contains(d.OrdenDetalleId)).ToList();
        if (detallesACobrar.Count != idsUnicos.Count)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Alguna línea seleccionada no pertenece a esta orden");
        if (detallesACobrar.Any(d => d.Estado == EstadoOrdenDetalle.Cancelado))
            return ApiResponse<ComprobanteResultadoDto>.Fallido("No se puede cobrar una línea cancelada");
        if (detallesACobrar.Any(d => d.ComprobanteId is not null))
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Alguna línea seleccionada ya fue cobrada");

        var items = detallesACobrar.Select(d =>
        {
            var valorUnit = Math.Round(d.PrecioUnitario / 1.18m, 2);
            var subtotal  = Math.Round(valorUnit * d.Cantidad, 2);
            return new ItemComprobante
            {
                Descripcion    = d.ItemMenu?.Nombre ?? d.Producto?.Nombre ?? "Producto",
                Cantidad       = d.Cantidad,
                ValorUnitario  = valorUnit,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal       = subtotal,
                Igv            = Math.Round(d.PrecioUnitario * d.Cantidad - subtotal, 2),
                Total          = Math.Round(d.PrecioUnitario * d.Cantidad, 2),
            };
        }).ToList();

        var monto = detallesACobrar.Sum(d => d.Subtotal);

        var resultado = await Emitir(dto, monto, items, "comedor", ordenId);
        if (!resultado.Exito) return resultado;

        foreach (var detalle in detallesACobrar)
        {
            detalle.ComprobanteId = resultado.Data!.ComprobanteId;
            await _uow.Ordenes.ActualizarAsync(orden);
        }

        // Solo se cierra la mesa cuando TODAS las líneas no canceladas ya
        // tienen comprobante — mientras queden platos sin cobrar, la orden
        // sigue "para cobrar" para que el resto del grupo pague después.
        var quedanPendientes = orden.Detalles.Any(d =>
            d.Estado != EstadoOrdenDetalle.Cancelado && d.ComprobanteId is null);

        if (!quedanPendientes)
        {
            orden.Estado      = EstadoOrden.Pagada;
            orden.FechaCierre = DateTime.UtcNow;
            if (orden.MesaId is int mesaId)
            {
                var mesa = await _uow.Mesas.ObtenerConSecundariasAsync(mesaId);
                if (mesa is not null)
                {
                    mesa.Estado = EstadoMesa.Disponible;
                    await _uow.Mesas.ActualizarAsync(mesa);
                    foreach (var secundaria in mesa.MesasSecundarias)
                    {
                        secundaria.MesaPrincipalId = null;
                        secundaria.Estado = EstadoMesa.Disponible;
                        await _uow.Mesas.ActualizarAsync(secundaria);
                    }
                }
            }
        }

        await _uow.GuardarCambiosAsync();

        return resultado;
    }

    // ── Baño termal: boleto de precio fijo/combo, sin control de ocupación ──
    public async Task<ApiResponse<ComprobanteResultadoDto>> GenerarComprobanteBanio(GenerarComprobanteBanioDto dto)
    {
        if (dto.TipoServicioIds is null || dto.TipoServicioIds.Count == 0)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Debe seleccionar al menos un servicio");
        if (dto.CantidadPersonas <= 0)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("La cantidad de personas debe ser mayor a 0");

        var idsUnicos = dto.TipoServicioIds.Distinct().OrderBy(x => x).ToList();
        var tipos = (await _uow.TiposServicio.BuscarAsync(t => idsUnicos.Contains(t.TipoServicioId) && t.Activo)).ToList();
        if (tipos.Count != idsUnicos.Count)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Alguno de los servicios seleccionados no existe o está inactivo");

        var paquetes = await _uow.PaquetesBanio.ObtenerActivosConTiposAsync();
        var paqueteCoincidente = paquetes.FirstOrDefault(p =>
            p.Tipos.Select(t => t.TipoServicioId).OrderBy(x => x).SequenceEqual(idsUnicos));

        decimal precioUnitario;
        string descripcion;
        if (paqueteCoincidente is not null)
        {
            precioUnitario = paqueteCoincidente.Precio;
            descripcion    = paqueteCoincidente.Nombre;
        }
        else
        {
            precioUnitario = tipos.Sum(t => t.PrecioPorPersona);
            descripcion    = string.Join(" + ", tipos.Select(t => t.Nombre));
        }

        var monto     = Math.Round(precioUnitario * dto.CantidadPersonas, 2);
        var valorUnit = Math.Round(monto / 1.18m, 2);

        var items = new List<ItemComprobante> { new()
        {
            Descripcion    = $"{descripcion} ({dto.CantidadPersonas} pers.)",
            Cantidad       = 1,
            ValorUnitario  = valorUnit,
            PrecioUnitario = monto,
            Subtotal       = valorUnit,
            Igv            = Math.Round(monto - valorUnit, 2),
            Total          = monto,
        }};

        // Sin piscinaId ni control de ocupación: es un boleto plano, no una
        // asignación de un baño/piscina físico específico.
        var resultado = await Emitir(dto, monto, items, "banio", 0);

        // Cuando la venta cubre un combo (más de un área), la boleta trae un
        // solo ítem — se imprime un ticket aparte de referencia para poder
        // controlar el ingreso a cada área por separado.
        if (resultado.Exito && paqueteCoincidente is not null)
        {
            var nombresAreas = string.Join(" + ", tipos.Select(t => t.Nombre)).ToUpperInvariant();
            await _reciboPrinter.ImprimirTicketControlAsync(
                $"ACCESO {nombresAreas}",
                $"{dto.CantidadPersonas} persona(s) — {resultado.Data!.NumeroFormateado}");
        }

        return resultado;
    }

    // ── Habitación ────────────────────────────────────────────────────
    public async Task<ApiResponse<ComprobanteResultadoDto>> GenerarComprobanteHabitacion(int habitacionId, GenerarComprobanteDto dto)
    {
        var hab = await _uow.Habitaciones.ObtenerPorIdAsync(habitacionId);
        if (hab is null)    return ApiResponse<ComprobanteResultadoDto>.Fallido("Habitación no encontrada");
        // Ahora se cobra al asignar (al entrar el cliente), no al liberar.
        if (hab.Ocupado)    return ApiResponse<ComprobanteResultadoDto>.Fallido("La habitación ya está ocupada");
        if (dto.Monto is null or <= 0) return ApiResponse<ComprobanteResultadoDto>.Fallido("Debe indicar el monto a cobrar");

        var monto     = dto.Monto.Value;
        var valorUnit = Math.Round(monto / 1.18m, 2);

        var items = new List<ItemComprobante> { new()
        {
            Descripcion    = $"Hospedaje - {hab.Nombre}",
            Cantidad       = 1,
            ValorUnitario  = valorUnit,
            PrecioUnitario = monto,
            Subtotal       = valorUnit,
            Igv            = Math.Round(monto - valorUnit, 2),
            Total          = monto,
        }};

        var resultado = await Emitir(dto, monto, items, "habitacion", habitacionId);
        if (!resultado.Exito) return resultado;

        hab.Ocupado = true;
        hab.FechaCheckIn = DateTime.UtcNow;
        hab.FechaCheckOut = null;
        await _uow.Habitaciones.ActualizarAsync(hab);
        await _uow.GuardarCambiosAsync();

        return resultado;
    }

    // ── Tienda ────────────────────────────────────────────────────────
    public async Task<ApiResponse<ComprobanteResultadoDto>> GenerarComprobanteTienda(GenerarComprobanteTiendaDto dto)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Debe agregar al menos un producto");

        var items = new List<ItemComprobante>();
        decimal total = 0;

        foreach (var itemDto in dto.Items)
        {
            var producto = await _uow.Productos.ObtenerPorIdAsync(itemDto.ProductoId);
            if (producto is null || !producto.Activo)
                return ApiResponse<ComprobanteResultadoDto>.Fallido($"Producto ID {itemDto.ProductoId} no encontrado");

            var subtotalItem = Math.Round(producto.Precio * itemDto.Cantidad, 2);
            var valorUnit    = Math.Round(producto.Precio / 1.18m, 2);
            var igvItem      = Math.Round(subtotalItem - (valorUnit * itemDto.Cantidad), 2);

            items.Add(new ItemComprobante
            {
                Descripcion    = producto.Nombre,
                Cantidad       = itemDto.Cantidad,
                ValorUnitario  = valorUnit,
                PrecioUnitario = producto.Precio,
                Subtotal       = Math.Round(valorUnit * itemDto.Cantidad, 2),
                Igv            = igvItem,
                Total          = subtotalItem,
            });

            total += subtotalItem;

            producto.Stock -= itemDto.Cantidad;
            await _uow.Productos.ActualizarAsync(producto);
        }

        var resultado = await Emitir(dto, total, items, "tienda", 0);
        if (resultado.Exito)
            await _uow.GuardarCambiosAsync();

        return resultado;
    }

    // ── Router: NV / BI / FI ─────────────────────────────────────────
    private async Task<ApiResponse<ComprobanteResultadoDto>> Emitir(
        GenerarComprobanteDto dto, decimal total,
        List<ItemComprobante> items, string tipoAmbiente, int referenciaId)
    {
        var resultado = await (dto.TipoComprobante switch
        {
            "NV" => EmitirNotaVenta(dto, total, items, tipoAmbiente, referenciaId),
            "BI" => EmitirConNubefact(dto, total, items, tipoAmbiente, referenciaId, tipoDoc: 2, serie: _cfg.SerieBoleta),
            "FI" => EmitirConNubefact(dto, total, items, tipoAmbiente, referenciaId, tipoDoc: 1, serie: _cfg.SerieFactura),
            _    => Task.FromResult(ApiResponse<ComprobanteResultadoDto>.Fallido("Tipo de comprobante no válido"))
        });

        if (resultado.Exito)
        {
            var clienteLabel = dto.TipoComprobante == "FI"
                ? dto.ClienteRazonSocial ?? dto.ClienteRuc ?? "Empresa"
                : dto.ClienteNombre ?? dto.ClienteDni ?? "CLIENTES VARIOS";

            var itemsRecibo = items.Select(i => new ItemReciboDto
            {
                Descripcion    = i.Descripcion,
                Cantidad       = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                Total          = i.Total,
            });

            await _reciboPrinter.ImprimirAsync(resultado.Data!, itemsRecibo, clienteLabel);
        }

        return resultado;
    }

    // ── Nota de Venta (local, sin SUNAT) ─────────────────────────────
    private async Task<ApiResponse<ComprobanteResultadoDto>> EmitirNotaVenta(
        GenerarComprobanteDto dto, decimal total,
        List<ItemComprobante> items, string tipoAmbiente, int referenciaId)
    {
        var numero       = await _uow.Comprobantes.ObtenerUltimoNumeroAsync(_cfg.SerieNV) + 1;
        var gravada      = Math.Round(total / 1.18m, 2);
        var impuesto     = Math.Round(total - gravada, 2);
        var cajero       = ObtenerCajero();
        var clienteNombre = dto.ClienteNombre ?? "CLIENTES VARIOS";

        var comprobante = new Comprobante
        {
            Serie             = _cfg.SerieNV,
            Numero            = numero,
            TipoComprobante   = "NV",
            TipoAmbiente      = tipoAmbiente,
            ReferenciaId      = referenciaId,
            ClienteDni        = dto.ClienteDni,
            ClienteNombre     = clienteNombre,
            Cajero            = cajero,
            TotalGravada      = gravada,
            Impuesto          = impuesto,
            Total             = total,
            Estado            = "EMITIDO",
            EnlacePdf         = string.Empty,
            MetodoPago        = dto.MetodoPago,
            Cobrado           = dto.MetodoPago != MetodoPago.Fiado,
            ClienteId         = dto.ClienteId,
            Detalles          = MapearDetalles(items),
        };
        await _uow.Comprobantes.AgregarAsync(comprobante);
        await _uow.GuardarCambiosAsync();

        return ApiResponse<ComprobanteResultadoDto>.Exitoso(new ComprobanteResultadoDto
        {
            ComprobanteId    = comprobante.ComprobanteId,
            TipoComprobante  = "NV",
            Ambiente         = tipoAmbiente,
            Serie            = _cfg.SerieNV,
            Numero           = numero,
            NumeroFormateado = $"{_cfg.SerieNV}-{numero:D5}",
            Cajero           = cajero,
            TotalGravada     = gravada,
            Impuesto         = impuesto,
            Total            = total,
            Estado           = "EMITIDO",
            EnlacePdf        = string.Empty,
            ModoSimulacion   = false,
        }, "Nota de venta emitida");
    }

    // ── Boleta / Factura via Nubefact (o simulación) ──────────────────
    private async Task<ApiResponse<ComprobanteResultadoDto>> EmitirConNubefact(
        GenerarComprobanteDto dto, decimal total,
        List<ItemComprobante> items, string tipoAmbiente, int referenciaId,
        int tipoDoc, string serie)
    {
        var totalGravada = Math.Round(total / 1.18m, 2);
        var totalIgv     = Math.Round(total - totalGravada, 2);
        var numero       = await _uow.Comprobantes.ObtenerUltimoNumeroAsync(serie) + 1;

        // Factura: requiere RUC
        if (tipoDoc == 1 && string.IsNullOrWhiteSpace(dto.ClienteRuc))
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Para factura se requiere el RUC del cliente");

        string enlacePdf;

        if (_cfg.ModoSimulacion)
        {
            enlacePdf = string.Empty;
        }
        else
        {
            var esFactura   = tipoDoc == 1;
            var clienteTipo = esFactura ? 6 : (string.IsNullOrWhiteSpace(dto.ClienteDni) ? 0 : 1);
            var clienteDoc  = esFactura ? dto.ClienteRuc! : dto.ClienteDni ?? "";
            var clienteNom  = esFactura
                ? dto.ClienteRazonSocial ?? ""
                : dto.ClienteNombre ?? "CLIENTES VARIOS";

            var payload = new
            {
                operacion              = "generar_comprobante",
                tipo_de_comprobante    = tipoDoc,
                serie,
                numero,
                sunat_transaction      = 1,
                cliente_tipo_de_documento   = clienteTipo,
                cliente_numero_de_documento = clienteDoc,
                cliente_denominacion        = clienteNom,
                cliente_direccion      = "",
                cliente_email          = "",
                fecha_de_emision       = DateTime.Now.ToString("dd/MM/yyyy"),
                moneda                 = 1,
                porcentaje_de_igv      = 18,
                total_gravada          = totalGravada,
                total_igv              = totalIgv,
                total,
                enviar_automaticamente_a_la_sunat = true,
                enviar_automaticamente_al_cliente = false,
                items = items.Select(i => new
                {
                    unidad_de_medida = "ZZ",
                    i.Descripcion,
                    cantidad         = i.Cantidad,
                    valor_unitario   = i.ValorUnitario,
                    precio_unitario  = i.PrecioUnitario,
                    subtotal         = i.Subtotal,
                    igv              = i.Igv,
                    total            = i.Total,
                    tipo_de_igv      = 1,
                    anticipo_regularizacion  = false,
                    anticipo_documento_serie = "",
                    anticipo_documento_numero = "",
                }).ToList()
            };

            try
            {
                var req = new HttpRequestMessage(HttpMethod.Post,
                    $"{_cfg.UrlBase.TrimEnd('/')}/{_cfg.Ruc}/comprobantes")
                { Content = JsonContent.Create(payload) };
                req.Headers.Add("Authorization", $"Token {_cfg.Token}");

                var resp = await _nubefactHttp.SendAsync(req);
                var json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    return ApiResponse<ComprobanteResultadoDto>.Fallido($"Error Nubefact: {json}");

                using var doc = JsonDocument.Parse(json);
                enlacePdf = doc.RootElement.TryGetProperty("enlace_del_pdf", out var p) ? p.GetString() ?? "" : "";
            }
            catch (Exception ex)
            {
                return ApiResponse<ComprobanteResultadoDto>.Fallido($"Error al conectar con Nubefact: {ex.Message}");
            }
        }

        var cajero  = ObtenerCajero();
        var tipoStr = tipoDoc == 1 ? "FI" : "BI";
        var estado  = _cfg.ModoSimulacion ? "SIMULADO" : "ENVIADO A SUNAT";

        var comprobante = new Comprobante
        {
            Serie              = serie,
            Numero             = numero,
            TipoComprobante    = tipoStr,
            TipoAmbiente       = tipoAmbiente,
            ReferenciaId       = referenciaId,
            ClienteDni         = tipoDoc == 1 ? null : dto.ClienteDni,
            ClienteRuc         = tipoDoc == 1 ? dto.ClienteRuc : null,
            ClienteNombre      = tipoDoc == 1 ? null : (dto.ClienteNombre ?? "CLIENTES VARIOS"),
            ClienteRazonSocial = tipoDoc == 1 ? dto.ClienteRazonSocial : null,
            Cajero             = cajero,
            TotalGravada       = totalGravada,
            Impuesto           = totalIgv,
            Total              = total,
            Estado             = estado,
            EnlacePdf          = enlacePdf,
            MetodoPago         = dto.MetodoPago,
            Cobrado            = dto.MetodoPago != MetodoPago.Fiado,
            ClienteId          = dto.ClienteId,
            Detalles           = MapearDetalles(items),
        };
        await _uow.Comprobantes.AgregarAsync(comprobante);
        await _uow.GuardarCambiosAsync();

        return ApiResponse<ComprobanteResultadoDto>.Exitoso(new ComprobanteResultadoDto
        {
            ComprobanteId    = comprobante.ComprobanteId,
            TipoComprobante  = tipoStr,
            Ambiente         = tipoAmbiente,
            Serie            = serie,
            Numero           = numero,
            NumeroFormateado = $"{serie}-{numero:D5}",
            Cajero           = cajero,
            TotalGravada     = totalGravada,
            Impuesto         = totalIgv,
            Total            = total,
            Estado           = estado,
            EnlacePdf        = enlacePdf,
            ModoSimulacion   = _cfg.ModoSimulacion,
        }, _cfg.ModoSimulacion ? $"{(tipoDoc == 1 ? "Factura" : "Boleta")} simulada" : "Comprobante enviado a SUNAT");
    }

    // ── Listado y anulación ───────────────────────────────────────────
    public async Task<IEnumerable<ComprobanteListadoDto>> ObtenerPorFechaAsync(string? fecha, string? tipoAmbiente)
    {
        var dia = DateOnly.TryParse(fecha, out var d) ? d : DateOnly.FromDateTime(DateTime.UtcNow - TimeSpan.FromHours(5));
        var comprobantes = await _uow.Comprobantes.ObtenerPorFechaAsync(dia, tipoAmbiente);
        return comprobantes.Select(c => new ComprobanteListadoDto
        {
            ComprobanteId   = c.ComprobanteId,
            TipoComprobante = c.TipoComprobante,
            Serie           = c.Serie,
            Numero          = c.Numero,
            TipoAmbiente    = c.TipoAmbiente,
            ClienteNombre   = c.ClienteNombre ?? c.ClienteRazonSocial,
            Cajero          = c.Cajero,
            Total           = c.Total,
            Estado          = c.Estado,
            FechaEmision    = c.FechaEmision,
            MetodoPago      = c.MetodoPago,
            Cobrado         = c.Cobrado,
            FechaCobro      = c.FechaCobro,
            ClienteId       = c.ClienteId,
        });
    }

    // ── Cuentas pendientes (fiado) ────────────────────────────────────
    public async Task<IEnumerable<ComprobanteListadoDto>> ObtenerPendientesDeCobroAsync()
    {
        var comprobantes = await _uow.Comprobantes.ObtenerPendientesDeCobroAsync();
        return comprobantes.Select(c => new ComprobanteListadoDto
        {
            ComprobanteId   = c.ComprobanteId,
            TipoComprobante = c.TipoComprobante,
            Serie           = c.Serie,
            Numero          = c.Numero,
            TipoAmbiente    = c.TipoAmbiente,
            ClienteNombre   = c.Cliente is not null
                ? $"{c.Cliente.Nombres} {c.Cliente.Apellidos}"
                : c.ClienteNombre ?? c.ClienteRazonSocial,
            Cajero          = c.Cajero,
            Total           = c.Total,
            Estado          = c.Estado,
            FechaEmision    = c.FechaEmision,
            MetodoPago      = c.MetodoPago,
            Cobrado         = c.Cobrado,
            FechaCobro      = c.FechaCobro,
            ClienteId       = c.ClienteId,
        });
    }

    public async Task<ApiResponse> MarcarCobradoAsync(int comprobanteId, MetodoPago metodoPagoReal)
    {
        var comprobante = await _uow.Comprobantes.ObtenerPorIdAsync(comprobanteId);
        if (comprobante is null)
            return ApiResponse.Fallido("Comprobante no encontrado");
        if (comprobante.Estado == "ANULADO")
            return ApiResponse.Fallido("El comprobante está anulado");
        if (comprobante.Cobrado)
            return ApiResponse.Fallido("El comprobante ya estaba marcado como cobrado");
        if (metodoPagoReal == MetodoPago.Fiado)
            return ApiResponse.Fallido("Debe indicar el método de pago real usado para cobrar, no 'Fiado'");

        comprobante.Cobrado    = true;
        comprobante.FechaCobro = DateTime.UtcNow;
        comprobante.MetodoPago = metodoPagoReal;
        await _uow.Comprobantes.ActualizarAsync(comprobante);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Comprobante marcado como cobrado");
    }

    public Task<ApiResponse> SolicitarAnulacionAsync(int id, string motivo, string cajero) =>
        _solicitudes.SolicitarAsync(id, motivo, cajero);

    // ── Anulaciones (vista supervisor) ───────────────────────────────
    public async Task<IEnumerable<AnulacionListadoDto>> ObtenerAnulacionesAsync(string? desde, string? hasta)
    {
        var desdeDate = DateOnly.TryParse(desde, out var d) ? d : DateOnly.FromDateTime(DateTime.UtcNow);
        var hastaDate = DateOnly.TryParse(hasta, out var h) ? h : DateOnly.FromDateTime(DateTime.UtcNow);

        var lista = await _uow.Comprobantes.ObtenerAnulacionesAsync(desdeDate, hastaDate);
        return lista.Select(c => new AnulacionListadoDto
        {
            ComprobanteId    = c.ComprobanteId,
            TipoComprobante  = c.TipoComprobante,
            Serie            = c.Serie,
            Numero           = c.Numero,
            NumeroFormateado = $"{c.Serie}-{c.Numero:D5}",
            TipoAmbiente     = c.TipoAmbiente,
            ClienteNombre    = c.ClienteNombre ?? c.ClienteRazonSocial,
            Cajero           = c.Cajero,
            Total            = c.Total,
            MotivoAnulacion  = c.MotivoAnulacion,
            AutorizadoPor    = c.AutorizadoPor,
            FechaEmision     = c.FechaEmision,
        });
    }

    // ── Ver/descargar un comprobante ya emitido ───────────────────────
    public async Task<ApiResponse<ComprobanteDetalleCompletoDto>> ObtenerDetalleAsync(int comprobanteId)
    {
        var c = await _uow.Comprobantes.ObtenerConDetalleAsync(comprobanteId);
        if (c is null)
            return ApiResponse<ComprobanteDetalleCompletoDto>.Fallido("Comprobante no encontrado");

        return ApiResponse<ComprobanteDetalleCompletoDto>.Exitoso(new ComprobanteDetalleCompletoDto
        {
            ComprobanteId      = c.ComprobanteId,
            TipoComprobante    = c.TipoComprobante,
            Serie              = c.Serie,
            Numero             = c.Numero,
            TipoAmbiente       = c.TipoAmbiente,
            ClienteDni         = c.ClienteDni,
            ClienteRuc         = c.ClienteRuc,
            ClienteNombre      = c.Cliente is not null ? $"{c.Cliente.Nombres} {c.Cliente.Apellidos}" : c.ClienteNombre,
            ClienteRazonSocial = c.ClienteRazonSocial,
            Cajero             = c.Cajero,
            TotalGravada       = c.TotalGravada,
            Impuesto           = c.Impuesto,
            Total              = c.Total,
            Estado             = c.Estado,
            EnlacePdf          = string.IsNullOrWhiteSpace(c.EnlacePdf) ? null : c.EnlacePdf,
            FechaEmision       = c.FechaEmision,
            Items = c.Detalles.Select(d => new ItemReciboDto
            {
                Descripcion    = d.Descripcion,
                Cantidad       = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Total          = d.Subtotal,
            }).ToList(),
        });
    }

    // ── Nota de Crédito ───────────────────────────────────────────────
    public async Task<ApiResponse<ComprobanteResultadoDto>> EmitirNotaCreditoAsync(
        int comprobanteOrigenId, EmitirNotaCreditoDto dto)
    {
        var origen = await _uow.Comprobantes.ObtenerPorIdAsync(comprobanteOrigenId);
        if (origen is null)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Comprobante original no encontrado");
        if (origen.TipoComprobante is not ("BI" or "FI"))
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Solo se pueden emitir notas de crédito para Boletas o Facturas");
        if (origen.Estado == "ANULADO")
            return ApiResponse<ComprobanteResultadoDto>.Fallido("No se puede emitir nota de crédito sobre un comprobante anulado");

        var esBoleta  = origen.TipoComprobante == "BI";
        var tipoNc    = esBoleta ? 7 : 8;
        var serieNc   = esBoleta ? _cfg.SerieNcBoleta : _cfg.SerieNcFactura;

        decimal montoNc;
        if (dto.Tipo == "parcial")
        {
            if (dto.MontoDevolucion is null or <= 0)
                return ApiResponse<ComprobanteResultadoDto>.Fallido("Debe indicar el monto de devolución para nota de crédito parcial");
            if (dto.MontoDevolucion > origen.Total)
                return ApiResponse<ComprobanteResultadoDto>.Fallido("El monto de devolución no puede superar el total del comprobante original");
            montoNc = dto.MontoDevolucion.Value;
        }
        else
        {
            montoNc = origen.Total;
        }

        var gravadaNc = Math.Round(montoNc / 1.18m, 2);
        var igvNc     = Math.Round(montoNc - gravadaNc, 2);
        var numero    = await _uow.Comprobantes.ObtenerUltimoNumeroAsync(serieNc) + 1;
        var cajero    = ObtenerCajero();

        string enlacePdf;

        if (_cfg.ModoSimulacion)
        {
            enlacePdf = string.Empty;
        }
        else
        {
            var clienteTipo = esBoleta ? (string.IsNullOrWhiteSpace(origen.ClienteDni) ? 0 : 1) : 6;
            var clienteDoc  = esBoleta ? origen.ClienteDni ?? "" : origen.ClienteRuc ?? "";
            var clienteNom  = esBoleta ? origen.ClienteNombre ?? "CLIENTES VARIOS" : origen.ClienteRazonSocial ?? "";
            var descripcionNc = dto.Tipo == "parcial"
                ? $"Devolución parcial - {origen.Serie}-{origen.Numero:D5}"
                : $"Anulación total - {origen.Serie}-{origen.Numero:D5}";

            var payload = new
            {
                operacion           = "generar_comprobante",
                tipo_de_comprobante = tipoNc,
                serie               = serieNc,
                numero,
                tipo_de_nota_de_credito = dto.CodigoMotivo,
                nota_credito_o_debito_serie_comprobante_afectado  = origen.Serie,
                nota_credito_o_debito_numero_comprobante_afectado = origen.Numero.ToString(),
                sunat_transaction   = 3,
                cliente_tipo_de_documento   = clienteTipo,
                cliente_numero_de_documento = clienteDoc,
                cliente_denominacion        = clienteNom,
                cliente_direccion    = "",
                cliente_email        = "",
                fecha_de_emision     = DateTime.Now.ToString("dd/MM/yyyy"),
                moneda               = 1,
                porcentaje_de_igv    = 18,
                total_gravada        = gravadaNc,
                total_igv            = igvNc,
                total                = montoNc,
                enviar_automaticamente_a_la_sunat = true,
                enviar_automaticamente_al_cliente = false,
                items = new[]
                {
                    new
                    {
                        unidad_de_medida  = "ZZ",
                        descripcion       = descripcionNc,
                        cantidad          = 1,
                        valor_unitario    = gravadaNc,
                        precio_unitario   = montoNc,
                        subtotal          = gravadaNc,
                        igv               = igvNc,
                        total             = montoNc,
                        tipo_de_igv       = 1,
                        anticipo_regularizacion   = false,
                        anticipo_documento_serie  = "",
                        anticipo_documento_numero = "",
                    }
                }
            };

            try
            {
                var req = new HttpRequestMessage(HttpMethod.Post,
                    $"{_cfg.UrlBase.TrimEnd('/')}/{_cfg.Ruc}/comprobantes")
                { Content = JsonContent.Create(payload) };
                req.Headers.Add("Authorization", $"Token {_cfg.Token}");

                var resp = await _nubefactHttp.SendAsync(req);
                var json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    return ApiResponse<ComprobanteResultadoDto>.Fallido($"Error Nubefact: {json}");

                using var doc = JsonDocument.Parse(json);
                enlacePdf = doc.RootElement.TryGetProperty("enlace_del_pdf", out var p) ? p.GetString() ?? "" : "";
            }
            catch (Exception ex)
            {
                return ApiResponse<ComprobanteResultadoDto>.Fallido($"Error al conectar con Nubefact: {ex.Message}");
            }
        }

        var estado = _cfg.ModoSimulacion ? "SIMULADO" : "ENVIADO A SUNAT";

        var nc = new Comprobante
        {
            Serie                = serieNc,
            Numero               = numero,
            TipoComprobante      = "NC",
            TipoAmbiente         = origen.TipoAmbiente,
            ReferenciaId         = origen.ReferenciaId,
            ComprobanteOrigenId  = origen.ComprobanteId,
            ClienteDni           = origen.ClienteDni,
            ClienteRuc           = origen.ClienteRuc,
            ClienteNombre        = origen.ClienteNombre,
            ClienteRazonSocial   = origen.ClienteRazonSocial,
            Cajero               = cajero,
            TotalGravada         = gravadaNc,
            Impuesto             = igvNc,
            Total                = montoNc,
            Estado               = estado,
            EnlacePdf            = enlacePdf,
        };
        await _uow.Comprobantes.AgregarAsync(nc);
        await _uow.GuardarCambiosAsync();

        var tipoLabel = dto.Tipo == "parcial" ? "parcial" : "total";
        var msg = _cfg.ModoSimulacion
            ? $"Nota de crédito {tipoLabel} simulada"
            : $"Nota de crédito {tipoLabel} enviada a SUNAT";

        return ApiResponse<ComprobanteResultadoDto>.Exitoso(new ComprobanteResultadoDto
        {
            TipoComprobante  = "NC",
            Ambiente         = origen.TipoAmbiente,
            Serie            = serieNc,
            Numero           = numero,
            NumeroFormateado = $"{serieNc}-{numero:D5}",
            Cajero           = cajero,
            TotalGravada     = gravadaNc,
            Impuesto         = igvNc,
            Total            = montoNc,
            Estado           = estado,
            EnlacePdf        = enlacePdf,
            ModoSimulacion   = _cfg.ModoSimulacion,
        }, msg);
    }

    private static List<ComprobanteDetalle> MapearDetalles(List<ItemComprobante> items) =>
        items.Select(i => new ComprobanteDetalle
        {
            Descripcion    = i.Descripcion,
            Cantidad       = i.Cantidad,
            PrecioUnitario = i.PrecioUnitario,
            Subtotal       = i.Total,
        }).ToList();

    // ── Modelo interno ────────────────────────────────────────────────
    private class ItemComprobante
    {
        public string  Descripcion    { get; set; } = string.Empty;
        public int     Cantidad       { get; set; }
        public decimal ValorUnitario  { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal       { get; set; }
        public decimal Igv            { get; set; }
        public decimal Total          { get; set; }
    }
}
