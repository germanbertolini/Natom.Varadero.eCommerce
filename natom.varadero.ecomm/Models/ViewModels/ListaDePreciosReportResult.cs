using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Models.ViewModels
{
    public class ListaDePreciosReportResult
    {
        public string Producto { get; set; }
        public string ConIVA { get; set; }
        public string PVP { get; set; }
        public string PorcDto { get; set; }
        public string Precio { get; set; }
    }
}