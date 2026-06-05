using System.Net.Http;
using Newtonsoft.Json;
using WeatherApp.Models;

namespace WeatherApp.Services;

public interface IWeatherService
{
    Task<WeatherResponse?> GetCurrentWeatherAsync(string city);
    Task<ForecastResponse?> GetForecastAsync(string city);
}

public class WeatherService : IWeatherService, IDisposable
{
    private readonly HttpClient _httpClient;
    // Using OpenWeatherMap free API - user should replace with their own key
    // Get free key at: https://openweathermap.org/api
    private const string ApiKey = "OPENWEATHER_API_KEY_HERE";
    private const string BaseUrl = "https://api.openweathermap.org/data/2.5";

    public WeatherService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    public async Task<WeatherResponse?> GetCurrentWeatherAsync(string city)
    {
        try
        {
            var url = $"{BaseUrl}/weather?q={Uri.EscapeDataString(city)}&appid={ApiKey}&units=metric&lang=ru";
            var response = await _httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<WeatherResponse>(response);
        }
        catch (HttpRequestException ex)
        {
            throw new WeatherServiceException($"Ошибка сети: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new WeatherServiceException("Превышено время ожидания запроса");
        }
        catch (Exception ex)
        {
            throw new WeatherServiceException($"Ошибка: {ex.Message}", ex);
        }
    }

    public async Task<ForecastResponse?> GetForecastAsync(string city)
    {
        try
        {
            var url = $"{BaseUrl}/forecast?q={Uri.EscapeDataString(city)}&appid={ApiKey}&units=metric&lang=ru";
            var response = await _httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<ForecastResponse>(response);
        }
        catch (HttpRequestException ex)
        {
            throw new WeatherServiceException($"Ошибка сети: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new WeatherServiceException("Превышено время ожидания запроса");
        }
        catch (Exception ex)
        {
            throw new WeatherServiceException($"Ошибка: {ex.Message}", ex);
        }
    }

    public void Dispose() => _httpClient.Dispose();
}

public class WeatherServiceException : Exception
{
    public WeatherServiceException(string message) : base(message) { }
    public WeatherServiceException(string message, Exception inner) : base(message, inner) { }
}
