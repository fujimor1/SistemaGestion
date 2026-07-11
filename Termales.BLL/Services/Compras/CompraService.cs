using Termales.BLL.Interfaces;
using Termales.BLL.Interfaces.Compras;
using Termales.Common.DTOs.Caja;
using Termales.Common.DTOs.Compras;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models.Compras;
using Termales.Entities.Models.Inventario;

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
        if (dto.Detalles.Count == 0)
            throw new InvalidOperationException("La compra debe tener al menos una línea de detalle");

        var proveedor = await _uow.Proveedores.ObtenerPorIdAsync(dto.ProveedorId)
            ?? throw new Exception($"Proveedor {dto.ProveedorId} no encontrado");

        if (await _uow.Compras.ExisteAsync(c =>
                c.ProveedorId == dto.ProveedorId && c.Serie == dto.Serie && c.Numero == dto.Numero))
            throw new InvalidOperationException(
                "Ya existe una compra registrada con esa serie y número para este proveedor");

        var compra = new Compra
        {
            ProveedorId = dto.ProveedorId,
            Proveedor = proveedor,
            TipoComprobante = dto.TipoComprobante,
            Serie = dto.Serie,
            Numero = dto.Numero,
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
            if (esInsumo && linea.InsumoId is null)
                throw new InvalidOperationException("Falta InsumoId en una línea de tipo INSUMO");
            if (esProducto && linea.ProductoId is null)
                throw new InvalidOperationException("Falta ProductoId en una línea de tipo PRODUCTO");

            var totalLinea = linea.Cantidad * linea.PrecioUnitario;
            totalCompra += totalLinea;

            var detalle = new DetalleCompra
            {
                TipoItem = linea.TipoItem.ToUpperInvariant(),
                InsumoId = linea.InsumoId,
                ProductoId = linea.ProductoId,
                Cantidad = linea.Cantidad,
                PrecioUnitario = linea.PrecioUnitario,
                Total = totalLinea,
                Compra = compra
            };
            compra.Detalles.Add(detalle);

            if (esInsumo)
            {
                var insumo = await _uow.Insumos.ObtenerPorIdAsync(linea.InsumoId!.Value)
                    ?? throw new Exception($"Insumo {linea.InsumoId} no encontrado");

                insumo.StockActual += linea.Cantidad;
                insumo.PrecioReferencia = linea.PrecioUnitario;
                await _uow.Insumos.ActualizarAsync(insumo);

                await _uow.EntradasInsumo.AgregarAsync(new EntradaInsumo
                {
                    InsumoId = insumo.InsumoId,
                    Cantidad = linea.Cantidad,
                    PrecioUnitario = linea.PrecioUnitario,
                    Total = totalLinea,
                    Compra = compra
                });
            }
            else
            {
                var producto = await _uow.Productos.ObtenerPorIdAsync(linea.ProductoId!.Value)
                    ?? throw new Exception($"Producto {linea.ProductoId} no encontrado");

                producto.Stock += (int)linea.Cantidad;
                producto.PrecioCompra = linea.PrecioUnitario;
                await _uow.Productos.ActualizarAsync(producto);

                await _uow.EntradasProducto.AgregarAsync(new EntradaProducto
                {
                    ProductoId = producto.ProductoId,
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
                Concepto = $"Pago proveedor: {compra.Proveedor.RazonSocial} - {compra.TipoComprobante} {compra.Serie}-{compra.Numero}",
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
        RazonSocialProveedor = c.Proveedor?.RazonSocial ?? string.Empty,
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
