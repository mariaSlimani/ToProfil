using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ToProfil
{
    public class Courbe
    {
        public Polyline p;
        public double alt;
        private Color color = Color.FromRgb(0, 0, 0);
        private bool initialised = false;

        #region Constructeurs + seters/getters
        public Courbe()
        {

        }
        public Courbe(Polyline p, double alt)
        {
            this.p = p;
            this.alt = alt;
        }
        public Courbe(Polyline p)
        {
            this.p = p;
        }

        public void SetAlt(double alt)
        {
            this.alt = alt;
        }
        public void SetInitialise(bool val)
        {
            initialised = val;
        }
        public void Initialise()
        {
            initialised = true;
        }
        public bool GetInitialise()
        { return initialised; } //retourne si l'altitude de la courbe a été initialisée
        #endregion

        #region Methodes de calcul des couleurs de courbe
        public Color Color
        {
            get { return color; }   // get method
            set { color = value; }  // set method
        } 
        public Color CurveColor(double min, double max)
        {
            int blue_hue = GetHue(0, 0, 255);
            int red_hue = GetHue(255, 0, 0);
            double hue = blue_hue + (red_hue - blue_hue) * (alt - min) / (max - min);
            double v1, v2;
            float huee = (float)hue / 360;
            v2 = (0.5 < 0.5) ? (0.5 * (1 + 1)) : ((0.5 + 1) - (0.5 * 1));
            v1 = 2 * 0.5 - v2;
            byte r = (byte)(255 * HueToRGB(v1, v2, huee + 1.0f / 3));
            byte g = (byte)(255 * HueToRGB(v1, v2, huee));
            byte b = (byte)(255 * HueToRGB(v1, v2, huee - 1.0f / 3));
            Color x = Color.FromRgb(r, g, b);
            return x;
        } // Calcul la couleur de la courbe 
        public static double HueToRGB(double v1, double v2, double vH)
        {
            if (vH < 0)
                vH += 1;
            if (vH > 1)
                vH -= 1;
            if ((6 * vH) < 1)
                return (v1 + (v2 - v1) * 6 * vH);
            if ((2 * vH) < 1)
                return v2;
            if ((3 * vH) < 2)
                return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);
            return v1;
        }
        public int GetHue(int red, int green, int blue)
        {
            float min = Math.Min(Math.Min(red, green), blue);
            float max = Math.Max(Math.Max(red, green), blue);
            if (min == max)
            {
                return 0;
            }
            float hue;
            if (max == red)
            {
                hue = (green - blue) / (max - min);
            }
            else if (max == green)
            {
                hue = 2f + (blue - red) / (max - min);
            }
            else
            {
                hue = 4f + (red - green) / (max - min);
            }
            hue *= 60;
            if (hue < 0) hue += 360;
            return Convert.ToInt32(Math.Round(hue));
        }
        #endregion

        #region Intersection
        public bool IntersecSegSeg(Line seg1, Line seg2, ref List<Point> inters)
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

        } // Intersection entre segments
        public bool IntersectionCourbeItself(ref List<Point> inters)
        {
            Line l = new Line();
            Line l2 = new Line();
            List<Point> pnt = new List<Point>();

            for (int i = 0; i < p.Points.Count - 1; i++)
            {
                l.X1 = p.Points[i].X;
                l.Y1 = p.Points[i].Y;
                l.X2 = p.Points[i + 1].X;
                l.Y2 = p.Points[i + 1].Y;

                for (int j = 0; j < p.Points.Count - 1; j++)
                {

                    if ((j != i + 1) && (j != i - 1) && (j != i))
                    {
                        pnt.Clear();
                        l2.X1 = p.Points[j].X;
                        l2.Y1 = p.Points[j].Y;
                        l2.X2 = p.Points[j + 1].X;
                        l2.Y2 = p.Points[j + 1].Y;

                        if (Fonctions.IntersecSegSeg(l, l2, ref pnt))
                        {
                            foreach (Point point in pnt)
                            {
                                if (!(p.Points[0].Equals(p.Points[p.Points.Count - 1]) && (point.X < p.Points[0].X + 0.3) && (point.Y < p.Points[0].Y + 0.3) && (point.X > p.Points[0].X - 0.3) && (point.Y > p.Points[0].Y - 0.3)))
                                {
                                    inters.Add(point);
                                }

                            }
                        }

                    }
                }
            }
            if (inters.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        } // Intersection d'une courbe avec elle meme
        public bool IntersectionCoubreCourbes(ref List<Point> inters, List<Courbe> courbes)
        {
            Line l = new Line();
            Line l2 = new Line();
            List<Point> pnt = new List<Point>();
            for (int i = 0; i < p.Points.Count - 1; i++)
            {
                l.X1 = p.Points[i].X;
                l.Y1 = p.Points[i].Y;
                l.X2 = p.Points[i + 1].X;
                l.Y2 = p.Points[i + 1].Y;
                for (int k = 0; k < courbes.Count; k++)
                {
                    if (!courbes[k].p.Equals(p))
                    {
                        for (int j = 0; j < courbes[k].p.Points.Count - 1; j++)
                        {
                            pnt.Clear();
                            l2.X1 = courbes[k].p.Points[j].X;
                            l2.Y1 = courbes[k].p.Points[j].Y;
                            l2.X2 = courbes[k].p.Points[j + 1].X;
                            l2.Y2 = courbes[k].p.Points[j + 1].Y;

                            if (Fonctions.IntersecSegSeg(l, l2, ref pnt))
                            {
                                foreach (Point point in pnt)
                                {
                                    inters.Add(point);
                                }
                            }
                        }
                    }
                }

            }

            if (inters.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        } // Intersections entre les courbes
        #endregion

        public bool Contains_point(Point point)
        {
            double a, b;
            Line l = new Line();
            for (int i = 0; i < p.Points.Count - 1; i++)
            {
                if (point.Equals(p.Points[i]) || point.Equals(p.Points[i + 1])) { return true; }
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
        public Courbe Duplicate()
        {
            Courbe courbe = new Courbe();
            Polyline poly = new Polyline();
            courbe.color = Color.FromRgb(this.color.R, this.color.G, this.color.B);
            poly.Stroke = this.p.Stroke;
            poly.StrokeThickness = this.p.StrokeThickness;
            foreach (Point pt in this.p.Points)
            {
                Point point = new Point(pt.X, pt.Y);
                poly.Points.Add(point);
            }
            courbe.p = poly;
            courbe.alt = this.alt;
            courbe.initialised = this.initialised;
            return courbe;
        } // duplique une courbe
    }

    public class Cmp_point_x : IComparer<Point> // ordonner les points selon leur position
    {
        public int Compare(Point pa, Point pb)
        {
            return pa.X.CompareTo(pb.X);
        }
    }
}
