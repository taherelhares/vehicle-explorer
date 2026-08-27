using VehicleExplorer.Api.Cors;
using VehicleExplorer.Api.Endpoints;
using VehicleExplorer.Api.ErrorHandling;
using VehicleExplorer.Application;
using VehicleExplorer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<NhtsaExceptionHandler>();
builder.Services.AddHealthChecks();

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

// The React build lands in wwwroot at image build time, which is what makes one
// container the whole application: one origin for the browser, one port to publish, and
// no CORS in production because there is no second origin to allow.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapVehicleEndpoints();

// Deliberately dependency-free. It answers "is this container up and serving", which is
// the only question a deployment script or a health probe needs answered; checking vPIC
// here would report someone else's outage as our own.
app.MapHealthChecks("/health");

// Anything that matched no endpoint and no file on disk is a client-side route, so the
// SPA shell answers it and React resolves the path. Registered last, so it can only
// catch what nothing above wanted.
app.MapFallbackToFile("index.html");

app.Run();
