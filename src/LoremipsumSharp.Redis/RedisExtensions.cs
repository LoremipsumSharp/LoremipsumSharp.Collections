using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace LoremipsumSharp.Redis
{
    public static class RedisExtensions
    {
        public static void KeyDeleteWithPrefix(this IConnectionMultiplexer connection, string prefix)
        {
            var keys = GetKeys(connection, prefix);
            var database = connection.GetDatabase();
            database.KeyDelete(keys.Select(key => (RedisKey)key).ToArray());
        }

        public static int KeyCount(this IConnectionMultiplexer connection, string prefix)
        {
            return GetKeys(connection, prefix).Count();
        }

        public static IEnumerable<string> GetKeys(this IConnectionMultiplexer connection, string prefix)
        {
            if (connection == null)
            {
                throw new ArgumentException("Connection cannot be null", "connection");
            }

            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentException("Prefix cannot be empty", "database");
            }

            var keys = new List<string>();
            var databaseId = connection.GetDatabase().Database;

            foreach (var endPoint in connection.GetEndPoints())
            {
                var server = connection.GetServer(endPoint);
                keys.AddRange(server.Keys(pattern: prefix, database: databaseId).Select(x => x.ToString()));
            }

            return keys.Distinct();
        }
        public static T GetOrSet<T>(this IDatabase database, string key, Func<T> action, TimeSpan? timeSpan = null)
        {
            var value = database.StringGet(key);
            if (value == RedisValue.Null)
            {
                var result = action();
                var resultString = JsonConvert.SerializeObject(result);
                database.StringSet(key, resultString, timeSpan);
                return result;
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
        }
        public static async Task<T> GetOrSetAsync<T>(this IDatabase database, string key, Func<Task<T>> action, TimeSpan? timeSpan = null)
        {
            var value = await database.StringGetAsync(key);
            if (value == RedisValue.Null)
            {
                var result = await action();
                var resultString = JsonConvert.SerializeObject(result);
                await database.StringSetAsync(key, resultString, timeSpan);
                return result;
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
        }

        public static async Task SetAsync<T>(this IDatabase database, T obj, string key, TimeSpan? expiry = null)
        {
            RedisValue content;
            if (obj != null)
            {
                content = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            }
            else
            {
                content = RedisValue.Null;
            }
            await database.StringSetAsync(key, content, expiry);
        }

        public static async Task<T> GetAsync<T>(this IDatabase database, string key)
        {
            var value = await database.StringGetAsync(key);
            return value == RedisValue.Null ? default : JsonConvert.DeserializeObject<T>(value);
        }

        public static async Task DeleteKeyAsync(this IDatabase database, string redisKey)
        {
            if ((await database.KeyExistsAsync(redisKey)))
                await database.KeyDeleteAsync(redisKey);
        }
    }
}