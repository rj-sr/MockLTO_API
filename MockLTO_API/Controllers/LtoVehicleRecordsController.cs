using Microsoft.AspNetCore.Mvc;
using MockLTO_API.Models;
using MockLTO_API.Repositories;

namespace MockLTO_API.Controllers;

[ApiController]
[Route("api/lto-vehicle-records")]
public sealed class LtoVehicleRecordsController(ILtoVehicleRecordRepository repository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<LtoVehicleRecord>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<LtoVehicleRecord>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? registrationStatus = null,
        [FromQuery] bool? hasLtoAlarm = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 200)
        {
            return BadRequest("Page must be at least 1 and pageSize must be between 1 and 200.");
        }

        return Ok(await repository.GetAsync(
            page, pageSize, registrationStatus, hasLtoAlarm, cancellationToken));
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType<LtoVehicleRecord>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LtoVehicleRecord>> GetByRegistrationId(
        long id,
        CancellationToken cancellationToken)
    {
        var record = await repository.GetByRegistrationIdAsync(id, cancellationToken);
        return record is null ? NotFound() : Ok(record);
    }

    [HttpGet("by-plate/{plateNumber}")]
    [ProducesResponseType<LtoVehicleRecord>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LtoVehicleRecord>> GetByPlateNumber(
        string plateNumber,
        CancellationToken cancellationToken)
    {
        var record = await repository.GetByPlateNumberAsync(plateNumber, cancellationToken);
        return record is null ? NotFound() : Ok(record);
    }

    [HttpGet("by-mv-file/{mvFileNumber}")]
    [ProducesResponseType<LtoVehicleRecord>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LtoVehicleRecord>> GetByMvFileNumber(
        string mvFileNumber,
        CancellationToken cancellationToken)
    {
        var record = await repository.GetByMvFileNumberAsync(mvFileNumber, cancellationToken);
        return record is null ? NotFound() : Ok(record);
    }
}
