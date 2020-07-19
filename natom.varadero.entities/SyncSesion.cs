using System;
using System.Collections.Generic;
using System.Text;

namespace natom.varadero.entities
{
    public class SyncSesion
    {
        public string EjecucionId { get; set; }
        public string SesionIP { get; set; }
        public string Aplicativo { get; set; }
        public DateTime? FechaHoraInicio { get; set; }
        public DateTime? FechaHoraUltimaConexion { get; set; }
    }
}
