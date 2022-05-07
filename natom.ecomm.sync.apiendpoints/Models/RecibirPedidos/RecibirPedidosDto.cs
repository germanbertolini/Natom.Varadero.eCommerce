using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace natom.ecomm.sync.apiendpoints.Models.RecibirPedidos
{
    public class RecibirPedidosDto
    {
        public int IDPedidoExterno { get; set; }
        public string Cliente { get; set; }
        public int MonedaPedido { get; set; }
        public decimal Cotizacion { get; set; }
        public int Descuento { get; set; }

        public List<DetalleDto> Detalle { get; set; }
    }
}
