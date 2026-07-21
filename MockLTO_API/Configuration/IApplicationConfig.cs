namespace MockLTO_API.Configuration;

public interface IApplicationConfig
{
    string SqlConnectionString { get; }

    int SqlCommandTimeoutSeconds { get; }
}
