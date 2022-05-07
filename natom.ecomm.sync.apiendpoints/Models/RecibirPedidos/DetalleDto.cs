using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace natom.ecomm.sync.apiendpoints.Models.RecibirPedidos
{
    public class DetalleDto
    {
        public string ArticuloPedido { get; set; }
        public string CantidadPedido { get; set; }
        public decimal pcio { get; set; }
    }
}
