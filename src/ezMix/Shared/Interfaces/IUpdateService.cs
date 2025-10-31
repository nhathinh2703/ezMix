namespace Shared.Interfaces
{
    public interface IUpdateService
    {
        Task<bool> CheckAndUpdateAsync(string localPath);
    }
}
