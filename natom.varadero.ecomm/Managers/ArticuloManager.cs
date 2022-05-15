using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Managers
{
    public class ArticuloManager
    {
        private DbEcommerceContext db = new DbEcommerceContext();

        public Articulo ObtenerArticuloPorCodigo(string articuloCodigo)
        {
            return this.db.Articulos.FirstOrDefault(x => x.ArticuloCodigo == articuloCodigo);
        }
    }
}