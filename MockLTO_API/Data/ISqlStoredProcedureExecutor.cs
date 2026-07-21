using Microsoft.Data.SqlClient;

namespace MockLTO_API.Data;

public interface ISqlStoredProcedureExecutor
{
    Task<int> ExecuteAsync(string storedProcedure, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default);
    Task<T?> ExecuteScalarAsync<T>(string storedProcedure, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default);
    Task<T?> QuerySingleOrDefaultAsync<T>(string storedProcedure, Func<SqlDataReader, T> map, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> QueryAsync<T>(string storedProcedure, Func<SqlDataReader, T> map, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default);
}
