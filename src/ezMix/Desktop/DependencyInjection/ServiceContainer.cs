using Desktop.Services.Implementations;
using Desktop.Services.Interfaces;
using Desktop.ViewModels;
using Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using Shared.Interfaces;
using Shared.Services;

namespace Desktop.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            // Đăng ký Services
            services.AddSingleton<IViewLocator, ViewLocator>();
            services.AddTransient<IExcelAnswerExporter, ExcelAnswerExporter>();
            services.AddTransient<IOpenXMLService, OpenXMLService>();
            services.AddTransient<IInteropWordService, InteropWordService>();

            services.AddHttpClient();
            services.AddScoped<IVersionChecker, VersionChecker>();
            services.AddScoped<IUpdateService, UpdateService>();

            // Đăng ký Views
            services.AddSingleton<HomeView>();
            services.AddTransient<MainWindow>();
            services.AddTransient<NormalizationView>();

            // Đăng ký ViewModels
            services.AddTransient<HomeViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddTransient<NormalizationViewModel>();

            return services;
        }
    }
}
