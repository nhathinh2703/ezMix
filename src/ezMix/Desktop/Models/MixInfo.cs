using CommunityToolkit.Mvvm.ComponentModel;

namespace Desktop.Models
{
    public class MixInfo
    {
        public string Code { get; set; } = string.Empty;
        public int NumberOfVersions { get; set; } = 4;
        public string[] Versions { get; set; } = [];
        public string StartCode { get; set; } = "01";
        public string SuperiorUnit { get; set; } = "SỞ GDĐT ...";
        public string Unit { get; set; } = "TRƯỜNG THPT ...";
        public string TestPeriod { get; set; } = "KIỂM TRA GIỮA KÌ 1";
        public string Grade { get; set; } = "12";
        public string SchoolYear { get; set; } = "2025-2026";
        public string Subject { get; set; } = "Tin học 9";
        public string Time { get; set; } = "45 phút";

        // 👇 Thêm các thông số hiển thị
        public string FontFamily { get; set; } = "Times New Roman";
        public double FontSize { get; set; } = 12;

        public bool IsFixMathType { get; set; } = true;
        public bool IsDeleteLogWhenStart { get; set; } = true;
    }
}
