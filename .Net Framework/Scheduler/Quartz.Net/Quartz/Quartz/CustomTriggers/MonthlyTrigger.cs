using System;

namespace Quartz.CustomTriggers
{
    public class MonthlyTrigger : BaseTrigger, ITrigger, IComparable<ITrigger>
    {
        private int _interval = 1;
        private IntervalMode _mode;

        public MonthlyTrigger(int interval, StartModel startModel, TimeZoneInfo timeZone) : base(timeZone)
        {
            _interval = interval;
            _ = startModel ?? throw new ArgumentNullException();
            _mode = startModel.NthDay.HasValue ? IntervalMode.Day :
                startModel.NthDayOfWeek.HasValue ? IntervalMode.DayOfWeek :
                throw new ArgumentException(nameof(startModel));
        }

        protected override void Init()
        {
            
        }

        protected override DateTimeOffset? GetNext(DateTimeOffset dateTimeOffset)
        {
            return null;
        }

        public class StartModel
        {
            public int? NthDay { get; set; }

            public int? NthDayOfWeek { get; set; }
            public DayOfWeek DayOfWeek { get; set; }
        }

        private enum IntervalMode
        {
            Day,
            DayOfWeek
        }
    }
}
