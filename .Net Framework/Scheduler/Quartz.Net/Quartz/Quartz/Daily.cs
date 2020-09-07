using Quartz.Impl;
using System;
using System.Threading.Tasks;

namespace Quartz
{
    class Daily
    {
        /// <summary>
        /// 每隔N天
        /// </summary>
        /// <returns></returns>
        public static async Task DailyInterval(int n)
        {
            const string key = "123";
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            var trigger = TriggerBuilder.Create().WithIdentity(key).StartNow()
                .WithCalendarIntervalSchedule(p => p.WithIntervalInDays(n))
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
