using System;

namespace Civic.Core.Caching.Providers
{
	public class NoCacheProvider : ICacheProvider
    {
		#region Methods

        public TV ReadCache<TV>(string key, CacheStore cacheStore) where TV : class 
	    {
	        return null;
	    }

        public TV ReadCache<TV>(string key, TV nullValue, CacheStore cacheStore) where TV : class 
        {
            return nullValue;
        }

        public void WriteCache<TV>(string key, TV value, CacheStore cacheStore) where TV : class 
        {
        }

        public void WriteCache<TV>(string key, TV value, TimeSpan decay, CacheStore cacheStore) where TV : class 
		{
		}

		#endregion
    }
}