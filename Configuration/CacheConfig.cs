using System.Collections.Generic;
using Civic.Core.Caching.Providers;
using Civic.Core.Configuration;

namespace Civic.Core.Caching.Configuration
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
                if (_coreConfig == null) _coreConfig = CivicSection.Current;
                _current = new CacheConfig(_coreConfig.Children.ContainsKey(SectionName) ? _coreConfig.Children[SectionName] : null);
                return _current;
            }
		}
        private static CivicSection _coreConfig;
        private static CacheConfig _current;

        /// <summary>
        /// Gets or sets the typename for the skin for the header and footer
        /// </summary>
        public string DefaultProvider
        {
            get { return Attributes.ContainsKey(Constants.CONFIG_PROP_DEFAULTPROVIDER) ? Attributes[Constants.CONFIG_PROP_DEFAULTPROVIDER] : Constants.CONFIG_DEFAULTPROVIDER; }
            set { Attributes[Constants.CONFIG_DEFAULTPROVIDER] = value; }
        }

        /// <summary>
        /// Gets the collection of custom link type element collections.
        /// </summary>
        public Dictionary<string,CacheProviderElement> Providers
        {
            get
            {
                if (_providers != null) return _providers;
                if (Children.Count == 0)
                {
                    Children.Add("WebCacheProvider", new CacheProviderElement(new WebCacheProvider()));
                    Children.Add("SqlCacheProvider",
                        new CacheProviderElement(new SqlCacheProvider(),
                            new NamedConfigurationElement()
                            {
                                Attributes = new Dictionary<string, string> {{"connectionStringName", "CIVIC"}}
                            }));
                    Children.Add("TokenCacheProvider",
                        new CacheProviderElement(new SqlCacheProvider(),
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