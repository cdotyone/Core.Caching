using System.Configuration;
using Civic.Core.Caching.Providers;
using Civic.Core.Configuration;

namespace Civic.Core.Caching.Configuration
{

	public class CacheConfigurationSection : ConfigurationSection
	{
      
        #region Properties

		/// <summary>
		/// Name of the assembly for the cache provider
		/// </summary>
		[ConfigurationProperty(Constants.ASSEMBLY, IsRequired = false)]
		public string Assembly
		{
			get
			{
                var name = (string)base[Constants.ASSEMBLY];
				return string.IsNullOrEmpty(name) ? GetType().Assembly.FullName : name;
			}
		}

		/// <summary>
		/// Name of the Type for the cache provider
		/// </summary>
        [ConfigurationProperty(Constants.TYPE, IsRequired = false)]
		public string Type
		{
			get
			{
                var name = (string)base[Constants.TYPE];
				return string.IsNullOrEmpty(name) ? typeof(WebCacheProvider).FullName : name;
			}
		}

        /// <summary>
        /// The current configuration for caching module
        /// </summary>
		public static CacheConfigurationSection Current
		{
			get
			{
                if (_coreConfig == null) _coreConfig = (CacheConfigurationSection)ConfigurationManager.GetSection(SectionName);
                if (_coreConfig == null || _coreConfig.Providers.Count == 0)
                {
                    if (_coreConfig == null)
                        _coreConfig = new CacheConfigurationSection();

                    _coreConfig.Providers.Add(new CacheProviderElement(new WebCacheProvider()));
                }

                return _coreConfig;
            }
		}
        private static CacheConfigurationSection _coreConfig;

        /// <summary>
        /// Gets or sets the typename for the skin for the header and footer
        /// </summary>
        [ConfigurationProperty(Constants.CONFIG_PROP_DEFAULTPROVIDER, IsRequired = false, DefaultValue = Constants.CONFIG_DEFAULTPROVIDER)]
        public string DefaultProvider
        {
            get { return (string)this[Constants.CONFIG_PROP_DEFAULTPROVIDER]; }
            set { this[Constants.CONFIG_PROP_DEFAULTPROVIDER] = value; }
        }

        /// <summary>
        /// Gets the collection of custom link type element collections.
        /// </summary>
        [ConfigurationProperty("providers", IsDefaultCollection = true)]
        public NamedElementCollection<CacheProviderElement> Providers
        {
            get
            {
                if (_providers == null) _providers = (NamedElementCollection<CacheProviderElement>)base["providers"];
                if (_providers == null || _providers.Count == 0) _providers = null;
                return _providers ?? (_providers = new NamedElementCollection<CacheProviderElement>
                    {
                        new CacheProviderElement(new WebCacheProvider()){Name = "WebCacheProvider"},
                        new CacheProviderElement(new NoCacheProvider()){Name = "NoCacheProvider"}
                    });
            }
        }
        private NamedElementCollection<CacheProviderElement> _providers;

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