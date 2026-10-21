using KalshiSharp.Models.Requests;
using KalshiSharp.Models.Responses;

namespace KalshiSharp.Rest.Historical
{
    public interface IHistoricalClient
    {
        /// <summary>
        /// Returns the cutoff timestamps that define the boundary between live and historical data.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HistoricalCutoffResponse> GetHistoricalCutoff(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets markets that have been archived to the historical database.
        /// </summary>
        /// <param name="query">Optional query parameters for filtering and pagination.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A paginated list of historical markets.</returns>
        Task<MarketsResponse> ListHistoricalMarketsAsync(HistoricalMarketQuery? query = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Endpoint for fetching historical candlestick data for markets that have been archived from the live data set. Time period length of each candlestick in minutes. Valid values: 1 (1 minute), 60 (1 hour), 1440 (1 day).
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HistoricalMarketCandlesticksResponse> GetHistoricalMarketCandlesticks(string ticker, MarketCandlesticksQuery query, CancellationToken cancellationToken = default);
    }
}
