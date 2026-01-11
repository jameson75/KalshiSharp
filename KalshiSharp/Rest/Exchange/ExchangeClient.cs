using KalshiSharp.Http;
using KalshiSharp.Models.Responses;

namespace KalshiSharp.Rest.Exchange;

/// <summary>
/// Implementation of the exchange client for status and schedule endpoints.
/// </summary>
internal sealed class ExchangeClient : IExchangeClient
{
    private const string BasePath = "/trade-api/v2/exchange";

    private readonly IKalshiHttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExchangeClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public ExchangeClient(IKalshiHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public Task<ExchangeStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var request = new KalshiRequest
        {
            Method = HttpMethod.Get,
            Path = $"{BasePath}/status"
        };

        return _httpClient.SendAsync<ExchangeStatusResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ExchangeScheduleResponse> GetScheduleAsync(CancellationToken cancellationToken = default)
    {
        var request = new KalshiRequest
        {
            Method = HttpMethod.Get,
            Path = $"{BasePath}/schedule"
        };

        return _httpClient.SendAsync<ExchangeScheduleResponse>(request, cancellationToken);
    }
}
