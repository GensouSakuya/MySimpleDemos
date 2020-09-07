using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quartz.CustomTriggers;
using Quartz.Impl;

namespace Quartz
{
    class Weekly
    {
        /// <summary>
        /// 每隔n天
        /// </summary>
        /// <param name="n"></param>
        /// <param name="dayOfWeeks"></param>
        /// <returns></returns>
        public static async Task Interval(int n, IEnumerable<DayOfWeek> dayOfWeeks)
        {
            const string key = "123";
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            var trigger = TriggerBuilder.Create().WithIdentity("123")
                .WithWeeklyTrigger(p=>p.WithInterval(n).WithDayOfWeek(dayOfWeeks))
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
