using System.Text.Json.Serialization;

namespace KalshiSharp.Models.Responses
{
    public sealed record HistoricalCutoffResponse
    {
        /// <summary>
        /// Cutoff based on market settlement time. Markets and their candlesticks that settled before this timestamp must be accessed via GET /historical/markets and GET /historical/markets/{ticker}/candlesticks.
        /// </summary>
        [JsonPropertyName("market_settled_ts")]
        public DateTimeOffset MarketSettled { get; init; }

        /// <summary>
        /// Cutoff based on trade fill time. Fills that occurred before this timestamp must be accessed via GET /historical/fills.
        /// </summary>
        [JsonPropertyName("trades_created_ts")]
        public DateTimeOffset TradesCreated { get; init; }

        /// <summary>
        /// Cutoff based on order cancellation or execution time. Orders canceled or fully executed before this timestamp must be accessed via GET /historical/orders. Resting (active) orders are always available in GET /portfolio/orders.
        /// </summary>
        [JsonPropertyName("orders_updated_ts")]
        public DateTimeOffset OrdersUpdated { get; init; }

        /// <summary>
        /// Cutoff based on position last-update time. Settled positions archived from the live data set before this timestamp are served through the historical section of position reads.
        /// </summary>
        [JsonPropertyName("market_positions_last_updated_ts")]
        public DateTimeOffset MarketPositionsLastUpdated { get; init; }
    }
}