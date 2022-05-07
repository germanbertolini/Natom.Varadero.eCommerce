using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace natom.ecomm.sync.apiendpoints.Models.EnviarListaDePrecios
{
    public class EnviarListadePreciosDto
    {
        public int idLista { get; set; }
        public string articulo_id { get; set; }
        public decimal precio_neto { get; set; }
    }
}
