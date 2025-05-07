using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using VetSqlClient.Exceptions;
using VetSqlClient.Models.DTOs;

namespace VetSqlClient.Services;

public class ClientService : IClientService
{
    private readonly string? _connectionString;

    public ClientService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<ClientTripDto>> GetClientTripsAsync(int clientId)
    {
        await using var connection = new SqlConnection(_connectionString);

        const string sql = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, 
                   ct.RegisteredAt, ct.PaymentDate
            FROM Client_Trip ct
            JOIN Trip t ON ct.IdTrip = t.IdTrip
            WHERE ct.IdClient = @id";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", clientId);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        var trips = new List<ClientTripDto>();
        while (await reader.ReadAsync())
        {
            trips.Add(new ClientTripDto
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5),
                RegisteredAt = reader.GetDateTime(6),
                PaymentDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
            });
        }

        if (trips.Count == 0)
            throw new NotFoundException($"Client with ID {clientId} not found or has no trips.");

        return trips;
    }

    public async Task<int> CreateClientAsync(ClientDto client)
    {
        await using var connection = new SqlConnection(_connectionString);

        const string sql = @"
            INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
            OUTPUT INSERTED.IdClient
            VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);

        await connection.OpenAsync();
        var newId = await command.ExecuteScalarAsync();
        return Convert.ToInt32(newId);
    }

    public async Task RegisterClientToTripAsync(int clientId, int tripId)
    {
        await using var connection = new SqlConnection(_connectionString);

        const string checkSql = @"
            SELECT COUNT(*) 
            FROM Client c, Trip t 
            WHERE c.IdClient = @clientId AND t.IdTrip = @tripId";

        await using var checkCmd = new SqlCommand(checkSql, connection);
        checkCmd.Parameters.AddWithValue("@clientId", clientId);
        checkCmd.Parameters.AddWithValue("@tripId", tripId);

        await connection.OpenAsync();
        var exists = (int)await checkCmd.ExecuteScalarAsync();
        if (exists == 0)
            throw new NotFoundException("Client or trip does not exist.");

        const string countSql = @"
            SELECT COUNT(*) 
            FROM Client_Trip 
            WHERE IdTrip = @tripId";

        await using var countCmd = new SqlCommand(countSql, connection);
        countCmd.Parameters.AddWithValue("@tripId", tripId);

        var currentCount = (int)await countCmd.ExecuteScalarAsync();

        const string maxSql = "SELECT MaxPeople FROM Trip WHERE IdTrip = @tripId";
        await using var maxCmd = new SqlCommand(maxSql, connection);
        maxCmd.Parameters.AddWithValue("@tripId", tripId);

        var maxPeople = (int)await maxCmd.ExecuteScalarAsync();

        if (currentCount >= maxPeople)
            throw new Exception("Trip has reached maximum number of participants.");

        const string insertSql = @"
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt) 
            VALUES (@clientId, @tripId, @now)";

        await using var insertCmd = new SqlCommand(insertSql, connection);
        insertCmd.Parameters.AddWithValue("@clientId", clientId);
        insertCmd.Parameters.AddWithValue("@tripId", tripId);
        insertCmd.Parameters.AddWithValue("@now", DateTime.UtcNow);

        await insertCmd.ExecuteNonQueryAsync();
    }

    public async Task RemoveClientFromTripAsync(int clientId, int tripId)
    {
        await using var connection = new SqlConnection(_connectionString);

        const string checkSql = @"
            SELECT COUNT(*) 
            FROM Client_Trip 
            WHERE IdClient = @clientId AND IdTrip = @tripId";

        await using var checkCmd = new SqlCommand(checkSql, connection);
        checkCmd.Parameters.AddWithValue("@clientId", clientId);
        checkCmd.Parameters.AddWithValue("@tripId", tripId);

        await connection.OpenAsync();
        var exists = (int)await checkCmd.ExecuteScalarAsync();
        if (exists == 0)
            throw new NotFoundException("Registration does not exist.");

        const string deleteSql = @"
            DELETE FROM Client_Trip 
            WHERE IdClient = @clientId AND IdTrip = @tripId";

        await using var deleteCmd = new SqlCommand(deleteSql, connection);
        deleteCmd.Parameters.AddWithValue("@clientId", clientId);
        deleteCmd.Parameters.AddWithValue("@tripId", tripId);

        await deleteCmd.ExecuteNonQueryAsync();
    }
}

