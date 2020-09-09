using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentScheduler
{
    internal class Weekly
    {
        public static void Interval(int n, DayOfWeek week)
        {
            new Registry().Schedule<Job>().ToRunEvery(2).Weeks().On(DayOfWeek.Monday);
        }
    }
}
