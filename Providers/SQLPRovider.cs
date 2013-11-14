using System;
using System.Web;
using System.Web.Caching;

namespace Civic.Core.Caching.Providers
{
    public static class SQLProvider
    {
        public static void WriteCache<TV>(string key, TV value, TimeSpan decay,
            CacheStore cacheStore = CacheStore.Session)
        {
            try
            {
                Cache cache = HttpRuntime.Cache;
                if (cache != null)
                {
                    cache[key] = value;
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key,
                        cacheStore, ex.Message));
            }
        }


        public static TV ReadCache<TV>(string key, TV nullValue, CacheStore cacheStore = CacheStore.Session)
        {
            Cache cache = HttpRuntime.Cache;
            if (cache != null)
            {
                try
                {
                    //return (cache[key] == null) ? nullValue : (TV) cache[key];

                    //return (TV)LoginHelper.ReadCachefromDB<TV>(key, date);

                    if (cache[key] != null)
                    {
                        return (TV) cache[key];
                    }
                    return nullValue;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key,
                            cacheStore, ex.Message));
                }
            }

            return nullValue;
        }


        public static void WriteCache<TV>(string key, TV value, CacheStore cacheStore = CacheStore.Session)
        {
            WriteCache(key, value, TimeSpan.FromHours(1), cacheStore);
        }
    }
}