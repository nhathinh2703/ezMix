using Updater.Models;

namespace Updater.Services
{
    public interface IVersionChecker
    {
        Task<VersionInfo?> ReadLocalAsync(string path);
        Task<VersionInfo?> ReadRemoteAsync(string url);
    }
}
