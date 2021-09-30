using natom.varadero.ecomm.Exceptions;
using natom.varadero.ecomm.Managers;
using natom.varadero.ecomm.Models.DataTable;
using natom.varadero.ecomm.Models.ViewModels;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        [HttpPost]
        public ActionResult AgregarMontoMinimo(RegionMontoMinimo montoMinimo)
        {
            try
            {
                var manager = new RegionMontosMinimosManager();
                manager.Grabar(montoMinimo);

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

        public ActionResult MontosMinimosPorRegion()
        {
            ClienteManager clienteMgr = new ClienteManager();
            eCommStatusManager mgr = new eCommStatusManager();

            if (mgr.IsRunnningSyncRoutine())
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");

            if (!new SesionManager().SesionTokenDashboardValido(this.SesionTokenDashboard))
                return RedirectToAction("Login");

            ViewBag.Cliente = clienteMgr.ObtenerParaDashboard();
            ViewBag.Regiones = clienteMgr.ObtenerRegiones();

            return View();
        }

        public ActionResult Promocionales()
        {
            ClienteManager clienteMgr = new ClienteManager();
            eCommStatusManager mgr = new eCommStatusManager();

            if (mgr.IsRunnningSyncRoutine())
                return PartialView("~/Views/eCommerce/Promocionales.cshtml");

            if (!new SesionManager().SesionTokenDashboardValido(this.SesionTokenDashboard))
                return RedirectToAction("Login");

            ViewBag.Cliente = clienteMgr.ObtenerParaDashboard();
            ViewBag.VistaPreviaCargada = GetPromocionalVistaPreviaPath() != null;
            ViewBag.ContenidoCargado = GetPromocionalContenidoPath() != null;

            return View();
        }

        [HttpGet]
        public async Task<ActionResult> GetPromocionalesVistaPrevia()
        {
            try
            {
                var fileFullPath = GetPromocionalVistaPreviaPath();
                var bytes = System.IO.File.ReadAllBytes(fileFullPath);
                var contentType = MimeMapping.GetMimeMapping(fileFullPath);
                return File(bytes, contentType);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetPromocionalesContenido()
        {
            try
            {
                var fileFullPath = GetPromocionalContenidoPath();
                var bytes = System.IO.File.ReadAllBytes(fileFullPath);
                var contentType = MimeMapping.GetMimeMapping(fileFullPath);
                return File(bytes, contentType);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> GetMontoMinimo(int id)
        {
            try
            {
                var manager = new RegionMontosMinimosManager();
                var montoMinimo = manager.GetMontoMinimo(id);
                return Json(new { success = true, data = montoMinimo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EliminarMontoMinimo(int id)
        {
            try
            {
                var manager = new RegionMontosMinimosManager();
                manager.EliminarMontoMinimo(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult GetRegionMontoMinimoTableData(JQueryDataTableParamModel param, int filtro)
        {
            try
            {
                RegionMontosMinimosManager manager = new RegionMontosMinimosManager();
                int cantidadRegistros = manager.GetMontosMinimosCount();
                IEnumerable<RegionMontoMinimo> queryData = manager.GetMontosMinimos();

                if (filtro != 0)
                {
                    queryData = queryData.Where(r => r.RegionId == filtro);
                }

                var diasSemana = new List<string>() { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sabado" };
                var sSearch = Request.QueryString["sSearch"].ToString();
                var sortColumnIndex = Convert.ToInt32(Request.QueryString["iSortCol_0"].ToString());
                Func<RegionMontoMinimo, string> orderingFunction =
                    (c => sortColumnIndex == 0 ? c.Region.Descripcion.ToString().PadLeft(8, '0') :
                        sortColumnIndex == 1 ? (c.DiaDeLaSemana == 0 ? 7 : c.DiaDeLaSemana).ToString() :
                        sortColumnIndex == 2 ? c.MontoMinimo.ToString().PadLeft(8, '0') :
                "");

                if (!string.IsNullOrEmpty(sSearch))
                {
                    queryData = queryData.Where(r => diasSemana[r.DiaDeLaSemana].ToUpper().Contains(sSearch)
                                                        || r.Region.Descripcion.ToUpper().Contains(sSearch));
                }


                var sortDirection = Request.QueryString["sSortDir_0"].ToString(); // asc or desc
                if (sortDirection == "asc")
                    queryData = queryData.OrderBy(orderingFunction);
                else
                    queryData = queryData.OrderByDescending(orderingFunction);

                List<RegionMontoMinimo> displayedData = queryData
                                                .Skip(param.iDisplayStart)
                                                .Take(param.iDisplayLength)
                                                .ToList();

                var result = from c in displayedData
                             select new object[] {
                                 c.Region.Descripcion,
                                 diasSemana[c.DiaDeLaSemana],
                                 "$ " + c.MontoMinimo.ToString("F").Replace(".", ","),
                                 c.RegionMontoMinimoId
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
                                 "$ " + c.Total.ToString("F").Replace(".", ","),
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

        [HttpPost]
        public async Task<ActionResult> BorrarVistaPrevia()
        {
            try
            {
                var filePath = GetPromocionalVistaPreviaPath();
                System.IO.File.Delete(filePath);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> BorrarContenido()
        {
            try
            {
                var filePath = GetPromocionalContenidoPath();
                System.IO.File.Delete(filePath);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UploadVistaPrevia(reqUploadResource request)
        {
            try
            {
                var content = request.Base64.Split(',');
                byte[] bytes = Convert.FromBase64String(content[1]);
                string contentType = content[0].Split(';')[0].Split(':')[1].Trim();

                var uploadsPath = System.Web.HttpContext.Current.Server.MapPath("~/Uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);
                var di = new DirectoryInfo(uploadsPath);
                foreach (var fi in di.GetFiles("promocional_vista_previa.*"))
                    System.IO.File.Delete(fi.FullName);
                var fileExtension = contentType.Split('/').Last();
                System.IO.File.WriteAllBytes(uploadsPath + "\\promocional_vista_previa." + fileExtension, bytes);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UploadContenido(reqUploadResource request)
        {
            try
            {
                var content = request.Base64.Split(',');
                byte[] bytes = Convert.FromBase64String(content[1]);
                string contentType = content[0].Split(';')[0].Split(':')[1].Trim();

                var uploadsPath = System.Web.HttpContext.Current.Server.MapPath("~/Uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);
                var di = new DirectoryInfo(uploadsPath);
                foreach (var fi in di.GetFiles("promocional.*"))
                    System.IO.File.Delete(fi.FullName);
                var fileExtension = contentType.Split('/').Last();
                System.IO.File.WriteAllBytes(uploadsPath + "\\promocional." + fileExtension, bytes);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private string GetPromocionalVistaPreviaPath()
        {
            var uploadsPath = System.Web.HttpContext.Current.Server.MapPath("~/Uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);
            var di = new DirectoryInfo(uploadsPath);
            return di.GetFiles("promocional_vista_previa.*").FirstOrDefault()?.FullName;
        }

        private string GetPromocionalContenidoPath()
        {
            var uploadsPath = System.Web.HttpContext.Current.Server.MapPath("~/Uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);
            var di = new DirectoryInfo(uploadsPath);
            return di.GetFiles("promocional.*").FirstOrDefault()?.FullName;
        }

    }
}