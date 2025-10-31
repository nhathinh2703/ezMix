using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Updater.Services;
using Updater.ViewModels;
using Updater.Views;

namespace Updater;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = default!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        // Kiểm tra tham số dòng lệnh
        if (e.Args.Length != 2)
        {
            MessageBox.Show("Thiếu tham số cập nhật.\nCần truyền: <zipUrl> <targetExe>");
            Shutdown();
            return;
        }

        var zipUrl = e.Args[0];
        var targetExe = e.Args[1];

        var vm = Services.GetRequiredService<UpdateViewModel>();
        var window = new UpdateWindow(vm, zipUrl, targetExe);
        window.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddTransient<UpdateViewModel>();
    }
}
