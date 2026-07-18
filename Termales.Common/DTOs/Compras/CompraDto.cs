using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs.Compras;

public class CompraDto
{
    public int CompraId { get; set; }
    public int? ProveedorId { get; set; }
    public string RucProveedor { get; set; } = string.Empty;
    public string RazonSocialProveedor { get; set; } = string.Empty;

    public string TipoComprobante { get; set; } = string.Empty;
    public string? Serie { get; set; }
    public int? Numero { get; set; }
    public string NumeroFormateado => Serie is not null && Numero is not null ? $"{Serie}-{Numero:D5}" : "S/N";
    public DateTime FechaEmision { get; set; }

    public string FormaPago { get; set; } = string.Empty;
    public DateTime? FechaVencimiento { get; set; }

    public string Moneda { get; set; } = string.Empty;
    public decimal TotalGravada { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }

    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaPago { get; set; }
    public int? EgresoCajaChicaId { get; set; }

    public List<DetalleCompraDto> Detalles { get; set; } = [];
}

public class DetalleCompraDto
{
    public int DetalleCompraId { get; set; }
    public string TipoItem { get; set; } = string.Empty;
    public int? InsumoId { get; set; }
    public string? NombreInsumo { get; set; }
    public int? ProductoId { get; set; }
    public string? NombreProducto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
}

public class RegistrarCompraDto
{
    public int? ProveedorId { get; set; }

    public string? NombreProveedorManual { get; set; }

    [Required]
    public string TipoComprobante { get; set; } = string.Empty;

    // No son obligatorios porque una GUIA puede no traer serie/número
    // (se valida en el servicio para FACTURA/BOLETA, que sí los necesitan).
    public string? Serie { get; set; }
    public int? Numero { get; set; }

    [Required]
    public DateTime FechaEmision { get; set; }

    [Required]
    public string FormaPago { get; set; } = string.Empty;

    public DateTime? FechaVencimiento { get; set; }
    public string Moneda { get; set; } = "PEN";
    public string? Observaciones { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "La compra debe tener al menos una línea de detalle")]
    public List<RegistrarDetalleCompraDto> Detalles { get; set; } = [];
}

public class RegistrarDetalleCompraDto
{
    [Required]
    public string TipoItem { get; set; } = string.Empty;

    public int? InsumoId { get; set; }
    public int? ProductoId { get; set; }

    // Si InsumoId/ProductoId vienen null, se crea uno nuevo con estos datos
    // (a veces entra un insumo/producto que todavía no existe en el catálogo).
    public string? NombreNuevo { get; set; }
    /// <summary>Solo aplica si se crea un insumo nuevo: kg, litros, unidad, etc.</summary>
    public string? UnidadNuevoInsumo { get; set; }
    /// <summary>Solo aplica si se crea un insumo nuevo: comedor | banio | habitacion.</summary>
    public string? TipoAmbienteNuevoInsumo { get; set; }
    /// <summary>Solo aplica si se crea un producto nuevo: precio de venta en tienda.</summary>
    public decimal? PrecioVentaNuevoProducto { get; set; }
    /// <summary>Solo aplica si se crea un producto nuevo: si aparece como vendible en Tienda.
    /// Falso para insumos operativos que se registran como "producto" (ej. artículos de limpieza)
    /// pero que no se le venden al cliente. Por defecto true (comportamiento histórico).</summary>
    public bool? ActivoParaVenta { get; set; }

    [Required]
    public decimal Cantidad { get; set; }

    [Required]
    public decimal PrecioUnitario { get; set; }
}

public class PagarCompraDto
{
    public bool PagarConCajaChica { get; set; }
}

public class ResumenComprasDto
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public decimal TotalGastado { get; set; }
    public int CantidadCompras { get; set; }
}
