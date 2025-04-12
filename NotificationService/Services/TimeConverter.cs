namespace NotificationService.Services;

public class TimeConverter
{
 
    
    public DateTime ConvertToUtc(DateTime dateTime,  string timezone)
    {
        try
        {
            //_logger.LogInformation("Converting {Time} to UTC using TimeZone: {TimeZone}", dateTime, timezone);
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);

            DateTime unspecified = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(unspecified, tz);

            return utcTime;
        }
        catch (TimeZoneNotFoundException)
        {
            //_logger.LogError("TimeZoneNotFound: {TimeZone}", timezone);
            throw new ArgumentException($"Nie znaleziono strefy czasowej: {timezone}");
        }
        catch (InvalidTimeZoneException)
        {
            //_logger.LogError("InvalidTimeZone: {TimeZone}", timezone);
            throw new ArgumentException($"Strefa czasowa jest nieprawidłowa lub uszkodzona: {timezone}");
        }
    }
}