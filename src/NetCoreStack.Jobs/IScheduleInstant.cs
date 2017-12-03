using System;
using System.Collections.Generic;

namespace NetCoreStack.Jobs
{
    public interface IScheduleInstant
    {
        DateTime NowInstant { get; }
        DateTime? NextInstant { get; }
        IEnumerable<DateTime> GetNextInstants(DateTime lastInstant);
    }
}
