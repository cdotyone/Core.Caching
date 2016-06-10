using System;
using Civic.Core.Configuration;

namespace Civic.Core.Caching.Providers
{
	public class NoCacheProvider : ICacheProvider
    {
        /// <summary>
        /// The configuration for this provider
        /// </summary>
        public INamedElement Configuration { get; set; }

		#region Methods

        public TV ReadCache<TV>(string scope, string cacheKey) where TV : class 
	    {
	        return null;
	    }

        public void WriteCache<TV>(string scope, string cacheKey, TV value, TimeSpan decay) where TV : class 
		{
		}

	    public void RemoveAllByScope(string scope)
	    {
	    }

	    #endregion
    }
}