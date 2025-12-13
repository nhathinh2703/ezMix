using Desktop.Models;

namespace Desktop.Services.Interfaces
{
    public interface IUpdateCheckService
    {
        Task<UpdateInfo?> GetLatestAsync(string url);

        bool HasUpdate(string currentVersion, string latestVersion);
    }
}
