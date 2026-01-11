using System.Text;
using FluentAssertions;
using KalshiSharp.Auth;

namespace KalshiSharp.Tests.Auth;

public class CanonicalRequestBuilderTests
{
    [Fact]
    public void Build_GetRequestWithoutBody_ReturnsCorrectCanonicalString()
    {
        // Arrange
        const long timestampMs = 1704067200000;
        const string method = "GET";
        const string path = "/trade-api/v2/exchange/status";

        // Act
        var result = CanonicalRequestBuilder.Build(timestampMs, method, path, ReadOnlySpan<byte>.Empty);

        // Assert
        var expected = "1704067200000\nGET\n/trade-api/v2/exchange/status\n";
        Encoding.UTF8.GetString(result).Should().Be(expected);
    }

    [Fact]
    public void Build_PostRequestWithBody_ReturnsCorrectCanonicalString()
    {
        // Arrange
        const long timestampMs = 1704067200000;
        const string method = "POST";
        const string path = "/trade-api/v2/portfolio/orders";
        var body = Encoding.UTF8.GetBytes("{\"ticker\":\"TEST\"}");

        // Act
        var result = CanonicalRequestBuilder.Build(timestampMs, method, path, body);

        // Assert
        var expected = "1704067200000\nPOST\n/trade-api/v2/portfolio/orders\n{\"ticker\":\"TEST\"}";
        Encoding.UTF8.GetString(result).Should().Be(expected);
    }

    [Fact]
    public void Build_GetRequestWithQueryString_IncludesQueryInPath()
    {
        // Arrange
        const long timestampMs = 1704067200000;
        const string method = "GET";
        const string path = "/trade-api/v2/markets?status=open&limit=100";

        // Act
        var result = CanonicalRequestBuilder.Build(timestampMs, method, path, ReadOnlySpan<byte>.Empty);

        // Assert
        var expected = "1704067200000\nGET\n/trade-api/v2/markets?status=open&limit=100\n";
        Encoding.UTF8.GetString(result).Should().Be(expected);
    }

    [Fact]
    public void Build_LowercaseMethod_IsUppercased()
    {
        // Arrange
        const long timestampMs = 1704067200000;
        const string method = "get";
        const string path = "/test";

        // Act
        var result = CanonicalRequestBuilder.Build(timestampMs, method, path, ReadOnlySpan<byte>.Empty);

        // Assert
        Encoding.UTF8.GetString(result).Should().Contain("\nGET\n");
    }

    [Fact]
    public void Build_WithHttpRequestMessage_ExtractsPathAndQuery()
    {
        // Arrange
        const long timestampMs = 1704067200000;
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.kalshi.com/trade-api/v2/markets?limit=10");

        // Act
        var result = CanonicalRequestBuilder.Build(timestampMs, request, ReadOnlySpan<byte>.Empty);

        // Assert
        var expected = "1704067200000\nGET\n/trade-api/v2/markets?limit=10\n";
        Encoding.UTF8.GetString(result).Should().Be(expected);
    }

    [Fact]
    public void Build_WithHttpRequestMessage_NullUri_DefaultsToSlash()
    {
        // Arrange
        const long timestampMs = 1704067200000;
        var request = new HttpRequestMessage(HttpMethod.Get, (Uri?)null);

        // Act
        var result = CanonicalRequestBuilder.Build(timestampMs, request, ReadOnlySpan<byte>.Empty);

        // Assert
        var expected = "1704067200000\nGET\n/\n";
        Encoding.UTF8.GetString(result).Should().Be(expected);
    }

    [Fact]
    public void Build_DeleteRequest_FormatsCorrectly()
    {
        // Arrange
        const long timestampMs = 1704067200000;
        const string method = "DELETE";
        const string path = "/trade-api/v2/portfolio/orders/order-123";

        // Act
        var result = CanonicalRequestBuilder.Build(timestampMs, method, path, ReadOnlySpan<byte>.Empty);

        // Assert
        var expected = "1704067200000\nDELETE\n/trade-api/v2/portfolio/orders/order-123\n";
        Encoding.UTF8.GetString(result).Should().Be(expected);
    }

    [Fact]
    public void Build_PutRequestWithBody_FormatsCorrectly()
    {
        // Arrange
        const long timestampMs = 1704067200000;
        const string method = "PUT";
        const string path = "/trade-api/v2/portfolio/orders/order-123";
        var body = Encoding.UTF8.GetBytes("{\"count\":5}");

        // Act
        var result = CanonicalRequestBuilder.Build(timestampMs, method, path, body);

        // Assert
        var expected = "1704067200000\nPUT\n/trade-api/v2/portfolio/orders/order-123\n{\"count\":5}";
        Encoding.UTF8.GetString(result).Should().Be(expected);
    }

    [Fact]
    public void Build_WithUtf8BodyContent_PreservesBytes()
    {
        // Arrange
        const long timestampMs = 1704067200000;
        const string method = "POST";
        const string path = "/test";
        var body = Encoding.UTF8.GetBytes("{\"name\":\"Test ñ\"}");

        // Act
        var result = CanonicalRequestBuilder.Build(timestampMs, method, path, body);

        // Assert
        var resultStr = Encoding.UTF8.GetString(result);
        resultStr.Should().EndWith("{\"name\":\"Test ñ\"}");
    }
}
