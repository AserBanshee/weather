using Newtonsoft.Json;

namespace WeatherApp.Models;

public class WeatherResponse
{
    [JsonProperty("name")]
    public string CityName { get; set; } = "";

    [JsonProperty("sys")]
    public SysInfo Sys { get; set; } = new();

    [JsonProperty("main")]
    public MainWeather Main { get; set; } = new();

    [JsonProperty("wind")]
    public WindInfo Wind { get; set; } = new();

    [JsonProperty("weather")]
    public List<WeatherDescription> Weather { get; set; } = new();

    [JsonProperty("visibility")]
    public int Visibility { get; set; }

    [JsonProperty("dt")]
    public long Timestamp { get; set; }

    [JsonProperty("clouds")]
    public CloudsInfo Clouds { get; set; } = new();
}

public class SysInfo
{
    [JsonProperty("country")]
    public string Country { get; set; } = "";

    [JsonProperty("sunrise")]
    public long Sunrise { get; set; }

    [JsonProperty("sunset")]
    public long Sunset { get; set; }
}

public class MainWeather
{
    [JsonProperty("temp")]
    public double Temp { get; set; }

    [JsonProperty("feels_like")]
    public double FeelsLike { get; set; }

    [JsonProperty("temp_min")]
    public double TempMin { get; set; }

    [JsonProperty("temp_max")]
    public double TempMax { get; set; }

    [JsonProperty("pressure")]
    public int Pressure { get; set; }

    [JsonProperty("humidity")]
    public int Humidity { get; set; }
}

public class WindInfo
{
    [JsonProperty("speed")]
    public double Speed { get; set; }

    [JsonProperty("deg")]
    public int Deg { get; set; }

    [JsonProperty("gust")]
    public double Gust { get; set; }
}

public class WeatherDescription
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("main")]
    public string Main { get; set; } = "";

    [JsonProperty("description")]
    public string Description { get; set; } = "";

    [JsonProperty("icon")]
    public string Icon { get; set; } = "";
}

public class CloudsInfo
{
    [JsonProperty("all")]
    public int All { get; set; }
}

public class ForecastResponse
{
    [JsonProperty("list")]
    public List<ForecastItem> List { get; set; } = new();

    [JsonProperty("city")]
    public ForecastCity City { get; set; } = new();
}

public class ForecastItem
{
    [JsonProperty("dt")]
    public long Timestamp { get; set; }

    [JsonProperty("main")]
    public MainWeather Main { get; set; } = new();

    [JsonProperty("weather")]
    public List<WeatherDescription> Weather { get; set; } = new();

    [JsonProperty("wind")]
    public WindInfo Wind { get; set; } = new();

    [JsonProperty("dt_txt")]
    public string DateText { get; set; } = "";

    [JsonProperty("pop")]
    public double Pop { get; set; }
}

public class ForecastCity
{
    [JsonProperty("name")]
    public string Name { get; set; } = "";

    [JsonProperty("country")]
    public string Country { get; set; } = "";
}

public class DayForecast
{
    public DateTime Date { get; set; }
    public double TempMin { get; set; }
    public double TempMax { get; set; }
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
    public string WeatherEmoji { get; set; } = "";
    public double PrecipitationChance { get; set; }
}

public class HourlyForecast
{
    public DateTime DateTime { get; set; }
    public double Temperature { get; set; }
    public string Icon { get; set; } = "";
}

public class CityWeatherData
{
    public string CityName { get; set; } = "";
    public string Country { get; set; } = "";
    public WeatherResponse? CurrentWeather { get; set; }
    public ForecastResponse? Forecast { get; set; }
    public DateTime LastUpdated { get; set; }
}
