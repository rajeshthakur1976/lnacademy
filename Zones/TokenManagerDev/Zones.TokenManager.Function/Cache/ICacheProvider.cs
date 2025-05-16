using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TokenManager.Cache
{
    /// <summary>
    /// Contract for various cache providers. At the least, there are two providers. InMem and Azure. 
    /// InMem will help in testing the platform on development or simulated environments.
    ///  All the items going in the cache should be marked as Serializable. As this can be the requirement of underlying caching store.
    /// </summary>
    public interface ICacheProvider
    {
        void Add<T>(string key, T value);

        //void Add<T>(string key, T value, string[] tags);

        T Get<T>(string key);
        Task<RedisValue[]> GetHashValue(string key);
        Task<RedisValue> GetHashValue(string key, RedisValue redisValue);
        Task SettHashValue(string key, HashEntry[] hashEntries);
        Task<bool> IsHashExists(string key, RedisValue redisValue);
        void Remove(string key);

        IList<string> AllKeys();
        bool Contains(string key);

        void Purge();


    }
}
