using System;
using System.Collections.Generic;
using System.Windows;

namespace ToProfil
{
    public class Point_courbe
    {
        public Point p;
        public Courbe c;
        public Point_courbe(Point p, Courbe c)
        {
            this.p = p;
            this.c = c;
        }
    }
    class Cmp_distance_point_courbe : IComparer<Point_courbe>
    {
        Point m;
        public Cmp_distance_point_courbe(Point m)
        {
            this.m = m;
        }
        static public double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((y2 - y1), 2) + Math.Pow((x2 - x1), 2));
        }
        public int Compare(Point_courbe pa, Point_courbe pb)
        {
            return Distance(m.X, m.Y, pa.p.X, pa.p.Y).CompareTo(Distance(m.X, m.Y, pb.p.X, pb.p.Y));

        }
    }
}
