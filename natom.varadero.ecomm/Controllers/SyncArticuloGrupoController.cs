﻿using natom.varadero.ecomm.Managers;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace natom.varadero.ecomm.Controllers
{
    public class SyncArticuloGrupoController : BaseController
    {
        [HttpPost]
        public ActionResult Post(List<ArticuloGrupo> data)
        {
            var response = new EndpointResponse<string>();
            try
            {
                LogManager.Instance.LogInfo(null, "/SyncArticuloGrupo/Post", "INICIO PERSISTENCIA DE DATOS", new ReceivedDataInfo().BuildInfo<ArticuloGrupo>(data));

                eCommStatusManager.Instance.RegisterStartedSync();
                using (var db = new DbEcommerceContext())
                {
                    db.ArticulosGrupos.RemoveRange(db.ArticulosGrupos);
                    db.ArticulosGrupos.AddRange(data);
                    db.SaveChanges();
                }
                response.Success = true;

                LogManager.Instance.LogInfo(null, "/SyncArticuloGrupo/Post", "FIN PERSISTENCIA DE DATOS");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = (ex.InnerException ?? ex).Message;
                LogManager.Instance.LogException("/SyncArticuloGrupo/Post", ex, Request);
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
            string schedule = "ArticuloGrupoRoutine";
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