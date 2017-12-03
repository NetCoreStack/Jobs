using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCoreStack.Jobs
{
    internal class ScheduleInstant : IScheduleInstant
    {
        private readonly TimeZoneInfo _timeZone;
        private readonly CrontabSchedule _schedule;

        public static Func<CrontabSchedule, TimeZoneInfo, IScheduleInstant> Factory =
            (schedule, timeZone) => new ScheduleInstant(DateTime.UtcNow, timeZone, schedule);

        public ScheduleInstant(DateTime nowInstant, TimeZoneInfo timeZone, CrontabSchedule schedule)
        {
            if (nowInstant.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Only DateTime values in UTC should be passed.", nameof(nowInstant));
            }

            _timeZone = timeZone;
            _schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));

            NowInstant = nowInstant.AddSeconds(-nowInstant.Second);

            var nextOccurrences = _schedule.GetNextOccurrences(
                TimeZoneInfo.ConvertTime(NowInstant, TimeZoneInfo.Utc, _timeZone),
                DateTime.MaxValue);

            foreach (var nextOccurrence in nextOccurrences)
            {
                if (_timeZone.IsInvalidTime(nextOccurrence)) continue;

                NextInstant = TimeZoneInfo.ConvertTime(nextOccurrence, _timeZone, TimeZoneInfo.Utc);
                break;
            }
        }

        public ScheduleInstant()
        {
        }

        public DateTime NowInstant { get; }
        public DateTime? NextInstant { get; }

        public IEnumerable<DateTime> GetNextInstants(DateTime lastInstant)
        {
            if (lastInstant.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Only DateTime values in UTC should be passed.", nameof(lastInstant));
            }

            return _schedule
                .GetNextOccurrences(
                    TimeZoneInfo.ConvertTime(lastInstant, TimeZoneInfo.Utc, _timeZone),
                    TimeZoneInfo.ConvertTime(NowInstant.AddSeconds(1), TimeZoneInfo.Utc, _timeZone))
                .Where(x => !_timeZone.IsInvalidTime(x))
                .Select(x => TimeZoneInfo.ConvertTime(x, _timeZone, TimeZoneInfo.Utc))
                .ToList();
        }
    }
}
