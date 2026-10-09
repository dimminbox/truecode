namespace TrueCode.Finance.Updater.Options;

public sealed class UpdaterOptions
{
    public const string SectionName = "Updater";

    public string Url { get; init; } = "";
    public TimeSpan Interval { get; init; }
    public TimeSpan RequestTimeout { get; init; }

    internal static bool IsValid(UpdaterOptions options)
    {
        return Uri.TryCreate(options.Url, UriKind.Absolute, out var uri)
               && (uri.Scheme == "http" || uri.Scheme == "https")
               && options.Interval > TimeSpan.Zero
               && options.RequestTimeout > TimeSpan.Zero;
    }
}