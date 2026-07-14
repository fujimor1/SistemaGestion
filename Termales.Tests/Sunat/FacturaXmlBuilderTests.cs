using System.Xml.Schema;
using Termales.BLL.Services.Sunat.Xml;

namespace Termales.Tests.Sunat;

public class FacturaXmlBuilderTests
{
    [Fact]
    public void Construir_GeneraXmlValidoContraElXsdOficialDeSunat()
    {
        var xml = new FacturaXmlBuilder().Construir(FacturaMuestras.Comprobante(), FacturaMuestras.Empresa());

        var schemas = new XmlSchemaSet();
        var commonDir = Path.Combine(AppContext.BaseDirectory, "Schemas", "common");
        schemas.Add(null, Path.Combine(commonDir, "CCTS_CCT_SchemaModule-2.1.xsd"));
        schemas.Add(null, Path.Combine(commonDir, "UBL-CoreComponentParameters-2.1.xsd"));
        schemas.Add(null, Path.Combine(commonDir, "UBL-QualifiedDataTypes-2.1.xsd"));
        schemas.Add(null, Path.Combine(commonDir, "UBL-UnqualifiedDataTypes-2.1.xsd"));
        schemas.Add(null, Path.Combine(commonDir, "UBL-CommonBasicComponents-2.1.xsd"));
        schemas.Add(null, Path.Combine(commonDir, "UBL-ExtensionContentDataType-2.1.xsd"));
        schemas.Add(null, Path.Combine(commonDir, "UBL-CommonExtensionComponents-2.1.xsd"));
        schemas.Add(null, Path.Combine(commonDir, "UBL-CommonAggregateComponents-2.1.xsd"));
        var xsdPath = Path.Combine(AppContext.BaseDirectory, "Schemas", "maindoc", "UBL-Invoice-2.1.xsd");
        schemas.Add(null, xsdPath);

        var errores = new List<string>();
        xml.Validate(schemas, (_, e) => errores.Add($"{e.Severity}: {e.Message}"));

        Assert.True(errores.Count == 0, string.Join("\n", errores));
    }
}
