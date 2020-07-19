using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace natom.varadero.entities
{
    public class PedidoDetalle
    {
        public int PedidoDetalleId { get; set; }
            
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        public int ArticuloId { get; set; }
        public int NroAlicIVA { get; set; }
        public decimal CostoUnitario { get; set; }
        public int ArticuloUnidMin { get; set; }

        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string ArticuloDescripcionAbreviada { get; set; }
        public string Marca { get; set; }

        public decimal Cantidad { get; set; }
        public bool TienePVP { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public decimal PorcentajeDescuento { get; set; }
        public decimal PrecioUnitarioConDescuento { get; set; }
        public decimal PorcentajeIVA { get; set; }
        public decimal PrecioUnitarioConDescuentoNeto { get; set; }
    }
}