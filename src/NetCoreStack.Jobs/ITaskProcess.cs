using System.Threading.Tasks;

namespace NetCoreStack.Jobs
{
    internal interface ITaskProcess
    {
        Task InvokeAsync(TaskContext context);
    }
}
