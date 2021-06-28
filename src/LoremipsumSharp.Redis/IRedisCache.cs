using System;
using StackExchange.Redis;

namespace LoremipsumSharp.Redis
{
   public interface IRedisCache
    {
        ConnectionMultiplexer Connection { get; }
        IDatabase Database { get; }
    }

    public class RedisCache : IRedisCache
    {
        private static Lazy<ConnectionMultiplexer> _lazyConnection;
        public RedisCache(string connStr)
        {
            _lazyConnection =
                new Lazy<ConnectionMultiplexer>(() =>
                {
                    return ConnectionMultiplexer.Connect(connStr);
                });
        }

        public ConnectionMultiplexer Connection => _lazyConnection.Value;

        public IDatabase Database => Connection.GetDatabase();
    }
}