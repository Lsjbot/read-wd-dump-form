using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace read_wd_dump_form
{
    class domaintableclass
    {
        public Dictionary<int, string> langnames = new Dictionary<int, string>();
        public Dictionary<string, int> langcolumns = new Dictionary<string, int>();
        public Dictionary<int, int> langstat = new Dictionary<int, int>();
        public Dictionary<int, int> conceptstat = new Dictionary<int, int>();
        public Dictionary<int, string> conceptnames = new Dictionary<int, string>();
        public Dictionary<int, int> conceptq = new Dictionary<int, int>();
        public double[][] data;
        private Dictionary<int, double[]> conceptdict = new Dictionary<int, double[]>();
        public int maxdim = -1;
        //public Dictionary<string, double> normdict = new Dictionary<string, double>();

        public void merge(domaintableclass dc2)
        {
            Dictionary<int, int> crosslang12 = new Dictionary<int, int>();
            Dictionary<int, int> crosslang21 = new Dictionary<int, int>();

            int colmax = this.langnames.Keys.Max();
            foreach (string lang2 in dc2.langcolumns.Keys)
            {
                if ( !this.langcolumns.ContainsKey(lang2))
                {
                    colmax++;
                    this.langcolumns.Add(lang2, colmax);
                    this.langnames.Add(colmax, lang2);
                    this.langstat.Add(colmax, 0);
                }
                crosslang12.Add(this.langcolumns[lang2], dc2.langcolumns[lang2]);
                crosslang21.Add(dc2.langcolumns[lang2], this.langcolumns[lang2]);
            }

            int icmax = this.conceptdict.Keys.Max();
            foreach (int ic in dc2.conceptdict.Keys)
            {
                icmax++;
                this.conceptnames.Add(icmax, dc2.conceptnames[ic]);
                this.conceptstat.Add(icmax, 0);
                this.conceptq.Add(icmax, dc2.conceptq[ic]);
                this.conceptdict.Add(icmax, dc2.conceptdict[ic]);
            }

            this.data = makedata();
        }

        public double[][] makedata()
        {
            double[][] newdata = new double[langnames.Count][];

            List<int> dummy = langstat.Keys.ToList();
            foreach (int il in dummy)
                langstat[il] = 0;
            dummy = conceptstat.Keys.ToList();
            foreach (int ic in dummy)
                conceptstat[ic] = 0;

            foreach (int ilang in langnames.Keys)
            {
                newdata[ilang] = new double[conceptdict.Count];
                foreach (int ic in conceptdict.Keys)
                {
                    newdata[ilang][ic] = conceptdict[ic][ilang];
                    if (newdata[ilang][ic] == 1)
                    {
                        langstat[ilang]++;
                        conceptstat[ic]++;
                    }
                }

            }
            return newdata;
        }

        public bool readnamedfile(string fn)
        {
            return readnamedfile(fn, false);
        }

        public bool readnamedfile(string fn, bool normalize)
        {
            if (!File.Exists(fn))
            {
                return false;
            }

            using (StreamReader sr = new StreamReader(fn))
            {
                string header = sr.ReadLine();
                string[] hwords = header.Split('\t');
                int offset = 5;

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hwords.Length - offset; i++)
                {
                    //if (normdict.ContainsKey(hwords[i + offset]))
                    {
                        //normindex.Add(i, langindex[hwords[i + offset]]);
                        sb.Append("\t" + hwords[i + offset]);
                        langnames.Add(i, hwords[i + offset]);
                        langcolumns.Add(hwords[i + offset], i);
                        langstat.Add(i, 0);
                    }
                }
                Console.WriteLine(sb.ToString());

                int nline = 0;

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    string[] words = line.Split('\t');

                    double[] dd = new double[words.Length - offset];
                    for (int j = 0; j < dd.Length; j++)
                        dd[j] = 0;
                    conceptnames.Add(nline, words[1]);
                    conceptstat.Add(nline, 0);
                    conceptq.Add(nline, util.qtoint(words[0]));

                    for (int i = 0; i < words.Length - offset; i++)
                    {
                        if (!langnames.ContainsKey(i))
                            continue;
                        dd[i] = util.tryconvert0(words[i + offset]);
                        if (normalize && dd[i] == 0 && Form1.coverdict.ContainsKey(langnames[i]))
                        {
                            dd[i] = (double)1 - Form1.coverdict[langnames[i]];
                        }
                    }

                    conceptdict.Add(nline, dd);

                    nline++;
                    if (nline % 100 == 0)
                        Console.WriteLine("nline = " + nline);

                    if (maxdim > 0)
                        if (nline > maxdim)
                            break;
                }
            }

            data = makedata();


            return true;
        }
        
        public string readfile(String folder)
        {
            return readfile(folder, false);
        }

        public string readfile(string folder, bool normalize)
        {
            return readfile(folder, "Category language table", normalize);
        }

        public string readfile(string folder, string prompt, bool normalize)
        {
            //if (normalize)
            //{
            //    OpenFileDialog odn = new OpenFileDialog();
            //    odn.InitialDirectory = folder;
            //    odn.Title = "Normcount";
            //    if (odn.ShowDialog() == DialogResult.OK)
            //    {
            //        string fnn = odn.FileName;
            //        using (StreamReader sr = new StreamReader(fnn))
            //        {
            //            while (!sr.EndOfStream)
            //            {
            //                string line = sr.ReadLine();
            //                string[] words = line.Split('\t');
            //                if (words.Length < 2)
            //                    continue;

            //                normdict.Add(words[0], util.tryconvert0(words[1]));
            //            }
            //        }
            //        double total = normdict["total"];
            //        List<string> dummy = normdict.Keys.ToList();
            //        foreach (string s in dummy)
            //        {
            //            if (s != "total")
            //            {
            //                normdict[s] = normdict[s] / total;
            //                Console.WriteLine(s.PadRight(10) + normdict[s]);
            //            }
            //        }
            //    }
            //}

            OpenFileDialog od = new OpenFileDialog();
            od.InitialDirectory = folder;
            od.Title = prompt;
            if (od.ShowDialog() == DialogResult.OK)
            {
                string fn = od.FileName;
                readnamedfile(fn, normalize);
                return fn;
            }
            else
                return null;
                
        }

    }
}
