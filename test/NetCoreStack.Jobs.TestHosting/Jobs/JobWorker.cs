using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace NetCoreStack.Jobs.TestHosting.Jobs
{
    public class JobWorker : IJob
    {
        private readonly IJobStorage _jobStorage;
        private readonly ILoggerFactory _loggerFactory;

        public string Id => "1";

        public JobWorker(IJobStorage jobStorage, ILoggerFactory loggerFactory)
        {
            _jobStorage = jobStorage;
            _loggerFactory = loggerFactory;
        }

        public async Task InvokeAsync(TaskContext context)
        {
            var jobs = await _jobStorage.GetJobsAsync();
            if (jobs != null)
            {
                bool jobState = false;
                foreach (var job in jobs)
                {
                    try
                    {
                        await job.InvokeAsync(context);
                        jobState = true;
                    }
                    catch (Exception ex)
                    {
                        var logger = _loggerFactory.CreateLogger(typeof(JobWorker));
                        logger.LogError("Job execution error: ", ex);
                    }
                    finally
                    {
                        if (jobState)
                        {
                            _jobStorage.Remove(job.Id);
                        }
                    }
                }
            }
        }
    }
}
