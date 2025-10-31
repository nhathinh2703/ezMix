using Desktop.DependencyInjection;
using Desktop.Helpers;
using Desktop.Services.Interfaces;
using Desktop.ViewModels;
using Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Updater.Services;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddService();
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (await CheckInternet.IsInternetAvailableAsync())
            {
                var updater = _serviceProvider.GetRequiredService<IUpdateService>();
                await updater.CheckAndUpdateAsync("version.json");
            }

            // 🚪 Khởi động giao diện chính+
            ViewTemplateSelector.ViewLocator = _serviceProvider.GetRequiredService<IViewLocator>();
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow!.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }
    }
}
