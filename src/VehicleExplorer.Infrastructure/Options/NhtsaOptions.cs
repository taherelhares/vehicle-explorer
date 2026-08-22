namespace VehicleExplorer.Infrastructure.Options;

/// <summary>
/// Configuration for the vPIC HTTP client, bound from the "Nhtsa" section.
/// Values live in appsettings.json and may be overridden per environment.
/// </summary>
public sealed class NhtsaOptions : ExternalApiOptions, IExternalApiSection
{
    public static string SectionName => "Nhtsa";
}
