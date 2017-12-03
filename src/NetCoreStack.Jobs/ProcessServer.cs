using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.Jobs
{
    internal class ProcessServer : ITaskProcess, IDisposable
    {
        private readonly IList<ITaskProcess> _processes;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;        
        private readonly JobBuilderOptions _options;
        private readonly Task _bootstrapTask;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public static readonly TimeSpan DefaultShutdownTimeout = TimeSpan.FromSeconds(15);

        public static ConcurrentDictionary<string, ProcessableCronJob> JobDictionary { get; private set; }

        private static void TrySetThreadName(string name)
        {
            try
            {
                Thread.CurrentThread.Name = name;
            }
            catch (InvalidOperationException)
            {
            }
        }

        private ITaskProcess WrapProcess(ITaskProcess process)
        {
            return new InfiniteLoopProcess(process);
        }

        private string GetGloballyUniqueServerId()
        {
            var serverName = Environment.GetEnvironmentVariable("COMPUTERNAME") ?? 
                Environment.GetEnvironmentVariable("HOSTNAME");

            var guid = Guid.NewGuid().ToString();

            return !String.IsNullOrWhiteSpace(serverName)
                ? $"{serverName.ToLowerInvariant()}:{guid}"
                : guid;
        }

        public ProcessServer(IServiceProvider serviceProvider, 
            JobBuilderOptions options,
            IJobStorage jobStorage,
            ILoggerFactory loggerFactory)
        {            
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(_serviceProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            JobDictionary = new ConcurrentDictionary<string, ProcessableCronJob>(_options.JobList.ToDictionary(p => p.Id, p => new ProcessableCronJob(p)));

            _processes = new List<ITaskProcess>
            {
                new CronJobScheduler(_serviceProvider, new EveryMinuteThrottler(), ScheduleInstant.Factory, JobDictionary, jobStorage, _loggerFactory)
            };

            var context = new TaskContext(
               GetGloballyUniqueServerId(),
               properties,
               _cts.Token);

            _bootstrapTask = WrapProcess(this).CreateTask(_loggerFactory, context);
        }

        public Task InvokeAsync(TaskContext context)
        {
            try
            {
                var tasks = _processes
                    .Select(WrapProcess)
                    .Select(process => process.CreateTask(_loggerFactory, context))
                    .ToArray();

                Task.WaitAll(tasks);
            }
            finally
            {
                
            }

            return Task.CompletedTask;
        }

        public void SendStop()
        {
            _cts.Cancel();
        }

        public void Dispose()
        {
            SendStop();

            if (!_bootstrapTask.Wait(DefaultShutdownTimeout))
            {
                var logger = _loggerFactory.CreateLogger(typeof(ProcessServer));
                logger.LogWarning("Processing server takes too long to shutdown. Performing ungraceful shutdown.");
            }
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
