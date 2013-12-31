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
				if (_internalSyncObject == null)
				{
					var obj2 = new object();
					Interlocked.CompareExchange(ref _internalSyncObject, obj2, null);
				}
				return _internalSyncObject;
			}
		}
		private static object _internalSyncObject;

        #region Methods

        public static TV ReadSessionCache<TV>(string key, TV nullValue) where TV : class
        {
            return readCache(key, TimeSpan.FromMinutes(60), nullValue, CacheStore.Session);
        }

        public static TV ReadSessionCacheDelegate<TV>(string key, Func<TV> action) where TV : class
        {
            return readCache(key, TimeSpan.FromMinutes(60), action, CacheStore.Session);
        }

        public static TV ReadCache<TV>(string key, TV nullValue) where TV : class
        {
            return readCache(key, TimeSpan.FromMinutes(60), nullValue, CacheStore.Application);
        }

        public static TV ReadCacheDelegate<TV>(string key, Func<TV> action) where TV : class
        {
            return readCache(key, TimeSpan.FromMinutes(60), action, CacheStore.Application);
        }

        public static TV ReadCache<TV>(string key, TimeSpan decay, TV nullValue) where TV : class
        {
            return readCache(key, decay, nullValue, CacheStore.Application);
        }

        public static TV ReadCacheDelegate<TV>(string key, TimeSpan decay, Func<TV> action) where TV : class
        {
            return readCache(key, decay, action, CacheStore.Application);
        }

        private static TV readCache<TV>(string key, TimeSpan decay, TV nullValue, CacheStore cacheStore) where TV : class 
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

        private static TV readCache<TV>(string key, TimeSpan decay, Func<TV> action, CacheStore cacheStore) where TV : class 
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

        public static void WriteSessionCache<TV>(string key, TV value) where TV : class
        {
            try
            {
                Current.WriteCache(key, value, TimeSpan.FromMinutes(60), CacheStore.Session);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Store: Session\r\nError:{1}", key, ex.Message), ex);
            }
        }

        public static void WriteCache<TV>(string key, TV value) where TV : class
        {
            try
			{
                Current.WriteCache(key, value, TimeSpan.FromMinutes(60), CacheStore.Application);
			}
            catch (Exception ex)
            {
            	throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Store: Application\r\nError:{1}", key, ex.Message), ex);
            }
        }

        public static void WriteSessionCache<TV>(string key, TV value, TimeSpan decay) where TV : class
        {
            try
            {
                Current.WriteCache(key, value, decay, CacheStore.Session);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nError:{1}", key, ex.Message), ex);
            }
        }

        public static void WriteCache<TV>(string key, TV value, TimeSpan decay) where TV : class
		{
			try
			{
				Current.WriteCache(key, value, decay, CacheStore.Application);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nError:{1}", key, ex.Message), ex);
			}
		}

        #endregion
    }
}