using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

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
    }
}