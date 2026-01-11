namespace KalshiSharp.Http;

/// <summary>
/// HTTP client abstraction for Kalshi API requests.
/// </summary>
public interface IKalshiHttpClient
{
    /// <summary>
    /// Sends a request to the Kalshi API and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The type to deserialize the response to.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    Task<TResponse> SendAsync<TResponse>(KalshiRequest request, CancellationToken cancellationToken = default)
        where TResponse : class;

    /// <summary>
    /// Sends a request to the Kalshi API without expecting a response body.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendAsync(KalshiRequest request, CancellationToken cancellationToken = default);
}
