using Desktop.DependencyInjection;
using Desktop.Helpers;
using Desktop.Services.Interfaces;
using Desktop.ViewModels;
using Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using Shared.Interfaces;
using System.Diagnostics;
using System.IO;
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

            // Kiểm tra kết nối mạng
            if (await CheckInternet.IsInternetAvailableAsync())
            {
                var updateService = _serviceProvider.GetRequiredService<IUpdateService>();
                var context = await updateService.GetUpdateContextAsync("version.json");

                if (context != null)
                {
                    var updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater.exe");
                    var currentExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, context.FileExe);

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = updaterPath,
                        Arguments = $"\"{context.Url}\" \"{currentExe}\"",
                        UseShellExecute = true
                    });

                    Shutdown();
                    return;
                }
            }

            // 🚪 Khởi động giao diện chính+
            ViewTemplateSelector.ViewLocator = _serviceProvider.GetRequiredService<IViewLocator>();
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow!.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }
    }
}
