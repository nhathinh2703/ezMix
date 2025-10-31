using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using Updater.Services;

namespace Updater.ViewModels
{
    public partial class UpdateViewModel : ObservableObject
    {
        private readonly IUpdateService _service;
        [ObservableProperty] private string _status = "🔄 Đang chuẩn bị cập nhật...";

        public UpdateViewModel(IUpdateService service)
        {
            _service = service;
        }

        public async Task RunAsync(string sourceExe, string targetExe)
        {
            Status = "⏳ Đang cập nhật...";
            Status = await _service.RunUpdateAsync(sourceExe, targetExe);
            await Task.Delay(1000);
            Application.Current.Shutdown();
        }
    }
}
