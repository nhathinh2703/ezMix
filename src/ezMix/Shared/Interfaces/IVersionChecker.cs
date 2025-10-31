using Shared.Models;

namespace Shared.Interfaces
{
    public interface IVersionChecker
    {
        Task<VersionInfo?> ReadLocalAsync(string path);
        Task<VersionInfo?> ReadRemoteAsync(string url);
    }
}
