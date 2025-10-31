using Shared.Models;

namespace Shared.Interfaces
{
    public interface IUpdateService
    {
        Task<UpdateContext?> GetUpdateContextAsync(string localPath);
    }
}
