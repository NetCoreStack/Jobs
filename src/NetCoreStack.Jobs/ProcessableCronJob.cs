using NCrontab;
using System;

namespace NetCoreStack.Jobs
{
    internal class ProcessableCronJob
    {
        protected JobDescriptor JobDescriptor { get; }
        public CrontabSchedule Schedule { get; }
        public Type Type { get; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Next { get; set; }
        public DateTime LastRun { get; set; }
        public ProcessableCronJob(JobDescriptor jobDescriptor)
        {
            JobDescriptor = jobDescriptor;
            Schedule = jobDescriptor.Cron;
            Type = jobDescriptor.Type;
        }

        public override string ToString()
        {
            return $"Job: {Type.Name}";
        }
    }
}