using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class MonthlyExpense
    {
        public int Electricity { get; set; }
        public int Salaries { get; set; }
        public int Stationery { get; set; }
        public int Cleaning { get; set; }
        public int Total => Electricity + Salaries + Stationery + Cleaning;
    }

}
