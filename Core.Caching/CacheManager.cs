using System;
using System.Threading;
using Core.Caching.Configuration;
using Core.Caching.Providers;
using Core.Configuration;
using Core.Configuration.Framework;

namespace Core.Caching
{
    /// <summary>
    /// Provides Caching Abstraction Layer
    /// </summary>
	public static class CacheManager
    {
        #region Fields

        public static ICacheProvider Current
		{
			get
			{
				if( _current==null )
				{
					var config = CacheConfig.Current;
				    var currentConfig = config.Providers[config.DefaultProvider.ToLowerInvariant()];
                    Current = DynamicInstance.CreateInstance<ICacheProvider>(currentConfig.Assembly, currentConfig.Type);
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

        #endregion Fields

        #region Methods

        #region ReadCache

        public static TV ReadCache<TV>(string scope, string key, TV nullValue) where TV : class
        {
            return readCache(null, scope, key, TimeSpan.FromMinutes(60), nullValue);
        }

        public static TV ReadCacheDelegate<TV>(string scope, string key, Func<TV> action) where TV : class
        {
            return readCache(null, scope, key, TimeSpan.FromMinutes(60), action);
        }

        public static TV ReadCache<TV>(string scope, string key, TimeSpan decay, TV nullValue) where TV : class
        {
            return readCache(null, scope, key, decay, nullValue);
        }

        public static TV ReadCacheDelegate<TV>(string scope, string key, TimeSpan decay, Func<TV> action) where TV : class
        {
            return readCache(null, scope, key, decay, action);
        }

        #endregion ReadCache

        #region ReadProvider

        public static TV ReadProviderCache<TV>(string provider, string scope, string key, TV nullValue) where TV : class
        {
            return readCache(provider, scope, key, TimeSpan.FromMinutes(60), nullValue);
        }

        public static TV ReadProviderCacheDelegate<TV>(string provider, string scope, string key, Func<TV> action) where TV : class
        {
            return readCache(provider, scope, key, TimeSpan.FromMinutes(60), action);
        }

        public static TV ReadProviderCache<TV>(string provider, string scope, string key, TimeSpan decay, TV nullValue) where TV : class
        {
            return readCache(provider, scope, key, decay, nullValue);
        }

        public static TV ReadProviderCacheDelegate<TV>(string provider, string scope, string key, TimeSpan decay, Func<TV> action) where TV : class
        {
            return readCache(provider, scope, key, decay, action);
        }

        #endregion ReadProvider

        #region WriteCache

        public static void WriteCache<TV>(string scope, string key, TV value) where TV : class
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

        public static void WriteCache<TV>(string scope, string key, TV value, TimeSpan decay) where TV : class
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

        #endregion WriteCache

        #region WriteProviderCache

        public static void WriteProviderCache<TV>(string provider, string scope, string key, TV value) where TV : class
        {
            try
            {
                ICacheProvider cache = Current;
                if (!string.IsNullOrEmpty(provider)) cache = CacheConfig.Current.Providers[provider.ToLowerInvariant()].Provider;
                
                cache.WriteCache(scope, key, value, TimeSpan.FromMinutes(60));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Store:{1} Application\r\nError:{2}", key, scope, ex.Message), ex);
            }
        }

        public static void WriteProviderCache<TV>(string provider, string scope, string key, TV value, TimeSpan decay) where TV : class
        {
            try
            {
                ICacheProvider cache = Current;
                if (!string.IsNullOrEmpty(provider)) cache = CacheConfig.Current.Providers[provider.ToLowerInvariant()].Provider;

                cache.WriteCache(scope, key, value, decay);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Store:{1} Application\r\nError:{2}", key, scope, ex.Message), ex);
            }
        }

        #endregion WriteProviderCache

        #region Privates

        private static TV readCache<TV>(string provider, string scope, string key, TimeSpan decay, TV nullValue) where TV : class 
		{
			try
			{
			    ICacheProvider cache = Current;
                if (!string.IsNullOrEmpty(provider)) cache = CacheConfig.Current.Providers[provider.ToLowerInvariant()].Provider;

                var value = cache.ReadCache<TV>(scope, key);
                if (value == null && nullValue!=null)
                {
                    cache.WriteCache(scope, key, nullValue, decay);
                    return nullValue;
                }
			    return value;
			}
			catch (Exception ex)
			{
                throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Scope:{1}\r\nError:{2}", key, scope, ex.Message), ex);
			}
        }

        private static TV readCache<TV>(string provider, string scope, string key, TimeSpan decay, Func<TV> action) where TV : class 
		{
			try
			{
                ICacheProvider cache = Current;
                if (!string.IsNullOrEmpty(provider)) cache = CacheConfig.Current.Providers[provider.ToLowerInvariant()].Provider;

                var value = cache.ReadCache<TV>(scope, key);
                if (value == null)
                {
                    value = action();
                    if(value!=null) cache.WriteCache(scope, key, value, decay);
                    return value;
                }
			    return value;
			}
			catch (Exception ex)
			{
                throw new Exception(string.Format("Error Accessing Cache Provider.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key, scope, ex.Message), ex);
			}
        }

        #endregion Privates

        #region RemoveAll

        public static void RemoveAllProvider(string provider, string scope)
        {
            try
            {
                ICacheProvider cache = Current;
                if (!string.IsNullOrEmpty(provider)) cache = CacheConfig.Current.Providers[provider.ToLowerInvariant()].Provider;
                cache.RemoveAllByScope(scope);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error Accessing Cache Provider.\r\nCache Store:{0}\r\nError:{1}", scope, ex.Message), ex);
            }
        }

        public static void RemoveAll(string scope)
        {
            RemoveAllProvider(null, scope);
        }

        #endregion RemoveAll
        
        #endregion
    }
}