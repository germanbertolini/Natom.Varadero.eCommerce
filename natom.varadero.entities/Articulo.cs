using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace natom.varadero.entities
{
    public class Articulo
    {
        public long EF_Id { get; set; }
        public int PKArticuloId { get; set; }
        public string ArticuloCodigo { get; set; }
        public string ArticuloNombre { get; set; }
        public string ArticuloDescripcion { get; set; }
        public string ArticuloDescripcionAbreviada { get; set; }
        public decimal? ArticuloStock { get; set; }
        public bool ArticuloActivo { get; set; }
        public string MarcaId { get; set; }
        public string RubroId { get; set; }
        public string SubRubroId { get; set; }
        public int? GrupoId { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public int? UnidMin { get; set; }
        public int? UnidMay { get; set; }
        public decimal? PrecioVentaPublico { get; set; }

        public int? NroAlicIVA { get; set; }
        public decimal? PorcentajeIVA { get; set; }
        
        public decimal? PrecCompra { get; set; }
        public decimal? UnidCompra { get; set; }
    }
}
