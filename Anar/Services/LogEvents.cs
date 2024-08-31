namespace Anar.Services;

internal static class LogEvents
{
    // ERRORS 2000-2999
    public static readonly EventId CertificateThumbprintMismatch = 2001;
    public static readonly EventId LayoutFileNotFound = 2002;
    public static readonly EventId LayoutFileFormatError = 2003;

    // WARNINGS 3000-3999
    public static readonly EventId GetInvertersFailed = 3001;

    // INFOS 4000-4999
    public static readonly EventId CurrentOutput = 4001;
    public static readonly EventId NoLayoutFile = 4002;
    public static readonly EventId LayoutFileLoaded = 4003;

    // DEBUGS

}