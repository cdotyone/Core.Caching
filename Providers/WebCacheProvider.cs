using System;
using System.Web;
using System.Web.Caching;

namespace Civic.Core.Caching.Providers
{
	public class WebCacheProvider : ICacheProvider
	{

        public void WriteCache<TV>(string scope, string key, TV value, TimeSpan decay) where TV : class 
		{
			if (key == null)
				throw new NotSupportedException(SR.GetString(SR.CACHE_MANAGER_WRITE_CACHE_KEY_NULL));

			try
			{
			    key = scope + "|" + key;
				var cache = HttpRuntime.Cache;
				if (cache != null)
				{
					// ReSharper disable CompareNonConstrainedGenericWithNull
					if (typeof(TV).IsValueType) cache[key] = value;
					else
					{
						if (value == null) cache.Remove(key);
						else cache.Add(key, value, null, DateTime.Now.Add(decay), Cache.NoSlidingExpiration, CacheItemPriority.Default, null); 
					}
				}
			}
			catch (Exception ex)
			{
                throw new ArgumentException(string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key, scope, ex.Message));
			}
		}

	    public void RemoveAllByScope(string scope)
	    {
	    }

	    public TV ReadCache<TV>(string scope, string key) where TV : class
	    {
            if (key == null)
                throw new NotSupportedException(SR.GetString(SR.CACHE_MANAGER_READ_CACHE_KEY_NULL));

            var cache = HttpRuntime.Cache;
            if (cache != null)
            {
                key = scope + "|" + key;
                try
                {
                    return (cache[key] == null) ? null : (TV) cache[key];
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key, scope, ex.Message));
                }
            }

            return null;
	    }

	}
}
