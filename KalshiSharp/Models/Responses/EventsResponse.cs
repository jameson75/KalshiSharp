using KalshiSharp.Models.Common;

namespace KalshiSharp.Models.Responses;

/// <summary>
/// Response for listing events.
/// </summary>
public sealed record EventsResponse : PagedResponse<EventResponse>
{
    /// <summary>
    /// The events in this page.
    /// </summary>
    public IReadOnlyList<EventResponse> Events { get; init; } = [];

    /// <inheritdoc />
    public override IReadOnlyList<EventResponse> Items => Events;
}
