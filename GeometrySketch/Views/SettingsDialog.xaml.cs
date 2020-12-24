using GeometrySketch.ViewModels;

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
