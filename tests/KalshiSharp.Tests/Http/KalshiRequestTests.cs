using FluentAssertions;
using KalshiSharp.Core.Http;
using Xunit;

namespace KalshiSharp.Tests.Http;

public sealed class KalshiRequestTests
{
    [Fact]
    public void BuildRelativeUri_WithNoQueryParams_ReturnsPath()
    {
        // Arrange
        var request = new KalshiRequest
        {
            Method = HttpMethod.Get,
            Path = "/trade-api/v2/markets"
        };

        // Act
        var uri = request.BuildRelativeUri();

        // Assert
        uri.Should().Be("/trade-api/v2/markets");
    }

    [Fact]
    public void BuildRelativeUri_WithQueryParams_AppendsQueryString()
    {
        // Arrange
        var request = new KalshiRequest
        {
            Method = HttpMethod.Get,
            Path = "/markets",
            QueryParameters = new Dictionary<string, string?>
            {
                ["limit"] = "100",
                ["status"] = "open"
            }
        };

        // Act
        var uri = request.BuildRelativeUri();

        // Assert
        uri.Should().StartWith("/markets?");
        uri.Should().Contain("limit=100");
        uri.Should().Contain("status=open");
    }

    [Fact]
    public void BuildRelativeUri_WithNullQueryParams_ExcludesNulls()
    {
        // Arrange
        var request = new KalshiRequest
        {
            Method = HttpMethod.Get,
            Path = "/markets",
            QueryParameters = new Dictionary<string, string?>
            {
                ["limit"] = "100",
                ["cursor"] = null,
                ["status"] = "open"
            }
        };

        // Act
        var uri = request.BuildRelativeUri();

        // Assert
        uri.Should().Contain("limit=100");
        uri.Should().Contain("status=open");
        uri.Should().NotContain("cursor");
    }

    [Fact]
    public void BuildRelativeUri_WithAllNullQueryParams_ReturnsPath()
    {
        // Arrange
        var request = new KalshiRequest
        {
            Method = HttpMethod.Get,
            Path = "/markets",
            QueryParameters = new Dictionary<string, string?>
            {
                ["cursor"] = null,
                ["filter"] = null
            }
        };

        // Act
        var uri = request.BuildRelativeUri();

        // Assert
        uri.Should().Be("/markets");
    }

    [Fact]
    public void BuildRelativeUri_WithSpecialCharacters_EncodesCorrectly()
    {
        // Arrange
        var request = new KalshiRequest
        {
            Method = HttpMethod.Get,
            Path = "/markets",
            QueryParameters = new Dictionary<string, string?>
            {
                ["query"] = "hello world",
                ["symbol"] = "USD/EUR"
            }
        };

        // Act
        var uri = request.BuildRelativeUri();

        // Assert
        uri.Should().Contain("query=hello%20world");
        uri.Should().Contain("symbol=USD%2FEUR");
    }

    [Fact]
    public void BuildRelativeUri_WithEmptyQueryParams_ReturnsPath()
    {
        // Arrange
        var request = new KalshiRequest
        {
            Method = HttpMethod.Get,
            Path = "/markets",
            QueryParameters = new Dictionary<string, string?>()
        };

        // Act
        var uri = request.BuildRelativeUri();

        // Assert
        uri.Should().Be("/markets");
    }
}
