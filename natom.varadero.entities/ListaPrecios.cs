using System;
using System.ComponentModel.DataAnnotations;

namespace natom.varadero.entities
{
    public class ListaPrecios
    {
        public long EF_Id { get; set; }
        public int PKListaDePreciosId { get; set; }
        public int? PKArticuloId { get; set; }
        public decimal? Variacion { get; set; }
    }
}
