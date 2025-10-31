using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using WebApp.ViewModels;

namespace WebApp.Services;

public interface IInventarioSoapClient
{
    Task<(bool ok, string message)> InsertarArticuloAsync(ArticuloCreateVm vm, CancellationToken ct = default);
    // Luego añadimos: ActualizarArticuloAsync, EliminarArticuloAsync, etc.
}

public class InventarioSoapClient : IInventarioSoapClient
{
    private readonly HttpClient _http;

    // IMPORTANTE: usa SIEMPRE los prefijos que tu servidor espera:
    // tem = http://tempuri.org/
    // mod = http://schemas.datacontract.org/2004/07/Ferreteria.Inventory.Models
    private const string NsTem = "http://tempuri.org/";
    private const string NsMod = "http://schemas.datacontract.org/2004/07/Ferreteria.Inventory.Models";
    private const string SoapActionBase = "http://tempuri.org/IInventarioSoapService/";

    public InventarioSoapClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool ok, string message)> InsertarArticuloAsync(ArticuloCreateVm vm, CancellationToken ct = default)
    {
        var soapAction = $"{SoapActionBase}InsertarArticulo";
        var envelope = BuildInsertEnvelope(vm);

        using var req = new HttpRequestMessage(HttpMethod.Post, "");
        req.Content = new StringContent(envelope, Encoding.UTF8, "text/xml");
        // algunos hosts requieren comillas
        req.Headers.TryAddWithoutValidation("SOAPAction", $"\"{soapAction}\"");

        using var resp = await _http.SendAsync(req, ct);
        var xml = await resp.Content.ReadAsStringAsync(ct);

        // --- Parseo robusto: busca por LocalName para no depender del namespace ---
        try
        {
            var xdoc = XDocument.Parse(xml);
            var successEl = xdoc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Success");
            var messageEl = xdoc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Message");

            // si el servicio devuelve validaciones en otra etiqueta, también las cazamos
            var validationEl = xdoc.Descendants().FirstOrDefault(e => e.Name.LocalName is "Errors" or "Error" or "ValidationMessage");

            bool ok = string.Equals(successEl?.Value, "true", StringComparison.OrdinalIgnoreCase);
            string msg = messageEl?.Value
                         ?? validationEl?.Value
                         ?? (!resp.IsSuccessStatusCode ? $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}" : "Sin mensaje");

            return (ok, msg);
        }
        catch
        {
            // Devuelve el XML crudo para diagnosticar
            return (false, $"No se pudo interpretar la respuesta SOAP.\nRespuesta:\n{xml}");
        }
    }

    private static string BuildInsertEnvelope(ArticuloCreateVm vm)
    {
        // OBLIGATORIO: usar los prefijos tem y mod y RESPETAR MAYÚSCULAS en propiedades
        // También asegurar punto decimal con InvariantCulture
        string d(decimal v) => v.ToString(System.Globalization.CultureInfo.InvariantCulture);

        string Encode(string? s) => System.Security.SecurityElement.Escape(s ?? "");

        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""{NsTem}"" xmlns:mod=""{NsMod}"">
  <soapenv:Header/>
  <soapenv:Body>
    <tem:InsertarArticulo>
      <tem:articulo>
        <mod:Codigo>{Encode(vm.Codigo)}</mod:Codigo>
        <mod:Nombre>{Encode(vm.Nombre)}</mod:Nombre>
        <mod:CategoriaId>{vm.CategoriaId}</mod:CategoriaId>
        <mod:ProveedorId>{vm.ProveedorId}</mod:ProveedorId>
        <mod:PrecioCompra>{d(vm.PrecioCompra)}</mod:PrecioCompra>
        <mod:PrecioVenta>{d(vm.PrecioVenta)}</mod:PrecioVenta>
        <mod:Stock>{vm.Stock}</mod:Stock>
        <mod:StockMinimo>{vm.StockMinimo}</mod:StockMinimo>
        <mod:Descripcion>{Encode(vm.Descripcion)}</mod:Descripcion>
        <!-- Activo es opcional; si tu servicio lo admite, descomenta -->
        <!-- <mod:Activo>{vm.Activo.ToString().ToLowerInvariant()}</mod:Activo> -->
      </tem:articulo>
    </tem:InsertarArticulo>
  </soapenv:Body>
</soapenv:Envelope>";
    }
}
