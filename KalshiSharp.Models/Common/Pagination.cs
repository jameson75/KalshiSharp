using System.Globalization;

namespace KalshiSharp.Models.Common;

/// <summary>
/// Base class for pagination parameters used in API queries.
/// </summary>
public abstract record PaginationParameters
{
    /// <summary>
    /// The maximum number of items to return per page.
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// The cursor for fetching the next page of results.
    /// Obtained from a previous <see cref="PagedResponse{T}.Cursor"/>.
    /// </summary>
    public string? Cursor { get; init; }

    /// <summary>
    /// Appends pagination query parameters to the provided query string builder.
    /// </summary>
    /// <param name="builder">The query string builder to append to.</param>
    protected void AppendPaginationParameters(QueryStringBuilder builder)
    {
        if (Limit.HasValue)
        {
            builder.Append("limit", Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (!string.IsNullOrEmpty(Cursor))
        {
            builder.Append("cursor", Cursor);
        }
    }
}

/// <summary>
/// Helper for building query strings from parameters.
/// </summary>
public sealed class QueryStringBuilder
{
    private readonly List<(string Key, string Value)> _parameters = [];

    /// <summary>
    /// Appends a query parameter.
    /// </summary>
    /// <param name="key">The parameter key.</param>
    /// <param name="value">The parameter value.</param>
    public void Append(string key, string value)
    {
        _parameters.Add((key, value));
    }

    /// <summary>
    /// Appends a query parameter if the value is not null or empty.
    /// </summary>
    /// <param name="key">The parameter key.</param>
    /// <param name="value">The parameter value.</param>
    public void AppendIfNotEmpty(string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _parameters.Add((key, value));
        }
    }

    /// <summary>
    /// Appends a query parameter if the value is not null.
    /// </summary>
    /// <param name="key">The parameter key.</param>
    /// <param name="value">The parameter value.</param>
    public void AppendIfNotNull<T>(string key, T? value) where T : struct
    {
        if (value.HasValue)
        {
            _parameters.Add((key, string.Format(CultureInfo.InvariantCulture, "{0}", value.Value)));
        }
    }

    /// <summary>
    /// Builds the query string. Returns empty string if no parameters.
    /// </summary>
    public string Build()
    {
        if (_parameters.Count == 0)
        {
            return string.Empty;
        }

        return "?" + string.Join("&", _parameters.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }

    /// <summary>
    /// Returns true if any parameters have been added.
    /// </summary>
    public bool HasParameters => _parameters.Count > 0;
}
