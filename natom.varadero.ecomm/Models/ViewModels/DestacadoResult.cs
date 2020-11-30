using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Models.ViewModels
{
    public class DestacadoResult
    {
        public int PKArticuloId { get; set; }
        public string Articulo { get; set; }
        public DateTime DesdeFecha { get; set; }
    }
}