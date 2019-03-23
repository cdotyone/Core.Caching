using System;
using System.Collections.Generic;
using System.Linq;
using Core.Configuration;
using Core.Logging;

namespace Core.Caching.Providers
{
	public class MultiCacheProvider : ICacheProvider
	{
        private static string[] _providers;

        /// <summary>
        /// The configuration for this provider
        /// </summary>
        public INamedElement Configuration { get; set; }

	    public string[] Providers
	    {
	        get
	        {
	            if (_providers != null) return _providers;

	            if (Configuration.Attributes.ContainsKey(Constants.CONFIG_PROP_PROVIDERS))
	            {
                    Logger.LogTrace(LoggingBoundaries.DataLayer, "MultiCacheProvider - Providers - {0}", Configuration.Attributes[Constants.CONFIG_PROP_PROVIDERS]);

                    _providers = Configuration.Attributes[Constants.CONFIG_PROP_PROVIDERS].Split(',');
	            }
	            else throw new Exception("No providers configured for MultiCacheProvider");

	            return _providers;
	        }
	    }

        public void WriteCache<TV>(string scope, string cacheKey, TV value, TimeSpan decay) where TV : class 
		{
            foreach (var provider in Providers)
            {
                try
                {
                    Logger.LogTrace(LoggingBoundaries.DataLayer, "MultiCacheProvider - Writing {0} Scope {1} Key {2}", provider, scope, cacheKey);

                    CacheManager.WriteProviderCache(provider, scope, cacheKey, value, decay);
                }
                catch (Exception ex)
                {
                    Logger.LogTrace(LoggingBoundaries.DataLayer, "MultiCacheProvider - Exception From {0} Scope {1} Key {2}", provider, scope, cacheKey);
                    Logger.HandleException(LoggingBoundaries.DataLayer, ex);
                }
            }
		}

	    public void RemoveAllByScope(string scope)
	    {
            foreach (var provider in Providers)
            {
                try
                {
                    Logger.LogTrace(LoggingBoundaries.DataLayer, "MultiCacheProvider - RemoveAllByScope {0} Scope {1}", provider, scope);

                    CacheManager.RemoveAllProvider(provider, scope);
                }
                catch (Exception ex)
                {
                    Logger.LogTrace(LoggingBoundaries.DataLayer, "MultiCacheProvider - Exception From {0} Scope {1} Key {2}", provider, scope);
                    Logger.HandleException(LoggingBoundaries.DataLayer, ex);
                }
            }
        }

	    public TV ReadCache<TV>(string scope, string cacheKey) where TV : class
	    {
            var failedToFind = new List<string>();

            foreach (var provider in Providers.ToArray().Reverse())
	        {
                try
                {
                    Logger.LogTrace(LoggingBoundaries.DataLayer, "MultiCacheProvider - Reading From {0} Scope {1} Key {2}", provider, scope, cacheKey);

                    var val = CacheManager.ReadProviderCache<TV>(provider, scope, cacheKey, null);
                    if (val == null)
                    {
                        Logger.LogTrace(LoggingBoundaries.DataLayer,
                            "MultiCacheProvider - Not Found From {0} Scope {1} Key {2}", provider, scope, cacheKey);
                        failedToFind.Add(provider);
                    }
                    else
                    {
                        foreach (var saveProvider in failedToFind)
                        {
                            Logger.LogTrace(LoggingBoundaries.DataLayer,
                                "MultiCacheProvider - Updating {0} Scope {1} Key {2}", provider, scope, cacheKey);
                            CacheManager.WriteProviderCache(saveProvider, scope, cacheKey, val);
                        }

                        return val;
                    }
                }
                catch(Exception ex)
                {
                    Logger.LogTrace(LoggingBoundaries.DataLayer, "MultiCacheProvider - Exception From {0} Scope {1} Key {2}", provider, scope, cacheKey);
                    Logger.HandleException(LoggingBoundaries.DataLayer, ex);
                }
	        }

	        return null;
	    }


	}
}
