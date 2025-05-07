using Microsoft.AspNetCore.Mvc;
using VetSqlClient.Models.DTOs;
using VetSqlClient.Services;

namespace VetSqlClient.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] ClientDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        int id = await _clientService.CreateClientAsync(dto);
        return Created($"api/clients/{id}", new { Id = id });
    }

    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTrips(int id)
    {
        var trips = await _clientService.GetClientTripsAsync(id);
        return Ok(trips);
    }

    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClient(int id, int tripId)
    {
        await _clientService.RegisterClientToTripAsync(id, tripId);
        return Ok("Client registered to trip.");
    }

    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> RemoveClient(int id, int tripId)
    {
        await _clientService.RemoveClientFromTripAsync(id, tripId);
        return Ok("Client removed from trip.");
    }
}