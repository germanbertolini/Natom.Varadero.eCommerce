using natom.varadero.ecomm.Managers;
using natom.varadero.ecomm.Models;
using natom.varadero.entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace natom.varadero.ecomm.Controllers
{
    public class ListaProductosController : BaseController
    {
        [HttpGet]
        public ActionResult Get(string prodfilters, bool soloDestacados, int itemsPerPage, int numPage)
        {
            if (string.IsNullOrEmpty(this.SesionToken))
            {
                return Content("SESION_FINALIZADA");
            }

            ViewBag.RowsCount = 0;
            var response = new eCommerceResponse();
            try
            {
                SesionManager sesionMgr = new SesionManager();
                Cliente cliente = sesionMgr.ObtenerCliente(this.SesionToken);
                if (cliente == null)
                {
                    return Content("SESION_FINALIZADA");
                }

                List<string> filters = JsonConvert.DeserializeObject<List<string>>(prodfilters);
                ListaProductosManager mgr = new ListaProductosManager();
                StockManager stockMgr = new StockManager();
                long rowsCount = 0;
                var taskRegistros = mgr.ConsultarAsync(cliente.ListaPreciosId, Server, filters, soloDestacados, itemsPerPage, numPage, out rowsCount);
                var taskStock = stockMgr.ConsultarReservadosAsync(Server);

                Task.WaitAll(taskRegistros, taskStock);

                ViewBag.RowsCount = rowsCount;

                //RESTO EL STOCK RESERVADO
                var registros = taskRegistros.Result;
                foreach (var registro in registros)
                {
                    var reserva = taskStock.Result.FirstOrDefault(r => r.ArticuloId == registro.Id);
                    if (reserva != null)
                    {
                        registro.Stock -= reserva.Cantidad;
                    }
                }

                return PartialView("~/Views/eCommerce/cards/ListaProductos_Registros.cshtml", registros);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogException(null, "/ListaProductos/Get", new { prodfilters, itemsPerPage, numPage }, ex, Request);
                return PartialView("~/Views/Shared/Error.cshtml");
            }
        }
    }
}