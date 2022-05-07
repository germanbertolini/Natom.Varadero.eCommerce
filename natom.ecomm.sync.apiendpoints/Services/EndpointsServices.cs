using natom.ecomm.sync.apiendpoints.Models.EnviarArticulos;
using natom.ecomm.sync.apiendpoints.Models.EnviarClientes;
using natom.ecomm.sync.apiendpoints.Models.EnviarListaDePrecios;
using natom.ecomm.sync.apiendpoints.Models.GetToken;
using natom.ecomm.sync.apiendpoints.Models.RecibirPedidos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace natom.ecomm.sync.apiendpoints.Services
{
    public class EndpointsServices
    {
        public static async Task<TokenResponse> GetToken()
        {
            using (HttpClient client = new HttpClient())
            {
                var dict = new Dictionary<string, string>();
                dict.Add("grant_type", "password");
                dict.Add("username", "DVaradero");
                dict.Add("password", "V@r@De21");
                dict.Add("client_id", "");
                var req = new HttpRequestMessage(HttpMethod.Post, "https://sms-180ws.sms.com.ar/WSD_DVaradero_carrito/oauth/token") { Content = new FormUrlEncodedContent(dict) };
                var res = await client.SendAsync(req);

                var result = JsonConvert.DeserializeObject<TokenResponse>(res.Content.ReadAsStringAsync().Result);
                return result;
            }
        }

        public static async Task<List<EnviarClientesDto>> GetEnviarClientes()
        {
            var token = await GetToken();

            using (var client = new WebClient())
            {
                client.Headers.Add("Accept", "application/json");
                client.Headers.Add("Content-Type", "application/json; charset=utf-8");
                client.Headers.Add("Authorization", "Bearer " + token.access_token);

                string response = client.DownloadString("https://sms-180ws.sms.com.ar/WSD_DVaradero_Carrito/api/wsd/ObtenerClientes");

                var result = JsonConvert.DeserializeObject<List<EnviarClientesDto>>(response);
                return result;
            }

            //using (HttpClient client = new HttpClient())
            //{
            //    HttpRequestMessage httpRequest = new HttpRequestMessage();
            //    httpRequest.Method = HttpMethod.Get;
            //    httpRequest.RequestUri = new Uri("https://sms-180ws.sms.com.ar/WSD_DVaradero_Carrito/api/wsd/ObtenerClientes");
            //    //httpRequest.Content = new StringContent("", Encoding.UTF8, "application/json");
            //    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
            //    //httpRequest.Headers.Add("Content-Type", "application/json");

            //    var response = await client.SendAsync(httpRequest);


            //    return JsonConvert.DeserializeObject<List<EnviarClientesDto>>(response.Content.ReadAsStringAsync().Result);
            //}
        }

        public static async Task<List<EnviarArticulosDto>> GetEnviarArticulos()
        {
            var token = await GetToken();

            using (var client = new WebClient())
            {
                client.Headers.Add("Accept", "application/json");
                client.Headers.Add("Content-Type", "application/json; charset=utf-8");
                client.Headers.Add("Authorization", "Bearer " + token.access_token);

                string response = client.DownloadString("https://sms-180ws.sms.com.ar/WSD_DVaradero_Carrito/api/wsd/EnviarArticulos");

                var result = JsonConvert.DeserializeObject<List<EnviarArticulosDto>>(response);
                return result;
            }
        }

        public static async Task<List<EnviarListadePreciosDto>> GetEnviarListaDePrecios()
        {
            var token = await GetToken();

            using (var client = new ExtendedWebClient(new Uri("https://sms-180ws.sms.com.ar/WSD_DVaradero_Carrito/api/wsd/EnviarListasP")))
            {
                client.Headers.Add("Accept", "application/json");
                client.Headers.Add("Content-Type", "application/json; charset=utf-8");
                client.Headers.Add("Authorization", "Bearer " + token.access_token);

                string response = client.DownloadString("https://sms-180ws.sms.com.ar/WSD_DVaradero_Carrito/api/wsd/EnviarListasP");

                var result = JsonConvert.DeserializeObject<List<EnviarListadePreciosDto>>(response);
                return result;
            }
        }

        public static async Task PostRecibirPedidos(List<RecibirPedidosDto> pedidos)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage httpRequest = new HttpRequestMessage();
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = new Uri("https://sms-180ws.sms.com.ar/WSD_DVaradero_Carrito/api/wsd/AltaPed");
                httpRequest.Content = new StringContent(JsonConvert.SerializeObject(pedidos), Encoding.UTF8, "application/json");
                var res = await client.SendAsync(httpRequest);
            }
        }

        //Creo esta clase para setear el timeout del WebClient
        public class ExtendedWebClient : WebClient
        {

            private int timeout;
            public int Timeout
            {
                get
                {
                    return timeout;
                }
                set
                {
                    timeout = value;
                }
            }
            public ExtendedWebClient(Uri address)
            {
                this.timeout = 600000;//In Milli seconds
                var objWebClient = GetWebRequest(address);
            }
            protected override WebRequest GetWebRequest(Uri address)
            {
                var objWebRequest = base.GetWebRequest(address);
                objWebRequest.Timeout = this.timeout;
                return objWebRequest;
            }
        }
    }
}
