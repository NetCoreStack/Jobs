using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCrontab;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreStack.Jobs
{
    internal class CronJobScheduler : ITaskProcess
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IThrottler _throttler;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, ProcessableCronJob> _jobDictionary;
        private readonly IJobStorage _jobStorage;
        private readonly Func<CrontabSchedule, TimeZoneInfo, IScheduleInstant> _instantFactory;
        private static readonly TimeSpan LockTimeout = TimeSpan.FromMinutes(1);
        private static readonly TimeZoneInfo timeZone = TimeZoneInfo.Utc;

        public CronJobScheduler(IServiceProvider serviceProvider, 
            IThrottler throttler,
            Func<CrontabSchedule, TimeZoneInfo, IScheduleInstant> instantFactory,
            ConcurrentDictionary<string, ProcessableCronJob> jobDictionary,
            IJobStorage jobStorage,
            ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _throttler = throttler;
            _instantFactory = instantFactory;
            _jobDictionary = jobDictionary;
            _jobStorage = jobStorage;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<CronJobScheduler>();
        }

        private async Task RunAsync(ProcessableCronJob job, TaskContext context)
        {
            var cronSchedule = job.Schedule;
            var nowInstant = _instantFactory(cronSchedule, timeZone);

            var lastInstant = GetLastInstant(job, nowInstant);

            if (nowInstant.GetNextInstants(lastInstant).Any())
            {
                using (var scopedContext = _serviceProvider.CreateScope())
                {
                    var provider = scopedContext.ServiceProvider;
                    var cronJob = (IJob)provider.GetService(job.Type);
                    job.CreatedAt = nowInstant.NowInstant;
                    try
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        await cronJob.InvokeAsync(context);
                        sw.Stop();
                        job.LastRun = nowInstant.NowInstant;
                        _logger.LogDebug($"{job.ToString()} is executed. Utc: {nowInstant.NowInstant}, TotalSeconds: {sw.Elapsed.TotalSeconds}");
                    }
                    catch (Exception ex)
                    {
                        _jobStorage.Set(cronJob.Id, cronJob);
                        _logger.LogError(ex, $"{nameof(CronJobScheduler)} exception");
                    }

                    job.Next = nowInstant.NextInstant.HasValue ? (DateTime?)nowInstant.NextInstant.Value : null;
                }
            }
        }

        private DateTime GetLastInstant(ProcessableCronJob job, IScheduleInstant instant)
        {
            DateTime lastInstant;
            if (job.LastRun != default(DateTime))
            {
                lastInstant = job.LastRun;
            }
            else if(job.CreatedAt != default(DateTime))
            {
                lastInstant = job.CreatedAt;
            }
            else if(job.Next.HasValue)
            {
                lastInstant = job.Next.Value;
                lastInstant = lastInstant.AddSeconds(-1);
            }
            else
            {
                lastInstant = instant.NowInstant.AddSeconds(-1);
            }

            return lastInstant;
        }

        public async Task InvokeAsync(TaskContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            await Task.CompletedTask;

            _throttler.Throttle(context.CancellationToken);

            var cronJobs = _jobDictionary.Select(p => p.Value).ToArray();
            await Task.WhenAll(cronJobs.Select(job => RunAsync(job, context)));

            _throttler.Delay(context.CancellationToken);
        }
    }
}
