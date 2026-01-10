using System.Text;
using FluentAssertions;
using KalshiSharp.Core.Auth;

namespace KalshiSharp.Tests.Auth;

public class HmacSha256RequestSignerTests : IDisposable
{
    // Test credentials (these are NOT real credentials, purely for testing)
    private const string TestApiKey = "test-api-key-12345";
    private const string TestApiSecret = "test-api-secret-67890-abcdef";

    private readonly HmacSha256RequestSigner _signer;

    public HmacSha256RequestSignerTests()
    {
        _signer = new HmacSha256RequestSigner(TestApiKey, TestApiSecret);
    }

    public void Dispose()
    {
        _signer.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_WithNullApiKey_ThrowsArgumentException()
    {
        var act = () => new HmacSha256RequestSigner(null!, TestApiSecret);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithEmptyApiKey_ThrowsArgumentException()
    {
        var act = () => new HmacSha256RequestSigner("", TestApiSecret);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithNullApiSecret_ThrowsArgumentException()
    {
        var act = () => new HmacSha256RequestSigner(TestApiKey, null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithEmptyApiSecret_ThrowsArgumentException()
    {
        var act = () => new HmacSha256RequestSigner(TestApiKey, "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Sign_AddsAllRequiredHeaders()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.kalshi.com/trade-api/v2/exchange/status");
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1704067200000); // 2024-01-01 00:00:00 UTC

        // Act
        _signer.Sign(request, ReadOnlySpan<byte>.Empty, timestamp);

        // Assert
        request.Headers.Should().Contain(h => h.Key == HmacSha256RequestSigner.AccessKeyHeader);
        request.Headers.Should().Contain(h => h.Key == HmacSha256RequestSigner.AccessTimestampHeader);
        request.Headers.Should().Contain(h => h.Key == HmacSha256RequestSigner.AccessSignatureHeader);
    }

    [Fact]
    public void Sign_SetsCorrectApiKey()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.kalshi.com/trade-api/v2/exchange/status");
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1704067200000);

        // Act
        _signer.Sign(request, ReadOnlySpan<byte>.Empty, timestamp);

        // Assert
        request.Headers.GetValues(HmacSha256RequestSigner.AccessKeyHeader)
            .Should().ContainSingle()
            .Which.Should().Be(TestApiKey);
    }

    [Fact]
    public void Sign_SetsCorrectTimestamp()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.kalshi.com/trade-api/v2/exchange/status");
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1704067200000);

        // Act
        _signer.Sign(request, ReadOnlySpan<byte>.Empty, timestamp);

        // Assert
        request.Headers.GetValues(HmacSha256RequestSigner.AccessTimestampHeader)
            .Should().ContainSingle()
            .Which.Should().Be("1704067200000");
    }

    [Fact]
    public void Sign_ProducesConsistentSignature_ForSameInputs()
    {
        // Arrange
        var request1 = new HttpRequestMessage(HttpMethod.Get, "https://api.kalshi.com/trade-api/v2/exchange/status");
        var request2 = new HttpRequestMessage(HttpMethod.Get, "https://api.kalshi.com/trade-api/v2/exchange/status");
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1704067200000);

        // Act
        _signer.Sign(request1, ReadOnlySpan<byte>.Empty, timestamp);
        _signer.Sign(request2, ReadOnlySpan<byte>.Empty, timestamp);

        // Assert
        var sig1 = request1.Headers.GetValues(HmacSha256RequestSigner.AccessSignatureHeader).Single();
        var sig2 = request2.Headers.GetValues(HmacSha256RequestSigner.AccessSignatureHeader).Single();
        sig1.Should().Be(sig2);
    }

    [Fact]
    public void Sign_ProducesDifferentSignatures_ForDifferentTimestamps()
    {
        // Arrange
        var request1 = new HttpRequestMessage(HttpMethod.Get, "https://api.kalshi.com/trade-api/v2/exchange/status");
        var request2 = new HttpRequestMessage(HttpMethod.Get, "https://api.kalshi.com/trade-api/v2/exchange/status");
        var timestamp1 = DateTimeOffset.FromUnixTimeMilliseconds(1704067200000);
        var timestamp2 = DateTimeOffset.FromUnixTimeMilliseconds(1704067200001);

        // Act
        _signer.Sign(request1, ReadOnlySpan<byte>.Empty, timestamp1);
        _signer.Sign(request2, ReadOnlySpan<byte>.Empty, timestamp2);

        // Assert
        var sig1 = request1.Headers.GetValues(HmacSha256RequestSigner.AccessSignatureHeader).Single();
        var sig2 = request2.Headers.GetValues(HmacSha256RequestSigner.AccessSignatureHeader).Single();
        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void Sign_ProducesDifferentSignatures_ForDifferentMethods()
    {
        // Arrange
        var getRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.kalshi.com/trade-api/v2/exchange/status");
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.kalshi.com/trade-api/v2/exchange/status");
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1704067200000);

        // Act
        _signer.Sign(getRequest, ReadOnlySpan<byte>.Empty, timestamp);
        _signer.Sign(postRequest, ReadOnlySpan<byte>.Empty, timestamp);

        // Assert
        var getSig = getRequest.Headers.GetValues(HmacSha256RequestSigner.AccessSignatureHeader).Single();
        var postSig = postRequest.Headers.GetValues(HmacSha256RequestSigner.AccessSignatureHeader).Single();
        getSig.Should().NotBe(postSig);
    }

    [Fact]
    public void Sign_ProducesDifferentSignatures_ForDifferentPaths()
    {
        // Arrange
        var request1 = new HttpRequestMessage(HttpMethod.Get, "https://api.kalshi.com/trade-api/v2/exchange/status");
        var request2 = new HttpRequestMessage(HttpMethod.Get, "https://api.kalshi.com/trade-api/v2/markets");
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1704067200000);

        // Act
        _signer.Sign(request1, ReadOnlySpan<byte>.Empty, timestamp);
        _signer.Sign(request2, ReadOnlySpan<byte>.Empty, timestamp);

        // Assert
        var sig1 = request1.Headers.GetValues(HmacSha256RequestSigner.AccessSignatureHeader).Single();
        var sig2 = request2.Headers.GetValues(HmacSha256RequestSigner.AccessSignatureHeader).Single();
        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void Sign_ProducesDifferentSignatures_ForDifferentBodies()
    {
        // Arrange
        var request1 = new HttpRequestMessage(HttpMethod.Post, "https://api.kalshi.com/trade-api/v2/portfolio/orders");
        var request2 = new HttpRequestMessage(HttpMethod.Post, "https://api.kalshi.com/trade-api/v2/portfolio/orders");
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1704067200000);
        var body1 = Encoding.UTF8.GetBytes("{\"ticker\":\"ABC\"}");
        var body2 = Encoding.UTF8.GetBytes("{\"ticker\":\"XYZ\"}");

        // Act
        _signer.Sign(request1, body1, timestamp);
        _signer.Sign(request2, body2, timestamp);

        // Assert
        var sig1 = request1.Headers.GetValues(HmacSha256RequestSigner.AccessSignatureHeader).Single();
        var sig2 = request2.Headers.GetValues(HmacSha256RequestSigner.AccessSignatureHeader).Single();
        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void Sign_ReplacesExistingHeaders()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.kalshi.com/trade-api/v2/exchange/status");
        request.Headers.TryAddWithoutValidation(HmacSha256RequestSigner.AccessKeyHeader, "old-key");
        request.Headers.TryAddWithoutValidation(HmacSha256RequestSigner.AccessTimestampHeader, "old-timestamp");
        request.Headers.TryAddWithoutValidation(HmacSha256RequestSigner.AccessSignatureHeader, "old-signature");
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1704067200000);

        // Act
        _signer.Sign(request, ReadOnlySpan<byte>.Empty, timestamp);

        // Assert
        request.Headers.GetValues(HmacSha256RequestSigner.AccessKeyHeader)
            .Should().ContainSingle()
            .Which.Should().Be(TestApiKey);
        request.Headers.GetValues(HmacSha256RequestSigner.AccessTimestampHeader)
            .Should().ContainSingle()
            .Which.Should().Be("1704067200000");
    }

    [Fact]
    public void Sign_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var signer = new HmacSha256RequestSigner(TestApiKey, TestApiSecret);
        signer.Dispose();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.kalshi.com/trade-api/v2/exchange/status");

        // Act
        var act = () => signer.Sign(request, ReadOnlySpan<byte>.Empty, DateTimeOffset.UtcNow);

        // Assert
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Sign_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _signer.Sign(null!, ReadOnlySpan<byte>.Empty, DateTimeOffset.UtcNow);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // Golden vector tests - these establish known-good signatures for regression testing
    [Theory]
    [MemberData(nameof(GoldenVectorTestCases))]
    public void Sign_GoldenVector_ProducesExpectedSignature(
        string apiKey,
        string apiSecret,
        string method,
        string url,
        string body,
        long timestampMs,
        string expectedSignature)
    {
        // Arrange
        using var signer = new HmacSha256RequestSigner(apiKey, apiSecret);
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs);
        var bodyBytes = string.IsNullOrEmpty(body) ? ReadOnlySpan<byte>.Empty : Encoding.UTF8.GetBytes(body);

        // Act
        signer.Sign(request, bodyBytes, timestamp);

        // Assert
        var actualSignature = request.Headers.GetValues(HmacSha256RequestSigner.AccessSignatureHeader).Single();
        actualSignature.Should().Be(expectedSignature);
    }

    public static TheoryData<string, string, string, string, string, long, string> GoldenVectorTestCases()
    {
        // These are computed golden vectors for regression testing
        // Canonical format: {timestamp}\n{method}\n{path}\n{body}
        return new TheoryData<string, string, string, string, string, long, string>
        {
            // GET request without body
            // Canonical: "1704067200000\nGET\n/trade-api/v2/exchange/status\n"
            {
                "test-api-key",
                "test-secret-key",
                "GET",
                "https://api.kalshi.com/trade-api/v2/exchange/status",
                "",
                1704067200000,
                "IGeDgqtsFwD/eG58ZzylEmsVa/PMK+C4cksICcq7VeQ="
            },
            // GET request with query string
            // Canonical: "1704067200000\nGET\n/trade-api/v2/markets?status=open&limit=100\n"
            {
                "test-api-key",
                "test-secret-key",
                "GET",
                "https://api.kalshi.com/trade-api/v2/markets?status=open&limit=100",
                "",
                1704067200000,
                "1gMBOFnnnPvFaPJVFkw3z65iN4A2vp2hGGZttYwtWtA="
            },
            // POST request with JSON body
            // Canonical: "1704067200000\nPOST\n/trade-api/v2/portfolio/orders\n{\"ticker\":\"BTCUSD\",\"side\":\"yes\",\"type\":\"limit\"}"
            {
                "test-api-key",
                "test-secret-key",
                "POST",
                "https://api.kalshi.com/trade-api/v2/portfolio/orders",
                "{\"ticker\":\"BTCUSD\",\"side\":\"yes\",\"type\":\"limit\"}",
                1704067200000,
                "61UF0U/Njo4AEw7VM36USVWVOPD/jkZenH8nAhrD61s="
            },
            // DELETE request
            // Canonical: "1704067200000\nDELETE\n/trade-api/v2/portfolio/orders/order-123\n"
            {
                "test-api-key",
                "test-secret-key",
                "DELETE",
                "https://api.kalshi.com/trade-api/v2/portfolio/orders/order-123",
                "",
                1704067200000,
                "RVeu8WQ/ZrX4XtXrkgVtZXOyIjWM14rTDDhVNytzWak="
            },
            // Different timestamp
            // Canonical: "1704153600000\nGET\n/trade-api/v2/exchange/status\n"
            {
                "test-api-key",
                "test-secret-key",
                "GET",
                "https://api.kalshi.com/trade-api/v2/exchange/status",
                "",
                1704153600000,
                "BIr+caJ0RdOYCHTg2gLM2dT6o2aGRERc73FVQCOGfwc="
            }
        };
    }
}
