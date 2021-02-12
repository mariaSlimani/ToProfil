using System.Windows;

namespace ToProfil
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow window = new MainWindow();
            PlanDeTravail planDeTravail = new PlanDeTravail();
            Plan plan = new Plan("clair", planDeTravail);
            window.Show();

            if (e.Args.Length != 0)
            {
                string a = e.Args[0].Substring(e.Args[0].Length - 4);
                if (a == ".tpr")
                {
                    plan.OuvrirDoubleClick(e.Args[0]);
                }
                else
                {
                    //msg d'erreur
                    MessageBox.Show("Impossible d'ouvrir ce fichier car ce n'est pas un Projet ToProfil");

                }
                window.main.NavigationService.Navigate(plan);
            }
        }
    }
}
