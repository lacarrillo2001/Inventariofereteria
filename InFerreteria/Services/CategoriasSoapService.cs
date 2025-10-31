using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using InFerreteria.Services.Soap;
using Microsoft.Extensions.Options;

namespace InFerreteria.Services
{
    public class CategoriasSoapService
    {
        private readonly SoapClient _soap;
        private readonly SoapEndpointsOptions _opt;
        public CategoriasSoapService(SoapClient soap, IOptions<SoapEndpointsOptions> opt)
        {
            _soap = soap; _opt = opt.Value;
        }

        public async Task<List<(int id, string nombre)>> ListarAsync()
        {
            var envelope = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <ListarCategorias xmlns=""http://tempuri.org/""/>
  </s:Body>
</s:Envelope>";
            var xml = await _soap.PostAsync(_opt.Categorias, "http://tempuri.org/ListarCategorias", envelope);
            // Parseo: ajusta los namespaces según tu respuesta real
            var doc = XDocument.Parse(xml);
            // Ejemplo básico de extracción (adapta a la forma de tu <Data>)
            var items = doc.Descendants().Where(x => x.Name.LocalName == "CategoriaDto");
            return items.Select(x => (
                id: (int)x.Elements().First(e => e.Name.LocalName == "Id"),
                nombre: (string)x.Elements().First(e => e.Name.LocalName == "Nombre")
            )).ToList();
        }
    }
}
