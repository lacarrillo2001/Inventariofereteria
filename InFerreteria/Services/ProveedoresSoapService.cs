//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Xml.Linq;
//using InFerreteria.Services.Soap;
//using Microsoft.Extensions.Options;

//namespace InFerreteria.Services
//{
//    public class ProveedoresSoapService
//    {
//        private readonly SoapClient _soap;
//        private readonly SoapEndpointsOptions _opt;
//        public ProveedoresSoapService(SoapClient soap, IOptions<SoapEndpointsOptions> opt)
//        {
//            _soap = soap; _opt = opt.Value;
//        }

//        public async Task<List<(int id, string nombre)>> ListarAsync()
//        {
//            var envelope = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
//  <s:Body>
//    <ListarProveedores xmlns=""http://tempuri.org/""/>
//  </s:Body>
//</s:Envelope>";
//            var xml = await _soap.PostAsync(_opt.Proveedores, "http://tempuri.org/ListarProveedores", envelope);
//            var doc = XDocument.Parse(xml);
//            var items = doc.Descendants().Where(x => x.Name.LocalName == "ProveedorDto");
//            return items.Select(x => (
//                id: (int)x.Elements().First(e => e.Name.LocalName == "Id"),
//                nombre: (string)x.Elements().First(e => e.Name.LocalName == "Nombre")
//            )).ToList();
//        }
//    }
//}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using InFerreteria.Models;
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

        // Intenta acción con interfaz y sin interfaz (evita 404)
        private async Task<string> PostTryAsync(string op, string envelope)
        {
            var xml = await _soap.PostAsync(_opt.Proveedores, $"http://tempuri.org/IInventarioService/{op}", envelope);
            if ((xml ?? "").TrimStart().StartsWith("<")) return xml;
            return await _soap.PostAsync(_opt.Proveedores, $"http://tempuri.org/{op}", envelope);
        }

        private static XDocument LoadXml(string xml)
        {
            var cleaned = (xml ?? "").TrimStart('\uFEFF', '\u0000', ' ', '\t', '\r', '\n');
            if (!cleaned.StartsWith("<"))
                throw new System.Exception("Respuesta no XML. Inicio: " + (cleaned.Length > 200 ? cleaned[..200] : cleaned));
            return XDocument.Parse(cleaned);
        }
        private static bool ParseSuccess(string xml)
        {
            var doc = LoadXml(xml);
            var s = doc.Descendants().FirstOrDefault(n => n.Name.LocalName == "Success");
            if (s == null) return true;
            return (bool?)s ?? false;
        }
        private static string ParseMessage(string xml)
        {
            var doc = LoadXml(xml);
            return (string?)doc.Descendants().FirstOrDefault(n => n.Name.LocalName == "Message") ?? "Operación completada.";
        }

        // -------- INSERTAR --------
        public async Task<(bool ok, string message)> InsertarAsync(ProveedorCreateVm vm)
        {
            var envelope = $@"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <InsertarProveedor xmlns=""http://tempuri.org/"">
      <input>
        <Nombre>{System.Security.SecurityElement.Escape(vm.Nombre.Trim())}</Nombre>
        <Contacto>{System.Security.SecurityElement.Escape(vm.Contacto ?? "")}</Contacto>
        <Ruc>{System.Security.SecurityElement.Escape(vm.Ruc ?? "")}</Ruc>
        <Correo>{System.Security.SecurityElement.Escape(vm.Correo ?? "")}</Correo>
        <Direccion>{System.Security.SecurityElement.Escape(vm.Direccion ?? "")}</Direccion>
        <Activo>{(vm.Activo ? "true" : "false")}</Activo>
      </input>
    </InsertarProveedor>
  </s:Body>
</s:Envelope>";
            var xml = await PostTryAsync("InsertarProveedor", envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }

        // -------- ACTUALIZAR --------
        public async Task<(bool ok, string message)> ActualizarAsync(ProveedorEditVm vm)
        {
            var envelope = $@"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <ActualizarProveedor xmlns=""http://tempuri.org/"">
      <input>
        <Id>{vm.Id}</Id>
        <Nombre>{System.Security.SecurityElement.Escape(vm.Nombre.Trim())}</Nombre>
        <Contacto>{System.Security.SecurityElement.Escape(vm.Contacto ?? "")}</Contacto>
        <Ruc>{System.Security.SecurityElement.Escape(vm.Ruc ?? "")}</Ruc>
        <Correo>{System.Security.SecurityElement.Escape(vm.Correo ?? "")}</Correo>
        <Direccion>{System.Security.SecurityElement.Escape(vm.Direccion ?? "")}</Direccion>
        <Activo>{(vm.Activo ? "true" : "false")}</Activo>
      </input>
    </ActualizarProveedor>
  </s:Body>
</s:Envelope>";
            var xml = await PostTryAsync("ActualizarProveedor", envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }

        // -------- LISTAR --------
        public async Task<List<ProveedorDto>> ListarAsync()
        {
            var envelope = @"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <ListarProveedores xmlns=""http://tempuri.org/"" />
  </s:Body>
</s:Envelope>";
            var xml = await PostTryAsync("ListarProveedores", envelope);
            var doc = LoadXml(xml);

            // Intentamos detectar nodos de proveedor (agnóstico a namespaces)
            var items = doc.Descendants().Where(n =>
                 n.Name.LocalName == "ProveedorDto" ||
                 (n.Elements().Any(e => e.Name.LocalName == "Id") &&
                  n.Elements().Any(e => e.Name.LocalName == "Nombre") &&
                  n.Elements().Any(e => e.Name.LocalName == "Activo"))
            );

            var list = new List<ProveedorDto>();
            foreach (var x in items)
            {
                int GetInt(string n) => (int?)x.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? 0;
                string GetStr(string n) => ((string?)x.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? "").Trim();
                bool GetBool(string n) => (bool?)x.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? true;

                list.Add(new ProveedorDto
                {
                    Id = GetInt("Id"),
                    Nombre = GetStr("Nombre"),
                    Contacto = GetStr("Contacto"),
                    Ruc = GetStr("Ruc"),
                    Correo = GetStr("Correo"),
                    Direccion = GetStr("Direccion"),
                    Activo = GetBool("Activo")
                });
            }
            return list.OrderBy(p => p.Id).ToList();
        }

        // -------- CONSULTAR POR ID --------
        public async Task<ProveedorDto?> ConsultarPorIdAsync(int id)
        {
            var envelope = $@"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <ConsultarProveedorPorId xmlns=""http://tempuri.org/"">
      <id>{id}</id>
    </ConsultarProveedorPorId>
  </s:Body>
</s:Envelope>";

            var xml = await PostTryAsync("ConsultarProveedorPorId", envelope);
            var doc = LoadXml(xml);

            // Tu respuesta real tiene <ConsultarProveedorPorIdResult> ... </...>
            var result = doc.Descendants().FirstOrDefault(n => n.Name.LocalName == "ConsultarProveedorPorIdResult");
            if (result == null) return null;

            int GetInt(string n) => (int?)result.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? 0;
            string GetStr(string n) => ((string?)result.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? "").Trim();
            bool GetBool(string n) => (bool?)result.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? true;

            return new ProveedorDto
            {
                Id = GetInt("Id"),
                Nombre = GetStr("Nombre"),
                Contacto = GetStr("Contacto"),
                Ruc = GetStr("Ruc"),
                Correo = GetStr("Correo"),
                Direccion = GetStr("Direccion"),
                Activo = GetBool("Activo")
            };
        }

        // -------- INACTIVAR --------
        public async Task<(bool ok, string message)> InactivarAsync(int id)
        {
            var envelope = $@"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <InactivarProveedor xmlns=""http://tempuri.org/"">
      <id>{id}</id>
    </InactivarProveedor>
  </s:Body>
</s:Envelope>";
            var xml = await PostTryAsync("InactivarProveedor", envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }

        // -------- ACTIVAR --------
        public async Task<(bool ok, string message)> ActivarAsync(int id)
        {
            var envelope = $@"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <ActivarProveedor xmlns=""http://tempuri.org/"">
      <id>{id}</id>
    </ActivarProveedor>
  </s:Body>
</s:Envelope>";
            var xml = await PostTryAsync("ActivarProveedor", envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }

        // -------- ELIMINAR (físico) --------
        public async Task<(bool ok, string message)> EliminarAsync(int id)
        {
            var envelope = $@"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <EliminarProveedor xmlns=""http://tempuri.org/"">
      <id>{id}</id>
    </EliminarProveedor>
  </s:Body>
</s:Envelope>";
            var xml = await PostTryAsync("EliminarProveedor", envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }
    }
}
