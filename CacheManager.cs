using System;
using System.Threading;
using Civic.Core.Caching.Configuration;
using Civic.Core.Caching.Providers;
using Civic.Core.Configuration;

namespace Civic.Core.Caching
{
	public static class CacheManager
    {
		public static ICacheProvider Current
		{
			get
			{
				if( _current==null )
				{
					var config = CacheConfigurationSection.Current;
					Current = DynamicInstance.CreateInstance<ICacheProvider>(config.Assembly, config.Type);
				}

				if (_current == null) Current = new NoCacheProvider();

				return _current;
			}	
			set {
				lock (InternalSyncObject)
				{
					_current = value;
				}
			}
		}
		private static ICacheProvider _current;

		private static object InternalSyncObject
		{
			get
			{
				if (_sInternalSyncObject == null)
				{
					object obj2 = new object();
					Interlocked.CompareExchange(ref _sInternalSyncObject, obj2, null);
				}
				return _sInternalSyncObject;
			}
		}
		private static object _sInternalSyncObject;

        #region Methods

        public static TV ReadCache<TV>(string key, TV nullValue, CacheStore cacheStore = CacheStore.Session) where TV : class
        {
            return ReadCache(key, TimeSpan.FromHours(1), nullValue, cacheStore);
        }

        public static TV ReadCache<TV>(string key, Func<TV> action, CacheStore cacheStore = CacheStore.Session) where TV : class
        {
            return ReadCache(key, TimeSpan.FromHours(1), action, cacheStore);
        }

        public static TV ReadCache<TV>(string key, TimeSpan decay, TV nullValue, CacheStore cacheStore = CacheStore.Session) where TV : class 
		{
			try
			{
				var value = Current.ReadCache<TV>(key, cacheStore);
                if (value == null)
                {
                    Current.WriteCache(key, nullValue, decay, cacheStore);
                    return nullValue;
                }
			    return value;
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key, cacheStore, ex.Message), ex);
			}
        }

        public static TV ReadCache<TV>(string key, TimeSpan decay, Func<TV> action, CacheStore cacheStore = CacheStore.Session) where TV : class 
		{
			try
			{
				var value = Current.ReadCache<TV>(key, cacheStore);
                if (value == null)
                {
                    value = action();
                    Current.WriteCache(key, value, decay, cacheStore);
                    return value;
                }
			    return value;
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key, cacheStore, ex.Message), ex);
			}
        }

        public static void WriteCache<TV>(string key, TV value, CacheStore cacheStore = CacheStore.Session) where TV : class
        {
            try
			{
				if (cacheStore == CacheStore.DoNotStore) return;
				Current.WriteCache(key, value, cacheStore);
			}
            catch (Exception ex)
            {
            	throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key, cacheStore, ex.Message), ex);
            }
        }

        public static void WriteCache<TV>(string key, TV value, TimeSpan decay, CacheStore cacheStore = CacheStore.Session) where TV : class
		{
			try
			{
				if (cacheStore == CacheStore.DoNotStore) return;
				Current.WriteCache(key, value, decay, cacheStore);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key, cacheStore, ex.Message), ex);
			}
		}

        #endregion
    }
}