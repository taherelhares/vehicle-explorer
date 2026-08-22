namespace VehicleExplorer.Infrastructure.Options;

/// <summary>
/// Configuration shared by every third-party HTTP API this application talks to.
/// Only the address is configurable: where a service lives genuinely differs between
/// environments, whereas how long we are willing to wait for it does not, and letting
/// that drift would mean local runs never reproduce a production timeout.
/// </summary>
public abstract class ExternalApiOptions
{
    /// <summary>Root address of the upstream service. Required.</summary>
    public Uri? BaseAddress { get; set; }
}
