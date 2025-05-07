using VetSqlClient.Models.DTOs;

namespace VetSqlClient.Services;

public interface IClientService
{
    Task<List<ClientTripDto>> GetClientTripsAsync(int clientId);
    Task<int> CreateClientAsync(ClientDto client);
    Task RegisterClientToTripAsync(int clientId, int tripId);
    Task RemoveClientFromTripAsync(int clientId, int tripId);
    
}
