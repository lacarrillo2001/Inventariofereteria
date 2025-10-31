//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Xml.Linq;
//using InFerreteria.Services.Soap;
//using Microsoft.Extensions.Options;

//namespace InFerreteria.Services
//{
//    public class CategoriasSoapService
//    {
//        private readonly SoapClient _soap;
//        private readonly SoapEndpointsOptions _opt;
//        public CategoriasSoapService(SoapClient soap, IOptions<SoapEndpointsOptions> opt)
//        {
//            _soap = soap; _opt = opt.Value;
//        }

//        public async Task<List<(int id, string nombre)>> ListarAsync()
//        {
//            var envelope = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
//  <s:Body>
//    <ListarCategorias xmlns=""http://tempuri.org/""/>
//  </s:Body>
//</s:Envelope>";
//            var xml = await _soap.PostAsync(_opt.Categorias, "http://tempuri.org/ListarCategorias", envelope);
//            // Parseo: ajusta los namespaces según tu respuesta real
//            var doc = XDocument.Parse(xml);
//            // Ejemplo básico de extracción (adapta a la forma de tu <Data>)
//            var items = doc.Descendants().Where(x => x.Name.LocalName == "CategoriaDto");
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
using InFerreteria.Services.Soap;
using Microsoft.Extensions.Options;
using InFerreteria.Models;

namespace InFerreteria.Services
{
    public class CategoriasSoapService
    {
        private readonly SoapClient _soap;
        private readonly SoapEndpointsOptions _opt;

        public CategoriasSoapService(SoapClient soap, IOptions<SoapEndpointsOptions> opt)
        {
            _soap = soap;
            _opt = opt.Value;
        }

        // Intenta con interfaz y sin interfaz
        private async Task<string> PostTryAsync(string op, string envelope)
        {
            // 1) con interfaz (común en SoapCore)
            var xml = await _soap.PostAsync(_opt.Categorias, $"http://tempuri.org/IInventarioService/{op}", envelope);
            if (xml?.TrimStart().StartsWith("<") == true) return xml;

            // 2) sin interfaz (algunos hosts)
            xml = await _soap.PostAsync(_opt.Categorias, $"http://tempuri.org/{op}", envelope);
            return xml;
        }

        // ---------- INSERTAR ----------
        public async Task<(bool ok, string message)> InsertarAsync(CategoriaCreateVm vm)
        {
            var envelope = $@"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <InsertarCategoria xmlns=""http://tempuri.org/"">
      <input>
        <Id>0</Id>
        <Nombre>{System.Security.SecurityElement.Escape(vm.Nombre.Trim())}</Nombre>
        <Descripcion>{System.Security.SecurityElement.Escape(vm.Descripcion ?? "")}</Descripcion>
        <Activo>{(vm.Activo ? "true" : "false")}</Activo>
      </input>
    </InsertarCategoria>
  </s:Body>
</s:Envelope>";
            var xml = await PostTryAsync("InsertarCategoria", envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }

        // ---------- ACTUALIZAR ----------
        public async Task<(bool ok, string message)> ActualizarAsync(CategoriaEditVm vm)
        {
            var envelope = $@"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <ActualizarCategoria xmlns=""http://tempuri.org/"">
      <input>
        <Id>{vm.Id}</Id>
        <Nombre>{System.Security.SecurityElement.Escape(vm.Nombre.Trim())}</Nombre>
        <Descripcion>{System.Security.SecurityElement.Escape(vm.Descripcion ?? "")}</Descripcion>
        <Activo>{(vm.Activo ? "true" : "false")}</Activo>
      </input>
    </ActualizarCategoria>
  </s:Body>
</s:Envelope>";
            var xml = await PostTryAsync("ActualizarCategoria", envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }

        // ---------- LISTAR ----------
        public async Task<List<CategoriaDto>> ListarAsync()
        {
            var envelope = @"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <ListarCategorias xmlns=""http://tempuri.org/"" />
  </s:Body>
</s:Envelope>";
            var xml = await PostTryAsync("ListarCategorias", envelope);
            var doc = LoadXml(xml);

            // Busca cualquier elemento que represente una categoría (agnóstico al namespace)
            var items = doc.Descendants().Where(n =>
                n.Name.LocalName == "CategoriaDto" ||
                (n.Parent != null && n.Parent.Name.LocalName.Contains("Array") && // por si viene como ArrayOf...
                 n.Name.LocalName == "Categoria") ||
                (n.Elements().Any(e => e.Name.LocalName == "Id" && e.Parent == n &&
                                       n.Elements().Any(e2 => e2.Name.LocalName == "Nombre")))
            );

            var list = new List<CategoriaDto>();
            foreach (var x in items)
            {
                int GetInt(string n) => (int?)x.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? 0;
                string GetStrTrim(string n) => ((string?)x.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? "").Trim();
                bool GetBool(string n) => (bool?)x.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? true;

                list.Add(new CategoriaDto
                {
                    Id = GetInt("Id"),
                    Nombre = GetStrTrim("Nombre"),
                    Descripcion = GetStrTrim("Descripcion"),
                    Activo = GetBool("Activo")
                });
            }
            return list.OrderBy(c => c.Id).ToList();
        }

        // ---------- CONSULTAR POR ID ----------
        public async Task<CategoriaDto?> ConsultarPorIdAsync(int id)
        {
            var envelope = $@"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <ConsultarCategoriaPorId xmlns=""http://tempuri.org/"">
      <id>{id}</id>
    </ConsultarCategoriaPorId>
  </s:Body>
</s:Envelope>";
            var xml = await PostTryAsync("ConsultarCategoriaPorId", envelope);
            var doc = LoadXml(xml);

            var result = doc.Descendants().FirstOrDefault(n => n.Name.LocalName == "ConsultarCategoriaPorIdResult");
            if (result == null) return null;

            int GetInt(string n) => (int?)result.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? 0;
            string GetStrTrim(string n) => ((string?)result.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? "").Trim();
            bool GetBool(string n) => (bool?)result.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? true;

            return new CategoriaDto
            {
                Id = GetInt("Id"),
                Nombre = GetStrTrim("Nombre"),
                Descripcion = GetStrTrim("Descripcion"),
                Activo = GetBool("Activo")
            };
        }

        // ---------- INACTIVAR ----------
        public async Task<(bool ok, string message)> InactivarAsync(int id)
        {
            var envelope = $@"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <InactivarCategoria xmlns=""http://tempuri.org/"">
      <id>{id}</id>
    </InactivarCategoria>
  </s:Body>
</s:Envelope>";
            var xml = await PostTryAsync("InactivarCategoria", envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }

        // ---------- ACTIVAR ----------
        public async Task<(bool ok, string message)> ActivarAsync(int id)
        {
            var envelope = $@"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <ActivarCategoria xmlns=""http://tempuri.org/"">
      <id>{id}</id>
    </ActivarCategoria>
  </s:Body>
</s:Envelope>";
            var xml = await PostTryAsync("ActivarCategoria", envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }

        // ---------- ELIMINAR (físico) ----------
        public async Task<(bool ok, string message)> EliminarAsync(int id)
        {
            var envelope = $@"
<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Body>
    <EliminarCategoria xmlns=""http://tempuri.org/"">
      <id>{id}</id>
    </EliminarCategoria>
  </s:Body>
</s:Envelope>";
            var xml = await PostTryAsync("EliminarCategoria", envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }

        // --------- utilidades parse ----------
        private static XDocument LoadXml(string xml)
        {
            var cleaned = (xml ?? "").TrimStart('\uFEFF', '\u0000', ' ', '\t', '\r', '\n');
            if (!cleaned.StartsWith("<"))
                throw new System.Exception("Respuesta no XML. Inicio: " + (cleaned.Length > 200 ? cleaned[..200] : cleaned));
            return XDocument.Parse(cleaned);
        }

        // algunos servicios devuelven Success/Message, otros no; soporta ambos
        private static bool ParseSuccess(string xml)
        {
            var doc = LoadXml(xml);
            var s = doc.Descendants().FirstOrDefault(n => n.Name.LocalName == "Success");
            if (s == null) return true; // si no hay bandera, asumimos OK si no hubo fault
            return (bool?)s ?? false;
        }
        private static string ParseMessage(string xml)
        {
            var doc = LoadXml(xml);
            return (string?)doc.Descendants().FirstOrDefault(n => n.Name.LocalName == "Message") ?? "Operación completada.";
        }
    }
}
