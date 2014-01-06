using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace Civic.Core.Caching.Providers
{
    public class SqlProvider : ICacheProvider
    {

        public void WriteCache<TV>(string scope, string key, TV value, TimeSpan decay) where TV : class 
        {
            try
            {
                if (value != null)
                {
                    string output = JsonConvert.SerializeObject(value);
                    saveCachetoDB(scope, key, output, decay);
                }
                else
                {
                    saveCachetoDB(scope, key, null, TimeSpan.MinValue);                    
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

        private static void saveCachetoDB(string scope, string cacheKey, string value, TimeSpan decay)
        {
            var constring = ConfigurationManager.ConnectionStrings["Civic"].ConnectionString;

            var database = new SqlConnection(constring);
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
                    command.CommandText = "[dbo].[usp_SystemCacheRemove]";
                }
                else
                {
                    command.CommandText = "[dbo].[usp_SystemCacheSave]";

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

        private static string readCachefromDB(string scope, string cacheKey)
        {
            var constring = ConfigurationManager.ConnectionStrings["Civic"].ConnectionString;

            var database = new SqlConnection(constring);
            using (var command = database.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[usp_SystemCacheGet]";

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

                string value;
                using (IDataReader dataReader = command.ExecuteReader())
                {
                    value = dataReader["Value"].ToString();
                }

                return value;
            }
        }
    }
}