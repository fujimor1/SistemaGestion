using Termales.BLL.Interfaces;
using Termales.BLL.Interfaces.Compras;
using Termales.Common.DTOs.Caja;
using Termales.Common.DTOs.Compras;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models.Compras;
using Termales.Entities.Models.Inventario;
using Termales.Entities.Models.Tienda;

namespace Termales.BLL.Services.Compras;

public class CompraService : ICompraService
{
    private readonly IUnitOfWork _uow;
    private readonly ICajaService _cajaService;

    public CompraService(IUnitOfWork uow, ICajaService cajaService)
    {
        _uow = uow;
        _cajaService = cajaService;
    }

    public async Task<CompraDto?> ObtenerPorIdAsync(int id)
    {
        var compra = await _uow.Compras.ObtenerConDetallesAsync(id);
        return compra is null ? null : MapearDto(compra);
    }

    public async Task<(IEnumerable<CompraDto> Items, int Total)> ObtenerPaginadoAsync(
        int pagina, int tamanoPagina, int? proveedorId, string? estado)
    {
        var (items, total) = await _uow.Compras.ObtenerPaginadoAsync(pagina, tamanoPagina, proveedorId, estado);
        return (items.Select(MapearDto), total);
    }

    public async Task<CompraDto> RegistrarAsync(RegistrarCompraDto dto, string registradoPor)
    {
        if (!await _cajaService.HayCajaAbiertaAsync())
            throw new InvalidOperationException("Debes abrir la caja antes de registrar una compra");

        if (dto.Detalles.Count == 0)
            throw new InvalidOperationException("La compra debe tener al menos una línea de detalle");

        Proveedor? proveedor = null;
        if (dto.ProveedorId.HasValue)
        {
            proveedor = await _uow.Proveedores.ObtenerPorIdAsync(dto.ProveedorId.Value)
                ?? throw new Exception($"Proveedor {dto.ProveedorId} no encontrado");
        }
        else if (string.IsNullOrWhiteSpace(dto.NombreProveedorManual))
        {
            throw new InvalidOperationException(
                "Debe seleccionar un proveedor o indicar a quién se le realizó la compra");
        }

        // Las guías de remisión a veces no traen serie/número; factura y
        // boleta sí los necesitan siempre.
        var esGuia = dto.TipoComprobante.Equals("GUIA", StringComparison.OrdinalIgnoreCase);
        if (!esGuia && (string.IsNullOrWhiteSpace(dto.Serie) || dto.Numero is null))
            throw new InvalidOperationException("Debes indicar la serie y el número del comprobante");

        if (!esGuia && await _uow.Compras.ExisteAsync(c =>
                c.ProveedorId == dto.ProveedorId && c.Serie == dto.Serie && c.Numero == dto.Numero))
            throw new InvalidOperationException(
                "Ya existe una compra registrada con esa serie y número para este proveedor");

        var compra = new Compra
        {
            ProveedorId = dto.ProveedorId,
            Proveedor = proveedor,
            NombreProveedorManual = proveedor is null ? dto.NombreProveedorManual!.Trim() : null,
            TipoComprobante = dto.TipoComprobante,
            Serie = esGuia ? null : dto.Serie,
            Numero = esGuia ? null : dto.Numero,
            FechaEmision = dto.FechaEmision,
            FormaPago = dto.FormaPago,
            FechaVencimiento = dto.FechaVencimiento,
            Moneda = dto.Moneda,
            Estado = "REGISTRADA",
            Observaciones = dto.Observaciones,
            RegistradoPor = registradoPor,
            FechaRegistro = DateTime.UtcNow
        };

        decimal totalCompra = 0;

        foreach (var linea in dto.Detalles)
        {
            var esInsumo = linea.TipoItem.Equals("INSUMO", StringComparison.OrdinalIgnoreCase);
            var esProducto = linea.TipoItem.Equals("PRODUCTO", StringComparison.OrdinalIgnoreCase);

            if (esInsumo == esProducto)
                throw new InvalidOperationException("TipoItem debe ser INSUMO o PRODUCTO");
            if (esInsumo && linea.InsumoId is null && string.IsNullOrWhiteSpace(linea.NombreNuevo))
                throw new InvalidOperationException("Selecciona un insumo existente o escribe el nombre de uno nuevo");
            if (esProducto && linea.ProductoId is null && string.IsNullOrWhiteSpace(linea.NombreNuevo))
                throw new InvalidOperationException("Selecciona un producto existente o escribe el nombre de uno nuevo");

            var totalLinea = linea.Cantidad * linea.PrecioUnitario;
            totalCompra += totalLinea;

            var detalle = new DetalleCompra
            {
                TipoItem = linea.TipoItem.ToUpperInvariant(),
                Cantidad = linea.Cantidad,
                PrecioUnitario = linea.PrecioUnitario,
                Total = totalLinea,
                Compra = compra
            };
            compra.Detalles.Add(detalle);

            if (esInsumo)
            {
                Insumo insumo;
                if (linea.InsumoId is int insumoIdExistente)
                {
                    insumo = await _uow.Insumos.ObtenerPorIdAsync(insumoIdExistente)
                        ?? throw new InvalidOperationException($"Insumo {insumoIdExistente} no encontrado");

                    insumo.StockActual += linea.Cantidad;
                    insumo.PrecioReferencia = linea.PrecioUnitario;
                    await _uow.Insumos.ActualizarAsync(insumo);
                }
                else
                {
                    // Insumo nuevo: se crea al vuelo con el stock de esta compra
                    // (queda tracked como "Added", no hace falta ActualizarAsync).
                    insumo = new Insumo
                    {
                        Nombre = linea.NombreNuevo!.Trim(),
                        TipoAmbiente = string.IsNullOrWhiteSpace(linea.TipoAmbienteNuevoInsumo) ? "comedor" : linea.TipoAmbienteNuevoInsumo!,
                        TipoArticulo = "insumo",
                        Unidad = linea.UnidadNuevoInsumo,
                        StockActual = linea.Cantidad,
                        StockMinimo = 0,
                        PrecioReferencia = linea.PrecioUnitario,
                        Activo = true,
                    };
                    await _uow.Insumos.AgregarAsync(insumo);
                }

                detalle.Insumo = insumo;

                await _uow.EntradasInsumo.AgregarAsync(new EntradaInsumo
                {
                    Insumo = insumo,
                    Cantidad = linea.Cantidad,
                    PrecioUnitario = linea.PrecioUnitario,
                    Total = totalLinea,
                    Compra = compra
                });
            }
            else
            {
                Producto producto;
                if (linea.ProductoId is int productoIdExistente)
                {
                    producto = await _uow.Productos.ObtenerPorIdAsync(productoIdExistente)
                        ?? throw new InvalidOperationException($"Producto {productoIdExistente} no encontrado");

                    producto.Stock += (int)linea.Cantidad;
                    producto.PrecioCompra = linea.PrecioUnitario;
                    await _uow.Productos.ActualizarAsync(producto);
                }
                else
                {
                    // Producto nuevo: si no se indicó precio de venta, se parte del
                    // precio de compra (se puede ajustar después desde Tienda).
                    producto = new Producto
                    {
                        Nombre = linea.NombreNuevo!.Trim(),
                        Descripcion = "----",
                        PrecioCompra = linea.PrecioUnitario,
                        Precio = linea.PrecioVentaNuevoProducto ?? linea.PrecioUnitario,
                        Stock = (int)linea.Cantidad,
                        StockMinimo = 0,
                        Activo = linea.ActivoParaVenta ?? true,
                    };
                    await _uow.Productos.AgregarAsync(producto);
                }

                detalle.Producto = producto;

                await _uow.EntradasProducto.AgregarAsync(new EntradaProducto
                {
                    Producto = producto,
                    Cantidad = (int)linea.Cantidad,
                    PrecioUnitario = linea.PrecioUnitario,
                    Total = totalLinea,
                    Compra = compra
                });
            }
        }

        compra.Total = totalCompra;
        compra.TotalGravada = Math.Round(totalCompra / 1.18m, 2);
        compra.Igv = compra.Total - compra.TotalGravada;

        await _uow.Compras.AgregarAsync(compra);
        await _uow.GuardarCambiosAsync();

        var creada = await _uow.Compras.ObtenerConDetallesAsync(compra.CompraId)
            ?? throw new Exception("Error al recuperar la compra registrada");
        return MapearDto(creada);
    }

    public async Task<CompraDto> PagarAsync(int id, PagarCompraDto dto, string registradoPor)
    {
        var compra = await _uow.Compras.ObtenerConDetallesAsync(id)
            ?? throw new Exception($"Compra {id} no encontrada");

        if (compra.Estado != "REGISTRADA")
            throw new InvalidOperationException("Solo se pueden pagar compras en estado REGISTRADA");

        if (dto.PagarConCajaChica)
        {
            var egresoDto = new RegistrarEgresoDto
            {
                Concepto = $"Pago proveedor: {compra.Proveedor?.RazonSocial ?? compra.NombreProveedorManual} - {compra.TipoComprobante} {compra.Serie}-{compra.Numero}",
                Monto = compra.Total,
                TipoDocumento = compra.TipoComprobante,
                NumeroDocumento = $"{compra.Serie}-{compra.Numero}",
                Observaciones = compra.Observaciones
            };
            var egreso = await _cajaService.RegistrarEgresoAsync(egresoDto, registradoPor);
            compra.EgresoCajaChicaId = egreso.EgresoCajaChicaId;
        }

        compra.Estado = "PAGADA";
        compra.FechaPago = DateTime.UtcNow;

        await _uow.Compras.ActualizarAsync(compra);
        await _uow.GuardarCambiosAsync();

        return MapearDto(compra);
    }

    private static CompraDto MapearDto(Compra c) => new()
    {
        CompraId = c.CompraId,
        ProveedorId = c.ProveedorId,
        RucProveedor = c.Proveedor?.Ruc ?? string.Empty,
        RazonSocialProveedor = c.Proveedor?.RazonSocial ?? c.NombreProveedorManual ?? string.Empty,
        TipoComprobante = c.TipoComprobante,
        Serie = c.Serie,
        Numero = c.Numero,
        FechaEmision = c.FechaEmision,
        FormaPago = c.FormaPago,
        FechaVencimiento = c.FechaVencimiento,
        Moneda = c.Moneda,
        TotalGravada = c.TotalGravada,
        Igv = c.Igv,
        Total = c.Total,
        Estado = c.Estado,
        Observaciones = c.Observaciones,
        RegistradoPor = c.RegistradoPor,
        FechaRegistro = c.FechaRegistro,
        FechaPago = c.FechaPago,
        EgresoCajaChicaId = c.EgresoCajaChicaId,
        Detalles = c.Detalles.Select(d => new DetalleCompraDto
        {
            DetalleCompraId = d.DetalleCompraId,
            TipoItem = d.TipoItem,
            InsumoId = d.InsumoId,
            NombreInsumo = d.Insumo?.Nombre,
            ProductoId = d.ProductoId,
            NombreProducto = d.Producto?.Nombre,
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            Total = d.Total
        }).ToList()
    };
}
