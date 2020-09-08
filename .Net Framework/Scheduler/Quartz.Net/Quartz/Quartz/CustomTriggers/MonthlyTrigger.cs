using System;

namespace Quartz.CustomTriggers
{
    public class MonthlyTrigger : BaseTrigger, ITrigger, IComparable<ITrigger>
    {
        private int _interval = 1;
        private IntervalMode _mode;
        private int _nth;
        private DayOfWeek _dayOfWeek;

        public MonthlyTrigger(int interval, IntervalMode mode, StartModel startModel, TimeZoneInfo timeZone) : base(timeZone)
        {
            _interval = interval;
            _ = startModel ?? throw new ArgumentNullException();
            _mode = mode;
            _nth = startModel.Nth;
            if (mode == IntervalMode.DayOfWeek)
            {
                _dayOfWeek = startModel.DayOfWeek;
            }
        }

        protected override void Init()
        {
            if (_mode == IntervalMode.Day)
            {
                //如果是第29天执行，则需要确认是否只会在2月触发
                //如果是，需要确认循环间隔是否等于4年
                //如果是，需要判断当年2月是否有29号
                //如果没有，则说明这个定时任务永远不会执行
                if (_nth == 29)
                {
                    if (StartTimeLocal.Month == 2)
                    {
                        if (_interval % 12 == 0 && _interval / 12 == 4)
                        {
                            if (new DateTime(StartTimeLocal.Year, StartTimeLocal.Month, 1).AddDays(28).Month != 2)
                            {
                                throw new ArgumentException("this trigger will never hit");
                            }
                        }
                    }
                }
                else if (_nth > 29)
                {
                    var startDate = new DateTime(StartTimeLocal.Year, StartTimeLocal.Month, 1);
                    //如果满足下面条件，说明在循环时会存在无法覆盖到的月份
                    //此时需要检查覆盖到的月份是否存在NthDay
                    if (_interval % 12 != 1 && (_interval % 12 == 0 || 12 % (_interval % 12) == 0))
                    {
                        if (startDate.AddDays(_nth - 1).Month != startDate.Month)
                        {
                            int nextMonth;
                            var isHit = false;
                            do
                            {
                                var nextDate = startDate.AddMonths(_interval);
                                nextMonth = nextDate.Month;
                                if (nextDate.AddDays(_nth - 1).Month == nextMonth)
                                {
                                    isHit = true;
                                    break;
                                }

                            } while (nextMonth != startDate.Month);
                            if (!isHit)
                                throw new ArgumentException("this trigger will never hit");
                        }
                    }
                }
            }
        }

        protected override DateTimeOffset? GetNext(DateTimeOffset dateTimeOffset)
        {
            if (_finalFireTime.HasValue && dateTimeOffset > _finalFireTime)
                return null;
            var beginDate = new DateTime(dateTimeOffset.Year, dateTimeOffset.Month, 1,
                dateTimeOffset.Hour, dateTimeOffset.Minute, dateTimeOffset.Second);
            var firstDayOfMonth = new DateTimeOffset(beginDate);
            DateTimeOffset? next = null;
            if (_mode == IntervalMode.Day)
            {
                var newDate = firstDayOfMonth.AddDays(_nth - 1);
                var newMonth = firstDayOfMonth.Month;
                var nextFirstDayOfMonth = firstDayOfMonth;
                //如果第n天后的月份与当前月份不一致，说明该月没有这一天
                while (newDate.Month != newMonth)
                {
                    nextFirstDayOfMonth = nextFirstDayOfMonth.AddMonths(_interval);
                    newMonth = nextFirstDayOfMonth.Month;
                    newDate = nextFirstDayOfMonth.AddDays(_nth - 1);

                    if (_finalFireTime.HasValue && next > _finalFireTime)
                        return null;
                }

                next = newDate;
            }
            else
            {
                var nextFirstDayOfMonth = firstDayOfMonth;
                var isHit = false;
                do
                {
                    var firstDate = nextFirstDayOfMonth;
                    //查找第一个星期x
                    while (firstDate.DayOfWeek != _dayOfWeek)
                    {
                        firstDate = firstDate.AddDays(1);
                    }

                    next = firstDate;
                    if (_nth == 1)
                    {
                        isHit = true;
                    }
                    else
                    {
                        next = firstDate.AddDays((_nth - 1) * 7);
                        if (next.Value.Month == firstDate.Month)
                        {
                            isHit = true;
                        }
                        else
                        {
                            nextFirstDayOfMonth = nextFirstDayOfMonth.AddMonths(_interval);
                            next = nextFirstDayOfMonth;
                        }
                    }
                    if (_finalFireTime.HasValue && next > _finalFireTime)
                        return null;
                } while (!isHit);
            }

            if (_finalFireTime.HasValue && next > _finalFireTime)
                return null;

            //如果结果一致的话说明当月已经进行过了，需要再获取一次
            if (next == dateTimeOffset)
                next = GetNext(firstDayOfMonth.AddMonths(_interval));

            return next;
        }

        public class StartModel
        {
            public int Nth { get; set; }
            public DayOfWeek DayOfWeek { get; set; }
        }

        public enum IntervalMode
        {
            Day,
            DayOfWeek
        }
    }
}
