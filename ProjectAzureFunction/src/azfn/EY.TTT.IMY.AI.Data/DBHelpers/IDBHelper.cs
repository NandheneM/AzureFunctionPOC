using System.Data;
using static Dapper.SqlMapper;

namespace EY.TTT.IMY.AI.Data.DBHelpers
{
    public interface IDBHelper
    {
        Task<IEnumerable<T>> QueryAsyncWithRetry<T>(string sql, object param = null, CommandType? commandType = null);
        Task<T> QuerySingleOrDefaultAsyncWithRetry<T>(string sql, object param = null, CommandType? commandType = null);
        Task<T> QueryFirstAsyncWithRetry<T>(string sql, object param = null, CommandType? commandType = null);
        Task<int> ExecuteAsyncWithRetry(string sql, object param = null, CommandType? commandType = null);
        Task<T> QuerySingleAsyncWithRetry<T>(string sql, object param = null, CommandType? commandType = null);
        Task<GridReader> QueryMultipleAsyncWithRetry(string sql, object param = null, CommandType? commandType = null);
        Task<T> ExecuteScalarAsyncWithRetry<T>(string sql, object param = null, CommandType? commandType = null);
        Task<DataTable> ExecuteQueryToDataTableAsync(string sql, object param = null, CommandType? commandType = null);
    }
}
