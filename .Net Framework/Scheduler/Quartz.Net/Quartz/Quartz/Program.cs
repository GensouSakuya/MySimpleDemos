using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quartz
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //await Monthly.Interval(1, 10);
            await Monthly.Interval(1, 5, DayOfWeek.Monday);
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
            catch
            {
                var trigger = TriggerBuilder.Create().ForJob(context.JobDetail.Key).WithIdentity("retry").StartAt(DateTime.UtcNow.AddMinutes(5))
                    .Build();
                await context.Scheduler.ScheduleJob(trigger);
            }
        }
    }
}
