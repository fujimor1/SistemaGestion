using System.Xml.Linq;
using Termales.Common.Settings;
using Termales.Entities.Models;

namespace Termales.BLL.Interfaces.Sunat;

public interface INotaCreditoXmlBuilder
{
    XDocument Construir(Comprobante notaCredito, Comprobante origen, EmpresaSettings empresa);
}
