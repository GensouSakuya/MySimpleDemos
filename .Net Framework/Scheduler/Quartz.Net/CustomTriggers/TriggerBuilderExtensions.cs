using System;

namespace Quartz.CustomTriggers
{
    public static class TriggerBuilderExtensions
    {
        public static TriggerBuilder WithWeeklyTrigger(this TriggerBuilder builder,Action<WeeklyTriggerBuilder> action)
        {
            var wb = new WeeklyTriggerBuilder();
            action(wb);
            return builder.WithSchedule(wb);
        }
        public static TriggerBuilder WithMonthlyTrigger(this TriggerBuilder builder, Action<MonthlyTriggerBuilder> action)
        {
            var wb = new MonthlyTriggerBuilder();
            action(wb);
            return builder.WithSchedule(wb);
        }
    }
}
