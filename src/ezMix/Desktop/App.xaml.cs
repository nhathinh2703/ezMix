using Desktop.DependencyInjection;
using Desktop.Helpers;
using Desktop.Services.Implementations;
using Desktop.Services.Interfaces;
using Desktop.ViewModels;
using Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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

            bool updaterStarted = false;
            if (await InternetHelper.IsInternetAvailableAsync())
            {
                updaterStarted = await TryCheckAndStartUpdateAsync();
            }

            if (!updaterStarted)
            {
                StartMainWindow();
            }
        }
        private async Task<bool> TryCheckAndStartUpdateAsync()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version!;
            string currentVersion = $"{version.Major}.{version.Minor}.{version.Build}";

            const string updateJsonUrl = "https://raw.githubusercontent.com/nhathinh2703/ez-updates/main/apps/ezMix/latest.json";
            var updateService = new UpdateCheckService();

            try
            {
                var latest = await updateService.GetLatestAsync(updateJsonUrl);

                if (latest != null && updateService.HasUpdate(currentVersion, latest.Version))
                {
                    var message =
                        $"Có phiên bản mới {latest.Version}\n\n{string.Join("\n", latest.Changelog)}\n\nCập nhật ngay?";

                    var result = MessageBox.Show(
                        message,
                        "Cập nhật phần mềm",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes || latest.Mandatory)
                    {
                        StartUpdater(latest.Url);
                        return true;   // ✅ ĐÃ BẮT ĐẦU UPDATE
                    }
                }
            }
            catch
            {
                // Không làm app chết nếu lỗi mạng / json
            }

            return false; // ❌ Không update → tiếp tục chạy app
        }

        private void StartMainWindow()
        {
            ViewTemplateSelector.ViewLocator = _serviceProvider.GetRequiredService<IViewLocator>();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();

            mainWindow.Show();
        }


        private void StartUpdater(string zipUrl)
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string updaterExe = Path.Combine(appDir, "ezUpdater.exe");

            if (!File.Exists(updaterExe))
                return;

            int pid = Environment.ProcessId;

            var psi = new ProcessStartInfo
            {
                FileName = updaterExe,
                UseShellExecute = true
            };

            psi.ArgumentList.Add("--app-dir");
            psi.ArgumentList.Add(appDir);

            psi.ArgumentList.Add("--zip-url");
            psi.ArgumentList.Add(zipUrl);

            psi.ArgumentList.Add("--exe-name");
            psi.ArgumentList.Add("ezMix.exe");

            psi.ArgumentList.Add("--parent-pid");
            psi.ArgumentList.Add(pid.ToString());

            Process.Start(psi);

            Shutdown();
        }
    }
}
