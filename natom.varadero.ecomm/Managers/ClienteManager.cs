using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Managers
{
    public class ClienteManager
    {
        private DbEcommerceContext db = new DbEcommerceContext();

        public Cliente ObtenerPorToken(string sesionToken)
        {
            return db.Clientes.FirstOrDefault(c => c.SesionToken.Equals(sesionToken));
        }

        public Cliente ObtenerParaDashboard()
        {
            return new Cliente
            {
                ClienteId = -1,
                RazonSocial = "Varadero",
                NombreFantasia = "Varadero",
                UsuarioAlias = "Admin"
            };
        }

        public List<ClienteDireccion> ObtenerDirecciones(Cliente cliente)
        {
            List<ClienteDireccion> direcciones = new List<ClienteDireccion>();
            var dires = this.db.ClientesDirecciones
                            .Where(c => c.PKClienteId == cliente.PKClienteId && c.PKSucursalId == cliente.PKSucursalId)
                            .GroupBy(k => new { k.Direccion, k.CodigoPostal, k.Telefono },
                                       (k, v) => new {
                                            ClienteDireccionId = v.FirstOrDefault().ClienteDireccionId,
                                            EF_Id = v.FirstOrDefault().EF_Id,
                                            PKClienteId = v.FirstOrDefault().PKClienteId,
                                            PKDireccionId = v.FirstOrDefault().PKDireccionId,
                                            PKSucursalId = v.FirstOrDefault().PKSucursalId,
                                            CodigoPostal = k.CodigoPostal,
                                            Direccion = k.Direccion,
                                            Telefono = k.Telefono
                                        })
                            .ToList();
            foreach (var s in dires)
            {
                var dire = new ClienteDireccion()
                {
                    ClienteDireccionId = s.ClienteDireccionId,
                    CodigoPostal = s.CodigoPostal,
                    Direccion = s.Direccion,
                    EF_Id = s.EF_Id,
                    PKClienteId = s.PKClienteId,
                    PKDireccionId = s.PKDireccionId,
                    PKSucursalId = s.PKSucursalId,
                    Telefono = s.Telefono
                };
                direcciones.Add(dire);
            }
            return direcciones;
        }

        public List<Region> ObtenerRegiones()
        {
            return db.Regiones.Where(r => r.DeletedAt == null).ToList();
        }
    }
}