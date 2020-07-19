using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm.Managers
{
    public class ReceivedDataInfo
    {
        public string ListOfEntity { get; set; }
        public int Length { get; set; }
        public bool IsNull { get; set; }

        public ReceivedDataInfo BuildInfo<T>(List<T> listToProcess)
        {
            this.Length = listToProcess == null ? 0 : listToProcess.Count;
            this.IsNull = listToProcess == null;
            this.ListOfEntity = typeof(T).FullName;
            return this;
        }
    }
}