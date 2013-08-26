using System;

namespace Civic.Core.Caching.Providers
{
	public interface ICacheProvider
	{
		
		TV ReadCache<TV>(string key, TV nullValue, CacheStore cacheStore);

		void WriteCache<TV>(string key, TV value, CacheStore cacheStore);

		void WriteCache<TV>(string key, TV value, TimeSpan decay, CacheStore cacheStore);

	}
}
