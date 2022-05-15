using natom.varadero.ecomm.Managers;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace natom.varadero.ecomm.Controllers
{
    public class SyncClienteController : BaseController
    {
        [HttpPost]
        public ActionResult Post(SyncCliente data)
        {
            var response = new EndpointResponse<string>();
            try
            {
                LogManager.Instance.LogInfo(null, "/SyncCliente/Post", "INICIO PERSISTENCIA DE DATOS", new ReceivedDataInfo().BuildInfo<Cliente>(data.Clientes));

                eCommStatusManager.Instance.RegisterStartedSync();

                //PREPARAMOS LOS DATOS
                var direcciones = new List<ClienteDireccion>();
                foreach (var cliente in data.Clientes)
                {
                    List<ClienteDireccion> direcciones2 = data.clienteDirecciones.Where(x => x.ClienteCUIT == cliente.CUIT).ToList();
                    foreach (var direccion in direcciones2)
                    {
                        direcciones.Add(new ClienteDireccion
                        {
                            ClienteCUIT = cliente.CUIT,
                            CodigoPostal = direccion.CodigoPostal,
                            Direccion = direccion.Direccion,
                            Telefono = direccion.Telefono
                        });
                    }
                }

                //INSERTAMOS EN LA BASE DE DATOS
                using (var db = new DbEcommerceContext())
                {
                    db.Database.ExecuteSqlCommand("TRUNCATE TABLE clientedireccion;");
                    db.Database.ExecuteSqlCommand("TRUNCATE TABLE cliente;");
                }

                using (var db = new DbEcommerceContext())
                {
                    db.Clientes.AddRange(data.Clientes);
                    db.ClientesDirecciones.AddRange(direcciones);

                    db.SaveChanges();
                }
                response.Success = true;

                LogManager.Instance.LogInfo(null, "/SyncCliente/Post", "FIN PERSISTENCIA DE DATOS");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = (ex.InnerException ?? ex).Message;
                LogManager.Instance.LogException("/SyncCliente/Post", ex, Request);
            }
            finally
            {
                eCommStatusManager.Instance.RegisterFinishedSync();
            }

            return Json(response);
        }

        [HttpPost]
        public ActionResult GetScriptSQL()
        {
            var response = new EndpointResponse<string>();
            string schedule = "ClienteRoutine";
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