using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WeatherApp.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue = value is bool b && b;
        bool invert = parameter?.ToString() == "Invert";
        return (boolValue ^ invert) ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class TemperatureColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double temp)
        {
            if (temp <= -10) return "#82B1FF";
            if (temp <= 0)   return "#80D8FF";
            if (temp <= 10)  return "#CCFF90";
            if (temp <= 20)  return "#FFD180";
            if (temp <= 30)  return "#FF9E80";
            return "#FF6E40";
        }
        return "#ECEFF1";
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class DateToDayNameConverter : IValueConverter
{
    private static readonly string[] Days = ["Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"];

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            if (date.Date == DateTime.Today) return "Сегодня";
            if (date.Date == DateTime.Today.AddDays(1)) return "Завтра";
            return $"{Days[(int)date.DayOfWeek]}, {date:dd MMM}";
        }
        return "";
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class PrecipitationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d) return $"{d:0}%";
        if (value is int i)   return $"{i}%";
        return "0%";
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class TemperatureFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d) return $"{(d > 0 ? "+" : "")}{d:0.#}°";
        return "0°";
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
