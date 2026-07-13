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

    /// <summary>Si esta mesa está unida a otra como secundaria, el ID de la principal.</summary>
    public int? MesaPrincipalId { get; set; }
    /// <summary>Números de las mesas secundarias unidas a esta (vacío si no es principal de ningún grupo).</summary>
    public List<int> NumerosMesasSecundarias { get; set; } = new();
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
