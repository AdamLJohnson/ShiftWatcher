using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShiftWatcher.OrcicornMonitor.Lambda.Services
{
    public interface IPersistantStorage
    {
        Task<PersistantStorageGetResult<T>> GetAsync<T>(string key);
        Task DeleteAsync<T>(string key);
        Task InsertAsync<T>(string key, T value);
        Task UpdateAsync<T>(string key, T value);
    }

    public record PersistantStorageGetResult<T>(bool IsSuccess, string Key, T Value);

    public abstract class PersistantStorage : IPersistantStorage
    {
        protected readonly string _tableName;

        public PersistantStorage(string tableName)
        {
            _tableName = tableName;
        }

        public abstract Task DeleteAsync<T>(string key);
        public abstract Task<PersistantStorageGetResult<T>> GetAsync<T>(string key);
        public abstract Task InsertAsync<T>(string key, T value);
        public abstract Task UpdateAsync<T>(string key, T value);
    }
}
