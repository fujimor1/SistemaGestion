using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs.Compras;

public class CompraDto
{
    public int CompraId { get; set; }
    public int? ProveedorId { get; set; }
    public string RucProveedor { get; set; } = string.Empty;
    public string RazonSocialProveedor { get; set; } = string.Empty;

    public string TipoComprobante { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string NumeroFormateado => $"{Serie}-{Numero:D5}";
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

    [Required]
    public string Serie { get; set; } = string.Empty;

    [Required]
    public int Numero { get; set; }

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

    [Required]
    public decimal Cantidad { get; set; }

    [Required]
    public decimal PrecioUnitario { get; set; }
}

public class PagarCompraDto
{
    public bool PagarConCajaChica { get; set; }
}
