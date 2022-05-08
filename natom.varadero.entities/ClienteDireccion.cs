using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace natom.varadero.entities
{
    public class ClienteDireccion
    {
        public long EF_Id { get; set; }
        public string ClienteCUIT { get; set; }
        public string Direccion { get; set; }
        public long? CodigoPostal { get; set; }
        public string Telefono { get; set; }
    }
}
