using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.Helpers;
using Desktop.Models;
using Desktop.Models.Enums;
using Desktop.Services.Interfaces;
using Microsoft.Office.Interop.Word;
using MTGetEquationAddin;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Task = System.Threading.Tasks.Task;
using Word = Microsoft.Office.Interop.Word;

namespace Desktop.ViewModels
{
    public partial class NormalizationViewModel : ObservableObject
    {
        private readonly IOpenXMLService _openXMLService;
        private readonly IInteropWordService _interopWordService;

        [ObservableProperty] private string sourceFile = string.Empty;
        [ObservableProperty] private string destinationFile = string.Empty;
        [ObservableProperty] private ObservableCollection<Question> questions = new ObservableCollection<Question>();
        [ObservableProperty] private bool isCenterImage = true;
        [ObservableProperty] private bool isBorderImage = true;

        [ObservableProperty] private ObservableCollection<ExamType> examTypes = new ObservableCollection<ExamType>();
        [ObservableProperty] private ExamType selectedExamType = ExamType.ezMix;

        private const string XmlFilePath = "config.xml";

        [ObservableProperty] private string outputFolder = string.Empty;
        [ObservableProperty] private bool isEnableMix = false;
        [ObservableProperty] private MixInfo mixInfo = new();
        [ObservableProperty] private string examCodes = string.Empty;
        [ObservableProperty] private bool isOK = false;
        [ObservableProperty] private FixedDocumentSequence? document;

        public NormalizationViewModel(IOpenXMLService openXMLService, IInteropWordService interopWordService)
        {
            _openXMLService = openXMLService;
            _interopWordService = interopWordService;

            ExamTypes = new ObservableCollection<ExamType>(Enum.GetValues(typeof(ExamType)).Cast<ExamType>());
            MixInfo = XmlHelper.LoadFromXml(XmlFilePath);
        }

        [RelayCommand]
        private async Task AnalyzeFile()
        {
            try
            {
                var sourcePath = BrowseFile();
                if (string.IsNullOrEmpty(sourcePath))
                    return;

                SourceFile = sourcePath;

                string folder = Path.Combine(Path.GetDirectoryName(sourcePath)!, Path.GetFileNameWithoutExtension(sourcePath));
                if (Directory.Exists(folder))
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    foreach (var file in Directory.GetFiles(folder))
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    }
                    Directory.Delete(folder, true);
                }
                Directory.CreateDirectory(folder);

                string fileName = $"{SelectedExamType}_{Path.GetFileName(sourcePath)}";
                string targetPath = Path.Combine(folder, fileName);

                if (File.Exists(targetPath))
                    File.Delete(targetPath);

                File.Copy(sourcePath, targetPath);
                DestinationFile = targetPath;

                await ProcessDocumentAsync(DestinationFile, SelectedExamType);

                var result = await _openXMLService.ParseDocxQuestionsAsync(DestinationFile);
                Questions = new ObservableCollection<Question>(result);
                IsOK = Questions.All(q => q.IsValid);
                IsEnableMix = !string.IsNullOrEmpty(SourceFile) && File.Exists(SourceFile) && IsOK;

                MessageHelper.Success($"Chuẩn hóa theo ({SelectedExamType}) thành công!");
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex);
            }
        }

        [RelayCommand]
        private async Task RecognitionFile()
        {
            try
            {
                var filePath = BrowseFile();
                if (string.IsNullOrEmpty(filePath))
                    return;

                SourceFile = DestinationFile = filePath;

                var result = await _openXMLService.ParseDocxQuestionsAsync(filePath);
                Questions = new ObservableCollection<Question>(result);
                IsOK = Questions.All(q => q.IsValid);
                IsEnableMix = !string.IsNullOrEmpty(SourceFile) && File.Exists(SourceFile) && IsOK;

                //string xpsPath = _interopWordService.ConvertDocxToXps(SourceFile);
                //using var xpsDoc = new XpsDocument(xpsPath, FileAccess.Read);
                //Document = xpsDoc.GetFixedDocumentSequence();
            }
            catch (Exception ex)
            {
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
                    ["<#>"] = Constants.QUESTION_TEMPLATE,
                    ["#"] = Constants.QUESTION_TEMPLATE,
                    ["[<br>]"] = Constants.QUESTION_TEMPLATE,
                    ["<NB>"] = Constants.QUESTION_TEMPLATE,
                    ["<TH>"] = Constants.QUESTION_TEMPLATE,
                    ["<VD>"] = Constants.QUESTION_TEMPLATE,
                    ["<VDC>"] = Constants.QUESTION_TEMPLATE,
                    ["^p "] = "^p",
                    [" ^p"] = "^p",
                    ["  "] = " "
                };

                await _interopWordService.FindAndReplaceAsync(document, replacements, matchCase: true, matchWholeWord: false);

                await _interopWordService.FindAndReplaceRedToUnderlinedAsync(document);

                Word.Range range = document.Range();
                range.Font.Name = "Times New Roman";
                range.Font.Size = 12;
                range.Font.Color = WdColor.wdColorBlack;

                foreach (Word.Paragraph paragraph in document.Paragraphs)
                {
                    try
                    {
                        paragraph.set_Style("Normal");
                        string str = paragraph.Range.Text.Trim();

                        string[] removeStarts = new[]
                        {
                            "phần 1", "phần 2", "phần 3", "phần 4",
                            "phần i", "phần ii", "phần iii", "phần iv",
                            "dạng 1", "dạng 2", "dạng 3", "dạng 4",
                            "dạng i", "dạng ii", "dạng iii", "dạng iv",
                            "i.", "ii.", "iii.", "iv.",
                            "<g0>", "<g1>", "<g2>", "<g3>",
                            "<#g0>", "<#g1>", "<#g2>", "<#g3>",
                            "---HẾT"
                        };

                        if (string.IsNullOrEmpty(str) || str.Equals(Constants.QUESTION_TEMPLATE) || str.Equals(Constants.ANSWER_TEMPLATE) ||
                            removeStarts.Any(prefix => str.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                        {
                            paragraph.Range.Delete();
                            continue;
                        }

                        await _interopWordService.ClearTabStopsAsync(paragraph);

                        // Kiểm tra đoạn có bắt đầu bằng "Câu" (không phân biệt hoa thường)
                        if (str.StartsWith("Câu", StringComparison.OrdinalIgnoreCase))
                        {
                            // Các mẫu cần chuẩn hóa thành QUESTION_TEMPLATE
                            string[] patterns = { "Câu [0-9]{1,3} ", "Câu [0-9]{1,3}:", "Câu [0-9]{1,3}.",
                                "Câu ? ", "Câu ?? ", "Câu ??? ",
                                "Câu ?:", "Câu ??:", "Câu ???:",
                                "Câu ?.", "Câu ??.", "Câu ???." };

                            foreach (var pattern in patterns)
                            {
                                await _interopWordService.FindAndReplaceFirstAsync(paragraph, pattern, Constants.QUESTION_TEMPLATE, matchWildcards: true);
                            }
                        }

                        // Chuẩn hóa a./b./c./d.
                        //string[] keys = { "a.", "b.", "c.", "d.", "a)", "b)", "c)", "d)", " a.", " b.", " c.", " d.", " a)", " b)", " c)", " d)" };
                        //foreach (var key in keys)
                        //{
                        //    if (str.StartsWith(key))
                        //    {
                        //        string label = key.Trim().Substring(0, 1) + ") ";
                        //        await _interopWordService.FindAndReplaceFirstAsync(paragraph, key.Trim(), label);
                        //        break;
                        //    }
                        //}

                        //var match = Regex.Match(str, @"^\s*[a-dA-D][\.\)]");
                        //if (match.Success)
                        //{
                        //    string label = match.Value.Trim().Substring(0, 1).ToUpper() + ") ";
                        //    await _interopWordService.FindAndReplaceFirstAsync(paragraph, match.Value.Trim(), label);
                        //}


                        // Nếu chỉ chứa 1 hình ảnh và "/"
                        //if (IsCenterImage && paragraph.Range.InlineShapes.Count == 1 && str == "/")
                        //{
                        //    paragraph.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        //}
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(paragraph);
                    }
                }

                // Xử lý biểu tượng câu hỏi tùy theo loại đề
                string symbolQuestion = typeExam switch
                {
                    ExamType.MasterTest => "<#>",
                    ExamType.Intest => "<#>",
                    ExamType.MCMix => "[<br>]",
                    ExamType.SmartTest => "#",
                    _ => string.Empty
                };

                if (string.IsNullOrEmpty(symbolQuestion))
                    await _interopWordService.SetQuestionsToNumberAsync(document);
                else
                    await _interopWordService.FindAndReplaceAsync(document, new Dictionary<string, string> { [Constants.QUESTION_TEMPLATE] = symbolQuestion }, matchCase: true);

                // Xử lý đáp án
                if (typeExam == ExamType.MasterTest)
                {
                    await _interopWordService.FindAndReplaceAsync(document, new Dictionary<string, string> { [Constants.ANSWER_TEMPLATE] = "<$>" }, matchCase: true);
                }
                else
                {
                    await _interopWordService.SetAnswersToABCDAsync(document);
                }

                await _interopWordService.FormatQuestionAndAnswerAsync(document);

                //if (IsBorderImage)
                //{
                //    await _interopWordService.ProcessImagesInDocumentAsync(document, IsBorderImage);
                //}

                await _interopWordService.FindAndReplaceAsync(document, new Dictionary<string, string>
                {
                    ["  "] = " ",
                    ["^p "] = "^p",
                    [" ^p"] = "^p"
                });

                try
                {
                    Connect connect = new Connect();
                    if (document != null)
                    {
                        Word.InlineShapes shapes = (Word.InlineShapes)document.GetType().InvokeMember("InlineShapes", BindingFlags.GetProperty, null, document, null)!;

                        int numShapesIterated = 0;

                        // Iterate over all of the shapes in the collection.
                        if (shapes != null && shapes.Count > 0)
                        {
                            numShapesIterated = connect.IterateShapes(ref shapes, true, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageHelper.Error(ex);
                }

                await _interopWordService.SaveDocumentAsync(document!);
                MessageHelper.Success("Chuẩn hóa thành công!");
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
                    await _interopWordService.DisposeAsync();
                }
            }
        }

        [RelayCommand]
        private void Mix()
        {
            if (!File.Exists(DestinationFile))
                return;

            try
            {
                OutputFolder = Path.Combine(Path.GetDirectoryName(DestinationFile)!, "ezMix");
                if (!Directory.Exists(OutputFolder))
                    Directory.CreateDirectory(OutputFolder);
                else
                {
                    Directory.Delete(OutputFolder, true);
                    Directory.CreateDirectory(OutputFolder);
                }

                var versions = ExamCodes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (versions.Count() == 0)
                {
                    MessageHelper.Error("Chưa tạo danh sách mã đề!");
                    return;
                }

                MixInfo.Versions = versions;
                _openXMLService.GenerateShuffledExamsAsync(DestinationFile, OutputFolder, MixInfo);

                MessageHelper.Success("Trộn đề hoàn tất!");
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
                XmlHelper.SaveToXml(XmlFilePath, MixInfo);
                MessageHelper.Success("Đã lưu thông tin cấu hình");
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex);
            }
        }

        [RelayCommand]
        private void GenerateRandomExamCodes()
        {
            var codes = new HashSet<string>();
            Random random = new Random();

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
            int startCode = int.TryParse(MixInfo.StartCode, out var code) ? code : 1;

            var codes = new List<string> { "000" }
                .Concat(Enumerable.Range(0, MixInfo.NumberOfVersions)
                .Select(i => (startCode * 100 + (i + 1)).ToString()));

            ExamCodes = string.Join(" ", codes);
        }

        private string? BrowseFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Word Documents (*.docx)|*.docx|All Files (*.*)|*.*",
                Title = "Chọn file Word"
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        [RelayCommand]
        private void OpenFile()
        {
            if (File.Exists(DestinationFile))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = DestinationFile,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageHelper.Error(ex);
                }
            }
            else
            {
                MessageHelper.Error("Tệp không tồn tại");
            }
        }
    }
}
