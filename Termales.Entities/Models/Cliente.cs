namespace Termales.Entities.Models;

public class Cliente
{
    public int ClienteId { get; set; }
    /// <summary>"DNI" para persona natural, "RUC" para empresa.</summary>
    public string TipoDocumento { get; set; } = "DNI";
    /// <summary>DNI (8 dígitos) o RUC (11 dígitos) según TipoDocumento.</summary>
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    /// <summary>Razón social — solo para TipoDocumento = "RUC".</summary>
    public string? RazonSocial { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
