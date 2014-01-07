using System.Configuration;
using Civic.Core.Caching.Providers;
using Civic.Core.Configuration;
using Civic.Core.Configuration.Providers;

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
		public string AssemblyName
		{
			get
			{
				if (string.IsNullOrEmpty(_assembly)) _assembly = (string) this[Constants.ASSEMBLY];
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
		public string TypeName
		{
			get
			{
				if (string.IsNullOrEmpty(_typeName)) _typeName = (string)this[Constants.TYPE];
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
				return _provider ?? (_provider = (ICacheProvider) DynamicInstance.CreateInstance(AssemblyName, TypeName));
			}
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public CacheProviderElement()
		{
		}

		/// <summary>
		/// Creates a CacheProviderElement from a ICacheProvider
		/// </summary>
		/// <param name="provider">the provider to create the configuration entry from</param>
        public CacheProviderElement(ICacheProvider provider)
		{
			_provider = provider;

			var typeConfigFile = typeof(ConfigFileProvider);
			Name = typeConfigFile.Name;
			AssemblyName = typeConfigFile.Assembly.FullName;
			TypeName = typeConfigFile.FullName;

			if (Name.EndsWith("Provider")) Name = Name.Substring(0, Name.Length - 8);
		}
	}
}