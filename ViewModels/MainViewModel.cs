using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;
    private readonly DispatcherTimer _autoRefreshTimer;

    // --- State ---
    private bool _isLoading;
    private string _errorMessage = "";
    private bool _hasError;
    private string _cityInput = "";
    private string _currentCityName = "";
    private string _lastUpdatedText = "";

    // --- Current Weather ---
    private double _temperature;
    private double _feelsLike;
    private double _tempMin;
    private double _tempMax;
    private int _humidity;
    private double _windSpeed;
    private string _windDirection = "";
    private int _pressure;
    private int _visibility;
    private string _weatherDescription = "";
    private string _weatherEmoji = "";
    private string _countryCode = "";
    private string _sunriseTime = "";
    private string _sunsetTime = "";
    private int _cloudiness;

    // --- Charts ---
    private ISeries[] _temperatureSeries = Array.Empty<ISeries>();
    private Axis[] _xAxes = Array.Empty<Axis>();
    private Axis[] _yAxes = Array.Empty<Axis>();

    public MainViewModel(IWeatherService weatherService)
    {
        _weatherService = weatherService;
        DailyForecasts = new ObservableCollection<DayForecast>();
        SavedCities = new ObservableCollection<string>();

        SearchCommand = new AsyncRelayCommand(SearchWeatherAsync, () => !string.IsNullOrWhiteSpace(CityInput));
        AddCityCommand = new AsyncRelayCommand(AddCityAsync, () => !string.IsNullOrWhiteSpace(CityInput));
        RemoveCityCommand = new RelayCommand(RemoveCity);
        SelectCityCommand = new AsyncRelayCommand<string>(SelectCityAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshCurrentCityAsync, () => !string.IsNullOrWhiteSpace(_currentCityName));

        // Default cities
        SavedCities.Add("Москва");
        SavedCities.Add("Санкт-Петербург");
        SavedCities.Add("London");
        SavedCities.Add("Berlin");

        // Auto refresh every 10 minutes
        _autoRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(10) };
        _autoRefreshTimer.Tick += async (_, _) => await RefreshCurrentCityAsync();
        _autoRefreshTimer.Start();

        InitializeEmptyChart();
    }

    #region Properties

    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public bool HasError { get => _hasError; set { SetProperty(ref _hasError, value); OnPropertyChanged(nameof(HasData)); } }
    public bool HasData => !string.IsNullOrEmpty(_currentCityName) && !_hasError;
    public bool IsEmptyState => string.IsNullOrEmpty(_currentCityName);
    public string CityInput { get => _cityInput; set => SetProperty(ref _cityInput, value); }
    public string CurrentCityName { get => _currentCityName; set { SetProperty(ref _currentCityName, value); OnPropertyChanged(nameof(HasData)); OnPropertyChanged(nameof(IsEmptyState)); } }
    public string LastUpdatedText { get => _lastUpdatedText; set => SetProperty(ref _lastUpdatedText, value); }

    public double Temperature { get => _temperature; set => SetProperty(ref _temperature, value); }
    public double FeelsLike { get => _feelsLike; set => SetProperty(ref _feelsLike, value); }
    public double TempMin { get => _tempMin; set => SetProperty(ref _tempMin, value); }
    public double TempMax { get => _tempMax; set => SetProperty(ref _tempMax, value); }
    public int Humidity { get => _humidity; set => SetProperty(ref _humidity, value); }
    public double WindSpeed { get => _windSpeed; set => SetProperty(ref _windSpeed, value); }
    public string WindDirection { get => _windDirection; set => SetProperty(ref _windDirection, value); }
    public int Pressure { get => _pressure; set => SetProperty(ref _pressure, value); }
    public int Visibility { get => _visibility; set => SetProperty(ref _visibility, value); }
    public string WeatherDescription { get => _weatherDescription; set => SetProperty(ref _weatherDescription, value); }
    public string WeatherEmoji { get => _weatherEmoji; set => SetProperty(ref _weatherEmoji, value); }
    public string CountryCode { get => _countryCode; set => SetProperty(ref _countryCode, value); }
    public string SunriseTime { get => _sunriseTime; set => SetProperty(ref _sunriseTime, value); }
    public string SunsetTime { get => _sunsetTime; set => SetProperty(ref _sunsetTime, value); }
    public int Cloudiness { get => _cloudiness; set => SetProperty(ref _cloudiness, value); }

    public ISeries[] TemperatureSeries { get => _temperatureSeries; set => SetProperty(ref _temperatureSeries, value); }
    public Axis[] XAxes { get => _xAxes; set => SetProperty(ref _xAxes, value); }
    public Axis[] YAxes { get => _yAxes; set => SetProperty(ref _yAxes, value); }

    public ObservableCollection<DayForecast> DailyForecasts { get; }
    public ObservableCollection<string> SavedCities { get; }

    #endregion

    #region Commands

    public AsyncRelayCommand SearchCommand { get; }
    public AsyncRelayCommand AddCityCommand { get; }
    public RelayCommand RemoveCityCommand { get; }
    public AsyncRelayCommand<string> SelectCityCommand { get; }
    public AsyncRelayCommand RefreshCommand { get; }

    #endregion

    #region Methods

    private async Task SearchWeatherAsync()
    {
        if (string.IsNullOrWhiteSpace(CityInput)) return;
        await LoadWeatherDataAsync(CityInput.Trim());
    }

    private async Task AddCityAsync()
    {
        var city = CityInput.Trim();
        if (string.IsNullOrWhiteSpace(city)) return;

        await LoadWeatherDataAsync(city);

        if (!HasError && !SavedCities.Contains(city))
        {
            SavedCities.Add(city);
        }
    }

    private void RemoveCity(object? parameter)
    {
        if (parameter is string city)
            SavedCities.Remove(city);
    }

    private async Task SelectCityAsync(string? city)
    {
        if (!string.IsNullOrWhiteSpace(city))
            await LoadWeatherDataAsync(city);
    }

    private async Task RefreshCurrentCityAsync()
    {
        if (!string.IsNullOrWhiteSpace(_currentCityName))
            await LoadWeatherDataAsync(_currentCityName);
    }

    private async Task LoadWeatherDataAsync(string city)
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = "";

        try
        {
            var currentTask = _weatherService.GetCurrentWeatherAsync(city);
            var forecastTask = _weatherService.GetForecastAsync(city);

            await Task.WhenAll(currentTask, forecastTask);

            var current = await currentTask;
            var forecast = await forecastTask;

            if (current != null)
                UpdateCurrentWeather(current);

            if (forecast != null)
            {
                UpdateDailyForecast(forecast);
                UpdateTemperatureChart(forecast);
            }

            CurrentCityName = city;
            LastUpdatedText = $"Обновлено: {DateTime.Now:HH:mm}";
        }
        catch (WeatherServiceException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message.Contains("404") || ex.Message.Contains("city")
                ? $"Город «{city}» не найден. Проверьте написание."
                : ex.Message;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Непредвиденная ошибка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateCurrentWeather(WeatherResponse weather)
    {
        Temperature = Math.Round(weather.Main.Temp, 1);
        FeelsLike = Math.Round(weather.Main.FeelsLike, 1);
        TempMin = Math.Round(weather.Main.TempMin, 1);
        TempMax = Math.Round(weather.Main.TempMax, 1);
        Humidity = weather.Main.Humidity;
        WindSpeed = Math.Round(weather.Wind.Speed, 1);
        WindDirection = WeatherDataProcessor.GetWindDirection(weather.Wind.Deg);
        Pressure = (int)(weather.Main.Pressure * 0.750064);  // hPa to mmHg
        Visibility = weather.Visibility / 1000;
        CountryCode = weather.Sys.Country;
        Cloudiness = weather.Clouds.All;
        SunriseTime = WeatherDataProcessor.FormatSunTime(weather.Sys.Sunrise);
        SunsetTime = WeatherDataProcessor.FormatSunTime(weather.Sys.Sunset);

        var desc = weather.Weather.FirstOrDefault();
        WeatherDescription = desc != null
            ? char.ToUpper(desc.Description[0]) + desc.Description[1..]
            : "";
        WeatherEmoji = WeatherDataProcessor.GetWeatherEmoji(desc?.Id ?? 0);
    }

    private void UpdateDailyForecast(ForecastResponse forecast)
    {
        var dailyForecasts = WeatherDataProcessor.GetDailyForecasts(forecast);
        DailyForecasts.Clear();
        foreach (var day in dailyForecasts)
            DailyForecasts.Add(day);
    }

    private void UpdateTemperatureChart(ForecastResponse forecast)
    {
        var hourly = WeatherDataProcessor.GetHourlyForecasts(forecast, 48);

        if (!hourly.Any()) return;

        var temps = hourly.Select(h => h.Temperature).ToArray();
        var labels = hourly.Select(h => h.DateTime.ToString("dd.MM\nHH:mm")).ToArray();

        TemperatureSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = temps,
                Name = "Температура (°C)",
                Stroke = new SolidColorPaint(SKColor.Parse("#4FC3F7"), 3),
                GeometryFill = new SolidColorPaint(SKColor.Parse("#4FC3F7")),
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#29B6F6"), 2),
                GeometrySize = 6,
                Fill = new LinearGradientPaint(
                    new[] { SKColor.Parse("#4FC3F740"), SKColor.Parse("#4FC3F705") },
                    new SKPoint(0.5f, 0),
                    new SKPoint(0.5f, 1)
                ),
                LineSmoothness = 0.5
            }
        };

        XAxes = new Axis[]
        {
            new Axis
            {
                Labels = labels,
                LabelsRotation = 0,
                TextSize = 10,
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#90A4AE")),
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#263238"))
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#90A4AE")),
                TextSize = 11,
                Labeler = val => $"{val:0.#}°",
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#1A2733"))
            }
        };
    }

    private void InitializeEmptyChart()
    {
        TemperatureSeries = Array.Empty<ISeries>();
        XAxes = new Axis[] { new Axis { LabelsPaint = new SolidColorPaint(SKColor.Parse("#90A4AE")) } };
        YAxes = new Axis[] { new Axis { LabelsPaint = new SolidColorPaint(SKColor.Parse("#90A4AE")) } };
    }

    #endregion
}

// Generic async relay command
public class AsyncRelayCommand<T> : System.Windows.Input.ICommand
{
    private readonly Func<T?, Task> _execute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<T?, Task> execute) => _execute = execute;

    public event EventHandler? CanExecuteChanged
    {
        add => System.Windows.Input.CommandManager.RequerySuggested += value;
        remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => !_isExecuting;

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter)) return;
        _isExecuting = true;
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        try { await _execute(parameter is T t ? t : default); }
        finally
        {
            _isExecuting = false;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }
}

// Extension to MainViewModel — add these properties after HasError:
// public bool HasData => !string.IsNullOrEmpty(_currentCityName) && !HasError;
// public bool IsEmptyState => string.IsNullOrEmpty(_currentCityName);
