using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Managers
{
    public class UsuarioManager
    {
        DbEcommerceContext db = new DbEcommerceContext();

        public Usuario ObtenerPorToken(string sesionToken)
        {
            return db.Usuarios.FirstOrDefault(c => c.SesionToken.Equals(sesionToken));
        }

        public Usuario ObtenerUsuarioPorClienteCUIT(string CUIT)
        {
            return this.db.Usuarios.FirstOrDefault(x => x.ClienteCUIT == CUIT);
        }

        public Usuario ObtenerUsuarioPorSecretConfirmationEmail(string secretConfirmationEmail)
        {
            return this.db.Usuarios.FirstOrDefault(x => x.SecretConfirmacionEmail == secretConfirmationEmail);
        }

        public int GetUsuariosCount()
        {
            return this.db.Usuarios.Where(x => x.Email != null && x.FechaHoraBaja == null).Count();
        }

        public List<Usuario> GetUsuarios()
        {
            return this.db.Usuarios.Where(x => x.Email != null && x.FechaHoraBaja == null).ToList();
        }

        public void Grabar(Usuario usuario)
        {
            if (this.db.Usuarios.Any(x => x.Email == usuario.Email))
            {
                throw new Exception("Ya existe un usuario registrado con ese email.");
            }
            usuario.SecretConfirmacionEmail = Guid.NewGuid().ToString("N");//Agregandole "N" crea un Guid de 32 caracteres.
            usuario.FechaHoraRegistracion = DateTime.Now;

            db.Usuarios.Add(usuario);
            db.SaveChanges();
        }

        public void Editar(Usuario usuario)
        {
            var usuarioDB = this.db.Usuarios.FirstOrDefault(x => x.EF_Id == usuario.EF_Id);

            if (usuarioDB.Email != usuario.Email)
            {
                usuarioDB.SecretConfirmacionEmail = Guid.NewGuid().ToString("N");
                usuarioDB.Email = usuario.Email;
            }

            if (usuarioDB.Clave != usuario.Clave || usuario.Clave.Length >= 6)
            {
                usuarioDB.Clave = usuario.Clave;
            }

            usuarioDB.ClienteCUIT = usuario.ClienteCUIT;

            db.Entry(usuarioDB).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void EliminarUsuario(string email)
        {
            var usuario = this.db.Usuarios.FirstOrDefault(x => x.Email == email);
            usuario.FechaHoraBaja = DateTime.Now;

            this.db.Entry(usuario).State = EntityState.Modified;
            this.db.SaveChanges();
        }

        public void ModificarClaveUsuarioPorSecretConfirmationEmail(string clave, string secretConfirmationEmail)
        {
            var usuario = this.db.Usuarios.FirstOrDefault(x => x.SecretConfirmacionEmail == secretConfirmationEmail);

            usuario.Clave = clave;
            usuario.SecretConfirmacionEmail = null;

            db.Entry(usuario).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}