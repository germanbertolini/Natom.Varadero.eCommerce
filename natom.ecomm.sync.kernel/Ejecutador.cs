using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace natom.ecomm.sync.kernel
{
    public class Ejecutador
    {
        public static long CancellationTokenMS;

        public static string GenerateEjecucionId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static void Ejecutar(string aplicativo, string aplicativoEjecucionId, string rutina)
        {
            ///LOGUEAR INICIO DE EJECUTAR DESDE APLICATIVO aplicativo.
            LogManager.LogInfo(aplicativo, aplicativoEjecucionId, "Ejecutador.Ejecutar", "INICIO DE EJECUCIÓN PARA RUTINA " + rutina.ToUpper() + " DESDE APLICATIVO " + aplicativo.ToUpper());

            try
            {
                string ejecucionId = GenerateEjecucionId();
                string exeName = GetExeName(rutina);
                string exeFullPath = Environment.CurrentDirectory + "\\" + exeName;

                if (string.IsNullOrEmpty(exeName) || !File.Exists(exeName))
                {
                    //LOGUEAR QUE NO SE ENCUENTRA EL EXE PARA LA RUTINA XXXXX
                    LogManager.LogInfo(aplicativo, aplicativoEjecucionId, "Ejecutador.Ejecutar", "NO SE ENCONTRÓ EL EXE (EJECUTABLE) PARA LA RUTINA " + rutina.ToUpper(), new { exeName, exeFullPath });
                    goto finnally;
                }

                if (ProgramIsRunning(exeFullPath))
                {
                    //LOGUEAR QUE LA RUTINA XXXXX SE ENCUENTRA EN EJECUCIÓN
                    LogManager.LogInfo(aplicativo, aplicativoEjecucionId, "Ejecutador.Ejecutar", "EL EXE (EJECUTABLE) DE LA RUTINA " + rutina.ToUpper() + " SE ENCUENTRA ACTUALMENTE EN EJECUCIÓN. NO SE PUEDE EJECUTAR DOS VECES EN SIMULTÁNEO.", new { exeName, exeFullPath });
                    goto finnally;
                }

                //LOGUEAR QUE SE LANZA LA EJECUCIÓN DE RUTINA XXXXXXX - ejecucionId
                LogManager.LogInfo(aplicativo, aplicativoEjecucionId, "Ejecutador.Ejecutar", "SE LANZA EJECUCIÓN DE RUTINA - SE EJECUTA EXE PARA LA RUTINA " + rutina.ToUpper(), new { ejecucionId, exeName, exeFullPath });

                var processInfo = Process.Start(exeFullPath, ejecucionId);

                LogManager.LogInfo(aplicativo, aplicativoEjecucionId, "Ejecutador.Ejecutar", "LANZAMIENTO OK - COMIENZO DE MONITOREO DE EJECUCIÓN - RUTINA " + rutina.ToUpper(), new { ejecucionId, exeName, exeFullPath, pid = processInfo.Id });

                //MONITOREO QUE NO SE PASE DEL TIEMPO LIMITE DE EJECUCIÓN
                bool enTiempoValido, enEjecucion;
                do
                {
                    enTiempoValido = (DateTime.Now - processInfo.StartTime).TotalMilliseconds < CancellationTokenMS;
                    enEjecucion = !processInfo.HasExited;
                    Thread.Sleep(1000);
                } while (enTiempoValido && enEjecucion);

                if (!enEjecucion)
                {
                    //LOGUEAR QUE FINALIZÓ EL PROCESO XXXXXXXX - ejecucionId. TIEMPO DE EJECUCIÓN: XXXXX
                    LogManager.LogInfo(aplicativo, aplicativoEjecucionId, "Ejecutador.Ejecutar", "FINALIZÓ LA EJECUCIÓN DEL EXE POR SUS MEDIOS - RUTINA " + rutina.ToUpper() + " /// TIEMPO DE EJECUCIÓN: " + (processInfo.ExitTime - processInfo.StartTime).TotalMilliseconds + "ms", new { ejecucionId, exeName, exeFullPath, pid = processInfo.Id });
                }

                if (enEjecucion && !enTiempoValido)
                {
                    processInfo.Kill();
                    TimeSpan tiempoDeEjecucionTotal = processInfo.ExitTime - processInfo.StartTime;
                    //LOGUEAR QUE SE MATÓ AL PROCESO XXXXXXXX - ejecucionId POR EXCESO DE TIEMPO. TIEMPO DE EJECUCIÓN: XXXXX
                    LogManager.LogInfo(aplicativo, aplicativoEjecucionId, "Ejecutador.Ejecutar", "[!!] SE MATÓ A LA EJECUCIÓN DEL EXE POR EXCESO DE TIEMPO - RUTINA " + rutina.ToUpper() + " /// TIEMPO DE EJECUCIÓN: " + (processInfo.ExitTime - processInfo.StartTime).TotalMilliseconds + "ms" + " /// TIEMPO TOLERANCIA: " + (CancellationTokenMS.ToString()) + "ms", new { ejecucionId, exeName, exeFullPath, pid = processInfo.Id });
                }
            }
            catch (Exception ex)
            {
                ///EJECUTAR DESDE APLICATIVO aplicativo CON ERRORES. ex.Message
                LogManager.LogException(aplicativo, aplicativoEjecucionId, "Ejecutador.Ejecutar", "[!!] SE PRODUJO UN ERROR AL LANZAR LA RUTINA " + rutina.ToUpper() + " /// MENSAJE: " + ex.Message, ex.StackTrace);
            }

            finnally:

            ///LOGUEAR FIN DE EJECUTAR DESDE APLICATIVO aplicativo.
            LogManager.LogInfo(aplicativo, aplicativoEjecucionId, "Ejecutador.Ejecutar", "FIN DE EJECUCIÓN PARA RUTINA " + rutina.ToUpper() + " DESDE APLICATIVO " + aplicativo.ToUpper());

            ///SINCRONIZA TODO EL LOG AL HOSTING
            LogManager.SyncToHosting(aplicativo, aplicativoEjecucionId);
        }

        public static bool ProgramIsRunning(string FullPath)
        {
            string FilePath = Path.GetDirectoryName(FullPath);
            string FileName = Path.GetFileNameWithoutExtension(FullPath).ToLower();
            bool isRunning = false;

            Process[] pList = Process.GetProcessesByName(FileName);

            foreach (Process p in pList)
            {
                if (p.MainModule.FileName.StartsWith(FilePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    isRunning = true;
                    break;
                }
            }

            return isRunning;
        }

        public static int ProgramRunningCount(string FullPath)
        {
            string FilePath = Path.GetDirectoryName(FullPath);
            string FileName = Path.GetFileNameWithoutExtension(FullPath).ToLower();
            int count = 0;

            Process[] pList = Process.GetProcessesByName(FileName);

            foreach (Process p in pList)
            {
                if (p.MainModule.FileName.StartsWith(FilePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        private static string GetExeName(string rutina)
        {
            return "natom.ecomm.sync.routine." + rutina.Replace("Routine", "").ToLower() + ".exe";
        }
    }
}
