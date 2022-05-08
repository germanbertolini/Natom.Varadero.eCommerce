using System;
using System.Collections.Generic;
using System.Text;

namespace natom.varadero.entities
{
    public class PedidoSync
    {
        public int IDPedidoExterno { get; set; }
        public string Cliente { get; set; }
        public int MonedaPedido { get; set; }
        public decimal Cotizacion { get; set; }
        public List<PedidoDetalleSync> Detalle { get; set; }
    }

    public class PedidoDetalleSync
    {
        public string ArticuloCodigo { get; set; }
        public decimal CantidadPedido { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
