﻿using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Core.Configuration;
using Core.Logging;
using Newtonsoft.Json;

namespace Core.Caching.Providers
{
    public class SqlCacheProvider : ICacheProvider
    {
        /// <summary>
        /// The configuration for this provider
        /// </summary>
        public INamedElement Configuration { get; set; }

        public void WriteCache<TV>(string scope, string cacheKey, TV value, TimeSpan decay) where TV : class 
        {
            try
            {
                if (value==null || (string.IsNullOrEmpty(value.ToString())))
                {
                    saveCachetoDB(scope, cacheKey, null, TimeSpan.MinValue);
                }
                else
                {
                    string output = JsonConvert.SerializeObject(value, new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                    saveCachetoDB(scope, cacheKey, output, decay);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", cacheKey,
                        scope, ex.Message));
            }
        }

        public void RemoveAllByScope(string scope)
        {
            saveCachetoDB(scope, null, null, TimeSpan.MinValue);
        }

        public TV ReadCache<TV>(string scope, string cacheKey) where TV : class
        {
            try
            {
                string value = readCachefromDB(scope, cacheKey);
                if (!string.IsNullOrEmpty(value)) 
                    return JsonConvert.DeserializeObject<TV>(value);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format("Error Accessing Cache Manager.\r\nKey:{0}\r\nCache Store:{1}\r\nError:{2}", cacheKey,
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
                connName = DataConfig.Current.GetConnectionName(connName);
                var constring = ConfigurationManager.ConnectionStrings[connName].ConnectionString;
                return constring;
            }
        }

        private void saveCachetoDB(string scope, string cacheKey, string value, TimeSpan decay)
        {
            using (var database = new SqlConnection(ConnectionString))
            {
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
                        Logger.LogTrace(LoggingBoundaries.DataLayer, "SqlCacheProvider - Write - Scope {0} Key {1} - Value Null - Remove", scope, cacheKey);
                        command.CommandText = "[core].[usp_CacheRemove]";
                    }
                    else
                    {
                        Logger.LogTrace(LoggingBoundaries.DataLayer, "SqlCacheProvider - Write - Scope {0} Key {1} - Save - {2}", scope, cacheKey, value);

                        command.CommandText = "[core].[usp_CacheSave]";

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

                    try
                    {
                        database.Open();
                        command.ExecuteNonQuery();
                    }
                    catch(Exception ex)
                    {
                        Logger.LogError(LoggingBoundaries.DataLayer, "SqlCacheProvider - Error Writing - Scope {0} Key {1} - {2}", scope, cacheKey, ex.Message);
                        throw;
                    }
                }
            }
        }

        private string readCachefromDB(string scope, string cacheKey)
        {
            using (var database = new SqlConnection(ConnectionString))
            {
                using (var command = database.CreateCommand())
                {
                    Logger.LogTrace(LoggingBoundaries.DataLayer, "SqlCacheProvider - Read - Scope {0} Key {1} - Before", scope, cacheKey);


                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[core].[usp_CacheGet]";

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

                    database.Open();
                    using (IDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var expire = DateTime.Parse(dataReader["TimeExpire"].ToString());
                            if (expire < DateTime.UtcNow)
                            {
                                // clear it
                                Logger.LogTrace(LoggingBoundaries.DataLayer, "SqlCacheProvider - Read - Scope {0} Key {0} - Expired", scope, cacheKey);
                                saveCachetoDB(scope, cacheKey, null, TimeSpan.Zero);
                                return null;
                            }

                            Logger.LogTrace(LoggingBoundaries.DataLayer, "SqlCacheProvider - Read - Scope {0} Key {0} - Found", scope, cacheKey);
                            return dataReader["Value"].ToString();
                        }

                        Logger.LogTrace(LoggingBoundaries.DataLayer, "SqlCacheProvider - Read - Scope {0} Key {1} - Not Found", scope, cacheKey);
                    }

                    return null;
                }
            }
        }
    }
}