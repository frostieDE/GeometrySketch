using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace GeometrySketch.Commons
{
    public static class EraserHelper
    {
        //Points on Cubic Bezier Curve siehe https://www.cubic.org/docs/bezier.htm
        public static Point lerp(Point a, Point b, float t)
        {
            Point p = new Point()
            {
                X = a.X + (b.X - a.X) * t,
                Y = a.Y + (b.Y - a.Y) * t,

            };
            return p;
        }
        public static Point bezier(Point a, Point b, Point c, Point d, float t)
        {
            Point ab = new Point();
            Point bc = new Point();
            Point cd = new Point();
            Point abbc = new Point();
            Point bccd = new Point();

            Point bezierPt = new Point();

            ab = lerp(a, b, t);           // point between a and b
            bc = lerp(b, c, t);           // point between b and c
            cd = lerp(c, d, t);           // point between c and d
            abbc = lerp(ab, bc, t);       // point between ab and bc
            bccd = lerp(bc, cd, t);       // point between bc and cd
            bezierPt = lerp(abbc, bccd, t);   // point on the bezier-curve

            return bezierPt;
        }
        public static List<Point> PointsOnSegment(Point startPt, Point controlPt1, Point controlPt2, Point PositionPt)
        {
            List<Point> points = new List<Point>();
            
            for (int i = 0; i < 10; i++)
            {
                Point p = new Point();
                float t = (float)(i) / 9.0f;
                p = bezier(startPt, controlPt1, controlPt2, PositionPt, t);
                points.Add(p);
            }

            return points;
        }
        public static List<Point> GetPointsOnStroke(InkStroke inst)
        {
            List<InkStrokeRenderingSegment> renderingSegments = new List<InkStrokeRenderingSegment>();

            List<Point> points = new List<Point>();
            //First Point on InkStroke
            points.Add(new Point() { X = inst.GetInkPoints().First().Position.X, Y = inst.GetInkPoints().First().Position.Y, });

            foreach (InkStrokeRenderingSegment isrs in inst.GetRenderingSegments())
            {
                List<Point> pointsOnSg = new List<Point>();
                pointsOnSg = PointsOnSegment(points.Last(), isrs.BezierControlPoint1, isrs.BezierControlPoint2, isrs.Position);
                points.AddRange(pointsOnSg);
            }

            return points;
        }

        public static bool PointInRectangle(Point ap, Point ec, int eraserwidth)
        {
            if ((ap.X >= ec.X - eraserwidth && ap.X <= ec.X + eraserwidth) && (ap.Y >= ec.Y - eraserwidth && ap.Y <= ec.Y + eraserwidth))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
