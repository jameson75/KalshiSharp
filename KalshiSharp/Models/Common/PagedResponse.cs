using System.Text.Json.Serialization;

namespace KalshiSharp.Models.Common;

/// <summary>
/// Represents a paginated response from the Kalshi API.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public abstract record PagedResponse<T>
{
    /// <summary>
    /// The items in this page of results.
    /// </summary>
    public abstract IReadOnlyList<T> Items { get; }

    /// <summary>
    /// The cursor to use for fetching the next page.
    /// Null or empty when there are no more pages.
    /// </summary>
    public string? Cursor { get; init; }

    /// <summary>
    /// True if there are more pages available.
    /// Based on non-empty cursor AND non-empty items.
    /// Empty cursor or empty items indicates end of pagination.
    /// </summary>
    public bool HasMore => !string.IsNullOrEmpty(Cursor) && Items.Count > 0;
}
