using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using MockLTO_API.Configuration;

namespace MockLTO_API.Data;

public sealed class SqlStoredProcedureExecutor : ISqlStoredProcedureExecutor
{
    private readonly string _connectionString;
    private readonly int _commandTimeoutSeconds;

    public SqlStoredProcedureExecutor(IApplicationConfig configuration)
    {
        _connectionString = configuration.SqlConnectionString;
        _commandTimeoutSeconds = configuration.SqlCommandTimeoutSeconds;
    }

    public async Task<int> ExecuteAsync(string storedProcedure, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await using var command = CreateCommand(connection, storedProcedure, parameters);
        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<T?> ExecuteScalarAsync<T>(string storedProcedure, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await using var command = CreateCommand(connection, storedProcedure, parameters);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);

        if (result is null or DBNull)
        {
            return default;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        if (targetType == typeof(Guid))
        {
            return (T)(object)(result is Guid guid ? guid : Guid.Parse(result.ToString()!));
        }

        if (targetType.IsEnum)
        {
            return (T)Enum.ToObject(targetType, result);
        }

        return (T)Convert.ChangeType(result, targetType, CultureInfo.InvariantCulture);
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string storedProcedure, Func<SqlDataReader, T> map, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(map);
        await using var connection = CreateConnection();
        await using var command = CreateCommand(connection, storedProcedure, parameters);
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? map(reader) : default;
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string storedProcedure, Func<SqlDataReader, T> map, IEnumerable<SqlParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(map);
        var results = new List<T>();
        await using var connection = CreateConnection();
        await using var command = CreateCommand(connection, storedProcedure, parameters);
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(map(reader));
        }

        return results;
    }

    private SqlConnection CreateConnection() => new(_connectionString);

    private SqlCommand CreateCommand(SqlConnection connection, string storedProcedure, IEnumerable<SqlParameter>? parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedure);
        var command = connection.CreateCommand();
        command.CommandText = storedProcedure;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = _commandTimeoutSeconds;

        if (parameters is not null)
        {
            command.Parameters.AddRange(parameters.ToArray());
        }

        return command;
    }
}
