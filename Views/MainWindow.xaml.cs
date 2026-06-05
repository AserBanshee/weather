using System.Windows;
using System.Windows.Input;
using WeatherApp.Services;
using WeatherApp.ViewModels;

namespace WeatherApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var weatherService = new WeatherService();
        DataContext = new MainViewModel(weatherService);
    }
}
