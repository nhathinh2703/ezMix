using System.Net.Http;

namespace Desktop.Helpers
{
    public class InternetHelper
    {
        public static async Task<bool> IsInternetAvailableAsync()
        {
            try
            {
                // 🔧 Tạo đối tượng HttpClient với thời gian chờ (timeout) là 3 giây
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(3)
                };

                // 🌐 Gửi yêu cầu HTTP GET đến trang Google
                using var response = await client.GetAsync("http://www.google.com");

                // ✅ Nếu phản hồi thành công (status code 200–299) → có internet
                return response.IsSuccessStatusCode;
            }
            catch
            {
                // ❌ Nếu có lỗi (không kết nối được, hết thời gian chờ, DNS lỗi,...) → không có internet
                return false;
            }
        }
    }
}
