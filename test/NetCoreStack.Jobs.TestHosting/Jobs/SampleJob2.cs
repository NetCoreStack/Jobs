using System;
using System.Threading.Tasks;

namespace NetCoreStack.Jobs.TestHosting.Jobs
{
    public class SampleJob2 : IJob
    {
        public string Id => "3";

        public Task InvokeAsync(TaskContext context)
        {
            throw new ArgumentNullException("An error occurred");
        }
    }
}
