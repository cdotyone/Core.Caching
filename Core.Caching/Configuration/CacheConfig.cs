using System.Collections.Generic;
using Stack.Core.Caching.Providers;
using Stack.Core.Configuration;

namespace Stack.Core.Caching.Configuration
{

	public class CacheConfig : NamedConfigurationElement
    {

        #region Properties

        public CacheConfig(INamedElement element)
        {
            if (element == null) element = new NamedConfigurationElement() { Name = SectionName };
            Children = element.Children;
            Attributes = element.Attributes;
            Name = element.Name;
        }

        /// <summary>
        /// The current configuration for caching module
        /// </summary>
        public static CacheConfig Current
		{
			get
			{
                if (_current != null) return _current;

                if (_coreConfig == null) _coreConfig = CivicSection.Current;
                _current = new CacheConfig(_coreConfig.Children.ContainsKey(SectionName) ? _coreConfig.Children[SectionName] : null);
                return _current;
            }
		}
        private static CivicSection _coreConfig;
        private static CacheConfig _current;

        /// <summary>
        /// Gets or sets the name of the default cache provider
        /// </summary>
        public string DefaultProvider
        {
            get { return Attributes.ContainsKey(Constants.CONFIG_PROP_DEFAULTPROVIDER) ? Attributes[Constants.CONFIG_PROP_DEFAULTPROVIDER] : Constants.CONFIG_DEFAULTPROVIDER; }
            set { Attributes[Constants.CONFIG_DEFAULTPROVIDER] = value; }
        }

        /// <summary>
        /// Gets the collection cache providers
        /// </summary>
        public Dictionary<string,CacheProviderElement> Providers
        {
            get
            {
                if (_providers != null) return _providers;
                if (Children.Count == 0)
                {
                    #if NETFULL
                    Children.Add("WebCacheProvider", new CacheProviderElement(new WebCacheProvider()));
                    #endif
                    Children.Add("SqlCacheProvider",
                        new CacheProviderElement(new SqlCacheProvider(),
                            new NamedConfigurationElement()
                            {
                                Attributes = new Dictionary<string, string> {{"connectionStringName", "CIVIC"}}
                            }));
                    Children.Add("TokenCacheProvider",
                        new CacheProviderElement(new MultiCacheProvider(),
                            new NamedConfigurationElement()
                            {
                                Attributes = new Dictionary<string, string> { { "providers", "SqlCacheProvider,WebCacheProvider" } }
                            }));
                    Children.Add("NoCacheProvider", new CacheProviderElement(new NoCacheProvider()));
                }

                _providers = new Dictionary<string, CacheProviderElement>();
                foreach (var element in Children)
                {
                    _providers[element.Key.ToLowerInvariant()] = new CacheProviderElement(element.Value);
                }

                return _providers;
            }
        }
        private Dictionary<string, CacheProviderElement> _providers;

		#endregion

		#region Methods

		/// <summary>
		/// The name of the configuration section.
		/// </summary>
		public static string SectionName
		{
			get { return Constants.CORE_CACHE_SECTION; }
		}

		#endregion
	}
}