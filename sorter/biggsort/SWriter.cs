using System.IO;
using System.Threading;

namespace bigsortns
{
    class SWriter
    {
        private StreamWriter sw;
        private bsdata mbsdata;
        private SepReader sepr;

        public SWriter(string fname, SepReader asepr)
        {
            sw = new StreamWriter(fname);
            sepr = asepr;
        }

        public void DoIt()
        {
            while ((mbsdata = sepr.getNext()) != null)
            {
                sw.WriteLine(mbsdata.ToString());
            }
            sw.Flush();
            sw.Close();
        }
    }
}
