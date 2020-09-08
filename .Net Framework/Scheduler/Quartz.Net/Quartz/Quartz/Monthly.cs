using Quartz.CustomTriggers;
using Quartz.Impl;
using System;
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
            const string key = "123";
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            var trigger = TriggerBuilder.Create().WithIdentity("123")
                .WithMonthlyTrigger(p => p.WithInterval(n).WithNthDay(days))
                .StartNow()
                .Build();
            var job = JobBuilder.Create<TestJob>().WithIdentity(key).Build();
            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
            await Task.Delay(3000);
            var dates = await scheduler.GetNextFireTimes(key, 20);
            dates.ForEach(d => { Console.WriteLine(d.ToString("yyyy-MM-dd HH:mm:ss")); });
        }

        /// <summary>
        /// 每隔n月的第x个星期y
        /// </summary>
        /// <param name="n"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public static async Task Interval(int n, int nth, DayOfWeek dayOfWeek)
        {
            const string key = "123";
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            var trigger = TriggerBuilder.Create().WithIdentity("123")
                .WithMonthlyTrigger(p => p.WithInterval(n).WithNthDayOfWeek(nth,dayOfWeek))
                .StartNow()
                .Build();
            var job = JobBuilder.Create<TestJob>().WithIdentity(key).Build();
            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
            await Task.Delay(3000);
            var dates = await scheduler.GetNextFireTimes(key, 20);
            dates.ForEach(d => { Console.WriteLine(d.ToString("yyyy-MM-dd HH:mm:ss")); });
        }
    }
}
