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

            if (await CheckInternet.IsInternetAvailableAsync())
            {
                var updateService = _serviceProvider.GetRequiredService<IUpdateService>();
                var success = await updateService.CheckAndUpdateAsync("version.json");

                if (success)
                {
                    // Gọi Updater.exe để ghi đè và khởi động lại
                    var updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater.exe");
                    var extractDir = Path.Combine(Path.GetTempPath(), "ezUpdateExtract");
                    var newExe = Path.Combine(extractDir, "ezMix.exe");
                    var currentExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ezMix.exe");

                    if (File.Exists(updaterPath) && File.Exists(newExe))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = updaterPath,
                            Arguments = $"\"{newExe}\" \"{currentExe}\"",
                            UseShellExecute = true
                        });

                        Shutdown(); // Thoát app hiện tại để Updater xử lý
                        return;
                    }
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
