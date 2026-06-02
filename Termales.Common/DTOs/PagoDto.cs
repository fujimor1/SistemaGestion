using System.ComponentModel.DataAnnotations;
using Termales.Entities.Enums;

namespace Termales.Common.DTOs;

public class PagoDto
{
    public int PagoId { get; set; }
    public int ReservaId { get; set; }
    public decimal Monto { get; set; }
    public TipoPago TipoPago { get; set; }
    public string TipoPagoDescripcion => TipoPago.ToString();
    public DateTime FechaPago { get; set; }
    public string? NumeroComprobante { get; set; }
    public string? Observaciones { get; set; }
}

public class RegistrarPagoDto
{
    [Required]
    public int ReservaId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }

    [Required]
    public TipoPago TipoPago { get; set; }

    [StringLength(50)]
    public string? NumeroComprobante { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }
}
