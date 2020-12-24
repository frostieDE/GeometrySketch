using GeometrySketch.ViewModels;

namespace GeometrySketch.Views
{
    public sealed partial class FirstStartDialog
    {
        public MainViewModel ViewModel { get; }

        public FirstStartDialog(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
        }
    }
}
