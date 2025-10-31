namespace Updater.Services
{
    public interface IUpdateService
    {
        Task<bool> CheckAndUpdateAsync(string localPath);
    }
}
