namespace Anar.Services;

internal static class LogEvents
{
    // ERRORS 2000-2999
    public static readonly EventId CertificateThumbprintMismatch = 2001;
    public static readonly EventId LayoutFileNotFound = 2002;
    public static readonly EventId LayoutFileFormatError = 2003;

    public static readonly EventId WorkerExecuteError = 2004;
    public static readonly EventId NotifyExecuteError = 2005;

    // WARNINGS 3000-3999
    public static readonly EventId GetInvertersAuthorizationError = 3001;
    public static readonly EventId GetInvertersThumbprintError = 3002;
    public static readonly EventId GetInvertersError = 3003;


    public static readonly EventId NotifySendError = 3005;
    public static readonly EventId InfluxDbWriteError = 3006;

    // INFOS 4000-4999
    public static readonly EventId CurrentOutput = 4001;
    public static readonly EventId NoLayoutFile = 4002;
    public static readonly EventId LayoutFileLoaded = 4003;
    public static readonly EventId NotifySending = 4004;

    // DEBUGS
    public static readonly EventId GetInverters = 5001;
    public static readonly EventId WrotePoints = 5002;
}