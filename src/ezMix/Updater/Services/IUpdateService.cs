namespace Updater.Services
{
    public interface IUpdateService
    {
        Task<string> RunUpdateAsync(string sourceExe, string targetExe);
    }
}
