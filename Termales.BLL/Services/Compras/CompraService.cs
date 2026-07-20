using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Termales.BLL.Interfaces;
using Termales.BLL.Interfaces.Compras;
using Termales.Common.DTOs.Caja;
using Termales.Common.DTOs.Compras;
using Termales.Common.Settings;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models.Compras;
using Termales.Entities.Models.Inventario;
using Termales.Entities.Models.Tienda;

namespace Termales.BLL.Services.Compras;

public class CompraService : ICompraService
{
    private static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
    private const long TamanoMaximoBytes = 8 * 1024 * 1024; // 8 MB por foto

    private readonly IUnitOfWork _uow;
    private readonly ICajaService _cajaService;
    private readonly UploadsSettings _uploadsCfg;

    public CompraService(IUnitOfWork uow, ICajaService cajaService, IOptions<UploadsSettings> uploadsCfg)
    {
        _uow = uow;
        _cajaService = cajaService;
        _uploadsCfg = uploadsCfg.Value;
    }

    // En el servidor real, Uploads:ComprasPath apunta fuera de /var/www/collpa-api
    // (esa carpeta se borra en cada deploy). En dev local, si queda vacío, cae a una
    // carpeta junto al build — no hace falta configurar nada para probar en local.
    private string ObtenerCarpetaBase() =>
        string.IsNullOrWhiteSpace(_uploadsCfg.ComprasPath)
            ? Path.Combine(AppContext.BaseDirectory, "uploads-dev", "compras")
            : _uploadsCfg.ComprasPath;

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
            var egreso = await _cajaService.RegistrarEgresoAsync(egresoDto, registradoPor, compra.CompraId);
            compra.EgresoCajaChicaId = egreso.EgresoCajaChicaId;
        }

        compra.Estado = "PAGADA";
        compra.FechaPago = DateTime.UtcNow;

        await _uow.Compras.ActualizarAsync(compra);
        await _uow.GuardarCambiosAsync();

        return MapearDto(compra);
    }

    public async Task<ResumenComprasDto> ObtenerResumenMesActualAsync()
    {
        var hoy = DateTime.UtcNow;
        var desde = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var hasta = desde.AddMonths(1);
        var (total, cantidad) = await _uow.Compras.ObtenerResumenAsync(desde, hasta);
        return new ResumenComprasDto
        {
            Desde = desde,
            Hasta = hasta.AddTicks(-1),
            TotalGastado = total,
            CantidadCompras = cantidad,
        };
    }

    public async Task<List<CompraImagenDto>> AgregarImagenesAsync(int compraId, List<IFormFile> archivos)
    {
        var compra = await _uow.Compras.ObtenerPorIdAsync(compraId)
            ?? throw new InvalidOperationException($"Compra {compraId} no encontrada");

        if (archivos.Count == 0)
            throw new InvalidOperationException("No se recibió ninguna imagen");

        foreach (var archivo in archivos)
        {
            var extension = Path.GetExtension(archivo.FileName);
            if (!ExtensionesPermitidas.Contains(extension))
                throw new InvalidOperationException($"Formato no permitido: {archivo.FileName} (solo JPG, PNG o WEBP)");
            if (archivo.Length > TamanoMaximoBytes)
                throw new InvalidOperationException($"\"{archivo.FileName}\" pesa demasiado (máximo 8 MB por foto)");
        }

        var carpeta = Path.Combine(ObtenerCarpetaBase(), compraId.ToString());
        Directory.CreateDirectory(carpeta);

        foreach (var archivo in archivos)
        {
            var extension = Path.GetExtension(archivo.FileName);
            var nombreEnDisco = $"{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(carpeta, nombreEnDisco);

            await using (var stream = File.Create(rutaCompleta))
                await archivo.CopyToAsync(stream);

            await _uow.CompraImagenes.AgregarAsync(new CompraImagen
            {
                CompraId = compraId,
                NombreArchivo = archivo.FileName,
                RutaArchivo = rutaCompleta,
            });
        }

        await _uow.GuardarCambiosAsync();
        return await ObtenerImagenesAsync(compraId);
    }

    public async Task<List<CompraImagenDto>> ObtenerImagenesAsync(int compraId)
    {
        var imagenes = await _uow.CompraImagenes.BuscarAsync(i => i.CompraId == compraId);
        return imagenes
            .OrderBy(i => i.FechaSubida)
            .Select(i => new CompraImagenDto
            {
                CompraImagenId = i.CompraImagenId,
                NombreArchivo = i.NombreArchivo,
                FechaSubida = i.FechaSubida,
            }).ToList();
    }

    public async Task<(byte[] Bytes, string ContentType, string NombreArchivo)?> ObtenerArchivoImagenAsync(int imagenId)
    {
        var imagen = await _uow.CompraImagenes.ObtenerPorIdAsync(imagenId);
        if (imagen is null || !File.Exists(imagen.RutaArchivo)) return null;

        var bytes = await File.ReadAllBytesAsync(imagen.RutaArchivo);
        var contentType = Path.GetExtension(imagen.RutaArchivo).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg",
        };
        return (bytes, contentType, imagen.NombreArchivo);
    }

    public async Task EliminarImagenAsync(int imagenId)
    {
        var imagen = await _uow.CompraImagenes.ObtenerPorIdAsync(imagenId)
            ?? throw new InvalidOperationException("Imagen no encontrada");

        try { if (File.Exists(imagen.RutaArchivo)) File.Delete(imagen.RutaArchivo); }
        catch { /* si el archivo ya no está en disco, igual se limpia el registro */ }

        await _uow.CompraImagenes.EliminarAsync(imagenId);
        await _uow.GuardarCambiosAsync();
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
        }).ToList(),
        Imagenes = c.Imagenes.Select(i => new CompraImagenDto
        {
            CompraImagenId = i.CompraImagenId,
            NombreArchivo = i.NombreArchivo,
            FechaSubida = i.FechaSubida,
        }).ToList()
    };
}
