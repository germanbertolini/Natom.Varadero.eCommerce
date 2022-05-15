using System;
using System.Collections.Generic;
using System.Text;

namespace natom.varadero.entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public long EF_Id { get; set; }
        public string ClienteCUIT { get; set; }
        public string Email { get; set; }
        public string Clave { get; set; }
        public DateTime FechaHoraRegistracion { get; set; }
        public DateTime FechaHoraConfirmacionEmail { get; set; }
        public string SecretConfirmacionEmail { get; set; }
        public DateTime? FechaHoraBaja { get; set; }
        public string SesionIP { get; set; }
        public string SesionAgent { get; set; }
        public string SesionToken { get; set; }
        public DateTime? SesionInicio { get; set; }
        public DateTime? SesionUltimaAccion { get; set; }
    }
}
