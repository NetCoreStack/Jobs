using System.Threading;

namespace NetCoreStack.Jobs
{
    internal interface IThrottler
    {
        void Throttle(CancellationToken token);
        void Delay(CancellationToken token);
    }
}
