using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace ToProfil
{
    public partial class Plan : Page
    {
        #region constructeurs
        public Plan(String mode, PlanDeTravail planDeTravail)
        {

            this.mode = mode;
            this.planDeTravail = planDeTravail;
            InitializeComponent();
            Problems_Checked();
            SetMode();
            paste.IsEnabled = false;
            copy.IsEnabled = false;
            back.IsEnabled = false;
            forward.IsEnabled = false;
            Undo_Redo.plans_redo.Clear();
            Undo_Redo.plans_undo.Clear();
            Undo_Redo.Save(this);
        }

        public Plan(String mode, Image imag, Canvas canva, PlanDeTravail planDeTravail)
        {
            this.mode = mode;
            this.planDeTravail = planDeTravail;
            InitializeComponent();
            if (imag.Source != null)
            {
                this.imag.Source = imag.Source;
            }
            UIElement[] table = new UIElement[canva.Children.Count];
            canva.Children.CopyTo(table, 0);
            foreach (UIElement child in table)
            {
                canva.Children.Remove(child);
                if (!(child is Image))
                {
                    this.DrawingArea.Children.Add(child);
                }
            }
            Problems_Checked();
            SetMode();
            paste.IsEnabled = false;
            copy.IsEnabled = false;
            back.IsEnabled = false;
            forward.IsEnabled = false;
            Undo_Redo.Save(this);
        }
        #endregion

        #region Data 
        public bool setdata = true;
        public string mode;
        public PlanDeTravail planDeTravail;
        private bool drawSegment = false;
        private bool selectSegment = false;
        private readonly Rectangle rect1 = new Rectangle(); //extrémité du segment
        private readonly Rectangle rect2 = new Rectangle();
        private Rectangle selectedRect = null;
        private bool dragSeg = false;
        private bool dragRect = false;
        private Polyline newPoly = null;
        private Point point = new Point();
        private readonly List<Ellipse> pointsSelect = new List<Ellipse>();
        private Polyline selectedPoly = null;
        private Ellipse selectedPoint = null;
        private bool dragPoly = false;
        private bool dragPoint = false;
        private Point click = new Point();
        private double tailleRond = 10;
        private double tailleRondSelection = 12;
        private double tailleRondGrand = 17;
        private readonly Brush draggingCourbe = Brushes.DarkMagenta;
        private readonly Brush draggingPoint = Brushes.DarkBlue;
        private readonly Brush showPointSegment = Brushes.Red;
        private bool refermerCourbe = false;
        private bool courbe_enabled = false, segment_enabled = false, ancrage_enabled = false;
        private readonly byte press = 63;
        public string act;
        private bool inter_seg = false, inter_cc = false;
        private bool unit = false, proceed = true;
        private bool conditiona = false, conditioni = false, conditionc = false, conditionq = false;
        Courbe pressePapier = null;
        string src_file;

        // Zoom
        private readonly double zoomMax = 5;
        private readonly double zoomMin = 1;
        private readonly double zoomSpeed = 0.001;
        private double leZoom = 1;
        private int i = 0, cd = 0, fnt = 14;
        #endregion

        #region methodes de dessin
        public void CliqueGauche_canvas(object sender, MouseButtonEventArgs e)
        {
            if (courbe_enabled == true) //Outil dessin courbe
            {
                DessinCourbe(sender, e);
            }

            else if (segment_enabled == true) // Outil dessin segment
            {
                DessinSegment(sender, e);
            }

            else if (ancrage_enabled == true) //ajout du point d'ancretage
            {
                AjoutPointAncrage(sender, e);
            }
            else
            {
                Selection(sender, e);
            }

        }//Evenement clique gauche sur le canvas
        public void DessinSegment(object sender, MouseButtonEventArgs e)
        {
            point = e.GetPosition(DrawingArea);
            if (planDeTravail.Segment == null)
            {
                DeselectionCourbe();
                selectSegment = true;
                drawSegment = true;
                planDeTravail.Segment = new Line
                {
                    X2 = point.X,
                    Y2 = point.Y,
                    X1 = point.X,
                    Y1 = point.Y,
                    StrokeThickness = 2,
                    Stroke = new SolidColorBrush(Color.FromRgb(7, 0, 18))
                };
                ;
                DrawingArea.Children.Add(planDeTravail.Segment);
                AfficherPointsSegment();
            }
            else if (drawSegment)
            {
                drawSegment = false;
                planDeTravail.Segment.X2 = point.X;
                planDeTravail.Segment.Y2 = point.Y;
                AfficherPointsSegment();
                erase.IsEnabled = true;
            }
            else
            {
                DeselectionCourbe();
                SupprimerPointsSegment();
                DrawingArea.Children.Remove(planDeTravail.Segment);
                drawSegment = true;
                planDeTravail.Segment = new Line
                {
                    X2 = point.X,
                    Y2 = point.Y,
                    X1 = point.X,
                    Y1 = point.Y,
                    StrokeThickness = 2,
                    Stroke = new SolidColorBrush(Color.FromRgb(7, 0, 18))
                };
                ;
                DrawingArea.Children.Add(planDeTravail.Segment);
                AfficherPointsSegment();
            }
            Undo_Redo.Save(this);
        }//Evenement dessin d'un segment
        public void DessinCourbe(object sender, MouseButtonEventArgs e)
        {
            setdata = true;
            point = e.GetPosition(DrawingArea);
            if (e.ClickCount == 2) return;
            object selection = this.InputHitTest(e.GetPosition(this)) as FrameworkElement;
            if (selection is Ellipse) //Si on referme la courbe
            {
                if (newPoly != null && newPoly.Points[0] == ((Ellipse)selection).RenderTransformOrigin && newPoly.Points.Count > 2)
                {
                    ReduirePremierPoint(selection, e);
                    if (newPoly.Points.Count >= 2)
                    {
                        Courbe courbe = new Courbe(newPoly);
                        planDeTravail.AddCourbe(courbe);
                    }
                    newPoly = null;
                    return;
                }
                if (newPoly != null) return;
            }
            if (newPoly == null)    //Créer une nouvelle Polyligne
            {
                if ((selectedPoly == null) || (planDeTravail.GetCourbe(selectedPoly).GetInitialise() == true))
                {
                    DeselectionTotale();
                    newPoly = new Polyline
                    {
                        StrokeThickness = 3,
                        Stroke = Brushes.Black
                    };
                    newPoly.Points.Add(point);
                    newPoly.Points.Add(point);
                    DrawingArea.Children.Add(newPoly);
                    newPoly.Margin = new Thickness(0, 0, 0, 0); //Marge par défault //Pour bouger les courbes
                    selectedPoly = newPoly;
                    AfficherPoint(newPoly);
                    Undo_Redo.Save(this);
                }

            }
            else   //Ajouter un point à la polyligne
            {
                if (point.X != newPoly.Points[newPoly.Points.Count - 2].X || point.Y != newPoly.Points[newPoly.Points.Count - 2].Y)
                {
                    newPoly.Points.Add(point);
                    AfficherPoint(newPoly);
                    Undo_Redo.Save(this);

                }
            }

        } //Evenement dessin d'une courbe
        public void Selection(object sender, MouseButtonEventArgs e)
        {
            #region Sélection des objets sur canevas 
            object selection = this.InputHitTest(e.GetPosition(this)) as FrameworkElement;  //Récupérer sur quoi on vient de cliquer
            setdata = true;
            Hide_interSeg();
            Problems_Unchecked();
            if (selection != null)
            {
                erase.IsEnabled = true;
                if (selection is Polyline) //si on a cliqué sur une courbe
                {
                    bool save = false;
                    #region Sélection d'une courbe
                    DeselectionSegment();
                    if (selectedPoint != null) save = true;
                    selectedPoint = null; // On déselectionne le point
                    if (selectedPoly != null)
                    {
                        if ((Polyline)selection != selectedPoly) //On desélectionne l'ancienne si ce n'est pas la même
                        {
                            SupprimerPoints();
                            DeselectionCourbe();
                            save = true;
                        }
                    }
                    else save = true;
                    selectedPoly = (Polyline)selection;
                    AfficherPoint((Polyline)selection); //On affiche ses points d'ancrage
                    click = e.GetPosition(selectedPoly); //Pour déclancher le mouvement des coubes simultanément
                    selectedPoly.MouseDown += new MouseButtonEventHandler(Drag_selectedPoly); //afin de gérer les évènements de la courbe
                    selectedPoly.MouseUp += new MouseButtonEventHandler(Stop_dragging);
                    selectedPoly.MouseMove += new MouseEventHandler(Bouger_courbe);
                    alt_canva.Visibility = Visibility.Visible;
                    altitude.BorderBrush = null;
                    altitude.ToolTip = null;
                    if (planDeTravail.GetCourbe(selectedPoly).Color == Color.FromRgb(181, 2, 71))
                    {
                        altitude.BorderBrush = Brushes.Red;
                        altitude.ToolTip = "Veuillez introduire une altitude valide";
                    }
                    else
                    {
                        altitude.BorderBrush = null;
                        altitude.ToolTip = null;
                    }
                    setdata = true;
                    altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    altitude.Text = planDeTravail.GetCourbe(selectedPoly).alt.ToString();
                    altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    if (save)
                    {
                        Undo_Redo.Save(this);
                    }
                    this.Cursor = Cursors.SizeAll;
                    dragPoly = true;
                    selectedPoly.Stroke = draggingCourbe;
                    SupprimerPoints();
                    copy.IsEnabled = true;
                    erase.IsEnabled = true;
                    ancretage.IsEnabled = true;
                    cut.IsEnabled = true;
                    #endregion
                }
                else if (selection is Line)
                {
                    bool save = false;
                    DeselectionCourbe();
                    if (!selectSegment) save = true;
                    if (selectedRect != null) save = true;
                    selectedRect = null;
                    selectSegment = true;
                    erase.IsEnabled = true;
                    planDeTravail.Segment.MouseDown += new MouseButtonEventHandler(Drag_Segment); //afin de gérer les évènements de la courbe
                    planDeTravail.Segment.MouseUp += new MouseButtonEventHandler(Stop_draggingSegment);
                    planDeTravail.Segment.MouseMove += new MouseEventHandler(Bouger_segment);
                    if (save)
                    {
                        Undo_Redo.Save(this);
                    }
                    click = e.GetPosition(planDeTravail.Segment); //Pour déclancher le mouvement des coubes simultanément
                    this.Cursor = Cursors.SizeAll;
                    dragSeg = true;
                    planDeTravail.Segment.Stroke = draggingCourbe;
                    SupprimerPointsSegment();
                }
                else if ((selection is Ellipse) && (selectedPoly != null) && (selectedPoly.Points.Contains(((Ellipse)selection).RenderTransformOrigin))) //si on a cliqué sur un point 
                {
                    if (selectedPoly != null)
                    {
                        #region Sélection d'un point d'une courbe 
                        bool save = false;
                        if (selectedPoint != null && selectedPoint != (Ellipse)selection) //On déselectionne l'ancien point 
                        {
                            selectedPoint.Fill = Brushes.DarkRed;
                            selectedPoint.Width = tailleRond;
                            selectedPoint.Height = tailleRond;
                            save = true;
                        }
                        else if (selectedPoint == null) save = true;

                        {
                            List<Ellipse> listePoints = GetEllipses(((Ellipse)selection).RenderTransformOrigin);
                            if (listePoints.Count >= 2) DrawingArea.Children.Remove(listePoints[0]);
                        }

                        selectedPoint = (Ellipse)selection;

                        if ((selectedPoint.RenderTransformOrigin == selectedPoly.Points.Last() || selectedPoint.RenderTransformOrigin == selectedPoly.Points[0]) && selectedPoly.Points[0] == selectedPoly.Points.Last())
                        {

                            Ellipse el = new Ellipse
                            {
                                RenderTransformOrigin = selectedPoint.RenderTransformOrigin,
                                Width = tailleRond,
                                Height = tailleRond
                            };
                            Canvas.SetTop(el, el.RenderTransformOrigin.Y - tailleRond / 2);
                            Canvas.SetLeft(el, el.RenderTransformOrigin.X - tailleRond / 2);
                            el.Fill = Brushes.DarkRed;
                            pointsSelect.Add(el);
                            DrawingArea.Children.Add(el);
                        }
                        else if (selectedPoint.RenderTransformOrigin == selectedPoly.Points[0])
                        {
                            List<Ellipse> listePoints = GetEllipses(selectedPoly.Points.Last());
                            pointsSelect.Remove(listePoints[0]);
                            DrawingArea.Children.Remove(listePoints[0]);
                            Ellipse el = new Ellipse
                            {
                                RenderTransformOrigin = listePoints[0].RenderTransformOrigin,
                                Width = tailleRond,
                                Height = tailleRond
                            };
                            Canvas.SetTop(el, el.RenderTransformOrigin.Y - tailleRond / 2);
                            Canvas.SetLeft(el, el.RenderTransformOrigin.X - tailleRond / 2);
                            el.Fill = Brushes.DarkRed;
                            pointsSelect.Add(el);
                            DrawingArea.Children.Add(el);
                            el.MouseEnter += new MouseEventHandler(AggrandirDernierPointDrag);
                            el.MouseLeave += new MouseEventHandler(ReduireDernierPointDrag);
                            el.MouseUp += new MouseButtonEventHandler(FermerCourbeDrag);
                        }
                        else if (selectedPoint.RenderTransformOrigin == selectedPoly.Points.Last())
                        {
                            List<Ellipse> listePoints = GetEllipses(selectedPoly.Points[0]);
                            pointsSelect.Remove(listePoints[0]);
                            DrawingArea.Children.Remove(listePoints[0]);
                            Ellipse el = new Ellipse
                            {
                                RenderTransformOrigin = listePoints[0].RenderTransformOrigin,
                                Width = tailleRond,
                                Height = tailleRond
                            };
                            Canvas.SetTop(el, el.RenderTransformOrigin.Y - tailleRond / 2);
                            Canvas.SetLeft(el, el.RenderTransformOrigin.X - tailleRond / 2);
                            el.Fill = Brushes.DarkRed;
                            pointsSelect.Add(el);
                            DrawingArea.Children.Add(el);
                            el.MouseEnter += new MouseEventHandler(AggrandirPremierPointDrag);
                            el.MouseLeave += new MouseEventHandler(ReduirePremierPointDrag);
                            el.MouseUp += new MouseButtonEventHandler(FermerCourbeDrag);
                        }

                        selectedPoint.MouseDown += new MouseButtonEventHandler(Drag_selectedPoint);
                        selectedPoint.MouseUp += new MouseButtonEventHandler(Stop_draggingPoint);
                        selectedPoint.MouseMove += new MouseEventHandler(Bouger_pointDeCourbe);
                        if (save)
                        {
                            Undo_Redo.Save(this);
                        }
                        click = e.GetPosition(DrawingArea); //Pour mouver le point simultanément 
                        dragPoint = true;
                        selectedPoint.Fill = draggingPoint;
                        this.Cursor = Cursors.SizeAll;
                        selectedPoint.Height = tailleRondSelection;
                        selectedPoint.Width = tailleRondSelection;
                        #endregion
                    }
                }
                else if (selection is Rectangle)
                {
                    bool save = false;
                    if (selectedRect != null && selectedRect != (Rectangle)selection) //On déselectionne l'ancien point 
                    {
                        selectedRect.Fill = showPointSegment;
                        save = true;
                    }
                    else if (selectedRect == null)
                    {
                        save = true;
                    }
                    selectedRect = (Rectangle)selection;
                    selectedRect.MouseDown += new MouseButtonEventHandler(Drag_SegPoint);
                    selectedRect.MouseUp += new MouseButtonEventHandler(Stop_draggingSegPoint);
                    selectedRect.MouseMove += new MouseEventHandler(Bouger_pointSeg);
                    if (save)
                    {
                        Undo_Redo.Save(this);
                    }
                    click = e.GetPosition(DrawingArea); //Pour mouver le point simultanément 
                    dragRect = true;
                    this.Cursor = Cursors.SizeAll;
                    selectedRect.Height = tailleRondSelection;
                    selectedRect.Width = tailleRondSelection;

                    selectedRect.Fill = draggingPoint;
                }
                else //déselection
                {
                    DeselectionTotale();
                }
            }

            #endregion
        } //Evenement selection d'un point / segment ou d'une courbe
        public void Drag_selectedPoint(object sender, MouseButtonEventArgs e)
        {
            if (selectedPoint != null)
            { 
            dragPoint = true;
            dragPoly = false;
            click = e.GetPosition(DrawingArea);
            selectedPoint.Height = tailleRondSelection;
            selectedPoint.Width = tailleRondSelection;
            this.Cursor = Cursors.SizeAll;
                if ((selectedPoint.RenderTransformOrigin == selectedPoly.Points.Last() || selectedPoint.RenderTransformOrigin == selectedPoly.Points[0]) && selectedPoly.Points[0] == selectedPoly.Points.Last())
                {
                    //ajouter un point si la courbe est fermée (un pour le début et un pour la fin)
                    Ellipse el = new Ellipse
                    {
                        RenderTransformOrigin = selectedPoly.Points.Last(),
                        Width = tailleRond,
                        Height = tailleRond
                    };
                    Canvas.SetTop(el, el.RenderTransformOrigin.Y - tailleRond / 2);
                    Canvas.SetLeft(el, el.RenderTransformOrigin.X - tailleRond / 2);
                    el.Fill = Brushes.DarkRed;
                    pointsSelect.Add(el);
                    DrawingArea.Children.Add(el);
                }
                else if (selectedPoint.RenderTransformOrigin == selectedPoly.Points[0])
                {
                    List<Ellipse> listePoints = GetEllipses(selectedPoly.Points.Last());
                    pointsSelect.Remove(listePoints[0]);
                    DrawingArea.Children.Remove(listePoints[0]);
                    Ellipse el = new Ellipse
                    {
                        RenderTransformOrigin = listePoints[0].RenderTransformOrigin,
                        Width = tailleRond,
                        Height = tailleRond
                    };
                    Canvas.SetTop(el, el.RenderTransformOrigin.Y - tailleRond / 2);
                    Canvas.SetLeft(el, el.RenderTransformOrigin.X - tailleRond / 2);
                    el.Fill = Brushes.DarkRed;
                    pointsSelect.Add(el);
                    DrawingArea.Children.Add(el);
                    el.MouseEnter += new MouseEventHandler(AggrandirDernierPointDrag);
                    el.MouseLeave += new MouseEventHandler(ReduireDernierPointDrag);
                    el.MouseUp += new MouseButtonEventHandler(FermerCourbeDrag);
                }
                else if (selectedPoint.RenderTransformOrigin == selectedPoly.Points.Last())
                {
                    List<Ellipse> listePoints = GetEllipses(selectedPoly.Points[0]);
                    pointsSelect.Remove(listePoints[0]);
                    DrawingArea.Children.Remove(listePoints[0]);
                    Ellipse el = new Ellipse
                    {
                        RenderTransformOrigin = listePoints[0].RenderTransformOrigin,
                        Width = tailleRond,
                        Height = tailleRond
                    };
                    Canvas.SetTop(el, el.RenderTransformOrigin.Y - tailleRond / 2);
                    Canvas.SetLeft(el, el.RenderTransformOrigin.X - tailleRond / 2);
                    el.Fill = Brushes.DarkRed;
                    pointsSelect.Add(el);
                    DrawingArea.Children.Add(el);
                    el.MouseEnter += new MouseEventHandler(AggrandirPremierPointDrag);
                    el.MouseLeave += new MouseEventHandler(ReduirePremierPointDrag);
                    el.MouseUp += new MouseButtonEventHandler(FermerCourbeDrag);
                }
            }
        }//Evenement deplacer un point d'une courbe
        public void Stop_draggingPoint(object sender, MouseButtonEventArgs e)
        {
            if (dragPoint)
            {
                dragPoint = false;
                if (inter_seg == true)
                {
                    Intersection();
                }
                if (inter_cc == true)
                {
                    Problems_Checked();
                }
                selectedPoint.Height = 10;
                selectedPoint.Width = 10;
                this.Cursor = Cursors.Arrow;
                if (refermerCourbe)
                {
                    if (selectedPoint.RenderTransformOrigin == selectedPoly.Points[0])
                    {
                        selectedPoly.Points[0] = selectedPoly.Points.Last();
                        AfficherPoint(selectedPoly);
                    }
                    else if (selectedPoint.RenderTransformOrigin == selectedPoly.Points.Last())
                    {
                        selectedPoly.Points[selectedPoly.Points.Count() - 1] = selectedPoly.Points[0];
                        AfficherPoint(selectedPoly);
                    }
                    refermerCourbe = false;
                }
                Point pnt = e.GetPosition(DrawingArea);
                if (click.X != pnt.X || click.Y != pnt.Y) Undo_Redo.Save(this);
            }
        }//Evenement on arrete de deplacer un point d'une courbe
        public void Bouger_pointDeCourbe(object sender, MouseEventArgs e)
        {
            if (dragPoint)
            {
                int ind = selectedPoly.Points.IndexOf(selectedPoint.RenderTransformOrigin);
                Canvas.SetLeft(selectedPoint, e.GetPosition(DrawingArea).X - tailleRondSelection / 2);
                Canvas.SetTop(selectedPoint, e.GetPosition(DrawingArea).Y - tailleRondSelection / 2);
                Point newCenter = e.GetPosition(DrawingArea);
                selectedPoint.RenderTransformOrigin = newCenter;
                selectedPoly.Points[ind] = newCenter;
                this.Cursor = Cursors.SizeAll;

            }
        } //Evenement bouger un point d'une courbe
        public void Drag_SegPoint(object sender, MouseButtonEventArgs e)
        {
            if (selectedRect != null)
            {
                dragRect = true;
                dragSeg = false;
                click = e.GetPosition(DrawingArea);
                selectedRect.Height = tailleRondSelection;
                selectedRect.Width = tailleRondSelection;
                this.Cursor = Cursors.SizeAll;
            }
        }//Evenement deplacement d'un point du segment
        public void Stop_draggingSegPoint(object sender, MouseButtonEventArgs e)
        {
            if (dragRect)
            {
                dragRect = false;
                if (inter_seg == true)
                {
                    Intersection();
                }
                if (inter_cc == true)
                {
                    Problems_Checked();
                }
                selectedRect.Height = 10;
                selectedRect.Width = 10;
                this.Cursor = Cursors.Arrow;
                Point pnt = e.GetPosition(DrawingArea);
                if (click.X != pnt.X || click.Y != pnt.Y) Undo_Redo.Save(this);

            }
        } //Evenement on arrete de bouger un point du segment
        public void Bouger_pointSeg(object sender, MouseEventArgs e)
        {
            if (dragRect)
            {
                Canvas.SetLeft(selectedRect, e.GetPosition(DrawingArea).X - tailleRondSelection / 2);
                Canvas.SetTop(selectedRect, e.GetPosition(DrawingArea).Y - tailleRondSelection / 2);
                Point newCenter = e.GetPosition(DrawingArea);
                if (selectedRect == rect1)
                {
                    planDeTravail.Segment.X1 = newCenter.X;
                    planDeTravail.Segment.Y1 = newCenter.Y;
                }
                else if (selectedRect == rect2)
                {
                    planDeTravail.Segment.X2 = newCenter.X;
                    planDeTravail.Segment.Y2 = newCenter.Y;
                }

                this.Cursor = Cursors.SizeAll;

            }
        } //Evenement bouger un point du segment
        public void FermerCourbeDrag(object sender, MouseButtonEventArgs e)
        {
            if (dragPoint)
            {
                dragPoint = false;
                selectedPoint.Height = 10;
                selectedPoint.Width = 10;
                this.Cursor = Cursors.Arrow;
                if (refermerCourbe)
                {
                    if (selectedPoint.RenderTransformOrigin == selectedPoly.Points[0])
                    {
                        selectedPoly.Points[0] = selectedPoly.Points.Last();
                        AfficherPoint(selectedPoly);
                    }
                    else if (selectedPoint.RenderTransformOrigin == selectedPoly.Points.Last())
                    {
                        selectedPoly.Points[selectedPoly.Points.Count() - 1] = selectedPoly.Points[0];
                        AfficherPoint(selectedPoly);
                    }
                    refermerCourbe = false;
                }
            }
        }
        public void Drag_selectedPoly(object sender, MouseButtonEventArgs e)
        {
            if (courbe_enabled == false && ancrage_enabled == false && segment_enabled == false && selectedPoly != null)
            {
                dragPoly = true;
                click = e.GetPosition(selectedPoly);
                selectedPoly.Stroke = draggingCourbe;
                SupprimerPoints();
                this.Cursor = Cursors.SizeAll;
            }
        }//Evenement deplacer une courbe
        public void Drag_Segment(object sender, MouseButtonEventArgs e)
        {
            if (courbe_enabled == false && ancrage_enabled == false && segment_enabled == false)
            {
                dragSeg = true;
                click = e.GetPosition(planDeTravail.Segment);
                planDeTravail.Segment.Stroke = draggingCourbe;
                SupprimerPointsSegment();
                this.Cursor = Cursors.SizeAll;
            }
        } //Evenement deplacer le segment
        public void Mouvement_canvas(object sender, MouseEventArgs e)
        {
            if (newPoly != null && courbe_enabled == true)
            {
                newPoly.Points[newPoly.Points.Count - 1] = e.GetPosition(DrawingArea);
            }
            if (drawSegment && segment_enabled == true)
            {
                planDeTravail.Segment.X2 = e.GetPosition(DrawingArea).X;
                planDeTravail.Segment.Y2 = e.GetPosition(DrawingArea).Y;
            }
            if (courbe_enabled == false && segment_enabled == false)
            {
                if (dragPoint)
                {
                    int ind = selectedPoly.Points.IndexOf(selectedPoint.RenderTransformOrigin);
                    Canvas.SetLeft(selectedPoint, e.GetPosition(DrawingArea).X - tailleRondSelection / 2);
                    Canvas.SetTop(selectedPoint, e.GetPosition(DrawingArea).Y - tailleRondSelection / 2);
                    Point newCenter = e.GetPosition(DrawingArea);
                    selectedPoint.RenderTransformOrigin = newCenter;
                    selectedPoly.Points[ind] = newCenter;
                    this.Cursor = Cursors.SizeAll;

                }
                else if (dragPoly)
                {
                    selectedPoly.Margin = new Thickness(e.GetPosition(DrawingArea).X - click.X, e.GetPosition(DrawingArea).Y - click.Y, 0, 0);
                    this.Cursor = Cursors.SizeAll;
                    selectedPoly.Stroke = draggingCourbe;

                }
                else if (dragRect)
                {
                    Canvas.SetLeft(selectedRect, e.GetPosition(DrawingArea).X - tailleRondSelection / 2);
                    Canvas.SetTop(selectedRect, e.GetPosition(DrawingArea).Y - tailleRondSelection / 2);
                    Point newCenter = e.GetPosition(DrawingArea);
                    if (selectedRect == rect1)
                    {
                        planDeTravail.Segment.X1 = newCenter.X;
                        planDeTravail.Segment.Y1 = newCenter.Y;
                    }
                    else if (selectedRect == rect2)
                    {
                        planDeTravail.Segment.X2 = newCenter.X;
                        planDeTravail.Segment.Y2 = newCenter.Y;
                    }

                    this.Cursor = Cursors.SizeAll;

                }
                else if (dragSeg)
                {
                    planDeTravail.Segment.Margin = new Thickness(e.GetPosition(DrawingArea).X - click.X, e.GetPosition(DrawingArea).Y - click.Y, 0, 0);
                    this.Cursor = Cursors.SizeAll;

                }
            }
        } //Evenement la souris bouge sur le canvas
        public void Stop_dragging(object sender, MouseButtonEventArgs e)
        {
            if (dragPoly)
            {
                dragPoly = false;
                selectedPoly.Stroke = new SolidColorBrush(planDeTravail.GetCourbe(selectedPoly).Color);
                Thickness marge = (selectedPoly).Margin;
                (selectedPoly).Margin = new Thickness(0, 0, 0, 0);
                Polyline interPoints = new Polyline();
                Point pt = new Point();
                for (int j = 0; j < (selectedPoly).Points.Count; j++)
                {
                    pt.X = (selectedPoly).Points[j].X + marge.Left;
                    pt.Y = (selectedPoly).Points[j].Y + marge.Top;
                    interPoints.Points.Add(pt);
                }
             (selectedPoly).Points = interPoints.Points;
                AfficherPoint((selectedPoly));
                this.Cursor = Cursors.Arrow;
                if (inter_seg == true)
                {
                    Intersection();
                }
                if (inter_cc == true)
                {
                    Problems_Checked();
                }
                if (marge.Left != 0 && marge.Top != 0) Undo_Redo.Save(this); //Sinon il a pas vraiment bougé (sélection)
            }
        }//Evenement on arrete de bouger une courbe 
        public void Stop_draggingSegment(object sender, MouseButtonEventArgs e)
        {
            if (dragSeg)
            {
                dragSeg = false;
                planDeTravail.Segment.Stroke = new SolidColorBrush(Color.FromRgb(7, 0, 18)); ;
                Thickness marge = planDeTravail.Segment.Margin;
                planDeTravail.Segment.Margin = new Thickness(0, 0, 0, 0);

                planDeTravail.Segment.X1 += marge.Left;
                planDeTravail.Segment.X2 += marge.Left;
                planDeTravail.Segment.Y1 += marge.Top;
                planDeTravail.Segment.Y2 += marge.Top;
                AfficherPointsSegment();
                this.Cursor = Cursors.Arrow;
                if (inter_seg == true)
                {
                    Intersection();
                }
                if (inter_cc == true)
                {
                    Problems_Checked();
                }
                if (marge.Left != 0 && marge.Top != 0) Undo_Redo.Save(this);
            }
        } //Evenement on arrete de bouger le segment
        public void Bouger_courbe(object sender, MouseEventArgs e)
        {
            if (dragPoly)
            {
                selectedPoly.Stroke = draggingCourbe;
                this.Cursor = Cursors.SizeAll;
                (selectedPoly).Margin = new Thickness(e.GetPosition(DrawingArea).X - click.X, e.GetPosition(DrawingArea).Y - click.Y, 0, 0);
            }
        } //Evenement bouger une courbe 
        public void Bouger_segment(object sender, MouseEventArgs e)
        {
            if (dragSeg)
            {
                this.Cursor = Cursors.SizeAll;
                planDeTravail.Segment.Margin = new Thickness(e.GetPosition(DrawingArea).X - click.X, e.GetPosition(DrawingArea).Y - click.Y, 0, 0);
            }
        } //Evenement bouger le segment
        public List<Ellipse> GetEllipses(Point point)
        {
            List<Ellipse> listePoints = new List<Ellipse>();
            foreach (Ellipse pititPoint in pointsSelect)
            {
                if (pititPoint.RenderTransformOrigin == point)
                {
                    listePoints.Add(pititPoint);
                }
            }
            return listePoints;
        } // renvoie les ellipses dont le centre est 'point'
        public void SupprimerPoints() //Supprime les points d'ancrage d'une polyligne visible
        {
            foreach (Ellipse pititPoint in pointsSelect)
            {
                DrawingArea.Children.Remove(pititPoint);
            }
            pointsSelect.Clear();
        }
        public void AfficherPoint(Polyline poly) //Afficher les points d'ancrage de la polyligne 
        {
            SupprimerPoints();
            foreach (Point ptn in poly.Points)
            {
                Ellipse elipse = new Ellipse
                {
                    Width = tailleRond,
                    Height = tailleRond,
                    Fill = Brushes.DarkRed,
                    RenderTransformOrigin = ptn
                };
                Canvas.SetLeft(elipse, ptn.X - tailleRond / 2);
                Canvas.SetTop(elipse, ptn.Y - tailleRond / 2);
                DrawingArea.Children.Add(elipse);
                pointsSelect.Add(elipse);
            }
            if (pointsSelect[0] != null && newPoly != null)
            {
                pointsSelect[0].MouseEnter += new MouseEventHandler(AggrandirPremierPoint);
                pointsSelect[0].MouseLeave += new MouseEventHandler(ReduirePremierPoint);
                pointsSelect[0].MouseDown += new MouseButtonEventHandler(FermerCourbe);
            }
        }
        public void AfficherPointsSegment()
        {
            SupprimerPointsSegment();
            if (planDeTravail.Segment != null)
            {
                rect1.Width = tailleRond; rect2.Width = tailleRond;
                rect1.Height = tailleRond; rect2.Height = tailleRond;
                rect1.Fill = showPointSegment; rect2.Fill = showPointSegment;
                Canvas.SetLeft(rect1, planDeTravail.Segment.X1 - tailleRond / 2);
                Canvas.SetTop(rect1, planDeTravail.Segment.Y1 - tailleRond / 2);
                Canvas.SetLeft(rect2, planDeTravail.Segment.X2 - tailleRond / 2);
                Canvas.SetTop(rect2, planDeTravail.Segment.Y2 - tailleRond / 2);
                DrawingArea.Children.Add(rect1);
                DrawingArea.Children.Add(rect2);
            }
        } //Afficher les points d'éxtemité du segment
        public void SupprimerPointsSegment()
        {
            if (DrawingArea.Children.Contains(rect1)) DrawingArea.Children.Remove(rect1);
            if (DrawingArea.Children.Contains(rect2)) DrawingArea.Children.Remove(rect2);
        }//Supprimer les points d'éxtemité du segment
        public void AggrandirPremierPoint(object sender, MouseEventArgs e)
        {
            if (newPoly != null)
            {
                if (newPoly.Points.Count >= 2)
                {
                    if (newPoly.Points[0] == ((Ellipse)sender).RenderTransformOrigin)
                    {
                        ((Ellipse)sender).Height = tailleRondGrand;
                        ((Ellipse)sender).Width = tailleRondGrand;
                        Canvas.SetTop(((Ellipse)sender), ((Ellipse)sender).RenderTransformOrigin.Y - tailleRondGrand / 2);
                        Canvas.SetLeft(((Ellipse)sender), ((Ellipse)sender).RenderTransformOrigin.X - tailleRondGrand / 2);
                    }
                }
            }
        }
        public void AggrandirDernierPointDrag(object sender, MouseEventArgs e)
        {
            if (selectedPoly != null && dragPoint == true)
            {
                if (selectedPoly.Points.Last() == ((Ellipse)sender).RenderTransformOrigin && selectedPoint.RenderTransformOrigin == selectedPoly.Points[0])
                {
                    List<Ellipse> listePoints = GetEllipses(selectedPoly.Points.Last());

                    listePoints[0].Height = tailleRondGrand;
                    listePoints[0].Width = tailleRondGrand;
                    Canvas.SetTop(listePoints[0], listePoints[0].RenderTransformOrigin.Y - tailleRondGrand / 2);
                    Canvas.SetLeft(listePoints[0], listePoints[0].RenderTransformOrigin.X - tailleRondGrand / 2);
                    refermerCourbe = true;
                }
            }
        }
        public void ReduireDernierPointDrag(object sender, MouseEventArgs e)
        {
            if (selectedPoly != null && dragPoint == true)
            {
                if (selectedPoly.Points.Last() == ((Ellipse)sender).RenderTransformOrigin)
                {
                    List<Ellipse> listePoints = GetEllipses(selectedPoly.Points.Last());
                    listePoints[0].Height = tailleRond;
                    listePoints[0].Width = tailleRond;
                    Canvas.SetTop(listePoints[0], listePoints[0].RenderTransformOrigin.Y - tailleRond / 2);
                    Canvas.SetLeft(listePoints[0], listePoints[0].RenderTransformOrigin.X - tailleRond / 2);
                    refermerCourbe = false;
                }
            }
        }
        public void AggrandirPremierPointDrag(object sender, MouseEventArgs e)
        {
            if (selectedPoly != null && selectedPoint != null && dragPoint == true)
            {
                if (selectedPoly.Points[0] == ((Ellipse)sender).RenderTransformOrigin && selectedPoint.RenderTransformOrigin == selectedPoly.Points.Last())
                {
                    List<Ellipse> listePoints = GetEllipses(selectedPoly.Points[0]);
                    listePoints[0].Height = tailleRondGrand;
                    listePoints[0].Width = tailleRondGrand;
                    Canvas.SetTop(listePoints[0], listePoints[0].RenderTransformOrigin.Y - tailleRondGrand / 2);
                    Canvas.SetLeft(listePoints[0], listePoints[0].RenderTransformOrigin.X - tailleRondGrand / 2);
                    refermerCourbe = true;
                }
            }
        }
        public void ReduirePremierPointDrag(object sender, MouseEventArgs e)
        {
            if (selectedPoly != null && dragPoint == true)
            {
                if (selectedPoly.Points[0] == ((Ellipse)sender).RenderTransformOrigin)
                {
                    List<Ellipse> listePoints = GetEllipses(selectedPoly.Points[0]);
                    listePoints[0].Height = tailleRond;
                    listePoints[0].Width = tailleRond;
                    Canvas.SetTop(listePoints[0], listePoints[0].RenderTransformOrigin.Y - tailleRond / 2);
                    Canvas.SetLeft(listePoints[0], listePoints[0].RenderTransformOrigin.X - tailleRond / 2);
                    refermerCourbe = false;
                }
            }
        }
        public void ReduirePremierPoint(object sender, MouseEventArgs e)
        {
            if (newPoly != null)
            {
                if (newPoly.Points[0] == ((Ellipse)sender).RenderTransformOrigin)
                {
                    ((Ellipse)sender).Height = tailleRond;
                    ((Ellipse)sender).Width = tailleRond;
                    Canvas.SetTop(((Ellipse)sender), ((Ellipse)sender).RenderTransformOrigin.Y - tailleRond / 2);
                    Canvas.SetLeft(((Ellipse)sender), ((Ellipse)sender).RenderTransformOrigin.X - tailleRond / 2);
                }
            }
        }
        public void FermerCourbe(object sender, MouseButtonEventArgs e)
        {
            if (newPoly != null)
            {
                if (newPoly.Points[0] == ((Ellipse)sender).RenderTransformOrigin)
                {
                    newPoly.Points[(newPoly.Points.Count - 1)] = ((Ellipse)sender).RenderTransformOrigin;
                    if (newPoly.Points.Count >= 2)
                    {
                        Courbe courbe = new Courbe(newPoly);
                        planDeTravail.AddCourbe(courbe);
                        newPoly = null;
                        DisableAlt();
                    }
                    else
                    {
                        DrawingArea.Children.Remove(newPoly);
                        selectedPoly = null;
                        DeselectionTotale();
                        newPoly = null;
                    }
                    copy.IsEnabled = true;
                    erase.IsEnabled = true;
                    ancretage.IsEnabled = true;
                    cut.IsEnabled = true;
                }
            }

        } //Evenement fermer la courbe 
        public void CliqueDroit_canvas(object sender, MouseButtonEventArgs e)
        {
            if (courbe_enabled == true)
            {
                if (newPoly != null)
                {
                    newPoly.Points.Remove(newPoly.Points[newPoly.Points.Count - 1]);
                    if (newPoly.Points.Count >= 2)
                    {
                        Courbe courbe = new Courbe(newPoly);
                        planDeTravail.AddCourbe(courbe);
                        newPoly = null;
                        DisableAlt();
                    }
                    else
                    {
                        newPoly = null;
                        DrawingArea.Children.Remove(newPoly);
                        selectedPoly = null;
                        DeselectionTotale();
                    }

                    copy.IsEnabled = true;
                    erase.IsEnabled = true;
                    ancretage.IsEnabled = true;
                    cut.IsEnabled = true;
                }
            }
            if (drawSegment)
            {
                if (planDeTravail.Segment != null)
                {
                    SupprimerPointsSegment();
                    selectSegment = false;
                    DrawingArea.Children.Remove(planDeTravail.Segment);
                    planDeTravail.Segment = null;
                    drawSegment = false;
                    Undo_Redo.Save(this);
                }
            }

        } //Evenement clique droit sur le canvas
        public void SourisLache(object sender, MouseButtonEventArgs e)
        {
            if (dragPoint)
            {
                dragPoint = false;
                if (inter_seg == true)
                {
                    Intersection();
                }
                selectedPoint.Height = 10;
                selectedPoint.Width = 10;
                this.Cursor = Cursors.Arrow;
                Point pnt = e.GetPosition(DrawingArea);
                if (click.X != pnt.X || click.Y != pnt.Y) Undo_Redo.Save(this);
            }
            else if (dragPoly)
            {
                dragPoly = false;
                if (inter_seg == true)
                {
                    Intersection();
                }
                if (inter_cc == true)
                {
                    Problems_Checked();
                }
                selectedPoly.Stroke = new SolidColorBrush(planDeTravail.GetCourbe(selectedPoly).Color);
                Thickness marge = selectedPoly.Margin;
                selectedPoly.Margin = new Thickness(0, 0, 0, 0);
                Polyline interPoints = new Polyline();
                Point pt = new Point();
                for (int j = 0; j < selectedPoly.Points.Count; j++)
                {
                    pt.X = selectedPoly.Points[j].X + marge.Left;
                    pt.Y = selectedPoly.Points[j].Y + marge.Top;
                    interPoints.Points.Add(pt);
                }
                selectedPoly.Points = interPoints.Points;
                AfficherPoint(selectedPoly);
                this.Cursor = Cursors.Arrow;
                Undo_Redo.Save(this);
            }
            else if (dragRect)
            {
                dragRect = false;
                if (inter_seg == true)
                {
                    Intersection();
                }
                selectedRect.Height = 10;
                selectedRect.Width = 10;
                this.Cursor = Cursors.Arrow;
                Point pnt = e.GetPosition(DrawingArea);
                if (click.X != pnt.X || click.Y != pnt.Y) Undo_Redo.Save(this);
            }
            else if (dragSeg)
            {
                dragSeg = false;
                if (inter_seg == true)
                {
                    Intersection();
                }
                if (inter_cc == true)
                {
                    Problems_Checked();
                }
                planDeTravail.Segment.Stroke = new SolidColorBrush(Color.FromRgb(7, 0, 18)); ;
                Thickness marge = planDeTravail.Segment.Margin;
                planDeTravail.Segment.Margin = new Thickness(0, 0, 0, 0);
                planDeTravail.Segment.X1 += marge.Left;
                planDeTravail.Segment.X2 += marge.Left;
                planDeTravail.Segment.Y1 += marge.Top;
                planDeTravail.Segment.Y2 += marge.Top;
                AfficherPointsSegment();
                this.Cursor = Cursors.Arrow;
                Undo_Redo.Save(this);

            }
        } //Evenement lacher la souris 
        public void AjoutPointAncrage(object sender, MouseButtonEventArgs e) //Ajout point d'ancrage
        {
            object selection = this.InputHitTest(e.GetPosition(this)) as FrameworkElement;
            Point pos = e.GetPosition(DrawingArea);
            if (selection != null)
            {
                if (selection is Polyline) //si on a cliqué sur une courbe
                {
                    if (selectedPoly == (Polyline)selection)
                    {
                        Fonctions.ClosestPoint((Polyline)selection, pos, out Point clPtn, out Point seg1, out Point seg2);
                        int indice = ((Polyline)selection).Points.IndexOf(seg1);
                        ((Polyline)selection).Points.Insert(indice + 1, clPtn);
                        AfficherPoint((Polyline)selection);
                        selectedPoly = (Polyline)selection;
                        selectedPoint = null;
                        Undo_Redo.Save(this);
                    }
                }
            }
        }
        public void DeselectionCourbe()
        {
            bool save = false;
            if (selectedPoly != null)
            {
                selectedPoly.Stroke = new SolidColorBrush(planDeTravail.GetCourbe(selectedPoly).Color);
            }
            if (selectedPoly != null || selectedPoint != null) save = true;
            if (selectedPoint != null)
            {
                selectedPoint.Width = tailleRond;
                selectedPoint.Height = tailleRond;
            }
            selectedPoint = null;
            selectedPoly = null;
            SupprimerPoints();
            Disable_boxs();
            alt_canva.Visibility = Visibility.Hidden;
            if (save) Undo_Redo.Save(this);
        } //Deselection des courbes
        public void DeselectionSegment()
        {
            bool save = false;
            if (selectSegment || selectedRect != null) save = true;
            selectSegment = false;
            selectedRect = null;
            SupprimerPointsSegment();
            Disable_boxs();
            if (save) Undo_Redo.Save(this);
        } //Deselection du segment
        public void DeselectionTotale()
        {
            DeselectionCourbe();
            DeselectionSegment();
            Problems_Checked();
            if (inter_seg)
            {
                Intersection();
            }
        } //Deselection totale (segment et courbes)
        #endregion
        #region boutons principales/autres
        public void Courbe_Click(object sender, RoutedEventArgs e)
        {
            if (Courbe.IsEnabled)
            {
                //hide toolbox in case
                // hide_toolbar();
                //outil courbe activé 
                if (courbe_enabled == true) //desactiver l'outil
                {
                    DisableAll();
                    if ((selectedPoly != null) || (selectedPoint != null))
                    {
                        if (planDeTravail.GetCourbe(selectedPoly).GetInitialise() == false)
                        {

                            DisableAlt();
                        }
                        else
                        {
                            DeselectionTotale();

                        }
                    }
                    else
                    {
                        DeselectionTotale();

                    }
                }
                else
                {
                    if (inter_seg == true)
                    {
                        DisableAll();
                        Hide_interSeg();
                        Color_press(Inter, 3.75);
                        inter_seg = false;
                    }
                    DeselectionTotale();
                    DisableAll();
                    courbe_enabled = true;
                    Color_press(Courbe, -1);
                }
            }

        } //Evenement cliquer sur le bouton de dessin de courbe

        public void Segment_Click(object sender, RoutedEventArgs e)
        {
            if (Segment.IsEnabled)
            {
                //hide toolbar in case
                // hide_toolbar();
                //outil segment activé 
                if (segment_enabled == true) //desactiver l'outil
                {
                    DisableAll();
                    DeselectionSegment();
                }
                else
                {
                    DisableAll();
                    if ((selectedPoly != null) || (selectedPoint != null))
                    {
                        if (planDeTravail.GetCourbe(selectedPoly).GetInitialise() == false)
                        {

                            DisableAlt();

                        }
                        else
                        {
                            DeselectionTotale();
                        }
                    }
                    if (inter_seg == true)
                    {
                        DisableAll();
                        Hide_interSeg();
                        Color_press(Inter, 3.75);
                        inter_seg = false;
                    }
                    if (Segment.IsEnabled == true)
                    {
                        DeselectionTotale();
                    }
                    segment_enabled = true;
                    Color_press(Segment, -1);
                }
            }

        } //Evenement cliquer sur le bouton de dessin de segment

        public void Profile_Click(object sender, RoutedEventArgs e)
        {
            if (Profile.IsEnabled)
            {
                //outil profile activé
                DisableAll();
                if ((selectedPoly != null) || (selectedPoint != null))
                {
                    if (planDeTravail.GetCourbe(selectedPoly).GetInitialise() == false)
                    {

                        DisableAlt();

                    }
                    else
                    {
                        DeselectionTotale();
                    }
                }
                if (Segment.IsEnabled == true)
                {
                    //navigation vers la page de generation de profile
                    DeselectionTotale();
                    Problems_Checked();
                    //conditions
                    if ((planDeTravail.e_probems.Count == 0) && (planDeTravail.probems.Count == 0) && (planDeTravail.Courbes_prob.Count == 0) && (conditiona && conditionc && conditionq && conditioni))
                    {
                        //navigate and send the data
                        Intersection();
                        Pointa[] inters = new Pointa[planDeTravail.Ellipse_Labels.Count];
                        planDeTravail.inters.CopyTo(inters);
                        Hide_interSeg();
                        Profile_generation page = new Profile_generation(this.mode, this.imag, this.DrawingArea, this.planDeTravail, inters);
                        this.NavigationService.Navigate(page);
                        inter_cc = false;
                    }
                    else
                    {
                        // problems
                        inter_cc = true;
                        if (!(conditiona && conditionc && conditionq && conditioni))
                        {
                            Change_Click(sender, e);
                            Chg_Click(sender, e);
                            if (i != 0)
                            {
                                DeselectionTotale();
                                menu_rec.Visibility = Visibility.Hidden;
                                theCh.Visibility = Visibility.Visible;
                                imp_menu.Visibility = Visibility.Hidden;
                                Outils.IsEnabled = false;
                                Courbe.IsEnabled = false;
                                Segment.IsEnabled = false;
                                Profile.IsEnabled = false;
                                Inter.IsEnabled = false;
                            }
                        }
                        if ((planDeTravail.e_probems.Count != 0) || (planDeTravail.probems.Count != 0) || (planDeTravail.Courbes_prob.Count != 0))
                        {
                            errorMsg_canvas.Visibility = Visibility.Hidden;
                            ErrorMsg_Set();
                        }
                    }
                }
            }
        } //Evenement cliquer sur le bouton de generation du profil

        public void Home_Click(object sender, RoutedEventArgs e)
        {
            //retour vers le dashboard
            if (Home.IsEnabled)
            {
                if (Undo_Redo.plans_undo.Count > 2)
                {
                    conf_canvas.Visibility = Visibility.Visible;
                }
                else
                {
                    Dashboard dash = new Dashboard(this.mode);
                    this.NavigationService.Navigate(dash);
                }
                act = "home";
            }
        } //Evenement cliquer sur le bouton d'accueil
        #endregion

        #region Menu

        public void Menu_Click(object sender, RoutedEventArgs e)
        {
            //ouvrir le menu
            if (Menu.IsEnabled)
            {
                menu_rec.Visibility = Visibility.Visible;
                Change.IsEnabled = false;
                Hide_toolbar();
            }
        }

        public void Retour_Click(object sender, RoutedEventArgs e)
        {
            //cacher le menu
            menu_rec.Visibility = Visibility.Hidden;
            imp_menu.Visibility = Visibility.Hidden;
            Change.IsEnabled = true;
        }

        public void Nouv_Click(object sender, RoutedEventArgs e)
        {
            //creer un nouveau projet
            imp_menu.Visibility = Visibility.Hidden;
            if (Undo_Redo.plans_undo.Count > 2)
            {
                conf_canvas.Visibility = Visibility.Visible;
            }
            else
            {
                Plan plan = new Plan(this.mode, new PlanDeTravail());
                this.NavigationService.Navigate(plan);
            }
            act = "nouv";
        }

        public void Aide_Click(object sender, RoutedEventArgs e)
        {
            //ouvrir l'aide en ligne
            imp_menu.Visibility = Visibility.Hidden;
            System.Diagnostics.Process.Start("AideOnline.html");
        }

        public void Sauvegarde_Click(object sender, RoutedEventArgs e)
        {
            //sauvegarder le projet courant
            imp_menu.Visibility = Visibility.Hidden;
            DisableAll();
            if ((selectedPoly != null) || (selectedPoint != null))
            {
                if (planDeTravail.GetCourbe(selectedPoly).GetInitialise() == false)
                {

                    DisableAlt();
                    proceed = false;
                }
                else
                {
                    DeselectionTotale();
                    proceed = true;
                    //sauvegarde
                    this.SaveThings();
                }
            }
            else
            {
                //sauvegarde
                proceed = true;
                this.SaveThings();
            }
        }

        public void Importer_Click(object sender, RoutedEventArgs e)
        {
            //importer une carte

            imp_menu.Visibility = Visibility.Visible;
        }

        public void Ouvrir_Click(object sender, RoutedEventArgs e)
        {
            //ouvrir un projet existant
            act = "ouvr";
            imp_menu.Visibility = Visibility.Hidden;
            if (Undo_Redo.plans_undo.Count > 2)
            {
                conf_canvas.Visibility = Visibility.Visible;
            }
            else
            {
                //ouvrir un projet existant
                this.LoadThings();
            }

        }
        #endregion

        #region Changement des parametres de la carte
        public void Change_Click(object sender, RoutedEventArgs e)
        {
            //changer les parametres de la carte
            if (Change.IsEnabled)
            {
                DisableAll();
                if ((selectedPoly != null) || (selectedPoint != null))
                {
                    if (planDeTravail.GetCourbe(selectedPoly).GetInitialise() == false)
                    {

                        DisableAlt();

                    }
                    else
                    {
                        DeselectionTotale();
                    }
                }
                if (Segment.IsEnabled == true)
                {
                    DeselectionTotale();
                    menu_rec.Visibility = Visibility.Hidden;
                    theCh.Visibility = Visibility.Visible;
                    imp_menu.Visibility = Visibility.Hidden;
                    Outils.IsEnabled = false;
                    Courbe.IsEnabled = false;
                    Segment.IsEnabled = false;
                    Profile.IsEnabled = false;
                    Inter.IsEnabled = false;
                }
                //hide toolbar in case
                Hide_toolbar();
                Undo_Redo.Save(this); //On save le fait qu'il ouvre (avec les anciens paramètres de la carte)
            }

        }

        public void Chg_Click(object sender, RoutedEventArgs e)
        {
            i = 0; cd = 0;
            //cacher la barre de changement de parametres de la carte
            AltMax_txt(); AltMin_txt();
            if (planDeTravail.Carte.altMax <= planDeTravail.Carte.altMin)
            {
                if (conditioni && conditiona)
                {
                    altMax.BorderBrush = Brushes.Red;
                    altMax.ToolTip = "Veuillez introduire une altitude maximale valide";
                    altMin.BorderBrush = Brushes.Red;
                    altMin.ToolTip = "Veuillez introduire une altitude minimale valide";
                }
            }
            else
            {
                if (conditiona && conditionc && conditionq && conditioni)
                {
                    DataPlan lastCtxt = Undo_Redo.GetLastContexte();
                    if (lastCtxt.echelle != echel.Text || lastCtxt.equ_distance != equi.Text || lastCtxt.km_m != (String)km_m.Content || lastCtxt.altmax != altMax.Text || lastCtxt.altmin != altMin.Text)
                        Undo_Redo.Save(this); // Save avant de fermer (Si changement il y a 'v' )
                    theCh.Visibility = Visibility.Hidden;
                    Outils.IsEnabled = true;
                    Home.IsEnabled = true;
                    Courbe.IsEnabled = true;
                    Segment.IsEnabled = true;
                    Profile.IsEnabled = true;
                    Menu.IsEnabled = true;
                    Inter.IsEnabled = true;
                    altMin.BorderBrush = null;
                    altMax.BorderBrush = null;
                    echel.BorderBrush = null;
                    equi.BorderBrush = null;
                    if (inter_seg)
                    {
                        Intersection();
                    }
                    AssignColors();
                    Undo_Redo.Save(this); //Save le fait de fermer la fenêtre
                }
                else
                {
                    altMin.BorderBrush = null;
                    altMax.BorderBrush = null;
                    echel.BorderBrush = null;
                    equi.BorderBrush = null;
                }
            }
            if (!conditiona)
            {
                cd++;
                AltMax_txt();
                if (altMax.Text == "")
                {
                    i++;
                    altMax.BorderBrush = Brushes.Red;
                }
            }
            if (!conditioni)
            {
                cd++;
                AltMin_txt();
                if (altMin.Text == "")
                {
                    i++;
                    altMin.BorderBrush = Brushes.Red;
                }
            }
            if (!conditionq)
            {
                cd++;
                Equi_txt();
                if (equi.Text == "")
                {
                    i++;
                    equi.BorderBrush = Brushes.Red;
                }
            }
            if (!conditionc)
            {
                cd++;
                Echel_txt();
                if (echel.Text == "")
                {
                    i++;
                    echel.BorderBrush = Brushes.Red;
                }
            }
            if ((i == cd) && (cd != 0))
            {
                theCh.Visibility = Visibility.Hidden;
                Outils.IsEnabled = true;
                Home.IsEnabled = true;
                Courbe.IsEnabled = true;
                Segment.IsEnabled = true;
                Profile.IsEnabled = true;
                Menu.IsEnabled = true;
                Inter.IsEnabled = true;
                if (inter_seg)
                {
                    Intersection();
                }
                AssignColors();
            }


        }

        public void Echel_TextChanged(object sender, TextChangedEventArgs e)
        {
            // l'echelle a été modifiée
            Echel_txt();
        }
        public void Echel_txt()
        {
            double max = planDeTravail.Carte.Getmax();
            if (!unit)
            {
                max *= 1000;
            }
            try
            {
                if ((echel.Text == "") || (double.Parse(echel.Text) <= 0) || (double.Parse(echel.Text) > max))
                {
                    conditionc = false;
                }
                else
                {
                    conditionc = true;
                }

                if (conditionc)
                {
                    echel.BorderBrush = null;
                    planDeTravail.Carte.Echelle = double.Parse(echel.Text);
                    echel.ToolTip = null;
                }
                else
                {
                    echel.BorderBrush = Brushes.Red;
                    if (echel.Text == "")
                    {
                        echel.ToolTip = "Champs vide";
                    }
                    else if (double.Parse(echel.Text) > max)
                    {
                        echel.ToolTip = "Cette valeur dépasse la valeur maximale permise";
                    }
                    else
                    {
                        echel.ToolTip = "Veuillez introduire une échelle valide";
                    }
                }
            }
            catch (Exception)
            {
                echel.BorderBrush = Brushes.Red;
                echel.ToolTip = "Veuillez introduire une échelle valide";
            }
        }

        public void Equi_txt()
        {
            try
            {
                if ((equi.Text == "") || (double.Parse(equi.Text) <= 0))
                {
                    conditionq = false;
                }
                else
                {
                    conditionq = true;
                }
            }
            catch (Exception)
            {
                conditionq = false;
            }
            if (conditionq)
            {
                equi.BorderBrush = null;
                planDeTravail.Carte.equidistance = double.Parse(equi.Text);
                equi.ToolTip = null;
            }

            else if (equi.Text == "")
            {
                equi.BorderBrush = Brushes.Red;
                equi.ToolTip = "Champs vide";
            }
            else
            {
                equi.BorderBrush = Brushes.Red;
                equi.ToolTip = "Veuillez introduire une équidistance valide";
            }
        }
        public void Equi_TextChanged(object sender, TextChangedEventArgs e)
        {

            // l'equidistance a été modifiée
            Equi_txt();
        }

        public void AltMax_txt()
        {
            try
            {
                double.Parse(altMax.Text);
                if (altMax.Text == "")
                {
                    conditiona = false;
                }
                else
                {
                    conditiona = true;
                }
            }
            catch (Exception)
            {
                conditiona = false;
            }
            if (conditiona)
            {
                altMax.BorderBrush = null;
                planDeTravail.Carte.altMax = double.Parse(altMax.Text);
                altMax.ToolTip = null;
            }
            else if (altMax.Text == "")
            {
                altMax.BorderBrush = Brushes.Red;
                altMax.ToolTip = "Champs vide";
            }
            else
            {
                altMax.BorderBrush = Brushes.Red;
                altMax.ToolTip = "Veuillez introduire une altitude maximale valide";
            }
        }
        public void AltMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            // l'alt max a été modifiée
            try
            {
                if ((conditioni) && (double.Parse(altMax.Text) > double.Parse(altMin.Text)))
                {
                    altMin.BorderBrush = null;
                    altMin.ToolTip = null;
                }
            }
            catch (Exception) { }
            AltMax_txt();
        }

        public void AltMin_txt()
        {
            try
            {
                double.Parse(altMin.Text);
                if (altMin.Text == "")
                {
                    conditioni = false;
                }
                else
                {
                    conditioni = true;
                }
            }
            catch (Exception)
            {
                conditioni = false;
            }
            if (conditioni)
            {

                altMin.BorderBrush = null;
                planDeTravail.Carte.altMin = double.Parse(altMin.Text);
                altMin.ToolTip = null;
            }

            else if (altMin.Text == "")
            {
                altMin.ToolTip = "Champs vide";
            }
            else
            {
                altMin.BorderBrush = Brushes.Red;
                altMin.ToolTip = "Veuillez introduire une altitude minimale valide";
            }
        }
        public void AltMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            // l'alt min a été modifiée
            try
            {
                if ((conditioni) && (double.Parse(altMax.Text) > double.Parse(altMin.Text)))
                {
                    altMax.BorderBrush = null;
                    altMax.ToolTip = null;
                }
            }
            catch (Exception) { }
            AltMin_txt();
        }

        private void Echel_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) || (e.Key == Key.Down))
            {
                equi.Focus();
                Echel_txt();
            }
            if (e.Key == Key.Up)
            {
                altMax.Focus();
                Echel_txt();
            }

        }
        private void Equi_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) || (e.Key == Key.Down))
            {
                altMin.Focus();
                Equi_txt();
            }
            else if (e.Key == Key.Up)
            {
                echel.Focus();
                Equi_txt();
            }
        }
        private void AltMin_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) || (e.Key == Key.Down))
            {
                altMax.Focus();
                AltMin_txt();
            }
            else if (e.Key == Key.Up)
            {
                equi.Focus();
                AltMin_txt();
            }
        }
        private void AltMax_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Chg_Click(new object(), new RoutedEventArgs());
                AltMax_txt();
            }
            else if (e.Key == Key.Down)
            {
                echel.Focus();
                AltMax_txt();
            }
            else if (e.Key == Key.Up)
            {
                altMin.Focus();
                AltMax_txt();
            }
        }


        public void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            unit = true; //km
            km_m.Content = "Km";
            km_m.ToolTip = "Modifier l'unité de l'échelle de la carte en m";
            planDeTravail.Carte.Km = true;
            planDeTravail.Carte.m = false;
            if (echel.Text != null)
            {
                Echel_txt();
            }
        }
        public void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            unit = false;//m
            km_m.Content = "m";
            km_m.ToolTip = "Modifier l'unité de l'échelle de la carte en Km";
            planDeTravail.Carte.Km = false;
            planDeTravail.Carte.m = true;
            if (echel.Text != null)
            {
                Echel_txt();
            }
        }
        #endregion

        #region Methodes d'Intersections
        public void Inter_Click(object sender, RoutedEventArgs e)
        {
            if (Inter.IsEnabled)
            {
                //afficher les points d'intersection
                if (inter_seg == true)
                {
                    DisableAll();
                    Hide_interSeg();
                    Color_press(Inter, 3.75);
                    inter_seg = false;
                }
                else
                {
                    DisableAll();
                    inter_seg = true;
                    Color_press(Inter, -3.75);
                    if ((selectedPoly != null) || (selectedPoint != null))
                    {
                        if (planDeTravail.GetCourbe(selectedPoly).GetInitialise() == false)
                        {

                            DisableAlt();

                        }
                        else
                        {
                            DeselectionTotale();
                        }
                    }
                    if (Inter.IsEnabled == true)
                    {
                        DeselectionTotale();
                        Intersection();
                    }
                }
                Hide_toolbar();
            }
        }
        public void Problems_Checked()
        {
            Problems_Unchecked();
            inter_cc = true;
            planDeTravail.probems.Clear();
            planDeTravail.e_probems.Clear();
            List<Courbe> listecourbes = planDeTravail.Getcourbes();
            List<Point> inters_itself = new List<Point>();
            List<Point> inters_courbe_courbes = new List<Point>();

            foreach (Courbe c in listecourbes)
            {
                c.IntersectionCourbeItself(ref inters_itself);
                c.IntersectionCoubreCourbes(ref inters_courbe_courbes, listecourbes);
                foreach (Point k in inters_itself)
                {
                    if (!planDeTravail.probems.Contains(k))
                    {
                        //Intersection courbe itself
                        planDeTravail.probems.Add(k);
                        Ellipse elipse = new Ellipse
                        {
                            Width = tailleRond,
                            Height = tailleRond,
                            Fill = Brushes.DeepPink,
                            ToolTip = "Intersection de la Courbe " + planDeTravail.GetI(c) + " avec elle meme",
                            RenderTransformOrigin = k
                        };
                        Canvas.SetLeft(elipse, k.X - tailleRond / 2);
                        Canvas.SetTop(elipse, k.Y - tailleRond / 2);
                        DrawingArea.Children.Add(elipse);
                        planDeTravail.e_probems.Add(elipse);
                    }

                }
                foreach (Point k in inters_courbe_courbes)
                {
                    if (!planDeTravail.probems.Contains(k))
                    {
                        //Intersection courbe courbe
                        planDeTravail.probems.Add(k);
                        Ellipse elipse = new Ellipse
                        {
                            Width = tailleRond,
                            Height = tailleRond,
                            Fill = Brushes.HotPink
                        };
                        List<int> li = new List<int>();
                        string str = "Intersection des courbes entre elles";
                        elipse.ToolTip = str;
                        elipse.RenderTransformOrigin = k;
                        Canvas.SetLeft(elipse, k.X - tailleRond / 2);
                        Canvas.SetTop(elipse, k.Y - tailleRond / 2);
                        DrawingArea.Children.Add(elipse);
                        planDeTravail.e_probems.Add(elipse);


                    }

                }
            }

        }
        public void Problems_Unchecked()
        {
            foreach (Ellipse elp in planDeTravail.e_probems)
            {
                DrawingArea.Children.Remove(elp);
            }
            planDeTravail.e_probems.Clear();
            planDeTravail.probems.Clear();
            inter_cc = false;
        }
        public void Intersection()
        {
            Hide_interSeg();
            if ((planDeTravail.Segment != null) && (planDeTravail.Courbes.Count != 0))
            {
                planDeTravail.Interpolation();
                List<Pointa> inters = planDeTravail.Getinters();
                int i = 0;
                foreach (Pointa pp in inters)
                {
                    Label altitude = new Label();
                    Canvas.SetLeft(altitude, pp.p.X - 20);
                    Canvas.SetTop(altitude, pp.p.Y - 1);
                    DrawingArea.Children.Add(altitude);
                    Ellipse elipse = new Ellipse
                    {
                        Width = tailleRond,
                        Height = tailleRond,
                        RenderTransformOrigin = pp.p
                    };
                    Canvas.SetLeft(elipse, pp.p.X - tailleRond / 2);
                    Canvas.SetTop(elipse, pp.p.Y - tailleRond / 2);
                    DrawingArea.Children.Add(elipse);
                    altitude.FontSize = fnt;
                    altitude.Content = "P" + i;
                    elipse.ToolTip = pp.alt.ToString() + " m";
                    if (mode == "clair")
                    {
                        elipse.Fill = Brushes.Salmon;
                        altitude.Foreground = Brushes.Black;
                    }
                    else
                    {
                        elipse.Fill = Brushes.Teal;
                        altitude.Foreground = Brushes.Silver;
                    }
                    planDeTravail.Ellipse_Labels.Add(new Ellipse_point(elipse, altitude));
                    i++;
                }
            }
            else
            {
                planDeTravail.inters.Clear();
            }
        }
        public void Hide_interSeg()
        {
            foreach (Ellipse_point l in planDeTravail.Ellipse_Labels)
            {
                DrawingArea.Children.Remove(l.el);
                DrawingArea.Children.Remove(l.text);
            }
            planDeTravail.Ellipse_Labels.Clear();

        }
        #endregion

        #region Boite A Outils        
        public void Outils_Click(object sender, RoutedEventArgs e)
        {
            //boite a outils
            if (Outils.IsEnabled)
            {
                if (tool_box.Visibility == Visibility.Hidden)
                {
                    tool_box.Visibility = Visibility.Visible;
                    if (barre.Fill.ToString() == "#FF333366") //light mode
                    {
                        o_t.Source = new BitmapImage(new Uri("toolsL.png", UriKind.Relative));
                    }
                    else //dark mode 
                    {
                        o_t.Source = new BitmapImage(new Uri("toolsD.png", UriKind.Relative));
                    }
                    Outils.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                }
                else
                {
                    Hide_toolbar();
                    if (ancrage_enabled)
                    {
                        Color_press(ancretage, 3.75);
                        ancrage_enabled = false;
                    }
                }
            }
        } // Ouvrir la boite à outils

        public void Ancretage_Click(object sender, RoutedEventArgs e)
        {
            if (ancretage.IsEnabled)
            {
                //ajout d'un nouveau point d'ancrage
                if (ancrage_enabled == true)
                {
                    DisableAll();
                }
                else
                {
                    DisableAll();
                    ancrage_enabled = true;
                    Color_press(ancretage, -3.75);
                }
                zoom_slider.Visibility = Visibility.Hidden;
            }
        } //Evenement clique sur le bouton d'ancrage


        public void Erase_Click(object sender, RoutedEventArgs e)
        {
            if ((erase.IsEnabled) && (altitude.IsFocused == false))
            {
                //effacer courbe/segment
                DisableAll();
                if (selectedPoint != null)
                {
                    Point center = selectedPoint.RenderTransformOrigin;
                    selectedPoly.Points.Remove(center);
                    pointsSelect.Remove(selectedPoint);
                    DrawingArea.Children.Remove(selectedPoint);
                    selectedPoint = null;
                    if (selectedPoly == null)
                    {
                        Disable_boxs();
                    }
                    Undo_Redo.Save(this);

                }
                else if (selectedPoly != null)
                {
                    DrawingArea.Children.Remove(selectedPoly);
                    Courbe courbe = planDeTravail.GetCourbe(selectedPoly);
                    if (courbe != null) planDeTravail.RemoveCourbe(courbe);
                    alt_canva.Visibility = Visibility.Hidden;
                    newPoly = null;
                    selectedPoly = null;
                    SupprimerPoints();
                    Disable_boxs();
                    Undo_Redo.Save(this);

                }
                else if (selectSegment || selectedRect != null)
                {
                    DeselectionTotale();
                    DrawingArea.Children.Remove(planDeTravail.Segment);
                    planDeTravail.Segment = null;
                    Disable_boxs();
                    Undo_Redo.Save(this);

                }
                if (inter_seg == true)
                {
                    Intersection();
                }
                if (inter_cc == true)
                {
                    Problems_Checked();
                }
                zoom_slider.Visibility = Visibility.Hidden;
            }
            AssignColors();
            Problems_Checked();
        } //Evenement clique sur le bouton de suppression

        private void CopyCourbe()
        {
            if (selectedPoly != null)
            {
                if (planDeTravail.GetCourbe(selectedPoly) != null) pressePapier = planDeTravail.GetCourbe(selectedPoly).Duplicate();
            }
        } // Copier une courbe
        private void CutCourbe()
        {

            if (selectedPoly != null)
            {
                if (planDeTravail.GetCourbe(selectedPoly) != null)
                {
                    pressePapier = planDeTravail.GetCourbe(selectedPoly);
                    planDeTravail.RemoveCourbe(pressePapier);
                    DrawingArea.Children.Remove(selectedPoly);
                    selectedPoly = null;
                    SupprimerPoints();
                }
            }
        } // couper une courbe
        private void PasteCourbe()
        {
            DeselectionTotale();
            Fonctions.DecalerCourbe(ref pressePapier, 10, 10);
            planDeTravail.AddCourbe(pressePapier);
            DrawingArea.Children.Add(pressePapier.p);
            selectedPoly = pressePapier.p;
            AfficherPoint(selectedPoly);
            pressePapier = pressePapier.Duplicate();
            AssignColors();
        } // Coller une courbe

        public void Copy_Click(object sender, RoutedEventArgs e)
        {
            //copier courbe/segment
            if (copy.IsEnabled)
            {
                DisableAll();
                zoom_slider.Visibility = Visibility.Hidden;

                CopyCourbe();
                paste.IsEnabled = true;
            }
        } //Evenement clique sur le bouton de copie
        public void Cut_Click(object sender, RoutedEventArgs e)
        {
            //copier courbe/segment
            if (cut.IsEnabled)
            {
                DisableAll();
                zoom_slider.Visibility = Visibility.Hidden;

                CutCourbe();
                paste.IsEnabled = true;
                AssignColors();
            }
        } //Evenement clique sur le bouton de couper
        public void Paste_Click(object sender, RoutedEventArgs e)
        {
            //coller courbe/segment
            if (paste.IsEnabled)
            {
                DisableAll();
                if ((selectedPoly != null) || (selectedPoint != null))
                {
                    if (planDeTravail.GetCourbe(selectedPoly).GetInitialise() == false)
                    {

                        DisableAlt();

                    }
                    else
                    {
                        DeselectionTotale();
                        PasteCourbe();
                        if (inter_seg == true)
                        {
                            Intersection();
                        }
                        if (inter_cc == true)
                        {
                            Problems_Checked();
                        }
                    }
                }
                else
                {
                    PasteCourbe();
                }
                if (Segment.IsEnabled == true)
                {
                    DeselectionTotale();
                    if (inter_seg == true)
                    {
                        Intersection();
                    }
                    if (inter_cc == true)
                    {
                        Problems_Checked();
                    }
                }
                zoom_slider.Visibility = Visibility.Hidden;
            }
        } //Evenement clique sur le bouton de coller

        #region Zoom
        public void Zoom_Click(object sender, RoutedEventArgs e)
        {
            //zoom dans le canvas
            if (zoom_slider.Visibility == Visibility.Visible)
            {
                zoom_slider.Visibility = Visibility.Hidden;
            }
            else
            {
                zoom_slider.Visibility = Visibility.Visible;
            }
        } //Evenement clique sur le bouton de zoom
        public void DrawingArea_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control) //Pour le Ctrl + scroll
            {
                leZoom += zoomSpeed * e.Delta; // Ajust zooming speed (e.Delta = Mouse spin value )
                if (leZoom < zoomMin) { leZoom = zoomMin; } // Limit Min Scale
                if (leZoom > zoomMax) { leZoom = zoomMax; } // Limit Max Scale
                zoom_slider.Value = Math.Round(leZoom * 100, 1);
                Point mousePos = e.GetPosition(DrawingArea);

                if (leZoom > 1)
                {
                    DrawingArea.LayoutTransform = new ScaleTransform(leZoom, leZoom, mousePos.X, mousePos.Y); // transform Canvas size from mouse position
                }
                else
                {
                    DrawingArea.LayoutTransform = new ScaleTransform(leZoom, leZoom); // transform Canvas size
                }
            }
            AdjustPoints();
        } //Evenement scroll
        public void ZoomIn()
        {
            // ctrl + 
            leZoom += 0.05;
            if (leZoom < zoomMin) { leZoom = zoomMin; } // Limit Min Scale
            if (leZoom > zoomMax) { leZoom = zoomMax; } // Limit Max Scale
            zoom_slider.Value = Math.Round(leZoom * 100, 1);
            DrawingArea.LayoutTransform = new ScaleTransform(leZoom, leZoom); // transform Canvas size
            AdjustPoints();
        }  //ctrl+
        public void ZoomOut()
        {
            // ctrl - 
            leZoom -= 0.05;
            if (leZoom < zoomMin) { leZoom = zoomMin; } // Limit Min Scale
            if (leZoom > zoomMax) { leZoom = zoomMax; } // Limit Max Scale
            zoom_slider.Value = Math.Round(leZoom * 100, 1);
            DrawingArea.LayoutTransform = new ScaleTransform(leZoom, leZoom); // transform Canvas size
            AdjustPoints();
        } //ctrl-
        public void Zoom_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //zoom slider 
            leZoom = e.NewValue / 100;
            if (leZoom < zoomMin) { leZoom = zoomMin; } // Limit Min Scale
            if (leZoom > zoomMax) { leZoom = zoomMax; } // Limit Max Scale
            DrawingArea.LayoutTransform = new ScaleTransform(leZoom, leZoom); // transform Canvas size 
            AdjustPoints();
        }//Evenement modifier la valeur du zoom 
        public void AdjustPoints()
        {
            if (leZoom < 2.5)
            {
                tailleRond = 10;
                tailleRondSelection = 12;
                tailleRondGrand = 17;
                fnt = 14;
                foreach (UIElement x in DrawingArea.Children)
                {
                    if (x is Line)
                    {
                        ((Line)x).StrokeThickness = 2;
                    }
                    else if (x is Polyline)
                    {
                        ((Polyline)x).StrokeThickness = 3;
                    }
                }
            }
            else if (leZoom < 3.5)
            {
                tailleRond = 8;
                tailleRondSelection = 10;
                tailleRondGrand = 15;
                fnt = 10;
                foreach (UIElement x in DrawingArea.Children)
                {
                    if (x is Line)
                    {
                        ((Line)x).StrokeThickness = 1;
                    }
                    else if (x is Polyline)
                    {
                        ((Polyline)x).StrokeThickness = 2;
                    }
                }
            }
            else
            {
                tailleRond = 6;
                tailleRondSelection = 8;
                tailleRondGrand = 13;
                fnt = 6;
                foreach (UIElement x in DrawingArea.Children)
                {
                    if (x is Line)
                    {
                        ((Line)x).StrokeThickness = 0.5;
                    }
                    else if (x is Polyline)
                    {
                        ((Polyline)x).StrokeThickness = 1;
                    }
                }
            }
            if (leZoom >= 4.5)
            {
                tailleRond = 4;
                tailleRondSelection = 6;
                tailleRondGrand = 11;
                fnt = 4;
                foreach (UIElement x in DrawingArea.Children)
                {
                    if (x is Line)
                    {
                        ((Line)x).StrokeThickness = 0.5;
                    }
                    else if (x is Polyline)
                    {
                        ((Polyline)x).StrokeThickness = 0.5;
                    }
                }
            }
            if (selectedPoly != null)
            {
                    foreach (Ellipse ell in pointsSelect)
                {
                    ell.Width = tailleRond;
                    ell.Height = tailleRond;
                    foreach (Point p in selectedPoly.Points)
                    {
                        if (ell.RenderTransformOrigin == p)
                        {
                            Canvas.SetLeft(ell, p.X - tailleRond / 2);
                            Canvas.SetTop(ell, p.Y - tailleRond / 2);
                        }
                    }
                }

            }
            else
            {
                Problems_Checked();
            }
            if (inter_seg)
            {
                Intersection();
            }
        } //Mise à jour du canvas avec le zoom
        #endregion
        #region  undo/redo

        public void TranfereData(DataPlan dataPlan)
        {
            dataPlan.menuCarte = (theCh.Visibility == Visibility.Visible);
            if (dataPlan.menuCarte)
            {
                dataPlan.equ_distance = equi.Text;
                dataPlan.km_m = (String)km_m.Content;
                dataPlan.echelle = echel.Text;
                dataPlan.altmax = altMax.Text;
                dataPlan.altmin = altMin.Text;
            }


            dataPlan.focus = altitude.IsFocused;
            dataPlan.drawSegment = this.drawSegment;
            dataPlan.selectSegment = this.selectSegment;
            dataPlan.dragSeg = false;
            dataPlan.dragRect = false;
            dataPlan.dragPoly = false;
            dataPlan.dragPoint = false;
            dataPlan.courbe_enabled = this.courbe_enabled;
            dataPlan.segment_enabled = this.segment_enabled;
            dataPlan.ancrage_enabled = this.ancrage_enabled;
            dataPlan.act = this.act;
            dataPlan.inter_cc = this.inter_cc;
            dataPlan.inter_seg = this.inter_seg;
            dataPlan.proceed = this.proceed;
            dataPlan.conditiona = this.conditiona;
            dataPlan.conditionc = this.conditionc;
            dataPlan.conditioni = this.conditioni;
            dataPlan.conditionq = this.conditionq;

            dataPlan.planDeTravail = this.planDeTravail.Duplicate();
            dataPlan.click = new Point(this.click.X, this.click.Y);
            dataPlan.point = new Point(this.point.X, this.point.Y);

            if (this.newPoly != null)
            {
                dataPlan.newPoly = new Polyline
                {
                    Stroke = this.newPoly.Stroke,
                    StrokeThickness = this.newPoly.StrokeThickness
                }; //dupliquer new poly
                foreach (Point pt in this.newPoly.Points)
                {
                    Point point = new Point(pt.X, pt.Y);
                    dataPlan.newPoly.Points.Add(point);
                }
            }
            else
            {
                dataPlan.newPoly = null;
            }
            if (this.selectedPoly != null && this.selectedPoly != this.newPoly)
            {
                Courbe selectedCourbe = this.planDeTravail.GetCourbe(selectedPoly);
                int indexe = this.planDeTravail.Getcourbes().IndexOf(selectedCourbe);
                dataPlan.selectedPoly = (dataPlan.planDeTravail.Getcourbes())[indexe].p; //retrouver la bonne courbe (dupliquée)
                dataPlan.mainBloquee = (alt_canva.IsVisible == true && selectedCourbe.GetInitialise() == false);
            }
            else if (this.selectedPoly != null && this.selectedPoly == this.newPoly)
            {
                dataPlan.selectedPoly = dataPlan.newPoly;
                dataPlan.mainBloquee = false;
            }
            else
            {
                dataPlan.selectedPoly = null;
                dataPlan.mainBloquee = false;
            }

            if (selectedPoly != null && selectedPoint != null)
            {
                dataPlan.selectedPoint = selectedPoly.Points.IndexOf(selectedPoint.RenderTransformOrigin);
            }
            else
            {
                dataPlan.selectedPoint = -1;
            }

            if (this.selectedRect != null)
            {
                if (this.selectedRect == this.rect1)
                { dataPlan.selectedRect = 1; }
                else if (this.selectedRect == this.rect2)
                { dataPlan.selectedRect = 2; }
                else
                { dataPlan.selectedRect = -1; }
            }
            else
            { dataPlan.selectedRect = -1; }

        } //sauvegarde du contexte

        public void SetData(DataPlan dataPlan)
        {
            setdata = true;
            #region affectations 
            this.drawSegment = dataPlan.drawSegment;
            this.selectSegment = dataPlan.selectSegment;
            this.dragSeg = dataPlan.dragSeg;
            this.dragRect = dataPlan.dragRect;
            this.point = dataPlan.point;
            this.selectedPoly = dataPlan.selectedPoly;
            this.dragPoly = dataPlan.dragPoly;
            this.dragPoint = dataPlan.dragPoint;
            this.click = dataPlan.click;
            this.courbe_enabled = dataPlan.courbe_enabled;
            this.segment_enabled = dataPlan.segment_enabled;
            this.ancrage_enabled = dataPlan.ancrage_enabled;
            this.act = dataPlan.act;
            this.inter_cc = dataPlan.inter_cc;
            this.inter_seg = dataPlan.inter_seg;
            this.proceed = dataPlan.proceed;
            this.conditiona = dataPlan.conditiona;
            this.conditionc = dataPlan.conditionc;
            this.conditioni = dataPlan.conditioni;
            this.conditionq = dataPlan.conditionq;

            this.newPoly = dataPlan.newPoly;
            this.planDeTravail = dataPlan.planDeTravail;
            #endregion

            DrawingArea.Children.Clear();
            this.imag.Source = planDeTravail.Carte.imag;
            DrawingArea.Children.Add(this.imag);

            foreach (Courbe courbe in planDeTravail.Getcourbes())
            {
                DrawingArea.Children.Add(courbe.p);
            }
            if (planDeTravail.Segment != null)
            {
                DrawingArea.Children.Add(planDeTravail.Segment);
            }
            if (newPoly != null)
            {
                DrawingArea.Children.Add(newPoly);
                AfficherPoint(newPoly);
            }
            else if (selectedPoly != null)
            {
                AfficherPoint(selectedPoly);
                if (dataPlan.selectedPoint != -1)
                {
                    Point pnt = selectedPoly.Points[dataPlan.selectedPoint];
                    selectedPoint = GetEllipses(pnt)[0];
                    selectedPoint.RenderTransformOrigin = pnt;
                    #region presque comme selection Point

                    {
                        List<Ellipse> listePoints = GetEllipses(pnt);
                        if (listePoints.Count >= 2) DrawingArea.Children.Remove(listePoints[1]);
                    }
                    if ((selectedPoint.RenderTransformOrigin == selectedPoly.Points.Last() || selectedPoint.RenderTransformOrigin == selectedPoly.Points[0]) && selectedPoly.Points[0] == selectedPoly.Points.Last())
                    {

                        Ellipse el = new Ellipse
                        {
                            RenderTransformOrigin = selectedPoint.RenderTransformOrigin,
                            Width = tailleRond,
                            Height = tailleRond
                        };
                        Canvas.SetTop(el, el.RenderTransformOrigin.Y - tailleRond / 2);
                        Canvas.SetLeft(el, el.RenderTransformOrigin.X - tailleRond / 2);
                        el.Fill = Brushes.DarkRed;
                        pointsSelect.Add(el);
                        DrawingArea.Children.Add(el);
                    }
                    else if (selectedPoint.RenderTransformOrigin == selectedPoly.Points[0])
                    {
                        List<Ellipse> listePoints = GetEllipses(selectedPoly.Points.Last());
                        pointsSelect.Remove(listePoints[0]);
                        DrawingArea.Children.Remove(listePoints[0]);
                        Ellipse el = new Ellipse
                        {
                            RenderTransformOrigin = listePoints[0].RenderTransformOrigin,
                            Width = tailleRond,
                            Height = tailleRond
                        };
                        Canvas.SetTop(el, el.RenderTransformOrigin.Y - tailleRond / 2);
                        Canvas.SetLeft(el, el.RenderTransformOrigin.X - tailleRond / 2);
                        el.Fill = Brushes.DarkRed;
                        pointsSelect.Add(el);
                        DrawingArea.Children.Add(el);
                        el.MouseEnter += new MouseEventHandler(AggrandirDernierPointDrag);
                        el.MouseLeave += new MouseEventHandler(ReduireDernierPointDrag);
                        el.MouseUp += new MouseButtonEventHandler(FermerCourbeDrag);
                    }
                    else if (selectedPoint.RenderTransformOrigin == selectedPoly.Points.Last())
                    {
                        List<Ellipse> listePoints = GetEllipses(selectedPoly.Points[0]);
                        pointsSelect.Remove(listePoints[0]);
                        DrawingArea.Children.Remove(listePoints[0]);
                        Ellipse el = new Ellipse
                        {
                            RenderTransformOrigin = listePoints[0].RenderTransformOrigin,
                            Width = tailleRond,
                            Height = tailleRond
                        };
                        Canvas.SetTop(el, el.RenderTransformOrigin.Y - tailleRond / 2);
                        Canvas.SetLeft(el, el.RenderTransformOrigin.X - tailleRond / 2);
                        el.Fill = Brushes.DarkRed;
                        pointsSelect.Add(el);
                        DrawingArea.Children.Add(el);
                        el.MouseEnter += new MouseEventHandler(AggrandirPremierPointDrag);
                        el.MouseLeave += new MouseEventHandler(ReduirePremierPointDrag);
                        el.MouseUp += new MouseButtonEventHandler(FermerCourbeDrag);
                    }

                    selectedPoint.Fill = draggingPoint;
                    selectedPoint.Height = tailleRondSelection;
                    selectedPoint.Width = tailleRondSelection;
                    selectedPoint.MouseDown += new MouseButtonEventHandler(Drag_selectedPoint);
                    selectedPoint.MouseUp += new MouseButtonEventHandler(Stop_draggingPoint);
                    selectedPoint.MouseMove += new MouseEventHandler(Bouger_pointDeCourbe);
                    #endregion
                }
                selectedPoly.MouseDown += new MouseButtonEventHandler(Drag_selectedPoly); //afin de gérer les évènements de la courbe
                selectedPoly.MouseUp += new MouseButtonEventHandler(Stop_dragging);
                selectedPoly.MouseMove += new MouseEventHandler(Bouger_courbe);
            }
            if (selectSegment == true || drawSegment == true)
            {
                AfficherPointsSegment();
                if (dataPlan.selectedRect != -1)
                {
                    if (dataPlan.selectedRect == 1)
                    {
                        selectedRect = rect1;
                    }
                    else if (dataPlan.selectedRect == 2)
                    {
                        selectedRect = rect2;
                    }
                    selectedRect.Fill = draggingPoint;
                    selectedRect.MouseDown += new MouseButtonEventHandler(Drag_SegPoint);
                    selectedRect.MouseUp += new MouseButtonEventHandler(Stop_draggingSegPoint);
                    selectedRect.MouseMove += new MouseEventHandler(Bouger_pointSeg);
                }
            }
            if (!dataPlan.menuCarte)
            {
                theCh.Visibility = Visibility.Hidden;
                menu_rec.Visibility = Visibility.Hidden;
                imp_menu.Visibility = Visibility.Hidden;
                Outils.IsEnabled = true;
                Courbe.IsEnabled = true;
                Segment.IsEnabled = true;
                Profile.IsEnabled = true;
                Inter.IsEnabled = true;
            }
            Refresh(dataPlan.mainBloquee);
            if (dataPlan.menuCarte)
            {
                theCh.Visibility = Visibility.Visible;
                menu_rec.Visibility = Visibility.Hidden;
                imp_menu.Visibility = Visibility.Hidden;
                Outils.IsEnabled = false;
                Courbe.IsEnabled = false;
                Segment.IsEnabled = false;
                Profile.IsEnabled = false;
                Inter.IsEnabled = false;
                equi.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                equi.Text = dataPlan.equ_distance;
                equi.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                km_m.Content = dataPlan.km_m;
                echel.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                echel.Text = dataPlan.echelle;
                echel.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                altMax.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                altMax.Text = dataPlan.altmax;
                altMax.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                altMin.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                altMin.Text = dataPlan.altmin;
                altMin.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

        } //upload du context

        public void Refresh(bool bol)
        {
            setdata = true;
            if (newPoly != null && planDeTravail.GetCourbe(newPoly) == null)
            {
                setdata = true;
                if (planDeTravail.GetCourbe(newPoly) == null)
                {
                    EnableAlt(); //pendant le dessin
                    newPoly.Points[newPoly.Points.Count - 1] = Mouse.GetPosition(DrawingArea);
                }
                else if (planDeTravail.GetCourbe(selectedPoly).GetInitialise() == true) //Ce cas ne devrait pas être permis, mais in case of
                {
                    newPoly = null;
                    setdata = true;
                    alt_canva.Visibility = Visibility.Visible;
                    altitude.BorderBrush = null;
                    altitude.ToolTip = null;
                    if (planDeTravail.GetCourbe(selectedPoly).Color == Color.FromRgb(181, 2, 71))
                    {
                        altitude.BorderBrush = Brushes.Red;
                        altitude.ToolTip = "Veuillez introduire une altitude valide";
                    }
                    else
                    {
                        altitude.BorderBrush = null;
                        altitude.ToolTip = null;
                    }
                    setdata = true;
                    altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    altitude.Text = planDeTravail.GetCourbe(selectedPoly).alt.ToString();
                    altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    RedonnerMain();
                }
                else
                {
                    DisableAltWithoutSave();
                }
            }
            else if (selectedPoly != null)
            {
                if (planDeTravail.GetCourbe(selectedPoly).GetInitialise() == true)
                {
                    setdata = true;
                    alt_canva.Visibility = Visibility.Visible;
                    altitude.BorderBrush = null;
                    altitude.ToolTip = null;
                    if (planDeTravail.GetCourbe(selectedPoly).Color == Color.FromRgb(181, 2, 71))
                    {
                        altitude.BorderBrush = Brushes.Red;
                        altitude.ToolTip = "Veuillez introduire une altitude valide";
                    }
                    else
                    {
                        altitude.BorderBrush = null;
                        altitude.ToolTip = null;
                    }
                    setdata = true;
                    altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    altitude.Text = planDeTravail.GetCourbe(selectedPoly).alt.ToString();
                    altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    RedonnerMain();
                    AssignColors();
                }
                else
                {
                    DisableAltWithoutSave();
                }
                //inter
                if (mode == "sombre")
                {
                    Inter.Background = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                }
                else
                {
                    Inter.Background = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                }
                Problems_Unchecked();
            }
            else
            {
                EnableAlt();
                //inter
                if (mode == "sombre")
                {
                    Inter.Background = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                }
                else
                {
                    Inter.Background = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                }
                if (inter_seg)
                {
                    Color_press(Inter, -3.75);
                    Intersection();
                }
                if (inter_cc == true)
                {
                    Problems_Checked();
                }
            }
            //courbe
            Courbe.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            if (courbe_enabled)
            {
                Color_press(Courbe, -1);
            }
            //segment
            Segment.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            if (segment_enabled)
            {
                Color_press(Segment, -1);
            }
            //ancrage
            if (mode == "clair")
            {
                ancretage.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
            }
            else
            {
                ancretage.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
            }
            if (ancrage_enabled)
            {
                Color_press(ancretage, -3.75);
            }

        }  //Mise à jour du UI


        public void Back_Click(object sender, RoutedEventArgs e)
        {
            //ctrl z
            if (back.IsEnabled)
            {
                zoom_slider.Visibility = Visibility.Hidden;
                Undo_Redo.Undo(this);
            }

        } //Annuler
        public void Forward_Click(object sender, RoutedEventArgs e)
        {
            // ctrl y
            if (forward.IsEnabled)
            {
                zoom_slider.Visibility = Visibility.Hidden;
                Undo_Redo.Redo(this);
            }

        } //Rétablir
        #endregion
        #endregion

        #region Importation de la Carte
        public void Importimage_Click(object sender, RoutedEventArgs e)
        {
            //importer une carte topographique
            OpenFileDialog d = new OpenFileDialog
            {
                Title = "Importer une carte",
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.bmp) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.bmp"
            };
            if (d.ShowDialog() == true)
            {
                var src = new Uri(d.FileName);
                imag.Source = new BitmapImage(src);
                planDeTravail.Carte.imag = new BitmapImage(src);
                imp_menu.Visibility = Visibility.Hidden;
                menu_rec.Visibility = Visibility.Hidden;
                Change.IsEnabled = true;
                Undo_Redo.Save(this); //Sauvegarder le contexte
            }
        } // Import d'une image
        public void Blank_Click(object sender, RoutedEventArgs e)
        {
            //importer un plan de travail vide
            bool save = true;
            if (imag.Source == null) save = false;
            imag.Source = null;
            planDeTravail.Carte.imag = null;
            planDeTravail.Carte.ImageBuffer = null;
            imp_menu.Visibility = Visibility.Hidden;
            menu_rec.Visibility = Visibility.Hidden;
            Change.IsEnabled = true;
            if (save) Undo_Redo.Save(this); //Sauvegarder le contexte
        } // Fond d'ecran vide
        private void Image_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] Image = (string[])e.Data.GetData(DataFormats.FileDrop);
                String ImgPath = Image[0];
                try
                {
                    var src = new Uri(ImgPath);
                    imag.Source = new BitmapImage(src);
                    planDeTravail.Carte.imag = new BitmapImage(src);
                    imp_menu.Visibility = Visibility.Hidden;
                    menu_rec.Visibility = Visibility.Hidden;
                    Change.IsEnabled = true;
                    Undo_Redo.Save(this);
                }
                catch (Exception)
                {
                    //Message d'erreur, il a drop autre chose qu'une image
                    MessageBox.Show("Format non supporté");
                }

            }
        } // Drag and drop d'une image
        #endregion

        #region Couleurs
        public void Mode_Switch_Click(object sender, RoutedEventArgs e)
        {
            //changer de mode 
            if (barre.Fill.ToString() == "#FF333366")
            {
                this.mode = "sombre";
            }
            else
            {
                this.mode = "clair";
            }
            SetMode();
        }

        public void SetMode()
        {

            if (mode == "sombre")
            {
                rect_bord.Fill = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                rect_fond.Fill = new SolidColorBrush(Color.FromRgb(63, 69, 70));
                conf_oui.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                conf_non.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                txt_conf.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                errorMsg.Foreground = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                errorMsg_canvas.Background = new SolidColorBrush(Color.FromRgb(166, 208, 214));
                backg.Background = new LinearGradientBrush(Color.FromRgb(102, 100, 101), Color.FromRgb(25, 25, 25), new Point(0, 1), new Point(1, 0));
                DrawingArea.Background = new LinearGradientBrush(Color.FromRgb(25, 25, 25), Color.FromRgb(102, 100, 101), new Point(0, 1), new Point(1, 0));
                DrawingArea.Opacity = 58;
                rect.Fill = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                barre.Fill = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                cote.Fill = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                chg.Fill = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                o_c.Source = new BitmapImage(new Uri("courbe2.png", UriKind.Relative));
                o_s.Source = new BitmapImage(new Uri("segment2.png", UriKind.Relative));
                o_p.Source = new BitmapImage(new Uri("profile2.png", UriKind.Relative));
                Inter.Background = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                Home.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                Menu.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                o_m.Source = new BitmapImage(new Uri("menu2.png", UriKind.Relative));
                Outils.Background = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                o_t.Source = new BitmapImage(new Uri("tools2.png", UriKind.Relative));
                Blank.Background = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                Importimage.Background = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                imp_menu.Background = new LinearGradientBrush(Color.FromRgb(31, 71, 88), Color.FromRgb(255, 255, 255), new Point(0, 1), new Point(1, 0));
                ancretage.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                zoom.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                copy.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                cut.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                paste.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                back.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                forward.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                erase.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                zoom_slider.Foreground = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                alt_rect.Fill = new SolidColorBrush(Color.FromRgb(31, 71, 88));
                Mode_Switch.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                RotateTransform rotateTransform1 = new RotateTransform(360, 0, 0);
                Mode_Switch.RenderTransform = rotateTransform1;
                Mode_Switch.IsChecked = true;
                Mode_Switch.ToolTip = "Mode clair";
            }
            else
            {
                rect_bord.Fill = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                rect_fond.Fill = new SolidColorBrush(Color.FromRgb(246, 248, 251));
                conf_oui.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                conf_non.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                txt_conf.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                errorMsg.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                errorMsg_canvas.Background = new SolidColorBrush(Color.FromRgb(253, 192, 199));
                backg.Background = new LinearGradientBrush(Color.FromRgb(255, 255, 255), Color.FromRgb(245, 247, 251), new Point(1, 0), new Point(0, 1));
                DrawingArea.Background = new LinearGradientBrush(Color.FromRgb(245, 247, 251), Color.FromRgb(255, 255, 255), new Point(1, 0), new Point(0, 1));
                DrawingArea.Opacity = 100;
                barre.Fill = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                cote.Fill = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                rect.Fill = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                chg.Fill = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                o_c.Source = new BitmapImage(new Uri("courbe2.png", UriKind.Relative));
                o_s.Source = new BitmapImage(new Uri("segment2.png", UriKind.Relative));
                o_p.Source = new BitmapImage(new Uri("profile2.png", UriKind.Relative));
                Blank.Background = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                Importimage.Background = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                imp_menu.Background = new LinearGradientBrush(Color.FromRgb(51, 51, 102), Color.FromRgb(255, 255, 255), new Point(0, 1), new Point(1, 0));
                Inter.Background = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                o_m.Source = new BitmapImage(new Uri("menu2.png", UriKind.Relative));
                Outils.Background = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                o_t.Source = new BitmapImage(new Uri("tools2.png", UriKind.Relative));
                Home.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                Menu.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                Outils.Background = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                ancretage.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                zoom.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                copy.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                cut.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                paste.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                back.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                forward.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                erase.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                zoom_slider.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 102));
                alt_rect.Fill = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                Mode_Switch.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                RotateTransform rotateTransform1 = new RotateTransform(180, 0, 0);
                Mode_Switch.RenderTransform = rotateTransform1;
                Mode_Switch.IsChecked = true;
                Mode_Switch.ToolTip = "Mode Sombre";
            }
            if (ancrage_enabled == true)
            {
                Color_press(ancretage, -3.75);
            }
            if (inter_seg == true)
            {
                Color_press(Inter, -3.75);
                Intersection();
            }
        }

        public void Color_press(Button Button, double signe)
        {
            if ((signe == 1) || (signe == -1))
            {
                Button.Background = new SolidColorBrush(Color.FromRgb((byte)(((SolidColorBrush)Button.Background).Color.R + (signe * press)), (byte)(((SolidColorBrush)Button.Background).Color.G + (signe * press)), (byte)(((SolidColorBrush)Button.Background).Color.B + (signe * press))));
            }
            else
            {
                if (mode == "clair")
                {
                    Button.Background = new SolidColorBrush(Color.FromRgb((byte)(((SolidColorBrush)Button.Background).Color.R + (2 * signe * press)), (byte)(((SolidColorBrush)Button.Background).Color.G), (byte)(((SolidColorBrush)Button.Background).Color.B + (signe * press))));
                }
                else
                {
                    Button.Background = new SolidColorBrush(Color.FromRgb((byte)(((SolidColorBrush)Button.Background).Color.R - (0.05 * signe * press / 3.75)), (byte)(((SolidColorBrush)Button.Background).Color.G - (1.1 * signe * press / 3.75)), (byte)(((SolidColorBrush)Button.Background).Color.B - (1.5 * signe * press / 3.75))));
                }
            }
        }
        public void AssignColors()
        {
            planDeTravail.Courbes_prob.Clear();
            if (planDeTravail.Courbes.Count != 0)
            {
                foreach (Courbe courbe in planDeTravail.Courbes)
                {
                    try
                    {
                        if ((courbe.alt > planDeTravail.Carte.altMax) || (courbe.alt < planDeTravail.Carte.altMin) || (courbe.alt % planDeTravail.Carte.equidistance != 0))
                        {
                            courbe.Color = Color.FromRgb(181, 2, 71);
                            planDeTravail.Courbes_prob.Add(courbe);
                        }
                        else
                        {
                            courbe.Color = courbe.CurveColor(planDeTravail.Carte.AltMin, planDeTravail.Carte.AltMax);
                        }
                    }
                    catch (Exception)
                    {
                        courbe.Color = Color.FromRgb(181, 2, 71);
                        planDeTravail.Courbes_prob.Add(courbe);
                    }
                }
                foreach (UIElement p in DrawingArea.Children)
                {
                    if (p is Polyline)
                    {
                        try
                        {
                            ((Polyline)p).Stroke = new SolidColorBrush(planDeTravail.GetCourbe((Polyline)p).Color);
                            ((Polyline)p).ToolTip = "N°" + planDeTravail.GetI(planDeTravail.GetCourbe((Polyline)p));
                        }
                        catch (Exception)
                        {

                        }

                    }
                }
            }
        }
        #endregion

        #region Sauvegarde

        public void Conf_close_Click(object sender, RoutedEventArgs e)
        {
            conf_canvas.Visibility = Visibility.Hidden;
        }

        public void Conf_oui_Click(object sender, RoutedEventArgs e)
        {
            conf_canvas.Visibility = Visibility.Hidden;
            Sauvegarde_Click(new object(), new RoutedEventArgs());
            if (proceed)
            {
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
                    this.LoadThings();
                }
                else
                {
                    //close
                    MainWindow win = (ToProfil.MainWindow)Window.GetWindow(this);
                    win.redButton = false;
                    win.Close();
                }
                act = "";
            }
        }

        public void Conf_non_Click(object sender, RoutedEventArgs e)
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
                this.LoadThings();
            }
            else
            {
                //close
                MainWindow win = (ToProfil.MainWindow)Window.GetWindow(this);
                win.redButton = false;
                win.Close();
            }
            act = "";
        }

        public void OuvrirDoubleClick(String path)
        {
            try
            {
                var src = new Uri(path);
                src_file = System.IO.Path.GetFileNameWithoutExtension(path);
                SaveFormat fileData;
                XmlSerializer xmlSeries = new XmlSerializer(typeof(SaveFormat));
                System.IO.TextReader reader = new System.IO.StreamReader(src.ToString().Substring(8));
                fileData = (SaveFormat)xmlSeries.Deserialize(reader);
                reader.Close();
                DrawingArea.Children.Clear();
                DrawingArea.Children.Add(imag);
                planDeTravail.Carte.ImageBuffer = fileData.ImageBuffer;
                imag.Source = planDeTravail.Carte.imag;
                this.unit = fileData.unit;
                planDeTravail.Courbes.Clear();
                planDeTravail.Courbes_prob.Clear();
                planDeTravail.Segment = null;
                planDeTravail.ClearInters();
                planDeTravail.Ellipse_Labels.Clear();
                planDeTravail.probems.Clear();
                planDeTravail.e_probems.Clear();
                planDeTravail.Carte.altMax = fileData.altMax;
                planDeTravail.Carte.altMin = fileData.altMin;
                planDeTravail.Carte.equidistance = fileData.equidistance;
                planDeTravail.Carte.echelle = fileData.echelle;
                for (int i = 0; i < fileData.ctrlPoints.Count; i++)
                {
                    Courbe crb = new Courbe();
                    Polyline pol = new Polyline();
                    crb.p = pol;
                    crb.alt = fileData.alts[i];
                    crb.p.Points = fileData.ctrlPoints[i];
                    crb.SetInitialise(fileData.initialized[i]);
                    crb.Color = crb.CurveColor(planDeTravail.Carte.altMin, planDeTravail.Carte.altMax);
                    planDeTravail.Courbes.Add(crb);
                    crb.p.StrokeThickness = 3;
                    crb.p.Stroke = Brushes.Black;
                    ((Polyline)crb.p).Stroke = new SolidColorBrush(planDeTravail.GetCourbe((Polyline)crb.p).Color);
                    crb.p.Margin = new Thickness(0, 0, 0, 0); //Marge par défault pour bouger les courbes
                    DrawingArea.Children.Add(crb.p);
                }

                Line line = new Line
                {
                    X1 = fileData.p1.X,
                    Y1 = fileData.p1.Y,
                    X2 = fileData.p2.X,
                    Y2 = fileData.p2.Y
                };
                planDeTravail.Segment = line;
                planDeTravail.Segment.StrokeThickness = 2;
                planDeTravail.Segment.Stroke = new SolidColorBrush(Color.FromRgb(7, 0, 18)); ;
                DrawingArea.Children.Add(planDeTravail.Segment);

                echel.Text = planDeTravail.Carte.echelle.ToString();
                equi.Text = planDeTravail.Carte.equidistance.ToString();
                altMin.Text = planDeTravail.Carte.altMin.ToString();
                altMax.Text = planDeTravail.Carte.altMax.ToString();
                this.AssignColors();
                this.Problems_Checked();
                newPoly = null;
                selectedPoint = null;
                selectSegment = false;
                selectedRect = null;
                selectedPoly = null;
                setdata = true;
                altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                EnableAlt();
                Undo_Redo.Save(this);
            }
            catch (Exception)
            {
                // message can't load this file
                MessageBox.Show("Format non supporté");
            }

        }
        public void LoadThings()// charger le contenu d'un fichier 
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "tpr files (*.tpr)|*.tpr",
                Title = "Ouvrir un projet ToProfil",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ToProfil"
            };
            if (open.ShowDialog() == true)
            {
                try
                {
                    var src = new Uri(open.FileName);
                    src_file = System.IO.Path.GetFileNameWithoutExtension(open.FileName);
                    SaveFormat fileData;
                    XmlSerializer xmlSeries = new XmlSerializer(typeof(SaveFormat));
                    System.IO.TextReader reader = new System.IO.StreamReader(src.ToString().Substring(8));
                    fileData = (SaveFormat)xmlSeries.Deserialize(reader);
                    reader.Close();
                    DrawingArea.Children.Clear();
                    DrawingArea.Children.Add(imag);
                    planDeTravail.Carte.ImageBuffer = fileData.ImageBuffer;
                    imag.Source = planDeTravail.Carte.imag;
                    this.unit = fileData.unit;
                    planDeTravail.Courbes.Clear();
                    planDeTravail.Courbes_prob.Clear();
                    planDeTravail.Segment = null;
                    planDeTravail.ClearInters();
                    planDeTravail.Ellipse_Labels.Clear();
                    planDeTravail.probems.Clear();
                    planDeTravail.e_probems.Clear();
                    planDeTravail.Carte.altMax = fileData.altMax;
                    planDeTravail.Carte.altMin = fileData.altMin;
                    planDeTravail.Carte.equidistance = fileData.equidistance;
                    planDeTravail.Carte.echelle = fileData.echelle;
                    for (int i = 0; i < fileData.ctrlPoints.Count; i++)
                    {
                        Courbe crb = new Courbe();
                        Polyline pol = new Polyline();
                        crb.p = pol;
                        crb.alt = fileData.alts[i];
                        crb.p.Points = fileData.ctrlPoints[i];
                        crb.SetInitialise(fileData.initialized[i]);
                        crb.Color = crb.CurveColor(planDeTravail.Carte.altMin, planDeTravail.Carte.altMax);
                        planDeTravail.Courbes.Add(crb);
                        crb.p.StrokeThickness = 3;
                        crb.p.Stroke = Brushes.Black;
                        ((Polyline)crb.p).Stroke = new SolidColorBrush(planDeTravail.GetCourbe((Polyline)crb.p).Color);
                        crb.p.Margin = new Thickness(0, 0, 0, 0); //Marge par défault //Pour bouger les courbes
                        DrawingArea.Children.Add(crb.p);
                    }

                    Line line = new Line
                    {
                        X1 = fileData.p1.X,
                        Y1 = fileData.p1.Y,
                        X2 = fileData.p2.X,
                        Y2 = fileData.p2.Y
                    };
                    planDeTravail.Segment = line;
                    planDeTravail.Segment.StrokeThickness = 2;
                    planDeTravail.Segment.Stroke = new SolidColorBrush(Color.FromRgb(7, 0, 18));
                    DrawingArea.Children.Add(planDeTravail.Segment);

                    echel.Text = planDeTravail.Carte.echelle.ToString();
                    equi.Text = planDeTravail.Carte.equidistance.ToString();
                    altMin.Text = planDeTravail.Carte.altMin.ToString();
                    altMax.Text = planDeTravail.Carte.altMax.ToString();
                    this.AssignColors();
                    this.Problems_Checked();
                    newPoly = null;
                    selectedPoint = null;
                    selectSegment = false;
                    selectedRect = null;
                    selectedPoly = null;
                    setdata = true;
                    altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    EnableAlt();
                    Undo_Redo.Save(this);
                }
                catch (Exception)
                {
                    // message can't load this file
                    // goes back to menu
                    MessageBox.Show("Format non supporté");
                }

            }
        }
        public void SaveThings()// enregistrer le contenu du plan de travail dans un fichier
        {
            SaveFileDialog open = new SaveFileDialog
            {
                Filter = "tpr files (*.tpr)|*.tpr",
                Title = "Sauvegarder un projet ToProfil",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ToProfil"
            };
            if (src_file != "")
            {
                open.FileName = src_file;
            }
            if (open.ShowDialog() == true)
            {
                var src = new Uri(open.FileName);


                List<PointCollection> ctrlPoints = new List<PointCollection>(); //ctrl points
                List<double> alts = new List<double>();               //Altitudes
                List<bool> initialized = new List<bool>();

                foreach (Courbe el in planDeTravail.Courbes)
                {

                    ctrlPoints.Add(el.p.Points);
                    alts.Add(el.alt);
                    initialized.Add(el.GetInitialise());

                }
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SaveFormat));
                    TextWriter writer = new StreamWriter(src.ToString().Substring(8));
                    serializer.Serialize(writer, new SaveFormat(ctrlPoints, alts, initialized, planDeTravail.Carte.altMin, planDeTravail.Carte.altMax, planDeTravail.Carte.equidistance,
                        planDeTravail.Carte.echelle, planDeTravail.Carte.imag, planDeTravail.Segment, this.unit));

                    writer.Close();
                }
                catch (Exception)
                {
                    // file being use can't open it
                }
            }
        }
        #endregion

        #region Altitude functions
        public void DisableAlt()
        {
            newPoly = null;
            altitude.Clear();
            alt_canva.Visibility = Visibility.Visible;
            altitude.BorderBrush = null;
            altitude.ToolTip = null;
            if (planDeTravail.GetCourbe(selectedPoly).GetInitialise() == false)
            {
                setdata = true;
                altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                altitude.Text = "";
                altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            else
            {
                setdata = true;
                altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                altitude.Text = planDeTravail.GetCourbe(selectedPoly).alt.ToString();
                altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

            DrawingArea.IsEnabled = false;
            Menu.IsEnabled = false;
            Courbe.IsEnabled = false;
            Segment.IsEnabled = false;
            Profile.IsEnabled = false;
            Inter.IsEnabled = false;
            Change.IsEnabled = false;
            Dis_en_ToolBox(false);
            menu_rec.Visibility = Visibility.Hidden;
            Undo_Redo.Save(this);
        }

        public void DisableAltWithoutSave()
        {
            setdata = true;
            altitude.Clear();
            alt_canva.Visibility = Visibility.Visible;
            altitude.BorderBrush = null;
            altitude.ToolTip = null;
            selectedPoly.Stroke = Brushes.Black;
            if (planDeTravail.GetCourbe(selectedPoly).GetInitialise() == false)
            {
                setdata = true;
                altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                altitude.Text = "";
                altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            else
            {
                setdata = true;
                altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                altitude.Text = planDeTravail.GetCourbe(selectedPoly).alt.ToString();
                altitude.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            DrawingArea.IsEnabled = false;
            Menu.IsEnabled = false;
            Courbe.IsEnabled = false;
            Segment.IsEnabled = false;
            Profile.IsEnabled = false;
            Inter.IsEnabled = false;
            Change.IsEnabled = false;
            Dis_en_ToolBox(false);
            menu_rec.Visibility = Visibility.Hidden;
        }

        private void Altitude_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (setdata == false)
            {
                RetrieveAlt();
            }
        }

        public void RetrieveAlt()
        {
            try
            {
                double.Parse(altitude.Text);
                if (altitude.Text == "")
                {
                    altitude.BorderBrush = Brushes.Red;
                    altitude.ToolTip = "Veuillez remplire ce champs";
                }
                else if ((double.Parse(altitude.Text) > planDeTravail.Carte.altMax) || (double.Parse(altitude.Text) < planDeTravail.Carte.altMin) || ((double.Parse(altitude.Text) % planDeTravail.Carte.equidistance != 0)))
                {
                    altitude.BorderBrush = Brushes.Red;
                    altitude.ToolTip = "Veuillez introduire une altitude valide";
                    planDeTravail.GetCourbe(selectedPoly).SetAlt(double.Parse(altitude.Text));
                    planDeTravail.GetCourbe(selectedPoly).Initialise();
                    DrawingArea.IsEnabled = true;
                    Menu.IsEnabled = true;
                    Courbe.IsEnabled = true;
                    Segment.IsEnabled = true;
                    Profile.IsEnabled = true;
                    Inter.IsEnabled = true;
                    Outils.IsEnabled = true;
                    Change.IsEnabled = true;
                    tool_box.IsEnabled = true;
                    Dis_en_ToolBox(true);
                    if (inter_seg == true)
                    {
                        Intersection();
                    }
                }
                else
                {
                    altitude.BorderBrush = null;
                    altitude.ToolTip = null;
                    planDeTravail.GetCourbe(selectedPoly).SetAlt(double.Parse(altitude.Text));
                    planDeTravail.GetCourbe(selectedPoly).Initialise();
                    DrawingArea.IsEnabled = true;
                    Menu.IsEnabled = true;
                    Courbe.IsEnabled = true;
                    Segment.IsEnabled = true;
                    Profile.IsEnabled = true;
                    Inter.IsEnabled = true;
                    Outils.IsEnabled = true;
                    Change.IsEnabled = true;
                    tool_box.IsEnabled = true;
                    Dis_en_ToolBox(true);
                    if (inter_seg == true)
                    {
                        Intersection();
                    }
                }
            }
            catch
            {
                altitude.BorderBrush = Brushes.Red;
                altitude.ToolTip = "Veuillez introduire une altitude valide";
            }
            AssignColors();
            Undo_Redo.Save(this);
        }

        private void Altitude_GotFocus(object sender, RoutedEventArgs e)
        {
            setdata = false;
        }

        private void Altitude_LostFocus(object sender, RoutedEventArgs e)
        {
            setdata = true;
        }

        private void alt_canva_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            alt_canva.Focus();
        }

        public void EnableAlt() //Redonner la main 
        {
            setdata = true;
            altitude.BorderBrush = null;
            altitude.ToolTip = null;
            DrawingArea.IsEnabled = true;
            Menu.IsEnabled = true;
            Courbe.IsEnabled = true;
            Segment.IsEnabled = true;
            Profile.IsEnabled = true;
            Inter.IsEnabled = true;
            Outils.IsEnabled = true;
            Change.IsEnabled = true;
            tool_box.IsEnabled = true;
            Dis_en_ToolBox(true);
            alt_canva.Visibility = Visibility.Hidden;
            setdata = true;
        }

        public void RedonnerMain()
        {
            DrawingArea.IsEnabled = true;
            Menu.IsEnabled = true;
            Courbe.IsEnabled = true;
            Segment.IsEnabled = true;
            Profile.IsEnabled = true;
            Inter.IsEnabled = true;
            Outils.IsEnabled = true;
            Change.IsEnabled = true;
            tool_box.IsEnabled = true;
            Dis_en_ToolBox(true);
        }
        #endregion

        #region hiding/disabeling
        public void Dis_en_ToolBox(bool boole)
        {
            container.IsEnabled = boole;
        }

        public void DisableAll()
        {
            if (courbe_enabled)
            {
                courbe_enabled = false;
                Color_press(Courbe, 1);
            }
            if (segment_enabled)
            {
                segment_enabled = false;
                Color_press(Segment, 1);
            }
            if (ancrage_enabled)
            {
                Color_press(ancretage, 3.75);
                ancrage_enabled = false;
            }

            if (newPoly != null)
            {
                newPoly.Points.Remove(newPoly.Points[newPoly.Points.Count - 1]);
                if (newPoly.Points.Count >= 2)
                {
                    Courbe courbe = new Courbe(newPoly);
                    planDeTravail.AddCourbe(courbe);
                }
                else { selectedPoly = null; }
                newPoly = null;
            }
            if (drawSegment)
            {
                if (planDeTravail.Segment != null)
                {
                    SupprimerPointsSegment();
                    selectSegment = false;
                    DrawingArea.Children.Remove(planDeTravail.Segment);
                    planDeTravail.Segment = null;
                    drawSegment = false;
                }
            }

        }

        public void Disable_boxs()
        {
            erase.IsEnabled = false;
            ancretage.IsEnabled = false;
            copy.IsEnabled = false;
            cut.IsEnabled = false;
            if (mode == "clair")
            {
                ancretage.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
            }
            else
            {
                ancretage.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
            }
        }

        public void Hide_toolbar()
        {
            tool_box.Visibility = Visibility.Hidden;
            zoom_slider.Visibility = Visibility.Hidden;
            if (barre.Fill.ToString() == "#FF333366")//light mode
            {
                Outils.Background = new SolidColorBrush(Color.FromRgb(51, 51, 102));
            }
            else //dark mode
            {
                Outils.Background = new SolidColorBrush(Color.FromRgb(31, 71, 88));
            }
            o_t.Source = new BitmapImage(new Uri("tools2.png", UriKind.Relative));
        }
        #endregion

        #region errors
        public void ErrorMsg_Set()
        {
            if (mode == "clair")
            {
                errorMsg_canvas.Background = new SolidColorBrush(Color.FromRgb(253, 192, 199));
                errorMsg.Foreground = new SolidColorBrush(Color.FromRgb(192, 39, 57));
            }
            else
            {
                errorMsg_canvas.Background = new SolidColorBrush(Color.FromRgb(166, 208, 214));
                errorMsg.Foreground = new SolidColorBrush(Color.FromRgb(31, 71, 88));
            }
            errorMsg_canvas.Visibility = Visibility.Visible;
            String error = "Impossible de générer le profil Veulliez régler les problems suivants:\n";
            errorMsg.Height = 43;
            List<Point> inters_itself = new List<Point>();
            List<Point> inters_courbe_courbes = new List<Point>();
            List<Courbe> listecourbes = planDeTravail.Getcourbes();
            foreach (Courbe c in listecourbes)
            {
                c.IntersectionCourbeItself(ref inters_itself);
                c.IntersectionCoubreCourbes(ref inters_courbe_courbes, listecourbes);
            }

            if (planDeTravail.Courbes_prob.Count != 0)
            {
                error += "-Les altitudes des courbes suivantes doivent respecter les paramétres de la carte : ";
                int i = 0;
                foreach (Courbe c in listecourbes)
                {
                    if (planDeTravail.Courbes_prob.Contains(c))
                    {
                        error += "N°" + planDeTravail.GetI(c) + " ";
                        if (i < planDeTravail.Courbes_prob.Count - 1)
                        {
                            error += ", ";
                        }
                        i++;
                    }
                }
                errorMsg.Height += 6;
                error += ".\n";
            }
            if (inters_courbe_courbes.Count != 0)
            {
                error += "-Les courbes ne doivent pas s'intersecter avec d'autres courbes .\n";
                errorMsg.Height += 6;
            }
            if (inters_itself.Count != 0)
            {
                error += "-Les courbes suivantes ne doivent pas s'intersecter avec elles memes : ";
                List<int> l = new List<int>();
                int i = 0;
                foreach (Point p in inters_itself)
                {
                    foreach (Courbe c in listecourbes)
                    {
                        if (c.Contains_point(p))
                        {
                            l.Add(planDeTravail.GetI(c));

                        }

                    }

                }
                l = l.Distinct().ToList<int>();
                foreach (int k in l)
                {
                    error += "N°" + k + " ";
                    if (i < l.Count - 1)
                    {
                        error += ", ";
                    }
                    i++;
                }

                errorMsg.Height += 6;
                error += ".\n";

            }
            errorMsg.Text = error;
            if (errorMsg.Height >= errorMsg_canvas.Height - 5)
            { errorMsg_canvas.Height = errorMsg.Height + 15; }

        }

        public void ErrorMsg_canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            errorMsg_canvas.Visibility = Visibility.Hidden;
        }

        public void ErrorMsg_canvas_MouseEnter(object sender, MouseEventArgs e)
        {

        }
        #endregion



    }
}
