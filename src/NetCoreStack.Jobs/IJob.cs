using System.Threading.Tasks;

namespace NetCoreStack.Jobs
{
    public interface IJob
    {
        string Id { get; }
        Task InvokeAsync(TaskContext context);
    }
}