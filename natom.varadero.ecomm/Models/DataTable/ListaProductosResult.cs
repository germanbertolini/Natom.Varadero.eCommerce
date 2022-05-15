using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Models.DataTable
{
    public class ListaProductosResult
    {
        public long ROWNUM { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Stock { get; set; }
        public string Marca { get; set; }

        public decimal PorcentajeDescuento { get; set; }

        public decimal PrecioNeto { get; set; }
        public decimal PrecioConDescuentoNeto { get; set; }

        public decimal PorcentajeIVA { get; set; }

        public decimal PrecioBruto { get; set; }
        public decimal PrecioConDescuentoBruto { get; set; }

        public bool TienePVP { get; set; }
        public decimal PrecioVentaPublico { get; set; }
        public decimal PorcentajeDescRespectoPVP { get; set; }

        public bool EsDestacado { get; set; }

        [NotMapped]
        public bool TieneIVADiscriminado
        {
            get { return this.PorcentajeIVA > 0; }
        }
    }
}