using Desktop.DependencyInjection;
using Desktop.Helpers;
using Desktop.Services.Interfaces;
using Desktop.ViewModels;
using Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

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
            
            // 🚪 Khởi động giao diện chính+
            ViewTemplateSelector.ViewLocator = _serviceProvider.GetRequiredService<IViewLocator>();
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow!.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }
    }
}
