using Microsoft.Extensions.Options;

namespace NetCoreStack.Jobs
{
    public class RedisStorageOptions : IOptions<RedisStorageOptions>
    {        
        public string Configuration { get; set; }

        public string InstanceName { get; set; }

        public RedisStorageOptions Value
        {
            get { return this; }
        }
    }
}
