using Termales.Common.Settings;
using Termales.Entities.Models;

namespace Termales.Tests.Sunat;

internal static class FacturaMuestras
{
    public static Comprobante Comprobante()
    {
        var comprobante = new Comprobante
        {
            Serie = "F001",
            Numero = 1,
            TipoComprobante = "FI",
            ClienteRuc = "20123456789",
            ClienteRazonSocial = "Cliente de Prueba SAC",
            Moneda = "PEN",
            TotalGravada = 84.75m,
            Impuesto = 15.25m,
            Total = 100.00m,
            FechaEmision = new DateTime(2026, 7, 14, 15, 0, 0, DateTimeKind.Utc),
        };
        comprobante.Detalles.Add(new ComprobanteDetalle
        {
            Descripcion = "Servicio de spa - paquete termal",
            Cantidad = 1,
            PrecioUnitario = 100.00m,
            Subtotal = 100.00m,
        });
        return comprobante;
    }

    public static Comprobante ComprobanteBoleta(string? clienteDni = "12345678")
    {
        var comprobante = new Comprobante
        {
            Serie = "B001",
            Numero = 1,
            TipoComprobante = "BI",
            ClienteDni = clienteDni,
            ClienteNombre = clienteDni is null ? null : "Cliente de Prueba",
            Moneda = "PEN",
            TotalGravada = 84.75m,
            Impuesto = 15.25m,
            Total = 100.00m,
            FechaEmision = new DateTime(2026, 7, 14, 15, 0, 0, DateTimeKind.Utc),
        };
        comprobante.Detalles.Add(new ComprobanteDetalle
        {
            Descripcion = "Servicio de spa - paquete termal",
            Cantidad = 1,
            PrecioUnitario = 100.00m,
            Subtotal = 100.00m,
        });
        return comprobante;
    }

    public static EmpresaSettings Empresa() => new()
    {
        RazonSocial = "EMP. COMUNAL BAÑOS TERMOMEDICINALES DE COLLPA",
        Ruc = "20284587970",
        Direccion = "CAR. FUJIMORI FUJIMORI NRO. S/N",
        Urbanizacion = "-",
        Distrito = "SANTA CRUZ DE ANDAMARCA",
        Provincia = "HUARAL",
        Departamento = "LIMA",
        Ubigeo = "150125",
        CodigoPais = "PE",
        CodigoEstablecimiento = "0000",
    };
}
