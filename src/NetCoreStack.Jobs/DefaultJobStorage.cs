using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreStack.Jobs
{
    internal class DefualtJobStorageObject
    {
        public string Type { get; set; }
        public object Instance { get; set; }
    }

    internal class DefaultJobStorage : IJobStorage
    {
        private readonly IMemoryCache _memoryCache;
        private readonly string _instance;
        public DefaultJobStorage(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _instance = nameof(Jobs) + ":";
        }

        public StorageResult Get(string key)
        {
            var obj = _memoryCache.Get(_instance + key);
            if (obj == null)
                return null;

            if (obj is DefualtJobStorageObject storageObject)
            {
                return new StorageResult
                {
                    Type = storageObject.Type,
                    Data = storageObject.Instance
                };
            }

            return new StorageResult
            {
                Data = obj
            };
        }

        public async Task<List<string>> GetAllKeysAsync()
        {
            await Task.CompletedTask;
            return new List<string>();
        }

        public async Task<StorageResult> GetAsync(string key)
        {
            await Task.CompletedTask;
            return Get(_instance + key);
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(_instance + key);
        }

        public Task RemoveAllKeysAsync()
        {
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(_instance + key);
            return Task.CompletedTask;
        }

        public Task RemoveKeysAsync(IEnumerable<IJob> jobs)
        {
            if (jobs != null && jobs.Any())
            {
                foreach (var job in jobs)
                {
                    _memoryCache.Remove(_instance + job.Id);
                }
            }
            return Task.CompletedTask;
        }

        public void Set(string key, object instance)
        {
            var storageObject = new DefualtJobStorageObject
            {
                Instance = instance,
                Type = instance.GetType().FullName
            };

            _memoryCache.Set(key, storageObject);
        }

        public async Task SetAsync(string key, object instance)
        {
            await Task.CompletedTask;
            Set(key, instance);
        }

        public void Dispose()
        {
            return;
        }
    }
}
