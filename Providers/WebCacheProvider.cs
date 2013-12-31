using System;
using System.Web;
using System.Web.Caching;

namespace Civic.Core.Caching.Providers
{
	public class WebCacheProvider : ICacheProvider
	{

        public void WriteCache<TV>(string key, TV value, TimeSpan decay, CacheStore cacheStore) where TV : class 
		{
			if (key == null)
				throw new NotSupportedException(SR.GetString(SR.CACHE_MANAGER_WRITE_CACHE_KEY_NULL));

			try
			{
				var cache = HttpRuntime.Cache;
				if (cache != null)
				{
					switch (cacheStore)
					{
						case CacheStore.Application:
							// ReSharper disable CompareNonConstrainedGenericWithNull
							if (typeof(TV).IsValueType) cache[key] = value;
							else
							{
								if (value == null) cache.Remove(key);
								else cache.Add(key, value, null, DateTime.Now.Add(decay), Cache.NoSlidingExpiration, CacheItemPriority.Default, null); 
							}
							break;
						case CacheStore.Session:
							var sessionID = SessionID;
							if (!string.IsNullOrEmpty(sessionID))
							{
								key += sessionID;
								if (typeof(TV).IsValueType) cache[key] = value;
								else
								{
									if (value == null) cache.Remove(key);
									else cache.Add(key, value, null, DateTime.Now.Add(decay), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
								}
							}
							else
							{
								throw new NotSupportedException(SR.GetString(SR.CACHE_MANAGER_WRITE_CACHE_STORE_NULL));
							}
							// ReSharper restore CompareNonConstrainedGenericWithNull
							break;
					}
				}
			}
			catch (Exception ex)
			{
				throw new ArgumentException(string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key, cacheStore, ex.Message));
			}
		}

	    public TV ReadCache<TV>(string key, CacheStore cacheStore) where TV : class
	    {
            if (key == null)
                throw new NotSupportedException(SR.GetString(SR.CACHE_MANAGER_READ_CACHE_KEY_NULL));

            var cache = HttpRuntime.Cache;
            if (cache != null)
            {
                try
                {
                    switch (cacheStore)
                    {
                        case CacheStore.Application:
                            return (cache[key] == null) ? null : (TV)cache[key];
                        case CacheStore.Session:
                            var sessionID = SessionID;
                            if (!string.IsNullOrEmpty(sessionID))
                            {
                                key += sessionID;
                                return (cache[key] == null) ? null : (TV)cache[key];
                            }
                            break;
                    }

                    throw new NotSupportedException(SR.GetString(SR.CACHE_MANAGER_READ_CACHE_STORE_NULL));
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key, cacheStore, ex.Message));
                }
            }

            return null;
	    }

		public string SessionID
		{
			get
			{
				string sessionID = string.Empty;
				if (HttpContext.Current == null) return string.Empty;
				if (HttpContext.Current.Session==null)
				{
					var cookie = HttpContext.Current.Request.Cookies["CoreCacheSessionId"];
					if(cookie!=null) sessionID = cookie.Value;
					if(string.IsNullOrEmpty(sessionID))
					{
						sessionID = Guid.NewGuid().ToString().Replace("-", "");
						cookie = new HttpCookie("CoreCacheSessionId",sessionID);
						HttpContext.Current.Response.Cookies.Add(cookie);
						return sessionID;
					}
				} else return HttpContext.Current.Session.SessionID;
				return sessionID;
			}
		}

	}
}
