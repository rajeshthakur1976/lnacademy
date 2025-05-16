using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;

namespace TokenManager.Cache
{
    public class CacheSettings
    {
        public static  IConfiguration? configuration;

        private static readonly Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            var redisCacheConnString = configuration["RedisConnectionString"];
            return ConnectionMultiplexer.Connect(redisCacheConnString);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

    }
}
