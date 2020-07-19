using natom.ecomm.sync.kernel;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace natom.ecomm.sync.routine.articulo
{
    class Program
    {
        private static bool _lanzadoManualmente = false;
        private static string _ejecucionId = "";
        private static string _endPointRelativeAddress = "/SyncArticulo/Post";
        private static string _endPointRelativeAddressGetScript = "/SyncArticulo/GetScriptSQL";


        static void Main(string[] args)
        {
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            Console.WriteLine("%%                 ArticuloRoutine               %%");
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            Console.WriteLine("                                                   ");

            if (args.Length == 0)
            {
                _ejecucionId = Ejecutador.GenerateEjecucionId();
                _lanzadoManualmente = true;
                Console.WriteLine(">> EJECUCIÓN MANUAL");
                LogManager.LogInfo("routine.articulo", _ejecucionId, "Program.Main", "EJECUCIÓN [" + _ejecucionId + "] MANUAL");
            }
            else
            {
                _ejecucionId = args[0];
            }

            Console.WriteLine("> Registrando comienzo de operacion en el LOG...");
            LogManager.LogInfo("routine.articulo", _ejecucionId, "Program.Main", "INICIO RUTINA ArticuloRoutine");

            try
            {
                Console.WriteLine("> Obteniendo Script SQL del servidor...");
                LogManager.LogInfo("routine.articulo", _ejecucionId, "Program.Main", "OBTENIENDO SCRIPT SQL DEL SERVIDOR");
                var taskPost = ServiceAccess.DoPost<string>(_endPointRelativeAddressGetScript, new { });
                Task.WaitAll(taskPost);
                if (!taskPost.Result.Success)
                {
                    throw new Exception("SE HA PRODUCIDO UN ERROR DEL LADO DEL SERVIDOR: " + taskPost.Result.ErrorMessage);
                }

                Console.WriteLine("> Ejecutando sentencia SQL para obtener datos...");
                LogManager.LogInfo("routine.articulo", _ejecucionId, "Program.Main", "EJECUTANDO SENTENCIA SQL PARA OBTENER DATOS...");

                string query = taskPost.Result.Data;
                List<Articulo> dataToSync = new List<Articulo>();
                using (var db = new DbVaraderoContext())
                {
                    while (true)
                    {
                        int reqfrom = dataToSync.Count;
                        string _query = query + String.Format("\nLIMIT {0}, 1000", reqfrom.ToString());
                        var data = db.Database.SqlQuery<Articulo>(_query).ToList();
                        if (data.Count == 0)
                            break;
                        dataToSync.AddRange(data);
                    }
                }

                Console.WriteLine("> Sincronizando datos al servidor eCommerce...");
                LogManager.LogInfo("routine.articulo", _ejecucionId, "Program.Main", "SINCRONIZANDO DATOS AL SERVIDOR ECOMMERCE...");

                var task = ServiceAccess.DoPost<string>(_endPointRelativeAddress, dataToSync);
                Task.WaitAll(task);

                if (task.IsCompleted && task.Result.Success)
                {
                    Console.WriteLine("> Registrando fin de operacion exitosa en el LOG...");
                    LogManager.LogInfo("routine.articulo", _ejecucionId, "Program.Main", "FIN RUTINA ArticuloRoutine EXITOSA");
                }
                else
                {
                    Console.WriteLine("> Registrando fin de operacion con errores en el LOG...");
                    LogManager.LogInfo("routine.articulo", _ejecucionId, "Program.Main", "FIN RUTINA ArticuloRoutine CON ERRORES DEL LADO DEL SERVIDOR", new { taskResult = task.Result });
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException("routine.articulo", _ejecucionId, "Program.Main", ex);
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
