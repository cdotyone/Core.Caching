using System;

namespace Civic.Core.Caching.Providers
{
	public class NoCacheProvider : ICacheProvider
    {
		#region Methods

        public TV ReadCache<TV>(string scope, string key) where TV : class 
	    {
	        return null;
	    }

        public void WriteCache<TV>(string scope, string key, TV value, TimeSpan decay) where TV : class 
		{
		}

	    public void RemoveAllByScope(string scope)
	    {
	    }

	    #endregion
    }
}