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

        private bool _firstStartOnBuild1_2_1;
        public bool FirstStartOnBuild1_2_1
        {
            get { return _firstStartOnBuild1_2_1; }
            set { _firstStartOnBuild1_2_1 = value; OnPropertyChanged(); }
        }

        public Settings()
        {
            FirstStartOnBuild1_2_1 = true;
        }
    }
}
