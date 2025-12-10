using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.Helpers;
using Desktop.Models;
using Desktop.Models.Enums;
using Desktop.Services.Interfaces;
using Microsoft.Office.Interop.Word;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Task = System.Threading.Tasks.Task;
using Word = Microsoft.Office.Interop.Word;

namespace Desktop.ViewModels
{
    public partial class MixViewModel : ObservableObject
    {
        private readonly IOpenXMLService _openXMLService;
        private readonly IInteropWordService _interopWordService;
        private readonly IGeminiService _geminiService;

        [ObservableProperty] private string geminiAPIKey = Constants.GeminiApiKey;
        [ObservableProperty] private string geminiModel = Constants.GeminiModel;
        [ObservableProperty] private string promtAnalyzeExam = string.Empty;
        [ObservableProperty] private string promptJson = string.Empty;

        [ObservableProperty] private string sourceFile = string.Empty;
        [ObservableProperty] private string destinationFile = string.Empty;
        [ObservableProperty] private string outputFolder = string.Empty;

        [ObservableProperty] private ObservableCollection<Question> questions = [];

        [ObservableProperty] private ObservableCollection<ExamType> examTypes = [];
        [ObservableProperty] private ExamType selectedExamType = ExamType.ezMix;

        [ObservableProperty] private bool isEnableMix = false;
        [ObservableProperty] private bool isOK = false;

        [ObservableProperty] private MixInfo mixInfo = new();
        [ObservableProperty] private string examCodes = string.Empty;
        [ObservableProperty] private string fontFamily = "Times New Roman";
        [ObservableProperty] private string fontSize = "12";
        [ObservableProperty] private string processContent = string.Empty;

        [ObservableProperty] private string inputText = string.Empty;
        [ObservableProperty] private string resultText = string.Empty;
        public Dictionary<string, string> Prompts { get; set; } = new Dictionary<string, string>();

        public ObservableCollection<string> FontFamilies { get; } =
        [
            "Times New Roman",
            "Arial",
            "Tahoma",
            "Calibri",
            "Cambria",
            "Verdana",
            "Georgia"
        ];
        public ObservableCollection<string> FontSizes { get; } =
        [
            "10", "11", "12", "13", "14", "16", "18", "20"
        ];

        public MixViewModel(IOpenXMLService openXMLService, IInteropWordService interopWordService, IGeminiService geminiService)
        {
            _openXMLService = openXMLService;
            _interopWordService = interopWordService;
            _geminiService = geminiService;

            ExamTypes = new ObservableCollection<ExamType>(Enum.GetValues(typeof(ExamType)).Cast<ExamType>());
            MixInfo = XmlHelper.LoadFromXml<MixInfo>(Constants.XmlFilePath);

            FontFamily = MixInfo.FontFamily;
            FontSize = MixInfo.FontSize;
            GeminiAPIKey = Constants.GeminiApiKey;

            // Load từ file JSON
            Prompts = JsonHelper.LoadFromJson<Dictionary<string, string>>(Constants.ConfigFile);
            if (Prompts == null || Prompts.Count == 0)
            {
                Prompts = new Dictionary<string, string>
                {
                    ["PromptAnalyzeExam"] = Constants.PromptAnalyzeExam,
                    ["PromptOcrMathToLatex"] = Constants.PromptOcrMathToLatex,
                    ["PromptOcrMathToMathML"] = Constants.PromptOcrMathToMathML
                };
                JsonHelper.SaveToJson(Constants.ConfigFile, Prompts);
            }
            LoadPromptJson();
        }

        [RelayCommand]
        private async Task AnalyzeFile()
        {
            try
            {
                var sourcePath = FileHelper.BrowseFile();
                if (string.IsNullOrEmpty(sourcePath))
                    return;

                ResetLog();
                AddLog("---CHỨC NĂNG CHUẨN HÓA---");
                AddLog($"- Chuẩn hóa đề kiểu: {SelectedExamType}");

                SourceFile = sourcePath;
                AddLog($"- Chọn tệp nguồn: {SourceFile}");

                string sourceFolder = Path.GetDirectoryName(sourcePath)!;

                string fileName = $"{SelectedExamType}_{Path.GetFileName(sourcePath)}";
                string targetPath = Path.Combine(sourceFolder, fileName);

                if (File.Exists(targetPath))
                {
                    AddLog("- Phát hiện tệp ezMix cũ, tiến hành xóa...");
                    File.SetAttributes(targetPath, FileAttributes.Normal);
                    File.Delete(targetPath);
                }

                File.Copy(sourcePath, targetPath);
                DestinationFile = targetPath;
                AddLog($"- Tạo tệp đích: {DestinationFile}");

                AddLog("Bắt đầu chuẩn hóa nội dung...");
                await ProcessDocumentAsync(DestinationFile, SelectedExamType);

                AddLog("- Phân tích câu hỏi từ tệp đã chuẩn hóa...");
                var result = await _openXMLService.ParseDocxQuestionsAsync(DestinationFile);
                Questions = new ObservableCollection<Question>(result);

                IsOK = Questions.All(q => q.IsValid);
                AddLog(IsOK ? "- Tất cả câu hỏi hợp lệ." : "- Có câu hỏi không hợp lệ.");

                IsEnableMix = !string.IsNullOrEmpty(SourceFile) && File.Exists(SourceFile) && IsOK;
                AddLog(IsEnableMix ? "- Cho phép trộn đề." : "- ERROR: Không thể trộn do lỗi.");

                MessageHelper.Success($"Chuẩn hóa theo ({SelectedExamType}) thành công!");
                AddLog("- Chuẩn hóa hoàn tất thành công!");
            }
            catch (Exception ex)
            {
                AddLog($"- ERROR: Lỗi khi chuẩn hóa: {ex.Message}");
                MessageHelper.Error(ex);
            }
        }

        [RelayCommand]
        private async Task RecognitionFile()
        {
            try
            {
                var filePath = FileHelper.BrowseFile();
                if (string.IsNullOrEmpty(filePath))
                    return;

                ResetLog();
                AddLog("---CHỨC NĂNG NHẬN DẠNG---");

                SourceFile = DestinationFile = filePath;
                AddLog($"- Chọn tệp nguồn/đích: {filePath}");

                AddLog("- Bắt đầu phân tích câu hỏi từ file...");
                var result = await _openXMLService.ParseDocxQuestionsAsync(filePath);
                Questions = new ObservableCollection<Question>(result);
                AddLog($"- Đã phân tích được {Questions.Count} câu hỏi.");

                IsOK = Questions.All(q => q.IsValid);
                AddLog(IsOK ? "- Tất cả câu hỏi hợp lệ." : "- ERROR: Tồn tại câu hỏi không hợp lệ.");

                IsEnableMix = File.Exists(SourceFile) && IsOK;
                AddLog(IsEnableMix ? "- Tệp hợp lệ, có thể trộn đề." : "- Tệp không hợp lệ, không thể trộn đề.");
            }
            catch (Exception ex)
            {
                AddLog($"- ERROR: Lỗi khi nhận dạng: {ex.Message}");
                MessageHelper.Error(ex);
            }
        }

        private async Task ProcessDocumentAsync(string filePath, ExamType typeExam)
        {
            _Document? document = null;
            try
            {
                document = await _interopWordService.OpenDocumentAsync(filePath, visible: true);
                document.Activate();

                await _interopWordService.FormatDocumentAsync(document);
                await _interopWordService.DeleteAllHeadersAndFootersAsync(document);
                await _interopWordService.ConvertListFormatToTextAsync(document);
                await _interopWordService.RejectAllChangesAsync(document);

                var fixs = new Dictionary<string, string>
                {
                    ["^p "] = "^p",
                    [" ^p"] = "^p",
                    ["  "] = " ",
                    [" ?"] = "?",
                    [" ."] = ".",
                    ["?."] = "?",
                };
                await _interopWordService.ReplaceUntilDoneAsync(document, fixs, matchCase: true, matchWholeWord: false, matchWildcards: false);

                var replacements = new Dictionary<string, string>
                {
                    ["^t"] = " ",
                    ["^l"] = " ",
                    ["^s"] = " ",
                    ["<$>"] = "^p" + Constants.ANSWER_TEMPLATE,
                    ["A. "] = "^p" + Constants.ANSWER_TEMPLATE,
                    ["B. "] = "^p" + Constants.ANSWER_TEMPLATE,
                    ["C. "] = "^p" + Constants.ANSWER_TEMPLATE,
                    ["D. "] = "^p" + Constants.ANSWER_TEMPLATE,
                    ["Đáp án: "] = "^p" + Constants.ANSWER_TEMPLATE,
                    ["Đáp án. "] = "^p" + Constants.ANSWER_TEMPLATE,
                    ["ĐÁP ÁN: "] = "^p" + Constants.ANSWER_TEMPLATE,
                    ["ĐÁP ÁN. "] = "^p" + Constants.ANSWER_TEMPLATE,
                    ["ĐÁ:"] = "^p" + Constants.ANSWER_TEMPLATE,
                    ["ĐÁ."] = "^p" + Constants.ANSWER_TEMPLATE,
                    ["ĐA:"] = "^p" + Constants.ANSWER_TEMPLATE,
                    ["ĐA."] = "^p" + Constants.ANSWER_TEMPLATE,
                    ["<#>"] = Constants.QUESTION_TEMPLATE,
                    ["#"] = Constants.QUESTION_TEMPLATE,
                    ["[<br>]"] = Constants.QUESTION_TEMPLATE,
                    ["<NB>"] = Constants.QUESTION_TEMPLATE,
                    ["<TH>"] = Constants.QUESTION_TEMPLATE,
                    ["<VD>"] = Constants.QUESTION_TEMPLATE,
                    ["<VDC>"] = Constants.QUESTION_TEMPLATE,
                    ["<Đ>"] = "a) ",
                    ["<S>"] = "a) "
                };
                await _interopWordService.ReplaceAsync(document, replacements, matchCase: true, matchWholeWord: false);
                await _interopWordService.ReplaceRedTextWithUnderlineAsync(document);

                var range = document.Range();
                range.Font.Color = WdColor.wdColorBlack;
                range.Font.Name = MixInfo.FontFamily;
                range.Font.Size = Convert.ToSingle(MixInfo.FontSize);

                var removeStarts = new[]
                {
                    "phần 1", "phần 2", "phần 3", "phần 4",
                    "phần i", "phần ii", "phần iii", "phần iv",
                    "dạng 1", "dạng 2", "dạng 3", "dạng 4",
                    "dạng i", "dạng ii", "dạng iii", "dạng iv",
                    "i.", "ii.", "iii.", "iv.",
                    "<g0>", "<g1>", "<g2>", "<g3>",
                    "<#g0>", "<#g1>", "<#g2>", "<#g3>",
                    "---HẾT", "---", "- Thí sinh không", "- Giám thị không"
                };

                var questionPatterns = new[]
                {
                    "Câu [0-9]{1,3} ", "Câu [0-9]{1,3}:", "Câu [0-9]{1,3}.",
                    "Câu ? ", "Câu ?? ", "Câu ??? ",
                    "Câu ?:", "Câu ??:", "Câu ???:",
                    "Câu ?.", "Câu ??.", "Câu ???."
                };

                foreach (Word.Paragraph paragraph in document.Paragraphs)
                {
                    try
                    {
                        //paragraph.set_Style("Normal");  lênh này phá để test :D
                        string str = paragraph.Range.Text.Trim();

                        var rangeParagraph = paragraph.Range;
                        var format = rangeParagraph.ParagraphFormat;

                        rangeParagraph.ListFormat.RemoveNumbers();
                        format.OutlineLevel = WdOutlineLevel.wdOutlineLevelBodyText;

                        format.TabStops.ClearAll();
                        format.Alignment = WdParagraphAlignment.wdAlignParagraphJustify;
                        format.LeftIndent = format.RightIndent = format.FirstLineIndent = 0f;
                        format.SpaceBefore = format.SpaceAfter = 0f;
                        format.KeepWithNext = format.KeepTogether = 0;
                        format.LineSpacingRule = WdLineSpacing.wdLineSpaceMultiple;
                        format.LineSpacing = 14.4f;

                        await _interopWordService.ClearTabStopsAsync(paragraph);

                        // Xóa dòng thừa
                        if (string.IsNullOrEmpty(str) ||
                            str.Equals(Constants.QUESTION_TEMPLATE) ||
                            str.Equals(Constants.ANSWER_TEMPLATE) ||
                            removeStarts.Any(prefix => str.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                        {
                            paragraph.Range.Delete();
                            continue;
                        }


                        if (str.StartsWith("Câu", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var pattern in questionPatterns)
                            {
                                await _interopWordService.ReplaceFirstAsync(paragraph, pattern, Constants.QUESTION_TEMPLATE, matchWildcards: true);
                            }
                        }

                        // Thay kí hiệu câu hỏi True/False
                        var match = Regex.Match(str, @"^\s*([a-d])[\.\)]");
                        if (match.Success)
                        {
                            var label = match.Groups[1].Value + ") ";
                            await _interopWordService.ReplaceFirstAsync(paragraph, match.Value.Trim(), label);
                        }

                        if (typeExam is ExamType.Intest or ExamType.MasterTest)
                        {
                            var matchTF = Regex.Match(str, @"^([a-d])\)");
                            if (matchTF.Success)
                            {
                                var rangeTF = paragraph.Range;
                                bool isUnderlined = rangeTF.Characters[1].Font.Underline == Word.WdUnderline.wdUnderlineSingle
                                                 && rangeTF.Characters[2].Font.Underline == Word.WdUnderline.wdUnderlineSingle;
                                string replacement = isUnderlined ? "<Đ>" : "<S>";
                                await _interopWordService.ReplaceFirstAsync(paragraph, matchTF.Value.Trim(), replacement);
                            }
                        }

                        // Nếu chỉ chứa 1 hình ảnh và "/"
                        if (paragraph.Range.InlineShapes.Count == 1 && str == "/")
                        {
                            paragraph.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                        }
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(paragraph);
                    }
                }

                // Thay kí hiệu câu hỏi
                string symbolQuestion = typeExam switch
                {
                    ExamType.MasterTest => "<#>",
                    ExamType.Intest => "<#>",
                    ExamType.MCMix => "[<br>]",
                    ExamType.SmartTest => "#",
                    _ => string.Empty
                };

                if (string.IsNullOrEmpty(symbolQuestion))
                {
                    await _interopWordService.SetQuestionsToNumberAsync(document);
                }
                else
                {
                    await _interopWordService.ReplaceAsync(document, new Dictionary<string, string>
                    {
                        [Constants.QUESTION_TEMPLATE] = symbolQuestion
                    }, true);
                }

                // Thay kí hiệu đáp án
                if (typeExam == ExamType.MasterTest || typeExam == ExamType.Intest)
                {
                    await _interopWordService.ReplaceAsync(document, new Dictionary<string, string>
                    {
                        [Constants.ANSWER_TEMPLATE] = "<$>"
                    }, true);
                    await _interopWordService.ReplaceUnderlineWithRedTextAsync(document);
                }
                else
                {
                    await _interopWordService.SetAnswersToABCDAsync(document);
                }

                // Thay những cái còn sót
                await _interopWordService.ReplaceUntilDoneAsync(document, new Dictionary<string, string>
                {
                    ["^p "] = "^p",
                    [" ^p"] = "^p",
                    ["  "] = " ",
                    ["<#> "] = "<#>",
                    ["<Đ> "] = "<Đ>",
                    ["<S> "] = "<S>",
                });

                if (MixInfo.IsFixMathType)
                {
                    await _interopWordService.FixMathTypeAsync(document);
                }

                await _interopWordService.FormatQuestionAndAnswerAsync(document);
                await _interopWordService.SaveDocumentAsync(document);
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex);
            }
            finally
            {
                if (document != null)
                {
                    await _interopWordService.CloseDocumentAsync(document);
                    await _interopWordService.QuitWordAppAsync();
                }
            }
        }

        [RelayCommand]
        private async Task Mix()
        {
            if (!File.Exists(DestinationFile))
                return;

            try
            {
                OutputFolder = Path.Combine(Path.GetDirectoryName(DestinationFile)!, "ezMix");
                // Xóa thư mục nếu đã tồn tại
                if (Directory.Exists(OutputFolder))
                {
                    Directory.Delete(OutputFolder, true);
                }

                Directory.CreateDirectory(OutputFolder);

                var versions = ExamCodes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (versions.Length == 0)
                {
                    MessageHelper.Error("Chưa tạo danh sách mã đề!");
                    return;
                }

                MixInfo.Versions = versions;
                MixInfo.FontFamily = FontFamily;
                MixInfo.FontSize = FontSize;
                await _openXMLService.GenerateShuffledExamsAsync(DestinationFile, OutputFolder, MixInfo);

                MessageHelper.Success("Trộn đề hoàn tất!");

                if (Directory.Exists(OutputFolder))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = OutputFolder,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex);
            }
        }

        [RelayCommand]
        private void SaveMixInfo()
        {
            try
            {
                // 💾 Lưu đối tượng MixInfo vào file XML cấu hình
                MixInfo.FontFamily = FontFamily;
                MixInfo.FontSize = FontSize;
                XmlHelper.SaveToXml(Constants.XmlFilePath, MixInfo);

                // ✅ Thông báo lưu thành công
                MessageHelper.Success("Đã lưu thông tin cấu hình");
            }
            catch (Exception ex)
            {
                // ❌ Báo lỗi nếu có sự cố khi lưu
                MessageHelper.Error($"Lỗi khi lưu cấu hình: {ex.Message}");
            }
        }

        [RelayCommand]
        private void LoadMixInfo()
        {
            try
            {
                var dialog = MessageHelper.Question("Bạn có chắc chắn muốn nạp lại cấu hình mặc định không?", "Xác nhận", System.Windows.MessageBoxImage.Question);
                if (dialog == System.Windows.MessageBoxResult.No)
                    return;

                var defaultInfo = new MixInfo();
                FontFamily = MixInfo.FontFamily = defaultInfo.FontFamily;
                FontSize = MixInfo.FontSize = defaultInfo.FontSize;
                XmlHelper.SaveToXml(Constants.XmlFilePath, MixInfo);
                MessageHelper.Success("Đã nạp lại cấu hình định");

            }
            catch (Exception ex)
            {
                MessageHelper.Error($"Lỗi khi nạp cấu hình: {ex.Message}");
            }
        }

        [RelayCommand]
        private void GenerateRandomExamCodes()
        {
            var codes = new HashSet<string>();
            Random random = new();

            codes.Add("000");
            for (int i = 0; i < MixInfo.NumberOfVersions; i++)
            {
                string code = $"{(i % 9 + 1)}{random.Next(99):D2}";
                codes.Add(code);
            }

            ExamCodes = string.Join(" ", codes.OrderBy(c => c));
        }

        [RelayCommand]
        private void GenerateSequentialExamCodes()
        {
            string prefix = string.IsNullOrWhiteSpace(MixInfo.StartCode) ? "00" : MixInfo.StartCode.Trim();

            var codes = new List<string> { "000" }
                .Concat(Enumerable.Range(1, MixInfo.NumberOfVersions)
                .Select(i => $"{prefix}{i:D2}"));

            ExamCodes = string.Join(" ", codes);
        }

        [RelayCommand]
        private async Task LoadExamAsync()
        {
            if (File.Exists(DestinationFile))
            {
                InputText = await _openXMLService.ExtractTextAsync(DestinationFile);
            }
        }

        [RelayCommand]
        private async Task AnalyzeByGeminiAsync()
        {
            if (string.IsNullOrWhiteSpace(InputText)) return;

            try
            {
                ResultText = "Đang phân tích...";
                string promptAnalyzeExam = string.Format(Prompts["PromtAnalyzeExam"], MixInfo.Subject, MixInfo.Grade);
                string prompt = $"{promptAnalyzeExam}\n\nĐỀ KIỂM TRA:\n{InputText}";
                ResultText = await _geminiService.CallGeminiAsync(GeminiModel, GeminiAPIKey, prompt);
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi cho người dùng
                ResultText = $"❌ Có lỗi xảy ra khi kiểm tra chính tả: {ex.Message}";
                // Nếu muốn log chi tiết hơn:
                // Debug.WriteLine(ex.ToString());
            }
        }

        [RelayCommand]
        private void ResetPrompt()
        {
            Prompts = new Dictionary<string, string>
            {
                ["PromtAnalyzeExam"] = Constants.PromptAnalyzeExam,
                ["PromptOcrMathToLatex"] = Constants.PromptOcrMathToLatex,
                ["PromptOcrMathToMathML"] = Constants.PromptOcrMathToMathML
            };

            JsonHelper.SaveToJson(Constants.ConfigFile, Prompts);

            LoadPromptJson();

            MessageHelper.Success("✅ Prompt đã được reset về mặc định");
        }

        [RelayCommand]
        private void SavePrompt()
        {
            try
            {
                File.WriteAllText(Constants.ConfigFile, PromptJson);
                MessageHelper.Success("💾 PromptJson đã được lưu thành công");
            }
            catch (Exception ex)
            {
                MessageHelper.Error($"❌ Lỗi khi lưu PromptJson: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task LoadPdfAndOcrAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(GeminiModel) || string.IsNullOrWhiteSpace(GeminiAPIKey))
                {
                    InputText += "\n⚠️ Chưa nhập Gemini Model hoặc Gemini API Key, không thể chạy.";
                    return;
                }

                var path = FileHelper.BrowsePdf();
                if (!string.IsNullOrEmpty(path))
                {
                    InputText = $"Đã chọn: {path}\n\n";
                    InputText += "Đang trích xuất văn bản từ PDF...\n";

                    var result = await ExtractTextByGeminiAsync(GeminiModel, GeminiAPIKey, path);
                    InputText += result;
                }
            }
            catch (Exception ex)
            {
                // Ghi log hoặc hiển thị thông báo lỗi
                InputText += $"\nLỗi khi xử lý PDF: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task LoadImageAndOcrAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(GeminiModel) || string.IsNullOrWhiteSpace(GeminiAPIKey))
                {
                    InputText += "\n⚠️ Chưa nhập Gemini Model hoặc Gemini API Key, không thể chạy.";
                    return;
                }

                var path = FileHelper.BrowseImage();
                if (!string.IsNullOrEmpty(path))
                {
                    InputText = $"Đã chọn:\n{path}\n\n";
                    InputText += "Đang trích xuất văn bản từ ảnh...\n";

                    var result = await ExtractTextByGeminiAsync(GeminiModel, GeminiAPIKey, path);
                    InputText += result;
                }
            }
            catch (Exception ex)
            {
                // Ghi log hoặc hiển thị thông báo lỗi
                InputText += $"\nLỗi khi xử lý ảnh: {ex.Message}";
            }
        }

        [RelayCommand]
        private void OpenFile(string path)
        {
            FileHelper.OpenFile(path);
        }

        private void AddLog(string message)
        {
            if (string.IsNullOrWhiteSpace(ProcessContent))
            {
                ProcessContent = message;
            }
            else
            {
                ProcessContent = $"{ProcessContent}{Environment.NewLine}{message}";
            }
        }

        private void ResetLog()
        {
            if (MixInfo.IsDeleteLogWhenStart)
            {
                ProcessContent = string.Empty;
            }
        }

        public string ParseGeminiResponse(string jsonResponse)
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            var text = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text!;
        }

        private void LoadPromptJson()
        {
            PromptJson = JsonSerializer.Serialize(Prompts, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        private async Task<string> ExtractTextByGeminiAsync(string model, string apiKey, string path)
        {
            try
            {
                string ext = Path.GetExtension(path).ToLowerInvariant();
                string text = string.Empty;

                switch (ext)
                {
                    case ".pdf":
                        text = await _geminiService.CallGeminiExtractTextFromPdfAsync(model, apiKey, path, null!);
                        break;

                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".bmp":
                        text = await _geminiService.CallGeminiExtractTextFromImageAsync(model, apiKey, path, null!);
                        break;

                    default:
                        MessageHelper.Error("Định dạng tệp không được hỗ trợ");
                        return string.Empty;
                }

                return ParseGeminiResponse(text);
            }
            catch (Exception ex)
            {
                MessageHelper.Error($"Lỗi khi xử lý OCR: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
