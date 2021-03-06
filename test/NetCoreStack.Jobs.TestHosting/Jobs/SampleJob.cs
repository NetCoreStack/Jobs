﻿using System;
using System.Threading.Tasks;

namespace NetCoreStack.Jobs.TestHosting.Jobs
{
    public class SampleJob : IJob
    {
        public string Id => "2";

        public Task InvokeAsync(TaskContext context)
        {
            Console.WriteLine(context.Properties);
            return Task.CompletedTask;
        }
    }
}
