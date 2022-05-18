using natom.ecomm.sync.apiendpoints.Models.RecibirPedidos;
using natom.ecomm.sync.apiendpoints.Services;
using natom.ecomm.sync.kernel;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace natom.ecomm.sync.routine.pedido
{
    class Program
    {
        private static bool _lanzadoManualmente = false;
        private static string _ejecucionId = "";
        private static string _endPointRelativeAddress = "/SyncPedido";
        private static string _endPointRelativeAddressGetScript = "/SyncPedido/GetScriptSQL";
        private static string _endPointRelativeAddressGetEndpoint = "/SyncPedido/GetAPIEndpoint";

        static void Main(string[] args)
        {
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            Console.WriteLine("%%                  PedidoRoutine                %%");
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            Console.WriteLine("                                                   ");

            if (args.Length == 0)
            {
                _ejecucionId = Ejecutador.GenerateEjecucionId();
                _lanzadoManualmente = true;
                Console.WriteLine(">> EJECUCIÓN MANUAL");
                LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", "EJECUCIÓN [" + _ejecucionId + "] MANUAL");
            }
            else
            {
                _ejecucionId = args[0];
            }

            Console.WriteLine("> Registrando comienzo de operacion en el LOG...");
            LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", "INICIO RUTINA PedidoRoutine");

            try
            {
                //Console.WriteLine("> Obteniendo Script SQL del servidor...");
                //LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", "OBTENIENDO SCRIPT SQL DEL SERVIDOR");
                //var taskPost = ServiceAccess.DoPost<string>(_endPointRelativeAddressGetScript, new { });
                //Task.WaitAll(taskPost);
                //if (!taskPost.Result.Success)
                //{
                //    throw new Exception("SE HA PRODUCIDO UN ERROR DEL LADO DEL SERVIDOR: " + taskPost.Result.ErrorMessage);
                //}
                Console.WriteLine("> Obteniendo Varadero API Url del servidor...");
                LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", "OBTENIENDO DEL SERVIDOR LA URL DE 'VARADERO API'");
                var taskPost = ServiceAccess.DoPost<string>(_endPointRelativeAddressGetEndpoint, new { });
                Task.WaitAll(taskPost);
                if (!taskPost.Result.Success)
                {
                    throw new Exception("SE HA PRODUCIDO UN ERROR DEL LADO DEL SERVIDOR: " + taskPost.Result.ErrorMessage);
                }

                //OBTIENE PEDIDOS PENDIENTES DE SINCRONIZAR DEL ECOMMERCE
                Console.WriteLine("> Obteniendo pedidos desde eCommerce...");
                LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", "OBTENIENDO PEDIDOS DESDE EL ECOMMERCE...");

                var taskGet = ServiceAccess.DoPost<List<PedidoSync>>(_endPointRelativeAddress + "/Get", null);
                Task.WaitAll(taskGet);

                if (taskGet.IsCompleted && taskGet.Result.Success)
                {
                    Console.WriteLine("> Se obtuvieron {0} pedidos nuevos", taskGet.Result.Data.Count);
                    LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", String.Format("SE OBTUVIERON {0} PEDIDOS NUEVOS", taskGet.Result.Data.Count));
                }
                else
                {
                    Console.WriteLine("> Registrando fin de operacion con errores en el LOG...");
                    LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", "FIN RUTINA PedidoRoutine CON ERRORES DEL LADO DEL SERVIDOR", new { taskResult = taskGet.Result });
                }


                //GRABA DATOS EN LA BASE DE DATOS!
                //Console.WriteLine("> Preparando sentencias SQL para grabar datos...");
                //LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", "PREPARANDO SENTENCIAS SQL PARA GRABAR DATOS...");


                //List<Cliente> dataToSync = new List<Cliente>();
                //string query = taskPost.Result.Data;
                //string[] query_split = query.Replace("/***QUERY_SPLIT***/", "¬").Split('¬');
                //string query_insertPedidoCAB = query_split[0];
                //string query_insertPedidoDET = query_split[1];
                //string query_insertMovStock = query_split[2];
                //string query_updateArticulo = query_split[3];
                //StringBuilder toExecute = new StringBuilder();

                //List<int> syncronized = new List<int>();
                //using (var db = new DbVaraderoContext())
                //{
                //    Console.WriteLine("> Preparando sentencias SQL para grabar pedidos...");
                //    List<string> statements = new List<string>();

                //    foreach (var pedido in taskGet.Result.Data)
                //    {
                //        int nroLinea = 0;

                //        toExecute.AppendLine(String.Format("/*** >> PEDIDO CABECERA << PEDIDO ID {0} *** NUMERO {1} ***/", pedido.PedidoId, pedido.Numero.ToString().PadLeft(8, '0')));
                //        toExecute.AppendLine(GetInsertPedidoCabecera(query_insertPedidoCAB, pedido));

                //        toExecute.AppendLine(String.Format("/*** >> PEDIDO DETALLE << PEDIDO ID {0} *** NUMERO {1} ***/", pedido.PedidoId, pedido.Numero.ToString().PadLeft(8, '0')));
                //        foreach (var item in pedido.Detalle)
                //        {
                //            nroLinea++;
                //            toExecute.AppendLine(GetInsertPedidoDetalle(query_insertPedidoDET, pedido, item, nroLinea));
                //        }

                //        //  11/02/2020: LO COMENTO PORQUE EL ECOMMERCE VIEJO NO MUEVE STOCK!
                //        //toExecute.AppendLine(String.Format("/*** >> MOV STOCK << PEDIDO ID {0} *** NUMERO {1} ***/", pedido.PedidoId, pedido.Numero.ToString().PadLeft(8, '0')));
                //        //foreach (var item in pedido.Detalle)
                //        //{
                //        //    toExecute.AppendLine(GetInsertMovStock(query_insertMovStock, pedido, item));
                //        //}

                //        //toExecute.AppendLine(String.Format("/*** >> UPDATE STOCK ACT << PEDIDO ID {0} *** NUMERO {1} ***/", pedido.PedidoId, pedido.Numero.ToString().PadLeft(8, '0')));
                //        //foreach (var item in pedido.Detalle)
                //        //{
                //        //    toExecute.AppendLine(GetUpdateArticuloStockAct(query_updateArticulo, pedido, item));
                //        //}

                //        syncronized.Add(pedido.PedidoId);
                //    }

                //    Console.WriteLine("> Ejecutando sentencias SQL para grabar pedidos...");
                //    if (!string.IsNullOrEmpty(toExecute.ToString()))
                //        db.Database.ExecuteSqlCommand(toExecute.ToString());
                //    else
                //    {
                //        //NO HAY SCRIPT A EJECUTAR = NO HAY PEDIDOS QUE SINCRONIZAR!
                //        Console.WriteLine("> No hay pedidos para sincronizar...");
                //        LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", "NO HAY PEDIDOS PARA SINCRONIZAR");
                //    }
                //}

                List<int> syncronized = new List<int>();
                if (taskGet.Result.Data.Count == 0)
                {
                    //NO HAY SCRIPT A EJECUTAR = NO HAY PEDIDOS QUE SINCRONIZAR!
                    Console.WriteLine("> No hay pedidos para sincronizar...");
                    LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", "NO HAY PEDIDOS PARA SINCRONIZAR");
                }
                else
                {
                    foreach (var pedido in taskGet.Result.Data)
                    {
                        try
                        {
                            var dto = new RecibirPedidosDto
                            {
                                IDPedidoExterno = pedido.IDPedidoExterno,
                                Cliente = pedido.Cliente,
                                MonedaPedido = pedido.MonedaPedido,
                                Cotizacion = pedido.Cotizacion,
                                Descuento = 0,
                                Detalle = pedido.Detalle.Select(d => new DetalleDto
                                {
                                    ArticuloPedido = d.ArticuloCodigo,
                                    CantidadPedido = Convert.ToInt32(d.CantidadPedido).ToString(),
                                    pcio = d.PrecioUnitario
                                }).ToList()
    };
                            EndpointsServices.PostPedido(apiAddress: taskPost.Result.Data, dto).GetAwaiter().GetResult();
                            syncronized.Add(pedido.IDPedidoExterno);
                        }
                        catch (Exception ex)
                        {
                            if(ex.Message.Contains("ID de pedido externo ya existe"))
                                syncronized.Add(pedido.IDPedidoExterno);
                            Console.WriteLine($"> No se pudo sincronizar el pedido ID {pedido.IDPedidoExterno}: {ex.Message}");
                            LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", $"ERROR AL SINCRONIZAR PEDIDO ID {pedido.IDPedidoExterno}: {ex.Message}");
                        }
                    }
                    
                }


                //CONFIRMA SYNC CON EL ECOMMERCE
                Console.WriteLine("> Confirmando sync con el servidor eCommerce...");
                LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", "CONFIRMANDO SYNC CON EL SERVIDOR ECOMMERCE...");

                var task = ServiceAccess.DoPost<string>(_endPointRelativeAddress + "/ConfirmSync", syncronized);
                Task.WaitAll(task);

                if (task.IsCompleted && task.Result.Success)
                {
                    Console.WriteLine("> Registrando fin de operacion exitosa en el LOG...");
                    LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", "FIN RUTINA PedidoRoutine EXITOSA");
                }
                else
                {
                    Console.WriteLine("> Registrando fin de operacion con errores en el LOG...");
                    LogManager.LogInfo("routine.pedido", _ejecucionId, "Program.Main", "FIN RUTINA PedidoRoutine CON ERRORES DEL LADO DEL SERVIDOR", new { taskResult = task.Result });
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException("routine.pedido", _ejecucionId, "Program.Main", ex);
                Console.WriteLine("> Se finalizo el proceso con errores: " + ex.Message);
            }


            if (_lanzadoManualmente)
            {
                Console.WriteLine("\n>>La ventana se cerrará automáticamente en 5 segundos...");
                Thread.Sleep(5000);
            }
        }

        //private static string GetUpdateArticuloStockAct(string query_updateArticulo, Pedido pedido, PedidoDetalle item)
        //{
        //    string query = query_updateArticulo
        //                    .Replace("/**[[-ARTICULO_ID-]]**/", item.ArticuloId.ToString())
        //                    .Replace("/**[[-CANT_PEDIDO-]]**/", item.Cantidad.ToString("F").Replace(',', '.'));
        //    return query;
        //}

        //private static string GetInsertMovStock(string query_insertMovStock, Pedido pedido, PedidoDetalle item)
        //{
        //    string query = query_insertMovStock
        //                    .Replace("/**[[-ARTICULO_ID-]]**/", item.ArticuloId.ToString())
        //                    .Replace("/**[[-CANTIDAD-]]**/", item.Cantidad.ToString("F").Replace(',', '.'))
        //                    .Replace("/**[[-NUM_PEDIDO-]]**/", "1" + pedido.Numero.ToString().PadLeft(8, '0'));
        //    return query;
        //}

        //private static string GetInsertPedidoDetalle(string query_insertPedidoDET, Pedido pedido, PedidoDetalle item, int nroLinea)
        //{
        //    string query = query_insertPedidoDET
        //                    .Replace("/**[[-PK_CLIENTE_ID-]]**/", pedido.ClienteId.ToString())
        //                    .Replace("/**[[-COMPROBANTE_NUMERO_SIN_GUIONES-]]**/", "1" + pedido.Numero.ToString().PadLeft(8, '0'))
        //                    .Replace("/**[[-NRO_LINEA-]]**/", nroLinea.ToString())
        //                    .Replace("/**[[-ARTICULO_ID-]]**/", item.ArticuloId.ToString())
        //                    .Replace("/**[[-CANTIDAD-]]**/", item.Cantidad.ToString("F").Replace(',', '.'))                            
        //                    .Replace("/**[[-ARTICULO_COSTO_UN-]]**/", item.CostoUnitario.ToString("F").Replace(',', '.'))
        //                    .Replace("/**[[-ARTICULO_UNIDMIN-]]**/", item.ArticuloUnidMin.ToString())
        //                    .Replace("/**[[-ARTICULO_NROALICIVA-]]**/", item.NroAlicIVA.ToString())
        //                    .Replace("/**[[-PRECIO_UNITARIO_BRUTO-]]**/", item.PrecioUnitarioConDescuento.ToString("F").Replace(',', '.'))
        //                    .Replace("/**[[-PRECIO_TOTAL_BRUTO-]]**/", (item.PrecioUnitarioConDescuento * item.Cantidad).ToString("F").Replace(',', '.'))
        //                    .Replace("/**[[-PRECIO_UNITARIO_NETO-]]**/", item.PrecioUnitarioConDescuentoNeto.ToString("F").Replace(',', '.'))
        //                    .Replace("/**[[-PRECIO_TOTAL_NETO-]]**/", (item.PrecioUnitarioConDescuentoNeto * item.Cantidad).ToString("F").Replace(',', '.'))
        //                    .Replace("/**[[-IMPORTE-]]**/", (item.Cantidad * item.PrecioUnitarioConDescuento).ToString("F").Replace(',', '.'));
        //    return query;
        //}

        //private static string GetInsertPedidoCabecera(string query_insertPedidoCAB, Pedido pedido)
        //{
        //    string observaciones = "Sin observaciones";
        //    if (!string.IsNullOrEmpty(pedido.Observaciones))
        //    {
        //        observaciones = pedido.Observaciones;
        //        if (observaciones.Length > 200)
        //        {
        //            observaciones = observaciones.Substring(0, 200);
        //        }
        //    }

        //    string query = query_insertPedidoCAB
        //                    .Replace("/**[[-PK_CLIENTE_ID-]]**/", pedido.ClienteId.ToString())
        //                    .Replace("/**[[-CLIENTE_RESPONSABLE_ID-]]**/", pedido.ResponsableId.ToString())
        //                    .Replace("/**[[-NRO_TRANSP-]]**/", string.IsNullOrEmpty(pedido.EnvioDireccion) ? "1" : "21")
        //                    .Replace("/**[[-COMPROBANTE_NUMERO_SIN_GUIONES-]]**/", "1" + pedido.Numero.ToString().PadLeft(8, '0'))
        //                    .Replace("/**[[-COMPROBANTE_FECHA-]]**/", pedido.FechaHoraConfirmacion.Value.ToString("yyyy-MM-dd"))
        //                    .Replace("/**[[-CLIENTE_COND_VTA_ID-]]**/", pedido.CondVtaId.ToString())
        //                    .Replace("/**[[-LISTA_PRECIO_ID-]]**/", pedido.ListaPreciosId.ToString())
        //                    .Replace("/**[[-MONTO_TOTAL_NETO-]]**/", pedido.Detalle.Sum(d => d.PrecioUnitarioConDescuentoNeto * d.Cantidad).ToString("F").Replace(',', '.'))
        //                    .Replace("/**[[-MONTO_TOTAL_IIBB-]]**/", (pedido.Detalle.Sum(d => d.PrecioUnitarioConDescuentoNeto * d.Cantidad) * (pedido.PorcentajeIIBB / 100)).ToString("F").Replace(',', '.'))
        //                    .Replace("/**[[-MONTO_TOTAL_BRUTO-]]**/", pedido.Detalle.Sum(d => d.PrecioUnitarioConDescuento * d.Cantidad).ToString("F").Replace(',', '.'))
        //                    .Replace("/**[[-MONTO_TOTAL_IVA-]]**/", pedido.Detalle.Sum(d => (d.PrecioUnitarioConDescuento - d.PrecioUnitarioConDescuentoNeto) * d.Cantidad).ToString("F").Replace(',', '.'))
        //                    .Replace("/**[[-MONTO_TOTAL_GRAVADO-]]**/", pedido.Detalle.Sum(d => (d.PorcentajeIVA > 0 ? d.PrecioUnitarioConDescuentoNeto : 0) * d.Cantidad).ToString("F").Replace(',', '.'))
        //                    .Replace("/**[[-MONTO_TOTAL_EXENTO-]]**/", pedido.Detalle.Sum(d => (d.PorcentajeIVA == 0 ? d.PrecioUnitarioConDescuentoNeto : 0) * d.Cantidad).ToString("F").Replace(',', '.'))
        //                    .Replace("/**[[-DIRECCION_ENTREGA-]]**/", pedido.EnvioDireccion + " // CP: " + pedido.EnvioCodigoPostal)
        //                    .Replace("/**[[-OBSERVACIONES_PEDIDO-]]**/", observaciones);
        //    return query;
        //}
    }
}
