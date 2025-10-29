using CommunityToolkit.Mvvm.ComponentModel;
using ezUpdater.Core.Models;
using Octokit;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace Desktop.ViewModels
{
    public partial class UpdateViewModel : ObservableObject
    {
        private readonly UpdateContext _context;

        [ObservableProperty]
        private string status = "Đang chuẩn bị cập nhật...";

        [ObservableProperty]
        private int progress = 0;

        public UpdateViewModel(UpdateContext context)
        {
            _context = context;
            _ = StartUpdateAsync();
        }
        private async Task StartUpdateAsync()
        {
            try
            {
                Status = "🔄 Đang tải bản cập nhật...";
                using var client = new HttpClient();
                var zip = await client.GetByteArrayAsync(_context.Url);
                Progress = 30;

                string tempZip = Path.GetTempFileName();
                await File.WriteAllBytesAsync(tempZip, zip);

                Status = "📦 Đang giải nén...";
                Progress = 60;

                string tempDir = Path.Combine(Path.GetTempPath(), "Update_" + Path.GetFileNameWithoutExtension(tempZip));
                ZipFile.ExtractToDirectory(tempZip, tempDir, true);
                File.Delete(tempZip);

                string src = Path.Combine(tempDir, _context.FileName);
                string dst = Path.Combine(AppContext.BaseDirectory, _context.FileName);
                File.Copy(src, dst, overwrite: true);
                Directory.Delete(tempDir, true);

                Status = "✅ Đã cập nhật xong. Đang khởi động lại...";
                Progress = 100;

                await Task.Delay(1000);
                Process.Start(new ProcessStartInfo
                {
                    FileName = _context.AppExe,
                    UseShellExecute = true
                });

                // Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Status = $"❌ Lỗi cập nhật: {ex.Message}";
            }
        }
    }
}
