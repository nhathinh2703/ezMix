namespace Shared.Models
{
    public class VersionInfo
    {
        public string? AppName { get; set; }
        public string? Version { get; set; }
        public string? File { get; set; }
        public string? ZipUrl { get; set; }
        public string? VersionUrl { get; set; }
        public string? GitHubUser { get; set; }
        public string? GitHubRepo { get; set; }
        public string? Build { get; set; }
        public string? Sha { get; set; }
        public string? ChangeLog { get; set; }
    }
}
