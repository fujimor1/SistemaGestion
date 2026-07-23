using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Termales.BLL.Interfaces;
using Termales.BLL.Interfaces.Sunat;
using Termales.Common.DTOs.Comprobante;
using Termales.Common.DTOs.Sunat;

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
    private readonly ICajaService _cajaService;
    private readonly IFacturaElectronicaService _facturaElectronica;
    private readonly SunatSettings _sunatCfg;
    private readonly INotaCreditoService _notaCredito;

    public ComprobanteService(
        IUnitOfWork uow,
        IHttpClientFactory httpFactory,
        IOptions<NubefactSettings> cfg,
        IHttpContextAccessor accessor,
        ISolicitudAnulacionService solicitudes,
        IReciboPrinterService reciboPrinter,
        ICajaService cajaService,
        IFacturaElectronicaService facturaElectronica,
        IOptions<SunatSettings> sunatCfg,
        INotaCreditoService notaCredito)
    {
        _uow          = uow;
        _nubefactHttp = httpFactory.CreateClient("Nubefact");
        _cfg          = cfg.Value;
        _accessor     = accessor;
        _solicitudes  = solicitudes;
        _reciboPrinter = reciboPrinter;
        _cajaService  = cajaService;
        _facturaElectronica = facturaElectronica;
        _sunatCfg     = sunatCfg.Value;
        _notaCredito  = notaCredito;
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

    // ── Baño termal: carrito de servicios/combos, sin control de ocupación ──
    public async Task<ApiResponse<ComprobanteResultadoDto>> GenerarComprobanteBanio(GenerarComprobanteBanioDto dto)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Debe agregar al menos un servicio");
        if (dto.Items.Any(i => i.Cantidad <= 0))
            return ApiResponse<ComprobanteResultadoDto>.Fallido("La cantidad debe ser mayor a 0");
        if (dto.Items.Any(i => (i.TipoServicioId is null) == (i.PaqueteBanioId is null)))
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Cada línea debe indicar un servicio o un combo, no ambos ni ninguno");

        var tipoIds = dto.Items.Where(i => i.TipoServicioId is not null).Select(i => i.TipoServicioId!.Value).Distinct().ToList();
        var paqueteIds = dto.Items.Where(i => i.PaqueteBanioId is not null).Select(i => i.PaqueteBanioId!.Value).Distinct().ToList();

        var tipos = tipoIds.Count == 0
            ? new List<TipoServicio>()
            : (await _uow.TiposServicio.BuscarAsync(t => tipoIds.Contains(t.TipoServicioId) && t.Activo)).ToList();
        if (tipos.Count != tipoIds.Count)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Alguno de los servicios seleccionados no existe o está inactivo");

        var paquetesActivos = (await _uow.PaquetesBanio.ObtenerActivosConTiposAsync()).ToList();
        var paquetes = paquetesActivos.Where(p => paqueteIds.Contains(p.PaqueteBanioId)).ToList();
        if (paquetes.Count != paqueteIds.Count)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Alguno de los combos seleccionados no existe o está inactivo");

        var tiposPorId = tipos.ToDictionary(t => t.TipoServicioId);
        var paquetesPorId = paquetes.ToDictionary(p => p.PaqueteBanioId);

        var items = new List<ItemComprobante>();
        var ticketsControl = new List<(string NombresAreas, int Cantidad)>();
        decimal monto = 0;

        foreach (var itemDto in dto.Items)
        {
            decimal precioUnitario;
            string descripcion;
            bool esCombo = itemDto.PaqueteBanioId is not null;

            if (esCombo)
            {
                var paquete = paquetesPorId[itemDto.PaqueteBanioId!.Value];
                precioUnitario = paquete.Precio;
                descripcion    = paquete.Nombre;
                ticketsControl.Add((string.Join(" + ", paquete.Tipos.Select(t => t.TipoServicio!.Nombre)).ToUpperInvariant(), itemDto.Cantidad));
            }
            else
            {
                var tipo = tiposPorId[itemDto.TipoServicioId!.Value];
                precioUnitario = tipo.PrecioPorPersona;
                descripcion    = tipo.Nombre;
            }

            var subtotalLinea = Math.Round(precioUnitario * itemDto.Cantidad, 2);
            var valorUnit     = Math.Round(precioUnitario / 1.18m, 2);
            var subtotalValor = Math.Round(valorUnit * itemDto.Cantidad, 2);

            items.Add(new ItemComprobante
            {
                Descripcion    = $"{descripcion} ({itemDto.Cantidad} pers.)",
                Cantidad       = itemDto.Cantidad,
                ValorUnitario  = valorUnit,
                PrecioUnitario = precioUnitario,
                Subtotal       = subtotalValor,
                Igv            = Math.Round(subtotalLinea - subtotalValor, 2),
                Total          = subtotalLinea,
            });

            monto += subtotalLinea;
        }
        monto = Math.Round(monto, 2);

        // Sin piscinaId ni control de ocupación: es un boleto plano, no una
        // asignación de un baño/piscina físico específico.
        var resultado = await Emitir(dto, monto, items, "banio", 0);

        // Cuando la venta cubre un combo (más de un área), la boleta trae un
        // solo ítem por combo — se imprime un ticket aparte de referencia para
        // poder controlar el ingreso a cada área por separado.
        if (resultado.Exito)
        {
            foreach (var (nombresAreas, cantidad) in ticketsControl)
            {
                await _reciboPrinter.ImprimirTicketControlAsync(
                    $"ACCESO {nombresAreas}",
                    $"{cantidad} persona(s) — {resultado.Data!.NumeroFormateado}");
            }
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
        if (hab.EstadoLimpieza == EstadoLimpieza.PorLimpiar)
            return ApiResponse<ComprobanteResultadoDto>.Fallido("La habitación está pendiente de limpieza — márcala como limpia antes de asignarla");
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
        if (!await _cajaService.HayCajaAbiertaAsync())
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Debes abrir la caja antes de registrar una venta");

        if (dto.MetodoPago == MetodoPago.Mixto)
        {
            if (dto.MontoEfectivoMixto is null || dto.MontoEfectivoMixto < 0 || dto.MontoEfectivoMixto > total)
                return ApiResponse<ComprobanteResultadoDto>.Fallido("El monto en efectivo del pago mixto debe estar entre 0 y el total");
        }

        var resultado = await (dto.TipoComprobante switch
        {
            "NV" => EmitirNotaVenta(dto, total, items, tipoAmbiente, referenciaId),
            "BI" => _sunatCfg.Habilitado
                ? EmitirDirectoSunat(dto, total, items, tipoAmbiente, referenciaId, "BI", _sunatCfg.SerieBoleta)
                : EmitirConNubefact(dto, total, items, tipoAmbiente, referenciaId, tipoDoc: 2, serie: _cfg.SerieBoleta),
            "FI" => _sunatCfg.Habilitado
                ? EmitirDirectoSunat(dto, total, items, tipoAmbiente, referenciaId, "FI", _sunatCfg.SerieFactura)
                : EmitirConNubefact(dto, total, items, tipoAmbiente, referenciaId, tipoDoc: 1, serie: _cfg.SerieFactura),
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
        var numero       = await _uow.ComprobanteSeries.SiguienteNumeroAsync(_cfg.SerieNV, "NV");
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
            MontoEfectivoMixto = dto.MetodoPago == MetodoPago.Mixto ? dto.MontoEfectivoMixto : null,
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
        var numero       = await _uow.ComprobanteSeries.SiguienteNumeroAsync(serie, tipoDoc == 1 ? "FI" : "BI");

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
                fecha_de_emision       = DateTime.UtcNow.AddHours(-5).ToString("dd/MM/yyyy"), // Perú UTC-5
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
            MontoEfectivoMixto = dto.MetodoPago == MetodoPago.Mixto ? dto.MontoEfectivoMixto : null,
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

    // ── Factura/Boleta directa a SUNAT (sin Nubefact) ─────────────────
    private async Task<ApiResponse<ComprobanteResultadoDto>> EmitirDirectoSunat(
        GenerarComprobanteDto dto, decimal total,
        List<ItemComprobante> items, string tipoAmbiente, int referenciaId,
        string tipoComprobante, string serie)
    {
        var esFactura = tipoComprobante == "FI";
        if (esFactura && string.IsNullOrWhiteSpace(dto.ClienteRuc))
            return ApiResponse<ComprobanteResultadoDto>.Fallido("Para factura se requiere el RUC del cliente");

        var totalGravada = Math.Round(total / 1.18m, 2);
        var totalIgv     = Math.Round(total - totalGravada, 2);
        var numero       = await _uow.ComprobanteSeries.SiguienteNumeroAsync(serie, tipoComprobante);
        var cajero       = ObtenerCajero();

        var comprobante = new Comprobante
        {
            Serie              = serie,
            Numero             = numero,
            TipoComprobante    = tipoComprobante,
            TipoAmbiente       = tipoAmbiente,
            ReferenciaId       = referenciaId,
            ClienteDni         = esFactura ? null : dto.ClienteDni,
            ClienteRuc         = esFactura ? dto.ClienteRuc : null,
            ClienteNombre      = esFactura ? null : (dto.ClienteNombre ?? "CLIENTES VARIOS"),
            ClienteRazonSocial = esFactura ? dto.ClienteRazonSocial : null,
            Cajero             = cajero,
            TotalGravada       = totalGravada,
            Impuesto           = totalIgv,
            Total              = total,
            Estado             = "PENDIENTE DE ENVÍO A SUNAT",
            EnlacePdf          = "",
            MetodoPago         = dto.MetodoPago,
            MontoEfectivoMixto = dto.MetodoPago == MetodoPago.Mixto ? dto.MontoEfectivoMixto : null,
            Cobrado            = dto.MetodoPago != MetodoPago.Fiado,
            ClienteId          = dto.ClienteId,
            Detalles           = MapearDetalles(items),
        };
        await _uow.Comprobantes.AgregarAsync(comprobante);
        await _uow.GuardarCambiosAsync(); // necesario para tener ComprobanteId antes de llamar a SUNAT

        // La venta ya quedó registrada localmente con su correlativo reservado — un fallo de SUNAT
        // a partir de aquí (red, timeout, rechazo) no bloquea el ticket ni la venta, solo queda
        // reflejado en el Estado para revisar/reintentar después (ver reenviar-sunat).
        var resultadoSunat = await _facturaElectronica.EmitirAsync(comprobante);

        comprobante.Estado = resultadoSunat.Exito
            ? (resultadoSunat.Data!.Aceptado ? "ENVIADO A SUNAT" : "RECHAZADO POR SUNAT")
            : "PENDIENTE DE ENVÍO A SUNAT";
        await _uow.Comprobantes.ActualizarAsync(comprobante);
        await _uow.GuardarCambiosAsync();

        return ApiResponse<ComprobanteResultadoDto>.Exitoso(new ComprobanteResultadoDto
        {
            ComprobanteId    = comprobante.ComprobanteId,
            TipoComprobante  = tipoComprobante,
            Ambiente         = tipoAmbiente,
            Serie            = serie,
            Numero           = numero,
            NumeroFormateado = $"{serie}-{numero:D5}",
            Cajero           = cajero,
            TotalGravada     = totalGravada,
            Impuesto         = totalIgv,
            Total            = total,
            Estado           = comprobante.Estado,
            EnlacePdf        = "",
            ModoSimulacion   = false,
        }, resultadoSunat.Exito ? resultadoSunat.Mensaje : $"Venta registrada; pendiente de envío a SUNAT ({resultadoSunat.Mensaje})");
    }

    // ── Reintento manual y visibilidad de pendientes SUNAT ────────────
    public async Task<ApiResponse<ResultadoEmisionSunatDto>> ReenviarSunatAsync(int comprobanteId)
    {
        var comprobante = await _uow.Comprobantes.ObtenerConDetalleAsync(comprobanteId);
        if (comprobante is null)
            return ApiResponse<ResultadoEmisionSunatDto>.Fallido("Comprobante no encontrado");
        if (comprobante.TipoComprobante is not ("FI" or "BI" or "NC"))
            return ApiResponse<ResultadoEmisionSunatDto>.Fallido("El reenvío solo aplica a Facturas, Boletas o Notas de Crédito emitidas directamente a SUNAT");

        var resultado = await _facturaElectronica.EmitirAsync(comprobante);

        comprobante.Estado = resultado.Exito
            ? (resultado.Data!.Aceptado ? "ENVIADO A SUNAT" : "RECHAZADO POR SUNAT")
            : "PENDIENTE DE ENVÍO A SUNAT";
        await _uow.Comprobantes.ActualizarAsync(comprobante);
        await _uow.GuardarCambiosAsync();

        return resultado;
    }

    public async Task<IEnumerable<ComprobanteSunatPendienteDto>> ObtenerPendientesSunatAsync()
    {
        var pendientes = await _uow.ComprobantesSunat.ObtenerPendientesAsync();
        return pendientes.Select(p => new ComprobanteSunatPendienteDto
        {
            ComprobanteId    = p.ComprobanteId,
            Serie            = p.Comprobante.Serie,
            Numero           = p.Comprobante.Numero,
            Total            = p.Comprobante.Total,
            Estado           = p.Estado.ToString(),
            IntentosEnvio    = p.IntentosEnvio,
            FechaLimiteEnvio = p.FechaLimiteEnvio,
            UltimoError      = p.CdrDescripcion,
        });
    }

    public async Task<IEnumerable<ComprobanteElectronicoDto>> ObtenerFacturasBoletasAsync(string? fecha)
    {
        var dia = DateOnly.TryParse(fecha, out var d) ? d : DateOnly.FromDateTime(DateTime.UtcNow - TimeSpan.FromHours(5));
        var comprobantes = await _uow.Comprobantes.ObtenerFacturasBoletasAsync(dia);
        return comprobantes.Select(c => new ComprobanteElectronicoDto
        {
            ComprobanteId    = c.ComprobanteId,
            NumeroFormateado = $"{c.Serie}-{c.Numero:D5}",
            TipoComprobante  = c.TipoComprobante,
            TipoAmbiente     = c.TipoAmbiente,
            ClienteNombre    = c.TipoComprobante == "FI" ? c.ClienteRazonSocial : c.ClienteNombre,
            ClienteDocumento = c.TipoComprobante == "FI" ? c.ClienteRuc : c.ClienteDni,
            Detalle          = string.Join("; ", c.Detalles.Select(det => $"{det.Cantidad}x {det.Descripcion}")),
            FechaEmision     = c.FechaEmision,
            Total            = c.Total,
            Cajero           = c.Cajero,
            MetodoPago       = (int)c.MetodoPago,
            Estado           = c.Estado,
        });
    }

    public async Task<IEnumerable<NotaCreditoListadoDto>> ObtenerNotasCreditoAsync(string? desde, string? hasta)
    {
        var desdeDate = DateOnly.TryParse(desde, out var d) ? d : DateOnly.FromDateTime(DateTime.UtcNow);
        var hastaDate = DateOnly.TryParse(hasta, out var h) ? h : DateOnly.FromDateTime(DateTime.UtcNow);
        var notasCredito = await _uow.Comprobantes.ObtenerNotasCreditoAsync(desdeDate, hastaDate);
        return notasCredito.Select(nc => new NotaCreditoListadoDto
        {
            ComprobanteId    = nc.ComprobanteId,
            NumeroFormateado = $"{nc.Serie}-{nc.Numero:D5}",
            ComprobanteOrigenTipo = nc.ComprobanteOrigen?.TipoComprobante,
            ComprobanteOrigenNumeroFormateado = nc.ComprobanteOrigen is null
                ? null
                : $"{nc.ComprobanteOrigen.Serie}-{nc.ComprobanteOrigen.Numero:D5}",
            Motivo       = nc.MotivoAnulacion,
            FechaEmision = nc.FechaEmision,
            Total        = nc.Total,
            Cajero       = nc.Cajero,
            Estado       = nc.Estado,
        });
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

    // Corrige la forma de pago de un comprobante ya emitido (ej. el cajero se
    // equivocó al elegir Efectivo en vez de Yape) — a diferencia de
    // MarcarCobradoAsync, no toca Cobrado/FechaCobro, solo el método de pago.
    public async Task<ApiResponse> ActualizarMetodoPagoAsync(int comprobanteId, ActualizarMetodoPagoDto dto)
    {
        var comprobante = await _uow.Comprobantes.ObtenerPorIdAsync(comprobanteId);
        if (comprobante is null)
            return ApiResponse.Fallido("Comprobante no encontrado");
        if (comprobante.Estado == "ANULADO")
            return ApiResponse.Fallido("El comprobante está anulado");
        if (dto.MetodoPago == MetodoPago.Fiado)
            return ApiResponse.Fallido("No se puede dejar un comprobante ya cobrado como 'Fiado'");
        if (dto.MetodoPago == MetodoPago.Mixto)
        {
            if (dto.MontoEfectivoMixto is null || dto.MontoEfectivoMixto < 0 || dto.MontoEfectivoMixto > comprobante.Total)
                return ApiResponse.Fallido("El monto en efectivo del pago mixto no puede ser mayor al total ni negativo");
        }

        comprobante.MetodoPago         = dto.MetodoPago;
        comprobante.MontoEfectivoMixto = dto.MetodoPago == MetodoPago.Mixto ? dto.MontoEfectivoMixto : null;
        await _uow.Comprobantes.ActualizarAsync(comprobante);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Forma de pago actualizada");
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
            MetodoPago         = c.MetodoPago,
            MontoEfectivoMixto = c.MontoEfectivoMixto,
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
    public Task<ApiResponse<ComprobanteResultadoDto>> EmitirNotaCreditoAsync(
        int comprobanteOrigenId, EmitirNotaCreditoDto dto) =>
        _notaCredito.EmitirAsync(comprobanteOrigenId, dto, ObtenerCajero());

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
