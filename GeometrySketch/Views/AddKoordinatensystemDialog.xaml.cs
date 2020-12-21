﻿using GeometrySketch.ViewModels;
using Windows.UI.Xaml;

namespace GeometrySketch.Views
{
    public sealed partial class AddKoordinatensystemDialog
    {
        public MainViewModel ViewModel { get; }

        public AddKoordinatensystemDialog(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();            
        }
    }
}
