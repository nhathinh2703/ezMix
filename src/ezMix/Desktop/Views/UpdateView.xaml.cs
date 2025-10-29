using Desktop.ViewModels;
using System.Windows;

namespace Desktop.Views
{
    /// <summary>
    /// Interaction logic for UpdateView.xaml
    /// </summary>
    public partial class UpdateView : Window
    {
        public UpdateView(UpdateViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
