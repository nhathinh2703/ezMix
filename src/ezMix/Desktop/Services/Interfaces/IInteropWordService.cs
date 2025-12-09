using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.Office.Interop.Word;
using Task = System.Threading.Tasks.Task;

namespace Desktop.Services.Interfaces
{
    public interface IInteropWordService
    {
        Task<Document> OpenDocumentAsync(string filePath, bool visible);

        Task SaveDocumentAsync(_Document document);

        Task CloseDocumentAsync(_Document document);

        Task CloseAllDocumentsAsync();

        Task QuitWordAppAsync();

        Task FormatDocumentAsync(_Document document);

        Task ReplaceAsync(
            _Document document, 
            Dictionary<string, string> replacements, 
            bool matchCase = false, 
            bool matchWholeWord = false, 
            bool matchWildcards = false);

        Task ReplaceFirstAsync(
            Paragraph paragraph, 
            string findText, 
            string replaceWithText, 
            bool matchCase = false, 
            bool matchWholeWord = false, 
            bool matchWildcards = false);

        Task ReplaceUntilDoneAsync(
            _Document document,
            Dictionary<string, string> replacements,
            bool matchCase = false,
            bool matchWholeWord = false,
            bool matchWildcards = false,
            int maxIterations = 100);

        Task ReplaceInSectionAsync(
            _Document document,
            int sectionIndex,
            string findText,
            string replaceWithText,
            bool matchCase = false,
            bool matchWholeWord = false,
            bool matchWildcards = false);

        Task ReplaceRedTextWithUnderlineAsync(_Document document);

        Task ReplaceUnderlineWithRedTextAsync(_Document document);

        Task ReplaceInRangeAsync(
            _Document document,
            int start,
            int end,
            string findText,
            string replaceWithText,
            bool matchCase = false,
            bool matchWholeWord = false,
            bool matchWildcards = false);

        Task ConvertListFormatToTextAsync(_Document document);

        Task DeleteAllHeadersAndFootersAsync(_Document document);

        Task SetAnswersToABCDAsync(_Document document);

        Task SetQuestionsToNumberAsync(_Document document);

        Task FormatQuestionAndAnswerAsync(_Document document);

        Task UpdateFieldsAsync(string filePath);

        Task ClearTabStopsAsync(Paragraph paragraph);

        Task<int> FixMathTypeAsync(_Document document);

        Task<int> ConvertEquationToMathTypeAsync(_Document document);

        Task RejectAllChangesAsync(_Document document);
    }
}
