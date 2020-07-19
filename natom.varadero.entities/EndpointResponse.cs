using System;
using System.Collections.Generic;
using System.Text;

namespace natom.varadero.entities
{
    public class EndpointResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
