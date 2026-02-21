using System.Globalization;
using System.Net;
using FluentAssertions;
using KalshiSharp.Auth;
using KalshiSharp.Tests.Auth;
using KalshiSharp.Configuration;
using KalshiSharp.Http;
using KalshiSharp.Models.Enums;
using KalshiSharp.Rest.Portfolio;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace KalshiSharp.Tests.Rest;

/// <summary>
/// HTTP contract tests for the Portfolio client.
/// </summary>
public sealed class PortfolioClientTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly PortfolioClient _portfolioClient;
    private readonly IKalshiRequestSigner _signer;

    public PortfolioClientTests()
    {
        _server = WireMockServer.Start();

        var options = Options.Create(new KalshiClientOptions
        {
            ApiKey = "test-api-key",
            ApiSecret = "test-api-secret",
            BaseUri = new Uri(_server.Url!),
            Timeout = TimeSpan.FromSeconds(5)
        });

        _signer = new MockRequestSigner(options.Value.ApiKey, options.Value.ApiSecret);
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

        _portfolioClient = new PortfolioClient(kalshiHttpClient);
    }

    public void Dispose()
    {
        _server.Dispose();
        (_signer as IDisposable)?.Dispose();
    }

    [Fact]
    public async Task GetBalanceAsync_ReturnsBalance()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/portfolio/balance")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {
                        "balance": 100000,
                        "portfolio_value": 25000,
                        "updated_ts": 1704067200
                    }
                    """));

        // Act
        var result = await _portfolioClient.GetBalanceAsync();

        // Assert
        result.Should().NotBeNull();
        result.Balance.Should().Be(100000);
        result.PortfolioValue.Should().Be(25000);
        result.UpdatedTs.Should().Be(1704067200);
    }

    [Fact]
    public async Task ListPositionsAsync_ReturnsPositions()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/portfolio/positions")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {
                        "market_positions": [
                            {
                                "ticker": "TICKER-1",
                                "total_traded": 500,
                                "total_traded_dollars": "5.00",
                                "position": 100,
                                "position_fp": "100.00",
                                "market_exposure": 100,
                                "market_exposure_dollars": "1.00",
                                "realized_pnl": 0,
                                "realized_pnl_dollars": "0.00",
                                "resting_orders_count": 0,
                                "fees_paid": 10,
                                "fees_paid_dollars": "0.10",
                                "last_updated_ts": "2026-02-21T10:00:00Z"
                            }
                        ],
                        "event_positions": [
                            {
                                "event_ticker": "EVENT-1",
                                "total_cost": 500,
                                "total_cost_dollars": "5.00",
                                "total_cost_shares": 10,
                                "total_cost_shares_fp": "10.00",
                                "event_exposure": 100,
                                "event_exposure_dollars": "1.00",
                                "realized_pnl": 0,
                                "realized_pnl_dollars": "0.00",
                                "fees_paid": 10,
                                "fees_paid_dollars": "0.10"
                            }
                        ],
                        "cursor": "next-cursor"
                    }
                    """));

        // Act
        var result = await _portfolioClient.ListPositionsAsync();

        // Assert
        result.Should().NotBeNull();
        result.MarketPositions.Should().HaveCount(1);
        result.Cursor.Should().Be("next-cursor");
        result.HasMore.Should().BeTrue();

        var marketPosition = result.MarketPositions[0];
        marketPosition.Ticker.Should().Be("TICKER-1");
        marketPosition.TotalTraded.Should().Be(500);
        marketPosition.TotalTradedDollars.Should().Be("5.00");
        marketPosition.Position.Should().Be(100);
        marketPosition.PositionFp.Should().Be("100.00");
        marketPosition.MarketExposure.Should().Be(100);
        marketPosition.MarketExposureDollars.Should().Be("1.00");
        marketPosition.RealizedPnl.Should().Be(0);
        marketPosition.RealizedPnlDollars.Should().Be("0.00");
        marketPosition.FeesPaid.Should().Be(10);
        marketPosition.FeesPaidDollars.Should().Be("0.10");
        marketPosition.LastUpdated.Should().Be(DateTimeOffset.Parse("2026-02-21T10:00:00Z", CultureInfo.InvariantCulture));

        result.EventPositions.Should().HaveCount(1);
        var eventPosition = result.EventPositions.First();
        eventPosition.EventTicker.Should().Be("EVENT-1");
        eventPosition.TotalCost.Should().Be(500);
        eventPosition.TotalCostDollars.Should().Be("5.00");
        eventPosition.TotalCostShares.Should().Be(10);
        eventPosition.TotalCostSharesFp.Should().Be("10.00");
        eventPosition.EventExposure.Should().Be(100);
        eventPosition.EventExposureDollars.Should().Be("1.00");
        eventPosition.RealizedPnl.Should().Be(0);
        eventPosition.RealizedPnlDollars.Should().Be("0.00");
        eventPosition.FeesPaid.Should().Be(10);
        eventPosition.FeesPaidDollars.Should().Be("0.10");
    }

    [Fact]
    public async Task ListPositionsAsync_WithPagination_IncludesQueryParams()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/portfolio/positions")
                .WithParam("cursor", "page-2")
                .WithParam("limit", "10")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"market_positions": [], "event_positions": [], "cursor": null}"""));

        // Act
        var result = await _portfolioClient.ListPositionsAsync(cursor: "page-2", limit: 10);

        // Assert
        result.Should().NotBeNull();
        result.MarketPositions.Should().BeEmpty();
        result.EventPositions.Should().BeEmpty();
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task ListPositionsAsync_WithTickerFilter_IncludesQueryParam()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/portfolio/positions")
                .WithParam("ticker", "SPECIFIC-TICKER")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {
                        "market_positions": [
                            {
                                "ticker": "SPECIFIC-TICKER",
                                "total_traded": 250,
                                "total_traded_dollars": "2.50",
                                "position": 50,
                                "position_fp": "50.00",
                                "market_exposure": 50,
                                "market_exposure_dollars": "0.50",
                                "realized_pnl": 0,
                                "realized_pnl_dollars": "0.00",
                                "resting_orders_count": 0,
                                "fees_paid": 5,
                                "fees_paid_dollars": "0.05",
                                "last_updated_ts": "2026-01-10T10:00:00Z"
                            }
                        ],
                        "event_positions": [],
                        "cursor": null
                    }
                    """));

        // Act
        var result = await _portfolioClient.ListPositionsAsync(ticker: "SPECIFIC-TICKER");

        // Assert
        result.Should().NotBeNull();
        result.MarketPositions.Should().HaveCount(1);
        result.MarketPositions[0].Ticker.Should().Be("SPECIFIC-TICKER");
    }

    [Fact]
    public async Task ListPositionsAsync_WithEventTickerFilter_IncludesQueryParam()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/portfolio/positions")
                .WithParam("event_ticker", "EVENT-123")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"market_positions": [], "event_positions": [], "cursor": null}"""));

        // Act
        var result = await _portfolioClient.ListPositionsAsync(eventTicker: "EVENT-123");

        // Assert
        result.Should().NotBeNull();
        _server.LogEntries.Should().HaveCount(1);
    }

    [Fact]
    public async Task ListFillsAsync_ReturnsFills()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/portfolio/fills")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    {
                        "fills": [
                            {
                                "trade_id": "trade-123",
                                "order_id": "order-456",
                                "ticker": "TICKER-1",
                                "side": "yes",
                                "action": "buy",
                                "count": 5,
                                "yes_price": 50,
                                "no_price": 50,
                                "is_taker": true,
                                "created_time": "2026-01-10T10:00:00Z"
                            }
                        ],
                        "cursor": "fill-cursor"
                    }
                    """));

        // Act
        var result = await _portfolioClient.ListFillsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Cursor.Should().Be("fill-cursor");
        result.HasMore.Should().BeTrue();

        var fill = result.Items[0];
        fill.TradeId.Should().Be("trade-123");
        fill.OrderId.Should().Be("order-456");
        fill.Ticker.Should().Be("TICKER-1");
        fill.Side.Should().Be(OrderSide.Yes);
        fill.Action.Should().Be("buy");
        fill.Count.Should().Be(5);
        fill.YesPrice.Should().Be(50);
        fill.NoPrice.Should().Be(50);
        fill.IsTaker.Should().BeTrue();
        fill.CreatedTime.Should().Be(DateTimeOffset.Parse("2026-01-10T10:00:00Z", CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task ListFillsAsync_WithPagination_IncludesQueryParams()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/portfolio/fills")
                .WithParam("cursor", "fill-page-2")
                .WithParam("limit", "25")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"fills": [], "cursor": null}"""));

        // Act
        var result = await _portfolioClient.ListFillsAsync(cursor: "fill-page-2", limit: 25);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task ListFillsAsync_WithTickerFilter_IncludesQueryParam()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/portfolio/fills")
                .WithParam("ticker", "FILTERED-TICKER")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"fills": [], "cursor": null}"""));

        // Act
        var result = await _portfolioClient.ListFillsAsync(ticker: "FILTERED-TICKER");

        // Assert
        result.Should().NotBeNull();
        _server.LogEntries.Should().HaveCount(1);
    }

    [Fact]
    public async Task ListFillsAsync_WithOrderIdFilter_IncludesQueryParam()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/portfolio/fills")
                .WithParam("order_id", "specific-order")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"fills": [], "cursor": null}"""));

        // Act
        var result = await _portfolioClient.ListFillsAsync(orderId: "specific-order");

        // Assert
        result.Should().NotBeNull();
        _server.LogEntries.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetBalanceAsync_IncludesAuthHeaders()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/trade-api/v2/portfolio/balance")
                .WithHeader(MockRequestSigner.AccessKeyHeader, "test-api-key")
                .WithHeader(MockRequestSigner.AccessTimestampHeader, "*")
                .WithHeader(MockRequestSigner.AccessSignatureHeader, "*")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"balance": 100000, "portfolio_value": 25000, "updated_ts": 1704067200}"""));

        // Act
        var result = await _portfolioClient.GetBalanceAsync();

        // Assert
        result.Should().NotBeNull();
        _server.LogEntries.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetBalanceAsync_SupportsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _portfolioClient.GetBalanceAsync(cts.Token));
    }

    [Fact]
    public async Task ListPositionsAsync_SupportsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _portfolioClient.ListPositionsAsync(cancellationToken: cts.Token));
    }

    [Fact]
    public async Task ListFillsAsync_SupportsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _portfolioClient.ListFillsAsync(cancellationToken: cts.Token));
    }
}
