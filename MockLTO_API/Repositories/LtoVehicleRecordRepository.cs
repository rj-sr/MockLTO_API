using System.Data;
using Microsoft.Data.SqlClient;
using MockLTO_API.Data;
using MockLTO_API.Models;

namespace MockLTO_API.Repositories;

public sealed class LtoVehicleRecordRepository(ISqlQueryExecutor sql) : ILtoVehicleRecordRepository
{
    private const string Columns = """
        VehicleRegistrationId, VehicleId, PlateNumber, MVFileNumber,
        EngineNumber, ChassisNumber, Make, SeriesModel, BodyType, Color,
        ModelYear, Classification, RegistrationStatus, RegistrationDate,
        RegistrationExpiryDate, HasLTOAlarm, RegisteredOwnerId,
        RegisteredOwnerFullName, RegisteredOwnerNationalId, MobileNumber,
        EmailAddress, RegisteredAddress
        """;

    public async Task<PagedResult<LtoVehicleRecord>> GetAsync(
        int page,
        int pageSize,
        string? registrationStatus,
        bool? hasLtoAlarm,
        CancellationToken cancellationToken = default)
    {
        var filterParameters = new List<SqlParameter>();
        var predicates = new List<string>();

        if (!string.IsNullOrWhiteSpace(registrationStatus))
        {
            predicates.Add("RegistrationStatus = @RegistrationStatus");
            filterParameters.Add(SqlParams.Input("RegistrationStatus", SqlDbType.VarChar, registrationStatus.Trim(), 30));
        }

        if (hasLtoAlarm.HasValue)
        {
            predicates.Add("HasLTOAlarm = @HasLTOAlarm");
            filterParameters.Add(SqlParams.Input("HasLTOAlarm", SqlDbType.Bit, hasLtoAlarm.Value));
        }

        var where = predicates.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", predicates)}";
        var query = $"""
            SELECT {Columns}
            FROM dbo.vw_LtoVehicleRecord
            {where}
            ORDER BY VehicleRegistrationId
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var totalCount = await sql.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM dbo.vw_LtoVehicleRecord {where};",
            filterParameters,
            cancellationToken);
        var pageParameters = filterParameters
            .Select(CloneParameter)
            .Append(SqlParams.Input("PageSize", SqlDbType.Int, pageSize))
            .Append(SqlParams.Input("Offset", SqlDbType.BigInt, (long)(page - 1) * pageSize));
        var rows = await sql.QueryAsync(query, Map, pageParameters, cancellationToken);

        return new PagedResult<LtoVehicleRecord>(
            rows,
            page,
            pageSize,
            totalCount);
    }

    public Task<LtoVehicleRecord?> GetByRegistrationIdAsync(long id, CancellationToken cancellationToken = default) =>
        QueryOneAsync(
            "VehicleRegistrationId = @VehicleRegistrationId",
            SqlParams.Input("VehicleRegistrationId", SqlDbType.BigInt, id),
            cancellationToken);

    public Task<LtoVehicleRecord?> GetByPlateNumberAsync(string plateNumber, CancellationToken cancellationToken = default) =>
        QueryOneAsync(
            "UPPER(REPLACE(REPLACE(PlateNumber, ' ', ''), '-', '')) = UPPER(REPLACE(REPLACE(@PlateNumber, ' ', ''), '-', ''))",
            SqlParams.Input("PlateNumber", SqlDbType.VarChar, plateNumber.Trim(), 20),
            cancellationToken);

    public Task<LtoVehicleRecord?> GetByMvFileNumberAsync(string mvFileNumber, CancellationToken cancellationToken = default) =>
        QueryOneAsync(
            "MVFileNumber = @MVFileNumber",
            SqlParams.Input("MVFileNumber", SqlDbType.VarChar, mvFileNumber.Trim(), 30),
            cancellationToken);

    private Task<LtoVehicleRecord?> QueryOneAsync(
        string predicate,
        SqlParameter parameter,
        CancellationToken cancellationToken) =>
        sql.QuerySingleOrDefaultAsync(
            $"SELECT {Columns} FROM dbo.vw_LtoVehicleRecord WHERE {predicate};",
            Map,
            [parameter],
            cancellationToken);

    private static SqlParameter CloneParameter(SqlParameter parameter) =>
        new(parameter.ParameterName, parameter.SqlDbType, parameter.Size)
        {
            Value = parameter.Value
        };

    private static LtoVehicleRecord Map(SqlDataReader reader) => new(
        reader.GetInt64(reader.GetOrdinal("VehicleRegistrationId")),
        reader.GetInt64(reader.GetOrdinal("VehicleId")),
        reader.GetString(reader.GetOrdinal("PlateNumber")),
        reader.GetString(reader.GetOrdinal("MVFileNumber")),
        reader.GetString(reader.GetOrdinal("EngineNumber")),
        reader.GetString(reader.GetOrdinal("ChassisNumber")),
        reader.GetString(reader.GetOrdinal("Make")),
        reader.GetString(reader.GetOrdinal("SeriesModel")),
        reader.GetString(reader.GetOrdinal("BodyType")),
        reader.GetNullableString("Color"),
        reader.GetInt16(reader.GetOrdinal("ModelYear")),
        reader.GetString(reader.GetOrdinal("Classification")),
        reader.GetString(reader.GetOrdinal("RegistrationStatus")),
        reader.GetNullableDateOnly("RegistrationDate"),
        reader.GetNullableDateOnly("RegistrationExpiryDate"),
        reader.GetBoolean(reader.GetOrdinal("HasLTOAlarm")),
        reader.GetInt64(reader.GetOrdinal("RegisteredOwnerId")),
        reader.GetString(reader.GetOrdinal("RegisteredOwnerFullName")),
        reader.GetNullableString("RegisteredOwnerNationalId"),
        reader.GetNullableString("MobileNumber"),
        reader.GetNullableString("EmailAddress"),
        reader.GetString(reader.GetOrdinal("RegisteredAddress")));
}
