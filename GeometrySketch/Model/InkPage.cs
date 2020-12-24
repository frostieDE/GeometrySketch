using GeometrySketch.Base;

namespace GeometrySketch.Model
{
    public class InkPage : Observable
    {
        public InkPage()
        {
            IsGridVisible = true;
        }
        private bool _isGridVisible;
        public bool IsGridVisible { get => _isGridVisible; set { _isGridVisible = value; OnPropertyChanged(); } }
    }
}
