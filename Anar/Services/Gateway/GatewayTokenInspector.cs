namespace Anar.Services.Gateway;

using Microsoft.Extensions.Options;
using System;
using System.Text.Json;

internal interface IGatewayTokenInspector
{
    /// <summary>
    /// Returns time remaining before the token expires. If the token has
    /// already expired, the returned value will be negative.
    /// </summary>
    /// <returns></returns>
    TimeSpan GetLifetimeRemaining();
}

/// <summary>
/// Inspects the JWT token to determine the remaining lifetime.
/// </summary>
/// <param name="options"></param>
/// <param name="timeProvider"></param>
internal class GatewayTokenInspector(
    IOptions<GatewayOptions> options,
    TimeProvider timeProvider
) : IGatewayTokenInspector
{
    private readonly DateTimeOffset _expirationDate = GetExpirationDate(options.Value.Token);

    /// <summary>
    /// Returns time remaining before the token expires. If the token has
    /// already expired, the returned value will be negative.
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetLifetimeRemaining()
        => _expirationDate - timeProvider.GetUtcNow();

    /// <summary>
    /// Gets expiration date from the supplied JWT token.
    /// </summary>s
    /// <param name="token"></param>
    private static DateTimeOffset GetExpirationDate(string token)
    {
        try
        {
            // The token is a JWT token, so we can decode it to get the
            // expiration date. The expiration date is in the "exp" claim.
            if (token?.Split('.') is [_, var base64Payload, _])
            {
                var doc = JsonDocument.Parse(Convert.FromBase64String(base64Payload));
                var root = doc.RootElement;
                if (root.TryGetProperty("exp", out var exp) && exp.TryGetInt64(out var expValue))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(expValue);
                }
            }
        }
        catch (Exception)
        {
            // Ignore any exceptions and return the minimum value.
        }

        // Treat anything unexpected as an expired token.
        return DateTimeOffset.MinValue;
    }
}