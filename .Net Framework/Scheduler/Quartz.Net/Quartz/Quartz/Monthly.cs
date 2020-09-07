using Quartz.Impl;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz
{
    class Monthly
    {
        /// <summary>
        /// 每隔n月的第days天
        /// </summary>
        /// <param name="n"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public static async Task Interval(int n,int days)
        {
            const string jobkey = "123";
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            var job = JobBuilder.Create<TestJob>().WithIdentity(jobkey).Build();

            if (days < 29)
            {
                var triggerBuilder = TriggerBuilder.Create().WithIdentity("1");
                triggerBuilder.WithSchedule(MonthlyOnDayAndHourAndMinute(n, days, 0, 0));
                var trigger = triggerBuilder.StartNow().Build();
                await scheduler.ScheduleJob(job, trigger);
            }
            else if (days == 29 || days == 30)
            {
                var startDate = DateTime.Today;
                /*
                 * 需要确认会不会进入2月
                 * 如果会的话，则需要先计算下一次到2月时间隔了多少年
                 */
                //如果2月开始，必然包含2月
                var hasFeb = startDate.Month == 2;
                if (startDate.Month != 2)
                {
                    //如果不是从2月开始，则需要计算未来会不会进入2月
                    if (12 % (n % 12) == 0)
                    {
                        hasFeb = true;
                    }
                    else
                    {
                        var nextFebInYear = startDate.Month % (n % 12) * (n / 12 + 1);
                        //var yearInterval = (n/12+1) 
                    }
                }
                var triggerBuilder1 = TriggerBuilder.Create().WithIdentity("1");
                var triggerBuilder2 = TriggerBuilder.Create().WithIdentity("2");
                triggerBuilder1.WithSchedule(MonthlyOnDayAndHourAndMinute(n, days, 0, 0));
                triggerBuilder2.WithSchedule(MonthlyOnLastdayAndHourAndMinute(DateTime.Today.Year,2,2, 0, 0));
                var trigger1 = triggerBuilder1.StartNow().Build();
                var trigger2 = triggerBuilder2.StartNow().Build();
                await scheduler.ScheduleJob(job, new List<ITrigger> { trigger1, trigger2 }.AsReadOnly(), false);
            }
            else if (days >= 31)
            {
                var triggerBuilder = TriggerBuilder.Create().WithIdentity("1");
                triggerBuilder.WithSchedule(MonthlyOnLastdayAndHourAndMinute(0, 0));
                var trigger = triggerBuilder.StartNow().Build();
                await scheduler.ScheduleJob(job, trigger);
            }

            await scheduler.Start();
            await Task.Delay(3000);
            var dates = await scheduler.GetNextFireTimes(jobkey, 20);
            dates.ForEach(d => { Console.WriteLine(d.ToString("yyyy-MM-dd HH:mm:ss")); });
        }

        private static CronScheduleBuilder MonthlyOnLastdayAndHourAndMinute(
            int year,
            int yearInterval,
            int month,
            int hour,
            int minute)
        {
            DateBuilder.ValidateMonth(month);
            DateBuilder.ValidateHour(hour);
            DateBuilder.ValidateMinute(minute);
            var presumedValidCronExpression =
                    string.Format("0 {0} {1} L {2}-{2} ? /{3}", (object) minute, (object) hour, (object) month, yearInterval);
            try
            {
                return CronScheduleBuilder.CronSchedule(new CronExpression(presumedValidCronExpression));
            }
            catch (FormatException ex)
            {
                throw new Exception("CronExpression '" + presumedValidCronExpression + "' is invalid, which should not be possible, please report bug to Quartz developers.", (Exception)ex);
            }
        }

        private static CronScheduleBuilder MonthlyOnLastdayAndHourAndMinute(
            int hour,
            int minute)
        {
            DateBuilder.ValidateHour(hour);
            DateBuilder.ValidateMinute(minute);
            var presumedValidCronExpression =
                string.Format("0 {0} {1} L /1 ?", (object)minute, (object)hour);
            try
            {
                return CronScheduleBuilder.CronSchedule(new CronExpression(presumedValidCronExpression));
            }
            catch (FormatException ex)
            {
                throw new Exception("CronExpression '" + presumedValidCronExpression + "' is invalid, which should not be possible, please report bug to Quartz developers.", (Exception)ex);
            }
        }

        private static CronScheduleBuilder MonthlyOnDayAndHourAndMinute(
            int monthInterval,
            int dayOfMonth,
            int hour,
            int minute)
        {
            DateBuilder.ValidateDayOfMonth(dayOfMonth);
            DateBuilder.ValidateHour(hour);
            DateBuilder.ValidateMinute(minute);
            var presumedValidCronExpression =
                string.Format("0 {0} {1} {2} /{3} ?", (object) minute, (object) hour, (object) dayOfMonth, monthInterval);
            try
            {
                return CronScheduleBuilder.CronSchedule(new CronExpression(presumedValidCronExpression));
            }
            catch (FormatException ex)
            {
                throw new Exception("CronExpression '" + presumedValidCronExpression + "' is invalid, which should not be possible, please report bug to Quartz developers.", (Exception)ex);
            }
        }
    }
}
