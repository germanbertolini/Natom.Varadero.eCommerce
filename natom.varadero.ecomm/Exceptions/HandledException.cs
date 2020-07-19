using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Exceptions
{
    public class HandledException : Exception
    {
        public bool Existe { get; set; }
        public bool DadoDeBaja { get; set; }
        public object DadoDeBajaId { get; set; }

        public HandledException()
            : base() { }

        public HandledException(string message, bool existe, bool dadoDeBaja, object dadoDeBajaId)
            : base(message)
        {
            this.Existe = existe;
            this.DadoDeBaja = dadoDeBaja;
            this.DadoDeBajaId = dadoDeBajaId;
        }

        public HandledException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public HandledException(string message, Exception innerException)
            : base(message, innerException) { }

        public HandledException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }
}