using System.Data;
using Microsoft.Data.SqlClient;

namespace MockLTO_API.Data;

public static class SqlParams
{
    public static SqlParameter Input(string name, SqlDbType type, object? value, int? size = null)
    {
        var parameter = Create(name, type, size);
        parameter.Value = value ?? DBNull.Value;
        return parameter;
    }

    public static SqlParameter Output(string name, SqlDbType type, int? size = null)
    {
        var parameter = Create(name, type, size);
        parameter.Direction = ParameterDirection.Output;
        return parameter;
    }

    public static SqlParameter InputOutput(string name, SqlDbType type, object? value, int? size = null)
    {
        var parameter = Input(name, type, value, size);
        parameter.Direction = ParameterDirection.InputOutput;
        return parameter;
    }

    public static SqlParameter ReturnValue(string name = "@ReturnValue") =>
        new(name, SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };

    private static SqlParameter Create(string name, SqlDbType type, int? size)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalizedName = name.StartsWith('@') ? name : $"@{name}";
        var parameter = new SqlParameter(normalizedName, type);

        if (size is not null)
        {
            parameter.Size = size.Value;
        }

        return parameter;
    }
}
