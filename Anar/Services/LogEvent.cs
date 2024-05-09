namespace Anar.Services;

/// <summary>
/// Constants for logging service events.
/// </summary>
internal static class LogEvent
{
    // 1000-1999 Debug

    // 2000-2999 Info
    public const int PollingStarted = 2001;
    public const int CurrentOutput = 2002;
    public const int ShuttingDown = 2003;
    public const int ArrayLayoutCount = 2004;

    // 3000-3999 Warning
    public const int GetInvertersFailed = 3001;
    public const int WriteInvertersFailed = 3002;
    public const int NoArrayLayout = 3003;

    // 4000-5000 Error
    public const int WorkerError = 4001;
}