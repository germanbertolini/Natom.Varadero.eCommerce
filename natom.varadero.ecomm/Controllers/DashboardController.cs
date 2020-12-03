using natom.varadero.ecomm.Exceptions;
using natom.varadero.ecomm.Managers;
using natom.varadero.ecomm.Models.DataTable;
using natom.varadero.ecomm.Models.ViewModels;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Configuration;
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

        [HttpPost]
        public ActionResult MarcarComoPreparado(int pedidoId)
        {
            try
            {
                var manager = new CarritoManager();
                var pedido = manager.ObtenerPedido(pedidoId);
                var cliente = manager.ObtenerCliente(pedido.ClienteId);
                manager.MarcarComoPreparado(pedidoId);

                var htmlPath = System.Web.HttpContext.Current.Server.MapPath("~/Emails/PedidoListoParaDespachar.html");
                EmailManager.EnviarCorreoPedidoListoParaDespachar(htmlPath, cliente, pedido);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult MarcarComoCompletado(int pedidoId)
        {
            try
            {
                var manager = new CarritoManager();
                manager.MarcarComoCompletado(pedidoId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult Index()
        {
            return RedirectToAction("Ordenes");
        }

        public ActionResult Ordenes()
        {
            ClienteManager clienteMgr = new ClienteManager();
            eCommStatusManager mgr = new eCommStatusManager();
            
            if (mgr.IsRunnningSyncRoutine())
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");

            if (!new SesionManager().SesionTokenDashboardValido(this.SesionTokenDashboard))
                return RedirectToAction("Login");

            ViewBag.Cliente = clienteMgr.ObtenerParaDashboard();

            return View();
        }

        public ActionResult Destacados()
        {
            ClienteManager clienteMgr = new ClienteManager();
            eCommStatusManager mgr = new eCommStatusManager();
            
            if (mgr.IsRunnningSyncRoutine())
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");

            if (!new SesionManager().SesionTokenDashboardValido(this.SesionTokenDashboard))
                return RedirectToAction("Login");

            ViewBag.Cliente = clienteMgr.ObtenerParaDashboard();

            return View();
        }

        [HttpGet]
        public ActionResult GetOrdenesTableData(JQueryDataTableParamModel param, int filtro)
        {
            try
            {
                var limite = Convert.ToDateTime(ConfigurationManager.AppSettings["Dashboard.Ordenes.CircuitoAPartirDe"]);

                CarritoManager manager = new CarritoManager();
                int cantidadRegistros = manager.GetPedidosCount();
                IEnumerable<Pedido> queryData = manager.GetPedidos();

                if (filtro != 0)
                {
                    if (filtro == 1) /* PEND.SINCRONIZACIÓN */
                        queryData = queryData.Where(p => p.FechaHoraConfirmacion.Value >= limite && !p.FechaHoraFinSincronizado.HasValue);
                    else if (filtro == 2) /* CONFIRMADO */
                        queryData = queryData.Where(p => p.FechaHoraConfirmacion.Value >= limite && p.FechaHoraFinSincronizado.HasValue && !p.FechaHoraPreparado.HasValue);
                    else if (filtro == 3) /* PREPARADO */
                        queryData = queryData.Where(p => p.FechaHoraConfirmacion.Value >= limite && p.FechaHoraFinSincronizado.HasValue && p.FechaHoraPreparado.HasValue && !p.FechaHoraCompletado.HasValue);
                    else if (filtro == 4) /* COMPLETADO */
                        queryData = queryData.Where(p => p.FechaHoraConfirmacion.Value < limite || p.FechaHoraCompletado.HasValue);
                }
                

                var sSearch = Request.QueryString["sSearch"].ToString();
                var sortColumnIndex = Convert.ToInt32(Request.QueryString["iSortCol_0"].ToString());
                Func<Pedido, string> orderingFunction =
                    (c => sortColumnIndex == 0 ? c.Numero.ToString().PadLeft(8, '0') :
                        sortColumnIndex == 1 ? c.FechaHoraConfirmacion.Value.ToOADate().ToString() :
                        sortColumnIndex == 2 ? c.Total.ToString().PadLeft(8, '0') :
                "");


                if (!string.IsNullOrEmpty(sSearch))
                {
                    sSearch = sSearch.ToLower();
                    long n = 0;
                    if (long.TryParse(sSearch, out n))
                    {
                        queryData = queryData.Where(q => q.Numero.Equals(n));
                    }
                    
                }

                var sortDirection = Request.QueryString["sSortDir_0"].ToString(); // asc or desc
                if (sortDirection == "asc")
                    queryData = queryData.OrderBy(orderingFunction);
                else
                    queryData = queryData.OrderByDescending(orderingFunction);

                List<Pedido> displayedData = queryData
                                                .Skip(param.iDisplayStart)
                                                .Take(param.iDisplayLength)
                                                .ToList();

                var result = from c in displayedData
                             select new object[] {
                                 "# " + c.Numero,
                                 c.FechaHoraConfirmacion.Value.ToString("dd/MM/yyyy"),
                                 "$ " + c.Total.ToString("#,##0.00"),
                                 CarritoManager.ObtenerEstadoPedido(c),
                                 (c.FechaHoraAnulacion ?? c.FechaHoraCompletado ?? c.FechaHoraPreparado ?? c.FechaHoraFinSincronizado ?? c.FechaHoraConfirmacion).Value.ToString("dd/MM/yyyy HH:mm") + " hs",
                                 c.PedidoId,
                                 c.FechaHoraFinSincronizado.HasValue,               /* ESTÁ SINCRONIZADO */
                                 c.FechaHoraPreparado.HasValue,                     /* ESTÁ PREPARADO */
                                 (c.FechaHoraConfirmacion.HasValue && c.FechaHoraConfirmacion.Value < limite) || c.FechaHoraCompletado.HasValue         /* ESTÁ COMPLETADO */
                            };

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = cantidadRegistros,
                    iTotalDisplayRecords = queryData.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult BuscarProductos(string productos)
        {
            try
            {
                var manager = new ListaProductosManager();
                return Json(new
                {
                    success = true,
                    datos = from l in manager.BuscarArticulos(productos)
                            select new
                            {
                                id = l.PKArticuloId,
                                label = l.ArticuloNombre
                            }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public ActionResult GetDestacadosTableData(JQueryDataTableParamModel param)
        {
            try
            {
                ListaProductosManager manager = new ListaProductosManager();
                int cantidadRegistros = manager.GetDestacadosCount();
                IEnumerable<DestacadoResult> queryData = manager.GetDestacados();

                var sSearch = Request.QueryString["sSearch"].ToString();
                var sortColumnIndex = Convert.ToInt32(Request.QueryString["iSortCol_0"].ToString());
                Func<DestacadoResult, string> orderingFunction =
                    (c => sortColumnIndex == 0 ? c.Articulo :
                        sortColumnIndex == 1 ? c.DesdeFecha.ToOADate().ToString() :
                "");


                if (!string.IsNullOrEmpty(sSearch))
                {
                    sSearch = sSearch.ToLower();
                    queryData = queryData.Where(q => q.Articulo.ToLower().Contains(sSearch));
                }

                var sortDirection = Request.QueryString["sSortDir_0"].ToString(); // asc or desc
                if (sortDirection == "asc")
                    queryData = queryData.OrderBy(orderingFunction);
                else
                    queryData = queryData.OrderByDescending(orderingFunction);

                List<DestacadoResult> displayedData = queryData
                                                .Skip(param.iDisplayStart)
                                                .Take(param.iDisplayLength)
                                                .ToList();

                var result = from c in displayedData
                             select new object[] {
                                 c.Articulo,
                                 c.DesdeFecha.ToString("dd/MM/yyyy"),
                                 c.PKArticuloId
                            };

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = cantidadRegistros,
                    iTotalDisplayRecords = queryData.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult QuitarDestacado(int id)
        {
            try
            {
                new ListaProductosManager().QuitarDestacado(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult AgregarDestacado(int id)
        {
            try
            {
                new ListaProductosManager().AgregarDestacado(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult VerPedido(int id)
        {
            ClienteManager clienteMgr = new ClienteManager();
            CarritoManager carritoMgr = new CarritoManager();
            eCommStatusManager mgr = new eCommStatusManager();

            if (mgr.IsRunnningSyncRoutine())
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");

            if (!new SesionManager().SesionTokenDashboardValido(this.SesionTokenDashboard))
                return RedirectToAction("Login");

            var pedido = carritoMgr.ObtenerPedido(id);
            ViewBag.Cliente = clienteMgr.ObtenerParaDashboard();
            ViewBag.Pedido = pedido;
            ViewBag.ClientePedido = carritoMgr.ObtenerCliente(pedido.ClienteId);

            return View();
        }
    }
}