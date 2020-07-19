using System;
using System.Collections.Generic;
using System.Text;

namespace natom.varadero.entities
{
    public class eCommerceStatus
    {
        public int Id { get; set; }
        public DateTime? LastStartUp { get; set; }
        public int RunningSyncsCounter { get; set; }
    }
}
