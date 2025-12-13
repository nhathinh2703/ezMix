using Desktop.Models;
using Desktop.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;

namespace Desktop.Services.Implementations
{
    public class UpdateCheckService : IUpdateCheckService
    {
        private readonly HttpClient _http = new();

        public async Task<UpdateInfo?> GetLatestAsync(string url)
        {
            return await _http.GetFromJsonAsync<UpdateInfo>(url);
        }

        public bool HasUpdate(string current, string latest)
        {
            return new Version(latest) > new Version(current);
        }
    }
}
