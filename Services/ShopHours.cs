namespace LocalBuzz.Services;

public static class ShopHours
{
    private static readonly TimeZoneInfo IndiaTimeZone;

    static ShopHours()
    {
        try
        {
            IndiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                IndiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch
            {
                IndiaTimeZone = TimeZoneInfo.Utc;
            }
        }
    }

    public static DateTime GetCurrentIndiaTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IndiaTimeZone);
    }

    /// <summary>
    /// Computes whether a shop is currently open based on OpeningTime, ClosingTime, and WeeklyOff in India time.
    /// Handles overnight hours (ClosingTime &lt; OpeningTime) and weekly off days.
    /// </summary>
    public static bool IsOpenNow(TimeOnly openingTime, TimeOnly closingTime, string? weeklyOff)
    {
        var nowIndia = GetCurrentIndiaTime();
        var currentDay = nowIndia.DayOfWeek.ToString();

        // Check weekly off (case-insensitive)
        if (!string.IsNullOrWhiteSpace(weeklyOff) &&
            weeklyOff.Trim().Equals(currentDay, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var currentTime = TimeOnly.FromDateTime(nowIndia);

        if (openingTime == closingTime)
        {
            // 24 hours open
            return true;
        }

        if (closingTime > openingTime)
        {
            // Standard daytime hours: e.g. 09:00 to 21:00
            return currentTime >= openingTime && currentTime < closingTime;
        }
        else
        {
            // Overnight hours: e.g. 18:00 to 02:00
            // Shop is open if currentTime >= 18:00 OR currentTime < 02:00
            return currentTime >= openingTime || currentTime < closingTime;
        }
    }

    public static string FormatHours(TimeOnly openingTime, TimeOnly closingTime, string? weeklyOff)
    {
        var openStr = DateTime.Today.Add(openingTime.ToTimeSpan()).ToString("h:mm tt");
        var closeStr = DateTime.Today.Add(closingTime.ToTimeSpan()).ToString("h:mm tt");
        var hoursStr = $"{openStr} - {closeStr}";

        if (!string.IsNullOrWhiteSpace(weeklyOff))
        {
            return $"{hoursStr} (Closed {weeklyOff.Trim()})";
        }
        return hoursStr;
    }
}
