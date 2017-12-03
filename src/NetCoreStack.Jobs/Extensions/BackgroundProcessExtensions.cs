using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.Jobs
{
    internal static class BackgroundProcessExtensions
    {
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

        public static Type GetProcessType(this ITaskProcess process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));

            var nextProcess = process;

            while (nextProcess is IBackgroundProcessWrapper)
            {
                nextProcess = ((IBackgroundProcessWrapper)nextProcess).InnerProcess;
            }

            return nextProcess.GetType();
        }

        public static Task CreateTask(this ITaskProcess process, ILoggerFactory loggerFactory, TaskContext context)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));

            if (!(process is ITaskProcess))
            {
                throw new ArgumentOutOfRangeException(nameof(process), "Long-running process must be of type IBackgroundProcess.");
            }

            return Task.Factory.StartNew(
                () => RunProcess(process, loggerFactory, context),
                TaskCreationOptions.LongRunning);
        }

        private static async Task RunProcess(ITaskProcess process, ILoggerFactory loggerFactory, TaskContext context)
        {
            // Long-running tasks are based on custom threads (not threadpool ones) as in 
            // .NET Framework 4.5, so we can try to set custom thread name to simplify the
            // debugging experience.
            TrySetThreadName(process.ToString());

            // LogProvider.GetLogger does not throw any exception, that is why we are not
            // using the `try` statement here. It does not return `null` value as well.
            var logger = loggerFactory.CreateLogger(process.GetProcessType());
            logger.LogDebug($"Background process '{process}' started.");

            try
            {
                await process.InvokeAsync(context);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException && context.IsShutdownRequested)
                {
                    // Graceful shutdown
                    logger.LogTrace($"Background process '{process}' was stopped due to a shutdown request.");
                }
                else
                {
                    logger.LogError(
                        $"Fatal error occurred during execution of '{process}' process. It will be stopped. See the exception for details.",
                        ex);
                }
            }

            logger.LogDebug($"Background process '{process}' stopped.");
        }
    }
}
