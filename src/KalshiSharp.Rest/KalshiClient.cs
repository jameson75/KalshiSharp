using KalshiSharp.Core.Http;
using KalshiSharp.Rest.Events;
using KalshiSharp.Rest.Exchange;
using KalshiSharp.Rest.Markets;
using KalshiSharp.Rest.Orders;
using KalshiSharp.Rest.Portfolio;
using KalshiSharp.Rest.Users;

namespace KalshiSharp.Rest;

/// <summary>
/// Root client implementation for the Kalshi API.
/// </summary>
public sealed class KalshiClient : IKalshiClient
{
    private readonly IExchangeClient _exchange;
    private readonly IMarketClient _markets;
    private readonly IEventClient _events;
    private readonly IOrderClient _orders;
    private readonly IPortfolioClient _portfolio;
    private readonly IUserClient _users;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="KalshiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for API requests.</param>
    public KalshiClient(IKalshiHttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        _exchange = new ExchangeClient(httpClient);
        _markets = new MarketClient(httpClient);
        _events = new EventClient(httpClient);
        _orders = new OrderClient(httpClient);
        _portfolio = new PortfolioClient(httpClient);
        _users = new UserClient(httpClient);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KalshiClient"/> class with explicit sub-clients.
    /// Used for DI scenarios where sub-clients are injected directly.
    /// </summary>
    /// <param name="exchange">The exchange client.</param>
    /// <param name="markets">The markets client.</param>
    /// <param name="events">The events client.</param>
    /// <param name="orders">The orders client.</param>
    /// <param name="portfolio">The portfolio client.</param>
    /// <param name="users">The users client.</param>
    public KalshiClient(
        IExchangeClient exchange,
        IMarketClient markets,
        IEventClient events,
        IOrderClient orders,
        IPortfolioClient portfolio,
        IUserClient users)
    {
        _exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
        _markets = markets ?? throw new ArgumentNullException(nameof(markets));
        _events = events ?? throw new ArgumentNullException(nameof(events));
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
        _portfolio = portfolio ?? throw new ArgumentNullException(nameof(portfolio));
        _users = users ?? throw new ArgumentNullException(nameof(users));
    }

    /// <inheritdoc />
    public IExchangeClient Exchange
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _exchange;
        }
    }

    /// <inheritdoc />
    public IMarketClient Markets
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _markets;
        }
    }

    /// <inheritdoc />
    public IEventClient Events
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _events;
        }
    }

    /// <inheritdoc />
    public IOrderClient Orders
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _orders;
        }
    }

    /// <inheritdoc />
    public IPortfolioClient Portfolio
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _portfolio;
        }
    }

    /// <inheritdoc />
    public IUserClient Users
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _users;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
