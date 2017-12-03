namespace NetCoreStack.Jobs
{
    internal interface IBackgroundProcessWrapper : ITaskProcess
    {
        ITaskProcess InnerProcess { get; }
    }
}
