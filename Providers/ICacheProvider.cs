using System;

namespace Civic.Core.Caching.Providers
{
	public interface ICacheProvider
	{

        TV ReadCache<TV>( string scope, string key) where TV : class;

        void WriteCache<TV>( string scope, string key, TV value, TimeSpan decay) where TV : class;

	    void RemoveAllByScope(string scope);

	}
}
