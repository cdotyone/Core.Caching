using System.Configuration;
using Civic.Core.Caching.Providers;
using Civic.Core.Configuration;

namespace Civic.Core.Caching.Configuration
{
	public class CacheProviderElement : NamedConfigurationElement
    {
	    private ICacheProvider _provider;

		/// <summary>
		/// The "assembly" name given of the provider.
		/// 
		/// In the form: assembly="Civic.Core.Configuration, Version=1.0.0.0, Culture=neutral"
		/// </summary>
		[ConfigurationProperty(Constants.ASSEMBLY, IsRequired = true)]
		public string Assembly
		{
			get
			{
				if (string.IsNullOrEmpty(_assembly)) _assembly = Attributes[Constants.ASSEMBLY];
				return _assembly;
			}
			set { _assembly = value; }
		}
		private string _assembly;

		/// <summary>
		/// The "type" name of the provider.
		/// 
		/// In the form of type="Civic.Core.Caching.Providers.WebCacheProvider"
		/// </summary>
		[ConfigurationProperty(Constants.TYPE, IsRequired = true)]
		public string Type
		{
			get
			{
				if (string.IsNullOrEmpty(_typeName)) _typeName = Attributes[Constants.TYPE];
				return _typeName;
			}
			set { _typeName = value; }
		}
		private string _typeName;

		/// <summary>
		/// Trys to dynamically create the provider and then returns the provider.
		/// </summary>
		public ICacheProvider Provider
		{
			get {
                if(_provider!=null) return _provider;

			    _provider = (ICacheProvider) DynamicInstance.CreateInstance(Assembly, Type);
			    _provider.Configuration = this;

                return _provider;
			}
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public CacheProviderElement(INamedElement config)
		{
		    Attributes = config.Attributes;
		    Children = config.Children;
        }

        /// <summary>
        /// Creates a CacheProviderElement from a ICacheProvider
        /// </summary>
        /// <param name="provider">the provider to create the configuration entry from</param>
        public CacheProviderElement(ICacheProvider provider)
        {
            _provider = provider;
            if (Name.EndsWith("Provider")) Name = Name.Substring(0, Name.Length - 8);
        }

        /// <summary>
        /// Creates a CacheProviderElement from a ICacheProvider
        /// </summary>
        /// <param name="provider">the provider to create the configuration entry from</param>
        /// <param name="config">the configuration for the provider</param>
        public CacheProviderElement(ICacheProvider provider, INamedElement config) : this(config)
		{
			_provider = provider;
            if (Name.EndsWith("Provider")) Name = Name.Substring(0, Name.Length - 8);
		}
	}
}