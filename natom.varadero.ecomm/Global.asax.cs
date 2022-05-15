using natom.varadero.ecomm.Managers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace natom.varadero.ecomm
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer<DbEcommerceContext>(null);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            LogManager.LimpiarLogsViejos();

            BackgroundManager.Init();
            eCommStatusManager.Instance.RegisterStartUp();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(HttpContext.Current.Request.CurrentExecutionFilePathExtension))
            {
                HttpContextBase context = new HttpContextWrapper(HttpContext.Current);
                string actionName = "";
                string controllerName = "";
                RouteData rd = null;
                try
                {
                    rd = RouteTable.Routes.GetRouteData(context);
                    actionName = rd.GetRequiredString("action");
                    controllerName = rd.GetRequiredString("controller");
                }
                catch
                {
                }
                finally
                {
                    if (controllerName.ToLower().Equals("dashboard"))
                    {
                        if (!(actionName.ToLower().Equals("login")))
                        {
                            HttpCookie cookie = Request.Cookies["d"];
                            if (cookie == null)
                            {
                                Response.Redirect("Login");
                                Response.End();
                            }
                        }
                    }
                    else if (!controllerName.ToLower().StartsWith("sync") && !actionName.ToLower().Contains("crear_clave"))
                    {
                        if (rd != null && !string.IsNullOrEmpty(actionName))
                        {
                            if (!(actionName.ToLower().Equals("login"))
                                && !actionName.ToLower().Equals("obtenertotalespedido"))
                            {
                                HttpCookie cookie = Request.Cookies["t"];
                                if (cookie == null)
                                {
                                    Response.Redirect("Login");
                                    Response.End();
                                }
                            }
                            //else
                            //{
                            //    HttpCookie cookie = Request.Cookies["t"];
                            //    if (cookie != null)
                            //    {
                            //        Response.Redirect("/eCommerce/Principal");
                            //        Response.End();
                            //    }
                            //}
                        }
                    }
                }
            }
        }
    }
}