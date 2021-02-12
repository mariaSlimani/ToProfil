using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ToProfil
{

    public partial class Profile_generation : Page
    {
        #region Data
        private string mode;
        private readonly Image imag;
        private readonly Canvas canva;
        public string act;
        private readonly PlanDeTravail planDeTravail;
        private readonly List<Pointa> p = new List<Pointa>();
        public List<double> other = new List<double>();//liste des valeurs des pentes
        public List<int> indices = new List<int>();
        private readonly double echelle;
        private Color c, cMin, cMax, cx, bc;
        private LinearGradientBrush m;
        private double leZoom = 0;
        private readonly double zoomMax = 10;
        private readonly double zoomMin = 0;
        private readonly double zoomSpeed = 0.001;
        double stp;
        #endregion

        public Profile_generation(string mode, Image imag, Canvas canva, PlanDeTravail planDeTravail, Pointa[] inters)
        {
            this.mode = mode;
            this.imag = imag;
            this.canva = canva;
            foreach (Pointa pp in inters)
            {
                this.p.Add(pp);
            }
            this.planDeTravail = planDeTravail;
            this.echelle = planDeTravail.Carte.echelle;
            InitializeComponent();
            SetMode();
            AfficherGraph();
            AffichPentes();
        }

        public void Profile_Click(object sender, RoutedEventArgs e)
        {
            Plan plan = new Plan(this.mode, this.imag, this.canva, this.planDeTravail);
            plan.echel.Text = planDeTravail.Carte.echelle.ToString();
            plan.equi.Text = planDeTravail.Carte.equidistance.ToString();
            plan.altMax.Text = planDeTravail.Carte.altMax.ToString();
            plan.altMin.Text = planDeTravail.Carte.altMin.ToString();
            if (planDeTravail.Carte.Km)
            {
                plan.ToggleButton_Checked(new object(), new RoutedEventArgs());
                plan.km_m.IsChecked = true;
            }
            plan.AssignColors();
            this.NavigationService.Navigate(plan);
        }

        public void Home_Click(object sender, RoutedEventArgs e)
        {
            //retour vers le dashboard
            conf_canvas.Visibility = Visibility.Visible;
            act = "home";
        }


        #region Pentes
        public void AjouterPente()          //methode pour ajouter pente a la liste other
        {
            Pointa a, b;
            double pente;
            a = new Pointa();
            b = new Pointa();
            int i = 0;
            bool continu = true, stop = false;
            while ((i < p.Count - 1) && (!stop))    // parcourir la liste des points 
            {
                a = p[i];
                int j = i;
                int c = Compare(p[i], p[i + 1]);
                switch (c)          // manipuler les differents cas 
                {
                    case -1:
                        {
                            if (c == -1)
                            {
                                continu = true;
                            }
                            while ((continu))
                            {
                                i++;
                                b = p[i];

                                if (i < p.Count - 1)
                                {
                                    c = Compare(p[i], p[i + 1]);
                                }
                                else
                                {
                                    break;
                                }
                                if ((c == 1) || (c == 0))
                                {
                                    continu = false;
                                }
                            }
                            indices.Add(j);
                            indices.Add(i);
                            pente = CalculPente(p[i].p.X, a.p.X, p[i].alt, a.alt);
                            other.Add(Math.Round(pente, 2));
                            break;
                        }
                    case 1:
                        {
                            if (c == 1)
                            {
                                continu = true;
                            }
                            while ((continu))
                            {
                                i++;
                                b = p[i];
                                if (i < p.Count - 1)
                                {
                                    c = Compare(p[i], p[i + 1]);
                                }
                                else
                                {
                                    break;
                                }
                                if ((c == -1) || (c == 0))
                                {

                                    continu = false;
                                }
                            }
                            indices.Add(j);
                            indices.Add(i);
                            pente = CalculPente(b.p.X, a.p.X, b.alt, a.alt);
                            other.Add(Math.Round(pente, 2));
                            break;
                        }
                    case 0:
                        i++;
                        break;
                }
            }
            if (p.Count != 0)
            {
                if (p[0].p.X > p[p.Count - 1].p.X)
                {
                    for (int n = 0; n < other.Count; n++)
                    {
                        other[n] = other[n] * (-1);
                    }
                }
            }
        }

        public double CalculPente(double x1, double x2, double y1, double y2)       //fct pour calculer la pente 
        {
            double pente;
            if (planDeTravail.Carte.Km)
            {
                pente = ((y1 - y2) / ((x1 - x2) * echelle * 1000)) * 100;
            }
            else
            {
                pente = ((y1 - y2) / ((x1 - x2) * echelle)) * 100;
            }
            return pente;
        }
        public int Compare(Pointa a, Pointa b)      //methode de comparaison des points 
        {
            if (a.alt < b.alt)
            {
                return (-1);
            }
            else if (a.alt > b.alt)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public void AffichPentes()
        {
            AjouterPente();
            int height = 19, width = 75, x = 5, y = 2, i = 0;
            if (((indices.Count == 0) || (other.Count == 0)) && (p.Count != 0))
            {
                TextBlock block = new TextBlock
                {
                    Padding = new Thickness(4),
                    Height = height,
                    Width = width,
                    Foreground = cote.Fill,
                    Background = null,
                    Text = "P0-P" + (p.Count - 1) + " : 0 %"
                };
                Canvas.SetLeft(block, x);
                Canvas.SetTop(block, y);
                pentes_canvas.Children.Add(block);
            }
            else
            {
                foreach (double pente in other)
                {
                    width = 75;
                    if (pente.ToString().Length > 3)
                    {
                        width = 110;
                    }
                    TextBlock block = new TextBlock
                    {
                        Padding = new Thickness(4),
                        Foreground = cote.Fill,
                        Background = null,
                        Text = "P" + indices[i] + "-P" + indices[i + 1] + " : " + pente.ToString() + " %"
                    };
                    if (block.Text.Length > 10)
                    {
                        width = 110;
                    }
                    i += 2;
                    block.Height = height;
                    block.Width = width;
                    Canvas.SetLeft(block, x);
                    Canvas.SetTop(block, y);
                    pentes_canvas.Children.Add(block);
                    x += (width + 5);
                    if (x + width >= pentes_canvas.Width)
                    {
                        y += (height + 8);
                        x = 5;
                    }
                    if (y >= pentes_canvas.Height)
                    {
                        pentes_canvas.Height += height + 5;
                    }
                }
            }
        }
        #endregion

        #region Graph
        public void AfficherGraph()
        {
            //sort all   :::::::::::::::::::::::::::::::::::::::::::::::::
            Carte ech = planDeTravail.Carte;
            double min = planDeTravail.Carte.altMin, max = planDeTravail.Carte.altMax, equidistance = planDeTravail.Carte.equidistance;

            //graph      :::::::::::::::::::::::::::::::::::::::::::::::::
            ChartValues<ObservablePoint> v = new ChartValues<ObservablePoint>();

            foreach (Pointa pp in p)
            {
                v.Add(new ObservablePoint(Math.Abs(Math.Round(ech.ToKm_m(PxToCm(pp.p.X - planDeTravail.Segment.X1)), 2)), pp.alt));
            }

            g.Series = new SeriesCollection
            {
                new LineSeries
            {
                  Title="Point",
                  Values=v,
                  DataLabels=true,
                  LabelPoint = point => "P"+ point.Key.ToString(),
                  LineSmoothness=0,
                  Stroke= new SolidColorBrush(c),
                  Foreground = new SolidColorBrush(c),
                  Fill = m,
        }

            };

            //axis:::::::::::::::::::::::::::::
            Color k = Color.FromRgb(0, 128, 128);
            Axis x = new Axis();
            if (ech.Km)
            {
                x.LabelFormatter = value => value.ToString() + "Km";

            }
            else
            {
                x.LabelFormatter = value => value.ToString() + "m";
            }
            x.Foreground = new SolidColorBrush(cx);

            x.MaxValue = planDeTravail.Carte.echelle * 29;
            LiveCharts.Wpf.Separator spr = new LiveCharts.Wpf.Separator
            {
                Step = planDeTravail.Carte.echelle
            };
            x.Title = "\nDistance";
            x.Separator = spr;
            x.MaxRange = 1000;
            x.MinRange = -1000;
            g.AxisX.Add(x);

            g.AxisY.Add(new Axis
            {
                Title = "Altitude\n",
                MaxValue = max + equidistance,
                MinValue = min - equidistance,
                ShowLabels = true,
                LabelFormatter = value => value.ToString() + "m",
                Foreground = new SolidColorBrush(cx),
                Separator = new LiveCharts.Wpf.Separator
                {
                    Stroke = Brushes.Silver,

                },
                Sections = new SectionsCollection {
                    new AxisSection
                    {
                        SectionWidth = 2,
                        Value=max,
                        Label="max",
                        DataLabel=true,
                        Stroke=new SolidColorBrush(bc),
                        DataLabelForeground =new SolidColorBrush(cMax),

                        Fill = new SolidColorBrush
                        {
                            Color = cMax,
                            Opacity=4
                        }
                    },

                    new AxisSection
                    {
                        SectionWidth = 2,
                        Value=min,
                        Label="min",
                        DataLabel=true,
                        Stroke=new SolidColorBrush(bc),
                        DataLabelForeground =new SolidColorBrush(cMin),

                        Fill = new SolidColorBrush
                        {
                            Color = cMin,
                            Opacity=4
                        }
                    }
                }
            });
            foreach (Axis xis in g.AxisY)
            {
                if ((max - min) / equidistance <= 20)
                {
                    xis.Separator.Step = equidistance;
                }
                stp = xis.Separator.Step;
                xis.MaxRange = 1000;
                xis.MinRange = -1000;
            }
            g.DataTooltip = new ChartTooltip(ech, mode);
            g.TooltipTimeout = TimeSpan.FromMilliseconds(90);
        }

        //cmtopx  pxtocm***************************************************************************

        public static double CmToPx(double cm) { return cm / 0.0264583333333333; }
        public static double PxToCm(double px) { return px / 37.795275590551178; }
        //*****************************************************************************************
        #endregion


        #region Menu
        public void Menu_Click(object sender, RoutedEventArgs e)
        {
            //ouvrir le menu
            menu_rec.Visibility = Visibility.Visible;

        }

        public void Retour_Click(object sender, RoutedEventArgs e)
        {
            //cacher le menu
            menu_rec.Visibility = Visibility.Hidden;
        }

        public void Nouv_Click(object sender, RoutedEventArgs e)
        {
            //creer un nouveau projet
            conf_canvas.Visibility = Visibility.Visible;
            act = "nouv";
        }

        public void Aide_Click(object sender, RoutedEventArgs e)
        {
            //ouvrir l'aide en ligne
            System.Diagnostics.Process.Start("AideOnline.html");
        }

        public void Sauvegarde_Click(object sender, RoutedEventArgs e)
        {
            //sauvegarder le projet courant
            Plan plan = new Plan(this.mode, this.imag, this.canva, this.planDeTravail);
            plan.echel.Text = planDeTravail.Carte.echelle.ToString();
            plan.equi.Text = planDeTravail.Carte.equidistance.ToString();
            plan.altMax.Text = planDeTravail.Carte.altMax.ToString();
            plan.altMin.Text = planDeTravail.Carte.altMin.ToString();
            if (planDeTravail.Carte.Km)
            {
                plan.ToggleButton_Checked(new object(), new RoutedEventArgs());
                plan.km_m.IsChecked = true;
            }
            plan.AssignColors();
            plan.SaveThings();
        }

        public void Ouvrir_Click(object sender, RoutedEventArgs e)
        {
            //ouvrir un projet existant
            act = "ouvr";
            conf_canvas.Visibility = Visibility.Visible;
        }
        #endregion

        #region zoom
        public void Zoom_Click(object sender, RoutedEventArgs e)
        {
            //zoom dans le graph
            if (zoom_slider.Visibility == Visibility.Visible)
            {
                zoom_slider.Visibility = Visibility.Hidden;
            }
            else
            {
                zoom_slider.Visibility = Visibility.Visible;
            }
        }
        public void ZoomIn()
        {
            // ctrl + 
            leZoom += 0.1;
            if (leZoom > zoomMax) { leZoom = zoomMax; }
            zoom_slider.Value = Math.Round(leZoom, 1);
            g.AxisY[0].MaxValue = Math.Round(planDeTravail.Carte.altMax / (leZoom + 1), 2);
            g.AxisY[0].Separator.Step = Math.Round(stp / (leZoom + 1), 0);
            g.AxisX[0].MaxValue = Math.Round(planDeTravail.Carte.echelle * 29 / (leZoom + 1), 2);
            g.AxisX[0].Separator.Step = Math.Round(planDeTravail.Carte.echelle / (leZoom + 1), 0);
            if (g.AxisY[0].MaxValue <= g.AxisY[0].MinValue)
            {
                g.AxisY[0].MaxValue = Math.Round(g.AxisY[0].MinValue, 0) + planDeTravail.Carte.equidistance;
            }
        }
        public void ZoomOut()
        {
            // ctrl -
            leZoom -= 0.1;
            if (leZoom < zoomMin) { leZoom = zoomMin; }
            zoom_slider.Value = Math.Round(leZoom, 1);
            g.AxisY[0].MaxValue = Math.Round(planDeTravail.Carte.altMax / (leZoom + 1), 2);
            g.AxisY[0].Separator.Step = Math.Round(stp / (leZoom + 1), 0);
            g.AxisX[0].MaxValue = Math.Round(planDeTravail.Carte.echelle * 29 / (leZoom + 1), 2);
            g.AxisX[0].Separator.Step = Math.Round(planDeTravail.Carte.echelle / (leZoom + 1), 0);
            if (g.AxisY[0].MaxValue <= g.AxisY[0].MinValue)
            {
                g.AxisY[0].MaxValue = Math.Round(g.AxisY[0].MinValue, 0) + planDeTravail.Carte.equidistance;
            }
        }

        private void G_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control) //Pour le Ctrl + scroll
            {
                leZoom += Math.Round(zoomSpeed * e.Delta, 1); // Ajust zooming speed 
                if (leZoom < zoomMin) { leZoom = zoomMin; } // Limit Min Scale
                if (leZoom > zoomMax) { leZoom = zoomMax; } // Limit Max Scale
                zoom_slider.Value = Math.Round(leZoom, 1);
                g.AxisY[0].MaxValue = Math.Round(planDeTravail.Carte.altMax / (leZoom + 1), 2);
                g.AxisY[0].Separator.Step = Math.Round(stp / (leZoom + 1), 0);
                g.AxisX[0].MaxValue = Math.Round(planDeTravail.Carte.echelle * 29 / (leZoom + 1), 2);
                g.AxisX[0].Separator.Step = Math.Round(planDeTravail.Carte.echelle / (leZoom + 1), 0);
                if (g.AxisY[0].MaxValue <= g.AxisY[0].MinValue)
                {
                    g.AxisY[0].MaxValue = Math.Round(g.AxisY[0].MinValue, 0) + planDeTravail.Carte.equidistance;
                }
            }
        }

        public void Zoom_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //zoom slider
            if (sender is Slider)
            {
                leZoom = Math.Round(e.NewValue, 1);
                g.AxisY[0].MaxValue = Math.Round(planDeTravail.Carte.altMax / (leZoom + 1), 2);
                g.AxisY[0].Separator.Step = Math.Round(stp / (leZoom + 1), 0);
                g.AxisX[0].MaxValue = Math.Round(planDeTravail.Carte.echelle * 29 / (leZoom + 1), 2);
                g.AxisX[0].Separator.Step = Math.Round(planDeTravail.Carte.echelle / (leZoom + 1), 0);
                if (g.AxisY[0].MaxValue <= g.AxisY[0].MinValue)
                {
                    g.AxisY[0].MaxValue = Math.Round(g.AxisY[0].MinValue, 0) + planDeTravail.Carte.equidistance;
                }
            }
            if (leZoom == zoomMin)
            {
                g.AxisY[0].MinValue = Math.Round(planDeTravail.Carte.altMin, 2);
                g.AxisX[0].MinValue = 0;
            }

        }
        #endregion

        #region set colors
        public void Mode_Switch_Click(object sender, RoutedEventArgs e)
        {
            //changer de mode 
            if (cote.Fill.ToString() == "#FF333366")
            {
                mode = "sombre";

            }
            else
            {
                mode = "clair";

            }
            SetMode();
        }
        public void SetMode()
        {
            if (this.mode == "sombre")
            {
                rect_bord.Fill = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                rect_fond.Fill = new SolidColorBrush(Color.FromRgb(63, 69, 70));
                conf_oui.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                conf_non.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                txt_conf.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                bc = Color.FromRgb(135, 135, 135);
                c = Color.FromRgb(14, 230, 229);
                cx = Color.FromRgb(14, 230, 229);
                cMin = Color.FromRgb(53, 100, 120);
                cMax = Color.FromRgb(53, 100, 120);
                pentes_canvas.Background = new SolidColorBrush(Color.FromRgb(175, 174, 172));
                backg.Background = new LinearGradientBrush(Color.FromRgb(102, 100, 101), Color.FromRgb(25, 25, 25), new Point(0, 1), new Point(1, 0));
                rect.Fill = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                cote.Fill = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                o_p.Source = new BitmapImage(new Uri("ret2d.png", UriKind.Relative));
                Home.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                Menu.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                o_m.Source = new BitmapImage(new Uri("menu2.png", UriKind.Relative));
                zoom.Background = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                pt.Foreground = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                dp.Foreground = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                est_ouest.Fill = new RadialGradientBrush(Color.FromRgb(31, 71, 88), Color.FromRgb(39, 60, 69));
                zoom_slider.Foreground = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                Mode_Switch.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                RotateTransform rotateTransform1 = new RotateTransform(360, 0, 0);
                Mode_Switch.RenderTransform = rotateTransform1;
                Mode_Switch.IsChecked = true;
                Mode_Switch.ToolTip = "Mode clair";
            }
            else
            {
                bc = Color.FromRgb(255, 255, 255);
                c = Color.FromRgb(51, 51, 102);
                cx = Color.FromRgb(51, 51, 102);
                cMin = Color.FromRgb(255, 192, 203);
                cMax = Color.FromRgb(255, 192, 203);
                rect_bord.Fill = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                rect_fond.Fill = new SolidColorBrush(Color.FromRgb(246, 248, 251));
                conf_oui.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                conf_non.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                txt_conf.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                pentes_canvas.Background = new SolidColorBrush(Color.FromArgb(130, 255, 192, 203));
                backg.Background = new LinearGradientBrush(Color.FromRgb(255, 255, 255), Color.FromRgb(245, 247, 251), new Point(1, 0), new Point(0, 1));
                cote.Fill = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                rect.Fill = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                o_p.Source = new BitmapImage(new Uri("ret2.png", UriKind.Relative));
                o_m.Source = new BitmapImage(new Uri("menu2.png", UriKind.Relative));
                Home.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                Menu.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                zoom.Background = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                pt.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                dp.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 10));
                est_ouest.Fill = new RadialGradientBrush(Color.FromRgb(192, 39, 57), Color.FromRgb(164, 17, 37));
                zoom_slider.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                Mode_Switch.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                RotateTransform rotateTransform1 = new RotateTransform(180, 0, 0);
                Mode_Switch.RenderTransform = rotateTransform1;
                Mode_Switch.IsChecked = true;
                Mode_Switch.ToolTip = "Mode Sombre";
            }
            g.DataTooltip = new ChartTooltip(planDeTravail.Carte, mode);
            m = new LinearGradientBrush(c, bc, 90)
            {
                Opacity = 0.3
            };
            foreach (Series s in g.Series)
            {
                s.Stroke = new SolidColorBrush(c);
                s.Foreground = new SolidColorBrush(c);
                s.Fill = m;
            }
            foreach (Axis b in g.AxisY)
            {
                foreach (AxisSection a in b.Sections)
                {
                    if (a.Label == "min")
                    {
                        a.Fill = new SolidColorBrush(cMin)
                        {
                            Opacity = 4
                        };
                        a.DataLabelForeground = new SolidColorBrush(cMin);
                        a.Stroke = new SolidColorBrush(bc);
                    }
                    else if (a.Label == "max")
                    {
                        a.Fill = new SolidColorBrush(cMax)
                        {
                            Opacity = 4
                        };
                        a.DataLabelForeground = new SolidColorBrush(cMax);
                        a.Stroke = new SolidColorBrush(bc);
                    }
                }
                b.Foreground = new SolidColorBrush(cx);
                b.Separator.Stroke = Brushes.Silver;
            }
            foreach (Axis b in g.AxisX)
            {
                b.Separator.Stroke = Brushes.Silver;
                b.Foreground = new SolidColorBrush(cx);
            }
            foreach (TextBlock block in pentes_canvas.Children)
            {
                block.Foreground = cote.Fill;
            }
        }
        #endregion

        #region confirmation de la sauvegarde 

        private void Conf_close_Click(object sender, RoutedEventArgs e)
        {
            conf_canvas.Visibility = Visibility.Hidden;
        }

        private void Conf_oui_Click(object sender, RoutedEventArgs e)
        {
            conf_canvas.Visibility = Visibility.Hidden;
            Sauvegarde_Click(new object(), new RoutedEventArgs());
            if (act == "home")
            {
                Dashboard dash = new Dashboard(this.mode);
                this.NavigationService.Navigate(dash);
            }
            else if (act == "nouv")
            {
                Plan plan = new Plan(this.mode, new PlanDeTravail());
                this.NavigationService.Navigate(plan);
            }
            else if (act == "ouvr")
            {
                //ouvrir un projet existant
                Plan plan = new Plan(this.mode, new PlanDeTravail());
                plan.LoadThings();
                this.NavigationService.Navigate(plan);
            }
            else
            {
                //close

                MainWindow win = (ToProfil.MainWindow)Window.GetWindow(this);
                win.redButton = false;
                win.Close();
            }
        }

        private void Conf_non_Click(object sender, RoutedEventArgs e)
        {
            conf_canvas.Visibility = Visibility.Hidden;
            if (act == "home")
            {
                Dashboard dash = new Dashboard(this.mode);
                this.NavigationService.Navigate(dash);
            }
            else if (act == "nouv")
            {
                Plan plan = new Plan(this.mode, new PlanDeTravail());
                this.NavigationService.Navigate(plan);
            }
            else if (act == "ouvr")
            {
                //ouvrir un projet existant
                Plan plan = new Plan(this.mode, new PlanDeTravail());
                plan.LoadThings();
                this.NavigationService.Navigate(plan);
            }
            else
            {
                //close

                MainWindow win = (ToProfil.MainWindow)Window.GetWindow(this);
                win.redButton = false;
                win.Close();
            }
        }
        #endregion
    }
}
