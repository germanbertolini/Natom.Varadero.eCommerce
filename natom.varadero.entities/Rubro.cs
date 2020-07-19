using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace natom.varadero.entities
{
    public class Rubro
    {
        public long EF_Id { get; set; }
        public string PKRubroId { get; set; }
        public string Descripcion { get; set; }
        public string TipoArticulo { get; set; }
    }
}
