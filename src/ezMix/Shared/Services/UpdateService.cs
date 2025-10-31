using Microsoft.Extensions.Logging;
using Shared.Interfaces;
using Shared.Models;

namespace Shared.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly IVersionChecker _checker;
        private readonly ILogger<UpdateService> _log;

        public UpdateService(IVersionChecker checker, ILogger<UpdateService> log)
        {
            _checker = checker;
            _log = log;
        }

        public async Task<UpdateContext?> GetUpdateContextAsync(string localPath)
        {
            var local = await _checker.ReadLocalAsync(localPath);
            if (local == null) return null;

            var remoteUrl = local.VersionUrl ??
                $"https://raw.githubusercontent.com/{local.GitHubUser}/{local.GitHubRepo}/main/output/version.json";

            var remote = await _checker.ReadRemoteAsync(remoteUrl);
            if (remote == null || string.IsNullOrEmpty(remote.Version)) return null;

            if (new Version(remote.Version) <= new Version(local.Version!))
            {
                _log.LogInformation("✅ Phiên bản hiện tại đã là mới nhất.");
                return null;
            }

            _log.LogInformation("🚀 Có bản cập nhật mới: {version}", remote.Version);

            return new UpdateContext
            {
                Url = remote.ZipUrl!,
                FileZip = remote.File!,
                FileExe = $"{remote.AppName!}.exe"
            };
        }
    }
}