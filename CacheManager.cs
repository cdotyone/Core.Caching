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

        public static TV ReadCache<TV>(String scope, string key, TV nullValue) where TV : class
        {
            return readCache(scope, key, TimeSpan.FromMinutes(60), nullValue);
        }

        public static TV ReadCacheDelegate<TV>(String scope, string key, Func<TV> action) where TV : class
        {
            return readCache(scope, key, TimeSpan.FromMinutes(60), action);
        }

        public static TV ReadCache<TV>(String scope, string key, TimeSpan decay, TV nullValue) where TV : class
        {
            return readCache(scope, key, decay, nullValue);
        }

        public static TV ReadCacheDelegate<TV>(String scope, string key, TimeSpan decay, Func<TV> action) where TV : class
        {
            return readCache(scope, key, decay, action);
        }

        private static TV readCache<TV>(String scope, string key, TimeSpan decay, TV nullValue) where TV : class 
		{
			try
			{
                var value = Current.ReadCache<TV>(scope, key);
                if (value == null)
                {
                    Current.WriteCache(scope, key, nullValue, decay);
                    return nullValue;
                }
			    return value;
			}
			catch (Exception ex)
			{
                throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Scope:{1}\r\nError:{2}", key, scope, ex.Message), ex);
			}
        }

        private static TV readCache<TV>(String scope, string key, TimeSpan decay, Func<TV> action) where TV : class 
		{
			try
			{
                var value = Current.ReadCache<TV>(scope, key);
                if (value == null)
                {
                    value = action();
                    Current.WriteCache(scope, key, value, decay);
                    return value;
                }
			    return value;
			}
			catch (Exception ex)
			{
                throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key, scope, ex.Message), ex);
			}
        }

        public static void WriteCache<TV>(String scope, string key, TV value) where TV : class
        {
            try
			{
                Current.WriteCache(scope, key, value, TimeSpan.FromMinutes(60));
			}
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Store:{1} Application\r\nError:{2}", key, scope, ex.Message), ex);
            }
        }

        public static void WriteCache<TV>(String scope, string key, TV value, TimeSpan decay) where TV : class
		{
			try
			{
                Current.WriteCache(scope, key, value, decay);
			}
			catch (Exception ex)
			{
                throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Store:{1} Application\r\nError:{2}", key, scope, ex.Message), ex);
            }
		}

        #endregion
    }
}