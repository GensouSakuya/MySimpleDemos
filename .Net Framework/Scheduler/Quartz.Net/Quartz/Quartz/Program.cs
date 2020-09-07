using Quartz.Impl;
using Quartz.Impl.Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //await Monthly.Interval(2, 29);
            await Weekly.Interval(2,null);
        }

        private static async Task PerMonthDay()
        {
            const string key = "123";
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            Console.WriteLine(TimeZoneInfo.Local);
            var trigger = TriggerBuilder.Create().WithIdentity(key)
                .WithSchedule(MonthlyOnDayAndHourAndMinuteAndSec(31,1,1,1).InTimeZone(TimeZoneInfo.Local)).StartAt(new DateTimeOffset(DateTime.Now.AddMonths(2)))
                .Build();
            var job = JobBuilder.Create<TestJob>().Build();
            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
            Console.WriteLine((await scheduler.GetTrigger(new TriggerKey(key))).GetNextFireTimeUtc().Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            await Task.Delay(3000);
            Console.WriteLine((await scheduler.GetTrigger(new TriggerKey(key))).GetNextFireTimeUtc().Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        }



        public static CronScheduleBuilder MonthlyOnDayAndHourAndMinuteAndSec(
            int dayOfMonth,
            int hour,
            int minute,
            int sec)
        {
            DateBuilder.ValidateDayOfMonth(dayOfMonth);
            DateBuilder.ValidateHour(hour);
            DateBuilder.ValidateMinute(minute);
            var presumedValidCronExpression =
                string.Format("{0} {1} {2} {3} * ?",sec, (object) minute, (object) hour, (object) dayOfMonth);
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

    internal static class TriggerExtension
    {
        public static async Task<List<DateTime>> GetNextFireTimes(this IScheduler scheduler,string key, int count)
        {
            var list = new List<DateTime>();
            var triggers =(await scheduler.GetTriggersOfJob(new JobKey(key))).ToList();
            triggers.ForEach(t =>
            {
                DateTimeOffset? fireTime = null;
                for (var i = 0; i < count; i++)
                {
                    if (i == 0)
                    {
                        fireTime = t.GetNextFireTimeUtc();
                    }
                    else
                    {
                        fireTime = t.GetFireTimeAfter(fireTime);
                    }
                    if (fireTime.HasValue)
                        list.Add(fireTime.Value.ToLocalTime().DateTime);
                }
            });
            return list.Distinct().OrderBy(p => p).Take(count).ToList();
        }
    }

    internal class TestJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine($"start:{context.NextFireTimeUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            catch(Exception e)
            {
                var trigger = TriggerBuilder.Create().WithIdentity("retry").StartAt(DateTime.UtcNow.AddMinutes(5)).Build();
                await context.Scheduler.ScheduleJob(context.JobDetail, trigger);
                //build a new trigger
            }
        }
    }
}
