using Dapper;
using EY.TTT.IMY.AI.Domain.Resilience;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.TTT.IMY.AI.Data.DBHelpers
{
    public class DBHelper : IDBHelper
    {
        private readonly IDbConnection _dbConnection;
        private readonly IResiliencePolicy _dbResiliencePolicy;
        public DBHelper(IDbConnection dbConnection, IResiliencePolicy dbResiliencePolicy)
        {
            _dbConnection = dbConnection;
            _dbResiliencePolicy = dbResiliencePolicy;
        }

        public async Task<int> ExecuteAsyncWithRetry(string sql, object param = null, CommandType? commandType = null)
        {
            return await _dbResiliencePolicy.ExecuteAsync(async () => await _dbConnection.ExecuteAsync(sql, param, null, null, commandType));
        }

        public async Task<T> ExecuteScalarAsyncWithRetry<T>(string sql, object param = null, CommandType? commandType = null)
        {
            return await _dbResiliencePolicy.ExecuteAsync(async () => await _dbConnection.ExecuteScalarAsync<T>(sql, param, null, null, commandType));
        }

        public async Task<IEnumerable<T>> QueryAsyncWithRetry<T>(string sql, object param = null, CommandType? commandType = null)
        {
            return await _dbResiliencePolicy.ExecuteAsync(async () => await _dbConnection.QueryAsync<T>(sql, param, null, null, commandType));
        }
        public async Task<T> QueryFirstAsyncWithRetry<T>(string sql, object param = null, CommandType? commandType = null)
        {
            return await _dbResiliencePolicy.ExecuteAsync(async () => await _dbConnection.QueryFirstAsync<T>(sql, param, null, null, commandType));
        }

        public async Task<SqlMapper.GridReader> QueryMultipleAsyncWithRetry(string sql, object param = null, CommandType? commandType = null)
        {
            return await _dbResiliencePolicy.ExecuteAsync(async () => await _dbConnection.QueryMultipleAsync(sql, param, null, null, commandType));
        }

        public async Task<T> QuerySingleAsyncWithRetry<T>(string sql, object param = null, CommandType? commandType = null)
        {
            return await _dbResiliencePolicy.ExecuteAsync(async () => await _dbConnection.QuerySingleAsync<T>(sql, param, null, null, commandType));
        }

        public async Task<T> QuerySingleOrDefaultAsyncWithRetry<T>(string sql, object param = null, CommandType? commandType = null)
        {
            return await _dbResiliencePolicy.ExecuteAsync(async () => await _dbConnection.QuerySingleOrDefaultAsync<T>(sql, param, null, null, commandType));
        }

        public async Task<DataTable> ExecuteQueryToDataTableAsync(string sql, object param = null, CommandType? commandType = null)
        {
            return await _dbResiliencePolicy.ExecuteAsync(async () =>
            {
                var dataTable = new DataTable();
                using (var reader = await _dbConnection.ExecuteReaderAsync(sql, param, commandType: commandType))
                {
                    dataTable.Load(reader);
                }
                return dataTable;
            });
        }
    }
}
