namespace Termales.Entities.Models.Comedor;

public class CategoriaMenu
{
    public int CategoriaMenuId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public ICollection<ItemMenu> Items { get; set; } = new List<ItemMenu>();
}
