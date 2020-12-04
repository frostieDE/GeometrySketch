using GeometrySketch.ViewModels;
using Windows.UI.Xaml;

namespace GeometrySketch.Views
{
    public sealed partial class SettingsDialog
    {
        public MainViewModel ViewModel { get; }

        public SettingsDialog(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();            
        }
    }
}
