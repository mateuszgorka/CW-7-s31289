using VetSqlClient.Services; // <-- to odkomentuj
using System.Data.SqlClient;
using VetSqlClient.Models;

var builder = WebApplication.CreateBuilder(args);

// Dodaj dostęp do konfiguracji connection stringów
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Dodaj serwis biznesowy (np. ClientService)
builder.Services.AddScoped<IClientService, ClientService>();

// Kontrolery
builder.Services.AddControllers();

// Dokumentacja OpenAPI (może być używana przez np. Postmana)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// Konfiguracja potoku HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // działa jak Swagger, ale bez UI
}

// Jeśli masz uwierzytelnianie — zostaje
app.UseAuthorization();

// Mapuj wszystkie kontrolery (np. /api/clients)
app.MapControllers();

app.Run();