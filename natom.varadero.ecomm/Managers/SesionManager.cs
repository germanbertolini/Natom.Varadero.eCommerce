using natom.varadero.ecomm.Exceptions;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Managers
{
    public class SesionManager
    {
        private const string cTokenForDashboard = "98237_jc193jdxu-_2k11@9j2&%#cvko**fgsw3";
        private DbEcommerceContext db = new DbEcommerceContext();

        public Cliente ValidarLogin(string usuario, string clave, HttpRequestBase request)
        {
            Cliente cliente = null;

            if (string.IsNullOrEmpty(usuario))
            {
                throw new HandledException("Debe completar USUARIO");
            }

            if (string.IsNullOrEmpty(clave))
            {
                throw new HandledException("Debe completar CLAVE");
            }

            usuario = usuario.ToLower().Trim();
            bool esEmail = usuario.Contains("@");
            List<Cliente> encontrados = null;

            if (esEmail)
            {
                encontrados = db.Clientes.Where(c => c.UsuarioEmail.ToLower().Equals(usuario)).ToList();
            }
            else
            {
                encontrados = db.Clientes.Where(c => c.UsuarioAlias.ToLower().Equals(usuario)).ToList();
            }

            if (encontrados.Count == 0)
            {
                throw new HandledException("No existe el usuario");
            }

            encontrados = encontrados.Where(e => e.UsuarioClave != null && e.UsuarioClave.Equals(clave)).ToList();
            if (encontrados.Count == 0)
            {
                throw new HandledException("Clave incorrecta");
            }
            else if (encontrados.Count > 1)
            {
                throw new HandledException("Existe mas de un usuario con este Email / Alias. Contáctese con Varadero.");
            }
            else
            {
                cliente = encontrados[0];
                this.db.Entry(cliente).State = System.Data.Entity.EntityState.Modified;
                cliente.SesionAgent = request.UserAgent;
                cliente.SesionIP = request.UserHostAddress;
                cliente.SesionInicio = DateTime.Now;
                cliente.SesionUltimaAccion = DateTime.Now;
                cliente.SesionToken = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "").ToLower();
                this.db.SaveChanges();
            }

            return cliente;
        }

        public Cliente ValidarLoginDashboard(string usuario, string clave, HttpRequestBase request)
        {
            if (string.IsNullOrEmpty(usuario))
                throw new HandledException("Debe completar USUARIO");

            if (string.IsNullOrEmpty(clave))
                throw new HandledException("Debe completar CLAVE");

            usuario = usuario.ToLower().Trim();
            var userConfig = ConfigurationManager.AppSettings["Dashboard.Security.Login.UserName"].ToString().ToLower().Trim();
            var passwdConfig = ConfigurationManager.AppSettings["Dashboard.Security.Login.Password"].ToString();

            if (!usuario.Equals(userConfig))
                throw new HandledException("No existe el usuario");

            if (!clave.Equals(passwdConfig))
                throw new HandledException("Clave incorrecta");

            return new Cliente
            {
                ClienteId = -1,
                RazonSocial = "Varadero",
                NombreFantasia = "Varadero",
                SesionAgent = request.UserAgent,
                SesionIP = request.UserHostAddress,
                SesionInicio = DateTime.Now,
                SesionUltimaAccion = DateTime.Now,
                SesionToken = cTokenForDashboard //Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "").ToLower()
            };
        }

        public Cliente ObtenerCliente(string sesionToken)
        {
            return this.db.Clientes.FirstOrDefault(c => c.SesionToken.Equals(sesionToken));
        }
    }
}