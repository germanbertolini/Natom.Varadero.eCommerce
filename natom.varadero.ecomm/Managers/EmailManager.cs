using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Data.Entity;
using natom.varadero.entities;

namespace natom.varadero.ecomm.Managers
{
    public class EmailManager
    {
        public static void Enviar(string subject, string body, List<MailAddress> addresses)
        {
            try
            {
                var fromAddress = new MailAddress(ConfigurationManager.AppSettings["Varadero.Email.Emisor"], "e-Varadero");
                string fromPassword = ConfigurationManager.AppSettings["Varadero.Email.Clave"];

                var smtp = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["SMTP.Host"],
                    Port = Convert.ToInt32(ConfigurationManager.AppSettings["SMTP.Port"]),
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage()
                {
                    From = fromAddress,
                    IsBodyHtml = true,
                    Subject = subject,
                    Body = body.ToString()
                })
                {
                    foreach (var addr in addresses)
                        message.To.Add(addr);
                    smtp.Send(message);
                }
            }
            catch (SmtpFailedRecipientsException ex)
            {
                LogManager.Instance.LogInfo(null, "EmailManager.Enviar()", "SmtpFailedRecipientsException: " + ex.Message, new { subject, body, addresses }, null, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void EnviarConfirmacionesRecepcionPedido(string htmlPath, List<int> ids)
        {
            try
            {
                using (var db = new DbEcommerceContext())
                {
                    var pedidos = db.Pedidos
                                        .Where(p => ids.Contains(p.PedidoId))
                                        .ToList();
                    foreach (var pedido in pedidos)
                    {
                        var cliente = db.Clientes.First(c => c.PKClienteId.Equals(pedido.ClienteId));
                        EnviarCorreoConfirmacionPedido(htmlPath, cliente, pedido);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void EnviarCorreoConfirmacionPedido(string htmlPath, Cliente cliente, Pedido pedido)
        {
            var html = System.IO.File.ReadAllText(htmlPath);
            var content = html.Replace("{{PEDIDO_NUMERO}}", pedido.Numero.ToString());
            var dest = new List<System.Net.Mail.MailAddress>() { new System.Net.Mail.MailAddress(cliente.UsuarioEmail) };

            EmailManager.Enviar("Droguería Varadero | Confirmación de pedido", content, dest);
        }

        public static void EnviarCorreoPedidoListoParaDespachar(string htmlPath, Cliente cliente, Pedido pedido)
        {
            try
            {
                var html = System.IO.File.ReadAllText(htmlPath);
                var content = html.Replace("{{PEDIDO_NUMERO}}", pedido.Numero.ToString());
                var dest = new List<System.Net.Mail.MailAddress>() { new System.Net.Mail.MailAddress(cliente.UsuarioEmail) };
                EmailManager.Enviar("Droguería Varadero | Pedido preparado", content, dest);
            }
            catch (Exception ex)
            {

            }
        }
    }
}