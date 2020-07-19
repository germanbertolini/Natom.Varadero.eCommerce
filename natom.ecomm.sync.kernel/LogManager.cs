using natom.ecomm.sync.kernel.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace natom.ecomm.sync.kernel
{
    public class LogManager
    {
        private static string logsPath = Environment.CurrentDirectory + "\\Logs\\";



        public static void LogInfo(string aplicativo, string ejecucionId, string method, string message, object dataContext = null)
        {
            var log = new Log()
            {
                ActionUrl = method,
                Application = aplicativo,
                EjecucionId = ejecucionId,
                FechaHora = DateTime.Now,
                DataContext = dataContext == null ? null : JsonConvert.SerializeObject(dataContext),
                LogType = "INFO",
                Message = message,
                SesionAgent = "WindowsNETFrameworkConsole",
                SesionIP = null,
                StackTrace = null
            };
            WriteLogIntoTxt(log);
        }

        public static void DeleteOldLogs(string aplicativo, string ejecucionId)
        {
            DateTime fechaLimite = DateTime.Now.AddDays(-15);
            DirectoryInfo info = new DirectoryInfo(logsPath);
            FileInfo[] files = info.GetFiles()
                                        .Where(f => f.CreationTime.Date < fechaLimite.Date)
                                        .OrderBy(p => p.CreationTime)
                                        .ToArray();
            int cantidad = files.Length;
            foreach (FileInfo file in files)
            {
                File.Delete(file.FullName);
            }

            if (cantidad > 0)
                LogManager.LogInfo(aplicativo, ejecucionId, "LogManager.DeleteOldLogs", "SE HAN ELIMINADO " + cantidad + " ARCHIVOS DE LOG VIEJOS.");
        }

        public static void LogException(string aplicativo, string ejecucionId, string method, Exception ex, object dataContext)
        {
            LogException(aplicativo, ejecucionId, method, (ex.InnerException ?? ex).Message, (ex.InnerException ?? ex).StackTrace, dataContext);
        }

        public static void LogException(string aplicativo, string ejecucionId, string method, Exception ex)
        {
            LogException(aplicativo, ejecucionId, method, (ex.InnerException ?? ex).Message, (ex.InnerException ?? ex).StackTrace, null);
        }

        public static void LogException(string aplicativo, string ejecucionId, string method, string message, string stackTrace, object dataContext = null)
        {
            var log = new Log()
            {
                ActionUrl = method,
                Application = aplicativo,
                EjecucionId = ejecucionId,
                FechaHora = DateTime.Now,
                DataContext = dataContext == null ? null : JsonConvert.SerializeObject(dataContext),
                LogType = "EXCEPTION",
                Message = message,
                SesionAgent = "WindowsNETFrameworkConsole",
                SesionIP = null,
                StackTrace = stackTrace
            };
            WriteLogIntoTxt(log);
        }

        public static void LogAlive(string aplicativo, string ejecucionId)
        {
            ServiceAccess.DoGet<string>("/SyncSchedule/LogAlive?aplicativo=" + aplicativo + "&ejecucionId=" + ejecucionId).Wait();
        }

        public static void SyncToHosting(string aplicativo, string ejecucionId)
        {
            string filePathCola = logsPath + "toSync.txt";

            if (!File.Exists(filePathCola))
            {
                //SI NO SE VA A ENVIAR NADA, ME ASEGURO EL MENSAJE DE "ALIVE"
                LogAlive(aplicativo, ejecucionId);
                return;
            }

            ///RENOMBRO EL ARCHIVO PARA NO INTERFERIR EN LA OPERATORIA
            string filePathEnEnvio = logsPath + String.Format("_toSync-{0}.txt", DateTime.Now.ToString("yyMMddHHmmss"));
            File.Move(filePathCola, filePathEnEnvio);

            //LEO EL ARCHIVO PARA OBTENER TODOS LOS LOGS A ENVIAR
            string allContent = Encoding.Unicode.GetString(File.ReadAllBytes(filePathEnEnvio));
            if (string.IsNullOrEmpty(allContent))
            {
                //SI NO SE VA A ENVIAR NADA, ME ASEGURO EL MENSAJE DE "ALIVE"
                LogAlive(aplicativo, ejecucionId);
                return;
            }

            //QUITO LA PRIMER COMA Y LO METO ENTRE CORCHETES PARA LUEGO DESERIALIZARLO Y METERLO EN UN LIST
            allContent = allContent.Substring(1, allContent.Length - 1);
            allContent = String.Concat("[", allContent, "]");
            List<Log> logs = JsonConvert.DeserializeObject<List<Log>>(allContent);

            //LO MANDO AL HOSTING
            bool enviado = false;
            string errMessage = "";
            try
            {
                var taskPost = ServiceAccess.DoPost<List<Log>>("/SyncLog/Post", logs);
                Task.WaitAll(taskPost);

                if (!taskPost.Result.Success)
                    throw new Exception(taskPost.Result.ErrorMessage);

                enviado = true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                if (ex.InnerException != null) {
                    errMessage += "\nInner exception: " + ex.InnerException.Message;
                }
                if (ex.InnerException != null && ex.InnerException.InnerException != null)
                {
                    errMessage += "\nSub-Inner exception: " + ex.InnerException.InnerException.Message;
                }
            }

            //ELIMINO EL ARCHIVO
            File.Delete(filePathEnEnvio);

            //SI NO LO ENVIÓ, GENERO UN ARCHIVO NUEVO COMO BACKUP Y LO DEJO ALMACENADO EN EL DISCO RÍGIDO
            if (!enviado)
            {
                string newFilePath = logsPath + "_SyncLogFallido-" + DateTime.Now.ToString("yyMMddHHmmss") + ".txt";
                string mensajeNoEnvio = String.Format("[{0}hs] LOG NO ENVIADO POR Exception: {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), errMessage);
                allContent = mensajeNoEnvio + "\n" + allContent;
                WriteText(newFilePath, allContent);
            }
        }

        private static void WriteLogIntoTxt(Log log)
        {
            string fileName = String.Format("{0}-{1}-{2}.txt", DateTime.Now.ToString("yyMMdd"), log.Application.ToUpper(), log.EjecucionId);
            string filePath = logsPath + fileName;
            string filePathCola = logsPath + "toSync.txt";

            string contentLogTxt = String.Format("\n{0} hs >  [{1}] {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), log.LogType, log.Message);
            string contentColaTxt = "," + JsonConvert.SerializeObject(log);

            if (!string.IsNullOrEmpty(log.StackTrace))
                contentLogTxt += " //// StackTrace: " + log.StackTrace;

            if (!string.IsNullOrEmpty(log.DataContext))
                contentLogTxt += " //// DataContext: " + log.DataContext;
            
            WriteText(filePath, contentLogTxt);
            WriteText(filePathCola, contentColaTxt);
        }

        private static void WriteText(string filePath, string text)
        {
            if (!Directory.Exists(logsPath))
                Directory.CreateDirectory(logsPath);

            byte[] encodedText = Encoding.Unicode.GetBytes(text);

            using (var sourceStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                sourceStream.Write(encodedText, 0, encodedText.Length);
            };
        }
    }
}
