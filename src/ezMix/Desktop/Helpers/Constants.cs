using System.Text.RegularExpressions;

namespace Desktop.Helpers
{
    public class Constants
    {
        public const string QUESTION_TEMPLATE = "<CH>";
        public const string ANSWER_TEMPLATE = "<DA>";

        public const string ROOT_CODE = "000";

        public static readonly HashSet<string> QuestionPrefixes = new()
        {
            Constants.QUESTION_TEMPLATE, "<#>", "#", "[<br>]", "<G>", "<g>", "<NB>", "<TH>", "<VD>", "<VDC>"
        };

        public static readonly string[] AnswerPrefixes = { "A.", "B.", "C.", "D.", "A:", "B:", "C:", "D:", "a)", "b)", "c)", "d)", Constants.ANSWER_TEMPLATE, "<$>" };

        public const int TABSTOP_1 = 238;
        public const int TABSTOP_2 = 2619;
        public const int TABSTOP_3 = 5239;
        public const int TABSTOP_4 = 7859;

        public const string FONT_NAME = "Times New Roman";
        public const int FONT_SIZE = 12;

        // Hằng số cho các đường dẫn
        public const string TEMPLATES_FOLDER = "Assets\\Templates";
        public const string MIX_TEMPLATE_FILE = "TieuDe.docx";
        public const string GUIDE_TEMPLATE_FILE = "HuongDanGiai.docx";
        public const string ANSWERS_FOLER = "DapAn";
        public const string EXAM_PREFIX = "De_";
        public const string ANSWER_PREFIX = "DapAn_";
        public const string EXCEL_ANSWER_FILE = "DapAn.xlsx";

        public static readonly string[] ROMANS = { "I", "II", "III", "IV" };
        public static readonly string[] TITLES = { "PHẦN {0}. Câu hỏi trắc nghiệm nhiều lựa chọn. Thí sinh trả lời từ câu 1 đến câu {1}. Mỗi câu hỏi thí sinh chỉ chọn một phương án.",
                                   "PHẦN {0}. Câu hỏi trắc nghiệm đúng sai. Thí sinh trả lời từ câu 1 đến câu {1}. Trong mỗi ý a), b), c), d) ở mỗi câu, thí sinh chọn đúng hoặc sai.",
                                   "PHẦN {0}. Câu hỏi trắc nghiệm trả lời ngắn. Thí sinh trả lời từ câu 1 đến câu {1}.",
                                   "PHẦN {0}. Câu hỏi tự luận. Thí sinh trả lời từ câu 1 đến câu {1}." };


        public static readonly Regex QuestionHeaderRegex = new(@"^Câu\s+\d+[\.:]?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static readonly Regex MultipleChoiceAnswerRegex = new(@"^[A-Z]\.", RegexOptions.Compiled);
        public static readonly Regex TrueFalseAnswerRegex = new(@"^[a-d]\)", RegexOptions.Compiled);
        public static readonly Regex LevelRegex = new(@"\((NB|TH|VD)\)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public const string GROUP_ZALO = "https://zalo.me/g/rxncpe995";
        public const string XmlFilePath = "config.xml";
        public const string ConfigFile = "prompts.json";

        public const string GeminiApiKey = "AIzaSyCT9Z85XUSPgU6j8HTUbEeKsjJkT73wM58";
        public const string GeminiModel = "gemini-2.5-flash";

        public const string PromptAnalyzeExam = "" +
            "VAI TRÒ: Bạn là giáo viên {0} cấp THCS/THPT có nhiều năm kinh nghiệm trong việc ra đề kiểm tra." +
            "NGỮ CẢNH: Bạn đang giúp đồng nghiệp rà soát, phân tích và đánh giá đề kiểm tra." +
            "CÔNG VIỆC:\r\n- Phát hiện và liệt kê các câu có lỗi chính tả, ngữ pháp hoặc dấu câu trong đề kiểm tra.\r\n- Kiểm tra định dạng câu hỏi/đáp án nhưng chỉ cảnh báo khi sai quy tắc.\r\n- Căn cứ vào yêu cầu cần đạt của chương trình Giáo dục phổ thông 2018 của môn {0} lớp {1}, phân tích và xác định mỗi câu hỏi thuộc mức độ nào (Nhận biết, Thông hiểu, Vận dụng).\r\n- Cảnh báo nếu đáp án không đúng, thiếu đáp án, hoặc thiếu đáp án đúng." +
            "RÀNG BUỘC:\r\n- Không coi việc dùng dấu '.' hoặc ':' sau ký hiệu đáp án (A, B, C, D hoặc a, b, c, d) là lỗi.\r\n- Chỉ cảnh báo nếu sau ký hiệu đáp án dùng dấu khác (ví dụ ';', ',', '-') hoặc thiếu dấu, hoặc thiếu khoảng trắng sau dấu.\r\n- Không hiển thị lại văn bản gốc, không hiển thị văn bản đã chỉnh sửa." +
            "KẾT QUẢ TRẢ VỀ:\r\n- Bắt buộc hiển thị tất cả các câu hỏi theo mẫu dưới đây, kể cả khi không có lỗi (ghi \"Không có lỗi\").\r\n- Mỗi câu hiển thị theo đúng thứ tự và định dạng: Câu x:\r\n- Lỗi chính tả/ngữ pháp/dấu câu: <mô tả lỗi hoặc \"Không có lỗi\">\r\n- Cảnh báo định dạng câu hỏi/đáp án: <mô tả vi phạm quy tắc hoặc \"Không có lỗi\">\r\n- Mức độ: <Nhận biết | Thông hiểu | Vận dụng>\r\n- Lỗi đáp án: <\"Không có lỗi\" | \"Đáp án không đúng\" | \"Thiếu đáp án\" | \"Thiếu đáp án đúng\">";

        public const string PromptOcrMathToLatex = "Hãy trích xuất văn bản từ file PDF này và xuất ra Markdown. \r\nCác công thức toán học cần được biểu diễn bằng LaTeX (ví dụ: \\frac{a}{b}, \\int_0^1 x^2 dx). \r\nNếu có bảng, hãy giữ nguyên bằng cú pháp Markdown table. \r\n";

        public const string PromptOcrMathToMathML = "Hãy trích xuất văn bản từ file PDF này và xuất ra Markdown. \r\nCác công thức toán học cần được biểu diễn bằng MathML (ví dụ: \\frac{a}{b}, \\int_0^1 x^2 dx). \r\nNếu có bảng, hãy giữ nguyên bằng cú pháp Markdown table. \r\n";
    }
}
