using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class AttendanceEntry
    {
        public string Date { get; set; }
        public string Present { get; set; }  // "Present" or "Absent"

        public string Time { get; set; }  // ⏰ New field

    }

}
