using KalshiSharp.Models.Common;
using KalshiSharp.Models.Requests;
using KalshiSharp.Models.Responses;

namespace KalshiSharp.Rest.Events;

/// <summary>
/// Client for Kalshi event endpoints.
/// </summary>
public interface IEventClient
{
    /// <summary>
    /// Gets a single event by ticker.
    /// </summary>
    /// <param name="eventTicker">The event ticker.</param>
    /// <param name="withNestedMarkets">Whether to include nested markets in the response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The event details.</returns>
    Task<EventResponse> GetEventAsync(string eventTicker, bool? withNestedMarkets = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists events with optional filtering and pagination.
    /// </summary>
    /// <param name="query">Optional query parameters for filtering and pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of events.</returns>
    Task<PagedResponse<EventResponse>> ListEventsAsync(EventQuery? query = null, CancellationToken cancellationToken = default);
}
