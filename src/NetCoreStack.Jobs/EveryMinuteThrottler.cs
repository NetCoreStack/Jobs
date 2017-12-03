using System;
using System.Threading;

namespace NetCoreStack.Jobs
{
    internal class EveryMinuteThrottler : IThrottler
    {
        public void Throttle(CancellationToken token)
        {
            while (DateTime.Now.Second != 0)
            {
                WaitASecondOrThrowIfCanceled(token);
            }
        }

        public void Delay(CancellationToken token)
        {
            WaitASecondOrThrowIfCanceled(token);
        }

        private static void WaitASecondOrThrowIfCanceled(CancellationToken token)
        {
            token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
            token.ThrowIfCancellationRequested();
        }
    }
}