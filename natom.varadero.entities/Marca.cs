using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace natom.varadero.entities
{
    public class Marca
    {
        public long EF_Id { get; set; }
        public string PKMarcaId { get; set; }
        public string Descripcion { get; set; }
    }
}
