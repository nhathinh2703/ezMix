using CommunityToolkit.Mvvm.ComponentModel;
using Shared.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using Updater.Services;

namespace Updater.ViewModels
{
    public partial class UpdateViewModel : ObservableObject
    {
        private readonly IUpdateService _service;
        [ObservableProperty] private string _status = "🔄 Đang chuẩn bị cập nhật...";
        [ObservableProperty] private string _progressText = "0%";
        [ObservableProperty] private string _versionText = "0%";
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
            var tempZip = Path.Combine(Path.GetTempPath(), $"ezUpdate_{Guid.NewGuid()}.zip");
            var extractDir = Path.Combine(Path.GetTempPath(), $"ezUpdateExtract_{Guid.NewGuid()}");

            try
            {// 0. Hiển thị thông tin phiên bản trước khi tải
                var currentDir = Path.GetDirectoryName(targetExe)!;
                var currentVersion = LoadVersionInfo(currentDir)?.Version ?? "unknown";
                VersionText = $"📥 ezMix {currentVersion} → đang kiểm tra...";

                // 1. Tải file zip với tiến độ
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

                GC.Collect();
                GC.WaitForPendingFinalizers();

                // 2. Giải nén
                Status = "📦 Đang giải nén...";
                Progress = 0;

                if (Directory.Exists(extractDir))
                    Directory.Delete(extractDir, true);

                ZipFile.ExtractToDirectory(tempZip, extractDir);

                // 3. Đọc version hiện tại và mới
                currentDir = Path.GetDirectoryName(targetExe)!;
                currentVersion = LoadVersionInfo(currentDir)?.Version ?? "unknown";
                var newVersion = LoadVersionInfo(extractDir)?.Version ?? "unknown";

                VersionText = $"📥 ezMix {currentVersion} → {newVersion}";

                if (currentVersion == newVersion)
                {
                    Status = $"✅ Phiên bản hiện tại ({currentVersion}) đã là mới nhất.";
                    await Task.Delay(1500);
                    Application.Current.Shutdown();
                    return;
                }

                Status = $"⬆️ Cập nhật từ {currentVersion} lên {newVersion}";
                await Task.Delay(1000);

                // 4. Ghi đè toàn bộ file
                Status = "🛠 Đang ghi đè các file cập nhật...";
                Progress = 100;

                foreach (var sourcePath in Directory.GetFiles(extractDir, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(extractDir, sourcePath);
                    var destinationPath = Path.Combine(currentDir, relativePath);

                    var destinationDir = Path.GetDirectoryName(destinationPath)!;
                    if (!Directory.Exists(destinationDir))
                        Directory.CreateDirectory(destinationDir);

                    try
                    {
                        File.Copy(sourcePath, destinationPath, true);
                    }
                    catch (IOException ioEx)
                    {
                        Status = $"❌ Không thể ghi đè: {relativePath}";
                        File.AppendAllText("update.log", $"[IO ERROR] {relativePath}: {ioEx.Message}\n");
                    }
                }

                // 5. Khởi động lại ứng dụng
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
                File.AppendAllText("update.log", $"[ERROR] {DateTime.Now}: {ex}\n");
            }

            await Task.Delay(1500);
            Application.Current.Shutdown();
        }

        private VersionInfo? LoadVersionInfo(string folder)
        {
            var path = Path.Combine(folder, "version.json");
            if (!File.Exists(path)) return null;

            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<VersionInfo>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
