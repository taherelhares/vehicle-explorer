namespace VehicleExplorer.Infrastructure.Options;

/// <summary>
/// Lets an options type name the configuration section it binds from, so registration
/// needs no magic strings at the call site.
/// </summary>
public interface IExternalApiSection
{
    static abstract string SectionName { get; }
}
