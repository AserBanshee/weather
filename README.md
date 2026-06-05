# WeatherView — Погодный монитор WPF

Десктопное приложение для просмотра и анализа погодных данных с современным тёмным интерфейсом.

## Возможности

- 🌡️ **Текущая погода** — температура, влажность, давление, ветер, видимость, облачность
- 📅 **Прогноз на 5 дней** — мин/макс температура, описание, вероятность осадков
- 📈 **График температуры** — часовой прогноз на 48 часов (LiveCharts2)
- 🌆 **Список городов** — добавление, удаление, быстрое переключение
- 🔄 **Авто-обновление** — каждые 10 минут, UI не зависает (async/await)
- 🎨 **Тёмная тема** — профессиональный дизайн с цветовой индикацией температуры

## Технологии

| Технология | Назначение |
|---|---|
| C# 12 / .NET 8 | Язык и платформа |
| WPF | UI Framework |
| MVVM | Паттерн архитектуры |
| async/await | Асинхронная работа с сетью |
| HttpClient | HTTP запросы к API |
| Newtonsoft.Json | Десериализация JSON |
| LiveChartsCore 2 | Графики температуры |
| MaterialDesignThemes | UI компоненты |
| CommunityToolkit.Mvvm | Базовые MVVM классы |

## Установка и запуск

### Требования
- Windows 10/11
- .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
- Visual Studio 2022 или VS Code с C# расширением

### Получение API ключа
1. Зарегистрируйтесь на https://openweathermap.org/api
2. Перейдите в My API Keys
3. Скопируйте бесплатный ключ (лимит 60 запросов/минуту)

### Настройка
Откройте `Services/WeatherService.cs` и замените:
```csharp
private const string ApiKey = "OPENWEATHER_API_KEY_HERE";
```
на ваш реальный ключ.

### Сборка
```bash
# Клонируйте/скачайте проект
cd WeatherApp

# Восстановление пакетов
dotnet restore

# Сборка и запуск
dotnet run

# Или сборка в Release
dotnet publish -c Release -r win-x64 --self-contained true
```

### Из Visual Studio
1. Откройте `WeatherApp.csproj`
2. Нажмите F5 для запуска

## Структура проекта

```
WeatherApp/
├── Models/
│   └── WeatherModels.cs        # Модели данных (API + UI)
├── Services/
│   ├── WeatherService.cs       # HTTP клиент OpenWeatherMap
│   └── WeatherDataProcessor.cs # Обработка и преобразование данных
├── ViewModels/
│   ├── BaseViewModel.cs        # INotifyPropertyChanged base
│   ├── MainViewModel.cs        # Главный ViewModel (логика, состояние)
│   └── RelayCommand.cs         # ICommand реализации
├── Views/
│   └── MainWindow.xaml(.cs)    # Главное окно
├── Converters/
│   └── Converters.cs           # Value Converters для привязок
├── Styles/
│   └── WeatherStyles.xaml      # Стили, цвета, темы
└── App.xaml(.cs)               # Точка входа
```

## Архитектура MVVM

```
View (XAML)
    ↕ Binding
ViewModel (MainViewModel)
    ↕ Async calls
Service (WeatherService → OpenWeatherMap API)
    ↕ HTTP/JSON
Model (WeatherResponse, ForecastResponse, ...)
```

**Ключевые принципы:**
- View не знает о сервисах — только биндинги к ViewModel
- ViewModel не знает о View — только свойства и команды
- Все сетевые вызовы async/await — UI никогда не зависает
- Обновление UI всегда на главном потоке (WPF Dispatcher)

## API

Используется OpenWeatherMap Free Tier:
- `GET /data/2.5/weather` — текущая погода
- `GET /data/2.5/forecast` — прогноз (3-часовые интервалы, 5 дней)

Единицы измерения: метрические (°C, м/с, гПа → мм рт. ст.)
