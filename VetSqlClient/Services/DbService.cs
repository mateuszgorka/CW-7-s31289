using Microsoft.Data.SqlClient;
using VetSqlClient.Exceptions;
using VetSqlClient.Models;
using VetSqlClient.Models.DTOs;

namespace VetSqlClient.Services;

public interface IDbService
{
    public Task<IEnumerable<AnimalGetDTO>> GetAnimalsDetailsAsync();
    public Task<AnimalGetDTO> GetAnimalDetailsByIdAsync(int id);
    public Task<Animal> CreateAnimalAsync(AnimalCreateDTO animal);
    public Task ReplaceAnimalByIdAsync(int id, AnimalCreateDTO animal);
    public Task RemoveAnimalByIdAsync(int id);
    public Task<IEnumerable<VisitGetDTO>> GetVisitsByAnimalIdAsync(int animalId);
    public Task<Visit> CreateVisitAsync(int animalId, VisitCreateDTO visit);
}

public class DbService(IConfiguration config) : IDbService
{
    private readonly string? _connectionString = config.GetConnectionString("Default");
    
    public async Task<IEnumerable<AnimalGetDTO>> GetAnimalsDetailsAsync()
    {
        var result = new List<AnimalGetDTO>();
        
        await using var connection = new SqlConnection(_connectionString);
        const string sql = "select ID, Name, Weight, Category, CoatColor from Animals";
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new AnimalGetDTO
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Weight = Convert.ToDouble(reader.GetDecimal(2)),
                Category = reader.GetString(3),
                CoatColor = reader.GetString(4)
            });
        }

        return result;
    }

    public async Task<AnimalGetDTO> GetAnimalDetailsByIdAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        const string sql = "select ID, Name, Weight, Category, CoatColor from Animals where Id = @id";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        
        if (!await reader.ReadAsync())
        {
            throw new NotFoundException($"Animal with id: {id} does not exist");
        }

        return new AnimalGetDTO
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Weight = Convert.ToDouble(reader.GetDecimal(2)),
            Category = reader.GetString(3),
            CoatColor = reader.GetString(4)
        };
    }

    public async Task<Animal> CreateAnimalAsync(AnimalCreateDTO animal)
    {
        await using var connection = new SqlConnection(_connectionString);
        const string sql = "insert into Animals (Name, Weight, Category, CoatColor) values (@Name, @Weight, @Category, @CoatColor); Select scope_identity()";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Name", animal.Name);
        command.Parameters.AddWithValue("@Weight", animal.Weight);
        command.Parameters.AddWithValue("@Category", animal.Category);
        command.Parameters.AddWithValue("@CoatColor", animal.CoatColor);
        await connection.OpenAsync();
        var id = Convert.ToInt32(await command.ExecuteScalarAsync());
        
        return new Animal
        {
            Id = id,
            Category = animal.Category,
            CoatColor = animal.CoatColor,
            Name = animal.Name,
            Weight = animal.Weight
        };
    }

    public async Task ReplaceAnimalByIdAsync(int id, AnimalCreateDTO animal)
    {
        await using var connection = new SqlConnection(_connectionString);
        const string sql = "update Animals set Name = @Name, Weight = @Weight, Category = @Category, CoatColor = @CoatColor where Id = @id";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Name", animal.Name);
        command.Parameters.AddWithValue("@Weight", animal.Weight);
        command.Parameters.AddWithValue("@Category", animal.Category);
        command.Parameters.AddWithValue("@CoatColor", animal.CoatColor);
        command.Parameters.AddWithValue("@id", id);
        await connection.OpenAsync();
        var numOfRows = await command.ExecuteNonQueryAsync();

        if (numOfRows == 0)
        {
            throw new NotFoundException($"Animal with id: {id} does not exist");
        }
    }

    public async Task RemoveAnimalByIdAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        const string sql1 = "delete from Visits where Animal_ID = @id";
        await using (var command1 = new SqlCommand(sql1, connection))
        {
            command1.Parameters.AddWithValue("@id", id);
            await connection.OpenAsync();
            await command1.ExecuteNonQueryAsync();
        }
        
        const string sql2 = "delete from Animals where Id = @id";
        await using var command2 = new SqlCommand(sql2, connection);
        command2.Parameters.AddWithValue("@id", id);
        var numOfRows = await command2.ExecuteNonQueryAsync();

        if (numOfRows == 0)
        {
            throw new NotFoundException($"Animal with id: {id} does not exist");
        }
    }

    public async Task<IEnumerable<VisitGetDTO>> GetVisitsByAnimalIdAsync(int animalId)
    {
        await using var connection = new SqlConnection(_connectionString);
        const string sql = "select 1 from Animals where Id = @animalId";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@animalId", animalId);
        await connection.OpenAsync();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (!reader.HasRows)
            {
                throw new NotFoundException($"Animal with id: {animalId} does not exist");
            }
        }

        var result = new List<VisitGetDTO>();
        
        const string sql2 = "select ID, Date, Description, Price from Visits where Animal_ID = @animalId";
        await using var command2 = new SqlCommand(sql2, connection);
        command2.Parameters.AddWithValue("@animalId", animalId);
        await using var reader2 = await command2.ExecuteReaderAsync();
        while (await reader2.ReadAsync())
        {
            result.Add(new VisitGetDTO
            {
                Id = reader2.GetInt32(0),
                Date = reader2.GetDateTime(1),
                Description = reader2.GetString(2),
                Price = Convert.ToDouble(reader2.GetDecimal(3)),
            });
        }

        return result;
    }

    public async Task<Visit> CreateVisitAsync(int animalId, VisitCreateDTO visit)
    {
        await using var connection = new SqlConnection(_connectionString);
        const string sql1 = "select 1 from Animals where Id = @animalId";
        await using var command = new SqlCommand(sql1, connection);
        command.Parameters.AddWithValue("@animalId", animalId);
        await connection.OpenAsync();
        await using (var reader1 = await command.ExecuteReaderAsync())
        {
            if (!reader1.HasRows)
            {
                throw new NotFoundException($"Animal with id: {animalId} does not exist");
            }
        }

        const string sql2 =
            "insert into Visits (Date, Description, Price, Animal_ID) values (@Date, @Description, @Price, @Animal_ID); Select scope_identity()";
        await using var command2 = new SqlCommand(sql2, connection);
        command2.Parameters.AddWithValue("@Date", visit.Date);
        command2.Parameters.AddWithValue("@Description", visit.Description);
        command2.Parameters.AddWithValue("@Price", visit.Price);
        command2.Parameters.AddWithValue("@Animal_ID", animalId);
        
        var id = Convert.ToInt32(await command2.ExecuteScalarAsync());

        return new Visit
        {
            Id = id,
            Date = visit.Date,
            Description = visit.Description,
            Price = visit.Price,
            AnimalId = animalId
        };
    }
}