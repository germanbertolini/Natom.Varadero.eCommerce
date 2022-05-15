using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web;

namespace natom.varadero.ecomm.Managers
{
    public static class CotizacionHelper
    {
        public static decimal ObtenerCotizacion()
        {
            var cotizacion = new ResponseCotizacion() { Compra = 1, Venta = 1 };

            try
            {
                Uri address = new Uri("https://api-dolar-argentina.herokuapp.com/api/dolarblue");

                ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

                using (WebClient webClient = new WebClient())
                {
                    var stream = webClient.OpenRead(address);
                    var response = "";
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        response = sr.ReadToEnd();
                    }
                    cotizacion = JsonConvert.DeserializeObject<ResponseCotizacion>(response);
                }
            }
            catch (Exception ex)
            {
                cotizacion.Venta = (decimal)118.90;
            }

            return cotizacion.Venta;
        }

        /// <summary>
        /// Certificate validation callback.
        /// </summary>
        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (error == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'",
                cert.Subject,
                error.ToString());

            return false;
        }

        public class ResponseCotizacion
        {
            public string Fecha { get; set; }
            public decimal Compra { get; set; }
            public decimal Venta { get; set; }
        }
    }
}