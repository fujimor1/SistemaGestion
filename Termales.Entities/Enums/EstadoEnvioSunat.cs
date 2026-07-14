namespace Termales.Entities.Enums;

public enum EstadoEnvioSunat
{
    Pendiente = 0,
    Enviado = 1,
    Aceptado = 2,
    AceptadoConObservacion = 3,
    Rechazado = 4,
    ErrorEnvio = 5, // fallo de red/SOAP fault, no una respuesta de negocio de SUNAT
}
