using System.ComponentModel.DataAnnotations;
using Termales.Entities.Enums;

namespace Termales.Common.DTOs.Comedor;

public class OrdenDto
{
    public int OrdenId { get; set; }
    public int MesaId { get; set; }
    public int NumeroMesa { get; set; }
    public int UsuarioId { get; set; }
    public string NombreMesero { get; set; } = string.Empty;
    public EstadoOrden Estado { get; set; }
    public string EstadoDescripcion => Estado.ToString();
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public List<OrdenDetalleDto> Detalles { get; set; } = new();
}

public class OrdenDetalleDto
{
    public int OrdenDetalleId { get; set; }
    public int? ItemMenuId { get; set; }
    public int? ProductoId { get; set; }
    /// <summary>"cocina" (plato con receta, va al ticket de cocina) | "tienda" (producto de venta directa).</summary>
    public string Origen { get; set; } = string.Empty;
    public string NombreItem { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
    public EstadoOrdenDetalle Estado { get; set; }
    public string EstadoDescripcion => Estado.ToString();
    public string? Observaciones { get; set; }
    /// <summary>Comprobante que ya cobró esta línea — null si aún no se ha cobrado.</summary>
    public int? ComprobanteId { get; set; }
}

public class CrearOrdenDto
{
    [Required]
    public int MesaId { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    public string? Observaciones { get; set; }

    [Required, MinLength(1)]
    public List<CrearOrdenDetalleDto> Detalles { get; set; } = new();
}

public class CrearOrdenDetalleDto
{
    // Exactamente uno de los dos debe venir informado (se valida en el
    // servicio): un plato del menú o un producto de tienda.
    public int? ItemMenuId { get; set; }
    public int? ProductoId { get; set; }

    [Required, Range(1, 100)]
    public int Cantidad { get; set; }

    public string? Observaciones { get; set; }
}

public class AgregarItemsOrdenDto
{
    [Required, MinLength(1)]
    public List<CrearOrdenDetalleDto> Items { get; set; } = new();
}

public class ActualizarEstadoDetalleDto
{
    [Required]
    public EstadoOrdenDetalle Estado { get; set; }
}
