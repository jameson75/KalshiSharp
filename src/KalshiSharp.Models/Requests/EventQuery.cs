using System.Globalization;
using KalshiSharp.Models.Common;
using KalshiSharp.Models.Enums;

namespace KalshiSharp.Models.Requests;

/// <summary>
/// Query parameters for listing events.
/// </summary>
/// <remarks>
/// Supports cursor-based pagination via <see cref="PaginationParameters.Cursor"/>.
/// Use <see cref="PagedResponse{T}.Cursor"/> from the response to fetch the next page.
/// </remarks>
public sealed record EventQuery : PaginationParameters
{
    /// <summary>
    /// Filter by event status.
    /// </summary>
    public MarketStatus? Status { get; init; }

    /// <summary>
    /// Filter by series ticker.
    /// </summary>
    public string? SeriesTicker { get; init; }

    /// <summary>
    /// Search query to filter events by title or description.
    /// </summary>
    public string? WithNestedMarkets { get; init; }

    /// <summary>
    /// Builds the query string for the API request.
    /// </summary>
    /// <returns>The query string including the leading '?' if parameters exist.</returns>
    public string ToQueryString()
    {
        var builder = new QueryStringBuilder();

        AppendPaginationParameters(builder);

        if (Status.HasValue)
        {
            builder.Append("status", Status.Value.ToString().ToLowerInvariant());
        }

        builder.AppendIfNotEmpty("series_ticker", SeriesTicker);
        builder.AppendIfNotEmpty("with_nested_markets", WithNestedMarkets);

        return builder.Build();
    }
}
