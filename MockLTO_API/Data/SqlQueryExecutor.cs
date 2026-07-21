using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using MockLTO_API.Configuration;

namespace MockLTO_API.Data;

public sealed class SqlQueryExecutor(
    ISqlConnectionFactory connectionFactory,
    IApplicationConfig configuration) : ISqlQueryExecutor
{
    private readonly int _commandTimeoutSeconds = configuration.SqlCommandTimeoutSeconds;

    public async Task<T?> ExecuteScalarAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await using var command = CreateCommand(connection, sql, parameters);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);

        if (result is null or DBNull)
        {
            return default;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        return (T)Convert.ChangeType(result, targetType, CultureInfo.InvariantCulture);
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        Func<SqlDataReader, T> map,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(map);
        var results = new List<T>();

        await using var connection = connectionFactory.CreateConnection();
        await using var command = CreateCommand(connection, sql, parameters);
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(map(reader));
        }

        return results;
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        Func<SqlDataReader, T> map,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(map);

        await using var connection = connectionFactory.CreateConnection();
        await using var command = CreateCommand(connection, sql, parameters);
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? map(reader) : default;
    }

    private SqlCommand CreateCommand(
        SqlConnection connection,
        string sql,
        IEnumerable<SqlParameter>? parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.CommandTimeout = _commandTimeoutSeconds;

        if (parameters is not null)
        {
            command.Parameters.AddRange(parameters.ToArray());
        }

        return command;
    }
}
