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
                            .Where(c => c.ClienteCUIT == cliente.CUIT)
                            .GroupBy(k => new { k.Direccion, k.CodigoPostal, k.Telefono },
                                       (k, v) => new {
                                            EF_Id = v.FirstOrDefault().EF_Id,
                                            CodigoPostal = k.CodigoPostal,
                                            Direccion = k.Direccion,
                                            Telefono = k.Telefono
                                        })
                            .ToList();
            foreach (var s in dires)
            {
                var dire = new ClienteDireccion()
                {
                    EF_Id = s.EF_Id,
                    CodigoPostal = s.CodigoPostal,
                    Direccion = s.Direccion,
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

        public void ReflejarPedidoEnSaldoCtaCte(string clienteCodigo, decimal montoTotalPedido)
        {
            Cliente cliente = db.Clientes.First(c => c.Codigo == clienteCodigo);
            db.Entry(cliente).State = System.Data.Entity.EntityState.Modified;
            cliente.SaldoEnCtaCte += montoTotalPedido;
            db.SaveChanges();
        }
    }
}