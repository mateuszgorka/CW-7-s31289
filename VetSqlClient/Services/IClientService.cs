using VetSqlClient.Models.DTOs;

namespace VetSqlClient.Services
{
    public interface IClientService
    {
        Task<IEnumerable<ClientTripDto>> GetClientTrips(int clientId);
        Task<int> AddClient(ClientDto client);
        Task RegisterClientToTrip(int clientId, int tripId);
        Task UnregisterClientFromTrip(int clientId, int tripId);
    }
}
