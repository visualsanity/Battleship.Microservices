﻿namespace Battleship.Infrastructure.Core.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Dapper;

    using Microsoft.Data.SqlClient;

    public class RepositoryCore 
    {
        #region Fields

        private readonly string databaseName;

        #endregion

        #region Constructors

        public RepositoryCore(string databaseName)
        {
            this.databaseName = databaseName;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Executes the specified parameters and stored procedure via dapper
        /// </summary>
        /// <typeparam name="T">Parameter to pass</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <param name="procedure">The procedure name.</param>
        /// <returns>Type of T</returns>
        /// <exception cref="System.Exception">Execution failed.</exception>
        protected IEnumerable<T> Execute<T>(Dictionary<string, object> parameters, [CallerMemberName] string procedure = "")
            where T : class
        {
            IEnumerable<T> result = null;
            SqlConnection connection = this.GetOpenConnection();
            try
            {
                using (connection)
                {
                    DynamicParameters dynamicParameters = new DynamicParameters();
                    if (parameters != null && parameters.Count > 0)
                        dynamicParameters = this.SetupDynamicParameters(parameters);

                    result = connection.Query<T>(this.SetName(procedure), dynamicParameters, commandType: CommandType.StoredProcedure, commandTimeout: connection.ConnectionTimeout);
                }
            }
            catch (SqlException exp)
            {
                throw new Exception(exp.Message, exp);
            }
            catch (Exception exp)
            {
                throw new Exception("Execution failed.", exp);
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        /// <summary>
        ///     Executes the specified parameters.
        /// </summary>
        /// <typeparam name="T">The parameters.</typeparam>
        /// <param name="procedure">The procedure.</param>
        /// <returns>IEnumerable of Type T</returns>
        protected IEnumerable<T> Execute<T>([CallerMemberName] string procedure = "")
            where T : class
        {
            SqlConnection connection = this.GetOpenConnection();
            try
            {
                using (connection) 
                    return connection.Query<T>(this.SetName(procedure), commandType: CommandType.StoredProcedure, commandTimeout: connection.ConnectionTimeout);
            }
            catch (SqlException exp)
            {
                throw new Exception(exp.Message, exp);
            }
            catch (Exception exp)
            {
                throw new Exception("Execution failed.", exp);
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        ///     Executes the specified parameters and stored procedure via dapper QueryAsync query and returns IEnumerable of type T
        /// </summary>
        /// <typeparam name="T">Parameter to pass</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <param name="procedure">The procedure name.</param>
        /// <returns>IEnumerable Type of T</returns>
        /// <exception cref="System.Exception">Execution failed.</exception>
        protected async Task<IEnumerable<T>> ExecuteAsync<T>(Dictionary<string, object> parameters, [CallerMemberName] string procedure = "")
            where T : class
        {
            SqlConnection connection = this.GetOpenConnection();
            IEnumerable<T> result;
            try
            {
                using (connection)
                {
                    DynamicParameters dynamicParameters = new DynamicParameters();
                    if (parameters != null && parameters.Count > 0)
                        dynamicParameters = this.SetupDynamicParameters(parameters);

                    result = await connection.QueryAsync<T>(this.SetName(procedure), dynamicParameters, commandType: CommandType.StoredProcedure, commandTimeout: connection.ConnectionTimeout);
                }
            }
            catch (SqlException exp)
            {
                throw new Exception(exp.Message, exp);
            }
            catch (Exception exp)
            {
                throw new Exception("Execution failed.", exp);
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        /// <summary>
        ///     Executes the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="procedure">The procedure.</param>
        /// <exception cref="System.Exception">Execution failed.</exception>
        /// <returns>Integer of affected rows</returns>
        protected int Execute(Dictionary<string, object> parameters, [CallerMemberName] string procedure = "")
        {
            SqlConnection connection = this.GetOpenConnection();
            try
            {
                using (connection)
                {
                    DynamicParameters dynamicParameters = new DynamicParameters();
                    if (parameters != null && parameters.Count > 0)
                        dynamicParameters = this.SetupDynamicParameters(parameters);

                    return connection.Execute(this.SetName(procedure), dynamicParameters, commandType: CommandType.StoredProcedure, commandTimeout: connection.ConnectionTimeout);
                }
            }
            catch (SqlException exp)
            {
                throw new Exception(exp.Message, exp);
            }
            catch (Exception exp)
            {
                throw new Exception("Execution failed.", exp);
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        ///     Executes the specified parameters asynchronously
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="procedure">The stored procedure from the CallerMemberName</param>
        /// <returns>IEnumerable list of type T</returns>
        protected async Task<int> ExecuteAsync(Dictionary<string, object> parameters, [CallerMemberName] string procedure = "")
        {
            SqlConnection connection = this.GetOpenConnection();
            int result;
            try
            {
                using (connection)
                {
                    DynamicParameters dynamicParameters = new DynamicParameters();
                    if (parameters != null && parameters.Count > 0)
                        dynamicParameters = this.SetupDynamicParameters(parameters);

                    string sql = this.SetName(procedure);

                    result = await connection.ExecuteAsync(sql, dynamicParameters, null, connection.ConnectionTimeout, CommandType.StoredProcedure);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Execution failed.", ex);
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        /// <summary>
        ///     Executes the specified parameters
        /// </summary>
        /// <typeparam name="T">Type of T</typeparam>
        /// <param name="procedure">The stored procedure from the CallerMemberName</param>
        /// <returns>IEnumerable list of type T</returns>
        protected async Task<IEnumerable<T>> ExecuteAsync<T>([CallerMemberName] string procedure = "")
            where T : class
        {
            SqlConnection connection = this.GetOpenConnection();
            IEnumerable<T> result;
            try
            {
                using (connection)
                {
                    string sql = this.SetName(procedure);
                    result = await connection.QueryAsync<T>(sql, null, null, connection.ConnectionTimeout, CommandType.StoredProcedure);
                }
            }
            catch (SqlException exp)
            {
                throw new Exception(exp.Message, exp);
            }
            catch (Exception exp)
            {
                throw new Exception("Execution failed.", exp);
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        /// <summary>
        ///     Executes and return single result.
        /// </summary>
        /// <typeparam name="T">Type T</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <param name="procedure">The procedure.</param>
        /// <returns>Type of T</returns>
        /// <exception cref="System.Exception">Execution failed.</exception>
        protected T ExecuteScalar<T>(Dictionary<string, object> parameters, [CallerMemberName] string procedure = "")
        {
            SqlConnection connection = this.GetOpenConnection();
            try
            {
                using (connection)
                {
                    DynamicParameters dynamicParameters = new DynamicParameters();
                    if (parameters != null && parameters.Count > 0)
                        dynamicParameters = this.SetupDynamicParameters(parameters);
                    return connection.Query<T>(this.SetName(procedure), dynamicParameters, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }
            }
            catch (SqlException exp)
            {
                throw new Exception(exp.Message, exp);
            }
            catch (Exception exp)
            {
                throw new Exception("Execution failed.", exp);
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        ///     Executes and return single result asynchronously
        /// </summary>
        /// <typeparam name="T">Type T</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <param name="procedure">The procedure.</param>
        /// <returns>Type of T</returns>
        /// <exception cref="System.Exception">Execution failed.</exception>
        protected async Task<T> ExecuteScalarAsync<T>(Dictionary<string, object> parameters, [CallerMemberName] string procedure = "")
        {
            SqlConnection connection = this.GetOpenConnection();
            T result;
            try
            {
                using (connection)
                {
                    DynamicParameters dynamicParameters = new DynamicParameters();
                    if (parameters != null && parameters.Count > 0)
                        dynamicParameters = this.SetupDynamicParameters(parameters);
                    result = (await connection.QueryAsync<T>(this.SetName(procedure), dynamicParameters, null, new int?(), CommandType.StoredProcedure)).FirstOrDefault();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Execution failed.", ex);
            }
            finally
            {
                connection.Close();
            }

            return result;
        }

        private SqlConnection GetOpenConnection()
        {
            SqlConnection sqlConnection = new SqlConnection(this.databaseName);
            sqlConnection.Open();
            return sqlConnection;
        }

        private string SetName(string procedureName)
        {
            return $"sp{procedureName}";
        }

        private DynamicParameters SetupDynamicParameters(Dictionary<string, object> parameters)
        {
            DynamicParameters dynamicParameters = null;

            if (parameters != null && parameters.Count > 0)
            {
                dynamicParameters = new DynamicParameters();
                foreach (KeyValuePair<string, object> entry in parameters)
                {
                    string key = $"@{entry.Key}";
                    object value = entry.Value;

                    dynamicParameters.Add(key, value);
                }
            }

            return dynamicParameters;
        }

        #endregion
    }
}