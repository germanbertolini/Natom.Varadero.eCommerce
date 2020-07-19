using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Models
{
    public class eCommerceResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public object Data { get; set; }
    }
}