using System.Globalization;
using System.Net;
using FluentAssertions;
using KalshiSharp.Core.Auth;
using KalshiSharp.Core.Configuration;
using KalshiSharp.Core.Http;
using KalshiSharp.Rest.Exchange;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace KalshiSharp.Tests.Rest;

/// <summary>
/// HTTP contract tests for the Exchange client.
/// </summary>
public sealed class ExchangeClientTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly ExchangeClient _exchangeClient;
    private readonly IKalshiRequestSigner _signer;

    public ExchangeClientTests()
    {
        _server = WireMockServer.Start();

        var options = Options.Create(new KalshiClientOptions
        {
            ApiKey = "test-api-key",
            ApiSecret = "test-api-secret",
            BaseUri = new Uri(_server.Url!),
            Timeout = TimeSpan.FromSeconds(5)
        });

        _signer = new HmacSha256RequestSigner(options.Value.ApiKey, options.Value.ApiSecret);
        var clock = new SystemClock();

        var signingHandler = new SigningDelegatingHandler(
            _signer,
            clock,
            NullLogger<SigningDelegatingHandler>.Instance)
        {
            InnerHandler = new HttpClientHandler()
        };

        var httpClient = new HttpClient(signingHandler);
        var kalshiHttpClient = new KalshiHttpClient(
            httpClient,
            options,
            NullLogger<KalshiHttpClient>.Instance);

        _exchangeClient = new ExchangeClient(kalshiHttpClient);
    }

    public void Dispose()
    {
        _server.Dispose();
        (_signer as IDisposable)?.Dispose();
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsExchangeStatus()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/exchange/status")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {
                        "exchange_active": true,
                        "trading_active": true
                    }
                    """));

        // Act
        var result = await _exchangeClient.GetStatusAsync();

        // Assert
        result.Should().NotBeNull();
        result.ExchangeActive.Should().BeTrue();
        result.TradingActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatusAsync_WhenExchangeInactive_ReturnsCorrectStatus()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/exchange/status")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {
                        "exchange_active": false,
                        "trading_active": false
                    }
                    """));

        // Act
        var result = await _exchangeClient.GetStatusAsync();

        // Assert
        result.Should().NotBeNull();
        result.ExchangeActive.Should().BeFalse();
        result.TradingActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatusAsync_IncludesAuthHeaders()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/exchange/status")
                .WithHeader(HmacSha256RequestSigner.AccessKeyHeader, "test-api-key")
                .WithHeader(HmacSha256RequestSigner.AccessTimestampHeader, "*")
                .WithHeader(HmacSha256RequestSigner.AccessSignatureHeader, "*")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"exchange_active": true, "trading_active": true}"""));

        // Act
        var result = await _exchangeClient.GetStatusAsync();

        // Assert
        result.Should().NotBeNull();
        _server.LogEntries.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetScheduleAsync_ReturnsExchangeSchedule()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/exchange/schedule")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {
                        "schedule": [
                            {
                                "start_time": "2026-01-10T09:00:00Z",
                                "end_time": "2026-01-10T17:00:00Z"
                            },
                            {
                                "start_time": "2026-01-11T09:00:00Z",
                                "end_time": "2026-01-11T17:00:00Z",
                                "maintenance": "Scheduled maintenance"
                            }
                        ]
                    }
                    """));

        // Act
        var result = await _exchangeClient.GetScheduleAsync();

        // Assert
        result.Should().NotBeNull();
        result.Schedule.Should().HaveCount(2);

        result.Schedule[0].StartTime.Should().Be(DateTimeOffset.Parse("2026-01-10T09:00:00Z", CultureInfo.InvariantCulture));
        result.Schedule[0].EndTime.Should().Be(DateTimeOffset.Parse("2026-01-10T17:00:00Z", CultureInfo.InvariantCulture));
        result.Schedule[0].Maintenance.Should().BeNull();

        result.Schedule[1].StartTime.Should().Be(DateTimeOffset.Parse("2026-01-11T09:00:00Z", CultureInfo.InvariantCulture));
        result.Schedule[1].EndTime.Should().Be(DateTimeOffset.Parse("2026-01-11T17:00:00Z", CultureInfo.InvariantCulture));
        result.Schedule[1].Maintenance.Should().Be("Scheduled maintenance");
    }

    [Fact]
    public async Task GetScheduleAsync_WithEmptySchedule_ReturnsEmptyList()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/exchange/schedule")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"schedule": []}"""));

        // Act
        var result = await _exchangeClient.GetScheduleAsync();

        // Assert
        result.Should().NotBeNull();
        result.Schedule.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStatusAsync_SupportsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _exchangeClient.GetStatusAsync(cts.Token));
    }

    [Fact]
    public async Task GetScheduleAsync_SupportsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _exchangeClient.GetScheduleAsync(cts.Token));
    }
}
