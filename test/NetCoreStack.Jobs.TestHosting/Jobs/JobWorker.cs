using System;
using System.Threading.Tasks;

namespace NetCoreStack.Jobs.TestHosting.Jobs
{
    public class JobWorker : IJob
    {
        private readonly IJobStorage _jobStorage;

        public string Id => "1";

        public JobWorker(IJobStorage jobStorage)
        {
            _jobStorage = jobStorage;
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
                        // suppress
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
