using System;
using System.Collections.Generic;
using System.Text;

namespace natom.varadero.entities
{
    public class SyncScheduleInfo
    {
        public long CancellationTokenMS { get; set; }
        public List<SyncSchedule> Schedules { get; set; }
    }
}
