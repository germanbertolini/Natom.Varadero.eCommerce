using Microsoft.Reporting.WebForms;
using natom.varadero.ecomm.Exceptions;
using natom.varadero.ecomm.Managers;
using natom.varadero.ecomm.Models;
using natom.varadero.ecomm.Models.ViewModels;
using natom.varadero.ecomm.Reporting;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace natom.varadero.ecomm.Controllers
{
    public class eCommerceController : BaseController
    {
        [HttpPost]
        public ActionResult NuevaOrden()
        {
            try
            {
                CarritoManager carritoMgr = new CarritoManager();
                SesionManager sesionMgr = new SesionManager();

                Usuario usuario = sesionMgr.ObtenerCliente(this.SesionToken);
                Pedido pedido = carritoMgr.NuevoPedido(usuario);

                HttpCookie myCookie = new HttpCookie("c");
                myCookie.Value = pedido.PedidoId.ToString();
                Response.Cookies.Add(myCookie);
                return Json(new { success = true, pedidoId = pedido.PedidoId });
            }
            catch (HandledException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/eCommerce/NuevaOrden", new { SesionToken }, ex, Request);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult AgregarItem(string articuloCodigo, decimal cantidad, bool tienePVP, decimal? pu, decimal porcDesc, decimal puConDesc, decimal porcIVA, decimal puConDescNeto)
        {
            try
            {
                StockManager stockMgr = new StockManager();
                CarritoManager carritoMgr = new CarritoManager();
                
                decimal cantidadDisponible = stockMgr.ConsultarStockDisponible(Server, articuloCodigo);
                if (cantidadDisponible < cantidad)
                {
                    throw new HandledException(String.Format("No alcanza el stock: Hay disponible {0}", Convert.ToInt32(cantidadDisponible)));
                }
                
                PedidoDetalle item = carritoMgr.AgregarItem(this.PedidoId.Value, articuloCodigo, cantidad, tienePVP, pu, porcDesc, puConDesc, porcIVA, puConDescNeto);
                return Json(new
                {
                    success = true,
                    pedidoItemsCount = carritoMgr.ObtenerTotalItems(this.PedidoId.Value),
                    pedidoDetalleId = item.PedidoDetalleId,
                    articuloId = item.ArticuloId,
                    cantidad = item.Cantidad,
                    precioUnitario = item.PrecioUnitario
                });
            }
            catch (HandledException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/eCommerce/AgregarItem", new { SesionToken, this.PedidoId, articuloCodigo, cantidad, pu }, ex, Request);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult UpdateCantidadItem(int pedidoDetalleId, decimal cantidad)
        {
            try
            {
                StockManager stockMgr = new StockManager();
                CarritoManager carritoMgr = new CarritoManager();
                PedidoDetalle item = carritoMgr.ObtenerPedidoDetalle(pedidoDetalleId);
                decimal cantidadDisponible = 2000;//???stockMgr.ConsultarStockDisponible(Server, item.art);
                if (cantidadDisponible < cantidad)
                {
                    throw new HandledException(String.Format("No alcanza el stock: Hay disponible {0}", Convert.ToInt32(cantidadDisponible)));
                }

                item = carritoMgr.UpdateCantidadItem(pedidoDetalleId, cantidad);
                return Json(new
                {
                    success = true,
                    pedidoItemsCount = carritoMgr.ObtenerTotalItems(this.PedidoId.Value),
                    pedidoDetalleId = item.PedidoDetalleId,
                    articuloId = item.ArticuloId,
                    cantidad = item.Cantidad,
                    precioUnitario = item.PrecioUnitario,
                    totalesPedido = carritoMgr.ObtenerTotalesPedido(this.PedidoId.Value)
                });
            }
            catch (HandledException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/eCommerce/UpdateCantidadItem", new { SesionToken, pedidoDetalleId, cantidad }, ex, Request);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult EliminarItem(int pedidoDetalleId)
        {
            try
            {
                CarritoManager carritoMgr = new CarritoManager();
                PedidoDetalle item = carritoMgr.EliminarItem(pedidoDetalleId);
                return Json(new
                {
                    success = true,
                    pedidoItemsCount = carritoMgr.ObtenerTotalItems(this.PedidoId.Value),
                    pedidoDetalleId = item.PedidoDetalleId,
                    articuloId = item.ArticuloId,
                    totalesPedido = carritoMgr.ObtenerTotalesPedido(this.PedidoId.Value)
                });
            }
            catch (HandledException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/eCommerce/EliminarItem", new { SesionToken, pedidoDetalleId }, ex, Request);
                return Json(new { success = false, error = ex.Message });
            }
        }
        
        [HttpPost]
        public ActionResult ObtenerTotalesPedido(int pedidoId)
        {
            try
            {
                CarritoManager carritoMgr = new CarritoManager();
                return Json(new
                {
                    success = true,
                    totalesPedido = carritoMgr.ObtenerTotalesPedido(pedidoId)
                });
            }
            catch (HandledException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/eCommerce/ObtenerTotalesPedido", new { SesionToken, pedidoId }, ex, Request);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult AnularPedido(int pedidoId)
        {
            try
            {
                Usuario usuario = new UsuarioManager().ObtenerPorToken(this.SesionToken);
                Cliente cliente = new ClienteManager().ObtenerClientePorCUIT(usuario.ClienteCUIT);
                CarritoManager carritoMgr = new CarritoManager();
                Pedido pedido = carritoMgr.AnularPedido(pedidoId, cliente);
                return Json(new { success = true, pedidoId = pedido.PedidoId });
            }
            catch (HandledException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/eCommerce/AnularPedido", new { SesionToken, pedidoId }, ex, Request);
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult PedidoConfirmado(int id)
        {
            Usuario usuario = new UsuarioManager().ObtenerPorToken(this.SesionToken);
            Cliente clienteMgr = new ClienteManager().ObtenerClientePorCUIT(usuario.ClienteCUIT);
            CarritoManager carritoMgr = new CarritoManager();

            ViewBag.Cliente = clienteMgr;
            ViewBag.Pedido = carritoMgr.ObtenerPedido(id);
            ViewBag.PedidoItemsCount = 0;
            ViewBag.PedidoId = null;
            ViewBag.PromocionalesMostrarVistaPrevia = GetPromocionalVistaPreviaPath() != null;
            ViewBag.PromocionalesMostrarContenido = GetPromocionalContenidoPath() != null;
            return View();
        }

        public ActionResult Principal()
        {
            Usuario usuario = new UsuarioManager().ObtenerPorToken(this.SesionToken);
            Cliente cliente = new ClienteManager().ObtenerClientePorCUIT(usuario.ClienteCUIT);
            CarritoManager carritoMgr = new CarritoManager();
            eCommStatusManager mgr = new eCommStatusManager();
            if (mgr.IsRunnningSyncRoutine())
            {
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");
            }
            ViewBag.Cliente = cliente;
            ViewBag.PedidoAbierto = carritoMgr.ObtenerPedidoAbierto((Cliente)ViewBag.Cliente);
            ViewBag.PedidoItemsCount = ViewBag.PedidoAbierto != null ? ((Pedido)ViewBag.PedidoAbierto).Detalle.Count : 0;
            ViewBag.MisPedidos = carritoMgr.ObtenerPedidos((Cliente)ViewBag.Cliente);
            ViewBag.PedidoId = ViewBag.PedidoAbierto == null ? (int?)null : ((Pedido)ViewBag.PedidoAbierto).PedidoId;
            ViewBag.PromocionalesMostrarVistaPrevia = GetPromocionalVistaPreviaPath() != null;
            ViewBag.PromocionalesMostrarContenido = GetPromocionalContenidoPath() != null;

            return View();
        }

        public ActionResult Catalogo(bool destacados = false, bool? filtroDestacados = null)
        {
            UsuarioManager usuarioMgr = new UsuarioManager();
            ClienteManager clienteMgr = new ClienteManager();
            CarritoManager carritoMgr = new CarritoManager();
            eCommStatusManager mgr = new eCommStatusManager();
            if (mgr.IsRunnningSyncRoutine())
            {
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");
            }
            ViewBag.Cliente = clienteMgr.ObtenerClientePorCUIT(usuarioMgr.ObtenerPorToken(this.SesionToken).ClienteCUIT);
            ViewBag.PedidoAbierto = carritoMgr.ObtenerPedidoAbierto((Cliente)ViewBag.Cliente);
            ViewBag.PedidoItemsCount = ViewBag.PedidoAbierto != null ? ((Pedido)ViewBag.PedidoAbierto).Detalle.Count : 0;
            ViewBag.PedidoId = ViewBag.PedidoAbierto == null ? (int?)null : ((Pedido)ViewBag.PedidoAbierto).PedidoId;
            ViewBag.EsCatalogo = true;
            ViewBag.Destacados = destacados;
            ViewBag.FiltroDestacados = filtroDestacados ?? destacados;
            ViewBag.PromocionalesMostrarVistaPrevia = GetPromocionalVistaPreviaPath() != null;
            ViewBag.PromocionalesMostrarContenido = GetPromocionalContenidoPath() != null;

            return View();
        }

        public ActionResult Gondola()
        {
            Usuario usuario = new UsuarioManager().ObtenerPorToken(this.SesionToken);
            Cliente cliente = new ClienteManager().ObtenerClientePorCUIT(usuario.ClienteCUIT);
            CarritoManager carritoMgr = new CarritoManager();
            eCommStatusManager mgr = new eCommStatusManager();
            if (mgr.IsRunnningSyncRoutine())
            {
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");
            }
            ViewBag.Cliente = cliente;
            ViewBag.PedidoAbierto = carritoMgr.ObtenerPedidoAbierto((Cliente)ViewBag.Cliente);
            ViewBag.PedidoItemsCount = ViewBag.PedidoAbierto != null ? ((Pedido)ViewBag.PedidoAbierto).Detalle.Count : 0;
            ViewBag.PedidoId = ViewBag.PedidoAbierto == null ? (int?)null : ((Pedido)ViewBag.PedidoAbierto).PedidoId;
            ViewBag.EsCatalogo = false;
            ViewBag.PromocionalesMostrarVistaPrevia = GetPromocionalVistaPreviaPath() != null;
            ViewBag.PromocionalesMostrarContenido = GetPromocionalContenidoPath() != null;

            return View();
        }

        [HttpPost]
        public ActionResult ConfirmarPedido(long? clienteDireccionId)
        {
            Cliente cliente = null;
            Pedido pedido = null;
            try
            {
                ClienteManager clienteMgr = new ClienteManager();
                CarritoManager carritoMgr = new CarritoManager();
                eCommStatusManager mgr = new eCommStatusManager();
                if (mgr.IsRunnningSyncRoutine())
                {
                    return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");
                }
                Usuario usuario = new UsuarioManager().ObtenerPorToken(this.SesionToken);
                cliente = new ClienteManager().ObtenerClientePorCUIT(usuario.ClienteCUIT);
                pedido = carritoMgr.ConfirmarPedidoAbierto(cliente, clienteDireccionId);

                QuitarCookieCarrito();

                clienteMgr.ReflejarPedidoEnSaldoCtaCte(cliente.Codigo, pedido.Total);

                return Json(new { success = true, pedidoId = pedido.PedidoId });
            }
            catch (HandledException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/eCommerce/ConfirmarPedido", new { clienteDireccionId, clienteCodigo = cliente?.Codigo, pedidoId = pedido?.PedidoId }, ex, Request);
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult VerPedidoAbierto()
        {
            Usuario usuario = new UsuarioManager().ObtenerPorToken(this.SesionToken);
            Cliente cliente = new ClienteManager().ObtenerClientePorCUIT(usuario.ClienteCUIT);
            ClienteManager clienteMgr = new ClienteManager();
            CarritoManager carritoMgr = new CarritoManager();
            eCommStatusManager mgr = new eCommStatusManager();
            if (mgr.IsRunnningSyncRoutine())
            {
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");
            }
            ViewBag.Cliente = cliente;
            ViewBag.PedidoAbierto = carritoMgr.ObtenerPedidoAbierto((Cliente)ViewBag.Cliente);
            ViewBag.PedidoItemsCount = ViewBag.PedidoAbierto != null ? ((Pedido)ViewBag.PedidoAbierto).Detalle.Count : 0;
            ViewBag.PedidoId = ViewBag.PedidoAbierto == null ? (int?)null : ((Pedido)ViewBag.PedidoAbierto).PedidoId;
            ViewBag.Direcciones = clienteMgr.ObtenerDirecciones((Cliente)ViewBag.Cliente);
            ViewBag.PromocionalesMostrarVistaPrevia = GetPromocionalVistaPreviaPath() != null;
            ViewBag.PromocionalesMostrarContenido = GetPromocionalContenidoPath() != null;

            return View();
        }

        [HttpPost]
        public ActionResult UpdateObservaciones(int pedidoId, string observaciones)
        {
            try
            {
                new CarritoManager().UpdateObservaciones(pedidoId, observaciones);
                return Json(new { success = true });
            }
            catch (HandledException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/eCommerce/UpdateObservaciones", new { pedidoId, observaciones }, ex, Request);
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult VerPedido(int id)
        {
            Usuario usuario = new UsuarioManager().ObtenerPorToken(this.SesionToken);
            Cliente cliente = new ClienteManager().ObtenerClientePorCUIT(usuario.ClienteCUIT);
            ClienteManager clienteMgr = new ClienteManager();
            CarritoManager carritoMgr = new CarritoManager();
            eCommStatusManager mgr = new eCommStatusManager();
            if (mgr.IsRunnningSyncRoutine())
            {
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");
            }
            ViewBag.Cliente = cliente;
            ViewBag.PedidoAbierto = carritoMgr.ObtenerPedidoAbierto((Cliente)ViewBag.Cliente);

            if (ViewBag.PedidoAbierto != null && ((Pedido)ViewBag.PedidoAbierto).PedidoId == id)
            {
                return Redirect("/eCommerce/VerPedidoAbierto");
            }

            ViewBag.Pedido = carritoMgr.ObtenerPedido(id);
            ViewBag.PedidoItemsCount = ViewBag.PedidoAbierto != null ? ((Pedido)ViewBag.PedidoAbierto).Detalle.Count : 0;
            ViewBag.PedidoId = ViewBag.PedidoAbierto == null ? (int?)null : ((Pedido)ViewBag.PedidoAbierto).PedidoId;
            ViewBag.PromocionalesMostrarVistaPrevia = GetPromocionalVistaPreviaPath() != null;
            ViewBag.PromocionalesMostrarContenido = GetPromocionalContenidoPath() != null;

            return View();
        }

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

        [HttpGet]
        public ActionResult DownloadListaPrecios()
        {
            var listaProductosManager = new ListaProductosManager();
            var clienteMgr = new ClienteManager();
            if (new eCommStatusManager().IsRunnningSyncRoutine())
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");

            Usuario usuario = new UsuarioManager().ObtenerPorToken(this.SesionToken);
            Cliente cliente = new ClienteManager().ObtenerClientePorCUIT(usuario.ClienteCUIT); 

            /** OBTIENE PRECIOS **/
            long rowsCount = 0;
            var precios = listaProductosManager.ConsultarAsync(cliente.ListaPreciosId, Server, new List<string>() { "NONE" }, false, int.MaxValue, 1, out rowsCount).GetAwaiter().GetResult();
            var data = precios.Select(producto => new ListaDePreciosReportResult
            {
                Producto = producto.Nombre,
                ConIVA = producto.TieneIVADiscriminado ? "SI" : "NO",
                PVP = producto.TienePVP && !producto.TieneIVADiscriminado ? (producto.EsDestacado ? ("¡$ " + producto.PrecioVentaPublico.ToString("F").Replace(".", ",") + "!") : "$ " + producto.PrecioVentaPublico.ToString("F").Replace(".", ",")) : "",
                PorcDto = producto.TienePVP && !producto.TieneIVADiscriminado ? ("% " + producto.PorcentajeDescRespectoPVP.ToString("F").Replace(".", ",")) : "",
                Precio = producto.EsDestacado ? ("¡$ " + producto.PrecioConDescuentoBruto.ToString("F").Replace(".", ",") + "!") : "$ " + producto.PrecioConDescuentoBruto.ToString("F").Replace(".", ",")
            }).OrderBy(p => p.Producto).ToList();
            /****************/

            ReportViewer viewer = new ReportViewer();
            viewer.ProcessingMode = ProcessingMode.Local;
            viewer.LocalReport.ReportPath = System.Web.HttpContext.Current.Server.MapPath("~/Reporting/ListaDePreciosReport.rdlc");
            
            viewer.LocalReport.SetParameters(new ReportParameter("pCliente", cliente.RazonSocial));
            viewer.LocalReport.SetParameters(new ReportParameter("pDate", DateTime.Now.ToString("dd/MM/yyyy")));
            viewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", data));

            byte[] b = ReportHelper.ExportToPDF(viewer);

            viewer.Dispose();

            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AppendHeader("Content-Disposition", "attachment; filename=Lista de precios al " + DateTime.Now.ToString("dd-MM-yyyy") + ".pdf");
            Response.BinaryWrite(b);
            Response.End();
            return null;
            //return File(b, "application/pdf");
        }

        [HttpPost]
        public ActionResult Login(LoginView data)
        {
            try
            {
                SesionManager mgr = new SesionManager();
                Usuario usuario = mgr.ValidarLogin(data.Usuario, data.Clave, Request);
                HttpCookie myCookie = new HttpCookie("t");
                myCookie.Value = usuario.SesionToken.ToString();
                Response.Cookies.Add(myCookie);

                CarritoManager carritoMgr = new CarritoManager();
                Cliente cliente = new ClienteManager().ObtenerClientePorCUIT(usuario.ClienteCUIT);
                Pedido pedidoAbierto = carritoMgr.ObtenerPedidoAbierto(cliente);
                if (pedidoAbierto != null)
                {
                    HttpCookie myCookieCarrito = new HttpCookie("c");
                    myCookieCarrito.Value = pedidoAbierto.PedidoId.ToString();
                    Response.Cookies.Add(myCookieCarrito);

                    //REDIRECCIONA A CONTINUAR CON ORDEN
                    Response.Redirect("/eCommerce/Gondola");
                    Response.End();
                }
                else
                {
                    //Response.Redirect("/eCommerce/Principal");
                    Response.Redirect("/eCommerce/Catalogo?destacados=true&filtroDestacados=false");
                    Response.End();
                }
                
                return null;
            }
            catch (HandledException ex)
            {
                return RedirectToAction("Login", "eCommerce", new { @error = ex.Message });
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/eCommerce/Login", new { data }, ex, Request);
                return RedirectToAction("Login", "eCommerce", new { @error = ex.Message });
            }
        }

        public ActionResult Crear_clave(string secretConfirmacionEmail)
        {
            Usuario usuario = new UsuarioManager().ObtenerUsuarioPorSecretConfirmationEmail(secretConfirmacionEmail);
            if (usuario == null)
                usuario = new UsuarioManager().ObtenerPorToken(this.SesionToken);
            Cliente cliente = new ClienteManager().ObtenerClientePorCUIT(usuario.ClienteCUIT);
            ViewBag.Cliente = cliente;
            ViewBag.Usuario = usuario;
            return View();
        }

        [HttpPost]
        public ActionResult Crear_clave(string secretConfirmacionEmail, string contrasena)
        {
            new UsuarioManager().ModificarClaveUsuarioPorSecretConfirmationEmail(contrasena, secretConfirmacionEmail);

            return RedirectToAction("Login", "eCommerce");
        }

        private void QuitarCookieCarrito()
        {
            HttpCookie cookieCarrito = Request.Cookies["c"];
            if (cookieCarrito != null)
                Response.Cookies["c"].Expires = DateTime.Now.AddDays(-1);
        }

        private void QuitarCookieSesion()
        {
            HttpCookie cookie = Request.Cookies["t"];
            if (cookie != null)
                Response.Cookies["t"].Expires = DateTime.Now.AddDays(-1); //Request.Cookies.Remove("t");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                QuitarCookieSesion();
                QuitarCookieCarrito();

                Response.Redirect("Login");
                Response.End();
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/eCommerce/Logout", new { SesionToken = SesionToken }, ex, Request);
            }
            return RedirectToAction("Login", "eCommerce", new { @error = "" });
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