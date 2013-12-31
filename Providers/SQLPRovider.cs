using System;
using System.Web;
using System.Web.Caching;

namespace Civic.Core.Caching.Providers
{
    public class SqlProvider : ICacheProvider
    {

        public void WriteCache<TV>(string key, TV value, TimeSpan decay, CacheStore cacheStore = CacheStore.Session) where TV : class 
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

        public TV ReadCache<TV>(string key, CacheStore cacheStore) where TV : class
        {
            Cache cache = HttpRuntime.Cache;
            if (cache != null)
            {
                try
                {
                    if (cache[key] != null)
                    {
                        return (TV)cache[key];
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key,
                            cacheStore, ex.Message));
                }
            }
            return null;
        }

    }
}