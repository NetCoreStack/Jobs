using System;

namespace NetCoreStack.Jobs.BackgroundJob
{
    public interface IProcessFactory
    {
        IJob Create(Type type);
    }
}