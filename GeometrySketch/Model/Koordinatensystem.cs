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

        private int _originPositionY;
        public int OriginPositionY { get => _originPositionY; set { _originPositionY = value; OnPropertyChanged(); } }
        private int _originPositonX;
        public int OriginPsoitionX { get => _originPositonX; set { _originPositonX = value; OnPropertyChanged(); } }

        //lE in Kästchen
        private int _lE;
        public int LE { get => _lE; set { _lE = value; OnPropertyChanged(); } }
    }    
}
