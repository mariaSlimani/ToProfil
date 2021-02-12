using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace ToProfil
{
    public static class Fonctions
    {
        #region Fonctions utilisée pour Ajout Point d'ancrage
        static public void ClosestPoint(Polyline poly, Point pos, out Point clPtn, out Point seg1, out Point seg2) //Méthode de calcul du point le plus proche sur la polyligne (avec segment)
        {
            double savelengh = 0;
            seg1 = new Point();
            seg2 = new Point();
            clPtn = new Point();
            for (int i = 0; i < poly.Points.Count() - 1; i++)

            {
                Point start = poly.Points[i];
                Point end = poly.Points[i + 1];
                double longueur = FindDistanceToSegment(pos, start, end, out Point inter);
                if (longueur < 0) longueur *= -1;
                if (i == 0)
                {
                    seg1 = start;
                    seg2 = end;
                    clPtn = inter;
                    savelengh = longueur;
                    if (savelengh < 0) savelengh *= -1;
                }
                else if (longueur < savelengh)
                {

                    seg1 = start;
                    seg2 = end;
                    clPtn = inter;
                    savelengh = longueur;
                }
            }

        }
        // Calculate the distance between
        // point pt and the segment p1 --> p2.
        static public double FindDistanceToSegment(Point pt, Point p1, Point p2, out Point closest)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            double t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Point(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new Point(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new Point(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return Math.Sqrt(dx * dx + dy * dy);
        }
        #endregion

        #region Fonctions utilisée pour intersections
        public static bool IntersecSegSeg(Line seg1, Line seg2, ref List<Point> inters)
        {
            if ((seg1.X1 == seg1.X2 && seg1.Y1 == seg1.Y2) || (seg2.X1 == seg2.X2 && seg2.Y1 == seg2.Y2))
            { return false; }
            double a1, a2, b1, b2;
            Point inter = new Point();

            if ((seg1.X2 - seg1.X1) == 0)
            {
                a1 = (seg1.Y2 - seg1.Y1) / 0.0001;
            }
            else
            {
                a1 = (seg1.Y2 - seg1.Y1) / (seg1.X2 - seg1.X1);
            }

            if ((seg2.X2 - seg2.X1) == 0)
            {
                a2 = (seg2.Y2 - seg2.Y1) / 0.0001;
            }
            else
            {
                a2 = (seg2.Y2 - seg2.Y1) / (seg2.X2 - seg2.X1);
            }
            if ((seg2.X2 - seg2.X1) == 0 && (seg1.X2 - seg1.X1) == 0) { a1 = a2; }

            b1 = seg1.Y1 - (a1 * seg1.X1);
            b2 = seg2.Y1 - (a2 * seg2.X1);

            if (a1 == a2)
            {
                return false;
                if (b1 != b2)
                {
                    return false;
                }
                else
                {
                    Cmp_point_x c = new Cmp_point_x();
                    List<Point> lp1 = new List<Point>();
                    List<Point> lp2 = new List<Point>();

                    lp1.Add(new Point(seg1.X1, seg1.Y1));
                    lp1.Add(new Point(seg1.X2, seg1.Y2));
                    lp2.Add(new Point(seg2.X1, seg2.Y1));
                    lp2.Add(new Point(seg2.X2, seg2.Y2));

                    lp1.Sort(c);
                    lp2.Sort(c);
                    // ><
                    if ((lp1[1].X < lp2[0].X) || (lp2[1].X < lp1[0].X))
                    {

                        return false;

                    }
                    //1

                    if (lp1[1].X < lp2[0].X)
                    {

                        return false;

                    }
                    else if (lp2[1].X < lp1[0].X)
                    {

                        return false;

                    }

                    else if (lp1[0].X >= lp2[0].X && lp1[1].X <= lp2[1].X)
                    {
                        inters.Add(lp1[0]);
                        inters.Add(lp1[1]);
                        return true;

                    }
                    //2
                    else if (lp2[0].X >= lp1[0].X && lp2[1].X <= lp1[1].X)
                    {
                        inters.Add(lp2[0]);
                        inters.Add(lp2[1]);
                        return true;

                    }
                    //3
                    else if (lp1[0].X >= lp2[0].X && lp1[1].X >= lp2[1].X)
                    {
                        inters.Add(lp1[0]);
                        inters.Add(lp2[1]);
                        return true;

                    }
                    //4
                    else if (lp2[0].X >= lp1[0].X && lp2[1].X >= lp1[1].X)
                    {
                        inters.Add(lp2[0]);
                        inters.Add(lp1[1]);
                        return true;

                    }
                    //5
                    else if (lp1[0].X <= lp2[0].X && lp1[1].X <= lp2[1].X)
                    {
                        inters.Add(lp2[0]);
                        inters.Add(lp1[1]);
                        return true;

                    }
                    //6
                    else if (lp2[0].X <= lp1[0].X && lp2[1].X <= lp1[1].X)
                    {
                        inters.Add(lp1[0]);
                        inters.Add(lp2[1]);
                        return true;

                    }
                    else
                    {
                        return false;
                    }

                }

            }

            else
            {

                inter.X = (b2 - b1) / (a1 - a2);
                inter.Y = a1 * inter.X + b1;
                inters.Add(inter);


                if (((seg1.X2 - seg1.X1) == 0) && inter.X <= Math.Max(seg2.X2, seg2.X1) && inter.X >= Math.Min(seg2.X2, seg2.X1) && inter.Y >= Math.Min(seg2.Y2, seg2.Y1) && inter.Y >= Math.Min(seg1.Y2, seg1.Y1) && inter.Y <= Math.Max(seg1.Y2, seg1.Y1) && inter.Y <= Math.Max(seg2.Y2, seg2.Y1))
                {

                    return true;
                }
                else if (((seg2.X2 - seg2.X1) == 0) && inter.X <= Math.Max(seg1.X2, seg1.X1) && inter.X >= Math.Min(seg1.X2, seg1.X1) && inter.Y >= Math.Min(seg2.Y2, seg2.Y1) && inter.Y >= Math.Min(seg1.Y2, seg1.Y1) && inter.Y <= Math.Max(seg1.Y2, seg1.Y1) && inter.Y <= Math.Max(seg2.Y2, seg2.Y1))
                {
                    return true;
                }

                else if (inter.X <= Math.Max(seg1.X2, seg1.X1) && inter.X <= Math.Max(seg2.X2, seg2.X1) && inter.X >= Math.Min(seg1.X2, seg1.X1) && inter.X >= Math.Min(seg2.X2, seg2.X1) && inter.Y >= Math.Min(seg2.Y2, seg2.Y1) && inter.Y >= Math.Min(seg1.Y2, seg1.Y1) && inter.Y <= Math.Max(seg1.Y2, seg1.Y1) && inter.Y <= Math.Max(seg2.Y2, seg2.Y1))
                {
                    return true;
                }

            }
            return false;

        }  // Intesection entre segments

        static public double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((y2 - y1), 2) + Math.Pow((x2 - x1), 2));
        }

        static public bool ClosestPointSeg(ref Point closestpoint, Point p, Line l)
        {
            double a1, a2, b1, b2, a, b;

            if ((l.X2 - l.X1) == 0)
            {
                a1 = (l.Y2 - l.Y1) / 0.0001;
            }
            else
            {
                a1 = (l.Y2 - l.Y1) / (l.X2 - l.X1);
            }

            b1 = l.Y1 - (a1 * l.X1);

            a = l.X2 - l.X1;
            b = l.Y2 - l.Y1;

            a2 = -(a / b); b2 = (a * p.X + b * p.Y) / b;


            closestpoint.X = (b2 - b1) / (a1 - a2);
            closestpoint.Y = a1 * closestpoint.X + b1;
            if ((l.X2 - l.X1) == 0 && closestpoint.Y <= Math.Max(l.Y2, l.Y1) && closestpoint.Y >= Math.Min(l.Y2, l.Y1))
            {
                return true;
            }
            else if (closestpoint.X <= Math.Max(l.X2, l.X1) && closestpoint.X >= Math.Min(l.X2, l.X1) && closestpoint.Y <= Math.Max(l.Y2, l.Y1) && closestpoint.Y >= Math.Min(l.Y2, l.Y1))
            {
                return true;
            }
            else
            {
                return false;
            }

        } // Retourne le point le plus proche du segment
         
        static public void ClosestPointPolyline(ref Point closestpoint, Point p, Polyline poly)
        {
            Point point = new Point();
            Line l = new Line();
            List<Point> points = new List<Point>();
            for (int i = 0; i < poly.Points.Count - 1; i++)
            {
                l.X1 = poly.Points[i].X;
                l.Y1 = poly.Points[i].Y;
                l.X2 = poly.Points[i + 1].X;
                l.Y2 = poly.Points[i + 1].Y;

                if (ClosestPointSeg(ref point, p, l))
                {
                    points.Add(point);
                }

            }

            if (points.Count == 0)
            {
                points.AddRange(poly.Points);
                Cmp_distance cmp = new Cmp_distance(p);
                points.Sort(cmp);
                closestpoint = points[0];
            }
            else
            {
                Cmp_distance cmp = new Cmp_distance(p);
                points.Sort(cmp);
                closestpoint = points[0];

            }
        } // Retourne le point le plus proche de la polyline

        static public bool IntersecLineSeg(Line seg1, Line seg2, ref List<Point> inters)
        {
            double a1, a2, b1, b2;
            Point inter = new Point();
            if ((seg1.X1 == seg1.X2 && seg1.Y1 == seg1.Y2) || (seg2.X1 == seg2.X2 && seg2.Y1 == seg2.Y2))
            { return false; }
            if ((seg1.X2 - seg1.X1) == 0)
            {
                a1 = (seg1.Y2 - seg1.Y1) / 0.0001;
            }
            else
            {
                a1 = (seg1.Y2 - seg1.Y1) / (seg1.X2 - seg1.X1);
            }

            if ((seg2.X2 - seg2.X1) == 0)
            {
                a2 = (seg2.Y2 - seg2.Y1) / 0.0001;
            }
            else
            {
                a2 = (seg2.Y2 - seg2.Y1) / (seg2.X2 - seg2.X1);
            }

            b1 = seg1.Y1 - (a1 * seg1.X1);
            b2 = seg2.Y1 - (a2 * seg2.X1);

            if (a1 == a2)
            {
                if (b1 != b2)
                {
                    return false;
                }
                else
                {

                    inters.Add(new Point(seg2.X1, seg2.Y1));
                    inters.Add(new Point(seg2.X2, seg2.Y2));
                    return true;

                }

            }

            else
            {

                inter.X = (b2 - b1) / (a1 - a2);
                inter.Y = a1 * inter.X + b1;
                if (inters.Count == 0)
                {
                    inters.Add(inter);
                }
                else
                {
                    foreach (Point p in inters)
                    {
                        if (!((p.X - inter.X) < 1 && (p.X - inter.X) > -1 && (p.Y - inter.Y) < 1 && (p.Y - inter.Y) > -1))
                        {

                            inters.Add(inter);
                        }
                    }
                }

                if (((seg1.X2 - seg1.X1) == 0) && inter.X <= Math.Max(seg2.X2, seg2.X1) && inter.X >= Math.Min(seg2.X2, seg2.X1) && inter.Y >= Math.Min(seg2.Y2, seg2.Y1) && inter.Y <= Math.Max(seg2.Y2, seg2.Y1))
                {

                    return true;
                }
                else if (((seg2.X2 - seg2.X1) == 0) && inter.Y >= Math.Min(seg2.Y2, seg2.Y1) && inter.Y <= Math.Max(seg2.Y2, seg2.Y1))
                {
                    return true;
                }
                else if (inter.X <= Math.Max(seg2.X2, seg2.X1) && inter.X >= Math.Min(seg2.X2, seg2.X1) && inter.Y >= Math.Min(seg2.Y2, seg2.Y1) && inter.Y <= Math.Max(seg2.Y2, seg2.Y1))
                {
                    return true;
                }

            }
            return false;

        } // Intersection entre segments

        static public bool Contains_point(Polyline p, Point point)
        {
            double a, b;
            Line l = new Line();

            for (int i = 0; i < p.Points.Count - 1; i++)
            {
                l.X1 = p.Points[i].X;
                l.Y1 = p.Points[i].Y;
                l.X2 = p.Points[i + 1].X;
                l.Y2 = p.Points[i + 1].Y;

                if ((l.X2 - l.X1) == 0)
                {
                    a = (l.Y2 - l.Y1) / 0.0001;
                }
                else
                {
                    a = (l.Y2 - l.Y1) / (l.X2 - l.X1);
                }
                b = l.Y1 - (a * l.X1);
                if ((point.X * a + b) == point.Y && ((l.X2 - l.X1) == 0) && point.Y <= Math.Max(l.Y2, l.Y1) && point.Y >= Math.Min(l.Y2, l.Y1))
                {
                    return true;
                }
                else if ((point.X * a + b) == point.Y && point.X <= Math.Max(l.X2, l.X1) && point.X >= Math.Min(l.X2, l.X1) && point.Y <= Math.Max(l.Y2, l.Y1) && point.Y >= Math.Min(l.Y2, l.Y1))
                {
                    return true;
                }

            }
            return false;

        } // retourne si une courbe contient un point
        static public void DecalerCourbe(ref Courbe courbe, int x, int y)
        {
            Polyline poly = courbe.p;
            if (poly != null)
            {
                Polyline interPoints = new Polyline();
                for (int j = 0; j < poly.Points.Count; j++)
                {
                    Point pt = new Point(poly.Points[j].X, poly.Points[j].Y);
                    pt.X += x;
                    pt.Y += y;
                    interPoints.Points.Add(pt);
                }
                courbe.p.Points = interPoints.Points;
            }
        } // decale une courbe de (x,y)
        #endregion


        class Cmp_distance : IComparer<Point>
        {
            Point m;
            public Cmp_distance(Point m)
            {
                this.m = m;
            }
            public static double Distance(double x1, double y1, double x2, double y2)
            {
                return Math.Sqrt(Math.Pow((y2 - y1), 2) + Math.Pow((x2 - x1), 2));
            }
            public int Compare(Point pa, Point pb)
            {
                return Distance(m.X, m.Y, pa.X, pa.Y).CompareTo(Distance(m.X, m.Y, pb.X, pb.Y));

            }
        }

    }


}
