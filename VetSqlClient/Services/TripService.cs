using Microsoft.Data.SqlClient;
using VetSqlClient.Models.DTOs;

namespace VetSqlClient.Services;

public class TripService : ITripService
{
    private readonly string _connectionString;

    public TripService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
                            ?? throw new InvalidOperationException("Brak connection stringa 'Default' w konfiguracji.");
    }

    public async Task<List<TripDto>> GetAllTripsAsync()
    {
        var trips = new List<TripDto>();
        using var con = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("""
                                           SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                                                  c.Name as CountryName
                                           FROM Trip t
                                           JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
                                           JOIN Country c ON ct.IdCountry = c.IdCountry
                                       """, con);

        await con.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        var tripMap = new Dictionary<int, TripDto>();

        while (await reader.ReadAsync())
        {
            int idTrip = reader.GetInt32(0);
            if (!tripMap.ContainsKey(idTrip))
            {
                tripMap[idTrip] = new TripDto
                {
                    IdTrip = idTrip,
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                    Countries = new List<string>()
                };
            }
            tripMap[idTrip].Countries.Add(reader.GetString(6));
        }

        return tripMap.Values.ToList();
    }
}