using natom.varadero.entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace natom.varadero.ecomm.Managers
{
    public class LogManager
    {
        private DbEcommerceContext db;
        private System.Timers.Timer recorderTimer;
        private List<Log> stackLogs;
        private static LogManager _instance = null;
        public static LogManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LogManager();
                return _instance;
            }
        }

        public static void LimpiarLogsViejos()
        {
            using (var db = new DbEcommerceContext())
            {
                db.Database.ExecuteSqlCommand("DELETE FROM log WHERE id > 0 AND datediff(now(), fechahora) > 90;");
            }
        }

        public void Log(Log log)
        {
            this.stackLogs.Add(log);
        }

        public void Log(List<Log> logs)
        {
            this.stackLogs.AddRange(logs);
        }

        private LogManager()
        {
            db = new DbEcommerceContext();
            stackLogs = new List<Log>();
            StartUpRecorderTimer();
        }

        private void StartUpRecorderTimer()
        {
            recorderTimer = new System.Timers.Timer();
            recorderTimer.Interval = 5000;

            recorderTimer.Elapsed += OnTimedEvent;
            recorderTimer.AutoReset = true;
            recorderTimer.Enabled = true;
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            db.Logs.AddRange(stackLogs);
            db.SaveChanges();
            stackLogs.Clear();
        }

        public void LogInfo(string ejecucionId, string actionUrl, string message,
                                    object dataContext = null, string agent = null, string ip = null)
        {
            var log = new Log()
            {
                EjecucionId = ejecucionId,
                ActionUrl = actionUrl,
                Application = "natom.varadero.ecomm",
                FechaHora = DateTime.Now,
                DataContext = dataContext == null ? null : JsonConvert.SerializeObject(dataContext),
                LogType = "INFO",
                Message = message,
                SesionAgent = agent,
                SesionIP = ip,
                StackTrace = null
            };
            stackLogs.Add(log);
        }

        public void LogException(string ejecucionId, string actionUrl, Exception ex, HttpRequestBase request)
        {
            LogException(ejecucionId, actionUrl, (ex.InnerException?.InnerException ?? ex.InnerException ?? ex).Message, (ex.InnerException?.InnerException ?? ex.InnerException ?? ex).StackTrace, null, request.UserAgent, request.UserHostAddress);
        }

        public void LogException(string ejecucionId, string actionUrl, object dataContext, Exception ex, HttpRequestBase request)
        {
            LogException(ejecucionId, actionUrl, (ex.InnerException?.InnerException ?? ex.InnerException ?? ex).Message, (ex.InnerException?.InnerException ?? ex.InnerException ?? ex).StackTrace, dataContext, request.UserAgent, request.UserHostAddress);
        }

        public void LogException(string actionUrl, Exception ex, HttpRequestBase request)
        {
            LogException(null, actionUrl, (ex.InnerException?.InnerException ?? ex.InnerException ?? ex).Message, (ex.InnerException?.InnerException ?? ex.InnerException ?? ex).StackTrace, null, request.UserAgent, request.UserHostAddress);
        }
        
        public void LogException(string ejecucionId, string actionUrl, string message, string stackTrace,
                                                        object dataContext = null, string agent = null, string ip = null)
        {
            var log = new Log()
            {
                EjecucionId = ejecucionId,
                ActionUrl = actionUrl,
                Application = "natom.varadero.ecomm",
                FechaHora = DateTime.Now,
                DataContext = dataContext == null ? null : JsonConvert.SerializeObject(dataContext),
                LogType = "EXCEPTION",
                Message = message,
                SesionAgent = agent,
                SesionIP = ip,
                StackTrace = stackTrace
            };
            stackLogs.Add(log);

            //COMENTADO POR GERMAN EL 28/11/2020
            //EnviarLogPorMailAsync(log);
        }

        private void EnviarLogPorMailAsync(Log log)
        {
            Task.Factory.StartNew(() =>
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<h2>Se ha registrado una nueva excepción en el Log:</h2>");
                builder.AppendLine("<table>");

                builder.AppendLine("<tr>");
                builder.AppendLine("<td><b>ActionUrl</b></td>");
                builder.AppendLine(String.Format("<td>{0}</td>", log.ActionUrl));
                builder.AppendLine("</tr>");

                builder.AppendLine("<tr>");
                builder.AppendLine("<td><b>Application</b></td>");
                builder.AppendLine(String.Format("<td>{0}</td>", log.Application));
                builder.AppendLine("</tr>");

                builder.AppendLine("<tr>");
                builder.AppendLine("<td><b>FechaHora</b></td>");
                builder.AppendLine(String.Format("<td>{0} hs</td>", log.FechaHora.ToString("dd/MM/yyyy HH:mm:ss")));
                builder.AppendLine("</tr>");

                builder.AppendLine("<tr>");
                builder.AppendLine("<td><b>DataContext</b></td>");
                builder.AppendLine(String.Format("<td>{0}</td>", log.DataContext));
                builder.AppendLine("</tr>");

                builder.AppendLine("<tr>");
                builder.AppendLine("<td><b>LogType</b></td>");
                builder.AppendLine(String.Format("<td><b>{0}</b></td>", log.LogType));
                builder.AppendLine("</tr>");

                builder.AppendLine("<tr>");
                builder.AppendLine("<td><b>Message</b></td>");
                builder.AppendLine(String.Format("<td>{0}</td>", log.Message));
                builder.AppendLine("</tr>");

                builder.AppendLine("<tr>");
                builder.AppendLine("<td><b>EjecucionId</b></td>");
                builder.AppendLine(String.Format("<td>{0}</td>", log.EjecucionId));
                builder.AppendLine("</tr>");

                builder.AppendLine("<tr>");
                builder.AppendLine("<td><b>SesionAgent</b></td>");
                builder.AppendLine(String.Format("<td>{0}</td>", log.SesionAgent));
                builder.AppendLine("</tr>");

                builder.AppendLine("<tr>");
                builder.AppendLine("<td><b>SesionIP</b></td>");
                builder.AppendLine(String.Format("<td>{0}</td>", log.SesionIP));
                builder.AppendLine("</tr>");

                builder.AppendLine("<tr>");
                builder.AppendLine("<td><b>StackTrace</b></td>");
                builder.AppendLine(String.Format("<td>{0}</td>", log.StackTrace));
                builder.AppendLine("</tr>");

                builder.AppendLine("</table>");

                MailAddress address = new MailAddress(ConfigurationManager.AppSettings["Varadero.Log.Receptor"]);
                EmailManager.Enviar(String.Format("[{0}] Varadero eCommerce", log.LogType.ToUpper()), builder.ToString(), new List<MailAddress>() { address });
            });
        }
    }
}