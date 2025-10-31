using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using InFerreteria.Models;
using InFerreteria.Services.Soap;
using Ferreteria.Web.Models;    
using Microsoft.Extensions.Options;

namespace InFerreteria.Services
{
    public class ArticulosSoapService
    {
        private readonly SoapClient _soap;
        private readonly SoapEndpointsOptions _opt;
        public ArticulosSoapService(SoapClient soap, IOptions<SoapEndpointsOptions> opt)
        {
            _soap = soap; _opt = opt.Value;
        }

        // Helpers
        private static string Action(string op) => $"http://tempuri.org/IInventarioService/{op}";

        private static bool ParseSuccess(string xml)
        {
            var doc = XDocument.Parse(xml);
            return (bool?)doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "Success") ?? false;
        }
        private static string ParseMessage(string xml)
        {
            var doc = XDocument.Parse(xml);
            return (string?)doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "Message") ?? "Sin mensaje";
        }
        private static string? ParseCodigo(string xml)
        {
            var doc = XDocument.Parse(xml);
            return (string?)doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "Codigo");
        }

        // ============ INSERTAR (ya lo tenías, lo dejo igual) ============
        public async Task<(bool ok, string message, string? code)> InsertarAsync(ArticuloCreateDto dto)
        {
            var envelope = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"" xmlns:mod=""http://schemas.datacontract.org/2004/07/Ferreteria.Inventory.Models"">
  <soapenv:Header/>
  <soapenv:Body>
    <tem:InsertarArticulo>
      <tem:articulo>
        <mod:Codigo>{System.Security.SecurityElement.Escape(dto.Codigo)}</mod:Codigo>
        <mod:Nombre>{System.Security.SecurityElement.Escape(dto.Nombre)}</mod:Nombre>
        <mod:CategoriaId>{dto.CategoriaId}</mod:CategoriaId>
        <mod:ProveedorId>{dto.ProveedorId}</mod:ProveedorId>
        <mod:PrecioCompra>{dto.PrecioCompra}</mod:PrecioCompra>
        <mod:PrecioVenta>{dto.PrecioVenta}</mod:PrecioVenta>
        <mod:Stock>{dto.Stock}</mod:Stock>
        <mod:StockMinimo>{dto.StockMinimo}</mod:StockMinimo>
        <mod:Descripcion>{System.Security.SecurityElement.Escape(dto.Descripcion ?? "")}</mod:Descripcion>
      </tem:articulo>
    </tem:InsertarArticulo>
  </soapenv:Body>
</soapenv:Envelope>";
            var xml = await _soap.PostAsync(_opt.Inventario, Action("InsertarArticulo"), envelope);
            return (ParseSuccess(xml), ParseMessage(xml), ParseCodigo(xml));
        }

        // ============ LISTAR ============
        public async Task<List<ArticuloDto>> ListarAsync()
        {
            var envelope = @"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
  <soapenv:Header/>
  <soapenv:Body>
    <tem:ListarArticulos/>
  </soapenv:Body>
</soapenv:Envelope>";
            var xml = await _soap.PostAsync(_opt.Inventario, Action("ListarArticulos"), envelope);
            

            // quita BOM/espacios de seguridad
            var cleaned = xml.TrimStart('\uFEFF', '\u0000', ' ', '\t', '\r', '\n');

            // protección extra: si no arranca con '<', lanza detalle legible
            if (!cleaned.StartsWith("<"))
                throw new System.Exception("La respuesta no es XML. Primeros 200 chars: " +
                    (cleaned.Length > 200 ? cleaned.Substring(0, 200) : cleaned));

            var doc = XDocument.Parse(cleaned);

            // Ajusta el nodo según tu respuesta real (Data -> ArrayOfArticuloDto -> ArticuloDto)
            var items = doc.Descendants().Where(x => x.Name.LocalName == "ArticuloDto");
            var list = new List<ArticuloDto>();
            foreach (var x in items)
            {
                int GetInt(string n) => (int?)x.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? 0;
                decimal GetDec(string n) => (decimal?)x.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? 0m;
                string GetStrTrim(string n)=> ((string?)x.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? "").Trim();
                bool GetBool(string n) => (bool?)x.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? true;

                list.Add(new ArticuloDto
                {
                    Id = GetInt("Id"),
                    Codigo = GetStrTrim("Codigo"),
                    Nombre = GetStrTrim("Nombre"),
                    CategoriaId = GetInt("CategoriaId"),
                    ProveedorId = GetInt("ProveedorId"),
                    PrecioCompra = GetDec("PrecioCompra"),
                    PrecioVenta = GetDec("PrecioVenta"),
                    Stock = GetInt("Stock"),
                    StockMinimo = GetInt("StockMinimo"),
                    Descripcion = GetStrTrim("Descripcion"),
                    Activo = GetBool("Activo")
                });
            }
            return list;
        }

        // ============ ACTUALIZAR ============
        public async Task<(bool ok, string message)> ActualizarAsync(ArticuloEditVm vm)
        {
            var envelope = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"" xmlns:mod=""http://schemas.datacontract.org/2004/07/Ferreteria.Inventory.Models"">
  <soapenv:Header/>
  <soapenv:Body>
    <tem:ActualizarArticulo>
      <tem:articulo>
        <mod:Codigo>{System.Security.SecurityElement.Escape(vm.Codigo)}</mod:Codigo>
        <mod:Nombre>{System.Security.SecurityElement.Escape(vm.Nombre)}</mod:Nombre>
        <mod:CategoriaId>{vm.CategoriaId}</mod:CategoriaId>
        <mod:ProveedorId>{vm.ProveedorId}</mod:ProveedorId>
        <mod:PrecioCompra>{vm.PrecioCompra}</mod:PrecioCompra>
        <mod:PrecioVenta>{vm.PrecioVenta}</mod:PrecioVenta>
        <mod:Stock>{vm.Stock}</mod:Stock>
        <mod:StockMinimo>{vm.StockMinimo}</mod:StockMinimo>
        <mod:Descripcion>{System.Security.SecurityElement.Escape(vm.Descripcion ?? "")}</mod:Descripcion>
        <mod:Activo>{(vm.Activo ? "true" : "false")}</mod:Activo>
      </tem:articulo>
    </tem:ActualizarArticulo>
  </soapenv:Body>
</soapenv:Envelope>";
            var xml = await _soap.PostAsync(_opt.Inventario, Action("ActualizarArticulo"), envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }

        // ============ ELIMINAR LÓGICO (Inactivar) por código ============
        public async Task<(bool ok, string message)> InactivarAsync(string codigo)
        {
            var envelope = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
  <soapenv:Header/>
  <soapenv:Body>
    <tem:EliminarArticulo>
      <tem:codigo>{System.Security.SecurityElement.Escape(codigo)}</tem:codigo>
    </tem:EliminarArticulo>
  </soapenv:Body>
</soapenv:Envelope>";
            var xml = await _soap.PostAsync(_opt.Inventario, Action("EliminarArticulo"), envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }

        // ============ ACTIVAR por código ============
        public async Task<(bool ok, string message)> ActivarAsync(string codigo)
        {
            var envelope = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
  <soapenv:Header/>
  <soapenv:Body>
    <tem:ActivarArticulo>
      <tem:codigo>{System.Security.SecurityElement.Escape(codigo)}</tem:codigo>
    </tem:ActivarArticulo>
  </soapenv:Body>
</soapenv:Envelope>";
            var xml = await _soap.PostAsync(_opt.Inventario, Action("ActivarArticulo"), envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }

        // ============ ELIMINAR FÍSICO por Id ============
        public async Task<(bool ok, string message)> EliminarPorIdAsync(int id)
        {
            var envelope = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
  <soapenv:Header/>
  <soapenv:Body>
    <tem:EliminarArticuloPorId>
      <tem:id>{id}</tem:id>
    </tem:EliminarArticuloPorId>
  </soapenv:Body>
</soapenv:Envelope>";
            var xml = await _soap.PostAsync(_opt.Inventario, Action("EliminarArticuloPorId"), envelope);
            return (ParseSuccess(xml), ParseMessage(xml));
        }

        // ============ CONSULTAR por CÓDIGO ============


        public async Task<ArticuloDto?> ConsultarPorCodigoAsync(string codigo)
        {
            codigo = (codigo ?? "").Trim();

            var envelope = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
  <soapenv:Header/>
  <soapenv:Body>
    <tem:ConsultarArticuloPorCodigo>
      <tem:codigo>{System.Security.SecurityElement.Escape(codigo)}</tem:codigo>
    </tem:ConsultarArticuloPorCodigo>
  </soapenv:Body>
</soapenv:Envelope>";

            // Usa el SOAPAction con interfaz (ajústalo si tu host usa el corto)
            var xml = await _soap.PostAsync(_opt.Inventario,
                "http://tempuri.org/IInventarioService/ConsultarArticuloPorCodigo", envelope);

            var cleaned = xml.TrimStart('\uFEFF', '\u0000', ' ', '\t', '\r', '\n');
            if (!cleaned.StartsWith("<"))
                throw new System.Exception("Respuesta no XML en ConsultarPorCodigo. Inicio: " + (cleaned.Length > 200 ? cleaned[..200] : cleaned));

            var doc = XDocument.Parse(cleaned);

            // 🔎 El nodo que queremos es: <ConsultarArticuloPorCodigoResult> ... </...>
            var result = doc.Descendants().FirstOrDefault(n => n.Name.LocalName == "ConsultarArticuloPorCodigoResult");
            if (result == null)
                return null; // no hay datos para ese código

            // Helpers de lectura (independientes del namespace)
            int GetInt(string n) => (int?)result.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? 0;
            decimal GetDec(string n) => (decimal?)result.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? 0m;
            string GetStrTrim(string n) => ((string?)result.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? "").Trim();
            bool GetBool(string n) => (bool?)result.Elements().FirstOrDefault(e => e.Name.LocalName == n) ?? true;

            return new ArticuloDto
            {
                // OJO: tu XML no trae <Id>, así que lo dejamos en 0
                Id = 0,
                Codigo = GetStrTrim("Codigo"),
                Nombre = GetStrTrim("Nombre"),
                CategoriaId = GetInt("CategoriaId"),
                ProveedorId = GetInt("ProveedorId"),
                PrecioCompra = GetDec("PrecioCompra"),
                PrecioVenta = GetDec("PrecioVenta"),
                Stock = GetInt("Stock"),
                StockMinimo = GetInt("StockMinimo"),
                Descripcion = GetStrTrim("Descripcion"),
                Activo = GetBool("Activo")
            };
        }


    }
}
