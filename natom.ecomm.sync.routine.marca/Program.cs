using natom.ecomm.sync.kernel;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace natom.ecomm.sync.routine.marca
{
    class Program
    {
        private static bool _lanzadoManualmente = false;
        private static string _ejecucionId = "";
        private static string _endPointRelativeAddress = "/SyncMarca/Post";
        private static string _endPointRelativeAddressGetScript = "/SyncMarca/GetScriptSQL";

        static void Main(string[] args)
        {
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            Console.WriteLine("%%                    MarcaRoutine               %%");
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            Console.WriteLine("                                                   ");

            if (args.Length == 0)
            {
                _ejecucionId = Ejecutador.GenerateEjecucionId();
                _lanzadoManualmente = true;
                Console.WriteLine(">> EJECUCIÓN MANUAL");
                LogManager.LogInfo("routine.marca", _ejecucionId, "Program.Main", "EJECUCIÓN [" + _ejecucionId + "] MANUAL");
            }
            else
            {
                _ejecucionId = args[0];
            }

            Console.WriteLine("> Registrando comienzo de operacion en el LOG...");
            LogManager.LogInfo("routine.marca", _ejecucionId, "Program.Main", "INICIO RUTINA MarcaRoutine");

            try
            {
                Console.WriteLine("> Obteniendo Script SQL del servidor...");
                LogManager.LogInfo("routine.marca", _ejecucionId, "Program.Main", "OBTENIENDO SCRIPT SQL DEL SERVIDOR");
                var taskPost = ServiceAccess.DoPost<string>(_endPointRelativeAddressGetScript, new { });
                Task.WaitAll(taskPost);
                if (!taskPost.Result.Success)
                {
                    throw new Exception("SE HA PRODUCIDO UN ERROR DEL LADO DEL SERVIDOR: " + taskPost.Result.ErrorMessage);
                }

                Console.WriteLine("> Ejecutando sentencia SQL para obtener datos...");
                LogManager.LogInfo("routine.marca", _ejecucionId, "Program.Main", "EJECUTANDO SENTENCIA SQL PARA OBTENER DATOS...");

                string query = taskPost.Result.Data;
                List<Marca> dataToSync = new List<Marca>();
                using (var db = new DbVaraderoContext())
                {
                    dataToSync = db.Database.SqlQuery<Marca>(query).ToList();
                }

                Console.WriteLine("> Sincronizando datos al servidor eCommerce...");
                LogManager.LogInfo("routine.marca", _ejecucionId, "Program.Main", "SINCRONIZANDO DATOS AL SERVIDOR ECOMMERCE...");

                var task = ServiceAccess.DoPost<string>(_endPointRelativeAddress, dataToSync);
                Task.WaitAll(task);

                if (task.IsCompleted && task.Result.Success)
                {
                    Console.WriteLine("> Registrando fin de operacion exitosa en el LOG...");
                    LogManager.LogInfo("routine.marca", _ejecucionId, "Program.Main", "FIN RUTINA MarcaRoutine EXITOSA");
                }
                else
                {
                    Console.WriteLine("> Registrando fin de operacion con errores en el LOG...");
                    LogManager.LogInfo("routine.marca", _ejecucionId, "Program.Main", "FIN RUTINA MarcaRoutine CON ERRORES DEL LADO DEL SERVIDOR", new { taskResult = task.Result });
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException("routine.marca", _ejecucionId, "Program.Main", ex);
                Console.WriteLine("> Se finalizo el proceso con errores: " + ex.Message);
            }

            if (_lanzadoManualmente)
            {
                Console.WriteLine("\n>>La ventana se cerrará automáticamente en 5 segundos...");
                Thread.Sleep(5000);
            }
        }
    }
}
