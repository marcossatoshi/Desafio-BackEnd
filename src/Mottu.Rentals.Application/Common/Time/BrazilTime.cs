namespace Mottu.Rentals.Application.Common.Time;

public static class BrazilTime
{
    private static readonly string[] TimeZoneIds = new[]
    {
        "E. South America Standard Time", // Windows
        "America/Sao_Paulo"               // Linux/macOS
    };

    private static TimeZoneInfo ResolveTimeZone()
    {
        foreach (var id in TimeZoneIds)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(id);
                if (tz != null) return tz;
            }
            catch
            {
                // try next
            }
        }
        return TimeZoneInfo.Utc; // fallback
    }

    private static readonly Lazy<TimeZoneInfo> SaoPaulo = new(ResolveTimeZone);

    public static DateTime Now()
    {
        var utcNow = DateTime.UtcNow;
        return TimeZoneInfo.ConvertTimeFromUtc(utcNow, SaoPaulo.Value);
    }

    public static DateOnly Today()
    {
        return DateOnly.FromDateTime(Now().Date);
    }
}


