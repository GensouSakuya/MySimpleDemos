using System;
using System.Collections.Generic;
using System.Linq;
using Quartz.Spi;

namespace Quartz.CustomTriggers
{
    public class WeeklyTriggerBuilder : ScheduleBuilder<WeeklyTrigger>
    {
        private int _interval = 1;
        private List<DayOfWeek> _dayOfWeeks = new List<DayOfWeek>();
        private TimeZoneInfo _timeZone = TimeZoneInfo.Local;

        public WeeklyTriggerBuilder WithInterval(int interval)
        {
            Validator.ValidateInterval(interval);
            if (interval <= 0)
                throw new ArgumentException(nameof(interval));
            _interval = interval;
            return this;
        }
        public WeeklyTriggerBuilder WithDayOfWeek(DayOfWeek dayOfWeek)
        {
            Validator.ValidateDayOfWeek(dayOfWeek);
            if (!_dayOfWeeks.Contains(dayOfWeek))
                _dayOfWeeks.Add(dayOfWeek);
            return this;
        }
        public WeeklyTriggerBuilder WithDayOfWeek(IEnumerable<DayOfWeek> dayOfWeeks)
        {
            if (dayOfWeeks != null)
                _dayOfWeeks = dayOfWeeks.ToList();
            return this;
        }

        public WeeklyTriggerBuilder WithTimeZone(TimeZoneInfo timeZone)
        {
            _timeZone = timeZone;
            return this;
        }

        public override IMutableTrigger Build()
        {
            return new WeeklyTrigger(_interval, _dayOfWeeks.Any() ? _dayOfWeeks : null, _timeZone);
        }
    }
}
