using System;

namespace Quartz.CustomTriggers
{
    public class Validator
    {
        public static void ValidateDayOfWeek(DayOfWeek dayOfWeek)
        {
            if (dayOfWeek < 0 || (int)dayOfWeek > 6)
                throw new ArgumentException("Invalid dayOfWeek (must be >= 0 and <= 6).");
        }

        public static void ValidateInterval(int interval)
        {
            if (interval < 0)
                throw new ArgumentException("Invalid interval (must be >= 1).");
        }
    }
}
