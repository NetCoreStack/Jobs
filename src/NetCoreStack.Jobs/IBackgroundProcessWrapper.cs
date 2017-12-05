namespace NetCoreStack.Jobs
{
    internal interface IBackgroundProcessWrapper : IBackgroundTask
    {
        IBackgroundTask InnerProcess { get; }
    }
}
