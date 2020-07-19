using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace natom.ecomm.sync.kernel.Models
{
    public class Log
    {
        public long Id { get; set; }

        public string LogType { get; set; }
        public string Application { get; set; }
        public string EjecucionId { get; set; }

        public string SesionIP { get; set; }
        public string SesionAgent { get; set; }

        public DateTime FechaHora { get; set; }
        public string ActionUrl { get; set; }

        public string DataContext { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}
