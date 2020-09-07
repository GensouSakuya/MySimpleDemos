using System;
using System.Collections.Generic;
using System.Linq;

namespace Quartz.CustomTriggers
{
    public class WeeklyTrigger : BaseTrigger, ITrigger, IComparable<ITrigger>
    {
        private List<DayOfWeekModel> _repeatOnDayOfWeeks = new List<DayOfWeekModel>
        {
            new DayOfWeekModel(DayOfWeek.Monday, false),
            new DayOfWeekModel(DayOfWeek.Tuesday, false),
            new DayOfWeekModel(DayOfWeek.Wednesday, false),
            new DayOfWeekModel(DayOfWeek.Thursday, false),
            new DayOfWeekModel(DayOfWeek.Friday, false),
            new DayOfWeekModel(DayOfWeek.Saturday, false),
            new DayOfWeekModel(DayOfWeek.Sunday, false),
        };

        private int _interval = 1;
        private List<DayOfWeek> _configuredDayOfWeeks;


        public WeeklyTrigger(int interval, List<DayOfWeek> dayOfWeeks, TimeZoneInfo timeZone):base(timeZone)
        {
            _interval = interval;
            _configuredDayOfWeeks = dayOfWeeks;
        }

        protected override void Init()
        {
            if (_configuredDayOfWeeks != null)
                _configuredDayOfWeeks.ForEach(p => { _repeatOnDayOfWeeks.Find(q => q.DayOfWeek == p).IsIncluded = true; });
            //如果没设置重复星期x的话，就取执行时间的星期来重复执行
            else
                _repeatOnDayOfWeeks.Find(p => p.DayOfWeek == StartTimeLocal.DayOfWeek).IsIncluded = true;

            if (EndTimeLocal.HasValue)
            {
                DateTimeOffset? startTime = StartTimeLocal;
                DateTimeOffset? finalTime = null;
                while (startTime.HasValue && startTime < EndTimeLocal)
                {
                    finalTime = startTime;
                    startTime = GetNext(startTime.Value);
                }

                _finalFireTime = finalTime;
            }
        }

        protected override DateTimeOffset? GetNext(DateTimeOffset dateTimeOffset)
        {
            if (!_repeatOnDayOfWeeks.Any())
                return null;

            DateTimeOffset? nextDate;
            var dayofWeek = dateTimeOffset.DayOfWeek;
            //检查是否已经是当周最后一个需要执行的星期
            if (_repeatOnDayOfWeeks.FindLast(p => p.IsIncluded).DayOfWeek == dayofWeek)
            {
                //先前进到下个周一
                var diffIndex = 7 - _repeatOnDayOfWeeks.FindIndex(p => p.DayOfWeek == dayofWeek);
                var start = dateTimeOffset;
                start = start.AddDays(diffIndex);

                //因为周间隔==1时，已经由上面处理过了
                if (_interval > 1)
                {
                    start = start.AddDays((_interval - 1) * 7);
                }

                //前进到下一个要执行的星期
                var index = _repeatOnDayOfWeeks.FindIndex(p => p.IsIncluded);
                start = start.AddDays(index);
                nextDate = start;
            }
            else
            {
                var index = _repeatOnDayOfWeeks.FindIndex(p => p.DayOfWeek == dayofWeek);
                var nextIndex = _repeatOnDayOfWeeks.FindIndex(index + 1, p => p.IsIncluded);
                nextDate = dateTimeOffset.AddDays(nextIndex - index);
            }

            if (_manualNextTime.HasValue && _manualNextTime.Value > dateTimeOffset && _manualNextTime < nextDate)
                return _manualNextTime;

            if (_finalFireTime.HasValue && nextDate > _finalFireTime)
                return null;

            return nextDate;
        }

        private class DayOfWeekModel
        {
            public DayOfWeek DayOfWeek { get; set; }
            public bool IsIncluded { get; set; }

            public DayOfWeekModel(DayOfWeek dayOfWeek, bool isIncluded)
            {
                DayOfWeek = dayOfWeek;
                IsIncluded = isIncluded;
            }
        }
    }
}
