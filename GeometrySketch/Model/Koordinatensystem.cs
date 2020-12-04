using GeometrySketch.Base;

namespace GeometrySketch.Model
{
    public class Koordinatensystem : Observable
    {
        private int _minX;
        public int MinX { get => _minX; set { _minX = value; OnPropertyChanged(); } }
        private int _maxX;
        public int MaxX { get => _maxX; set { _maxX = value; OnPropertyChanged(); } }

        private int _minY;
        public int MinY { get => _minY; set { _minY = value; OnPropertyChanged(); } }
        private int _maxY;
        public int MaxY { get => _maxY; set { _maxY = value; OnPropertyChanged(); } }

        private int _originY;
        public int OriginY { get => _originY; set { _originY = value; OnPropertyChanged(); } }
        private int _originX;
        public int OriginX { get => _originX; set { _originX = value; OnPropertyChanged(); } }
    }    
}
