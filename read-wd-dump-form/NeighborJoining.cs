using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;

namespace PrepositionsClusters
{
    public class NeighborJoining
    {
        Dictionary<int, nodeclass> nodedict = new Dictionary<int, nodeclass>();
        int nnode = 0;

        public class nodeclass
        {
            public string nodelabel;
            public nodeclass branch1 = null;
            public nodeclass branch2 = null;
            public float[] vector;
            public bool active = true;
            public string ph = "";
            public static CultureInfo culture_en = CultureInfo.CreateSpecificCulture("en-US");


            public nodeclass(string label, float[] vectorpar)
            {
                nodelabel = label;
                vector = vectorpar;
                ph = label;
            }

            public static nodeclass merge(nodeclass b1,nodeclass b2)
            {
                nodeclass nc = new nodeclass(b1.nodelabel + "." + b2.nodelabel, vectormerge(b1.vector,b2.vector));
                nc.branch1 = b1;
                nc.branch2 = b2;
                float d1 = (float)0.000001 * vectdist(nc.vector, b1.vector);
                float d2 = (float)0.000001 * vectdist(nc.vector, b2.vector);
                nc.ph = "(" + b1.ph + ":" + d1.ToString(culture_en) + "," + b2.ph + ":" + d2.ToString(culture_en) + ")XX";
                Console.WriteLine("Merging "+nc.nodelabel);
                return nc;
            }

            public static nodeclass merge(nodeclass b1, nodeclass b2, double dist, double scalebase)
            {
                nodeclass nc = new nodeclass(b1.nodelabel + "." + b2.nodelabel, vectormerge(b1.vector, b2.vector));
                nc.branch1 = b1;
                nc.branch2 = b2;
                double scaleddist = 0;
                double logdist = 0; 
                if (dist > 0)
                {
                    logdist = Math.Log10(dist);
                    scaleddist = logdist - scalebase;
                }
                nc.ph = "(" + b1.ph + ":" + scaleddist.ToString(culture_en) + "," + b2.ph + ":" + scaleddist.ToString(culture_en) + ")XX";
                Console.WriteLine("Merging " + nc.nodelabel + " " + dist + " "+logdist + " " + scaleddist + " " + scalebase);
                return nc;
            }

            static float[] vectormerge(float[] v1, float[] v2)
            {
                if (v1 == null || v2 == null)
                    return null;

                float[] vv = new float[v1.Length];
                for (int i=0;i<v1.Length;i++)
                {
                    vv[i] = (float)0.5 * (v1[i] + v2[i]);
                }
                return vv;
            }

            public static float vectdist(float[] vect1, float[] vect2)
            //Euclidean distance between two vectors. Assumes same length
            {
                if (vect1 == null || vect2 == null)
                    return float.MaxValue;

                float sqsum = 0;
                for (int i = 0; i < vect1.Length; i++)
                    sqsum += (vect1[i] - vect2[i]) * (vect1[i] - vect2[i]);
                return sqsum;
            }
        }

        public void AddNode(string label,float[] vectorpar)
        {
            nodeclass nc = new nodeclass(label, vectorpar);
            nodedict.Add(nnode, nc);
            nnode++;
        }


        public float[,] MakeDistmatrixFromNodes()
        {
            int nactive = nodedict.Count;
            float[,] distmatrix = new float[nactive * 2, nactive * 2];
            for (int i = 0; i < nactive * 2; i++)
                for (int j = 0; j < nactive * 2; j++)
                {
                    distmatrix[i, j] = float.MaxValue;
                }
            for (int i = 0; i < nnode; i++)
                for (int j = 0; j < nnode; j++)
                {
                    if (j > i)
                        distmatrix[i, j] = nodeclass.vectdist(nodedict[i].vector, nodedict[j].vector);
                    else
                        distmatrix[i, j] = float.MaxValue;
                }
            return distmatrix;
        }

        public float[,] MakeDistmatrixFromDict(Dictionary<string, Dictionary<string, double>> distdict)
        {
            int nactive = distdict.Count;
            Console.WriteLine("MakeDistmatrixFromDict");
            nodedict.Clear();
            int k = 0;
            foreach (string s in distdict.Keys)
            {
                nodedict.Add(k, new nodeclass(s, null));
                k++;
            }
            nnode = nodedict.Count;
            Console.WriteLine("nnode = " + nnode);

            float[,] distmatrix = new float[nactive * 2, nactive * 2];
            for (int i = 0; i < nactive * 2; i++)
                for (int j = 0; j < nactive * 2; j++)
                {
                    distmatrix[i, j] = float.MaxValue;
                }
            for (int i = 0; i < nnode; i++)
                for (int j = 0; j < nnode; j++)
                {
                    if (j > i && distdict[nodedict[i].nodelabel].ContainsKey(nodedict[j].nodelabel))
                    {
                        distmatrix[i, j] = (float)distdict[nodedict[i].nodelabel][nodedict[j].nodelabel];
                        if ( distmatrix[i,j] < 0)
                            distmatrix[i, j] = float.MaxValue;
                    }
                    else
                        distmatrix[i, j] = float.MaxValue;
                }
            return distmatrix;
        }

        public void BuildTree()
        {
            Console.WriteLine("BuildTree");
            int nactive = nodedict.Count;
            float[,] distmatrix = MakeDistmatrixFromNodes();

            BuildTree(distmatrix, nactive, true,null,null);
        }

        public void BuildTree(Dictionary<string,Dictionary<string,double>> distdict, string fn)
        {
            int nactive = distdict.Count;
            float[,] distmatrix = MakeDistmatrixFromDict(distdict);
            BuildTree(distmatrix, nactive, false,distdict,fn);
        }

        public void BuildTree(float[,] distmatrix, int nactivepar, bool hasvectors, Dictionary<string, Dictionary<string, double>> distdict, string fn)
        {

            double min0 = float.MaxValue;
            double max0 = -1;
            for (int i = 0; i < nnode - 1; i++)
                for (int j = i + 1; j < nnode; j++)
                {
                    if (distmatrix[i, j] < min0 && distmatrix[i,j] > 0)
                    {
                        min0 = distmatrix[i, j];
                    }
                    if (distmatrix[i, j] > max0)
                    {
                        max0 = distmatrix[i, j];
                    }
                }

            using (StreamWriter sw = new StreamWriter(fn))
            {
                double scalebase = Math.Log10(min0) - 0.1;
                //scalebase = -8.1;
                sw.WriteLine("min0, scalebase = " + min0 + " " + scalebase);

                int nactive = nactivepar;
                while (nactive > 1)
                {
                    int imax = -1;
                    int jmax = -1;
                    float max = float.MaxValue;
                    for (int i = 0; i < nnode - 1; i++)
                        for (int j = i + 1; j < nnode; j++)
                        {
                            if (distmatrix[i, j] < max)
                            {
                                max = distmatrix[i, j];
                                imax = i;
                                jmax = j;
                            }
                        }
                    sw.WriteLine("imax,jmax,max,nactive = \t" + imax + "\t" + jmax + "\t" + max+"\t"+nactive);
                    if (imax < 0)
                    {
                        sw.WriteLine("break imax");
                        break;
                    }
                    nodeclass nc;
                    if (hasvectors)
                        nc = nodeclass.merge(nodedict[jmax], nodedict[imax]);
                    else
                    {
                        nc = nodeclass.merge(nodedict[jmax], nodedict[imax], 0.5 * max, scalebase);
                    }
                    nodedict[imax].active = false;
                    nodedict[jmax].active = false;
                    nodedict.Add(nnode, nc);
                    nnode++;
                    nactive--;

                    for (int i = 0; i < nnode - 1; i++)
                    {
                        if (nodedict[i].active)
                        {
                            if (hasvectors)
                                distmatrix[i, nnode - 1] = nodeclass.vectdist(nodedict[i].vector, nc.vector);
                            else
                            {

                                float dimax = 0;
                                if (imax > i)
                                    dimax = distmatrix[i, imax];
                                else
                                    dimax = distmatrix[imax, i];

                                float djmax = 0;
                                if (jmax > i)
                                    djmax = distmatrix[i, jmax];
                                else
                                    djmax = distmatrix[jmax, i];
                                if (dimax == float.MaxValue)
                                    distmatrix[i, nnode - 1] = djmax;
                                else if (djmax == float.MaxValue)
                                    distmatrix[i, nnode - 1] = dimax;
                                else
                                    distmatrix[i, nnode - 1] = (float)0.5 * (dimax + djmax);
                                //Console.WriteLine("dm=" + distmatrix[i, nnode - 1]);
                            }

                        }
                    }
                    for (int i = 0; i < nnode; i++)
                    {
                        distmatrix[i, imax] = float.MaxValue;
                        distmatrix[i, jmax] = float.MaxValue;
                        distmatrix[imax, i] = float.MaxValue;
                        distmatrix[jmax, i] = float.MaxValue;
                    }
                }

                sw.WriteLine("nactive = " + nactive);

                sw.WriteLine("PrintTree");

                foreach (int i in nodedict.Keys)
                    if (nodedict[i].active)
                    {
                        PrintTree(nodedict[i], "",sw);
                        sw.WriteLine(nodedict[i].ph);
                    }
            }
        }

        static string addindent = "  ";

        public void PrintTree(nodeclass root,string indent, StreamWriter sw)
        {
            if (root == null)
                return;
            sw.WriteLine(indent + root.nodelabel);
            PrintTree(root.branch1, indent + addindent,sw);
            PrintTree(root.branch2, indent + addindent,sw);
        }

        public void MakePhFile(StreamWriter sw, nodeclass root)
        {
            if (root == null)
                return;
            sw.WriteLine(root.ph);
        }
    }
}
