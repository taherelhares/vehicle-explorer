using System.Net;
using System.Text;

namespace VehicleExplorer.Infrastructure.IntegrationTests;

/// <summary>
/// Stands in for the network. Everything above it — Refit, the resilience pipeline,
/// System.Text.Json and the adapter's mapping — is the real thing.
/// </summary>
internal sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string body)
    : HttpMessageHandler
{
    public Uri? LastRequestUri { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastRequestUri = request.RequestUri;

        return Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
            RequestMessage = request
        });
    }
}
