using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TokenManager.Cache
{
    /// <summary>
    /// _redisCache provider that works on top of Azure RedisCache.
    /// </summary>
    public class AzureCacheProvider : ICacheProvider
	{
        #region Private Members
            IDatabase _redisCache;
        private readonly IConfiguration _configuration;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Get RedisCache.
        /// </summary>
        public AzureCacheProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            CacheSettings.configuration = _configuration;
            _redisCache = CacheSettings.Connection.GetDatabase();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Addes or updates an item in the RedisCache provided passed in value is not <c>null</c>.
        /// </summary>
        /// <typeparam name="T">Type of object that is being stored.</typeparam>
        /// <param name="key"></param>
        /// <param name="value">Value that is to be set. If it is <c>null</c>, it is ignored. </param>
        public void Add<T>(string key, T value)
        {
            if (value != null)
            {
                var srObj = JsonConvert.SerializeObject(value, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                _redisCache.StringSet(key, srObj);
            }
        }

        /// <summary>
        /// Returns current value of the given item in the RedisCache. 
        /// Throws InvalidCastException if item in the RedisCache doesn't have the same type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            T val = default(T);
            try
            {
                
                string strTempVal = null;
                if (_redisCache.KeyExists(key))
                {
                    strTempVal = _redisCache.StringGet(key);
                }
                if (!string.IsNullOrEmpty(strTempVal))
                {
                    val = JsonConvert.DeserializeObject<T>(strTempVal); 
                }
            }
            catch (InvalidCastException)
            {
                //TODO: IF T was value type, check the type and return default value, for reference type, return null.
                return default(T);
            }
            return val;
        }

        /// <summary>
        /// Removes item with same key from the RedisCache.
        /// </summary>
        /// <param name="key">Key of the item.</param>
        public void Remove(string key)
        {
            _redisCache.KeyDelete(key);
        }

        /// <summary>
        /// Supposed to return all the keys in the RedisCache. Currently, it is not implemented.
        /// </summary>
        /// <param name="container"></param>
        public IList<string> AllKeys()
        {
            List<string> allKey = null;
            var endpoints = CacheSettings.Connection.GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = CacheSettings.Connection.GetServer(endpoint);
                foreach (var key in server.Keys())
                {
                    allKey.Add(key.ToString());
                }
            }
            return allKey;
        }

        /// <summary>
        /// Checks if there is an object in the RedisCache with given key and that has a non-null value.
        /// </summary>
        /// <param name="key">Key of the item as <c>string</c>.</param>
        /// <returns><c>true</c> if non-null value for this key exists in the RedisCache, else <c>false</c>.</returns>
        public bool Contains(string key)
        {
            return _redisCache.KeyExists(key);
        }
        
        public Dictionary<string, object> All()
        {
            throw new NotImplementedException();
        }
        

        public void Purge()
        {
            var endpoints = CacheSettings.Connection.GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = CacheSettings.Connection.GetServer(endpoint);
                server.FlushAllDatabases();
            }
        }

        public async Task<RedisValue[]> GetHashValue(string key)
        {
            return await _redisCache.HashValuesAsync(key);
        }

        public async Task SettHashValue(string key,  HashEntry[] hashEntries)
        {
             await _redisCache.HashSetAsync(key, hashEntries);
        }

        public  async Task<bool> IsHashExists(string key, RedisValue redisValue)
        {
            var result= await _redisCache.HashExistsAsync(key, redisValue);
            return result;
        }

        public async Task<RedisValue> GetHashValue(string key, RedisValue redisValue)
        {
            var result= await _redisCache.HashGetAsync(key, redisValue);
            return result;
           
        }


        #endregion
    }
}
