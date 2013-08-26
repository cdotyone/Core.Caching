using System;

namespace Civic.Core.Caching.Providers
{
	public class NoCacheProvider : ICacheProvider
    {
		#region Methods

		public TV ReadCache<TV>(string key, TV nullValue, CacheStore cacheStore)
        {
            return nullValue;
        }

		public void WriteCache<TV>(string key, TV value, CacheStore cacheStore)
        {
        }

		public void WriteCache<TV>(string key, TV value, TimeSpan decay, CacheStore cacheStore)
		{
		}

		#endregion
    }
}