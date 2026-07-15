using System.Globalization;
using System.Xml.Linq;
using Termales.BLL.Interfaces.Sunat;
using Termales.Common.Settings;
using Termales.Entities.Models;

namespace Termales.BLL.Services.Sunat.Xml;

/// <summary>
/// Genera el XML UBL 2.1 (sin firmar) de una Nota de Crédito Electrónica que referencia una
/// Factura o Boleta ya aceptada por SUNAT (anulación total o devolución parcial). Mismos catálogos
/// y convenciones que <see cref="FacturaXmlBuilder"/>; el motivo usa el catálogo 09 de SUNAT.
/// </summary>
public class NotaCreditoXmlBuilder : INotaCreditoXmlBuilder
{
    private static readonly XNamespace Cn  = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2";
    private static readonly XNamespace Cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace Cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace Ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private static readonly XNamespace Ds  = "http://www.w3.org/2000/09/xmldsig#";

    private const string CatalogosBase = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo";
    private const string TipoAfectacionGravado = "10"; // catálogo 07
    private const string TipoDocumentoFactura = "01";  // catálogo 01
    private const string TipoDocumentoBoleta = "03";   // catálogo 01
    private const string TipoDocIdentidadRuc = "6";    // catálogo 06
    private const string TipoDocIdentidadDni = "1";    // catálogo 06

    public XDocument Construir(Comprobante notaCredito, Comprobante origen, EmpresaSettings empresa)
    {
        var moneda = string.IsNullOrWhiteSpace(notaCredito.Moneda) ? "PEN" : notaCredito.Moneda;
        var fechaLocal = notaCredito.FechaEmision.ToUniversalTime().AddHours(-5); // Perú: UTC-5
        var tipoDocOrigen = origen.TipoComprobante == "FI" ? TipoDocumentoFactura : TipoDocumentoBoleta;
        var codigoMotivo = notaCredito.CodigoMotivoNc ?? "01";
        var descripcionMotivo = notaCredito.MotivoAnulacion ?? "ANULACION DE LA OPERACION";

        var creditNote = new XElement(Cn + "CreditNote",
            new XAttribute(XNamespace.Xmlns + "cac", Cac),
            new XAttribute(XNamespace.Xmlns + "cbc", Cbc),
            new XAttribute(XNamespace.Xmlns + "ext", Ext),
            new XAttribute(XNamespace.Xmlns + "ds", Ds),

            // ext:UBLExtensions se omite sin firmar por la misma razón que en FacturaXmlBuilder:
            // si está presente, ext:ExtensionContent no puede estar vacío según el XSD.

            new XElement(Cbc + "UBLVersionID", "2.1"),
            new XElement(Cbc + "CustomizationID",
                new XAttribute("schemeAgencyName", "PE:SUNAT"),
                "2.0"),
            new XElement(Cbc + "ID", $"{notaCredito.Serie}-{notaCredito.Numero}"),
            new XElement(Cbc + "IssueDate", fechaLocal.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            new XElement(Cbc + "IssueTime", fechaLocal.ToString("HH:mm:ss", CultureInfo.InvariantCulture)),
            new XElement(Cbc + "DocumentCurrencyCode",
                new XAttribute("listID", "ISO 4217 Alpha"),
                new XAttribute("listName", "Currency"),
                new XAttribute("listAgencyName", "United Nations Economic Commission for Europe"),
                moneda),

            new XElement(Cac + "DiscrepancyResponse",
                new XElement(Cbc + "ReferenceID", $"{origen.Serie}-{origen.Numero}"),
                new XElement(Cbc + "ResponseCode",
                    new XAttribute("listAgencyName", "PE:SUNAT"),
                    new XAttribute("listName", "Tipo de Nota de Crédito"),
                    new XAttribute("listURI", CatalogosBase + "09"),
                    codigoMotivo),
                new XElement(Cbc + "Description", descripcionMotivo)),

            new XElement(Cac + "BillingReference",
                new XElement(Cac + "InvoiceDocumentReference",
                    new XElement(Cbc + "ID", $"{origen.Serie}-{origen.Numero}"),
                    new XElement(Cbc + "DocumentTypeCode", tipoDocOrigen))),

            ConstruirEmisor(empresa),
            ConstruirAdquirente(notaCredito),
            ConstruirTaxTotal(notaCredito, moneda),
            ConstruirLegalMonetaryTotal(notaCredito, moneda));

        var numero = 1;
        foreach (var detalle in notaCredito.Detalles)
            creditNote.Add(ConstruirLinea(detalle, moneda, numero++));

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), creditNote);
    }

    private XElement ConstruirEmisor(EmpresaSettings empresa) =>
        new(Cac + "AccountingSupplierParty",
            new XElement(Cac + "Party",
                new XElement(Cac + "PartyIdentification",
                    new XElement(Cbc + "ID",
                        new XAttribute("schemeID", TipoDocIdentidadRuc),
                        new XAttribute("schemeName", "Documento de Identidad"),
                        new XAttribute("schemeAgencyName", "PE:SUNAT"),
                        new XAttribute("schemeURI", CatalogosBase + "06"),
                        empresa.Ruc)),
                new XElement(Cac + "PartyName",
                    new XElement(Cbc + "Name", empresa.RazonSocial)),
                new XElement(Cac + "PartyLegalEntity",
                    new XElement(Cbc + "RegistrationName", empresa.RazonSocial),
                    new XElement(Cac + "RegistrationAddress",
                        new XElement(Cbc + "ID",
                            new XAttribute("schemeAgencyName", "PE:INEI"),
                            new XAttribute("schemeName", "Ubigeos"),
                            empresa.Ubigeo),
                        new XElement(Cbc + "AddressTypeCode",
                            new XAttribute("listAgencyName", "PE:SUNAT"),
                            new XAttribute("listName", "Establecimientos anexos"),
                            empresa.CodigoEstablecimiento),
                        new XElement(Cbc + "CitySubdivisionName", empresa.Urbanizacion),
                        new XElement(Cbc + "CityName", empresa.Provincia),
                        new XElement(Cbc + "CountrySubentity", empresa.Departamento),
                        new XElement(Cbc + "District", empresa.Distrito),
                        new XElement(Cac + "AddressLine",
                            new XElement(Cbc + "Line", empresa.Direccion)),
                        new XElement(Cac + "Country",
                            new XElement(Cbc + "IdentificationCode",
                                new XAttribute("listID", "ISO 3166-1"),
                                new XAttribute("listAgencyName", "United Nations Economic Commission for Europe"),
                                new XAttribute("listName", "Country"),
                                empresa.CodigoPais))))));

    private XElement ConstruirAdquirente(Comprobante notaCredito)
    {
        string schemeId;
        string numeroDocumento;
        string razonSocial;

        if (!string.IsNullOrWhiteSpace(notaCredito.ClienteRuc))
        {
            schemeId = TipoDocIdentidadRuc;
            numeroDocumento = notaCredito.ClienteRuc;
            razonSocial = notaCredito.ClienteRazonSocial ?? notaCredito.ClienteNombre ?? "";
        }
        else if (!string.IsNullOrWhiteSpace(notaCredito.ClienteDni))
        {
            schemeId = TipoDocIdentidadDni;
            numeroDocumento = notaCredito.ClienteDni;
            razonSocial = notaCredito.ClienteNombre ?? "CLIENTES VARIOS";
        }
        else
        {
            schemeId = TipoDocIdentidadDni;
            numeroDocumento = "00000000";
            razonSocial = notaCredito.ClienteNombre ?? "CLIENTES VARIOS";
        }

        return new XElement(Cac + "AccountingCustomerParty",
            new XElement(Cac + "Party",
                new XElement(Cac + "PartyIdentification",
                    new XElement(Cbc + "ID",
                        new XAttribute("schemeID", schemeId),
                        new XAttribute("schemeName", "Documento de Identidad"),
                        new XAttribute("schemeAgencyName", "PE:SUNAT"),
                        new XAttribute("schemeURI", CatalogosBase + "06"),
                        numeroDocumento)),
                new XElement(Cac + "PartyLegalEntity",
                    new XElement(Cbc + "RegistrationName", razonSocial))));
    }

    private XElement ConstruirLinea(ComprobanteDetalle detalle, string moneda, int numeroLinea)
    {
        var valorVenta = Math.Round(detalle.Subtotal / 1.18m, 2);
        var igv = detalle.Subtotal - valorVenta;
        var valorUnitario = Math.Round(detalle.PrecioUnitario / 1.18m, 2);

        return new XElement(Cac + "CreditNoteLine",
            new XElement(Cbc + "ID", numeroLinea),
            new XElement(Cbc + "CreditedQuantity",
                new XAttribute("unitCode", "ZZ"),
                new XAttribute("unitCodeListID", "UN/ECE rec 20"),
                new XAttribute("unitCodeListAgencyName", "United Nations Economic Commission for Europe"),
                detalle.Cantidad),
            new XElement(Cbc + "LineExtensionAmount",
                new XAttribute("currencyID", moneda), Money(valorVenta)),
            new XElement(Cac + "PricingReference",
                new XElement(Cac + "AlternativeConditionPrice",
                    new XElement(Cbc + "PriceAmount",
                        new XAttribute("currencyID", moneda), Money(detalle.PrecioUnitario)),
                    new XElement(Cbc + "PriceTypeCode",
                        new XAttribute("listName", "Tipo de Precio"),
                        new XAttribute("listAgencyName", "PE:SUNAT"),
                        new XAttribute("listURI", CatalogosBase + "16"),
                        "01"))),
            new XElement(Cac + "TaxTotal",
                new XElement(Cbc + "TaxAmount",
                    new XAttribute("currencyID", moneda), Money(igv)),
                new XElement(Cac + "TaxSubtotal",
                    new XElement(Cbc + "TaxableAmount",
                        new XAttribute("currencyID", moneda), Money(valorVenta)),
                    new XElement(Cbc + "TaxAmount",
                        new XAttribute("currencyID", moneda), Money(igv)),
                    new XElement(Cac + "TaxCategory",
                        new XElement(Cbc + "ID",
                            new XAttribute("schemeID", "UN/ECE 5305"),
                            new XAttribute("schemeName", "Tax Category Identifier"),
                            new XAttribute("schemeAgencyName", "United Nations Economic Commission for Europe"),
                            "S"),
                        new XElement(Cbc + "Percent", "18.00"),
                        new XElement(Cbc + "TaxExemptionReasonCode",
                            new XAttribute("listAgencyName", "PE:SUNAT"),
                            new XAttribute("listName", "Afectacion del IGV"),
                            new XAttribute("listURI", CatalogosBase + "07"),
                            TipoAfectacionGravado),
                        new XElement(Cac + "TaxScheme",
                            new XElement(Cbc + "ID",
                                new XAttribute("schemeName", "Codigo de tributos"),
                                new XAttribute("schemeAgencyName", "PE:SUNAT"),
                                new XAttribute("schemeURI", CatalogosBase + "05"),
                                "1000"),
                            new XElement(Cbc + "Name", "IGV"),
                            new XElement(Cbc + "TaxTypeCode", "VAT"))))),
            new XElement(Cac + "Item",
                new XElement(Cbc + "Description", detalle.Descripcion)),
            new XElement(Cac + "Price",
                new XElement(Cbc + "PriceAmount",
                    new XAttribute("currencyID", moneda), Money(valorUnitario))));
    }

    private XElement ConstruirTaxTotal(Comprobante notaCredito, string moneda) =>
        new(Cac + "TaxTotal",
            new XElement(Cbc + "TaxAmount",
                new XAttribute("currencyID", moneda), Money(notaCredito.Impuesto)),
            new XElement(Cac + "TaxSubtotal",
                new XElement(Cbc + "TaxableAmount",
                    new XAttribute("currencyID", moneda), Money(notaCredito.TotalGravada)),
                new XElement(Cbc + "TaxAmount",
                    new XAttribute("currencyID", moneda), Money(notaCredito.Impuesto)),
                new XElement(Cac + "TaxCategory",
                    new XElement(Cbc + "ID",
                        new XAttribute("schemeID", "UN/ECE 5305"),
                        new XAttribute("schemeName", "Tax Category Identifier"),
                        new XAttribute("schemeAgencyName", "United Nations Economic Commission for Europe"),
                        "S"),
                    new XElement(Cac + "TaxScheme",
                        new XElement(Cbc + "ID",
                            new XAttribute("schemeID", "UN/ECE 5153"),
                            new XAttribute("schemeAgencyID", "6"),
                            "1000"),
                        new XElement(Cbc + "Name", "IGV"),
                        new XElement(Cbc + "TaxTypeCode", "VAT")))));

    private XElement ConstruirLegalMonetaryTotal(Comprobante notaCredito, string moneda) =>
        new(Cac + "LegalMonetaryTotal",
            new XElement(Cbc + "LineExtensionAmount",
                new XAttribute("currencyID", moneda), Money(notaCredito.TotalGravada)),
            new XElement(Cbc + "TaxInclusiveAmount",
                new XAttribute("currencyID", moneda), Money(notaCredito.Total)),
            new XElement(Cbc + "PayableAmount",
                new XAttribute("currencyID", moneda), Money(notaCredito.Total)));

    private static string Money(decimal valor) => valor.ToString("F2", CultureInfo.InvariantCulture);
}
