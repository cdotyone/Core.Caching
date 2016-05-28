using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using Civic.Core.Configuration;

namespace Civic.Core.Caching.Providers
{
	public class WebCacheProvider : ICacheProvider
	{
        private static readonly ConcurrentDictionary<string,List<string>> _scopeMap = new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// The configuration for this provider
        /// </summary>
        public INamedElement Configuration { get; set; }

        public void WriteCache<TV>(string scope, string key, TV value, TimeSpan decay) where TV : class 
		{
			if (key == null)
				throw new NotSupportedException(SR.GetString(SR.CACHE_MANAGER_WRITE_CACHE_KEY_NULL));

			try
			{
                key = scope + "|" + key;
                AddToScopeMap(scope, key);

				var cache = HttpRuntime.Cache;
				if (cache != null)
				{
					// ReSharper disable CompareNonConstrainedGenericWithNull
					if (typeof(TV).IsValueType) cache[key] = value;
					else
					{
					    if (value == null) cache.Remove(key);
					    else
					    {
                            if(cache.Get(key)!=null) cache.Remove(key);
                            cache.Add(key, value, null, DateTime.Now.Add(decay), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
					    } 
					}
				}
			}
			catch (Exception ex)
			{
                throw new ArgumentException(string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key, scope, ex.Message));
			}
		}

	    private static void AddToScopeMap(string scope, string key)
	    {
	        if (!_scopeMap.ContainsKey(scope))
	        {
                _scopeMap[scope] = new List<string>();

	            if (!_scopeMap[scope].Contains(key))
	            {
                    _scopeMap[scope].Add(key);
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

	    public TV ReadCache<TV>(string scope, string key) where TV : class
	    {
            if (key == null)
                throw new NotSupportedException(SR.GetString(SR.CACHE_MANAGER_READ_CACHE_KEY_NULL));

            var cache = HttpRuntime.Cache;
            if (cache != null)
            {
                key = scope + "|" + key;
                AddToScopeMap(scope, key);

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
