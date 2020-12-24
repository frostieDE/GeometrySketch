using System;
using System.Linq;
using System.Numerics;
using Windows.Foundation;

namespace GeometrySketch.Commons
{
    public static class GeometryHelper
    {
        //nrp = nearest point, ap = actual point, sw = strokewidth
        public static Point NewInkPoint(Point nrp, Point ap, double sw)
        {
            Point pt = new Point();

            Vector2 vector_a = new Vector2((float)(ap.X - nrp.X), (float)(ap.Y - nrp.Y));
            vector_a = Vector2.Normalize(vector_a);

            pt.X = nrp.X + 0.5 * sw * vector_a.X;
            pt.Y = nrp.Y + 0.5 * sw * vector_a.Y;

            return pt;
        }

        public static Point NearestPointOnGeodreieck(Point p1, Point p2, Point p3, Point p)
        {
            Point pt = new Point();

            Point f1 = Lotfusspunkt(p1, p2, p);
            Point f2 = Lotfusspunkt(p2, p3, p);
            Point f3 = Lotfusspunkt(p1, p3, p);

            double[] ds = new double[6];

            if (DistancePointPoint(f1, p1) < DistancePointPoint(p1, p2) && DistancePointPoint(f1, p2) < DistancePointPoint(p1, p2))
            {
                ds[0] = DistancePointLine(p1, p2, p);
            }
            else
            {
                ds[0] = 10000;
            }
            if (DistancePointPoint(f2, p2) < DistancePointPoint(p2, p3) && DistancePointPoint(f2, p3) < DistancePointPoint(p2, p3))
            {
                ds[1] = DistancePointLine(p3, p2, p);
            }
            else
            {
                ds[1] = 10000;
            }
            if (DistancePointPoint(f3, p1) < DistancePointPoint(p1, p3) && DistancePointPoint(f3, p3) < DistancePointPoint(p1, p3))
            {
                ds[2] = DistancePointLine(p3, p1, p);
            }
            else
            {
                ds[2] = 10000;
            }

            ds[3] = DistancePointPoint(p, p1);
            ds[4] = DistancePointPoint(p, p2);
            ds[5] = DistancePointPoint(p, p3);


            int i = Array.IndexOf(ds, ds.Min());

            if (i == 0)
            {
                pt = f1;
            }
            else if (i == 1)
            {
                pt = f2;
            }
            else if (i == 2)
            {
                pt = f3;
            }
            else if (i == 3)
            {
                pt = p1;
            }
            else if (i == 4)
            {
                pt = p2;
            }
            else if (i == 5)
            {
                pt = p3;
            }
            return pt;
        }

        public static double MinimalDistanceToGeodreieck(Point p1, Point p2, Point p3, Point p)
        {
            double[] ds = new double[6];

            if (DistancePointPoint(Lotfusspunkt(p1, p2, p), p1) <= DistancePointPoint(p1, p2) && DistancePointPoint(Lotfusspunkt(p1, p2, p), p2) <= DistancePointPoint(p1, p2))
            {
                ds[0] = DistancePointLine(p1, p2, p);
            }
            else
            {
                ds[0] = 1000;
            }
            if (DistancePointPoint(Lotfusspunkt(p2, p3, p), p2) <= DistancePointPoint(p2, p3) && DistancePointPoint(Lotfusspunkt(p2, p3, p), p3) <= DistancePointPoint(p2, p3))
            {
                ds[1] = DistancePointLine(p2, p3, p);
            }
            else
            {
                ds[1] = 1000;
            }
            if (DistancePointPoint(Lotfusspunkt(p1, p3, p), p1) <= DistancePointPoint(p1, p3) && DistancePointPoint(Lotfusspunkt(p1, p3, p), p3) <= DistancePointPoint(p1, p3))
            {
                ds[2] = DistancePointLine(p1, p3, p);
            }
            else
            {
                ds[2] = 1000;
            }
            ds[3] = DistancePointPoint(p, p1);
            ds[4] = DistancePointPoint(p, p2);
            ds[5] = DistancePointPoint(p, p3);

            double d = ds.Min();
            return d;
        }

        public static bool PointIsInPolygon(Point p1, Point p2, Point p3, Point p)
        {
            Point[] polygon = new Point[3];
            polygon[0] = p1;
            polygon[1] = p2;
            polygon[2] = p3;

            bool result = false;
            var a = polygon.Last();
            foreach (var b in polygon)
            {
                if ((b.X == p.X) && (b.Y == p.Y))
                    return true;

                if ((b.Y == a.Y) && (p.Y == a.Y) && (a.X <= p.X) && (p.X <= b.X))
                    return true;

                if ((b.Y < p.Y) && (a.Y >= p.Y) || (a.Y < p.Y) && (b.Y >= p.Y))
                {
                    if (b.X + (p.Y - b.Y) / (a.Y - b.Y) * (a.X - b.X) <= p.X)
                        result = !result;
                }
                a = b;
            }
            return result;
        }

        public static double DistancePointLine(Point a, Point b, Point p)
        {
            double m;

            Vector2 vector_a = new Vector2((float)a.X, (float)a.Y);
            Vector2 vector_v = new Vector2((float)(b.X - a.X), (float)(b.Y - a.Y));
            Vector2 vector_p = new Vector2((float)p.X, (float)p.Y);
            Vector2 vector_pa = new Vector2(vector_p.X - vector_a.X, vector_p.Y - vector_a.Y);

            m = (vector_pa.X * vector_v.X + vector_pa.Y * vector_v.Y) / (vector_v.X * vector_v.X + vector_v.Y * vector_v.Y);
            double d = DistancePointPoint(p, Lotfusspunkt(a, b, p));
            return d;
        }

        public static Point Lotfusspunkt(Point a, Point b, Point p)
        {
            Point f = new Point();

            Vector2 vector_a = new Vector2((float)a.X, (float)a.Y);
            Vector2 vector_v = new Vector2((float)(b.X - a.X), (float)(b.Y - a.Y));
            Vector2 vector_p = new Vector2((float)p.X, (float)p.Y);
            Vector2 vector_pa = new Vector2(vector_p.X - vector_a.X, vector_p.Y - vector_a.Y);
            Vector2 vector_f = new Vector2();

            double m;
            m = (vector_pa.X * vector_v.X + vector_pa.Y * vector_v.Y) / (vector_v.X * vector_v.X + vector_v.Y * vector_v.Y);

            vector_f = vector_a + Vector2.Multiply((float)m, vector_v);

            f.X = vector_f.X;
            f.Y = vector_f.Y;

            return f;
        }

        public static double DistancePointPoint(Point a, Point b)
        {
            double d;
            Vector2 vector_d = new Vector2((float)(b.X - a.X), (float)(b.Y - a.Y));
            d = vector_d.Length();

            return d;
        }
    }
}
