using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ToProfil
{
    public partial class MainWindow : Window
    {
        public string leMode;
        public bool redButton = true;

        public MainWindow()
        {
            InitializeComponent();
            main.NavigationService.Navigate(new Dashboard("clair"));
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Delete) && (main.Content.ToString() == "ToProfil.Plan") && (((Plan)main.Content).erase.IsEnabled))//&&(((Plan)main.Content).altitude.IsFocused==false)) //supprimer la courbe selectionnée
            {
                ((Plan)main.Content).Erase_Click(new object(), new RoutedEventArgs()); //appelle la fonction erase
            }
            if ((Keyboard.IsKeyDown(Key.RightCtrl)) || (Keyboard.IsKeyDown(Key.LeftCtrl))) //ctrl is pressed
            {
                if (main.Content.ToString() == "ToProfil.Plan") //page== PLAN
                {
                    switch (e.Key)
                    {
                        case Key.C: //copy ctrl+c
                            ((Plan)main.Content).Copy_Click(new object(), new RoutedEventArgs()); //appelle la fonction copy
                            break;
                        case Key.X: //copy ctrl+x
                            ((Plan)main.Content).Cut_Click(new object(), new RoutedEventArgs()); //appelle la fonction copy
                            break;
                        case Key.V: //past ctrl+v
                            ((Plan)main.Content).Paste_Click(new object(), new RoutedEventArgs()); //appelle la fonction paste
                            break;

                        case Key.Z: //undo ctrl+z
                            ((Plan)main.Content).Back_Click(new object(), new RoutedEventArgs()); //appelle la fonction back
                            break;

                        case Key.Y: //redo ctrl+y
                            ((Plan)main.Content).Forward_Click(new object(), new RoutedEventArgs()); //appelle la fonction forward
                            break;


                        case Key.A: //ancretage ctrl+a
                            ((Plan)main.Content).Ancretage_Click(new object(), new RoutedEventArgs()); //appelle la fonction bancretage
                            break;

                        case Key.P: //generer le profile ctrl+p
                            ((Plan)main.Content).Profile_Click(new object(), new RoutedEventArgs()); //appelle la fonction Profil
                            break;

                        case Key.D: //outil courbe ctrl+d
                            ((Plan)main.Content).Courbe_Click(new object(), new RoutedEventArgs()); //appelle la fonction courbe
                            break;

                        case Key.L: //outil segment ctrl+l
                            ((Plan)main.Content).Segment_Click(new object(), new RoutedEventArgs()); //appelle la fonction segment 
                            break;

                        case Key.I: //afficher les points d'intersection
                            ((Plan)main.Content).Inter_Click(new object(), new RoutedEventArgs()); //appelle la fonction inter
                            break;

                        case Key.T: //importation d'une carte ctrl+t
                            ((Plan)main.Content).Importimage_Click(new object(), new RoutedEventArgs()); //appelle la fonction importer
                            break;
                        case Key.S: //save ctrl + s
                            ((Plan)main.Content).Sauvegarde_Click(new object(), new RoutedEventArgs()); //appelle la fonction sauvegarde
                            break;

                        case Key.M: //home ctrl+m
                            ((Plan)main.Content).Home_Click(new object(), new RoutedEventArgs()); //appelle la fonction home
                            break;
                        //zoom functions
                        case Key.Add: // zoom in ctrl+ +
                            ((Plan)main.Content).ZoomIn(); //appelle la fonction zoomin
                            break;
                        case Key.Subtract: // zoom in ctrl+ -
                            ((Plan)main.Content).ZoomOut(); //appelle la fonction zoomout
                            break;
                        case (Key.N): // nouveau ctrl+N
                            ((Plan)main.Content).Nouv_Click(new object(), new RoutedEventArgs()); //appelle la fonction nouveau
                            break;
                        case (Key.O): // ouvrir ctrl+O
                            ((Plan)main.Content).Ouvrir_Click(new object(), new RoutedEventArgs()); //appelle la fonction ouvrir
                            break;
                        case (Key.H): // aide enligne ctrl+H
                            ((Plan)main.Content).Aide_Click(new object(), new RoutedEventArgs()); //appelle la fonction aide
                            break;
                    }
                }
                else if (main.Content.ToString() == "ToProfil.Profile_generation") // page = profile_generation
                {
                    switch (e.Key)
                    {
                        case Key.S: //save ctrl + s
                            ((Profile_generation)main.Content).Sauvegarde_Click(new object(), new RoutedEventArgs()); //appelle la fonction sauvegarde
                            break;

                        case Key.M: //home ctrl+m
                            ((Profile_generation)main.Content).Home_Click(new object(), new RoutedEventArgs()); //appelle la fonction home
                            break;
                        //zoom functions
                        case Key.Add: // zoom in ctrl+ +
                            ((Profile_generation)main.Content).ZoomIn(); //appelle la fonction zoomin

                            break;
                        case Key.Subtract: // zoom in ctrl+ -
                            ((Profile_generation)main.Content).ZoomOut(); //appelle la fonction zoomout
                            break;
                        case (Key.N): // nouveau ctrl+N
                            ((Profile_generation)main.Content).Nouv_Click(new object(), new RoutedEventArgs()); //appelle la fonction nouveau
                            break;
                        case (Key.O): // ouvrir ctrl+O
                            ((Profile_generation)main.Content).Ouvrir_Click(new object(), new RoutedEventArgs()); //appelle la fonction ouvrir
                            break;
                        case (Key.H): // aide enligne ctrl+H
                            ((Profile_generation)main.Content).Aide_Click(new object(), new RoutedEventArgs()); //appelle la fonction aide
                            break;
                    }
                }
                else //dashboard
                {
                    switch (e.Key)
                    {
                        case (Key.N): // nouveau ctrl+N
                            ((Dashboard)main.Content).Nouveau_Click(new object(), new RoutedEventArgs()); //appelle la fonction nouveau
                            break;
                        case (Key.O): // ouvrir ctrl+O
                            ((Dashboard)main.Content).Ouvrir_Click(new object(), new RoutedEventArgs()); //appelle la fonction ouvrir
                            break;
                        case (Key.H): // aide enligne ctrl+H
                            ((Dashboard)main.Content).Aide_Click(new object(), new RoutedEventArgs()); //appelle la fonction aide
                            break;
                    }
                }


            }
        } //Raccourci clavier

        private void Main_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Forward)
            {
                e.Cancel = true;
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                e.Cancel = true;
            }
        } //disable navigation

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Undo_Redo.plans_undo.Count > 2)
            {
                if (redButton)
                {

                    if (main.Content.ToString() == "ToProfil.Plan")
                    {
                        ((Plan)main.Content).conf_canvas.Visibility = Visibility.Visible;
                        ((Plan)main.Content).act = "close";
                        e.Cancel = true;
                    }
                    else if (main.Content.ToString() == "ToProfil.Profile_generation")
                    {
                        ((Profile_generation)main.Content).conf_canvas.Visibility = Visibility.Visible;
                        ((Profile_generation)main.Content).act = "close";
                        e.Cancel = true;
                    }
                }
            }

        } //fermeture de l'application

    }

}


