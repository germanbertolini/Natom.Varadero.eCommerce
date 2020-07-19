using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace natom.ecomm.sync.kernel
{
    public class ServiceAccess
    {
        public static async Task<EndpointResponse<T>> DoGet<T>(string relativeUrl)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = MakeUrl(relativeUrl);
                    var taskGet = client.GetStringAsync(url);
                    Task.WaitAll(taskGet);
                    var content = taskGet.Result; //await client.GetStringAsync(url);
                    return JsonConvert.DeserializeObject<EndpointResponse<T>>(content);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<EndpointResponse<T>> DoPost<T>(string relativeUrl, object data)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = MakeUrl(relativeUrl);
                    var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    var taskPost = client.PostAsync(url, content);
                    Task.WaitAll(taskPost);
                    var result = taskPost.Result; //await client.PostAsync(url, content);
                    string returned = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<EndpointResponse<T>>(returned);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string MakeUrl(string relativeUrl)
        {
            string url = ConfigurationManager.AppSettings["eCommerce.URL"];
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception("Falta configurar la key [eCommerce.URL] en el app.config");
            }
            if (!url[url.Length - 1].Equals('/'))
            {
                url += "/";
            }
            if (relativeUrl.Length > 0 && relativeUrl.StartsWith("/"))
            {
                relativeUrl = relativeUrl.Substring(1, relativeUrl.Length - 1);
            }
            return url + relativeUrl;
        }
    }
}
