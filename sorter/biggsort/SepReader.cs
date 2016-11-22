using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace bigsortns
{
    class SepReader
    {
        private int fcount;

        private StreamReader[] srar;
        private SortedSet<bsdata> sset;

        public SepReader(int aFCount)
        {
            fcount = aFCount;
            srar = new StreamReader[aFCount];
            sset = new SortedSet<bsdata>();
            string ss = "";
            for (int i = 0; i < aFCount; i++)
            {
                srar[i] = new StreamReader("bsort" + i.ToString());
                ss = srar[i].ReadLine();
                if (!String.IsNullOrEmpty(ss))
                {
                    sset.Add(new bsdata(ss, i));
                }
                else
                {
                    srar[i] = null;
                }
            }
        }

        public bsdata getNext()
        {
            if (sset.Count() == 0)
            {
                return null;
            }
            else
            {
                bsdata bs = sset.Min();
                sset.Remove(bs);
                if (srar[bs.nf] != null)
                {
                    string s = srar[bs.nf].ReadLine();
                    if (!String.IsNullOrEmpty(s))
                    {
                        sset.Add(new bsdata(s, bs.nf));
                    }
                    else
                    {
                        srar[bs.nf].Close();
                        srar[bs.nf] = null;
                    }
                }
                return bs;
            }
        }
    }
}
