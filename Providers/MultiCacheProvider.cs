using System;
using System.Collections.Generic;
using Civic.Core.Caching.Configuration;

namespace Civic.Core.Caching.Providers
{
	public class MultiCacheProvider : ICacheProvider
	{
        private static string[] _providers;

        /// <summary>
        /// The configuration for this provider
        /// </summary>
        public CacheProviderElement Configuration { get; set; }

	    public string[] Providers
	    {
	        get
	        {
	            if (_providers != null) return _providers;

	            if (Configuration.Attributes.ContainsKey(Constants.CONFIG_PROP_PROVIDERS))
	                _providers = Configuration.Attributes[Constants.CONFIG_PROP_PROVIDERS].Split(',');
                else throw new Exception("No providers configured for MultiCacheProvider");

	            return _providers;
	        }
	    }

        public void WriteCache<TV>(string scope, string key, TV value, TimeSpan decay) where TV : class 
		{
            foreach (var provider in Providers)
            {
                try
                {
                    CacheManager.WriteProviderCache(provider, scope, key, value, decay);
                }
                catch
                {
                }
            }
		}

	    public void RemoveAllByScope(string scope)
	    {
            foreach (var provider in Providers)
            {
                try
                {
                    CacheManager.RemoveAllProvider(provider, scope);
                }
                catch
                {
                }
            }
	    }

	    public TV ReadCache<TV>(string scope, string key) where TV : class
	    {
            var failedToFind = new List<string>();

            foreach (var provider in Providers)
	        {
                try
                {
                    var val = CacheManager.ReadProviderCache<TV>(provider, scope, key, null);
                    if (val == null)
                    {
                        failedToFind.Add(provider);
                    }

                    foreach (var saveProvider in failedToFind)
                    {
                        CacheManager.WriteProviderCache(saveProvider, scope, key, val);
                    }

                    return val;
                }
                catch
                {
                }
	        }

	        return null;
	    }


	}
}
