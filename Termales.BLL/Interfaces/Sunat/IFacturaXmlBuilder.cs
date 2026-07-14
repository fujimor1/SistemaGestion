using System.Xml.Linq;
using Termales.Common.Settings;
using Termales.Entities.Models;

namespace Termales.BLL.Interfaces.Sunat;

public interface IFacturaXmlBuilder
{
    XDocument Construir(Comprobante comprobante, EmpresaSettings empresa);
}
