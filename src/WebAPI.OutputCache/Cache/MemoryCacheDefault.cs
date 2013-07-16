using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.Caching;

namespace WebAPI.OutputCache.Cache
{
    using System.Collections.Generic;
    using System.Linq;

    public class MemoryCacheDefault : IApiOutputCache
    {
        private static MemoryCache Cache = MemoryCache.Default;

        public void RemoveStartsWith(string key)
        {
            lock (Cache)
            {
                Cache.Remove(key);
            }
        }

        public void RemoveItemsWithKeyStartingWith(string key)
        {
            lock (Cache)
            {
                var itemsToRemove = Cache.Where(c => c.Key.StartsWith(key)).Select(k => k.Key);
                foreach (var itemToRemove in itemsToRemove) Cache.Remove(itemToRemove);
            }
        }

        public T Get<T>(string key) where T : class
        {
            var o = Cache.Get(key) as T;
            return o;
        }

        public object Get(string key)
        {
            return Cache.Get(key);
        }

        public void Remove(string key)
        {
            lock (Cache)
            {
                Cache.Remove(key);
            }
        }

        public bool Contains(string key)
        {
            return Cache.Contains(key);
        }

        public void Add(string key, object o, DateTimeOffset expiration, string dependsOnKey = null)
        {
            var cachePolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = expiration
            };

            if (!string.IsNullOrWhiteSpace(dependsOnKey))
            {
                cachePolicy.ChangeMonitors.Add(
                    Cache.CreateCacheEntryChangeMonitor(new[] {dependsOnKey})
                );
            }
            lock (Cache)
            {
                Cache.Add(key, o, cachePolicy);
            }
        }

        public void Clear()
        {
            lock (Cache)
            {
                var cacheItems = (from n in Cache select n).ToList();

                foreach (var item in cacheItems)
                {
                    Cache.Remove(item.Key);
                }
            }
        }
    }
}