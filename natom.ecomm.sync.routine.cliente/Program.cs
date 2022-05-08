using natom.ecomm.sync.apiendpoints.Services;
using natom.ecomm.sync.kernel;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace natom.ecomm.sync.routine.cliente
{
    class Program
    {
        private static bool _lanzadoManualmente = false;
        private static string _ejecucionId = "";
        private static string _endPointRelativeAddress = "/SyncCliente/Post";
        private static string _endPointRelativeAddressGetScript = "/SyncCliente/GetScriptSQL";
        private static string _endPointRelativeAddressGetEndpoint = "/SyncCliente/GetAPIEndpoint";

        static void Main(string[] args)
        {
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            Console.WriteLine("%%                  ClienteRoutine               %%");
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            Console.WriteLine("                                                   ");

            if (args.Length == 0)
            {
                _ejecucionId = Ejecutador.GenerateEjecucionId();
                _lanzadoManualmente = true;
                Console.WriteLine(">> EJECUCIÓN MANUAL");
                LogManager.LogInfo("routine.cliente", _ejecucionId, "Program.Main", "EJECUCIÓN [" + _ejecucionId + "] MANUAL");
            }
            else
            {
                _ejecucionId = args[0];
            }

            Console.WriteLine("> Registrando comienzo de operacion en el LOG...");
            LogManager.LogInfo("routine.cliente", _ejecucionId, "Program.Main", "INICIO RUTINA ClienteRoutine");

            try
            {
                //Console.WriteLine("> Obteniendo Script SQL del servidor...");
                //LogManager.LogInfo("routine.cliente", _ejecucionId, "Program.Main", "OBTENIENDO SCRIPT SQL DEL SERVIDOR");
                //var taskPost = ServiceAccess.DoPost<string>(_endPointRelativeAddressGetScript, new { });
                //Task.WaitAll(taskPost);
                //if (!taskPost.Result.Success)
                //{
                //    throw new Exception("SE HA PRODUCIDO UN ERROR DEL LADO DEL SERVIDOR: " + taskPost.Result.ErrorMessage);
                //}
                Console.WriteLine("> Obteniendo Varadero API Url del servidor...");
                LogManager.LogInfo("routine.cliente", _ejecucionId, "Program.Main", "OBTENIENDO DEL SERVIDOR LA URL DE 'VARADERO API'");
                var taskPost = ServiceAccess.DoPost<string>(_endPointRelativeAddressGetEndpoint, new { });
                Task.WaitAll(taskPost);
                if (!taskPost.Result.Success)
                {
                    throw new Exception("SE HA PRODUCIDO UN ERROR DEL LADO DEL SERVIDOR: " + taskPost.Result.ErrorMessage);
                }

                //Console.WriteLine("> Ejecutando sentencia SQL para obtener datos...");
                //LogManager.LogInfo("routine.cliente", _ejecucionId, "Program.Main", "EJECUTANDO SENTENCIA SQL PARA OBTENER DATOS...");

                //string query = taskPost.Result.Data;
                //List<Cliente> dataToSync = new List<Cliente>();
                //using (var db = new DbVaraderoContext())
                //{
                //    dataToSync = db.Database.SqlQuery<Cliente>(query).ToList();
                //}
                Console.WriteLine("> LLamando a la API Varadero para obtener datos...");
                LogManager.LogInfo("routine.cliente", _ejecucionId, "Program.Main", "LLAMANDO A 'VARADERO API' PARA OBTENER DATOS...");

                var clientes = EndpointsServices.GetClientes(apiAddress: taskPost.Result.Data).GetAwaiter().GetResult();
                List<Cliente> dataToSync = clientes.Select(dto => new Cliente
                {
                    Codigo = dto.cliente_id,
                    CUIT = dto.CUIT,
                    RazonSocial = dto.razon_social,
                    NombreFantasia = dto.nombre_fantasia,
                    CodigoProvincia = dto.provincia_id,
                    ListaPreciosId = dto.lista_precios_id,
                    RegionId = Convert.ToInt32(dto.region_id),
                    LimiteDeCredito = dto.limite_de_credito,
                    SaldoEnCtaCte = dto.saldo_cta_cte,
                    Activo = dto.Activo.Equals("SI"),
                    Direcciones = dto.Domicilios.Select(dom => new ClienteDireccion
                    {
                        ClienteCUIT = dto.cliente_id,
                        CodigoPostal = dom.codigo_postal,
                        Direccion = dom.direccion,
                        Telefono = dom.telefono
                    }).ToList()
                }).ToList();

                Console.WriteLine("> Sincronizando datos al servidor eCommerce...");
                LogManager.LogInfo("routine.cliente", _ejecucionId, "Program.Main", "SINCRONIZANDO DATOS AL SERVIDOR ECOMMERCE...");

                var task = ServiceAccess.DoPost<string>(_endPointRelativeAddress, dataToSync);
                Task.WaitAll(task);

                if (task.IsCompleted && task.Result.Success)
                {
                    Console.WriteLine("> Registrando fin de operacion exitosa en el LOG...");
                    LogManager.LogInfo("routine.cliente", _ejecucionId, "Program.Main", "FIN RUTINA ClienteRoutine EXITOSA");
                }
                else
                {
                    Console.WriteLine("> Registrando fin de operacion con errores en el LOG...");
                    LogManager.LogInfo("routine.cliente", _ejecucionId, "Program.Main", "FIN RUTINA ClienteRoutine CON ERRORES DEL LADO DEL SERVIDOR", new { taskResult = task.Result });
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException("routine.cliente", _ejecucionId, "Program.Main", ex);
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
