namespace Anar.Services.Locator;

internal sealed record LocatorOptions
{
    /// <summary>
    /// Name of file containing array layout.  Optional but must point to
    /// existing file if specified.
    /// </summary>
    public string LayoutFile { get; init; } = string.Empty;
}