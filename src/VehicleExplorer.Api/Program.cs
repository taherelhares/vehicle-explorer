using VehicleExplorer.Api.Cors;
using VehicleExplorer.Api.Endpoints;
using VehicleExplorer.Api.ErrorHandling;
using VehicleExplorer.Application;
using VehicleExplorer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<NhtsaExceptionHandler>();

builder.Services.AddClientCors(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsExtensions.PolicyName);

app.MapVehicleEndpoints();

app.Run();
