using MockLTO_API.Models;

namespace MockLTO_API.Repositories;

public interface ILtoVehicleRecordRepository
{
    Task<PagedResult<LtoVehicleRecord>> GetAsync(
        int page,
        int pageSize,
        string? registrationStatus,
        bool? hasLtoAlarm,
        CancellationToken cancellationToken = default);

    Task<LtoVehicleRecord?> GetByRegistrationIdAsync(long id, CancellationToken cancellationToken = default);
    Task<LtoVehicleRecord?> GetByPlateNumberAsync(string plateNumber, CancellationToken cancellationToken = default);
    Task<LtoVehicleRecord?> GetByMvFileNumberAsync(string mvFileNumber, CancellationToken cancellationToken = default);
}
