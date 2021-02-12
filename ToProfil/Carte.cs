using System.IO;
using System.Windows.Media.Imaging;

namespace ToProfil
{
    public class Carte
    {
        public BitmapSource imag;
        public double altMin, altMax, equidistance, echelle;
        public const double max = 200;//km 
        public bool Km = false;
        public bool m = true;
        public Carte() { }
        #region setters/getters
        public double AltMin   // property
        {
            get { return altMin; }   // get method
            set { altMin = value; }  // set method
        }
        public double AltMax   // property
        {
            get { return altMax; }   // get method
            set { altMax = value; }  // set method
        }
        public double Equidistance   // property
        {
            get { return equidistance; }   // get method
            set { equidistance = value; }  // set method
        }
        public double Echelle   // property
        {
            get { return echelle; }   // get method
            set { echelle = value; }  // set method
        }
        public byte[] ImageBuffer
        {
            get
            {
                byte[] imageBuffer = null;

                if (imag != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(imag));
                        encoder.Save(stream);
                        imageBuffer = stream.ToArray();
                    }
                }

                return imageBuffer;
            }
            set
            {
                if (value == null)
                {
                    imag = null;
                }
                else
                {
                    using (var stream = new MemoryStream(value))
                    {
                        var decoder = BitmapDecoder.Create(stream,
                            BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        imag = decoder.Frames[0];
                    }
                }
            }
        }
        public double Getmax()
        {
            return max;
        }
        #endregion

        public double ToKm_m(double x)
        {
            return x * echelle;
        } // Convertir vers la distance réelle
        public double ToCm(double x)
        {
            return x / echelle;
        } // Convertire vers al distance sur la carte
        public Carte Duplicate()
        {
            Carte carte = new Carte();

            if (imag != null)
            { carte.imag = this.imag.Clone(); }
            else carte.imag = null;
            carte.echelle = this.echelle;
            carte.equidistance = this.equidistance;
            carte.altMin = this.altMin;
            carte.altMax = this.altMax;
            return carte;
        } // Dupliquer la carte 
    }
}
