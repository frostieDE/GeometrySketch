using GeometrySketch.Base;

namespace GeometrySketch.Model
{
    public class Settings : Observable
    {
        private int _theme;
        public int Theme
        {
            get { return _theme; }
            set { _theme = value; OnPropertyChanged(); }
        }

        public Settings()
        {

        }
    }
}
