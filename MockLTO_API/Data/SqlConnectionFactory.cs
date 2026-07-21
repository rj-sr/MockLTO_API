using Microsoft.Data.SqlClient;
using MockLTO_API.Configuration;

namespace MockLTO_API.Data;

public sealed class SqlConnectionFactory(IApplicationConfig configuration) : ISqlConnectionFactory
{
    private readonly string _connectionString = configuration.SqlConnectionString;

    public SqlConnection CreateConnection() => new(_connectionString);
}
