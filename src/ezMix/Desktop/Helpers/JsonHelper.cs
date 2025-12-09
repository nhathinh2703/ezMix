using System.IO;
using System.Text.Json;

namespace Desktop.Helpers
{
    public static class JsonHelper
    {
        public static T LoadFromJson<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath)) return new T();

            try
            {
                string json = File.ReadAllText(filePath);
                var obj = JsonSerializer.Deserialize<T>(json);
                return obj ?? new T();
            }
            catch
            {
                // Nếu lỗi parse thì trả về object mặc định
                return new T();
            }
        }

        public static void SaveToJson<T>(string filePath, T data)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, json);
        }
    }
}
