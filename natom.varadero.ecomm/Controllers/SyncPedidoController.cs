using natom.varadero.ecomm.Managers;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Entity;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Configuration;

namespace natom.varadero.ecomm.Controllers
{
    public class SyncPedidoController : BaseController
    {
        [HttpPost]
        public ActionResult Get()
        {
            var response = new EndpointResponse<List<PedidoSync>>();
            try
            {
                LogManager.Instance.LogInfo(null, "/SyncPedido/Get", "INICIO OBTENCIÓN DE DATOS");
                
                using (var db = new DbEcommerceContext())
                {
                    var pedidos = db.Pedidos
                                        .Include(p => p.Detalle)
                                        .Where(p => !p.FechaHoraAnulacion.HasValue
                                                        && p.FechaHoraConfirmacion.HasValue
                                                        && !p.FechaHoraFinSincronizado.HasValue)
                                        .ToList();

                    response.Data = pedidos.Select(p => new PedidoSync
                    {
                        IDPedidoExterno = p.PedidoId,
                        Cliente = p.ClienteCodigo,
                        Cotizacion = p.Cotizacion ?? 1,
                        MonedaPedido = p.Moneda ?? 1,
                        Detalle = p.Detalle.Select(d => new PedidoDetalleSync
                        {
                            ArticuloCodigo = d.Codigo,
                            CantidadPedido = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitarioConDescuentoNeto
                        }).ToList()
                    }).ToList();

                    string _ids = String.Join(",", pedidos.Select(d => d.PedidoId));
                    db.Database.ExecuteSqlCommandAsync(String.Format("UPDATE pedido SET FechaHoraInicioSincronizado = NOW() WHERE PedidoId IN ({0});", _ids));
                }
                response.Success = true;

                LogManager.Instance.LogInfo(null, "/SyncPedido/Get", "FIN OBTENCIÓN DE DATOS");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = (ex.InnerException ?? ex).Message;
                LogManager.Instance.LogException("/SyncPedido/Get", ex, Request);
            }

            //return Json(response);
            return Content(JsonConvert.SerializeObject(response, Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            }));
        }

        [HttpPost]
        public ActionResult ConfirmSync(List<int> ids)
        {
            var response = new EndpointResponse<string>();
            try
            {
                LogManager.Instance.LogInfo(null, "/SyncPedido/ConfirmSync", "INICIO CONFIRMACIÓN SYNC PEDIDOS", ids);

                if (ids != null && ids.Count > 0)
                {
                    string _ids = String.Join(",", ids);
                    using (var db = new DbEcommerceContext())
                    {
                        db.Database.ExecuteSqlCommandAsync(String.Format("UPDATE pedido SET FechaHoraFinSincronizado = NOW() WHERE PedidoId IN ({0});", _ids));
                    }
                }
                
                response.Success = true;

                LogManager.Instance.LogInfo(null, "/SyncPedido/ConfirmSync", "FIN CONFIRMACIÓN SYNC PEDIDOS");

                var htmlPath = System.Web.HttpContext.Current.Server.MapPath("~/Emails/ConfirmacionRecepcionPedido.html");
                Task.Run(() => EmailManager.EnviarConfirmacionesRecepcionPedido(htmlPath, ids));
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = (ex.InnerException ?? ex).Message;
                LogManager.Instance.LogException("/SyncPedido/ConfirmSync", ex, Request);
            }

            return Json(response);
        }

        [HttpPost]
        public ActionResult GetScriptSQL()
        {
            var response = new EndpointResponse<string>();
            string schedule = "PedidoRoutine";
            string routine = schedule.ToLower().Replace("routine", "");
            string fileName = String.Format("routine.{0}.sql", routine);
            string filePath = Server.MapPath("~/Resources/routines_sql_queries/" + fileName);
            string content = null;
            try
            {
                content = System.IO.File.ReadAllText(filePath);
                response.Data = content;
                response.Success = true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/SyncSchedule/GetScriptSQL", new { schedule = schedule }, ex, Request);
            }
            return Json(response);
        }

        [HttpPost]
        public ActionResult GetAPIEndpoint()
        {
            var response = new EndpointResponse<string>();
            string schedule = "SubRubroRoutine";
            var config = ConfigurationManager.AppSettings["Varadero.API.Endpoint.URL"]?.ToString();
            try
            {
                response.Data = config;
                response.Success = true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/SyncSchedule/GetAPIEndpoint", new { schedule = schedule }, ex, Request);
            }
            return Json(response);
        }
    }
}