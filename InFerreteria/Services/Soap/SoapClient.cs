//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;

//namespace InFerreteria.Services.Soap
//{
//    public class SoapClient
//    {
//        private readonly IHttpClientFactory _httpClientFactory;
//        public SoapClient(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

//        public async Task<string> PostAsync(string url, string soapAction, string envelopeXml)
//        {
//            var client = _httpClientFactory.CreateClient();
//            var request = new HttpRequestMessage(HttpMethod.Post, url);
//            request.Headers.Add("SOAPAction", soapAction);
//            request.Content = new StringContent(envelopeXml, Encoding.UTF8, "text/xml");
//            var response = await client.SendAsync(request);
//            response.EnsureSuccessStatusCode();
//            return await response.Content.ReadAsStringAsync();
//        }
//    }
//}
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InFerreteria.Services.Soap
{
    public class SoapClient
    {
        private readonly IHttpClientFactory _http;
        public SoapClient(IHttpClientFactory http) => _http = http;

        public async Task<string> PostAsync(string url, string soapAction, string envelopeXml)
        {
            var client = _http.CreateClient();

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("SOAPAction", soapAction);
            request.Content = new StringContent(envelopeXml, Encoding.UTF8, "text/xml");

            using var response = await client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            // Si falla, lanza con el cuerpo para diagnosticar (pero sin prefijos raros).
            response.EnsureSuccessStatusCode();
            return payload; // <- SOLO XML
        }
    }
}
