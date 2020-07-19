using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace natom.varadero.entities
{
    public class ArticuloGrupo
    {
        public long EF_Id { get; set; }
        public int PKGrupoId { get; set; }
        public string Descripcion { get; set; }
    }
}
