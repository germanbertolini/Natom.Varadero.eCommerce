using natom.ecomm.sync.apiendpoints.Services;
using natom.ecomm.sync.kernel;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace natom.ecomm.sync.routine.listaprecios
{
    class Program
    {
        private static bool _lanzadoManualmente = false;
        private static string _ejecucionId = "";
        private static string _endPointRelativeAddress = "/SyncListaPrecios/Post";
        private static string _endPointRelativeAddressGetScript = "/SyncListaPrecios/GetScriptSQL";
        private static string _endPointRelativeAddressGetEndpoint = "/SyncListaPrecios/GetAPIEndpoint";

        static void Main(string[] args)
        {
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            Console.WriteLine("%%                ListaPreciosRoutine            %%");
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            Console.WriteLine("                                                   ");

            if (args.Length == 0)
            {
                _ejecucionId = Ejecutador.GenerateEjecucionId();
                _lanzadoManualmente = true;
                Console.WriteLine(">> EJECUCIÓN MANUAL");
                LogManager.LogInfo("routine.listaprecios", _ejecucionId, "Program.Main", "EJECUCIÓN [" + _ejecucionId + "] MANUAL");
            }
            else
            {
                _ejecucionId = args[0];
            }

            Console.WriteLine("> Registrando comienzo de operacion en el LOG...");
            LogManager.LogInfo("routine.listaprecios", _ejecucionId, "Program.Main", "INICIO RUTINA ListaPreciosRoutine");

            try
            {
                //Console.WriteLine("> Obteniendo Script SQL del servidor...");
                //LogManager.LogInfo("routine.listaprecios", _ejecucionId, "Program.Main", "OBTENIENDO SCRIPT SQL DEL SERVIDOR");
                //var taskPost = ServiceAccess.DoPost<string>(_endPointRelativeAddressGetScript, new { });
                //Task.WaitAll(taskPost);
                //if (!taskPost.Result.Success)
                //{
                //    throw new Exception("SE HA PRODUCIDO UN ERROR DEL LADO DEL SERVIDOR: " + taskPost.Result.ErrorMessage);
                //}
                Console.WriteLine("> Obteniendo Varadero API Url del servidor...");
                LogManager.LogInfo("routine.listaprecios", _ejecucionId, "Program.Main", "OBTENIENDO DEL SERVIDOR LA URL DE 'VARADERO API'");
                var taskPost = ServiceAccess.DoPost<string>(_endPointRelativeAddressGetEndpoint, new { });
                Task.WaitAll(taskPost);
                if (!taskPost.Result.Success)
                {
                    throw new Exception("SE HA PRODUCIDO UN ERROR DEL LADO DEL SERVIDOR: " + taskPost.Result.ErrorMessage);
                }

                //Console.WriteLine("> Ejecutando sentencia SQL para obtener datos...");
                //LogManager.LogInfo("routine.listaprecios", _ejecucionId, "Program.Main", "EJECUTANDO SENTENCIA SQL PARA OBTENER DATOS...");

                //string query = taskPost.Result.Data;
                //List<ListaPrecios> dataToSync = new List<ListaPrecios>();
                //using (var db = new DbVaraderoContext())
                //{
                //    dataToSync = db.Database.SqlQuery<ListaPrecios>(query).ToList();
                //}
                Console.WriteLine("> LLamando a la API Varadero para obtener datos...");
                LogManager.LogInfo("routine.listaprecios", _ejecucionId, "Program.Main", "LLAMANDO A 'VARADERO API' PARA OBTENER DATOS...");

                var listas = EndpointsServices.GetListaDePrecios(apiAddress: taskPost.Result.Data).GetAwaiter().GetResult();
                List<ListaPrecios> dataToSync = listas.Select(dto => new ListaPrecios
                {
                    ListaDePreciosId = dto.idLista,
                    CodigoArticulo = dto.articulo_id.Trim(),
                    PrecioNeto = dto.precio_neto
                }).ToList();

                Console.WriteLine("> Sincronizando datos al servidor eCommerce...");
                LogManager.LogInfo("routine.listaprecios", _ejecucionId, "Program.Main", "SINCRONIZANDO DATOS AL SERVIDOR ECOMMERCE...");

                List<List<ListaPrecios>> tandas = new List<List<ListaPrecios>>();
                for (int i = 0; i < dataToSync.Count; i++)
                {
                    if (i == 0 || i % 999 == 0)
                    {
                        tandas.Add(new List<ListaPrecios>());
                    }

                    tandas[tandas.Count - 1].Add(dataToSync[i]);
                }

                Task<natom.ecomm.sync.kernel.EndpointResponse<string>> task = default;
                bool esPrimero = true;
                foreach (var tanda in tandas)
                {
                    task = ServiceAccess.DoPost<string>(_endPointRelativeAddress, new SyncListaPrecio() { ListaPrecios = tanda, EsPrimero = esPrimero });
                    Task.WaitAll(task);

                    esPrimero = false;

                    if (!(task.IsCompleted && task.Result.Success))
                    {
                        break;
                    }
                }

                if (task.IsCompleted && task.Result.Success)
                {
                    Console.WriteLine("> Registrando fin de operacion exitosa en el LOG...");
                    LogManager.LogInfo("routine.listaprecios", _ejecucionId, "Program.Main", "FIN RUTINA ListaPreciosRoutine EXITOSA");
                }
                else
                {
                    Console.WriteLine("> Registrando fin de operacion con errores en el LOG...");
                    LogManager.LogInfo("routine.listaprecios", _ejecucionId, "Program.Main", "FIN RUTINA ListaPreciosRoutine CON ERRORES DEL LADO DEL SERVIDOR", new { taskResult = task.Result });
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException("routine.listaprecios", _ejecucionId, "Program.Main", ex);
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
