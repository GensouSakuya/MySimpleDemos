using System;
using System.Threading.Tasks;
using Quartz.Impl;

namespace Quartz
{
    class Once
    {
        /// <summary>
        /// 仅一次
        /// </summary>
        /// <returns></returns>
        private static async Task FireOnce()
        {
            const string key = "123";
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            var trigger = TriggerBuilder.Create().WithIdentity(key).StartNow().Build();
            var job = JobBuilder.Create<TestJob>().Build();
            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
            trigger = await scheduler.GetTrigger(new TriggerKey(key));
            Console.WriteLine(trigger.GetNextFireTimeUtc()?.ToString("yyyy-MM-dd HH:mm:ss"));
            await Task.Delay(3000);
            trigger = await scheduler.GetTrigger(new TriggerKey(key));
            //触发之后trigger就不存在了
            Console.WriteLine($"trigger is null:{trigger == null}");
        }
    }
}
