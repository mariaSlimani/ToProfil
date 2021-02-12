using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace ToProfil
{
    public class PlanDeTravail
    {
        #region Data
        public List<Courbe> Courbes = new List<Courbe>();
        public List<Courbe> Courbes_prob = new List<Courbe>();
        public Line Segment = null;
        public List<Pointa> inters = new List<Pointa>();
        public List<Ellipse_point> Ellipse_Labels = new List<Ellipse_point>();
        public List<Point> probems = new List<Point>();
        public List<Ellipse> e_probems = new List<Ellipse>();
        #endregion
        #region Methodes
        public Carte Carte { get; set; } = new Carte(); //l'atribut carte
        public Courbe GetCourbe(Polyline poly)
        {
            foreach (Courbe courbe in Courbes)
            {
                if (courbe.p == poly) return courbe;
            }
            return null;
        }
        public void RemoveCourbe(Courbe courbe)
        {
            if (courbe != null)
            {
                if (Courbes.Contains(courbe)) Courbes.Remove(courbe);
            }
        }
        public void AddCourbe(Courbe courbe)
        {
            if (courbe != null)
            {
                Courbes.Add(courbe);
            }
        }
        public int GetI(Courbe courbe)
        {
            int i = -1;
            int j = 0;
            foreach (Courbe c in Courbes)
            {
                if (c == courbe)
                {
                    i = j;
                }
                j++;
            }
            return i;
        }

        public void Interpolation()
        {
            inters.Clear();
            List<Point_courbe> points_courbes = new List<Point_courbe>();
            List<Point> pointsAB = new List<Point>
            {
                new Point(Segment.X1, Segment.Y1),
                new Point(Segment.X2, Segment.Y2)
            };
            List<Pointa> initial = new List<Pointa>();
            Line l = new Line();
            List<Point> points_inter = new List<Point>();
            foreach (Courbe c in Courbes)
            {
                for (int i = 0; i < c.p.Points.Count - 1; i++)
                {
                    points_inter.Clear();
                    l.X1 = c.p.Points[i].X;
                    l.Y1 = c.p.Points[i].Y;
                    l.X2 = c.p.Points[i + 1].X;
                    l.Y2 = c.p.Points[i + 1].Y;

                    if (Fonctions.IntersecSegSeg(Segment, l, ref points_inter))
                    {
                        foreach (Point p in points_inter)
                        {
                            initial.Add(new Pointa(p, c.alt));
                        }
                    }
                }

            }

            inters.AddRange(initial);

            foreach (Point pointAB in pointsAB)
            {
                bool boole = false;
                foreach (Pointa p in initial)
                {
                    if ((p.p.X - pointAB.X) < 1 && (p.p.X - pointAB.X) > -1 && (p.p.Y - pointAB.Y) < 1 && (p.p.Y - pointAB.Y) > -1)
                    {
                        boole = true;

                    }
                }

                if (!boole)
                {
                    //list2 : intersection line_seg
                    points_courbes.Clear();
                    foreach (Courbe c in Courbes)
                    {
                        for (int i = 0; i < c.p.Points.Count - 1; i++)
                        {
                            points_inter.Clear();
                            l.X1 = c.p.Points[i].X;
                            l.Y1 = c.p.Points[i].Y;
                            l.X2 = c.p.Points[i + 1].X;
                            l.Y2 = c.p.Points[i + 1].Y;

                            if (Fonctions.IntersecLineSeg(Segment, l, ref points_inter))
                            {
                                foreach (Point p in points_inter)
                                {
                                    points_courbes.Add(new Point_courbe(p, c));
                                }
                            }
                        }

                    }


                    List<Point> pnt = new List<Point>();
                    pnt.Clear();
                    pnt.Add(pointAB);
                    foreach (Point_courbe p in points_courbes)
                    {
                        pnt.Add(p.p);
                    }

                    Cmp_point_x cmp = new Cmp_point_x();
                    pnt.Sort(cmp);
                    double alt1 = 0, alt2 = 0, alt4 = 0, alt5 = 0, alt9 = 0;
                    if (pnt.Count >= 3)
                    {
                        List<Point_courbe> closetpoints = new List<Point_courbe>();
                        Point closestpoint = new Point();

                        foreach (Courbe cc in Courbes)
                        {
                            Fonctions.ClosestPointPolyline(ref closestpoint, pointAB, cc.p);
                            closetpoints.Add(new Point_courbe(closestpoint, cc));
                        }
                        //sort
                        Cmp_distance_point_courbe cdpc = new Cmp_distance_point_courbe(pointAB);
                        closetpoints.Sort(cdpc);
                        if (pnt[0].X == pointAB.X && pnt[0].Y == pointAB.Y)
                        {

                            Boolean booleen = false;
                            foreach (Point_courbe pc in points_courbes)
                            {
                                if (pc.c == closetpoints[0].c)
                                {
                                    booleen = true;
                                }
                            }

                            double alt3;
                            if (booleen)
                            {
                                for (int jk = 0; jk < points_courbes.Count; jk++)
                                {
                                    if (points_courbes[jk].p.X == pnt[1].X && points_courbes[jk].p.Y == pnt[1].Y)
                                    {
                                        alt1 = points_courbes[jk].c.alt;
                                    }
                                    else if (points_courbes[jk].p.X == pnt[2].X && points_courbes[jk].p.Y == pnt[2].Y)
                                    {

                                        alt2 = points_courbes[jk].c.alt;

                                    }

                                }
                                if (alt1 > alt2)
                                {
                                    alt3 = alt1 + 0.5 * Carte.equidistance;
                                    if (alt3 > Carte.altMax)
                                        alt3 = Carte.altMax;
                                }
                                else if (alt1 < alt2)
                                {
                                    alt3 = alt1 - 0.5 * Carte.equidistance;
                                    if (alt3 < Carte.altMin)
                                        alt3 = Carte.altMin;
                                }
                                else
                                {
                                    alt3 = alt1;
                                }
                                inters.Add(new Pointa(pointAB, Math.Round(alt3, 2)));

                            }
                            else
                            {
                                alt3 = (closetpoints[1].c.alt - closetpoints[0].c.alt) / (Fonctions.Distance(pointAB.X, pointAB.Y, closetpoints[0].p.X, closetpoints[0].p.Y) + Fonctions.Distance(pointAB.X, pointAB.Y, closetpoints[1].p.X, closetpoints[1].p.Y));

                                if (closetpoints[0].c.alt < closetpoints[1].c.alt)
                                {
                                    alt3 = Math.Abs((alt3 * (Fonctions.Distance(pointAB.X, pointAB.Y, closetpoints[0].p.X, closetpoints[0].p.Y))));
                                    alt3 += closetpoints[0].c.alt;
                                }
                                else
                                {
                                    alt3 = Math.Abs((alt3 * (Fonctions.Distance(pointAB.X, pointAB.Y, closetpoints[1].p.X, closetpoints[1].p.Y))));
                                    alt3 += closetpoints[1].c.alt;

                                }
                                inters.Add(new Pointa(pointAB, Math.Round(alt3, 2)));
                            }

                        }


                        else if (pnt.Last().X == pointAB.X && pnt.Last().Y == pointAB.Y)
                        {


                            Boolean booleen = false;
                            foreach (Point_courbe pc in points_courbes)
                            {
                                if (pc.c == closetpoints[0].c)
                                {
                                    booleen = true;
                                }
                            }

                            double alt6;
                            if (booleen)
                            {
                                for (int mm = 0; mm < points_courbes.Count; mm++)
                                {
                                    if (points_courbes[mm].p.X == pnt[pnt.Count - 2].X && points_courbes[mm].p.Y == pnt[pnt.Count - 2].Y)
                                    {
                                        alt5 = points_courbes[mm].c.alt;
                                    }
                                    else if (points_courbes[mm].p.X == pnt[pnt.Count - 3].X && points_courbes[mm].p.Y == pnt[pnt.Count - 3].Y)

                                    {

                                        alt4 = points_courbes[mm].c.alt;

                                    }

                                }
                                if (alt4 > alt5)
                                {
                                    alt6 = alt5 - 0.5 * Carte.equidistance;
                                    if (alt6 < Carte.altMin)
                                        alt6 = Carte.altMin;

                                }
                                else if (alt4 < alt5)
                                {
                                    alt6 = alt5 + 0.5 * Carte.equidistance;
                                    if (alt6 > Carte.altMax)
                                        alt6 = Carte.altMax;
                                }
                                else { alt6 = alt5; }
                                inters.Add(new Pointa(pointAB, Math.Round(alt6, 2)));
                            }
                            else
                            {
                                alt6 = (closetpoints[1].c.alt - closetpoints[0].c.alt) / (Fonctions.Distance(pointAB.X, pointAB.Y, closetpoints[0].p.X, closetpoints[0].p.Y) + Fonctions.Distance(pointAB.X, pointAB.Y, closetpoints[1].p.X, closetpoints[1].p.Y));

                                if (closetpoints[0].c.alt < closetpoints[1].c.alt)
                                {
                                    alt6 = Math.Abs((alt6 * (Fonctions.Distance(pointAB.X, pointAB.Y, closetpoints[0].p.X, closetpoints[0].p.Y))));
                                    alt6 += closetpoints[0].c.alt;
                                }
                                else
                                {
                                    alt6 = Math.Abs((alt6 * (Fonctions.Distance(pointAB.X, pointAB.Y, closetpoints[1].p.X, closetpoints[1].p.Y))));
                                    alt6 += closetpoints[1].c.alt;

                                }
                                inters.Add(new Pointa(pointAB, Math.Round(alt6, 2)));

                            }
                        }

                        else
                        {
                            Courbe courbe1 = new Courbe();
                            Courbe courbe2 = new Courbe();

                            for (int hh = 0; hh < points_courbes.Count; hh++)
                            {
                                if (points_courbes[hh].p.X == pnt[pnt.IndexOf(pointAB) - 1].X && points_courbes[hh].p.Y == pnt[pnt.IndexOf(pointAB) - 1].Y)
                                {
                                    courbe1 = points_courbes[hh].c;
                                    Point p1 = points_courbes[hh].p;

                                }
                                else if (points_courbes[hh].p.X == pnt[pnt.IndexOf(pointAB) + 1].X && points_courbes[hh].p.Y == pnt[pnt.IndexOf(pointAB) + 1].Y)

                                {

                                    courbe2 = points_courbes[hh].c;
                                    Point p2 = points_courbes[hh].p;

                                }

                            }
                            if (courbe2.alt == courbe1.alt && courbe1.p.Equals(courbe2.p))
                            {
                                if (closetpoints[0].c == courbe2)
                                {
                                    alt9 = courbe2.alt;
                                    inters.Add(new Pointa(pointAB, Math.Round(alt9, 2)));
                                }
                                else
                                {
                                    alt9 = (closetpoints[1].c.alt - closetpoints[0].c.alt) / (Fonctions.Distance(pointAB.X, pointAB.Y, closetpoints[0].p.X, closetpoints[0].p.Y) + Fonctions.Distance(pointAB.X, pointAB.Y, closetpoints[1].p.X, closetpoints[1].p.Y));
                                    if (closetpoints[0].c.alt < closetpoints[1].c.alt)
                                    {
                                        alt9 = Math.Abs((alt9 * (Fonctions.Distance(pointAB.X, pointAB.Y, closetpoints[0].p.X, closetpoints[0].p.Y))));
                                        alt9 += closetpoints[0].c.alt;
                                    }
                                    else
                                    {
                                        alt9 = Math.Abs((alt9 * (Fonctions.Distance(pointAB.X, pointAB.Y, closetpoints[1].p.X, closetpoints[1].p.Y))));
                                        alt9 += closetpoints[1].c.alt;

                                    }
                                    inters.Add(new Pointa(pointAB, Math.Round(alt9, 2)));
                                }
                            }
                            else
                            {

                                Courbe courbe11 = new Courbe();
                                Courbe courbe22 = new Courbe();
                                Point closestpoint11 = new Point();
                                Point closestpoint22 = new Point();


                                if (closetpoints.Count == 1)
                                {
                                    alt9 = closetpoints[0].c.alt;
                                    inters.Add(new Pointa(pointAB, Math.Round(alt9, 2)));
                                }
                                else
                                {
                                    courbe11 = closetpoints[0].c;
                                    courbe22 = closetpoints[1].c;
                                    closestpoint11 = closetpoints[0].p;
                                    closestpoint22 = closetpoints[1].p;

                                    alt9 = (courbe22.alt - courbe11.alt) / (Fonctions.Distance(pointAB.X, pointAB.Y, closestpoint11.X, closestpoint11.Y) + Fonctions.Distance(pointAB.X, pointAB.Y, closestpoint22.X, closestpoint22.Y));

                                    if (courbe11.alt < courbe22.alt)
                                    {
                                        alt9 = Math.Abs((alt9 * (Fonctions.Distance(pointAB.X, pointAB.Y, closestpoint11.X, closestpoint11.Y))));
                                        alt9 += courbe11.alt;
                                    }
                                    else
                                    {
                                        alt9 = Math.Abs((alt9 * (Fonctions.Distance(pointAB.X, pointAB.Y, closestpoint22.X, closestpoint22.Y))));
                                        alt9 += courbe22.alt;

                                    }

                                    inters.Add(new Pointa(pointAB, Math.Round(alt9, 2)));
                                }
                            }

                        }
                    }
                    else if (pnt.Count == 2)
                    {
                        for (int hh = 0; hh < points_courbes.Count; hh++)
                        {
                            if (points_courbes[hh].p.X == pnt[0].X && points_courbes[hh].p.Y == pnt[0].Y)
                            {
                                inters.Add(new Pointa(pointAB, Math.Round((points_courbes[hh].c.alt), 2)));
                            }
                            else
                            {
                                inters.Add(new Pointa(pointAB, Math.Round((points_courbes[hh].c.alt), 2)));
                            }

                        }
                    }
                    else
                    {
                        //pas d'intersection
                    }

                }

            }
            if (Segment.X1 > Segment.X2)
            {
                Cmp_pointa_x_inverse cmp_inverse = new Cmp_pointa_x_inverse();
                inters.Sort(cmp_inverse);

            }
            else
            {
                Cmp_pointa_x cmp = new Cmp_pointa_x();
                inters.Sort(cmp);
            }

        } 
        public List<Pointa> Getinters()
        {
            return inters;
        }
        public List<Courbe> Getcourbes()
        {
            return Courbes;
        }
        public void ClearInters()
        {
            inters.Clear();
        }
        public PlanDeTravail Duplicate()
        {
            PlanDeTravail pdt = new PlanDeTravail
            {
                Courbes = new List<Courbe>()
            };
            foreach (Courbe courbe in this.Courbes)
            {
                pdt.Courbes.Add(courbe.Duplicate());
            }
            if (this.Segment != null)
            {
                pdt.Segment = new Line();
                pdt.Segment.Stroke = this.Segment.Stroke;
                pdt.Segment.StrokeThickness = this.Segment.StrokeThickness;
                pdt.Segment.X1 = this.Segment.X1;
                pdt.Segment.Y1 = this.Segment.Y1;
                pdt.Segment.X2 = this.Segment.X2;
                pdt.Segment.Y2 = this.Segment.Y2;
            }
            else pdt.Segment = null;

            pdt.Carte = this.Carte.Duplicate();

            return pdt;
        }

        #endregion
    }
}
