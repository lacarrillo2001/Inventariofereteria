using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using InFerreteria.Services.Soap;
using Microsoft.Extensions.Options;

namespace InFerreteria.Services
{
    public class ProveedoresSoapService
    {
        private readonly SoapClient _soap;
        private readonly SoapEndpointsOptions _opt;
        public ProveedoresSoapService(SoapClient soap, IOptions<SoapEndpointsOptions> opt)
        {
            _soap = soap; _opt = opt.Value;
        }

        public async Task<List<(int id, string nombre)>> ListarAsync()
        {
            var envelope = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <ListarProveedores xmlns=""http://tempuri.org/""/>
  </s:Body>
</s:Envelope>";
            var xml = await _soap.PostAsync(_opt.Proveedores, "http://tempuri.org/ListarProveedores", envelope);
            var doc = XDocument.Parse(xml);
            var items = doc.Descendants().Where(x => x.Name.LocalName == "ProveedorDto");
            return items.Select(x => (
                id: (int)x.Elements().First(e => e.Name.LocalName == "Id"),
                nombre: (string)x.Elements().First(e => e.Name.LocalName == "Nombre")
            )).ToList();
        }
    }
}
