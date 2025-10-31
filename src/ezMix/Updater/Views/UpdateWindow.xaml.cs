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

        public UpdateWindow(UpdateViewModel vm, string sourceExe, string targetExe)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            Loaded += async (_, _) => await _vm.RunAsync(sourceExe, targetExe);
        }
    }
}
