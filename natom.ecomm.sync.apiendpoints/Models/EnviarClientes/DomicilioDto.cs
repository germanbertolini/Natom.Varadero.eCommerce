using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace natom.ecomm.sync.apiendpoints.Models.EnviarClientes
{
    public class DomicilioDto
    {
        public string direccion { get; set; }
        public int codigo_postal { get; set; }
        public string telefono { get; set; }
    }
}
