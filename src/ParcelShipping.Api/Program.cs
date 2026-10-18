using ParcelShipping.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IPackageCatalogue, InCodePackageCatalogue>();
builder.Services.AddSingleton<PackageSelector>();

var app = builder.Build();

app.MapControllers();

app.Run();

// Exposed so the integration tests can host the app with WebApplicationFactory.
public partial class Program;
