using WeatherApp.Models;

namespace WeatherApp.Services;

public static class WeatherDataProcessor
{
    public static List<DayForecast> GetDailyForecasts(ForecastResponse forecast)
    {
        var dailyGroups = forecast.List
            .GroupBy(item => DateTimeOffset.FromUnixTimeSeconds(item.Timestamp).LocalDateTime.Date)
            .Take(5)
            .ToList();

        var result = new List<DayForecast>();

        foreach (var group in dailyGroups)
        {
            var items = group.ToList();
            var mainWeather = items.First().Weather.FirstOrDefault();

            result.Add(new DayForecast
            {
                Date = group.Key,
                TempMin = items.Min(i => i.Main.TempMin),
                TempMax = items.Max(i => i.Main.TempMax),
                Description = mainWeather?.Description ?? "",
                Icon = mainWeather?.Icon ?? "",
                WeatherEmoji = GetWeatherEmoji(mainWeather?.Id ?? 0),
                PrecipitationChance = items.Max(i => i.Pop) * 100
            });
        }

        return result;
    }

    public static List<HourlyForecast> GetHourlyForecasts(ForecastResponse forecast, int hours = 24)
    {
        return forecast.List
            .Take(hours / 3)
            .Select(item => new HourlyForecast
            {
                DateTime = DateTimeOffset.FromUnixTimeSeconds(item.Timestamp).LocalDateTime,
                Temperature = item.Main.Temp,
                Icon = item.Weather.FirstOrDefault()?.Icon ?? ""
            })
            .ToList();
    }

    public static string GetWeatherEmoji(int weatherId)
    {
        return weatherId switch
        {
            >= 200 and < 300 => "⛈️",  // Thunderstorm
            >= 300 and < 400 => "🌦️",  // Drizzle
            >= 500 and < 600 => "🌧️",  // Rain
            >= 600 and < 700 => "❄️",  // Snow
            >= 700 and < 800 => "🌫️",  // Atmosphere (fog, mist)
            800 => "☀️",               // Clear sky
            801 => "🌤️",              // Few clouds
            802 => "⛅",              // Scattered clouds
            803 or 804 => "☁️",       // Broken/overcast clouds
            _ => "🌡️"
        };
    }

    public static string GetWindDirection(int degrees)
    {
        return degrees switch
        {
            >= 337 or < 23 => "С",
            >= 23 and < 67 => "СВ",
            >= 67 and < 113 => "В",
            >= 113 and < 157 => "ЮВ",
            >= 157 and < 203 => "Ю",
            >= 203 and < 247 => "ЮЗ",
            >= 247 and < 293 => "З",
            >= 293 and < 337 => "СЗ",
            _ => ""
        };
    }

    public static string FormatSunTime(long unixTimestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).LocalDateTime.ToString("HH:mm");
    }
}
