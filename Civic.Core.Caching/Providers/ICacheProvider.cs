using System;
using Civic.Core.Configuration;

namespace Civic.Core.Caching.Providers
{
	public interface ICacheProvider
	{
        /// <summary>
        /// The configuration for this provider
        /// </summary>
        INamedElement Configuration { get; set; }

        TV ReadCache<TV>( string scope, string cacheKey) where TV : class;

        void WriteCache<TV>( string scope, string cacheKey, TV value, TimeSpan decay) where TV : class;

	    void RemoveAllByScope(string scope);
	}
}
