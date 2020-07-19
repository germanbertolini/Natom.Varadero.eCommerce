using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Models.ViewModels
{
    public class LoginView
    {
        public long EstablecimientoId { get; set; }
        public string Usuario { get; set; }
        public string Clave { get; set; }
    }
}