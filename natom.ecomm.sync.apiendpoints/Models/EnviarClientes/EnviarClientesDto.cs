using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace natom.ecomm.sync.apiendpoints.Models.EnviarClientes
{
    public class EnviarClientesDto
    {
        public string cliente_id { get; set; }
        public string CUIT { get; set; }
        public string razon_social { get; set; }
        public string nombre_fantasia { get; set; }
        public decimal limite_de_credito { get; set; }
        public decimal saldo_cta_cte { get; set; }
        public int lista_precios_id { get; set; }
        public string provincia_id { get; set; }
        public string zona_id { get; set; }
        public string region_id { get; set; }
        public string Activo { get; set; }

        public List<DomicilioDto> Domicilios { get; set; }
    }
}
