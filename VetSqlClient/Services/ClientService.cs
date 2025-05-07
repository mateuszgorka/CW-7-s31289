using Microsoft.Data.SqlClient;
using VetSqlClient.Exceptions;
using VetSqlClient.Models.DTOs;

namespace VetSqlClient.Services
{
    public class ClientService : IClientService
    {
        private readonly IConfiguration _configuration;

        public ClientService(IConfiguration configuration)
        {
            _configuration = configuration;
            
        }

        
       public async Task<IEnumerable<ClientTripDto>> GetClientTrips(int clientId)
{
    var trips = new List<ClientTripDto>();

    using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
    {
        await connection.OpenAsync();

        // Sprawdzenie czy klient istnieje
        using (var checkClientCmd = new SqlCommand("SELECT 1 FROM Client WHERE IdClient = @id", connection))
        {
            checkClientCmd.Parameters.AddWithValue("@id", clientId);
            var exists = await checkClientCmd.ExecuteScalarAsync();
            if (exists == null)
                throw new NotFoundException($"Client with id {clientId} not found");
        }

        // Pobranie wycieczek klienta
        using (var command = new SqlCommand(@"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                   ct.RegisteredAt, ct.PaymentDate
            FROM Client_Trip ct
            JOIN Trip t ON ct.IdTrip = t.IdTrip
            WHERE ct.IdClient = @id", connection))
        {
            command.Parameters.AddWithValue("@id", clientId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    trips.Add(new ClientTripDto
                    {
                        IdTrip = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                        DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                        MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                        RegisteredAt = DateTime.ParseExact(reader.GetInt32(reader.GetOrdinal("RegisteredAt")).ToString(), "yyyyMMdd", null),
                        PaymentDate = reader.IsDBNull(reader.GetOrdinal("PaymentDate"))
                            ? null
                            : DateTime.ParseExact(reader.GetInt32(reader.GetOrdinal("PaymentDate")).ToString(), "yyyyMMdd", null)

                    });
                }
            }
        }
    }

    return trips;
}

        public async Task<int> AddClient(ClientDto client)
        {
            if (string.IsNullOrWhiteSpace(client.FirstName) ||
                string.IsNullOrWhiteSpace(client.LastName) ||
                string.IsNullOrWhiteSpace(client.Email) ||
                string.IsNullOrWhiteSpace(client.Telephone) ||
                string.IsNullOrWhiteSpace(client.Pesel))
            {
                throw new ArgumentException("All fields are required");
            }

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(@"
                    INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                    OUTPUT INSERTED.IdClient
                    VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)", connection))
                {
                    command.Parameters.AddWithValue("@FirstName", client.FirstName);
                    command.Parameters.AddWithValue("@LastName", client.LastName);
                    command.Parameters.AddWithValue("@Email", client.Email);
                    command.Parameters.AddWithValue("@Telephone", client.Telephone);
                    command.Parameters.AddWithValue("@Pesel", client.Pesel);

                    return (int)(await command.ExecuteScalarAsync());
                }
            }
        }

        public async Task RegisterClientToTrip(int clientId, int tripId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // Sprawdzenie czy klient istnieje
                using (var cmd = new SqlCommand("SELECT 1 FROM Client WHERE IdClient = @id", connection))
                {
                    cmd.Parameters.AddWithValue("@id", clientId);
                    if (await cmd.ExecuteScalarAsync() == null)
                        throw new NotFoundException("Client not found");
                }

                // Sprawdzenie czy wycieczka istnieje
                using (var cmd = new SqlCommand("SELECT MaxPeople FROM Trip WHERE IdTrip = @tripId", connection))
                {
                    cmd.Parameters.AddWithValue("@tripId", tripId);
                    if (await cmd.ExecuteScalarAsync() == null)
                        throw new NotFoundException("Trip not found");
                }

                // Sprawdzenie czy już zapisany
                using (var cmd = new SqlCommand("SELECT 1 FROM Client_Trip WHERE IdClient = @id AND IdTrip = @tripId", connection))
                {
                    cmd.Parameters.AddWithValue("@id", clientId);
                    cmd.Parameters.AddWithValue("@tripId", tripId);
                    if (await cmd.ExecuteScalarAsync() != null)
                        throw new Exception("Client already registered for this trip");
                }

                // Sprawdzenie limitu miejsc
                int currentCount;
                int maxPeople;

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @tripId", connection))
                {
                    cmd.Parameters.AddWithValue("@tripId", tripId);
                    currentCount = (int)(await cmd.ExecuteScalarAsync());
                }

                using (var cmd = new SqlCommand("SELECT MaxPeople FROM Trip WHERE IdTrip = @tripId", connection))
                {
                    cmd.Parameters.AddWithValue("@tripId", tripId);
                    maxPeople = (int)(await cmd.ExecuteScalarAsync());
                }

                if (currentCount >= maxPeople)
                    throw new Exception("Maximum number of participants reached");

                // Rejestracja
                using (var cmd = new SqlCommand(@"
                    INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
                    VALUES (@id, @tripId, GETDATE())", connection))
                {
                    cmd.Parameters.AddWithValue("@id", clientId);
                    cmd.Parameters.AddWithValue("@tripId", tripId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UnregisterClientFromTrip(int clientId, int tripId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // Sprawdzenie rejestracji
                using (var cmd = new SqlCommand("SELECT 1 FROM Client_Trip WHERE IdClient = @id AND IdTrip = @tripId", connection))
                {
                    cmd.Parameters.AddWithValue("@id", clientId);
                    cmd.Parameters.AddWithValue("@tripId", tripId);
                    if (await cmd.ExecuteScalarAsync() == null)
                        throw new NotFoundException("Registration not found");
                }

                // Usunięcie rejestracji
                using (var cmd = new SqlCommand("DELETE FROM Client_Trip WHERE IdClient = @id AND IdTrip = @tripId", connection))
                {
                    cmd.Parameters.AddWithValue("@id", clientId);
                    cmd.Parameters.AddWithValue("@tripId", tripId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
