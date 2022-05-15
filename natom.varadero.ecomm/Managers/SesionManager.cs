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

        public Usuario ValidarLogin(string usuario, string clave, HttpRequestBase request)
        {
            Usuario modelUsuario = null;

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
            List<Usuario> encontrados = null;


            encontrados = db.Usuarios.Where(c => c.Email.ToLower().Equals(usuario)).ToList();


            if (encontrados.Count == 0)
            {
                throw new HandledException("No existe el usuario");
            }

            encontrados = encontrados.Where(e => e.Clave != null && e.Clave.Equals(clave)).ToList();
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
                modelUsuario = encontrados[0];
                this.db.Entry(modelUsuario).State = System.Data.Entity.EntityState.Modified;
                modelUsuario.SesionAgent = request.UserAgent;
                modelUsuario.SesionIP = request.UserHostAddress;
                modelUsuario.SesionInicio = DateTime.Now;
                modelUsuario.SesionUltimaAccion = DateTime.Now;
                modelUsuario.SesionToken = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "").ToLower();
                this.db.SaveChanges();
            }

            return modelUsuario;
        }

        public Usuario ValidarLoginDashboard(string usuario, string clave, HttpRequestBase request)
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

            return new Usuario
            {
                Id = -1,
                SesionAgent = request.UserAgent,
                SesionIP = request.UserHostAddress,
                SesionInicio = DateTime.Now,
                SesionUltimaAccion = DateTime.Now,
                SesionToken = cTokenForDashboard //Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "").ToLower()
            };
        }

        public Usuario ObtenerCliente(string sesionToken)
        {
            return this.db.Usuarios.FirstOrDefault(c => c.SesionToken.Equals(sesionToken));
        }

        public bool SesionTokenDashboardValido(string sesionTokenDashboard)
        {
            return cTokenForDashboard.Equals(sesionTokenDashboard);
        }
    }
}