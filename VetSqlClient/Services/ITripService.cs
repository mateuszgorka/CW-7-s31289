using VetSqlClient.Models.DTOs;

namespace VetSqlClient.Services;

public interface ITripService
{
    Task<List<TripDto>> GetAllTripsAsync();
}

