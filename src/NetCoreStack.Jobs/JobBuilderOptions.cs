using NCrontab;
using System;
using System.Collections.Generic;

namespace NetCoreStack.Jobs
{
    public class JobBuilderOptions
    {
        internal List<JobDescriptor> JobList { get; }

        public JobBuilderOptions()
        {
            JobList = new List<JobDescriptor>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TJob">Job type</typeparam>
        /// <param name="cronExpression">Cron expression, <see cref="Cron"/> </param>
        public void Register<TJob>(string cronExpression) where TJob : IJob
        {
            var cron = CrontabSchedule.TryParse(cronExpression);
            if (cron == null)
            {
                throw new ArgumentOutOfRangeException($"{nameof(cronExpression)} is not valid!");
            }

            JobList.Add(new JobDescriptor(cron, typeof(TJob)));
        }
    }
}
