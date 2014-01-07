using System;
using Civic.Core.Caching.Configuration;
using Civic.Core.Configuration;

namespace Civic.Core.Caching.Providers
{
	public interface ICacheProvider
	{
        /// <summary>
        /// The configuration for this provider
        /// </summary>
        CacheProviderElement Configuration { get; set; }

        TV ReadCache<TV>( string scope, string key) where TV : class;

        void WriteCache<TV>( string scope, string key, TV value, TimeSpan decay) where TV : class;

	    void RemoveAllByScope(string scope);
	}
}
