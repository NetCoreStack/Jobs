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

        public static Type GetProcessType(this IBackgroundTask process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));

            var nextProcess = process;

            while (nextProcess is IBackgroundProcessWrapper)
            {
                nextProcess = ((IBackgroundProcessWrapper)nextProcess).InnerProcess;
            }

            return nextProcess.GetType();
        }

        public static Task CreateTask(this IBackgroundTask process, ILoggerFactory loggerFactory, TaskContext context)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));

            if (!(process is IBackgroundTask))
            {
                throw new ArgumentOutOfRangeException(nameof(process), "Long-running process must be of type IBackgroundTask.");
            }

            return Task.Factory.StartNew(
                () => RunProcess(process, loggerFactory, context),
                TaskCreationOptions.LongRunning);
        }

        private static void RunProcess(IBackgroundTask process, ILoggerFactory loggerFactory, TaskContext context)
        {
            TrySetThreadName(process.ToString());
            
            var logger = loggerFactory.CreateLogger(process.GetProcessType());
            logger.LogDebug($"Background process '{process}' started.");

            try
            {
                process.Invoke(context);
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
