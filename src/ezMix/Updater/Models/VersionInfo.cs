namespace Updater.Models
{
    public class VersionInfo
    {
        public string? AppName { get; set; }
        public string? Version { get; set; }           // Phiên bản dạng string ("1.2.0")
        public string? File { get; set; }              // File chính cần chạy ("EasyMix.exe", "MyApp.dll")
        public string? ZipUrl { get; set; }            // Đường dẫn tới file zip cập nhật
        public string? VersionUrl { get; set; }        // Link public đến Version.json
        public string? GitHubUser { get; set; }        // (optional) để generate VersionUrl
        public string? GitHubRepo { get; set; }        // (optional)
        public string? Build { get; set; }             // (optional) - phân biệt giữa các build
        public string? Sha { get; set; }               // (optional) - mã commit
        public string? ChangeLog { get; set; }         // (optional) - mô tả cập nhật
    }

}
