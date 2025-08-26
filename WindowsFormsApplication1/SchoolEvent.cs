using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class SchoolEvent
    {
        public string Title { get; set; }
        public string Time { get; set; }
        public string Date { get; set; } // e.g. "Aug 05"
        public string Status { get; set; } // Upcoming, Today, etc.
        public string LastUpdated { get; set; } // ISO string for sync
    }
}
