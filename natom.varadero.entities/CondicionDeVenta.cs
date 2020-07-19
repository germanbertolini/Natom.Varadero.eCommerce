using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace natom.varadero.entities
{
    public class CondicionDeVenta
    {
        public long EF_Id { get; set; }
        public int PKCondicionVentaId { get; set; }
        public string Descripcion { get; set; }
        public int? Dias { get; set; }
    }
}
