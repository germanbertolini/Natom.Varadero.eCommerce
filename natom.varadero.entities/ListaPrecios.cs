using System;
using System.ComponentModel.DataAnnotations;

namespace natom.varadero.entities
{
    public class ListaPrecios
    {
        public long EF_Id { get; set; }
        public int ListaDePreciosId { get; set; }
        public string CodigoArticulo { get; set; }
        public decimal PrecioNeto { get; set; }
    }
}
