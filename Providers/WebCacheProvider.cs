using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using Civic.Core.Configuration;
using Civic.Core.Logging;

namespace Civic.Core.Caching.Providers
{
	public class WebCacheProvider : ICacheProvider
	{
        private static readonly ConcurrentDictionary<string,List<string>> _scopeMap = new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// The configuration for this provider
        /// </summary>
        public INamedElement Configuration { get; set; }

        public void WriteCache<TV>(string scope, string cacheKey, TV value, TimeSpan decay) where TV : class 
		{
			if (cacheKey == null)
				throw new NotSupportedException(SR.GetString(SR.CACHE_MANAGER_WRITE_CACHE_KEY_NULL));

			try
			{
                cacheKey = scope + "|" + cacheKey;

                Logger.LogTrace(LoggingBoundaries.DataLayer, "WebCacheProvider - Scope {1} Key {2} - Write", scope, cacheKey);

                AddToScopeMap(scope, cacheKey);

				var cache = HttpRuntime.Cache;
				if (cache != null)
				{
					// ReSharper disable CompareNonConstrainedGenericWithNull
					if (typeof(TV).IsValueType) cache[cacheKey] = value;
					else
					{
					    if (value == null)
					    {
                            Logger.LogTrace(LoggingBoundaries.DataLayer, "WebCacheProvider - Scope {1} Key {2} - Value Null - Remove", scope, cacheKey);
                            cache.Remove(cacheKey);
					    }
					    else
					    {
                            if(cache.Get(cacheKey)!=null) cache.Remove(cacheKey);
                            cache.Add(cacheKey, value, null, DateTime.Now.Add(decay), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
					    } 
					}
				}
			}
			catch (Exception ex)
			{
                throw new ArgumentException(string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", cacheKey, scope, ex.Message));
			}
		}

	    private static void AddToScopeMap(string scope, string cacheKey)
	    {
	        if (!_scopeMap.ContainsKey(scope))
	        {
                _scopeMap[scope] = new List<string>();

	            if (!_scopeMap[scope].Contains(cacheKey))
	            {
                    _scopeMap[scope].Add(cacheKey);
	            }
	        }
	    }

	    public void RemoveAllByScope(string scope)
	    {
            var cache = HttpRuntime.Cache;
	        if (cache != null)
	        {
                if (_scopeMap.ContainsKey(scope))
                {
                    foreach (var key in _scopeMap[scope])
                    {
                        cache.Remove(key);
                    }
                    _scopeMap[scope].Clear();
                }
	        }
	    }

	    public TV ReadCache<TV>(string scope, string cacheKey) where TV : class
	    {
            if (cacheKey == null)
                throw new NotSupportedException(SR.GetString(SR.CACHE_MANAGER_READ_CACHE_KEY_NULL));

            var cache = HttpRuntime.Cache;
            if (cache != null)
            {
                cacheKey = scope + "|" + cacheKey;
                AddToScopeMap(scope, cacheKey);

                Logger.LogTrace(LoggingBoundaries.DataLayer, "WebCacheProvider - Scope {1} Key {2} - Read", scope, cacheKey);

                try
                {
                    Logger.LogTrace(LoggingBoundaries.DataLayer, "WebCacheProvider - Scope {1} Key {2} - Read - ", scope, cacheKey, ((cache[cacheKey] == null) ? "Not Found" : "Found"));

                    return (cache[cacheKey] == null) ? null : (TV) cache[cacheKey];
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", cacheKey, scope, ex.Message));
                }
            }

            return null;
	    }


	}
}
