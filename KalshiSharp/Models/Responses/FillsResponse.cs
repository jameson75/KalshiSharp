using KalshiSharp.Models.Common;

namespace KalshiSharp.Models.Responses;

/// <summary>
/// Response for listing fills.
/// </summary>
public sealed record FillsResponse : PagedResponse<FillResponse>
{
    /// <summary>
    /// The fills in this page.
    /// </summary>
    public IReadOnlyList<FillResponse> Fills { get; init; } = [];

    /// <inheritdoc />
    public override IReadOnlyList<FillResponse> Items => Fills;
}
