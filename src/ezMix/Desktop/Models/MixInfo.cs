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
        public string Subject { get; set; } = "TIN HỌC";
        public string Time { get; set; } = "45 phút";

        public string FontFamily { get; set; } = "Times New Roman";
        public string FontSize { get; set; } = "12";

        public bool IsFixMathType { get; set; } = true;
        public bool IsDeleteLogWhenStart { get; set; } = true;

        public bool IsShuffledQuestionMultipleChoice { get; set; } = true;
        public bool IsShuffledAnswerMultipleChoice { get; set; } = true;
        public bool IsShuffledQuestionTrueFalse { get; set; } = true;
        public bool IsShuffledAnswerTrueFalse { get; set; } = true;
        public bool IsShuffledShortAnswer { get; set; } = true;
        public bool IsShuffledEssay { get; set; } = true;
        public bool IsShowWordWhenAnalyze { get; set; } = true;

        public string PointMultipleChoice { get; set; } = "3,0";
        public string PointTrueFalse { get; set; } = "2,0";
        public string PointShortAnswer { get; set; } = "2,0";
        public string PointEssay { get; set; } = "3,0";


        public string GeminiApiKey { get; set; } = "Nhập key của bạn";
        public string GeminiModel { get; set; } = "gemini-2.5-flash";
    }
}
