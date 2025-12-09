using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.Helpers;
using Desktop.Services.Interfaces;

namespace Desktop.ViewModels
{
    public partial class UtilityViewModel : ObservableObject
    {
        private readonly IOpenXMLService _openXMLService;
        private readonly IInteropWordService _interopWordService;

        public UtilityViewModel(IOpenXMLService openXMLService, IInteropWordService interopWordService)
        {
            _openXMLService = openXMLService;
            _interopWordService = interopWordService;
        }

        [RelayCommand]
        private async Task FixMathType()
        {
            try
            {
                var filePath = FileHelper.BrowseFile();
                if (string.IsNullOrEmpty(filePath))
                    return;

                var document = await _interopWordService.OpenDocumentAsync(filePath, visible: true);
                document.Activate();

                int count = await _interopWordService.FixMathTypeAsync(document);

                MessageHelper.Success($"✅ Đã xử lý {count} công thức MathType.");
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex);
            }
        }
    }
}
