using System.ComponentModel.DataAnnotations;
using Termales.Entities.Enums;

namespace Termales.Common.DTOs.Comedor;

public class OrdenDto
{
    public int OrdenId { get; set; }
    public int? MesaId { get; set; }
    public int? NumeroMesa { get; set; }
    /// <summary>Números de las mesas unidas a esta orden, ej. "3+4". Igual a NumeroMesa si no hay unión.</summary>
    public string? MesasLabel { get; set; }
    public int UsuarioId { get; set; }
    public string NombreMesero { get; set; } = string.Empty;
    public EstadoOrden Estado { get; set; }
    public string EstadoDescripcion => Estado.ToString();
    /// <summary>"comedor" | "llevar".</summary>
    public string TipoEntrega { get; set; } = "comedor";
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public string? MotivoCancelacion { get; set; }
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
    // Requerido solo si TipoEntrega es "comedor"; un pedido "llevar" no
    // necesita mesa (se valida en el servicio).
    public int? MesaId { get; set; }

    /// <summary>"comedor" | "llevar". Por defecto "comedor" para no romper clientes viejos.</summary>
    public string TipoEntrega { get; set; } = "comedor";

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

public class CancelarOrdenDto
{
    [Required, MinLength(5, ErrorMessage = "Describe el motivo con más detalle")]
    public string Motivo { get; set; } = string.Empty;
}

public class UnirMesaDto
{
    [Required]
    public int MesaSecundariaId { get; set; }
}
