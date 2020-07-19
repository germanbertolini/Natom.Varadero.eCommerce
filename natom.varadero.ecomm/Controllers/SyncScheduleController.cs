using natom.varadero.ecomm.Managers;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace natom.varadero.ecomm.Controllers
{
    public class SyncScheduleController : BaseController
    {
        [HttpGet]
        public ActionResult Get(string aplicativo, string ejecucionId)
        {
            var response = new EndpointResponse<SyncScheduleInfo>();
            try
            {
                using (var db = new DbEcommerceContext())
                {
                    db.Database.ExecuteSqlCommandAsync("CALL spSyncSesionRegistrarActividad('" + ejecucionId + "', '" + aplicativo + "', '" + Request.UserHostAddress + "')");
                    response.Data = new SyncScheduleInfo();
                    response.Data.CancellationTokenMS = Convert.ToInt64(ConfigurationManager.AppSettings["Varadero.Sync.Cancellation.Token.Period.MS"]);
                    response.Data.Schedules = db.SyncSchedules.ToList();
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = (ex.InnerException ?? ex).Message;
                LogManager.Instance.LogException("/SyncSchedule/Get", ex, Request);
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult LogAlive(string aplicativo, string ejecucionId)
        {
            var response = new EndpointResponse<string>();
            try
            {
                using (var db = new DbEcommerceContext())
                {
                    db.Database.ExecuteSqlCommandAsync("CALL spSyncSesionRegistrarActividad('" + ejecucionId + "', '" + aplicativo + "', '" + Request.UserHostAddress + "')");
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = (ex.InnerException ?? ex).Message;
                LogManager.Instance.LogException("/SyncSchedule/LogAlive", ex, Request);
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}