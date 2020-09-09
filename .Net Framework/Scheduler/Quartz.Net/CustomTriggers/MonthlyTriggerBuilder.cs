using System;
using Quartz.Spi;

namespace Quartz.CustomTriggers
{
    public class MonthlyTriggerBuilder : ScheduleBuilder<MonthlyTrigger>
    {
        private int _interval = 1;
        private MonthlyTrigger.IntervalMode _intervalMode;
        private DayOfWeek _dayOfWeek;
        private int _nth;
        private TimeZoneInfo _timeZone = TimeZoneInfo.Local;

        public MonthlyTriggerBuilder WithInterval(int interval)
        {
            Validator.ValidateInterval(interval);
            if (interval <= 0)
                throw new ArgumentException(nameof(interval));
            _interval = interval;
            return this;
        }

        public MonthlyTriggerBuilder WithNthDayOfWeek(int nth, DayOfWeek dayOfWeek)
        {
            Validator.ValidateNthDayOfWeek(nth);
            Validator.ValidateDayOfWeek(dayOfWeek);
            _intervalMode = MonthlyTrigger.IntervalMode.DayOfWeek;
            _nth = nth;
            _dayOfWeek = dayOfWeek;
            return this;
        }
        public MonthlyTriggerBuilder WithNthDay(int nth)
        {
            Validator.ValidateNthDay(nth);
            _intervalMode = MonthlyTrigger.IntervalMode.Day;
            _nth = nth;
            return this;
        }

        public MonthlyTriggerBuilder WithTimeZone(TimeZoneInfo timeZone)
        {
            _timeZone = timeZone;
            return this;
        }

        public override IMutableTrigger Build()
        {
            return new MonthlyTrigger(_interval, _intervalMode, new MonthlyTrigger.StartModel
            {
                DayOfWeek = _dayOfWeek,
                Nth = _nth
            }, _timeZone);
        }
    }
}
