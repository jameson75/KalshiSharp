using KalshiSharp.Models.Responses;

namespace KalshiSharp.Rest.Exchange;

/// <summary>
/// Client for Kalshi exchange status and schedule endpoints.
/// </summary>
public interface IExchangeClient
{
    /// <summary>
    /// Gets the current status of the Kalshi exchange.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The exchange status.</returns>
    Task<ExchangeStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the trading schedule of the Kalshi exchange.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The exchange schedule.</returns>
    Task<ExchangeScheduleResponse> GetScheduleAsync(CancellationToken cancellationToken = default);
}
