using KalshiSharp.Http;
using KalshiSharp.Models.Requests;
using KalshiSharp.Models.Responses;

namespace KalshiSharp.Rest.Historical
{
    public class HistoricalClient : IHistoricalClient
    {
        private const string BasePath = "/trade-api/v2/historical";

        private readonly IKalshiHttpClient _httpClient;

        public HistoricalClient(IKalshiHttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <inheritdoc />
        public Task<HistoricalCutoffResponse> GetHistoricalCutoff(CancellationToken cancellationToken = default)
        {
            var request = new KalshiRequest
            {
                Method = HttpMethod.Get,
                Path = $"{BasePath}/cutoff"
            };

            return _httpClient.SendAsync<HistoricalCutoffResponse>(request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<MarketsResponse> ListHistoricalMarketsAsync(HistoricalMarketQuery? query = null, CancellationToken cancellationToken = default)
        {
            var queryString = query?.ToQueryString() ?? string.Empty;

            var request = new KalshiRequest
            {
                Method = HttpMethod.Get,
                Path = $"{BasePath}/markets{queryString}"
            };

            return _httpClient.SendAsync<MarketsResponse>(request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<HistoricalMarketCandlesticksResponse> GetHistoricalMarketCandlesticks(string ticker, MarketCandlesticksQuery query, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ticker);

            var queryString = query.ToQueryString();

            var request = new KalshiRequest
            {
                Method = HttpMethod.Get,
                Path = $"{BasePath}/markets/{ticker}/candlesticks{queryString}"
            };

            return _httpClient.SendAsync<HistoricalMarketCandlesticksResponse>(request, cancellationToken);
        }
    }
}
