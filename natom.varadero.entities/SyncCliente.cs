using System;
using System.Collections.Generic;
using System.Text;

namespace natom.varadero.entities
{
    public class SyncCliente
    {
        public List<Cliente> Clientes { get; set; }
        public List<ClienteDireccion> clienteDirecciones { get; set; }
    }
}
