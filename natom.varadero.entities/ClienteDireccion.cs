using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace natom.varadero.entities
{
    public class ClienteDireccion
    {
        public long EF_Id { get; set; }
        public decimal ClienteDireccionId { get; set; }
	    public int PKSucursalId { get; set; }
        public int PKClienteId { get; set; }
        public int PKDireccionId { get; set; }
        public string Direccion { get; set; }
        public long? CodigoPostal { get; set; }
        public string Telefono { get; set; }
    }
}
