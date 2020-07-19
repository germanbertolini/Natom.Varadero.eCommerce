using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace natom.ecomm.sync.kernel
{
    public class EndpointResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
