using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace ToProfil
{
    [Serializable()]
    public class SaveFormat : ISerializable
    {
        // the class to be serialized by wpf 
        public List<PointCollection> ctrlPoints = new List<PointCollection>(); //ctrl points
        public List<double> alts = new List<double>();               //Altitudes
        public List<bool> initialized = new List<bool>(); //is initialized
        public double altMin, altMax, equidistance, echelle;
        public Point p1, p2 = new Point();
        public bool unit;

        [XmlIgnore]
        public BitmapSource imag;
        [XmlElement("imag")]
        public byte[] ImageBuffer
        {
            get
            {
                byte[] imageBuffer = null;

                if (imag != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        var encoder = new PngBitmapEncoder(); // or some other encoder
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
        public SaveFormat()
        { }
        public SaveFormat(List<PointCollection> l1, List<double> l4, List<bool> initialized, double altMin, double altMax, double equidistance, double echelle, BitmapSource imag, Line seg, bool unit)
        {

            this.ctrlPoints = l1;
            this.alts = l4;
            this.initialized = initialized;
            this.altMax = altMax;
            this.altMin = altMin;
            this.equidistance = equidistance;
            this.echelle = echelle;
            this.imag = imag;
            this.unit = unit;
            if (seg != null)
            {
                this.p1.X = seg.X1;
                this.p1.Y = seg.Y1;
                this.p2.X = seg.X2;
                this.p2.Y = seg.Y2;
            }
        }
        public SaveFormat(SerializationInfo info, StreamingContext context)
        {


            ctrlPoints = (List<PointCollection>)info.GetValue("ctrlPoints", typeof(List<PointCollection>));
            alts = (List<double>)info.GetValue("alts", typeof(List<double>));
            initialized = (List<bool>)info.GetValue("initialized", typeof(List<bool>));
            altMax = (double)info.GetValue("altMax", typeof(double));
            altMin = (double)info.GetValue("altMin", typeof(double));

            equidistance = (double)info.GetValue("equidistance", typeof(double));
            echelle = (double)info.GetValue("echelle", typeof(double));
            imag = (BitmapSource)info.GetValue("imag", typeof(BitmapSource));

            p1 = (Point)info.GetValue("p1", typeof(Point));
            p2 = (Point)info.GetValue("p2", typeof(Point));
            unit = (bool)info.GetValue("unit", typeof(bool));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {


            info.AddValue("ctrlPoints", ctrlPoints);
            info.AddValue("inatialized", initialized);
            info.AddValue("alts", alts);
            info.AddValue("altMax", altMax);
            info.AddValue("altMin", altMin);
            info.AddValue("equidistance", equidistance);
            info.AddValue("echelle", echelle);
            info.AddValue("imag", imag);
            info.AddValue("p1", p1);
            info.AddValue("p2", p2);
            info.AddValue("unit", unit);
        }
    }
}
