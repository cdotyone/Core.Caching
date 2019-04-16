using System.Globalization;
using System.Resources;

namespace Stack.Core.Caching
{
    internal sealed class SR
    {
        #region Exceptions

        internal const string CACHE_MANAGER_READ_CACHE_KEY_NULL = "CacheManager_Read_Cache_Key_Null";
        internal const string CACHE_MANAGER_READ_CACHE_STORE_NULL = "CacheManager_Read_Cache_Store_Null";
        internal const string CACHE_MANAGER_WRITE_CACHE_KEY_NULL = "CacheManager_Write_Cache_Key_Null";
        internal const string CACHE_MANAGER_WRITE_CACHE_STORE_NULL = "CacheManager_Write_Cache_Store_Null";

        #endregion

        #region Members

        private readonly ResourceManager _resources;
        private static SR _loader;

        #endregion

        #region Constructors

        internal SR()
        {
			_resources = new ResourceManager("Core.Caching.Properties.Resources", GetType().Assembly);
			_loader = this;
        }

        #endregion

        #region Properties

        public static ResourceManager Resources
        {
            get { return GetLoader()._resources; }
        }

        private static CultureInfo Culture
        {
            get { return null; }
        }

        #endregion

        #region Methods

        private static SR GetLoader()
        {
            return _loader;
        }

        public static object GetObject(string name)
        {
            SR theloader = GetLoader();
            if (theloader == null)
                return null;

            return theloader._resources.GetObject(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
			SR theloader = GetLoader();
            if (theloader == null)
                return null;

            string format = theloader._resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
                return format;

            for (int i = 0; i < args.Length; i++)
            {
                var str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                    args[i] = str2.Substring(0, 0x3fd) + "...";
            }

        	if (format != null) return string.Format(CultureInfo.CurrentCulture, format, args);
        	return null;
        }

        #endregion
    }
}
