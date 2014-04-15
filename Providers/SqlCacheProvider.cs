using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Civic.Core.Caching.Configuration;
using Newtonsoft.Json;

namespace Civic.Core.Caching.Providers
{
    public class SqlCacheProvider : ICacheProvider
    {
        /// <summary>
        /// The configuration for this provider
        /// </summary>
        public CacheProviderElement Configuration { get; set; }

        public void WriteCache<TV>(string scope, string key, TV value, TimeSpan decay) where TV : class 
        {
            try
            {
                if (value == null)
                {
                    saveCachetoDB(scope, key, null, TimeSpan.MinValue);
                }
                else
                {
                    string output = JsonConvert.SerializeObject(value, new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                    saveCachetoDB(scope, key, output, decay);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key,
                        scope, ex.Message));
            }
        }

        public void RemoveAllByScope(string scope)
        {
            saveCachetoDB(scope, null, null, TimeSpan.MinValue);
        }

        public TV ReadCache<TV>(string scope, string key) where TV : class
        {
            try
            {
                string value = readCachefromDB(scope, key);
                if (!string.IsNullOrEmpty(value)) 
                    return JsonConvert.DeserializeObject<TV>(value);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", key,
                        scope, ex.Message));
            }
            return null;
        }

        private string ConnectionString
        {
            get
            {
                string connName = Configuration.Attributes.ContainsKey(Constants.CONFIG_PROP_CONNECTIONNAME) ? 
                                      Configuration.Attributes[Constants.CONFIG_PROP_CONNECTIONNAME] : 
                                      Constants.CONFIG_DEFAULT_CONNECTIONNAME;
                var constring = ConfigurationManager.ConnectionStrings[connName].ConnectionString;
                return constring;
            }
        }

        private void saveCachetoDB(string scope, string cacheKey, string value, TimeSpan decay)
        {
            var database = new SqlConnection(ConnectionString);
            database.Open();

            using (var command = database.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;

                var param = command.CreateParameter();
                param.Direction = ParameterDirection.Input;
                param.ParameterName = "@scope";
                param.Value = scope;
                param.DbType = DbType.String;
                command.Parameters.Add(param);

                param = command.CreateParameter();
                param.Direction = ParameterDirection.Input;
                param.ParameterName = "@cacheKey";
                param.Value = cacheKey;
                param.DbType = DbType.String;
                command.Parameters.Add(param);

                if (string.IsNullOrEmpty(value))
                {
                    command.CommandText = "[civic].[usp_SystemCacheRemove]";
                }
                else
                {
                    command.CommandText = "[civic].[usp_SystemCacheSave]";

                    param = command.CreateParameter();
                    param.Direction = ParameterDirection.Input;
                    param.ParameterName = "@value";
                    param.Value = value;
                    param.DbType = DbType.String;
                    command.Parameters.Add(param);

                    param = command.CreateParameter();
                    param.Direction = ParameterDirection.Input;
                    param.ParameterName = "@timeExpire";
                    param.Value = DateTime.UtcNow.Add(decay);
                    param.DbType = DbType.DateTime;
                    command.Parameters.Add(param);                    
                }

                command.ExecuteNonQuery();
            }
        }

        private string readCachefromDB(string scope, string cacheKey)
        {
            var database = new SqlConnection(ConnectionString);
            database.Open();

            using (var command = database.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[civic].[usp_SystemCacheGet]";

                var param = command.CreateParameter();
                param.Direction = ParameterDirection.Input;
                param.ParameterName = "@scope";
                param.Value = scope;
                param.DbType = DbType.String;
                command.Parameters.Add(param);

                param = command.CreateParameter();
                param.Direction = ParameterDirection.Input;
                param.ParameterName = "@cacheKey";
                param.Value = cacheKey;
                param.DbType = DbType.String;
                command.Parameters.Add(param);

                using (IDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        return dataReader["Value"].ToString();
                    }
                }

                return null;
            }
        }
    }
}