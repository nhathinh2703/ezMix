using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.Helpers;
using Desktop.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace Desktop.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider? _serviceProvider;
        public ObservableCollection<MenuItem> Menus { get; }

        [ObservableProperty] private ObservableObject? currentViewModel;
        [ObservableProperty] private bool isMenuExpanded = true;
        [ObservableProperty] private double menuWidth = 140;
        [ObservableProperty] private string appVersion = string.Empty;
        [ObservableProperty] private string currentTitle = "Trang chủ";

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Menus = new ObservableCollection<MenuItem>
            {
                new("🏠", "Trang chủ", typeof(HomeViewModel)),
                new("🔀", "Trộn đề", typeof(MixViewModel)),
                new("🛠️", "Tiện ích", typeof(UtilityViewModel)),
                //new("❓", "Hỗ trợ", null)
                //{
                //    Children =
                //    {
                //        new("🔄", "Cập nhật", null),
                //        new("📞", "Liên hệ", null),
                //        new("📘", "Hướng dẫn", null),
                //    }
                //}
            };

            var version = Assembly.GetExecutingAssembly().GetName().Version!;
            AppVersion = $"{version.Major}.{version.Minor}.{version.Build}";
            CurrentViewModel = _serviceProvider?.GetService(typeof(HomeViewModel)) as ObservableObject;
        }

        [RelayCommand]
        private void ToggleMenu()
        {
            IsMenuExpanded = !IsMenuExpanded;
            MenuWidth = IsMenuExpanded ? 130 : 55;
        }

        [RelayCommand]
        private void ChangeView(MenuItem? menu)
        {
            if (menu == null || menu?.ViewModelType == null) return;

            var vm = _serviceProvider!.GetService(menu.ViewModelType) as ObservableObject;
            if (vm != null)
            {
                CurrentViewModel = vm;
                CurrentTitle = menu.Title;
            }
        }

        [RelayCommand]
        private static void OpenZalo()
        {
            string url = Constants.ZaloGroup;
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
