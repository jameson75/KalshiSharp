using KalshiSharp.Models.Common;

namespace KalshiSharp.Models.Responses;

/// <summary>
/// Response for listing positions.
/// </summary>
public sealed record PositionsResponse : PagedResponse<PositionResponse>
{
    /// <summary>
    /// The positions in this page.
    /// </summary>
    public IReadOnlyList<PositionResponse> Positions { get; init; } = [];

    /// <inheritdoc />
    public override IReadOnlyList<PositionResponse> Items => Positions;
}
