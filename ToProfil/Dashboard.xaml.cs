using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ToProfil
{
    public partial class Dashboard : Page
    {
        #region Data
        public string mode = "clair";
        #endregion

        #region Methdoes 
        public Dashboard(string mode)
        {
            this.mode = mode;
            InitializeComponent();
            SetMode();

        }

        public void Nouveau_Click(object sender, RoutedEventArgs e)
        {
            //Ouvrir un nouveau projet
            PlanDeTravail planDeTravail = new PlanDeTravail();
            Plan plan = new Plan(this.mode, planDeTravail);
            this.NavigationService.Navigate(plan);

        }

        public void Ouvrir_Click(object sender, RoutedEventArgs e)
        {
            // importer un projet
            PlanDeTravail planDeTravail = new PlanDeTravail();
            Plan plan = new Plan(this.mode, planDeTravail);
            plan.LoadThings();
            this.NavigationService.Navigate(plan);
        }

        public void Aide_Click(object sender, RoutedEventArgs e)
        {
            //ouvrir l'aide en ligne

        }
        #endregion

        #region Modification du mode d'affichage
        public void Mode_Switch_Click(object sender, RoutedEventArgs e)
        {
            //changer de mode 

            if (midd.Source.ToString() == "pack://application:,,,/ToProfil;component/Dash.png")
            {
                //mode sombre
                mode = "sombre";
            }
            else
            {
                //mode clair
                mode = "clair";
            }
            SetMode();
        }
        public void SetMode()
        {
            if (mode == "sombre")
            {
                //mode sombre
                midd.Source = new BitmapImage(new Uri("Dash2.png", UriKind.Relative));
                backg.Background = new LinearGradientBrush(Color.FromRgb(102, 100, 101), Color.FromRgb(25, 25, 25), new Point(0, 1), new Point(1, 0));
                courbes.Source = new BitmapImage(new Uri("Fichier 5.png", UriKind.Relative));
                Nouveau.Background = new SolidColorBrush(Color.FromRgb(39, 59, 67));
                Ouvrir.Background = new SolidColorBrush(Color.FromRgb(39, 59, 67));
                Mode_Switch.Background = new SolidColorBrush(Color.FromRgb(14, 230, 229));
                RotateTransform rotateTransform1 = new RotateTransform(360, 0, 0);
                Mode_Switch.RenderTransform = rotateTransform1;
                Mode_Switch.IsChecked = true;
                Mode_Switch.ToolTip = "Mode clair";
            }
            else
            {
                //mode clair
                midd.Source = new BitmapImage(new Uri("Dash.png", UriKind.Relative));
                backg.Background = new LinearGradientBrush(Color.FromRgb(255, 255, 255), Color.FromRgb(245, 247, 251), new Point(0, 1), new Point(1, 0));
                courbes.Source = new BitmapImage(new Uri("Fichier 4.png", UriKind.Relative));
                Nouveau.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                Ouvrir.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                Mode_Switch.Background = new SolidColorBrush(Color.FromRgb(192, 39, 57));
                RotateTransform rotateTransform1 = new RotateTransform(180, 0, 0);
                Mode_Switch.RenderTransform = rotateTransform1;
                Mode_Switch.IsChecked = true;
                Mode_Switch.ToolTip = "Mode Sombre";
            }
        }
        #endregion
    }
}
