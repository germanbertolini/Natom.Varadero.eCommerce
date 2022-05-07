using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace natom.ecomm.sync.apiendpoints.Models.EnviarArticulos
{
    public class EnviarArticulosDto
    {
        public string codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Descripcion_abreviada { get; set; }
        public decimal Stock { get; set; }
        public string Activo { get; set; }
        public string Marca { get; set; }
        public string Rubro { get; set; }
        public string SubRubro { get; set; }
        public bool tiene_pvp { get; set; }
        public decimal precio_venta_publico { get; set; }
        public decimal porcentaje_iva { get; set; }
    }
}
