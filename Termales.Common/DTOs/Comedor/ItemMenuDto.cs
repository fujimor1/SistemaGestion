using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs.Comedor;

public class ItemMenuDto
{
    public int ItemMenuId { get; set; }
    public int CategoriaMenuId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
    public List<RecetaInsumoDto> Receta { get; set; } = [];
}

public class RecetaInsumoDto
{
    public int RecetaInsumoId { get; set; }
    public int InsumoId { get; set; }
    public string NombreInsumo { get; set; } = string.Empty;
    public string? UnidadInsumo { get; set; }
    /// <summary>En gramos si el insumo se mide en kilos; si no, en la unidad propia del insumo.</summary>
    public decimal Cantidad { get; set; }
}

public class RecetaInsumoInputDto
{
    [Required]
    public int InsumoId { get; set; }

    // Antes era obligatorio (mínimo 0.01) — ahora se puede dejar en 0 y
    // completarlo después, porque no siempre se sabe el peso exacto al
    // momento de crear el plato. Se compensa con el consumo diario que
    // registra el cocinero manualmente (ver SalidaInsumo).
    [Range(0, double.MaxValue, ErrorMessage = "La cantidad no puede ser negativa")]
    public decimal Cantidad { get; set; }
}

public class CrearItemMenuDto
{
    [Required]
    public int CategoriaMenuId { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    [Required, Range(0.01, 9999)]
    public decimal Precio { get; set; }

    public List<RecetaInsumoInputDto> Receta { get; set; } = [];
}

public class ActualizarItemMenuDto : CrearItemMenuDto
{
    public int ItemMenuId { get; set; }
}
