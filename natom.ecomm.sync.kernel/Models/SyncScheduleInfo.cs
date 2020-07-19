using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace natom.ecomm.sync.kernel.Models
{
    public class SyncScheduleInfo
    {
        public long CancellationTokenMS { get; set; }
        public List<SyncSchedule> Schedules { get; set; }
    }
}
