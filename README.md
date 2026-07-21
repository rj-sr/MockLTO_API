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

List the configured secret keys:

```powershell
dotnet user-secrets list --project MockLTO_API/MockLTO_API.csproj
```

Do not paste real credentials into tracked configuration files, source code, commits, issues, or pull requests.

For deployed environments, provide the same setting through the environment variable `ConnectionStrings__DefaultConnection` or another ASP.NET Core configuration provider.

## Run

```powershell
dotnet run --project MockLTO_API/MockLTO_API.csproj
```

In Development, Swagger UI is available at `https://localhost:7175/swagger` using the HTTPS launch profile.

The default stored-procedure command timeout is configured through `Database:CommandTimeoutSeconds` in `appsettings.json`.

## LTO vehicle view endpoints

The API reads `dbo.vw_LtoVehicleRecord` through parameterized ADO.NET queries. Available routes are:

- `GET /api/lto-vehicle-records?page=1&pageSize=50`
- `GET /api/lto-vehicle-records?registrationStatus=REGISTERED&hasLtoAlarm=false`
- `GET /api/lto-vehicle-records/{vehicleRegistrationId}`
- `GET /api/lto-vehicle-records/by-plate/{plateNumber}`
- `GET /api/lto-vehicle-records/by-mv-file/{mvFileNumber}`

The list endpoint accepts page sizes from 1 through 200. Plate lookups ignore spaces,
hyphens, and letter casing. Lookup endpoints return `404 Not Found` when no view row matches.
