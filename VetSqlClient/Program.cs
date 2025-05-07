using VetSqlClient.Services;

var builder = WebApplication.CreateBuilder(args);

// Dodanie serwisów
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddTransient<ITripService, TripService>();
builder.Services.AddScoped<IClientService, ClientService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();