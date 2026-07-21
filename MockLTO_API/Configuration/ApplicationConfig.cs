namespace MockLTO_API.Configuration;

public sealed class ApplicationConfig : IApplicationConfig
{
    private const string SqlCommandTimeoutKey = "Database:CommandTimeoutSeconds";
    private const int DefaultSqlCommandTimeoutSeconds = 30;
    private readonly IConfiguration _configuration;

    public ApplicationConfig(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string SqlConnectionString => GetRequiredSecret(SecretKeys.SqlConnectionString);

    public int SqlCommandTimeoutSeconds
    {
        get
        {
            var timeout = _configuration.GetValue(SqlCommandTimeoutKey, DefaultSqlCommandTimeoutSeconds);

            return timeout > 0
                ? timeout
                : throw new InvalidOperationException(
                    $"Configuration value '{SqlCommandTimeoutKey}' must be greater than zero.");
        }
    }

    private string GetRequiredSecret(string key)
    {
        var value = _configuration[key];

        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException(
                $"Required secret '{key}' is not configured. " +
                "Set it with ASP.NET Core user secrets or an environment variable.");
    }
}
