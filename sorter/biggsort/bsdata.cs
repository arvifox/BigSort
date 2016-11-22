using System;

namespace bigsortns
{
    public class bsdata : IComparable<bsdata>
    {
        public UInt32 v { set; get; }
        public string s { set; get; }
        public int nf { set; get; }

        public bsdata() { }
        public bsdata(string af, int anf)
        {
            int po = af.IndexOf(". ");
            v = UInt32.Parse(af.Substring(0, po));
            s = af.Substring(po + 2, af.Length - po - 2);
            nf = anf;
        }

        public int CompareTo(bsdata obj)
        {
            int stres = s.CompareTo(obj.s);
            if (stres > 0)
            {
                return 1;
            }
            else if (stres < 0)
            {
                return -1;
            }
            else
            {
                if (v > obj.v)
                {
                    return 1;
                } else if (v < obj.v)
                {
                    return -1;
                } else if (nf == obj.nf)
                {
                    return 0;
                } else if (nf > obj.nf)
                {
                    return 1;
                } else
                {
                    return -1;
                }
            }
        }

        public override string ToString()
        {
            return v.ToString() + ". " + s;
        }
    }
}
