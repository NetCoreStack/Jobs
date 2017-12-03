using System;
using System.Threading.Tasks;

namespace NetCoreStack.Jobs
{
    internal class InfiniteLoopProcess : IBackgroundProcessWrapper
    {
        public InfiniteLoopProcess(ITaskProcess innerProcess)
        {
            InnerProcess = innerProcess ?? throw new ArgumentNullException(nameof(innerProcess));
        }

        public ITaskProcess InnerProcess { get; }

        public async Task InvokeAsync(TaskContext context)
        {
            while (!context.IsShutdownRequested)
            {
                await InnerProcess.InvokeAsync(context);
            }
        }

        public override string ToString()
        {
            return InnerProcess.ToString();
        }
    }
}
