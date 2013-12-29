using System;

namespace Civic.Core.Caching.Providers
{
	public interface ICacheProvider
	{

        TV ReadCache<TV>(string key, CacheStore cacheStore) where TV : class;

        TV ReadCache<TV>(string key, TV nullValue, CacheStore cacheStore) where TV : class;

        void WriteCache<TV>(string key, TV value, CacheStore cacheStore) where TV : class;

        void WriteCache<TV>(string key, TV value, TimeSpan decay, CacheStore cacheStore) where TV : class;

	}
}
