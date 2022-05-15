using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace natom.varadero.entities
{
    public class Articulo
    {
        public long EF_Id { get; set; }
        public string ArticuloCodigo { get; set; }
        public string ArticuloNombre { get; set; }
        public string ArticuloDescripcion { get; set; }
        public string ArticuloDescripcionAbreviada { get; set; }
        public decimal? ArticuloStock { get; set; }
        public bool ArticuloActivo { get; set; }
        public string Marca { get; set; }
        public string Rubro { get; set; }
        public string SubRubro { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public bool TienePVP { get; set; }
        public decimal? PrecioVentaPublico { get; set; }

        public decimal? PorcentajeIVA { get; set; }
    }
}
