using Microsoft.AspNetCore.Mvc;
using VetSqlClient.Exceptions;
using VetSqlClient.Models.DTOs;
using VetSqlClient.Services;

namespace VetSqlClient.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetClientTrips(int id)
        {
            try
            {
                var trips = await _clientService.GetClientTrips(id);
                return Ok(trips);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddClient([FromBody] ClientDto clientDto)
        {
            var id = await _clientService.AddClient(clientDto);
            return CreatedAtAction(nameof(GetClientTrips), new { id = id }, null);
        }

        [HttpPost("{clientId}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientToTrip(int clientId, int tripId)
        {
            try
            {
                await _clientService.RegisterClientToTrip(clientId, tripId);
                return Ok("Client registered for the trip");
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{clientId}/trips/{tripId}")]
        public async Task<IActionResult> UnregisterClientFromTrip(int clientId, int tripId)
        {
            try
            {
                await _clientService.UnregisterClientFromTrip(clientId, tripId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
