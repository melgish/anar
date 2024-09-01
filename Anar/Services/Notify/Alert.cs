namespace Anar.Services.Notify;

internal record Alert;
internal sealed record SimpleAlert(string Message) : Alert;
internal sealed record ThumbprintAlert(string Expected, string Actual) : Alert;
internal sealed record AuthenticationAlert(string Message) : Alert;