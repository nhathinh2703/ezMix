using CommunityToolkit.Mvvm.ComponentModel;

namespace Desktop.Models
{
    public partial class ProgressOverlay : ObservableObject
    {
        [ObservableProperty] private bool isVisible = false;
        [ObservableProperty] private bool isIndeterminate = false;
        [ObservableProperty] private double progressValue;
        [ObservableProperty] private string statusText = "Đang xử lý...";
    }
}
