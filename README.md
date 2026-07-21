# MockLTO API

ASP.NET Core mock LTO API with Swagger UI and SQL Server ADO.NET helpers for stored-procedure-based data access.

## Prerequisites

- .NET 10 SDK
- Microsoft SQL Server

## Configure local secrets

The SQL connection string is required at `ConnectionStrings:DefaultConnection`. Keep it out of `appsettings.json` and configure it with ASP.NET Core user secrets from the repository root:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True" --project MockLTO_API/MockLTO_API.csproj
```

For Windows authentication, a typical local value is:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=YOUR_DATABASE;Trusted_Connection=True;TrustServerCertificate=True" --project MockLTO_API/MockLTO_API.csproj
```

For the `MockLto` database on SQL Server LocalDB:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MockLto;Integrated Security=True;TrustServerCertificate=True" --project MockLTO_API/MockLTO_API.csproj
```

The database name (`Database` or `Initial Catalog`) is required. Otherwise SQL Server
uses the login's default database, which may not contain `dbo.vw_LtoVehicleRecord`.

List the configured secret keys:

```powershell
dotnet user-secrets list --project MockLTO_API/MockLTO_API.csproj
```

Do not paste real credentials into tracked configuration files, source code, commits, issues, or pull requests.

For deployed environments, provide the same setting through the environment variable `ConnectionStrings__DefaultConnection` or another ASP.NET Core configuration provider.

## Deploy to Render with Docker

The repository includes a Render Blueprint (`render.yaml`) and a production Dockerfile.
The container listens on port `10000`, exposes Swagger at `/swagger`, and provides a
database-independent health check at `/health`.

1. Push this repository to GitHub, GitLab, or Bitbucket.
2. In Render, select **New > Blueprint** and connect the repository.
3. When prompted for `ConnectionStrings__DefaultConnection`, enter the complete Azure
   SQL connection string, including the password. For this database, use:

```text
Data Source=tcp:rjsr.database.windows.net,1433;Initial Catalog=MockLtoDB;Persist Security Info=False;User ID=rj;Password=YOUR_PASSWORD;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;Command Timeout=0
```

Do not add the real password to `render.yaml`, the Dockerfile, or any tracked file.
After deployment, open `https://YOUR-SERVICE.onrender.com/swagger`.

If API calls cannot reach Azure SQL, allow the Render service's outbound IP ranges in
the Azure SQL server firewall. Find the service-specific ranges in Render under
**Connect > Outbound**. The `/health` route deliberately does not query the database,
so deployment health remains separate from database availability.

## Run

```powershell
dotnet run --project MockLTO_API/MockLTO_API.csproj
```

In Development, Swagger UI is available at `https://localhost:7175/swagger` using the HTTPS launch profile.

The default stored-procedure command timeout is configured through `Database:CommandTimeoutSeconds` in `appsettings.json`.

## LTO vehicle view endpoints

The API reads `dbo.vw_LtoVehicleRecord` through parameterized ADO.NET queries. Start
the API and use Swagger at `https://localhost:7175/swagger`, or call the routes directly.

### List and filter records

```http
GET /api/lto-vehicle-records?page=1&pageSize=50
```

Optional query parameters:

| Parameter | Description | Example |
| --- | --- | --- |
| `page` | Page number, starting at 1 | `1` |
| `pageSize` | Records per page, from 1 to 200 | `50` |
| `registrationStatus` | Exact registration status code | `REGISTERED` |
| `hasLtoAlarm` | Filter by active LTO alarm | `true` or `false` |

Filters can be combined:

```powershell
Invoke-RestMethod "https://localhost:7175/api/lto-vehicle-records?page=1&pageSize=20&registrationStatus=REGISTERED&hasLtoAlarm=false"
```

The list response includes the records and paging information:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 0
}
```

### Find one record

Use the vehicle registration ID:

```powershell
Invoke-RestMethod "https://localhost:7175/api/lto-vehicle-records/1"
```

Use a plate number. URL-encode spaces as `%20`; spaces, hyphens, and letter casing are
ignored during matching:

```powershell
Invoke-RestMethod "https://localhost:7175/api/lto-vehicle-records/by-plate/ABC%201234"
```

Use an MV file number:

```powershell
Invoke-RestMethod "https://localhost:7175/api/lto-vehicle-records/by-mv-file/1301-00000123456"
```

Single-record endpoints return `200 OK` with the matching record or `404 Not Found`
when there is no match. Invalid pagination values return `400 Bad Request`.

### Route summary

- `GET /api/lto-vehicle-records?page=1&pageSize=50`
- `GET /api/lto-vehicle-records?registrationStatus=REGISTERED&hasLtoAlarm=false`
- `GET /api/lto-vehicle-records/{vehicleRegistrationId}`
- `GET /api/lto-vehicle-records/by-plate/{plateNumber}`
- `GET /api/lto-vehicle-records/by-mv-file/{mvFileNumber}`
