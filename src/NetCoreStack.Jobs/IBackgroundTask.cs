using System.Threading.Tasks;

namespace NetCoreStack.Jobs
{
    internal interface IBackgroundTask
    {
        void Invoke(TaskContext context);
    }
}
