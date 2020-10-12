using GeometrySketch.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Input.Inking;

namespace GeometrySketch.Model
{
    public class InkPage: Observable
    {
        public InkPage()
        {
            IsGridVisible = true;            
        }
        private bool _isGridVisible;
        public bool IsGridVisible { get=>_isGridVisible; set { _isGridVisible = value; OnPropertyChanged(); } }              
    }
}
