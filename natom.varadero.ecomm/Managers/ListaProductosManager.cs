using natom.varadero.ecomm.Models.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace natom.varadero.ecomm.Managers
{
    public class ListaProductosManager
    {
        private static string _queryGet = null;
        private DbEcommerceContext db = new DbEcommerceContext();

        public Task<List<ListaProductosResult>> ConsultarAsync(int listaPreciosId, HttpServerUtilityBase server, List<string> filters, bool soloDestacados, int itemsPerPage, int numPage, out long rowsCount)
        {
            //QUERY
            if (_queryGet == null)
            {
                string filePath = server.MapPath("~/Resources/sql_queries/listaproductos.get.sql");
                _queryGet = System.IO.File.ReadAllText(filePath);
            }
            string query = _queryGet;

            //WHERE
            string whereStatement = "";
            query = query.Replace(@"@@LISTADEPRECIOSID@@", listaPreciosId.ToString());

            if (filters.Count > 0 && !filters.First().Equals("NONE"))
            {
                foreach (var filter in filters)
                {
                    var data = filter.Replace("??:??", "¬").Split('¬');
                    string _field = data[0];
                    string _operator = data[1].ToUpper();
                    string _value = data[2].Trim();

                    if (string.IsNullOrEmpty(_value))
                        continue;

                    switch (_operator)
                    {
                        case "LIKE":
                            string[] subValues = _value.Split(' ');
                            foreach (string sub in subValues)
                            {
                                if (!string.IsNullOrEmpty(sub))
                                    whereStatement += String.Format(" AND {0} LIKE '%{1}%'", _field, sub);
                            }
                            break;
                        case "MORE THAN 0":
                            if (_value.Equals("1"))
                            {
                                whereStatement += String.Format(" AND {0} > 0", _field);
                            }
                            break;
                        default:
                            whereStatement += String.Format(" AND {0} = '{1}'", _field, _value);
                            break;
                    }
                }
            }

            if (soloDestacados)
            {
                whereStatement += " AND D.PKArticuloId IS NOT NULL";
            }

            query = query.Replace("/**[[-WHERE_SENTENCIES-]]**/", whereStatement);


            //GET ROWSCOUNT BEFORE CONTINUE WITH MAIN QUERY
            string queryCount = "SELECT COUNT(*) AS Count FROM (" + query + ") AS R;";
            var resultCount = db.Database.SqlQuery<CountResult>(queryCount).First();
            rowsCount = resultCount.Count;


            //ORDER BY
            query = query.Replace("/**[[-ORDERBY_SENTENCIES-]]**/", "ORDER BY A.ArticuloNombre ASC");


            //LIMIT
            int start = itemsPerPage * (numPage - 1);
            int end = /*start + */itemsPerPage;
            query = query.Replace("/**[[-LIMIT_SENTENCIES-]]**/", String.Format("LIMIT {0}, {1}", start, end));

            return db.Database.SqlQuery<ListaProductosResult>(query).ToListAsync();
        }
    }
}