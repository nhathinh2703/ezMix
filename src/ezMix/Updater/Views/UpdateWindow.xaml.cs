using System.Windows;
using Updater.ViewModels;

namespace Updater.Views
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private readonly UpdateViewModel _vm;
        private readonly string _zipUrl;
        private readonly string _targetExe;

        public UpdateWindow(UpdateViewModel vm, string zipUrl, string targetExe)
        {
            InitializeComponent();
            DataContext = vm;


            _vm = vm;
            _zipUrl = zipUrl;
            _targetExe = targetExe;

            Loaded += UpdateWindow_Loaded;
        }
        private async void UpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _vm.RunAsync(_zipUrl, _targetExe);
        }
    }
}
