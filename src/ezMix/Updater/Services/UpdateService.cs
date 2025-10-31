using System.Diagnostics;
using System.IO;

namespace Updater.Services
{
    public class UpdateService : IUpdateService
    {
        public async Task<string> RunUpdateAsync(string sourceExe, string targetExe)
        {
            await Task.Delay(2000);

            try
            {
                File.Copy(sourceExe, targetExe, true);
                await Task.Delay(1000);
                Process.Start(new ProcessStartInfo
                {
                    FileName = targetExe,
                    UseShellExecute = true
                });
                return "✅ Cập nhật thành công. Đang khởi động lại...";
            }
            catch (Exception ex)
            {
                return $"❌ Lỗi: {ex.Message}";
            }
        }
    }
}