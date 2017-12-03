using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly static List<string> _keys = new List<string>();

        private readonly IMemoryCache _memoryCache;
        private readonly string _instance;
        public DefaultJobStorage(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _instance = nameof(Jobs) + ":";
        }

        private string GenerateKey(string key)
        {
            return _instance + key;
        }

        public StorageResult Get(string key)
        {
            var instanceKey = GenerateKey(key);
            var obj = _memoryCache.Get(instanceKey);
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
            return _keys;
        }

        public async Task<StorageResult> GetAsync(string key)
        {
            await Task.CompletedTask;
            return Get(key);
        }

        public void Remove(string key)
        {
            var instanceKey = GenerateKey(key);
            _memoryCache.Remove(instanceKey);

            _keys.Remove(instanceKey);
        }

        public Task RemoveAllKeysAsync()
        {
            _keys.Clear();
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public Task RemoveKeysAsync(IEnumerable<IJob> jobs)
        {
            if (jobs != null && jobs.Any())
            {
                foreach (var job in jobs)
                {
                    var instanceKey = GenerateKey(job.Id);
                    _memoryCache.Remove(instanceKey);
                    _keys.Remove(instanceKey);
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

            var instanceKey = GenerateKey(key);

            _memoryCache.Set(instanceKey, storageObject);

            if (!_keys.Contains(instanceKey))
            {
                _keys.Add(key);
            }
        }

        public async Task SetAsync(string key, object instance)
        {
            await Task.CompletedTask;
            Set(key, instance);
        }

        public async Task<List<T>> GetTypedJobsAsync<T>() where T : IJob
        {
            List<T> jobs = new List<T>();
            var keys = await GetAllKeysAsync();
            if (keys != null && keys.Any())
            {
                foreach (var key in keys)
                {
                    StorageResult storageResult = Get(key);
                    if (storageResult != null)
                    {
                        var fullname = storageResult.Type;
                        var typedJob = typeof(T).FullName;

                        if (fullname == typedJob)
                        {
                            var job = (T)storageResult.Data;
                            jobs.Add(job);
                        }
                    }
                }
            }

            return jobs;
        }

        public async Task<List<IJob>> GetJobsAsync()
        {
            List<IJob> jobs = new List<IJob>();
            var keys = await GetAllKeysAsync();
            if (keys != null && keys.Any())
            {
                foreach (var key in keys)
                {
                    StorageResult storageResult = Get(key);
                    if (storageResult != null)
                    {
                        var job = (IJob)storageResult.Data;
                        jobs.Add(job);
                    }
                }
            }

            return jobs;
        }

        public void Dispose()
        {
            return;
        }
    }
}
