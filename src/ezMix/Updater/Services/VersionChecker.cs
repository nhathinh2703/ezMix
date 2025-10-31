using Microsoft.Extensions.Logging;
using System.Text.Json;
using Updater.Models;

namespace Updater.Services
{
    public class VersionChecker : IVersionChecker
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<VersionChecker> _log;

        public VersionChecker(IHttpClientFactory http, ILogger<VersionChecker> log)
        {
            _http = http;
            _log = log;
        }

        public async Task<VersionInfo?> ReadLocalAsync(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    _log.LogWarning("⛔ Không tìm thấy file Version.json: {path}", path);
                    return null;
                }

                var json = await File.ReadAllTextAsync(path);
                var info = JsonSerializer.Deserialize<VersionInfo>(json);
                return info;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "❌ Lỗi khi đọc file Version.json: {path}", path);
                return null;
            }
        }

        public async Task<VersionInfo?> ReadRemoteAsync(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url)) return null;

                var client = _http.CreateClient();
                var json = await client.GetStringAsync(url);
                var info = JsonSerializer.Deserialize<VersionInfo>(json);
                return info;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "❌ Lỗi khi tải Version.json từ URL: {url}", url);
                return null;
            }
        }
    }
}
