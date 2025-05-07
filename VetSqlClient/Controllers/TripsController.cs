using Microsoft.AspNetCore.Mvc;
using VetSqlClient.Services;

namespace VetSqlClient.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;
    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _tripService.GetAllTripsAsync();
        return Ok(trips);
    }
}