using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public static class PeriodManager
    {
        public static Dictionary<int, Tuple<TimeSpan, TimeSpan>> PeriodTimes = new Dictionary<int, Tuple<TimeSpan, TimeSpan>>
        {
            { 1, Tuple.Create(TimeSpan.Parse("08:00"), TimeSpan.Parse("08:40")) },
            { 2, Tuple.Create(TimeSpan.Parse("08:40"), TimeSpan.Parse("09:20")) },
            { 3, Tuple.Create(TimeSpan.Parse("09:20"), TimeSpan.Parse("10:00")) },
            { 4, Tuple.Create(TimeSpan.Parse("10:00"), TimeSpan.Parse("10:40")) },
            { 5, Tuple.Create(TimeSpan.Parse("10:40"), TimeSpan.Parse("11:20")) },
            { 6, Tuple.Create(TimeSpan.Parse("11:20"), TimeSpan.Parse("12:00")) },
            { 7, Tuple.Create(TimeSpan.Parse("12:00"), TimeSpan.Parse("12:40")) },
            { 8, Tuple.Create(TimeSpan.Parse("12:40"), TimeSpan.Parse("13:20")) }
        };

        public static int GetCurrentPeriod()
        {
            var now = DateTime.Now.TimeOfDay;
            foreach (var period in PeriodTimes)
            {
                if (now >= period.Value.Item1 && now <= period.Value.Item2)
                    return period.Key;
            }
            return -1; // Not during school hours
        }
    }
}
