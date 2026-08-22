namespace VehicleExplorer.Application.Abstractions;

/// <summary>
/// The upstream vehicle data provider could not be reached, or answered in a way we
/// cannot use. Deliberately one exception rather than several: to everything above the
/// adapter, a timeout, a broken circuit and a 503 all mean the same thing.
/// </summary>
public sealed class NhtsaUnavailableException : Exception
{
    public NhtsaUnavailableException()
        : base("The vehicle data service is unavailable.")
    {
    }

    public NhtsaUnavailableException(string message)
        : base(message)
    {
    }

    public NhtsaUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
