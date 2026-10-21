using KalshiSharp.Models.Common;

namespace KalshiSharp.Models.Requests;

/// <summary>
/// Query parameters for listing historical markets.
/// </summary>
public sealed record HistoricalMarketQuery : PaginationParameters
{
    /// <summary>
    /// Filter by specific market tickers. Comma-separated list of market tickers to retrieve.
    /// </summary>
    public IReadOnlyList<string>? Tickers { get; init; }

    /// <summary>
    /// Filter by a single event ticker.
    /// </summary>
    public string? EventTicker { get; init; }

    /// <summary>
    /// Filter by series ticker.
    /// </summary>
    public string? SeriesTicker { get; init; }

    /// <summary>
    /// Filter by multivariate events (combos). Use "exclude" to filter out MVE markets.
    /// </summary>
    public string? MveFilter { get; init; }

    /// <summary>
    /// Builds the query string for the API request.
    /// </summary>
    /// <returns>The query string including the leading '?' if parameters exist.</returns>
    public string ToQueryString()
    {
        var builder = new QueryStringBuilder();

        AppendPaginationParameters(builder);

        if (Tickers is { Count: > 0 })
            builder.Append("tickers", string.Join(",", Tickers));

        builder.AppendIfNotEmpty("event_ticker", EventTicker);
        builder.AppendIfNotEmpty("series_ticker", SeriesTicker);
        builder.AppendIfNotEmpty("mve_filter", MveFilter);

        return builder.Build();
    }
}
