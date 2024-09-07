namespace Anar.Services.Notify;

internal abstract record Alert;
internal sealed record SimpleAlert(string Message) : Alert;

/// <summary>
/// Alert for when certificate thumbprint does not match the configured
/// value.
/// </summary>
/// <param name="Expected">The configured value</param>
/// <param name="Actual">The certificate value</param>
internal sealed record ThumbprintAlert(string Expected, string Actual) : Alert;

/// <summary>
/// Alert for when authentication fails.
/// </summary>
/// <param name="Message"></param>
internal sealed record AuthenticationAlert(string Message) : Alert;

/// <summary>
/// Alert for when a token is about to expire, or has expired.
/// </summary>
/// <param name="ExpiresAt"></param>
internal sealed record GatewayTokenExpirationAlert(DateTimeOffset ExpiresAt) : Alert;
