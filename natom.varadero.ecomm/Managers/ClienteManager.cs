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

        public Usuario ObtenerPorToken(string sesionToken)
        {
            return db.Usuarios.FirstOrDefault(c => c.SesionToken.Equals(sesionToken));
        }

        public Cliente ObtenerParaDashboard()
        {
            return new Cliente
            {
                RazonSocial = "Varadero",
                NombreFantasia = "Varadero",
            };
        }

        public Cliente ObtenerClientePorCUIT(string CUIT)
        {
            return this.db.Clientes.FirstOrDefault(x => x.CUIT == CUIT && x.Activo == true);
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

        public List<Cliente> ObtenerClientes()
        {
            return this.db.Clientes.ToList();
        }
    }
}