using Microsoft.Reporting.WebForms;
using natom.varadero.ecomm.Exceptions;
using natom.varadero.ecomm.Managers;
using natom.varadero.ecomm.Models;
using natom.varadero.ecomm.Models.ViewModels;
using natom.varadero.ecomm.Reporting;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Linq;
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

                Cliente cliente = sesionMgr.ObtenerCliente(this.SesionToken);
                Pedido pedido = carritoMgr.NuevoPedido(cliente);

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
        public ActionResult AgregarItem(int articuloId, decimal cantidad, bool tienePVP, decimal? pu, decimal porcDesc, decimal puConDesc, decimal porcIVA, decimal puConDescNeto)
        {
            try
            {
                StockManager stockMgr = new StockManager();
                CarritoManager carritoMgr = new CarritoManager();

                decimal cantidadDisponible = stockMgr.ConsultarStockDisponible(Server, articuloId);
                if (cantidadDisponible < cantidad)
                {
                    throw new HandledException(String.Format("No alcanza el stock: Hay disponible {0}", Convert.ToInt32(cantidadDisponible)));
                }
                
                PedidoDetalle item = carritoMgr.AgregarItem(this.PedidoId.Value, articuloId, cantidad, tienePVP, pu, porcDesc, puConDesc, porcIVA, puConDescNeto);
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
                LogManager.Instance.LogException(null, "/eCommerce/AgregarItem", new { SesionToken, this.PedidoId, articuloId, cantidad, pu }, ex, Request);
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
                decimal cantidadDisponible = stockMgr.ConsultarStockDisponible(Server, item.ArticuloId);
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
                ClienteManager clienteMgr = new ClienteManager();
                CarritoManager carritoMgr = new CarritoManager();
                Cliente cliente = clienteMgr.ObtenerPorToken(this.SesionToken);
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
            ClienteManager clienteMgr = new ClienteManager();
            CarritoManager carritoMgr = new CarritoManager();

            ViewBag.Cliente = clienteMgr.ObtenerPorToken(this.SesionToken);
            ViewBag.Pedido = carritoMgr.ObtenerPedido(id);
            ViewBag.PedidoItemsCount = 0;
            ViewBag.PedidoId = null;

            return View();
        }

        public ActionResult Principal()
        {
            ClienteManager clienteMgr = new ClienteManager();
            CarritoManager carritoMgr = new CarritoManager();
            eCommStatusManager mgr = new eCommStatusManager();
            if (mgr.IsRunnningSyncRoutine())
            {
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");
            }
            ViewBag.Cliente = clienteMgr.ObtenerPorToken(this.SesionToken);
            ViewBag.PedidoAbierto = carritoMgr.ObtenerPedidoAbierto((Cliente)ViewBag.Cliente);
            ViewBag.PedidoItemsCount = ViewBag.PedidoAbierto != null ? ((Pedido)ViewBag.PedidoAbierto).Detalle.Count : 0;
            ViewBag.MisPedidos = carritoMgr.ObtenerPedidos((Cliente)ViewBag.Cliente);
            ViewBag.PedidoId = ViewBag.PedidoAbierto == null ? (int?)null : ((Pedido)ViewBag.PedidoAbierto).PedidoId;

            return View();
        }

        public ActionResult Catalogo(bool destacados = false)
        {
            ClienteManager clienteMgr = new ClienteManager();
            CarritoManager carritoMgr = new CarritoManager();
            eCommStatusManager mgr = new eCommStatusManager();
            if (mgr.IsRunnningSyncRoutine())
            {
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");
            }
            ViewBag.Cliente = clienteMgr.ObtenerPorToken(this.SesionToken);
            ViewBag.PedidoAbierto = carritoMgr.ObtenerPedidoAbierto((Cliente)ViewBag.Cliente);
            ViewBag.PedidoItemsCount = ViewBag.PedidoAbierto != null ? ((Pedido)ViewBag.PedidoAbierto).Detalle.Count : 0;
            ViewBag.PedidoId = ViewBag.PedidoAbierto == null ? (int?)null : ((Pedido)ViewBag.PedidoAbierto).PedidoId;
            ViewBag.EsCatalogo = true;
            ViewBag.Destacados = destacados;

            return View();
        }

        public ActionResult Gondola()
        {
            ClienteManager clienteMgr = new ClienteManager();
            CarritoManager carritoMgr = new CarritoManager();
            eCommStatusManager mgr = new eCommStatusManager();
            if (mgr.IsRunnningSyncRoutine())
            {
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");
            }
            ViewBag.Cliente = clienteMgr.ObtenerPorToken(this.SesionToken);
            ViewBag.PedidoAbierto = carritoMgr.ObtenerPedidoAbierto((Cliente)ViewBag.Cliente);
            ViewBag.PedidoItemsCount = ViewBag.PedidoAbierto != null ? ((Pedido)ViewBag.PedidoAbierto).Detalle.Count : 0;
            ViewBag.PedidoId = ViewBag.PedidoAbierto == null ? (int?)null : ((Pedido)ViewBag.PedidoAbierto).PedidoId;
            ViewBag.EsCatalogo = false;

            return View();
        }

        [HttpPost]
        public ActionResult ConfirmarPedido(decimal? clienteDireccionId)
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
                cliente = clienteMgr.ObtenerPorToken(this.SesionToken);
                pedido = carritoMgr.ConfirmarPedidoAbierto(cliente, clienteDireccionId);

                QuitarCookieCarrito();

                return Json(new { success = true, pedidoId = pedido.PedidoId });
            }
            catch (HandledException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/eCommerce/ConfirmarPedido", new { clienteDireccionId, clienteId = cliente?.ClienteId, pedidoId = pedido?.PedidoId }, ex, Request);
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult VerPedidoAbierto()
        {
            ClienteManager clienteMgr = new ClienteManager();
            CarritoManager carritoMgr = new CarritoManager();
            eCommStatusManager mgr = new eCommStatusManager();
            if (mgr.IsRunnningSyncRoutine())
            {
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");
            }
            ViewBag.Cliente = clienteMgr.ObtenerPorToken(this.SesionToken);
            ViewBag.PedidoAbierto = carritoMgr.ObtenerPedidoAbierto((Cliente)ViewBag.Cliente);
            ViewBag.PedidoItemsCount = ViewBag.PedidoAbierto != null ? ((Pedido)ViewBag.PedidoAbierto).Detalle.Count : 0;
            ViewBag.PedidoId = ViewBag.PedidoAbierto == null ? (int?)null : ((Pedido)ViewBag.PedidoAbierto).PedidoId;
            ViewBag.Direcciones = clienteMgr.ObtenerDirecciones((Cliente)ViewBag.Cliente);
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
            ClienteManager clienteMgr = new ClienteManager();
            CarritoManager carritoMgr = new CarritoManager();
            eCommStatusManager mgr = new eCommStatusManager();
            if (mgr.IsRunnningSyncRoutine())
            {
                return PartialView("~/Views/eCommerce/Mantenimiento.cshtml");
            }
            ViewBag.Cliente = clienteMgr.ObtenerPorToken(this.SesionToken);
            ViewBag.PedidoAbierto = carritoMgr.ObtenerPedidoAbierto((Cliente)ViewBag.Cliente);

            if (ViewBag.PedidoAbierto != null && ((Pedido)ViewBag.PedidoAbierto).PedidoId == id)
            {
                return Redirect("/eCommerce/VerPedidoAbierto");
            }

            ViewBag.Pedido = carritoMgr.ObtenerPedido(id);
            ViewBag.PedidoItemsCount = ViewBag.PedidoAbierto != null ? ((Pedido)ViewBag.PedidoAbierto).Detalle.Count : 0;
            ViewBag.PedidoId = ViewBag.PedidoAbierto == null ? (int?)null : ((Pedido)ViewBag.PedidoAbierto).PedidoId;

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
            //var data = ObtenerDatosCarnet(id);

            ReportViewer viewer = new ReportViewer();
            viewer.ProcessingMode = ProcessingMode.Local;
            viewer.LocalReport.ReportPath = System.Web.HttpContext.Current.Server.MapPath("~/Reporting/ListaDePreciosReport.rdlc");
            
            //viewer.LocalReport.SetParameters(new ReportParameter("NombreApellido", data.NombreApellido));
            //viewer.LocalReport.SetParameters(new ReportParameter("Establecimiento", data.Establecimiento.Length > 40 ? data.Establecimiento.Substring(0, 40) : data.Establecimiento));
            //viewer.LocalReport.SetParameters(new ReportParameter("DNI", data.DNI));
            //viewer.LocalReport.SetParameters(new ReportParameter("NumeroAfiliado", data.NumeroAfiliado));
            //viewer.LocalReport.SetParameters(new ReportParameter("FechaAfiliacion", data.FechaAfiliacion.Value.ToString("dd/MM/yyyy")));
            //viewer.LocalReport.SetParameters(new ReportParameter("FechaVencimiento", data.FechaVencimiento.Value.ToString("dd/MM/yyyy")));

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
                Cliente cliente = mgr.ValidarLogin(data.Usuario, data.Clave, Request);
                HttpCookie myCookie = new HttpCookie("t");
                myCookie.Value = cliente.SesionToken.ToString();
                Response.Cookies.Add(myCookie);

                CarritoManager carritoMgr = new CarritoManager();
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
                    Response.Redirect("/eCommerce/Catalogo");
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
    }
}