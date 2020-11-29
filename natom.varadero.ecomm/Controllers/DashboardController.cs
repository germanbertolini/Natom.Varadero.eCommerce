using natom.varadero.ecomm.Exceptions;
using natom.varadero.ecomm.Managers;
using natom.varadero.ecomm.Models.ViewModels;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace natom.varadero.ecomm.Controllers
{
    public class DashboardController : BaseController
    {
        public ActionResult Login(string error = "")
        {
            eCommStatusManager mgr = new eCommStatusManager();
            if (mgr.IsRunnningSyncRoutine())
            {
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");
            }
            ViewBag.ErrorMessage = error;
            return PartialView();
        }

        [HttpPost]
        public ActionResult Login(LoginView data)
        {
            try
            {
                SesionManager mgr = new SesionManager();
                Cliente cliente = mgr.ValidarLoginDashboard(data.Usuario, data.Clave, Request);
                HttpCookie myCookie = new HttpCookie("d");
                myCookie.Value = cliente.SesionToken.ToString();
                Response.Cookies.Add(myCookie);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (HandledException ex)
            {
                return RedirectToAction("Login", "Dashboard", new { @error = ex.Message });
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/Dashboard/Login", new { data }, ex, Request);
                return RedirectToAction("Login", "Dashboard", new { @error = ex.Message });
            }
        }
    }
}