using KalshiSharp.Models.Common;
using KalshiSharp.Models.Requests;
using KalshiSharp.Models.Responses;

namespace KalshiSharp.Rest.Markets;

/// <summary>
/// Client for Kalshi market endpoints.
/// </summary>
public interface IMarketClient
{
    /// <summary>
    /// Gets a single market by ticker.
    /// </summary>
    /// <param name="ticker">The market ticker.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The market details.</returns>
    Task<MarketResponse> GetMarketAsync(string ticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists markets with optional filtering and pagination.
    /// </summary>
    /// <param name="query">Optional query parameters for filtering and pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of markets.</returns>
    Task<PagedResponse<MarketResponse>> ListMarketsAsync(MarketQuery? query = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the order book for a market.
    /// </summary>
    /// <param name="ticker">The market ticker.</param>
    /// <param name="depth">Optional depth of order book levels to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order book for the market.</returns>
    Task<OrderBookResponse> GetOrderBookAsync(string ticker, int? depth = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets trades for a market.
    /// </summary>
    /// <param name="ticker">The market ticker.</param>
    /// <param name="cursor">Optional cursor for pagination.</param>
    /// <param name="limit">Optional limit on number of trades to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of trades.</returns>
    Task<PagedResponse<TradeResponse>> GetTradesAsync(string ticker, string? cursor = null, int? limit = null, CancellationToken cancellationToken = default);
}
