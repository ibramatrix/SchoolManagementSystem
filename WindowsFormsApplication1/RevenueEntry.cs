using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class RevenueEntry
    {
        public string MonthYear { get; set; } 
        public double FeesCollected { get; set; }
        public double FeesRemaining { get; set; }
        public double PaperMoney { get; set; }
        public double OtherRevenue { get; set; }
        public int NewAdmissions { get; set; }
    }

}
