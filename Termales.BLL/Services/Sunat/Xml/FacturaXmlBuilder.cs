using System.Globalization;
using System.Xml.Linq;
using Termales.BLL.Interfaces.Sunat;
using Termales.Common.Settings;
using Termales.Entities.Models;

namespace Termales.BLL.Services.Sunat.Xml;

/// <summary>
/// Genera el XML UBL 2.1 de una Factura Electrónica sin firmar, siguiendo el Anexo N.° 9-A
/// (RS 097-2012/SUNAT y modificatorias) para una venta interna simple, gravada con IGV 18%,
/// en soles, con un solo adquirente identificado por RUC. No cubre exportación, detracciones,
/// operaciones gratuitas, anticipos ni otros regímenes especiales.
/// </summary>
public class FacturaXmlBuilder : IFacturaXmlBuilder
{
    private static readonly XNamespace Inv = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
    private static readonly XNamespace Cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace Cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace Ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private static readonly XNamespace Ds  = "http://www.w3.org/2000/09/xmldsig#";

    private const string CatalogosBase = "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo";
    private const string TipoOperacionVentaInterna = "0101"; // catálogo 51
    private const string TipoAfectacionGravado = "10";        // catálogo 07
    private const string TipoDocumentoFactura = "01";         // catálogo 01
    private const string TipoDocIdentidadRuc = "6";           // catálogo 06

    public XDocument Construir(Comprobante comprobante, EmpresaSettings empresa)
    {
        var moneda = string.IsNullOrWhiteSpace(comprobante.Moneda) ? "PEN" : comprobante.Moneda;
        var fechaLocal = comprobante.FechaEmision.ToUniversalTime().AddHours(-5); // Perú: UTC-5, sin horario de verano

        var invoice = new XElement(Inv + "Invoice",
            new XAttribute(XNamespace.Xmlns + "cac", Cac),
            new XAttribute(XNamespace.Xmlns + "cbc", Cbc),
            new XAttribute(XNamespace.Xmlns + "ext", Ext),
            new XAttribute(XNamespace.Xmlns + "ds", Ds),

            // ext:UBLExtensions es opcional a nivel de Invoice, pero si se incluye su contenido
            // (ext:ExtensionContent) no puede estar vacío según el XSD — por eso se omite aquí y
            // lo agrega completo el XmlDsigSigner en la Fase 2, junto con el ds:Signature real.

            new XElement(Cbc + "UBLVersionID", "2.1"),
            new XElement(Cbc + "CustomizationID",
                new XAttribute("schemeAgencyName", "PE:SUNAT"),
                "2.0"),
            new XElement(Cbc + "ProfileID",
                // Catálogo 17: identificador de tipo de operación/transacción. Sin este campo,
                // SUNAT rechaza el envío con "Debe consignar la informacion del tipo de transaccion".
                new XAttribute("schemeName", "SUNAT:Identificador de Tipo de Operación"),
                new XAttribute("schemeAgencyName", "PE:SUNAT"),
                new XAttribute("schemeURI", CatalogosBase + "17"),
                TipoOperacionVentaInterna),
            new XElement(Cbc + "ID", $"{comprobante.Serie}-{comprobante.Numero}"),
            new XElement(Cbc + "IssueDate", fechaLocal.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            new XElement(Cbc + "IssueTime", fechaLocal.ToString("HH:mm:ss", CultureInfo.InvariantCulture)),
            new XElement(Cbc + "InvoiceTypeCode",
                new XAttribute("listAgencyName", "PE:SUNAT"),
                new XAttribute("listName", "Tipo de Documento"),
                new XAttribute("listURI", CatalogosBase + "01"),
                new XAttribute("listID", TipoOperacionVentaInterna), // catálogo 51: tipo de operación
                TipoDocumentoFactura),
            new XElement(Cbc + "DocumentCurrencyCode",
                new XAttribute("listID", "ISO 4217 Alpha"),
                new XAttribute("listName", "Currency"),
                new XAttribute("listAgencyName", "United Nations Economic Commission for Europe"),
                moneda),

            ConstruirEmisor(empresa),
            ConstruirAdquirente(comprobante),
            // Forma de pago: sin este campo SUNAT rechaza el envío (error 3244).
            new XElement(Cac + "PaymentTerms",
                new XElement(Cbc + "ID", "FormaPago"),
                new XElement(Cbc + "PaymentMeansID", comprobante.Cobrado ? "Contado" : "Credito")),
            ConstruirTaxTotal(comprobante, moneda),
            ConstruirLegalMonetaryTotal(comprobante, moneda));

        var numero = 1;
        foreach (var detalle in comprobante.Detalles)
            invoice.Add(ConstruirLinea(detalle, moneda, numero++));

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), invoice);
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

    private XElement ConstruirAdquirente(Comprobante comprobante)
    {
        // Factura siempre exige RUC del adquirente (ya validado en ComprobanteService antes de llegar aquí).
        var ruc = comprobante.ClienteRuc ?? "";
        var razonSocial = comprobante.ClienteRazonSocial ?? comprobante.ClienteNombre ?? "";

        return new XElement(Cac + "AccountingCustomerParty",
            new XElement(Cac + "Party",
                new XElement(Cac + "PartyIdentification",
                    new XElement(Cbc + "ID",
                        new XAttribute("schemeID", TipoDocIdentidadRuc),
                        new XAttribute("schemeName", "Documento de Identidad"),
                        new XAttribute("schemeAgencyName", "PE:SUNAT"),
                        new XAttribute("schemeURI", CatalogosBase + "06"),
                        ruc)),
                new XElement(Cac + "PartyLegalEntity",
                    new XElement(Cbc + "RegistrationName", razonSocial))));
    }

    private XElement ConstruirLinea(ComprobanteDetalle detalle, string moneda, int numeroLinea)
    {
        var valorVenta = Math.Round(detalle.Subtotal / 1.18m, 2);
        var igv = detalle.Subtotal - valorVenta;
        var valorUnitario = Math.Round(detalle.PrecioUnitario / 1.18m, 2);

        return new XElement(Cac + "InvoiceLine",
            new XElement(Cbc + "ID", numeroLinea),
            new XElement(Cbc + "InvoicedQuantity",
                // "ZZ" = unidad de servicios (catálogo 03) — Collpa vende mayormente servicios (hospedaje/spa);
                // ventas de tienda (bienes) deberían usar "NIU", pendiente de distinguir por tipo de ítem.
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

    private XElement ConstruirTaxTotal(Comprobante comprobante, string moneda) =>
        new(Cac + "TaxTotal",
            new XElement(Cbc + "TaxAmount",
                new XAttribute("currencyID", moneda), Money(comprobante.Impuesto)),
            new XElement(Cac + "TaxSubtotal",
                new XElement(Cbc + "TaxableAmount",
                    new XAttribute("currencyID", moneda), Money(comprobante.TotalGravada)),
                new XElement(Cbc + "TaxAmount",
                    new XAttribute("currencyID", moneda), Money(comprobante.Impuesto)),
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

    private XElement ConstruirLegalMonetaryTotal(Comprobante comprobante, string moneda) =>
        new(Cac + "LegalMonetaryTotal",
            new XElement(Cbc + "LineExtensionAmount",
                new XAttribute("currencyID", moneda), Money(comprobante.TotalGravada)),
            new XElement(Cbc + "TaxInclusiveAmount",
                new XAttribute("currencyID", moneda), Money(comprobante.Total)),
            new XElement(Cbc + "PayableAmount",
                new XAttribute("currencyID", moneda), Money(comprobante.Total)));

    private static string Money(decimal valor) => valor.ToString("F2", CultureInfo.InvariantCulture);
}
