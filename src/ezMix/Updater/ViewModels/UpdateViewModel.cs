using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows;
using Updater.Services;

namespace Updater.ViewModels
{
    public partial class UpdateViewModel : ObservableObject
    {
        private readonly IUpdateService _service;
        [ObservableProperty] private string _status = "🔄 Đang chuẩn bị cập nhật...";
        [ObservableProperty] private string _progressText = "0%";
        private double _progress;
        public double Progress
        {
            get => _progress;
            set
            {
                SetProperty(ref _progress, value);
                ProgressText = $"{(int)value}%";
            }
        }

        public UpdateViewModel(IUpdateService service)
        {
            _service = service;
        }

        public async Task RunAsync(string zipUrl, string targetExe)
        {
            var tempZip = Path.Combine(Path.GetTempPath(), "ezUpdate.zip");
            var extractDir = Path.Combine(Path.GetTempPath(), "ezUpdateExtract");

            try
            {
                Status = "⏳ Đang tải bản cập nhật...";
                Progress = 0;

                using var client = new HttpClient();
                using var response = await client.GetAsync(zipUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes > 0;

                await using (var stream = await response.Content.ReadAsStreamAsync())
                await using (var fileStream = new FileStream(tempZip, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[8192];
                    long downloaded = 0;
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                        downloaded += bytesRead;

                        if (canReportProgress)
                            Progress = downloaded * 100.0 / totalBytes;
                    }
                }

                Status = "📦 Đang giải nén...";
                Progress = 0;

                if (Directory.Exists(extractDir))
                    Directory.Delete(extractDir, true);

                ZipFile.ExtractToDirectory(tempZip, extractDir);

                var newExe = Path.Combine(extractDir, Path.GetFileName(targetExe));
                if (!File.Exists(newExe))
                {
                    Status = "❌ Không tìm thấy file mới sau khi giải nén.";
                    await Task.Delay(1500);
                    Application.Current.Shutdown();
                    return;
                }

                Status = "🛠 Đang ghi đè ứng dụng...";
                Progress = 100;

                File.Copy(newExe, targetExe, true);

                Status = "🚀 Đang khởi động lại...";
                await Task.Delay(1000);

                Process.Start(new ProcessStartInfo
                {
                    FileName = targetExe,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Status = $"❌ Lỗi: {ex.Message}";
            }

            await Task.Delay(1500);
            Application.Current.Shutdown();
        }
    }
}
