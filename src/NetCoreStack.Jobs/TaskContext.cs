using System;
using System.Collections.Generic;
using System.Threading;

namespace NetCoreStack.Jobs
{
    public class TaskContext
    {
        public string ServerId { get; }
        public IReadOnlyDictionary<string, object> Properties { get; }
        public CancellationToken CancellationToken { get; }
        public bool IsShutdownRequested => CancellationToken.IsCancellationRequested;

        public TaskContext(string serverId, IDictionary<string, object> properties,
            CancellationToken cancellationToken)
        {
            ServerId = serverId;
            Properties = new Dictionary<string, object>(properties, StringComparer.OrdinalIgnoreCase);
            CancellationToken = cancellationToken;
        }
    }
}