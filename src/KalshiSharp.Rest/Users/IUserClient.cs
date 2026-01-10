using KalshiSharp.Models.Responses;

namespace KalshiSharp.Rest.Users;

/// <summary>
/// Client for user profile operations.
/// </summary>
public interface IUserClient
{
    /// <summary>
    /// Retrieves the authenticated user's profile.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user profile details.</returns>
    Task<UserResponse> GetMeAsync(CancellationToken cancellationToken = default);
}
