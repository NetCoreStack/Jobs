using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCoreStack.Jobs
{
    public interface IJobStorage : IDisposable
    {
        StorageResult Get(string key);
        Task<List<string>> GetAllKeysAsync();
        Task<StorageResult> GetAsync(string key);
        void Remove(string key);
        Task RemoveAllKeysAsync();
        Task RemoveAsync(string key);
        Task RemoveKeysAsync(IEnumerable<IJob> jobs);
        void Set(string key, object instance);
        Task SetAsync(string key, object instance);
        Task<List<IJob>> GetJobsAsync();
        Task<List<T>> GetTypedJobsAsync<T>() where T : IJob;
    }
}