namespace Termales.Entities.Models.Comedor;

public class ItemMenu
{
    public int ItemMenuId { get; set; }
    public int CategoriaMenuId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; } = true;

    public CategoriaMenu Categoria { get; set; } = null!;
    public ICollection<OrdenDetalle> OrdenDetalles { get; set; } = new List<OrdenDetalle>();
    public ICollection<RecetaInsumo> Receta { get; set; } = new List<RecetaInsumo>();
}
