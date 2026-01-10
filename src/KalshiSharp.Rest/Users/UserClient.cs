using KalshiSharp.Core.Http;
using KalshiSharp.Models.Responses;

namespace KalshiSharp.Rest.Users;

/// <summary>
/// Implementation of the user client for user profile endpoints.
/// </summary>
internal sealed class UserClient : IUserClient
{
    private const string BasePath = "/trade-api/v2/users";

    private readonly IKalshiHttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public UserClient(IKalshiHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public Task<UserResponse> GetMeAsync(CancellationToken cancellationToken = default)
    {
        var httpRequest = new KalshiRequest
        {
            Method = HttpMethod.Get,
            Path = $"{BasePath}/me"
        };

        return _httpClient.SendAsync<UserResponse>(httpRequest, cancellationToken);
    }
}
