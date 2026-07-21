using Microsoft.Data.SqlClient;

namespace MockLTO_API.Data;

public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection();
}
