using System.ComponentModel.DataAnnotations;

namespace Termales.Common.DTOs.Comedor;

public class CategoriaMenuDto
{
    public int CategoriaMenuId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class CrearCategoriaMenuDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;
}

public class ActualizarCategoriaMenuDto : CrearCategoriaMenuDto
{
    public int CategoriaMenuId { get; set; }
}
