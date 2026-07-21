using Microsoft.Data.SqlClient;

namespace MockLTO_API.Data;

public interface ISqlQueryExecutor
{
    Task<T?> ExecuteScalarAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        Func<SqlDataReader, T> map,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default);

    Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        Func<SqlDataReader, T> map,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default);
}
