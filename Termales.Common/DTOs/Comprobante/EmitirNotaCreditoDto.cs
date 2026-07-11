namespace Termales.Common.DTOs.Comprobante;

public class EmitirNotaCreditoDto
{
    /// <summary>"total" = crédito por el monto completo | "parcial" = crédito por monto específico</summary>
    public string Tipo { get; set; } = "total";

    /// <summary>Requerido cuando Tipo = "parcial"</summary>
    public decimal? MontoDevolucion { get; set; }

    /// <summary>
    /// Código de motivo Nubefact/SUNAT:
    /// 1 = Anulación de la operación
    /// 2 = Anulación por error en RUC
    /// 3 = Corrección por error en descripción
    /// 4 = Descuento global
    /// 5 = Descuento por ítem
    /// 6 = Devolución parcial de bienes
    /// </summary>
    public int CodigoMotivo { get; set; } = 1;
}
