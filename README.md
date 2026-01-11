# KalshiSharp

A production-grade .NET 8 SDK for the [Kalshi](https://kalshi.com) prediction market API.

## Features

- **Full API Coverage**: REST endpoints for exchange, markets, events, orders, portfolio, and users
- **Real-time WebSocket**: Order book, trade, and order update subscriptions with auto-reconnect
- **Async-First**: All operations are async/await with proper cancellation support
- **Thread-Safe**: Safe for concurrent use from multiple threads
- **Strongly Typed**: Complete type coverage with nullable reference types enabled
- **Automatic Signing**: RSA-PSS request signing handled transparently
- **Resilience**: Built-in retry with exponential backoff, rate limiting, and circuit breaker
- **Observability**: OpenTelemetry tracing and metrics integration
- **Dependency Injection**: First-class support for `IServiceCollection`

## Installation

```bash
dotnet add package KalshiSharp
```

## Quick Start

### Basic Usage

```csharp
using KalshiSharp.Configuration;
using KalshiSharp.Rest;

// Create a client with your API credentials
using var client = new KalshiClient(new KalshiClientOptions
{
    ApiKey = "your-api-key",
    ApiSecret = "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----", // RSA private key in PEM format
    Environment = KalshiEnvironment.Demo // or Production
});

var status = await client.Exchange.GetStatusAsync();
Console.WriteLine($"Trading Active: {status.TradingActive}");
```

### With Dependency Injection (ASP.NET Core)

For applications using dependency injection:

```csharp
using KalshiSharp.Configuration;
using KalshiSharp.DependencyInjection;

// In Program.cs or Startup.cs
services.AddKalshiClient(options =>
{
    options.ApiKey = configuration["Kalshi:ApiKey"]!;
    options.ApiSecret = configuration["Kalshi:ApiSecret"]!;
    options.Environment = KalshiEnvironment.Production;
});

// Inject IKalshiClient directly
public class MyService(IKalshiClient client)
{
    public async Task DoSomethingAsync()
    {
        var markets = await client.Markets.ListMarketsAsync();
    }
}
```

### Get Exchange Schedule

```csharp
var schedule = await client.Exchange.GetScheduleAsync();
Console.WriteLine($"Standard hours entries: {schedule.Schedule.StandardHours.Count}");
Console.WriteLine($"Maintenance windows: {schedule.Schedule.MaintenanceWindows.Count}");
```

### List Markets with Pagination

```csharp
using KalshiSharp.Models.Enums;
using KalshiSharp.Models.Requests;

var query = new MarketQuery
{
    Limit = 10,
    Status = MarketStatus.Active
};

var page1 = await client.Markets.ListMarketsAsync(query);

foreach (var market in page1.Items)
{
    Console.WriteLine($"{market.Ticker}: {market.Title}");
    Console.WriteLine($"  Yes: {market.YesBid}c / {market.YesAsk}c");
}

// Fetch next page
if (page1.HasMore)
{
    var page2 = await client.Markets.ListMarketsAsync(query with { Cursor = page1.Cursor });
}
```

### Get Order Book

```csharp
var orderBook = await client.Markets.GetOrderBookAsync("TICKER-ABC");

Console.WriteLine($"Yes levels: {orderBook.Orderbook.Yes.Count}");
Console.WriteLine($"No levels: {orderBook.Orderbook.No.Count}");

// Each level is [price, quantity]
foreach (var level in orderBook.Orderbook.Yes)
{
    Console.WriteLine($"  {level[0]}c: {level[1]} contracts");
}
```

### Place and Cancel Orders

```csharp
using KalshiSharp.Models.Requests;

// Create a limit order
var request = new CreateOrderRequest
{
    Ticker = "TICKER-ABC",
    Side = OrderSide.Yes,
    Action = "buy",
    Type = OrderType.Limit,
    Count = 10,
    YesPrice = 45, // 45 cents
    ClientOrderId = Guid.NewGuid().ToString("N")
};

var order = await client.Orders.CreateOrderAsync(request);
Console.WriteLine($"Order ID: {order.OrderId}, Status: {order.Status}");

// Cancel the order
var cancelled = await client.Orders.CancelOrderAsync(order.OrderId);
```

### Portfolio Information

```csharp
// Get balance
var balance = await client.Portfolio.GetBalanceAsync();
Console.WriteLine($"Balance: ${balance.Balance / 100m:F2}");

// List positions
var positions = await client.Portfolio.ListPositionsAsync();
foreach (var position in positions.Items)
{
    Console.WriteLine($"{position.Ticker}: {position.Position} contracts");
}

// List fills
var fills = await client.Portfolio.ListFillsAsync();
```

### WebSocket Real-Time Updates

```csharp
using KalshiSharp.Configuration;
using KalshiSharp.WebSockets;
using KalshiSharp.WebSockets.Subscriptions;
using KalshiSharp.Models.WebSocket;

// Create WebSocket client
await using var wsClient = new KalshiWebSocketClient(new KalshiClientOptions
{
    ApiKey = "your-api-key",
    ApiSecret = "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----",
    Environment = KalshiEnvironment.Production
});

// Connect and subscribe
await wsClient.ConnectAsync();
await wsClient.SubscribeAsync(OrderBookSubscription.ForMarkets("TICKER-ABC"));
await wsClient.SubscribeAsync(TradeSubscription.ForMarkets("TICKER-ABC"));

// Process messages
await foreach (var message in wsClient.Messages)
{
    switch (message)
    {
        case OrderBookSnapshotMessage snapshot:
            Console.WriteLine($"Snapshot: {snapshot.MarketTicker}");
            break;
        case OrderBookUpdate update:
            Console.WriteLine($"Delta: {update.Price}c, {update.Delta}");
            break;
        case TradeUpdate trade:
            Console.WriteLine($"Trade: {trade.Count} @ {trade.YesPrice}c");
            break;
    }
}
```

## Error Handling

The SDK provides strongly-typed exceptions for different error scenarios:

```csharp
using KalshiSharp.Errors;

try
{
    var market = await client.Markets.GetMarketAsync("INVALID");
}
catch (KalshiNotFoundException)
{
    Console.WriteLine("Market not found");
}
catch (KalshiAuthException)
{
    Console.WriteLine("Invalid credentials");
}
catch (KalshiRateLimitException ex)
{
    Console.WriteLine($"Rate limited, retry after: {ex.RetryAfter}");
}
catch (KalshiValidationException ex)
{
    foreach (var error in ex.ValidationErrors ?? new())
    {
        Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value)}");
    }
}
catch (KalshiException ex)
{
    Console.WriteLine($"API error: {ex.StatusCode} - {ex.Message}");
}
```

## Configuration Options

| Option | Default | Description |
|--------|---------|-------------|
| `ApiKey` | Required | Your Kalshi API key ID |
| `ApiSecret` | Required | Your RSA private key in PEM format |
| `Environment` | `Production` | `Production` or `Demo` |
| `BaseUri` | Auto | Override base URI |
| `Timeout` | 30s | HTTP request timeout |
| `ClockSkewTolerance` | 30s | Tolerance for timestamp validation |
| `EnableRateLimiting` | true | Enable client-side rate limiting |

## Project Structure

```
KalshiSharp/
├── KalshiSharp/                   # Main SDK library
│   ├── Auth/                      # RSA-PSS request signing
│   ├── Configuration/             # Client options
│   ├── DependencyInjection/       # IServiceCollection extensions
│   ├── Errors/                    # Exception types
│   ├── Http/                      # HTTP client pipeline
│   ├── Models/                    # DTOs and enums
│   │   ├── Enums/
│   │   ├── Requests/
│   │   ├── Responses/
│   │   └── WebSocket/
│   ├── RateLimiting/              # Token bucket limiter
│   ├── Rest/                      # REST API clients
│   │   ├── Exchange/
│   │   ├── Markets/
│   │   ├── Events/
│   │   ├── Orders/
│   │   ├── Portfolio/
│   │   └── Users/
│   ├── Serialization/             # JSON converters
│   └── WebSockets/                # WebSocket client
│       ├── Connections/
│       ├── ReconnectPolicy/
│       └── Subscriptions/
├── KalshiSharp.Tests/             # Unit and integration tests
└── KalshiSharp.Examples/          # Example console app
```

## Running Examples

```bash
# Set credentials via user-secrets (recommended)
cd KalshiSharp.Examples
dotnet user-secrets set "Kalshi:ApiKey" "your-api-key-id"
dotnet user-secrets set "Kalshi:ApiSecret" "-----BEGIN PRIVATE KEY-----
...your PEM key...
-----END PRIVATE KEY-----"

# Or via environment variables (double underscore for nesting)
export KALSHI__APIKEY="your-api-key-id"
export KALSHI__APISECRET="-----BEGIN PRIVATE KEY-----..."

# Run specific example
dotnet run --project KalshiSharp.Examples -- exchange
dotnet run --project KalshiSharp.Examples -- markets
dotnet run --project KalshiSharp.Examples -- order
dotnet run --project KalshiSharp.Examples -- websocket

# Run all examples
dotnet run --project KalshiSharp.Examples -- all
```

## Requirements

- .NET 8.0 or later
- Valid Kalshi API credentials (obtain from [Kalshi API Settings](https://kalshi.com/account/api))

## License

MIT
