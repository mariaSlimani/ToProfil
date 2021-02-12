using System.Collections.Generic;

namespace ToProfil
{
    public class Cmp_pointa_x : IComparer<Pointa> //Ordonner les points selon leur position
    {
        public int Compare(Pointa pa, Pointa pb)
        {
            return pa.p.X.CompareTo(pb.p.X);
        }
    }
    public class Cmp_pointa_x_inverse : IComparer<Pointa> // Ordonner les points selon leur position en sens inverse
    {
        public int Compare(Pointa pa, Pointa pb)
        {
            return pb.p.X.CompareTo(pa.p.X);
        }
    }
}
