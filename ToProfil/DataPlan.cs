using System.Windows;
using System.Windows.Shapes;

namespace ToProfil
{
    public class DataPlan 
    {
        #region Data
        public PlanDeTravail planDeTravail;
        public bool drawSegment = false;
        public bool selectSegment = false;
        public int selectedRect = -1;
        public bool dragSeg = false;
        public bool dragRect = false;
        public Polyline newPoly = null;
        public Point point = new Point();
        public Polyline selectedPoly = null;
        public int selectedPoint = -1;
        public bool dragPoly = false;
        public bool dragPoint = false;
        public Point click = new Point();
        public bool courbe_enabled = false, segment_enabled = false, ancrage_enabled = false;
        public string act;
        public bool inter_seg = false, inter_cc = false;
        public bool keyy = false, proceed = true;
        public bool conditiona = false, conditioni = false, conditionc = false, conditionq = false;
        public bool mainBloquee = false;
        public bool focus = false;
        public bool menuCarte = false;
        public string echelle = "", altmin = "", altmax = "", equ_distance = "", km_m = "";
        #endregion

        public DataPlan Duplicate()
        {
            DataPlan dataPlan = new DataPlan();
            dataPlan.focus = this.focus;
            dataPlan.menuCarte = this.menuCarte;
            dataPlan.altmax = this.altmax;
            dataPlan.altmin = this.altmin;
            dataPlan.echelle = this.echelle;
            dataPlan.equ_distance = this.equ_distance;
            dataPlan.km_m = this.km_m;

            dataPlan.drawSegment = this.drawSegment;
            dataPlan.selectSegment = this.selectSegment;
            dataPlan.dragSeg = this.dragSeg;
            dataPlan.dragRect = this.dragRect;
            dataPlan.dragPoly = this.dragPoly;
            dataPlan.dragPoint = this.dragPoint;
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
                dataPlan.newPoly = new Polyline(); //dupliquer new poly
                dataPlan.newPoly.Stroke = this.newPoly.Stroke;
                dataPlan.newPoly.StrokeThickness = this.newPoly.StrokeThickness;
                foreach (Point pt in this.newPoly.Points)
                {
                    Point point = new Point(pt.X, pt.Y);
                    dataPlan.newPoly.Points.Add(pt);
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
            }
            else if (this.selectedPoly != null && this.selectedPoly == this.newPoly)
            {
                dataPlan.selectedPoly = dataPlan.newPoly;
            }
            else
            {
                dataPlan.selectedPoly = null;
            }

            dataPlan.selectedPoint = this.selectedPoint;
            dataPlan.selectedRect = this.selectedRect;
            dataPlan.mainBloquee = this.mainBloquee;
            return dataPlan;
        }
    }
}
