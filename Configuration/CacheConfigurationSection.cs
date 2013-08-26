using System.Configuration;
using Civic.Core.Caching.Providers;
using Civic.Core.Configuration;

namespace Civic.Core.Caching.Configuration
{

	public class CacheConfigurationSection : ConfigurationSection
	{
		#region Members

		private const string SECTION_NAME = "coreFrameworkCaching";
		private const string ASSEMBLY = "assembly";
		private const string TYPE = "type";

		#endregion

		#region Properties

		/// <summary>
		/// Name of the assembly for the cache provider
		/// </summary>
		[ConfigurationProperty(ASSEMBLY, IsRequired = false)]
		public string Assembly
		{
			get
			{
				var name = (string)base[ASSEMBLY];
				return string.IsNullOrEmpty(name) ? GetType().Assembly.FullName : name;
			}
		}

		/// <summary>
		/// Name of the Type for the cache provider
		/// </summary>
		[ConfigurationProperty(TYPE, IsRequired = false)]
		public string Type
		{
			get
			{
				var name = (string)base[TYPE];
				return string.IsNullOrEmpty(name) ? typeof(WebCacheProvider).FullName : name;
			}
		}

		public static CacheConfigurationSection Current
		{
			get
			{
				return ConfigurationFactory.ReadConfigSection<CacheConfigurationSection>(SectionName) ?? new CacheConfigurationSection();
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// The name of the configuration section.
		/// </summary>
		public static string SectionName
		{
			get { return SECTION_NAME; }
		}

		#endregion
	}
}