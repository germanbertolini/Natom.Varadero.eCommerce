using natom.varadero.ecomm.Models.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace natom.varadero.ecomm.Managers
{
    public class StockManager
    {
        private static string _queryGet = null;
        private static string _queryGetDisp = null;
        private DbEcommerceContext db = new DbEcommerceContext();
        
        public decimal ConsultarStockDisponible(HttpServerUtilityBase server, int articuloId)
        {
            //QUERY
            if (_queryGetDisp == null)
            {
                string filePath = server.MapPath("~/Resources/sql_queries/listaproductos.get.sql");
                _queryGetDisp = System.IO.File.ReadAllText(filePath);
            }
            string query = _queryGetDisp;

            query = query.Replace(@"@@LISTADEPRECIOSID@@", "17");

            //WHERE
            string whereStatement = "";
            whereStatement = String.Format(" AND A.PKArticuloId = {0}", articuloId);

            query = query.Replace("/**[[-WHERE_SENTENCIES-]]**/", whereStatement);

            return db.Database.SqlQuery<ListaProductosResult>(query).FirstOrDefault()?.Stock ?? 0;
        }

        public Task<List<StockReservadoResult>> ConsultarReservadosAsync(HttpServerUtilityBase server, int? articuloId = null)
        {
            //QUERY
            if (_queryGet == null)
            {
                string filePath = server.MapPath("~/Resources/sql_queries/stockreservado.get.sql");
                _queryGet = System.IO.File.ReadAllText(filePath);
            }
            string query = _queryGet;

            //WHERE
            string whereStatement = "";
            if (articuloId.HasValue)
            {
                whereStatement = String.Format(" AND D.ArticuloId = {0}", articuloId.Value);
            }
            query = query.Replace("/**[[-WHERE_SENTENCIES-]]**/", whereStatement);

            return db.Database.SqlQuery<StockReservadoResult>(query).ToListAsync();
        }
    }
}