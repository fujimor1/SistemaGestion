namespace Termales.Entities.Models.Inventario;

public class Insumo
{
    public int InsumoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    /// <summary>comedor | banio | habitacion</summary>
    public string TipoAmbiente { get; set; } = string.Empty;
    /// <summary>insumo (consumible) | activo (bien físico: camas, sábanas, etc.)</summary>
    public string TipoArticulo { get; set; } = "insumo";
    public string? Unidad { get; set; }
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal PrecioReferencia { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<EntradaInsumo> Entradas { get; set; } = new List<EntradaInsumo>();
}
