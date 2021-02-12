using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ToProfil
{
    public partial class ChartTooltip : IChartTooltip
    {
        private TooltipData _data;

        public ChartTooltip(Carte ech, String mode)
        {
            InitializeComponent();

            DataTemplate datatmp = new DataTemplate
            {
                DataType = typeof(DataPointViewModel)
            };
            FrameworkElementFactory grid = new FrameworkElementFactory(typeof(StackPanel));
            grid.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
            datatmp.VisualTree = grid;


            FrameworkElementFactory textalt = new FrameworkElementFactory(typeof(TextBlock));
            FrameworkElementFactory textDistance = new FrameworkElementFactory(typeof(TextBlock));

            Binding X = new Binding("ChartPoint.Instance.X");
            if (ech.Km)
                X.StringFormat = "Distance : {0} km";
            else
                X.StringFormat = "Distance : {0} m";


            Binding Y = new Binding("ChartPoint.Instance.Y")
            {
                StringFormat = "Altitude : {0} m"
            };

            textalt.SetBinding(TextBlock.TextProperty, Y);
            textDistance.SetBinding(TextBlock.TextProperty, X);

            //************Change Colors
            if (mode == "sombre")
            {
                this.Background = new SolidColorBrush(Color.FromArgb(230, 135, 135, 135));
                this.BorderBrush = null;
                textalt.SetValue(TextBlock.ForegroundProperty, Foreground = new SolidColorBrush(Color.FromRgb(14, 230, 229)));
                textDistance.SetValue(TextBlock.ForegroundProperty, Foreground = new SolidColorBrush(Color.FromRgb(31, 71, 88)));
            }
            else
            {
                this.Background = new SolidColorBrush(Color.FromArgb(230, 255, 255, 255));
                this.BorderBrush = null;
                textalt.SetValue(TextBlock.ForegroundProperty, Foreground = new SolidColorBrush(Color.FromRgb(192, 39, 57)));
                textDistance.SetValue(TextBlock.ForegroundProperty, Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 102)));
            }


            grid.AppendChild(textDistance);
            grid.AppendChild(textalt);
            temctrl.ItemTemplate = datatmp;
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TooltipData Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged("Data");
            }
        }

        public TooltipSelectionMode? SelectionMode { get; set; }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
