namespace Termales.Common.DTOs.Reporte;

public class CatalogoDto
{
    public List<CatalogoItemDto> Tienda { get; set; } = [];
    public List<CatalogoItemDto> Comedor { get; set; } = [];
    public List<CatalogoItemDto> Banios { get; set; } = [];
    public List<CatalogoItemDto> Habitaciones { get; set; } = [];
}

public class CatalogoItemDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public decimal Precio { get; set; }
}
