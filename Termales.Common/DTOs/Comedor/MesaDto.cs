using System.ComponentModel.DataAnnotations;
using Termales.Entities.Enums;

namespace Termales.Common.DTOs.Comedor;

public class MesaDto
{
    public int MesaId { get; set; }
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public EstadoMesa Estado { get; set; }
    public string EstadoDescripcion => Estado.ToString();
    public bool Activo { get; set; }
}

public class CrearMesaDto
{
    [Required, Range(1, 999)]
    public int Numero { get; set; }

    [Required, Range(1, 50)]
    public int Capacidad { get; set; }
}

public class ActualizarMesaDto : CrearMesaDto
{
    public int MesaId { get; set; }
}
