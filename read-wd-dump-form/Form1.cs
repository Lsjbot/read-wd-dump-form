using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Globalization;
using PrepositionsClusters;
using Accord.Math;
using Accord.Statistics;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Statistics.Visualizations;
using Accord.Statistics.Distributions.DensityKernels;
using Accord.Controls;
using Accord.MachineLearning;


namespace read_wd_dump_form
{
    public partial class Form1 : Form
    {
        public string recentfile = "";
        
        public Form1()
        {
            InitializeComponent();
            fill_propdict();
            fill_whitelists();
            fill_coverdict();
            fill_wddict();

            foreach (string s in propdict.Keys)
                CB_props.Items.Add(s);
            foreach (string s in classdict.Keys)
                CB_categories.Items.Add(s);
            //read_labels();
            //read_countries();

            read_subclasses();

            Console.WriteLine("Test console");
        }

        private void quitbutton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public static string dumpfolder = @"G:\wikidata\";
        public static string outfolder = @"G:\wikidata\";

        public class subclassclass
        {
            public string label = "";
            public List<int> up = new List<int>();
            public List<int> down = new List<int>();
            public int instances = 0;
        }
        public static Dictionary<int, subclassclass> subclassdict = new Dictionary<int, subclassclass>();
        public static Dictionary<string, int> propdict = new Dictionary<string, int>(); //from wikidata property name to property id
        public static Dictionary<int,string> intpropdict = new Dictionary<int,string>(); //from property id to wikidata property name
        public static Dictionary<string, int> classdict = new Dictionary<string, int>(); //from wikidata class name to item id
        public static Dictionary<string, int> claimstatdict = new Dictionary<string, int>(); //statistics on how many claims for each property
        public static Dictionary<string, int> occupationstatdict = new Dictionary<string, int>(); //statistics on how many claims for each property
        public static Dictionary<string, int> countrystatdict = new Dictionary<string, int>(); //statistics on how many claims for each property
        public static Dictionary<string, int> positionstatdict = new Dictionary<string, int>(); //statistics on how many claims for each property
        public static Dictionary<string, string> propnamedict = new Dictionary<string, string>(); //from prop ID to prop label
        public static Dictionary<string, string> labeldict = new Dictionary<string, string>(); //from item ID to label
        public static Dictionary<string, string> countrynamedict = new Dictionary<string, string>(); //from wdid (Qxxx) to country name
        public static Dictionary<string, List<int>> whitelist = new Dictionary<string, List<int>>();
        public static Dictionary<string, List<int>> blacklist = new Dictionary<string, List<int>>();
        public static Dictionary<string, List<int>> whiteinstance = new Dictionary<string, List<int>>();
        public static Dictionary<string, List<int>> blackinstance = new Dictionary<string, List<int>>();
        public static Dictionary<string, List<int>> whitesubclass = new Dictionary<string, List<int>>();
        public static Dictionary<string, List<int>> blacksubclass = new Dictionary<string, List<int>>();
        public static List<string> blacklang = new List<string>();

        public static Dictionary<string, double> coverdict = new Dictionary<string, double>();
        public static List<string> domainlist = new List<string>();
        public static List<string> lopsided = new List<string>();
        public static Dictionary<string, List<string>> lopsideddict = new Dictionary<string, List<string>>();


        public static List<string> wanted_labels = new List<string>();

        public static char[] trimchars = "[,]".ToCharArray();

        public static Dictionary<string, string> wddict = new Dictionary<string, string>();
        public static Dictionary<string, string> subj1dict = new Dictionary<string, string>();
        public static Dictionary<string, string> subj2dict = new Dictionary<string, string>();
        public static Dictionary<string, string> wdqdict = new Dictionary<string, string>();
        public static Dictionary<int, string> qlabeldict = new Dictionary<int, string>();

        Dictionary<string, Dictionary<string, int>> firstnamecountrydict = new Dictionary<string, Dictionary<string, int>>();
        Dictionary<string, Dictionary<string, int>> familynamecountrydict = new Dictionary<string, Dictionary<string, int>>();
        Dictionary<string, Dictionary<int, int>> firstnamecenturydict = new Dictionary<string, Dictionary<int, int>>();
        Dictionary<string, Dictionary<int, int>> familynamecenturydict = new Dictionary<string, Dictionary<int, int>>();
        Dictionary<string, Dictionary<int, int>> firstnamedecadedict = new Dictionary<string, Dictionary<int, int>>();
        Dictionary<string, Dictionary<int, int>> familynamedecadedict = new Dictionary<string, Dictionary<int, int>>();
        Dictionary<string, string> nameplacedict = new Dictionary<string, string>();



        public class wdtopclass
        {
            public string id = "";
            public string type = "";
            public Dictionary<string, labelclass> labels = new Dictionary<string, labelclass>();
            public Dictionary<string, labelclass> descriptions = new Dictionary<string, labelclass>();
            public Dictionary<string, labelclass> aliases = new Dictionary<string, labelclass>();
            public string claims = "";
            public Dictionary<string, sitelinkclass> sitelinks = new Dictionary<string, sitelinkclass>();
            public int lastrevid = 0;
            public string modified = "";
        }

        public class wdtopclass2
        {
            public string id { get; set; }
            public string type { get; set; }
            public string labels { get; set; }
            public string descriptions { get; set; }
            public string aliases { get; set; }
            public string claims { get; set; }
            public string sitelinks { get; set; }
            public int lastrevid { get; set; }
            public string modified { get; set; }
        }

        public class sitelinkclass
        {
            public string site = "";
            public string title = "";
            public List<string> badges = new List<string>();
        }

        public class labelclass
        {
            public string lang = "";
            public string value = "";
        }

        public static string[] preferred_languages = { "en", "no", "sv", "es", "de", "fr", "ceb", "da", "it", "pt", "en-gb", "en-ca", "tl", "war", "nl" };
        public static int BCoffset = 3000;

        public static int iqentity = 35120;
        public static int[] humanprops = { 21, 27, 569, 106, 39, 28640 };

        public static int maxlang = 310;
        public Dictionary<string, int> langindex = new Dictionary<string, int>();
        public Dictionary<int, string> indexlang = new Dictionary<int, string>();
        public float[,] norm_matrix = new float[maxlang, maxlang];
        public float[,] dist_matrix = new float[maxlang, maxlang];
        public int[,] hit_matrix = new int[maxlang, maxlang];
        public int[,] miss_matrix = new int[maxlang, maxlang];
        public Dictionary<string, Dictionary<string, double>> normdict = new Dictionary<string, Dictionary<string, double>>();
        public Dictionary<string, Dictionary<string, double>> iwdistdict = new Dictionary<string, Dictionary<string, double>>();
        public Dictionary<string, Dictionary<string, double>> geodistdict = new Dictionary<string, Dictionary<string, double>>();
        Dictionary<string, int> langstats = new Dictionary<string, int>();


        public static hbookclass nlanghist = new hbookclass("nlanghist");
        public static hbookclass nitemhist = new hbookclass("nitemhist");
        public static hbookclass instancehist = new hbookclass("instancehist");

        public static void fill_whitelists()
        {
            foreach (string s in classdict.Keys)
            {
                whitelist.Add(s,new List<int>());
                blacklist.Add(s,new List<int>());
                whiteinstance.Add(s,new List<int>());
                blackinstance.Add(s,new List<int>());
                whitesubclass.Add(s, new List<int>());
                blacksubclass.Add(s, new List<int>());

            }

            whiteinstance["color"].Add(1075);

            whitelist["color"].Add(604079);
            whitelist["color"].Add(899318);
            whitelist["color"].Add(901731);
            whitelist["color"].Add(1358981);
            whitelist["color"].Add(5148721);
            whitelist["color"].Add(2722041);
            whitelist["color"].Add(2013559);
            whitelist["color"].Add(846504);
            whitelist["color"].Add(5069879);

            whiteinstance["clothing"].Add(11460);
            whiteinstance["clothing"].Add(337481);
            whiteinstance["clothing"].Add(516992);
            whiteinstance["clothing"].Add(463843);
            whiteinstance["clothing"].Add(1130359);
            whiteinstance["clothing"].Add(1187616);
            whiteinstance["clothing"].Add(1435365);
            whiteinstance["clothing"].Add(755313);
            whiteinstance["clothing"].Add(193204);
            whiteinstance["clothing"].Add(232191);
            whiteinstance["clothing"].Add(76768);
            whiteinstance["clothing"].Add(152563);
            whiteinstance["clothing"].Add(930924);
            whiteinstance["clothing"].Add(2160801);
            whiteinstance["clothing"].Add(18707829);
            whiteinstance["clothing"].Add(15807604);
            whiteinstance["clothing"].Add(1077396);
            whiteinstance["clothing"].Add(890119);
            whiteinstance["clothing"].Add(39908);
            whiteinstance["clothing"].Add(198763);
            whiteinstance["clothing"].Add(13716);
            whiteinstance["clothing"].Add(180225);
            whiteinstance["clothing"].Add(7863403);
            whiteinstance["clothing"].Add(14833626);
            whiteinstance["clothing"].Add(200539);
            whiteinstance["clothing"].Add(5100757);
            whiteinstance["clothing"].Add(1036729);
            whiteinstance["clothing"].Add(849964);
            whiteinstance["clothing"].Add(2490224);
            whiteinstance["clothing"].Add(212989);
            whiteinstance["clothing"].Add(522648);
            whiteinstance["clothing"].Add(614806);
            whiteinstance["clothing"].Add(2524539);
            whiteinstance["clothing"].Add(43663);
            whiteinstance["clothing"].Add(7434);
            whiteinstance["clothing"].Add(1196123);
            whiteinstance["clothing"].Add(4752912);
            whiteinstance["clothing"].Add(862633);
            whiteinstance["clothing"].Add(223269);
            whiteinstance["clothing"].Add(1034198);
            whiteinstance["clothing"].Add(4388799);
            whiteinstance["clothing"].Add(152574);
            whiteinstance["clothing"].Add(2018769);
            whiteinstance["clothing"].Add(4465194);
            whiteinstance["clothing"].Add(645292);
            whiteinstance["clothing"].Add(9053464);
            whiteinstance["clothing"].Add(1052147);
            whiteinstance["clothing"].Add(15830292);
            whiteinstance["clothing"].Add(4400800);
            whiteinstance["clothing"].Add(2457980);

            foreach (int i in whiteinstance["clothing"])
                whitesubclass["clothing"].Add(i);

            whiteinstance["container"].Add(987767);
            whiteinstance["container"].Add(38942);
            whiteinstance["container"].Add(673338);
            whiteinstance["container"].Add(869381);
            whiteinstance["container"].Add(10563755);
            whiteinstance["container"].Add(132397);
            whiteinstance["container"].Add(2413314);
            whiteinstance["container"].Add(191851);
            whiteinstance["container"].Add(476968);
            whiteinstance["container"].Add(11082837);
            whiteinstance["container"].Add(579418);
            whiteinstance["container"].Add(1350279);
            whiteinstance["container"].Add(745783);
            whiteinstance["container"].Add(717954);
            whiteinstance["container"].Add(18779123);
            whiteinstance["container"].Add(80228);
            whiteinstance["container"].Add(153988);
            whiteinstance["container"].Add(1537820);
            whiteinstance["container"].Add(4044785);
            whiteinstance["container"].Add(2846544);
            whiteinstance["container"].Add(10289);
            whiteinstance["container"].Add(47107);
            whiteinstance["container"].Add(188075);
            whiteinstance["container"].Add(22661409);
            whiteinstance["container"].Add(201097);
            whiteinstance["container"].Add(1323314);
            whiteinstance["container"].Add(2453629);
            whiteinstance["container"].Add(81727);
            whiteinstance["container"].Add(516844);
            whiteinstance["container"].Add(14920412);
            whiteinstance["container"].Add(1531435);
            whiteinstance["container"].Add(1635923);
            whiteinstance["container"].Add(18451251);
            whiteinstance["container"].Add(2100893);
            whiteinstance["container"].Add(81707);
            whiteinstance["container"].Add(386215);
            whiteinstance["container"].Add(668349);
            whiteinstance["container"].Add(210723);
            whiteinstance["container"].Add(178401);
            whiteinstance["container"].Add(216530);
            whiteinstance["container"].Add(740460);
            whiteinstance["container"].Add(865054);
            whiteinstance["container"].Add(1207302);
            whiteinstance["container"].Add(766983);
            whiteinstance["container"].Add(1861980);
            whiteinstance["container"].Add(3399907);
            whiteinstance["container"].Add(129211);
            whiteinstance["container"].Add(335661);
            whiteinstance["container"].Add(366134);
            whiteinstance["container"].Add(639460);
            whiteinstance["container"].Add(1180158);
            whiteinstance["container"].Add(1756525);
            whiteinstance["container"].Add(6501028);
            whiteinstance["container"].Add(14552508);
            whiteinstance["container"].Add(11083119);
            whiteinstance["container"].Add(15706035);
            whiteinstance["container"].Add(17379796);
            whiteinstance["container"].Add(4167876);
            whiteinstance["container"].Add(732177);

            foreach (int i in whiteinstance["container"])
                whitesubclass["container"].Add(i);

            whitelist["body part"].Add(15807);
            whitelist["body part"].Add(7881);

            blacklist["body part"].Add(5084416);
            blacklist["body part"].Add(22710);
            blacklist["body part"].Add(17013040);
            blacklist["body part"].Add(1418632);
            blacklist["body part"].Add(1342751);
            blacklist["body part"].Add(1514294);
            blacklist["body part"].Add(1952802);
            blacklist["body part"].Add(433259);
            blacklist["body part"].Add(1650656);
            blacklist["body part"].Add(1890593);
            blacklist["body part"].Add(3648825);
            blacklist["body part"].Add(27163629);
            blacklist["body part"].Add(23786);
            blacklist["body part"].Add(245388);
            blacklist["body part"].Add(503735);
            blacklist["body part"].Add(1593118);
            blacklist["body part"].Add(1640354);


            whiteinstance["body part"].Add(4620674);
            whiteinstance["body part"].Add(213456);
            whiteinstance["body part"].Add(18534278);
            whiteinstance["body part"].Add(17781690);
            whiteinstance["body part"].Add(34583);
            whiteinstance["body part"].Add(154425);
            whiteinstance["body part"].Add(620207);
            whiteinstance["body part"].Add(592177);
            whiteinstance["body part"].Add(841423);
            whiteinstance["body part"].Add(7881);

            foreach (int i in whiteinstance["body part"])
                whitesubclass["body part"].Add(i);

            blackinstance["food"].Add(8495); //milk
            blackinstance["food"].Add(40050); //drink
            blackinstance["food"].Add(26994960); //russian stuff
            blackinstance["food"].Add(26995321);
            blackinstance["food"].Add(26996564);
            blackinstance["food"].Add(26869408);
            blackinstance["food"].Add(1884224);
            blackinstance["food"].Add(185583); //candy
            blackinstance["food"].Add(1361086);
            blackinstance["food"].Add(27230949);
            blackinstance["food"].Add(2836947); //animal feed
            blackinstance["food"].Add(746549); // dish
            blackinstance["food"].Add(2596997); // condiments
            blackinstance["food"].Add(749316); //snacks
            blackinstance["food"].Add(6584340);
            //blackinstance["food"].Add();
            //blackinstance["food"].Add();

            whiteinstance["food"].Add(2095);
            whiteinstance["food"].Add(951964);
            whiteinstance["food"].Add(11002);
            whiteinstance["food"].Add(36465);
            whiteinstance["food"].Add(93189);
            whiteinstance["food"].Add(690292);
            whiteinstance["food"].Add(177);
            whiteinstance["food"].Add(178);
            whiteinstance["food"].Add(856330);
            whiteinstance["food"].Add(27242754);
            whiteinstance["food"].Add(185217); //dairy products
            whiteinstance["food"].Add(3506176);
            whiteinstance["food"].Add(26868224);
            whiteinstance["food"].Add(10943);
            whiteinstance["food"].Add(3546121);
            whiteinstance["food"].Add(186817);
            whiteinstance["food"].Add(5159627);
            whiteinstance["food"].Add(29214);
            whiteinstance["food"].Add(44541);
            whiteinstance["food"].Add(13276);
            whiteinstance["food"].Add(1427887);
            whiteinstance["food"].Add(260568);
            whiteinstance["food"].Add(3194888);
            whiteinstance["food"].Add(131419);
            whiteinstance["food"].Add(997002);
            whiteinstance["food"].Add(18560070);
            whiteinstance["food"].Add(13475519);
            whiteinstance["food"].Add(10426);
            whiteinstance["food"].Add(170486);
            whiteinstance["food"].Add(5200157);
            whiteinstance["food"].Add(13270);
            whiteinstance["food"].Add(13266);
            whiteinstance["food"].Add(1063096);
            whiteinstance["food"].Add(1854639);
            whiteinstance["food"].Add(339836);
            whiteinstance["food"].Add(1137679);
            whiteinstance["food"].Add(3314483); //fruit
            whiteinstance["food"].Add(1628963);
            whiteinstance["food"].Add(477248);
            whiteinstance["food"].Add(13360264);
            whiteinstance["food"].Add(5367573);
            whiteinstance["food"].Add(26791887);
            whiteinstance["food"].Add(1142483);
            whiteinstance["food"].Add(736427);
            whiteinstance["food"].Add(7802);
            whiteinstance["food"].Add(666242);
            whiteinstance["food"].Add(21595102);
            whiteinstance["food"].Add(21596812);
            whiteinstance["food"].Add(375);
            whiteinstance["food"].Add(13377687);
            whiteinstance["food"].Add(5090);
            whiteinstance["food"].Add(192874);
            whiteinstance["food"].Add(34156);
            whiteinstance["food"].Add(27566431);
            whiteinstance["food"].Add(2723400);
            whiteinstance["food"].Add(1322949);
            whiteinstance["food"].Add(1472481);
            whiteinstance["food"].Add(11004); //vegetables
            whiteinstance["food"].Add(11009);
            whiteinstance["food"].Add(5726634);
            whiteinstance["food"].Add(4426758);
            whiteinstance["food"].Add(18396735);
            whiteinstance["food"].Add(6950796);
            whiteinstance["food"].Add(18396734);
            whiteinstance["food"].Add(5194698);
            whiteinstance["food"].Add(192935);
            whiteinstance["food"].Add(17116319);
            //whiteinstance["food"].Add();
            //whiteinstance["food"].Add();

            foreach (int i in blackinstance["food"])
                whitesubclass["food"].Add(i);
            foreach (int i in blackinstance["food"])
                whitesubclass["food"].Add(i);


            whiteinstance["human"].Add(5);

            blacklang.Add("commons");
            blacklang.Add("species");
            blacklang.Add("data");
            blacklang.Add("meta");
            blacklang.Add("bug");
            blacklang.Add("cr");
            blacklang.Add("ve");
            blacklang.Add("tum");
            blacklang.Add("ik");
            blacklang.Add("ti");
            blacklang.Add("dz");
            blacklang.Add("ki");
            blacklang.Add("nv");
            blacklang.Add("nso");
            blacklang.Add("sg");


        }

        public static void fill_coverdict()
        {
            //languages where most of the coverage (>0.7) is in a single domain
            lopsided.Add("ik");
            lopsided.Add("atj");
            lopsided.Add("sn");
            lopsided.Add("tcy");
            lopsided.Add("jbo");
            lopsided.Add("arc");
            lopsided.Add("bjn");
            lopsided.Add("xh");
            lopsided.Add("za");
            lopsided.Add("roa_rup");
            lopsided.Add("fiu_vro");
            lopsided.Add("ug");
            lopsided.Add("mrj");
            lopsided.Add("gn");
            lopsided.Add("kab");
            lopsided.Add("ln");
            lopsided.Add("ksh");
            lopsided.Add("km");
            lopsided.Add("so");
            lopsided.Add("gan");
            lopsided.Add("new");
            lopsided.Add("cdo");
            lopsided.Add("hak");
            lopsided.Add("wa");
            lopsided.Add("ay");
            lopsided.Add("bh");
            lopsided.Add("co");
            lopsided.Add("sa");
            lopsided.Add("ia");
            lopsided.Add("diq");
            lopsided.Add("gu");
            lopsided.Add("bcl");
            lopsided.Add("gd");
            lopsided.Add("or");
            lopsided.Add("mzn");
            lopsided.Add("war");
            lopsided.Add("ckb");
            lopsided.Add("qu");
            lopsided.Add("an");
            lopsided.Add("uz");
            lopsided.Add("te");
            lopsided.Add("sw");
            lopsided.Add("bs");
            lopsided.Add("zh_min_nan");
            lopsided.Add("ht");
            lopsided.Add("is");
            lopsided.Add("ga");
            lopsided.Add("fy");
            lopsided.Add("mr");
            lopsided.Add("lv");

            foreach (string domain in domainlist)
                lopsideddict.Add(domain, new List<string>());
            lopsideddict["food"].Add("dv");
            lopsideddict["food"].Add("fiu_vro");
            lopsideddict["food"].Add("ay");
            lopsideddict["food"].Add("arc");
            lopsideddict["food"].Add("ik");
            lopsideddict["food"].Add("kbp");
            lopsideddict["food"].Add("lbe");
            lopsideddict["clothing"].Add("dv");
            lopsideddict["clothing"].Add("arc");
            lopsideddict["food"].Add("diq");
            lopsideddict["food"].Add("gn");
            lopsideddict["food"].Add("ug");
            lopsideddict["clothing"].Add("ug");
            lopsideddict["food"].Add("sn");
            lopsideddict["food"].Add("fy");
            lopsideddict["food"].Add("roa_rup");
            lopsideddict["food"].Add("gv");
            lopsideddict["food"].Add("pam");
            lopsideddict["food"].Add("sa");
            lopsideddict["food"].Add("ia");
            lopsideddict["clothing"].Add("ay");
            lopsideddict["food"].Add("kab");
            lopsideddict["food"].Add("qu");
            lopsideddict["food"].Add("ps");
            lopsideddict["food"].Add("cdo");
            lopsideddict["container"].Add("so");
            lopsideddict["color"].Add("gd");
            lopsideddict["food"].Add("ckb");
            lopsideddict["food"].Add("an");
            lopsideddict["container"].Add("sa");
            lopsideddict["food"].Add("hak");
            lopsideddict["clothing"].Add("lfn");
            lopsideddict["food"].Add("new");
            lopsideddict["food"].Add("war");
            lopsideddict["food"].Add("ln");
            lopsideddict["food"].Add("pdc");
            lopsideddict["food"].Add("gan");
            lopsideddict["food"].Add("ht");
            lopsideddict["food"].Add("so");
            lopsideddict["food"].Add("nds");
            lopsideddict["food"].Add("scn");
            lopsideddict["color"].Add("diq");
            lopsideddict["food"].Add("iu");
            lopsideddict["food"].Add("mzn");
            lopsideddict["food"].Add("gd");
            lopsideddict["food"].Add("bjn");
            lopsideddict["food"].Add("tcy");
            lopsideddict["food"].Add("km");
            lopsideddict["food"].Add("xmf");
            lopsideddict["container"].Add("tt");
            lopsideddict["food"].Add("sw");
            lopsideddict["food"].Add("dty");
            lopsideddict["clothing"].Add("pnb");
            lopsideddict["food"].Add("bo");
            lopsideddict["food"].Add("nah");
            lopsideddict["food"].Add("io");
            lopsideddict["container"].Add("new");
            lopsideddict["food"].Add("bcl");
            lopsideddict["container"].Add("sn");
            lopsideddict["food"].Add("pnb");
            lopsideddict["food"].Add("yo");
            lopsideddict["food"].Add("zh_min_nan");
            lopsideddict["clothing"].Add("ik");
            lopsideddict["container"].Add("za");
            lopsideddict["food"].Add("yi");
            lopsideddict["food"].Add("eml");
            lopsideddict["food"].Add("ksh");
            lopsideddict["color"].Add("kab");
            lopsideddict["food"].Add("wa");
            lopsideddict["food"].Add("ang");
            lopsideddict["clothing"].Add("hak");
            lopsideddict["clothing"].Add("za");
            lopsideddict["food"].Add("azb");
            lopsideddict["container"].Add("kab");
            lopsideddict["clothing"].Add("bs");
            lopsideddict["clothing"].Add("ln");
            lopsideddict["food"].Add("ne");
            lopsideddict["food"].Add("atj");
            lopsideddict["clothing"].Add("nap");
            lopsideddict["food"].Add("te");
            lopsideddict["food"].Add("am");
            lopsideddict["food"].Add("sd");
            lopsideddict["food"].Add("bs");
            lopsideddict["food"].Add("lmo");
            lopsideddict["clothing"].Add("ckb");
            lopsideddict["food"].Add("csb");
            lopsideddict["food"].Add("br");
            lopsideddict["food"].Add("vec");
            lopsideddict["food"].Add("cr");
            lopsideddict["food"].Add("sh");
            lopsideddict["container"].Add("hak");
            lopsideddict["container"].Add("fiu_vro");
            lopsideddict["food"].Add("hi");
            lopsideddict["color"].Add("mzn");
            lopsideddict["food"].Add("lv");
            lopsideddict["container"].Add("ln");
            lopsideddict["food"].Add("xh");
            lopsideddict["clothing"].Add("sah");
            lopsideddict["color"].Add("gu");
            lopsideddict["clothing"].Add("war");
            lopsideddict["clothing"].Add("diq");
            lopsideddict["clothing"].Add("ang");
            lopsideddict["clothing"].Add("oc");
            lopsideddict["color"].Add("ia");
            lopsideddict["food"].Add("af");
            lopsideddict["food"].Add("vls");
            lopsideddict["food"].Add("sco");
            lopsideddict["food"].Add("ga");
            lopsideddict["container"].Add("pdc");
            lopsideddict["food"].Add("ur");
            lopsideddict["color"].Add("ik");
            lopsideddict["container"].Add("ay");
            lopsideddict["clothing"].Add("iu");
            lopsideddict["food"].Add("oc");
            lopsideddict["food"].Add("lfn");
            lopsideddict["food"].Add("mdf");
            lopsideddict["clothing"].Add("cdo");
            lopsideddict["food"].Add("la");
            lopsideddict["container"].Add("war");
            lopsideddict["clothing"].Add("azb");
            lopsideddict["clothing"].Add("gu");
            lopsideddict["container"].Add("ik");
            lopsideddict["food"].Add("co");
            lopsideddict["food"].Add("ta");
            lopsideddict["food"].Add("sat");
            lopsideddict["clothing"].Add("mrj");
            lopsideddict["food"].Add("sq");
            lopsideddict["food"].Add("nrm");
            lopsideddict["container"].Add("vep");
            lopsideddict["clothing"].Add("ht");
            lopsideddict["clothing"].Add("ia");
            lopsideddict["container"].Add("fy");
            lopsideddict["clothing"].Add("xh");
            lopsideddict["clothing"].Add("bcl");
            lopsideddict["food"].Add("sk");
            lopsideddict["clothing"].Add("tt");
            lopsideddict["food"].Add("mr");
            lopsideddict["food"].Add("cv");
            lopsideddict["food"].Add("sr");
            lopsideddict["food"].Add("tg");
            lopsideddict["food"].Add("si");
            lopsideddict["food"].Add("sl");
            lopsideddict["food"].Add("sah");
            lopsideddict["clothing"].Add("bo");
            lopsideddict["food"].Add("ro");
            lopsideddict["clothing"].Add("fy");
            lopsideddict["clothing"].Add("ksh");
            lopsideddict["food"].Add("olo");
            lopsideddict["clothing"].Add("qu");
            lopsideddict["food"].Add("hr");
            lopsideddict["food"].Add("tt");
            lopsideddict["container"].Add("lv");
            lopsideddict["clothing"].Add("nah");
            lopsideddict["color"].Add("bjn");
            lopsideddict["food"].Add("tl");
            lopsideddict["food"].Add("uz");
            lopsideddict["food"].Add("bn");
            lopsideddict["food"].Add("gu");
            lopsideddict["food"].Add("nap");
            lopsideddict["food"].Add("bh");
            lopsideddict["color"].Add("ksh");
            lopsideddict["food"].Add("myv");
            lopsideddict["food"].Add("eu");
            lopsideddict["food"].Add("ml");
            lopsideddict["food"].Add("bar");
            lopsideddict["clothing"].Add("gd");
            lopsideddict["food"].Add("mn");
            lopsideddict["food"].Add("nn");
            lopsideddict["clothing"].Add("bh");
            lopsideddict["food"].Add("eo");
            lopsideddict["clothing"].Add("ga");
            lopsideddict["food"].Add("et");
            lopsideddict["container"].Add("cdo");
            lopsideddict["color"].Add("wa");
            lopsideddict["food"].Add("bat_smg");
            lopsideddict["container"].Add("sah");
            lopsideddict["color"].Add("nds");
            lopsideddict["container"].Add("is");
            lopsideddict["food"].Add("kn");
            lopsideddict["container"].Add("af");
            lopsideddict["food"].Add("ku");
            lopsideddict["food"].Add("vep");
            lopsideddict["food"].Add("lt");
            lopsideddict["food"].Add("is");
            lopsideddict["food"].Add("be");
            lopsideddict["body part"].Add("rue");
            lopsideddict["body part"].Add("sc");
            lopsideddict["body part"].Add("za");
            lopsideddict["body part"].Add("ku");
            lopsideddict["body part"].Add("bjn");
            lopsideddict["color"].Add("olo");
            lopsideddict["body part"].Add("sw");
            lopsideddict["body part"].Add("sco");
            lopsideddict["body part"].Add("lv");
            lopsideddict["body part"].Add("qu");
            lopsideddict["body part"].Add("chr");
            lopsideddict["body part"].Add("mr");
            lopsideddict["body part"].Add("ht");
            lopsideddict["body part"].Add("zh_min_nan");
            lopsideddict["body part"].Add("roa_rup");
            lopsideddict["body part"].Add("ay");
            lopsideddict["body part"].Add("is");
            lopsideddict["body part"].Add("te");
            lopsideddict["body part"].Add("fiu_vro");
            lopsideddict["body part"].Add("ga");
            lopsideddict["body part"].Add("uz");
            lopsideddict["body part"].Add("gn");
            lopsideddict["body part"].Add("an");
            lopsideddict["body part"].Add("ie");
            lopsideddict["body part"].Add("wa");
            lopsideddict["color"].Add("stq");
            lopsideddict["body part"].Add("cdo");
            lopsideddict["body part"].Add("atj");
            lopsideddict["body part"].Add("tcy");
            lopsideddict["body part"].Add("km");
            lopsideddict["body part"].Add("so");
            lopsideddict["body part"].Add("pam");
            lopsideddict["body part"].Add("hak");
            lopsideddict["body part"].Add("ln");
            lopsideddict["body part"].Add("gu");
            lopsideddict["body part"].Add("gan");
            lopsideddict["body part"].Add("ckb");
            lopsideddict["body part"].Add("ksh");
            lopsideddict["body part"].Add("gd");
            lopsideddict["body part"].Add("bh");
            lopsideddict["body part"].Add("co");
            lopsideddict["body part"].Add("war");
            lopsideddict["body part"].Add("mrj");
            lopsideddict["body part"].Add("new");
            lopsideddict["body part"].Add("sa");
            lopsideddict["body part"].Add("sat");
            lopsideddict["body part"].Add("fy");
            lopsideddict["body part"].Add("ia");
            lopsideddict["body part"].Add("mzn");
            lopsideddict["body part"].Add("lbe");
            lopsideddict["body part"].Add("xh");
            lopsideddict["body part"].Add("bs");
            lopsideddict["body part"].Add("bcl");
            lopsideddict["body part"].Add("arc");
            lopsideddict["body part"].Add("ug");
            lopsideddict["color"].Add("mdf");
            lopsideddict["body part"].Add("xmf");
            lopsideddict["body part"].Add("yo");
            lopsideddict["body part"].Add("ik");
            lopsideddict["body part"].Add("sn");
            lopsideddict["body part"].Add("kab");
            lopsideddict["body part"].Add("diq");
            lopsideddict["body part"].Add("iu");
            lopsideddict["body part"].Add("dv");

            //coverage in people domain:
            coverdict.Add("bug",2/(double)1109137);
            coverdict.Add("cr",2/(double)1109137);
            coverdict.Add("tum",2/(double)1109137);
            coverdict.Add("ve",2/(double)1109137);
            coverdict.Add("ik",3/(double)1109137);
            coverdict.Add("ti",4/(double)1109137);
            coverdict.Add("dz",5/(double)1109137);
            coverdict.Add("ki",6/(double)1109137);
            coverdict.Add("nv",7/(double)1109137);
            coverdict.Add("nso",8/(double)1109137);
            coverdict.Add("sg",8/(double)1109137);
            coverdict.Add("ff",10/(double)1109137);
            coverdict.Add("pi",10/(double)1109137);
            coverdict.Add("ady",11/(double)1109137);
            coverdict.Add("ee",11/(double)1109137);
            coverdict.Add("iu",13/(double)1109137);
            coverdict.Add("shn",13/(double)1109137);
            coverdict.Add("fj",14/(double)1109137);
            coverdict.Add("lg",14/(double)1109137);
            coverdict.Add("atj",15/(double)1109137);
            coverdict.Add("ch",15/(double)1109137);
            coverdict.Add("inh",15/(double)1109137);
            coverdict.Add("pnt",15/(double)1109137);
            coverdict.Add("ts",15/(double)1109137);
            coverdict.Add("lbe",16/(double)1109137);
            coverdict.Add("rn",16/(double)1109137);
            coverdict.Add("chr",17/(double)1109137);
            coverdict.Add("din",17/(double)1109137);
            coverdict.Add("bm",18/(double)1109137);
            coverdict.Add("kg",21/(double)1109137);
            coverdict.Add("pfl",24/(double)1109137);
            coverdict.Add("wo",24/(double)1109137);
            coverdict.Add("chy",25/(double)1109137);
            coverdict.Add("ss",25/(double)1109137);
            coverdict.Add("rmy",26/(double)1109137);
            coverdict.Add("sm",26/(double)1109137);
            coverdict.Add("sn",26/(double)1109137);
            coverdict.Add("tn",27/(double)1109137);
            coverdict.Add("ty",27/(double)1109137);
            coverdict.Add("ltg",28/(double)1109137);
            coverdict.Add("xal",28/(double)1109137);
            coverdict.Add("tet",29/(double)1109137);
            coverdict.Add("cu",30/(double)1109137);
            coverdict.Add("krc",30/(double)1109137);
            coverdict.Add("mdf",30/(double)1109137);
            coverdict.Add("ny",30/(double)1109137);
            coverdict.Add("st",30/(double)1109137);
            coverdict.Add("gag",31/(double)1109137);
            coverdict.Add("gor",32/(double)1109137);
            coverdict.Add("tpi",32/(double)1109137);
            coverdict.Add("tcy",33/(double)1109137);
            coverdict.Add("om",34/(double)1109137);
            coverdict.Add("pih",36/(double)1109137);
            coverdict.Add("kbd",37/(double)1109137);
            coverdict.Add("kl",38/(double)1109137);
            coverdict.Add("ks",38/(double)1109137);
            coverdict.Add("jbo",40/(double)1109137);
            coverdict.Add("ak",41/(double)1109137);
            coverdict.Add("roa_tara",41/(double)1109137);
            coverdict.Add("tw",41/(double)1109137);
            coverdict.Add("bpy",44/(double)1109137);
            coverdict.Add("pag",44/(double)1109137);
            coverdict.Add("ab",46/(double)1109137);
            coverdict.Add("arc",49/(double)1109137);
            coverdict.Add("bjn",52/(double)1109137);
            coverdict.Add("srn",52/(double)1109137);
            coverdict.Add("na",53/(double)1109137);
            coverdict.Add("xh",53/(double)1109137);
            coverdict.Add("za",55/(double)1109137);
            coverdict.Add("mi",56/(double)1109137);
            coverdict.Add("glk",58/(double)1109137);
            coverdict.Add("roa_rup",58/(double)1109137);
            coverdict.Add("got",59/(double)1109137);
            coverdict.Add("jam",60/(double)1109137);
            coverdict.Add("av",61/(double)1109137);
            coverdict.Add("koi",70/(double)1109137);
            coverdict.Add("lrc",70/(double)1109137);
            coverdict.Add("pap",71/(double)1109137);
            coverdict.Add("nov",73/(double)1109137);
            coverdict.Add("cbk_zam",75/(double)1109137);
            coverdict.Add("rw",79/(double)1109137);
            coverdict.Add("lez",80/(double)1109137);
            coverdict.Add("udm",82/(double)1109137);
            coverdict.Add("crh",84/(double)1109137);
            coverdict.Add("dv",89/(double)1109137);
            coverdict.Add("kaa",93/(double)1109137);
            coverdict.Add("map_bms",94/(double)1109137);
            coverdict.Add("nrm",94/(double)1109137);
            coverdict.Add("fiu_vro",95/(double)1109137);
            coverdict.Add("haw",95/(double)1109137);
            coverdict.Add("ig",99/(double)1109137);
            coverdict.Add("ug",109/(double)1109137);
            coverdict.Add("tyv",111/(double)1109137);
            coverdict.Add("zu",111/(double)1109137);
            coverdict.Add("olo",112/(double)1109137);
            coverdict.Add("frp",114/(double)1109137);
            coverdict.Add("mhr",116/(double)1109137);
            coverdict.Add("rm",118/(double)1109137);
            coverdict.Add("zea",118/(double)1109137);
            coverdict.Add("kv",119/(double)1109137);
            coverdict.Add("kbp",121/(double)1109137);
            coverdict.Add("stq",125/(double)1109137);
            coverdict.Add("gom",126/(double)1109137);
            coverdict.Add("bxr",127/(double)1109137);
            coverdict.Add("ace",130/(double)1109137);
            coverdict.Add("csb",137/(double)1109137);
            coverdict.Add("mrj",137/(double)1109137);
            coverdict.Add("sat",137/(double)1109137);
            coverdict.Add("gn",140/(double)1109137);
            coverdict.Add("kab",140/(double)1109137);
            coverdict.Add("fur",145/(double)1109137);
            coverdict.Add("ang",148/(double)1109137);
            coverdict.Add("lad",149/(double)1109137);
            coverdict.Add("to",154/(double)1109137);
            coverdict.Add("lo",155/(double)1109137);
            coverdict.Add("nds_nl",157/(double)1109137);
            coverdict.Add("pdc",160/(double)1109137);
            coverdict.Add("frr",161/(double)1109137);
            coverdict.Add("kw",170/(double)1109137);
            coverdict.Add("ln",171/(double)1109137);
            coverdict.Add("lij",174/(double)1109137);
            coverdict.Add("ksh",176/(double)1109137);
            coverdict.Add("se",180/(double)1109137);
            coverdict.Add("lfn",182/(double)1109137);
            coverdict.Add("myv",188/(double)1109137);
            coverdict.Add("km",193/(double)1109137);
            coverdict.Add("rue",198/(double)1109137);
            coverdict.Add("so",204/(double)1109137);
            coverdict.Add("hif",207/(double)1109137);
            coverdict.Add("gan",208/(double)1109137);
            coverdict.Add("bo",213/(double)1109137);
            coverdict.Add("gv",213/(double)1109137);
            coverdict.Add("min",218/(double)1109137);
            coverdict.Add("dsb",225/(double)1109137);
            coverdict.Add("pcd",235/(double)1109137);
            coverdict.Add("tk",236/(double)1109137);
            coverdict.Add("szl",237/(double)1109137);
            coverdict.Add("os",241/(double)1109137);
            coverdict.Add("ext",242/(double)1109137);
            coverdict.Add("nap",242/(double)1109137);
            coverdict.Add("bi",276/(double)1109137);
            coverdict.Add("new",296/(double)1109137);
            coverdict.Add("nah",303/(double)1109137);
            coverdict.Add("cdo",311/(double)1109137);
            coverdict.Add("vep",314/(double)1109137);
            coverdict.Add("dty",317/(double)1109137);
            coverdict.Add("ie",330/(double)1109137);
            coverdict.Add("sd",337/(double)1109137);
            coverdict.Add("hak",338/(double)1109137);
            coverdict.Add("wa",348/(double)1109137);
            coverdict.Add("as",353/(double)1109137);
            coverdict.Add("ay",355/(double)1109137);
            coverdict.Add("bh",359/(double)1109137);
            coverdict.Add("mt",363/(double)1109137);
            coverdict.Add("mwl",371/(double)1109137);
            coverdict.Add("co",377/(double)1109137);
            coverdict.Add("pam",383/(double)1109137);
            coverdict.Add("su",394/(double)1109137);
            coverdict.Add("ce",396/(double)1109137);
            coverdict.Add("li",426/(double)1109137);
            coverdict.Add("vec",427/(double)1109137);
            coverdict.Add("bat_smg",431/(double)1109137);
            coverdict.Add("vls",445/(double)1109137);
            coverdict.Add("hsb",468/(double)1109137);
            coverdict.Add("eml",483/(double)1109137);
            coverdict.Add("sa",502/(double)1109137);
            coverdict.Add("ps",516/(double)1109137);
            coverdict.Add("ia",529/(double)1109137);
            coverdict.Add("diq",537/(double)1109137);
            coverdict.Add("sc",545/(double)1109137);
            coverdict.Add("ha",546/(double)1109137);
            coverdict.Add("gu",565/(double)1109137);
            coverdict.Add("ilo",566/(double)1109137);
            coverdict.Add("am",570/(double)1109137);
            coverdict.Add("bcl",583/(double)1109137);
            coverdict.Add("bar",590/(double)1109137);
            coverdict.Add("si",608/(double)1109137);
            coverdict.Add("ceb",621/(double)1109137);
            coverdict.Add("sah",709/(double)1109137);
            coverdict.Add("xmf",776/(double)1109137);
            coverdict.Add("hyw",790/(double)1109137);
            coverdict.Add("gd",795/(double)1109137);
            coverdict.Add("or",801/(double)1109137);
            coverdict.Add("fo",818/(double)1109137);
            coverdict.Add("lmo",820/(double)1109137);
            coverdict.Add("my",855/(double)1109137);
            coverdict.Add("mzn",875/(double)1109137);
            coverdict.Add("zh_classical",890/(double)1109137);
            coverdict.Add("yi",904/(double)1109137);
            coverdict.Add("scn",1046/(double)1109137);
            coverdict.Add("ku",1054/(double)1109137);
            coverdict.Add("mai",1055/(double)1109137);
            coverdict.Add("kn",1257/(double)1109137);
            coverdict.Add("pms",1286/(double)1109137);
            coverdict.Add("war",1331/(double)1109137);
            coverdict.Add("wuu",1388/(double)1109137);
            coverdict.Add("als",1426/(double)1109137);
            coverdict.Add("ky",1504/(double)1109137);
            coverdict.Add("ckb",1510/(double)1109137);
            coverdict.Add("qu",1542/(double)1109137);
            coverdict.Add("ne",1639/(double)1109137);
            coverdict.Add("ba",1669/(double)1109137);
            coverdict.Add("vo",1708/(double)1109137);
            coverdict.Add("pnb",1710/(double)1109137);
            coverdict.Add("an",1946/(double)1109137);
            coverdict.Add("uz",2097/(double)1109137);
            coverdict.Add("cv",2129/(double)1109137);
            coverdict.Add("jv",2134/(double)1109137);
            coverdict.Add("te",2143/(double)1109137);
            coverdict.Add("mn",2297/(double)1109137);
            coverdict.Add("sw",2379/(double)1109137);
            coverdict.Add("bs",2488/(double)1109137);
            coverdict.Add("nds",2501/(double)1109137);
            coverdict.Add("yo",2660/(double)1109137);
            coverdict.Add("io",2677/(double)1109137);
            coverdict.Add("pa",2732/(double)1109137);
            coverdict.Add("arz",2957/(double)1109137);
            coverdict.Add("zh_min_nan",3087/(double)1109137);
            coverdict.Add("ht",3201/(double)1109137);
            coverdict.Add("is",3301/(double)1109137);
            coverdict.Add("oc",3340/(double)1109137);
            coverdict.Add("ga",3596/(double)1109137);
            coverdict.Add("tt",3635/(double)1109137);
            coverdict.Add("fy",3710/(double)1109137);
            coverdict.Add("be_x_old",3928/(double)1109137);
            coverdict.Add("lb",4350/(double)1109137);
            coverdict.Add("zh_yue",4447/(double)1109137);
            coverdict.Add("br",4516/(double)1109137);
            coverdict.Add("mr",4517/(double)1109137);
            coverdict.Add("ur",4644/(double)1109137);
            coverdict.Add("sco",4756/(double)1109137);
            coverdict.Add("ml",4781/(double)1109137);
            coverdict.Add("sq",5162/(double)1109137);
            coverdict.Add("mk",5312/(double)1109137);
            coverdict.Add("hi",5854/(double)1109137);
            coverdict.Add("kk",6319/(double)1109137);
            coverdict.Add("tl",6382/(double)1109137);
            coverdict.Add("bn",6580/(double)1109137);
            coverdict.Add("ta",7272/(double)1109137);
            coverdict.Add("ka",7344/(double)1109137);
            coverdict.Add("lv",7831/(double)1109137);
            coverdict.Add("cy",8109/(double)1109137);
            coverdict.Add("ms",8435/(double)1109137);
            coverdict.Add("tg",8841/(double)1109137);
            coverdict.Add("th",9069/(double)1109137);
            coverdict.Add("be",9411/(double)1109137);
            coverdict.Add("sk",9521/(double)1109137);
            coverdict.Add("ast",9709/(double)1109137);
            coverdict.Add("nn",9829/(double)1109137);
            coverdict.Add("la",10168/(double)1109137);
            coverdict.Add("az",10343/(double)1109137);
            coverdict.Add("lt",10554/(double)1109137);
            coverdict.Add("af",10912/(double)1109137);
            coverdict.Add("mg",11184/(double)1109137);
            coverdict.Add("hr",11296/(double)1109137);
            coverdict.Add("gl",11634/(double)1109137);
            coverdict.Add("species",11780/(double)1109137);
            coverdict.Add("sh",12027/(double)1109137);
            coverdict.Add("eu",13055/(double)1109137);
            coverdict.Add("hy",13636/(double)1109137);
            coverdict.Add("sl",14075/(double)1109137);
            coverdict.Add("sr",14746/(double)1109137);
            coverdict.Add("simple",15000/(double)1109137);
            coverdict.Add("et",15378/(double)1109137);
            coverdict.Add("el",16315/(double)1109137);
            coverdict.Add("eo",17548/(double)1109137);
            coverdict.Add("ro",18627/(double)1109137);
            coverdict.Add("azb",19198/(double)1109137);
            coverdict.Add("vi",19307/(double)1109137);
            coverdict.Add("bg",23667/(double)1109137);
            coverdict.Add("id",24959/(double)1109137);
            coverdict.Add("he",25420/(double)1109137);
            coverdict.Add("da",25484/(double)1109137);
            coverdict.Add("tr",29552/(double)1109137);
            coverdict.Add("ko",33886/(double)1109137);
            coverdict.Add("hu",36534/(double)1109137);
            coverdict.Add("cs",37170/(double)1109137);
            coverdict.Add("fi",49311/(double)1109137);
            coverdict.Add("fa",50256/(double)1109137);
            coverdict.Add("ca",50465/(double)1109137);
            coverdict.Add("no",52968/(double)1109137);
            coverdict.Add("uk",56787/(double)1109137);
            coverdict.Add("zh",63376/(double)1109137);
            coverdict.Add("nl",74827/(double)1109137);
            coverdict.Add("pt",74951/(double)1109137);
            coverdict.Add("sv",78683/(double)1109137);
            coverdict.Add("ja",98232/(double)1109137);
            coverdict.Add("pl",112994/(double)1109137);
            coverdict.Add("es",127627/(double)1109137);
            coverdict.Add("it",132669/(double)1109137);
            coverdict.Add("ar",133754/(double)1109137);
            coverdict.Add("ru",141931/(double)1109137);
            coverdict.Add("commons",163719/(double)1109137);
            coverdict.Add("fr",200425/(double)1109137);
            coverdict.Add("de",249376/(double)1109137);
            coverdict.Add("en", 520331 / (double)1109137);

        }

        public static void fill_propdict()
        {
            propdict.Add("instance_of", 31);
            propdict.Add("subclass_of", 279);
            
            propdict.Add("country", 17);
            propdict.Add("capital", 36);
            propdict.Add("commonscat", 373);
            propdict.Add("coat of arms", 94);
            propdict.Add("locatormap", 242);
            propdict.Add("flag", 41);
            propdict.Add("timezone", 421);
            propdict.Add("kids", 150);
            propdict.Add("parent", 131);
            propdict.Add("iso", 300);
            propdict.Add("borders", 47);
            propdict.Add("coordinates", 625);
            propdict.Add("inception", 571);
            propdict.Add("head of government", 6);
            propdict.Add("gnid", 1566);
            propdict.Add("follows", 155);
            propdict.Add("category dead", 1465);
            propdict.Add("category born", 1464);
            propdict.Add("category from", 1792);
            propdict.Add("image", 18);
            propdict.Add("banner", 948);
            //propdict.Add("sister city",190);
            propdict.Add("postal code", 281);
            propdict.Add("position", 625);
            propdict.Add("population", 1082);
            propdict.Add("instance", 31);
            propdict.Add("subclass", 279);
            propdict.Add("nexttowater", 206);

            propdict.Add("sex", 21);
            propdict.Add("citizenship", 27);
            propdict.Add("birthdate", 569);
            propdict.Add("deathdate", 570);
            propdict.Add("occupation", 106);
            propdict.Add("position_held", 39);
            propdict.Add("profession", 28640);

            propdict.Add("area", 2046);
            propdict.Add("elevation", 2044);

            propdict.Add("script", 282);
            propdict.Add("wikipedia", 424);
            propdict.Add("distribution_map", 1846);
            propdict.Add("iso1", 218);
            propdict.Add("iso2", 219);
            propdict.Add("iso3", 220);

            propdict.Add("family name", 734);
            propdict.Add("given name", 735);
            propdict.Add("pseudonym", 742);
            propdict.Add("official name", 1448);
            propdict.Add("birth name", 1477);
            propdict.Add("nickname", 1449);
            propdict.Add("name in native language", 1559);
            propdict.Add("religious name", 1635);
            propdict.Add("courtesy name", 1782);
            propdict.Add("temple name", 1785);
            propdict.Add("posthumous name", 1786);
            propdict.Add("art-name", 1787);
            propdict.Add("short name", 1813);
            propdict.Add("second family name in Spanish name", 1950);
            propdict.Add("Roman praenomen", 2358);
            propdict.Add("Roman nomen gentilicium", 2359);
            propdict.Add("Roman cognomen", 2365);
            propdict.Add("Roman agnomen", 2366);
            propdict.Add("name", 2561);
            propdict.Add("married name", 2562);
            propdict.Add("patronym or matronym", 5056);
            propdict.Add("Scandinavian middle name", 6978);
            propdict.Add("Vietnamese middle name", 8500);
            propdict.Add("Kunya", 8927);
            propdict.Add("first family name in Portuguese name", 9139);
            propdict.Add("ethnic group", 172);
            propdict.Add("place of birth", 19);
            propdict.Add("native language", 103);




            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);
            //propdict.Add("",);

            foreach (string ss in propdict.Keys)
                if (!intpropdict.ContainsKey(propdict[ss]))
                    intpropdict.Add(propdict[ss], ss);

            classdict.Add("human", 5);
            classdict.Add("mountain", 8502);
            classdict.Add("island", 23442);
            classdict.Add("island nation", 112099);
            classdict.Add("city", 515);
            classdict.Add("lake", 23397);
            classdict.Add("language", 34770);
            classdict.Add("volcano", 8072);
            classdict.Add("massif", 1061151);
            classdict.Add("human settlement", 486972);
            classdict.Add("country", 6256);
            classdict.Add("sovereign state", 3624078);
            classdict.Add("color", 1075);
            classdict.Add("food", 2095);
            classdict.Add("body part", 4936952);
            classdict.Add("container", 987767);
            classdict.Add("clothing", 11460);
            //classdict.Add("",);

            domainlist.Add("food");
            domainlist.Add("color");
            domainlist.Add("container");
            domainlist.Add("clothing");
            domainlist.Add("body part");


            string fn = dumpfolder + "wikidata-properties.txt";
            if (!File.Exists(fn))
                fn = fn.Replace(dumpfolder, outfolder);
            using (StreamReader sr = new StreamReader(fn))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    if (String.IsNullOrEmpty(s))
                        continue;
                    string[] words = s.Split('\t');
                    propnamedict.Add(words[0], words[1]);
                }

            }
        }

        public static void fill_wddict()
        {
            wddict.Add("Latin alphabet", "Q8229"); subj1dict.Add("Latin alphabet", "Language and literature"); subj2dict.Add("Latin alphabet", "Language and literature");
            wddict.Add("Borges, Jorge Luis", "Q909"); subj1dict.Add("Borges, Jorge Luis", "Biography"); subj2dict.Add("Borges, Jorge Luis", "Authors, playwrights and poets");

            wddict.Add("Bernhardt, Sarah", "Q4605"); subj1dict.Add("Bernhardt, Sarah", "Biography"); subj2dict.Add("Bernhardt, Sarah", "Actors, dancers and models");
            wddict.Add("Chaplin, Charlie", "Q882"); subj1dict.Add("Chaplin, Charlie", "Biography"); subj2dict.Add("Chaplin, Charlie", "Actors, dancers and models");
            wddict.Add("Dietrich, Marlene", "Q4612"); subj1dict.Add("Dietrich, Marlene", "Biography"); subj2dict.Add("Dietrich, Marlene", "Actors, dancers and models");
            wddict.Add("Monroe, Marilyn", "Q4616"); subj1dict.Add("Monroe, Marilyn", "Biography"); subj2dict.Add("Monroe, Marilyn", "Actors, dancers and models");



            wddict.Add("Le Corbusier", "Q4724"); subj1dict.Add("Le Corbusier", "Biography"); subj2dict.Add("Le Corbusier", "Artists and architects");
            wddict.Add("Dalí, Salvador", "Q5577"); subj1dict.Add("Dalí, Salvador", "Biography"); subj2dict.Add("Dalí, Salvador", "Artists and architects");
            wddict.Add("Dürer, Albrecht", "Q5580"); subj1dict.Add("Dürer, Albrecht", "Biography"); subj2dict.Add("Dürer, Albrecht", "Artists and architects");
            wddict.Add("Gogh, Vincent van", "Q5582"); subj1dict.Add("Gogh, Vincent van", "Biography"); subj2dict.Add("Gogh, Vincent van", "Artists and architects");
            wddict.Add("Goya, Francisco", "Q5432"); subj1dict.Add("Goya, Francisco", "Biography"); subj2dict.Add("Goya, Francisco", "Artists and architects");
            wddict.Add("Hokusai, Katsushika", "Q5586"); subj1dict.Add("Hokusai, Katsushika", "Biography"); subj2dict.Add("Hokusai, Katsushika", "Artists and architects");
            wddict.Add("Kahlo, Frida", "Q5588"); subj1dict.Add("Kahlo, Frida", "Biography"); subj2dict.Add("Kahlo, Frida", "Artists and architects");
            wddict.Add("Leonardo da Vinci", "Q762"); subj1dict.Add("Leonardo da Vinci", "Biography"); subj2dict.Add("Leonardo da Vinci", "Artists and architects");
            wddict.Add("Matisse, Henri", "Q5589"); subj1dict.Add("Matisse, Henri", "Biography"); subj2dict.Add("Matisse, Henri", "Artists and architects");
            wddict.Add("Michelangelo", "Q5592"); subj1dict.Add("Michelangelo", "Biography"); subj2dict.Add("Michelangelo", "Artists and architects");
            wddict.Add("Picasso, Pablo", "Q5593"); subj1dict.Add("Picasso, Pablo", "Biography"); subj2dict.Add("Picasso, Pablo", "Artists and architects");
            wddict.Add("Raphael", "Q5597"); subj1dict.Add("Raphael", "Biography"); subj2dict.Add("Raphael", "Artists and architects");
            wddict.Add("Rembrandt", "Q5598"); subj1dict.Add("Rembrandt", "Biography"); subj2dict.Add("Rembrandt", "Artists and architects");
            wddict.Add("Rubens, Peter Paul", "Q5599"); subj1dict.Add("Rubens, Peter Paul", "Biography"); subj2dict.Add("Rubens, Peter Paul", "Artists and architects");
            wddict.Add("Sinan, Mimar", "Q5600"); subj1dict.Add("Sinan, Mimar", "Biography"); subj2dict.Add("Sinan, Mimar", "Artists and architects");
            wddict.Add("Velázquez, Diego", "Q297"); subj1dict.Add("Velázquez, Diego", "Biography"); subj2dict.Add("Velázquez, Diego", "Artists and architects");
            wddict.Add("Warhol, Andy", "Q5603"); subj1dict.Add("Warhol, Andy", "Biography"); subj2dict.Add("Warhol, Andy", "Artists and architects");
            wddict.Add("Wright, Frank Lloyd", "Q5604"); subj1dict.Add("Wright, Frank Lloyd", "Biography"); subj2dict.Add("Wright, Frank Lloyd", "Artists and architects");



            wddict.Add("Abu Nuwas", "Q5670"); subj1dict.Add("Abu Nuwas", "Biography"); subj2dict.Add("Abu Nuwas", "Authors, playwrights and poets");
            wddict.Add("Andersen, Hans Christian", "Q5673"); subj1dict.Add("Andersen, Hans Christian", "Biography"); subj2dict.Add("Andersen, Hans Christian", "Authors, playwrights and poets");
            wddict.Add("Bashō", "Q5676"); subj1dict.Add("Bashō", "Biography"); subj2dict.Add("Bashō", "Authors, playwrights and poets");
            //wddict.Add("Borges, Jorge Luis", "Q909"); subj1dict.Add("Borges, Jorge Luis", "Biography"); subj2dict.Add("Borges, Jorge Luis", "Authors, playwrights and poets");
            wddict.Add("Byron, George", "Q5679"); subj1dict.Add("Byron, George", "Biography"); subj2dict.Add("Byron, George", "Authors, playwrights and poets");
            wddict.Add("Cervantes, Miguel de", "Q5682"); subj1dict.Add("Cervantes, Miguel de", "Biography"); subj2dict.Add("Cervantes, Miguel de", "Authors, playwrights and poets");
            wddict.Add("Chaucer, Geoffrey", "Q5683"); subj1dict.Add("Chaucer, Geoffrey", "Biography"); subj2dict.Add("Chaucer, Geoffrey", "Authors, playwrights and poets");
            wddict.Add("Chekhov, Anton", "Q5685"); subj1dict.Add("Chekhov, Anton", "Biography"); subj2dict.Add("Chekhov, Anton", "Authors, playwrights and poets");
            wddict.Add("Dante Alighieri", "Q1067"); subj1dict.Add("Dante Alighieri", "Biography"); subj2dict.Add("Dante Alighieri", "Authors, playwrights and poets");
            wddict.Add("Dickens, Charles", "Q5686"); subj1dict.Add("Dickens, Charles", "Biography"); subj2dict.Add("Dickens, Charles", "Authors, playwrights and poets");
            wddict.Add("Dostoyevsky, Fyodor", "Q991"); subj1dict.Add("Dostoyevsky, Fyodor", "Biography"); subj2dict.Add("Dostoyevsky, Fyodor", "Authors, playwrights and poets");
            wddict.Add("García Márquez, Gabriel", "Q5878"); subj1dict.Add("García Márquez, Gabriel", "Biography"); subj2dict.Add("García Márquez, Gabriel", "Authors, playwrights and poets");
            wddict.Add("Goethe, Johann Wolfgang von", "Q5879"); subj1dict.Add("Goethe, Johann Wolfgang von", "Biography"); subj2dict.Add("Goethe, Johann Wolfgang von", "Authors, playwrights and poets");
            wddict.Add("Hafez", "Q6240"); subj1dict.Add("Hafez", "Biography"); subj2dict.Add("Hafez", "Authors, playwrights and poets");
            wddict.Add("Homer", "Q6691"); subj1dict.Add("Homer", "Biography"); subj2dict.Add("Homer", "Authors, playwrights and poets");
            wddict.Add("Hugo, Victor", "Q535"); subj1dict.Add("Hugo, Victor", "Biography"); subj2dict.Add("Hugo, Victor", "Authors, playwrights and poets");
            wddict.Add("Joyce, James", "Q6882"); subj1dict.Add("Joyce, James", "Biography"); subj2dict.Add("Joyce, James", "Authors, playwrights and poets");
            wddict.Add("Kafka, Franz", "Q905"); subj1dict.Add("Kafka, Franz", "Biography"); subj2dict.Add("Kafka, Franz", "Authors, playwrights and poets");
            wddict.Add("Kālidāsa", "Q7011"); subj1dict.Add("Kālidāsa", "Biography"); subj2dict.Add("Kālidāsa", "Authors, playwrights and poets");
            wddict.Add("Li Bai", "Q7071"); subj1dict.Add("Li Bai", "Biography"); subj2dict.Add("Li Bai", "Authors, playwrights and poets");
            wddict.Add("Naguib Mahfouz", "Q7176"); subj1dict.Add("Naguib Mahfouz", "Biography"); subj2dict.Add("Naguib Mahfouz", "Authors, playwrights and poets");
            wddict.Add("Molière", "Q687"); subj1dict.Add("Molière", "Biography"); subj2dict.Add("Molière", "Authors, playwrights and poets");
            wddict.Add("Ovid", "Q7198"); subj1dict.Add("Ovid", "Biography"); subj2dict.Add("Ovid", "Authors, playwrights and poets");
            wddict.Add("Proust, Marcel", "Q7199"); subj1dict.Add("Proust, Marcel", "Biography"); subj2dict.Add("Proust, Marcel", "Authors, playwrights and poets");
            wddict.Add("Pushkin, Alexander", "Q7200"); subj1dict.Add("Pushkin, Alexander", "Biography"); subj2dict.Add("Pushkin, Alexander", "Authors, playwrights and poets");
            wddict.Add("Shakespeare, William", "Q692"); subj1dict.Add("Shakespeare, William", "Biography"); subj2dict.Add("Shakespeare, William", "Authors, playwrights and poets");
            wddict.Add("Sophocles", "Q7235"); subj1dict.Add("Sophocles", "Biography"); subj2dict.Add("Sophocles", "Authors, playwrights and poets");
            wddict.Add("Tagore, Rabindranath", "Q7241"); subj1dict.Add("Tagore, Rabindranath", "Biography"); subj2dict.Add("Tagore, Rabindranath", "Authors, playwrights and poets");
            wddict.Add("Tolstoy, Leo", "Q7243"); subj1dict.Add("Tolstoy, Leo", "Biography"); subj2dict.Add("Tolstoy, Leo", "Authors, playwrights and poets");
            wddict.Add("Twain, Mark", "Q7245"); subj1dict.Add("Twain, Mark", "Biography"); subj2dict.Add("Twain, Mark", "Authors, playwrights and poets");
            wddict.Add("Virgil", "Q1398"); subj1dict.Add("Virgil", "Biography"); subj2dict.Add("Virgil", "Authors, playwrights and poets");



            wddict.Add("Armstrong, Louis", "Q1779"); subj1dict.Add("Armstrong, Louis", "Biography"); subj2dict.Add("Armstrong, Louis", "Composers and musicians");
            wddict.Add("Bach, Johann Sebastian", "Q1339"); subj1dict.Add("Bach, Johann Sebastian", "Biography"); subj2dict.Add("Bach, Johann Sebastian", "Composers and musicians");
            wddict.Add("Beatles, The", "Q1299"); subj1dict.Add("Beatles, The", "Biography"); subj2dict.Add("Beatles, The", "Composers and musicians");
            wddict.Add("Beethoven, Ludwig van", "Q255"); subj1dict.Add("Beethoven, Ludwig van", "Biography"); subj2dict.Add("Beethoven, Ludwig van", "Composers and musicians");
            wddict.Add("Brahms, Johannes", "Q7294"); subj1dict.Add("Brahms, Johannes", "Biography"); subj2dict.Add("Brahms, Johannes", "Composers and musicians");
            wddict.Add("Chopin, Frédéric", "Q1268"); subj1dict.Add("Chopin, Frédéric", "Biography"); subj2dict.Add("Chopin, Frédéric", "Composers and musicians");
            wddict.Add("Dvořák, Antonín", "Q7298"); subj1dict.Add("Dvořák, Antonín", "Biography"); subj2dict.Add("Dvořák, Antonín", "Composers and musicians");
            wddict.Add("Handel, Georg Frideric", "Q7302"); subj1dict.Add("Handel, Georg Frideric", "Biography"); subj2dict.Add("Handel, Georg Frideric", "Composers and musicians");
            wddict.Add("Haydn, Joseph", "Q7349"); subj1dict.Add("Haydn, Joseph", "Biography"); subj2dict.Add("Haydn, Joseph", "Composers and musicians");
            wddict.Add("Mahler, Gustav", "Q7304"); subj1dict.Add("Mahler, Gustav", "Biography"); subj2dict.Add("Mahler, Gustav", "Composers and musicians");
            wddict.Add("Mozart, Wolfgang Amadeus", "Q254"); subj1dict.Add("Mozart, Wolfgang Amadeus", "Biography"); subj2dict.Add("Mozart, Wolfgang Amadeus", "Composers and musicians");
            wddict.Add("Palestrina, Giovanni Pierluigi da", "Q179277"); subj1dict.Add("Palestrina, Giovanni Pierluigi da", "Biography"); subj2dict.Add("Palestrina, Giovanni Pierluigi da", "Composers and musicians");
            wddict.Add("Piaf, Édith", "Q1631"); subj1dict.Add("Piaf, Édith", "Biography"); subj2dict.Add("Piaf, Édith", "Composers and musicians");
            wddict.Add("Presley, Elvis", "Q303"); subj1dict.Add("Presley, Elvis", "Biography"); subj2dict.Add("Presley, Elvis", "Composers and musicians");
            wddict.Add("Puccini, Giacomo", "Q7311"); subj1dict.Add("Puccini, Giacomo", "Biography"); subj2dict.Add("Puccini, Giacomo", "Composers and musicians");
            wddict.Add("Schubert, Franz", "Q7312"); subj1dict.Add("Schubert, Franz", "Biography"); subj2dict.Add("Schubert, Franz", "Composers and musicians");
            wddict.Add("Stravinsky, Igor", "Q7314"); subj1dict.Add("Stravinsky, Igor", "Biography"); subj2dict.Add("Stravinsky, Igor", "Composers and musicians");
            wddict.Add("Tchaikovsky, Petr", "Q7315"); subj1dict.Add("Tchaikovsky, Petr", "Biography"); subj2dict.Add("Tchaikovsky, Petr", "Composers and musicians");
            wddict.Add("Verdi, Giuseppe", "Q7317"); subj1dict.Add("Verdi, Giuseppe", "Biography"); subj2dict.Add("Verdi, Giuseppe", "Composers and musicians");
            wddict.Add("Vivaldi, Antonio", "Q1340"); subj1dict.Add("Vivaldi, Antonio", "Biography"); subj2dict.Add("Vivaldi, Antonio", "Composers and musicians");
            wddict.Add("Wagner, Richard", "Q1511"); subj1dict.Add("Wagner, Richard", "Biography"); subj2dict.Add("Wagner, Richard", "Composers and musicians");



            wddict.Add("Amundsen, Roald", "Q926"); subj1dict.Add("Amundsen, Roald", "Biography"); subj2dict.Add("Amundsen, Roald", "Explorers and travelers");
            wddict.Add("Armstrong, Neil", "Q1615"); subj1dict.Add("Armstrong, Neil", "Biography"); subj2dict.Add("Armstrong, Neil", "Explorers and travelers");
            wddict.Add("Cartier, Jacques", "Q7321"); subj1dict.Add("Cartier, Jacques", "Biography"); subj2dict.Add("Cartier, Jacques", "Explorers and travelers");
            wddict.Add("Columbus, Christopher", "Q7322"); subj1dict.Add("Columbus, Christopher", "Biography"); subj2dict.Add("Columbus, Christopher", "Explorers and travelers");
            wddict.Add("Cook, James", "Q7324"); subj1dict.Add("Cook, James", "Biography"); subj2dict.Add("Cook, James", "Explorers and travelers");
            wddict.Add("Cortés, Hernán", "Q7326"); subj1dict.Add("Cortés, Hernán", "Biography"); subj2dict.Add("Cortés, Hernán", "Explorers and travelers");
            wddict.Add("Gagarin, Yuri", "Q7327"); subj1dict.Add("Gagarin, Yuri", "Biography"); subj2dict.Add("Gagarin, Yuri", "Explorers and travelers");
            wddict.Add("da Gama, Vasco", "Q7328"); subj1dict.Add("da Gama, Vasco", "Biography"); subj2dict.Add("da Gama, Vasco", "Explorers and travelers");
            wddict.Add("Ibn Battuta", "Q7331"); subj1dict.Add("Ibn Battuta", "Biography"); subj2dict.Add("Ibn Battuta", "Explorers and travelers");
            wddict.Add("Magellan, Ferdinand", "Q1496"); subj1dict.Add("Magellan, Ferdinand", "Biography"); subj2dict.Add("Magellan, Ferdinand", "Explorers and travelers");
            wddict.Add("Polo, Marco", "Q6101"); subj1dict.Add("Polo, Marco", "Biography"); subj2dict.Add("Polo, Marco", "Explorers and travelers");
            wddict.Add("Zheng He", "Q7333"); subj1dict.Add("Zheng He", "Biography"); subj2dict.Add("Zheng He", "Explorers and travelers");



            wddict.Add("Bergman, Ingmar", "Q7546"); subj1dict.Add("Bergman, Ingmar", "Biography"); subj2dict.Add("Bergman, Ingmar", "Film directors and screenwriters");
            wddict.Add("Disney, Walt", "Q8704"); subj1dict.Add("Disney, Walt", "Biography"); subj2dict.Add("Disney, Walt", "Film directors and screenwriters");
            wddict.Add("Eisenstein, Sergei", "Q8003"); subj1dict.Add("Eisenstein, Sergei", "Biography"); subj2dict.Add("Eisenstein, Sergei", "Film directors and screenwriters");
            wddict.Add("Fellini, Federico", "Q7371"); subj1dict.Add("Fellini, Federico", "Biography"); subj2dict.Add("Fellini, Federico", "Film directors and screenwriters");
            wddict.Add("Hitchcock, Alfred", "Q7374"); subj1dict.Add("Hitchcock, Alfred", "Biography"); subj2dict.Add("Hitchcock, Alfred", "Film directors and screenwriters");
            wddict.Add("Kubrick, Stanley", "Q2001"); subj1dict.Add("Kubrick, Stanley", "Biography"); subj2dict.Add("Kubrick, Stanley", "Film directors and screenwriters");
            wddict.Add("Kurosawa, Akira", "Q8006"); subj1dict.Add("Kurosawa, Akira", "Biography"); subj2dict.Add("Kurosawa, Akira", "Film directors and screenwriters");
            wddict.Add("Ray, Satyajit", "Q8873"); subj1dict.Add("Ray, Satyajit", "Biography"); subj2dict.Add("Ray, Satyajit", "Film directors and screenwriters");
            wddict.Add("Spielberg, Steven", "Q8877"); subj1dict.Add("Spielberg, Steven", "Biography"); subj2dict.Add("Spielberg, Steven", "Film directors and screenwriters");



            wddict.Add("Archimedes", "Q8739"); subj1dict.Add("Archimedes", "Biography"); subj2dict.Add("Archimedes", "Inventors, scientists and mathematicians");
            wddict.Add("Avicenna", "Q8011"); subj1dict.Add("Avicenna", "Biography"); subj2dict.Add("Avicenna", "Inventors, scientists and mathematicians");
            wddict.Add("Berners-Lee, Tim", "Q80"); subj1dict.Add("Berners-Lee, Tim", "Biography"); subj2dict.Add("Berners-Lee, Tim", "Inventors, scientists and mathematicians");
            wddict.Add("Brahmagupta", "Q202943"); subj1dict.Add("Brahmagupta", "Biography"); subj2dict.Add("Brahmagupta", "Inventors, scientists and mathematicians");
            wddict.Add("Copernicus, Nicolaus", "Q619"); subj1dict.Add("Copernicus, Nicolaus", "Biography"); subj2dict.Add("Copernicus, Nicolaus", "Inventors, scientists and mathematicians");
            wddict.Add("Curie, Marie", "Q7186"); subj1dict.Add("Curie, Marie", "Biography"); subj2dict.Add("Curie, Marie", "Inventors, scientists and mathematicians");
            wddict.Add("Darwin, Charles", "Q1035"); subj1dict.Add("Darwin, Charles", "Biography"); subj2dict.Add("Darwin, Charles", "Inventors, scientists and mathematicians");
            wddict.Add("Edison, Thomas", "Q8743"); subj1dict.Add("Edison, Thomas", "Biography"); subj2dict.Add("Edison, Thomas", "Inventors, scientists and mathematicians");
            wddict.Add("Einstein, Albert", "Q937"); subj1dict.Add("Einstein, Albert", "Biography"); subj2dict.Add("Einstein, Albert", "Inventors, scientists and mathematicians");
            wddict.Add("Euclid", "Q8747"); subj1dict.Add("Euclid", "Biography"); subj2dict.Add("Euclid", "Inventors, scientists and mathematicians");
            wddict.Add("Euler, Leonhard", "Q7604"); subj1dict.Add("Euler, Leonhard", "Biography"); subj2dict.Add("Euler, Leonhard", "Inventors, scientists and mathematicians");
            wddict.Add("Faraday, Michael", "Q8750"); subj1dict.Add("Faraday, Michael", "Biography"); subj2dict.Add("Faraday, Michael", "Inventors, scientists and mathematicians");
            wddict.Add("Fermi, Enrico", "Q8753"); subj1dict.Add("Fermi, Enrico", "Biography"); subj2dict.Add("Fermi, Enrico", "Inventors, scientists and mathematicians");
            wddict.Add("Ford, Henry", "Q8768"); subj1dict.Add("Ford, Henry", "Biography"); subj2dict.Add("Ford, Henry", "Inventors, scientists and mathematicians");
            wddict.Add("Galen", "Q8778"); subj1dict.Add("Galen", "Biography"); subj2dict.Add("Galen", "Inventors, scientists and mathematicians");
            wddict.Add("Galileo Galilei", "Q307"); subj1dict.Add("Galileo Galilei", "Biography"); subj2dict.Add("Galileo Galilei", "Inventors, scientists and mathematicians");
            wddict.Add("Gauss, Carl Friedrich", "Q6722"); subj1dict.Add("Gauss, Carl Friedrich", "Biography"); subj2dict.Add("Gauss, Carl Friedrich", "Inventors, scientists and mathematicians");
            wddict.Add("Gutenberg, Johannes", "Q8958"); subj1dict.Add("Gutenberg, Johannes", "Biography"); subj2dict.Add("Gutenberg, Johannes", "Inventors, scientists and mathematicians");
            wddict.Add("Hilbert, David", "Q41585"); subj1dict.Add("Hilbert, David", "Biography"); subj2dict.Add("Hilbert, David", "Inventors, scientists and mathematicians");
            wddict.Add("Joule, James Prescott", "Q8962"); subj1dict.Add("Joule, James Prescott", "Biography"); subj2dict.Add("Joule, James Prescott", "Inventors, scientists and mathematicians");
            wddict.Add("Kepler, Johannes", "Q8963"); subj1dict.Add("Kepler, Johannes", "Biography"); subj2dict.Add("Kepler, Johannes", "Inventors, scientists and mathematicians");
            wddict.Add("al-Khwarizmi, Muhammad ibn Musa", "Q9038"); subj1dict.Add("al-Khwarizmi, Muhammad ibn Musa", "Biography"); subj2dict.Add("al-Khwarizmi, Muhammad ibn Musa", "Inventors, scientists and mathematicians");
            wddict.Add("Leibniz, Gottfried Wilhelm", "Q9047"); subj1dict.Add("Leibniz, Gottfried Wilhelm", "Biography"); subj2dict.Add("Leibniz, Gottfried Wilhelm", "Inventors, scientists and mathematicians");
            wddict.Add("Linnaeus, Carl", "Q1043"); subj1dict.Add("Linnaeus, Carl", "Biography"); subj2dict.Add("Linnaeus, Carl", "Inventors, scientists and mathematicians");
            wddict.Add("Maxwell, James Clerk", "Q9095"); subj1dict.Add("Maxwell, James Clerk", "Biography"); subj2dict.Add("Maxwell, James Clerk", "Inventors, scientists and mathematicians");
            wddict.Add("Mendeleev, Dmitri", "Q9106"); subj1dict.Add("Mendeleev, Dmitri", "Biography"); subj2dict.Add("Mendeleev, Dmitri", "Inventors, scientists and mathematicians");
            wddict.Add("Newton, Sir Isaac", "Q935"); subj1dict.Add("Newton, Sir Isaac", "Biography"); subj2dict.Add("Newton, Sir Isaac", "Inventors, scientists and mathematicians");
            wddict.Add("Pasteur, Louis", "Q529"); subj1dict.Add("Pasteur, Louis", "Biography"); subj2dict.Add("Pasteur, Louis", "Inventors, scientists and mathematicians");
            wddict.Add("Planck, Max", "Q9021"); subj1dict.Add("Planck, Max", "Biography"); subj2dict.Add("Planck, Max", "Inventors, scientists and mathematicians");
            wddict.Add("Rutherford, Ernest", "Q9123"); subj1dict.Add("Rutherford, Ernest", "Biography"); subj2dict.Add("Rutherford, Ernest", "Inventors, scientists and mathematicians");
            wddict.Add("Schrödinger, Erwin", "Q9130"); subj1dict.Add("Schrödinger, Erwin", "Biography"); subj2dict.Add("Schrödinger, Erwin", "Inventors, scientists and mathematicians");
            wddict.Add("Tesla, Nikola", "Q9036"); subj1dict.Add("Tesla, Nikola", "Biography"); subj2dict.Add("Tesla, Nikola", "Inventors, scientists and mathematicians");
            wddict.Add("Turing, Alan", "Q7251"); subj1dict.Add("Turing, Alan", "Biography"); subj2dict.Add("Turing, Alan", "Inventors, scientists and mathematicians");
            wddict.Add("Watt, James", "Q9041"); subj1dict.Add("Watt, James", "Biography"); subj2dict.Add("Watt, James", "Inventors, scientists and mathematicians");


            wddict.Add("Aristotle", "Q868"); subj1dict.Add("Aristotle", "Biography"); subj2dict.Add("Aristotle", "Philosophers and social scientists");
            wddict.Add("Beauvoir, Simone de", "Q7197"); subj1dict.Add("Beauvoir, Simone de", "Biography"); subj2dict.Add("Beauvoir, Simone de", "Philosophers and social scientists");
            wddict.Add("Chanakya", "Q9045"); subj1dict.Add("Chanakya", "Biography"); subj2dict.Add("Chanakya", "Philosophers and social scientists");
            wddict.Add("Chomsky, Noam", "Q9049"); subj1dict.Add("Chomsky, Noam", "Biography"); subj2dict.Add("Chomsky, Noam", "Philosophers and social scientists");
            wddict.Add("Confucius", "Q4604"); subj1dict.Add("Confucius", "Biography"); subj2dict.Add("Confucius", "Philosophers and social scientists");
            wddict.Add("Descartes, René", "Q9191"); subj1dict.Add("Descartes, René", "Biography"); subj2dict.Add("Descartes, René", "Philosophers and social scientists");
            wddict.Add("Freud, Sigmund", "Q9215"); subj1dict.Add("Freud, Sigmund", "Biography"); subj2dict.Add("Freud, Sigmund", "Philosophers and social scientists");
            wddict.Add("Hegel, Georg Wilhelm Friedrich", "Q9235"); subj1dict.Add("Hegel, Georg Wilhelm Friedrich", "Biography"); subj2dict.Add("Hegel, Georg Wilhelm Friedrich", "Philosophers and social scientists");
            wddict.Add("Ibn Khaldun", "Q9294"); subj1dict.Add("Ibn Khaldun", "Biography"); subj2dict.Add("Ibn Khaldun", "Philosophers and social scientists");
            wddict.Add("Kant, Immanuel", "Q9312"); subj1dict.Add("Kant, Immanuel", "Biography"); subj2dict.Add("Kant, Immanuel", "Philosophers and social scientists");
            wddict.Add("Keynes, John Maynard", "Q9317"); subj1dict.Add("Keynes, John Maynard", "Biography"); subj2dict.Add("Keynes, John Maynard", "Philosophers and social scientists");
            wddict.Add("Laozi", "Q9333"); subj1dict.Add("Laozi", "Biography"); subj2dict.Add("Laozi", "Philosophers and social scientists");
            wddict.Add("Locke, John", "Q9353"); subj1dict.Add("Locke, John", "Biography"); subj2dict.Add("Locke, John", "Philosophers and social scientists");
            wddict.Add("Machiavelli, Niccolò", "Q1399"); subj1dict.Add("Machiavelli, Niccolò", "Biography"); subj2dict.Add("Machiavelli, Niccolò", "Philosophers and social scientists");
            wddict.Add("Marx, Karl", "Q9061"); subj1dict.Add("Marx, Karl", "Biography"); subj2dict.Add("Marx, Karl", "Philosophers and social scientists");
            wddict.Add("Nietzsche, Friedrich", "Q9358"); subj1dict.Add("Nietzsche, Friedrich", "Biography"); subj2dict.Add("Nietzsche, Friedrich", "Philosophers and social scientists");
            wddict.Add("Plato", "Q859"); subj1dict.Add("Plato", "Biography"); subj2dict.Add("Plato", "Philosophers and social scientists");
            wddict.Add("Rousseau, Jean-Jacques", "Q6527"); subj1dict.Add("Rousseau, Jean-Jacques", "Biography"); subj2dict.Add("Rousseau, Jean-Jacques", "Philosophers and social scientists");
            wddict.Add("Sartre, Jean-Paul", "Q9364"); subj1dict.Add("Sartre, Jean-Paul", "Biography"); subj2dict.Add("Sartre, Jean-Paul", "Philosophers and social scientists");
            wddict.Add("Sima Qian", "Q9372"); subj1dict.Add("Sima Qian", "Biography"); subj2dict.Add("Sima Qian", "Philosophers and social scientists");
            wddict.Add("Smith, Adam", "Q9381"); subj1dict.Add("Smith, Adam", "Biography"); subj2dict.Add("Smith, Adam", "Philosophers and social scientists");
            wddict.Add("Socrates", "Q913"); subj1dict.Add("Socrates", "Biography"); subj2dict.Add("Socrates", "Philosophers and social scientists");
            wddict.Add("Voltaire", "Q9068"); subj1dict.Add("Voltaire", "Biography"); subj2dict.Add("Voltaire", "Philosophers and social scientists");
            wddict.Add("Weber, Max", "Q9387"); subj1dict.Add("Weber, Max", "Biography"); subj2dict.Add("Weber, Max", "Philosophers and social scientists");
            wddict.Add("Wittgenstein, Ludwig", "Q9391"); subj1dict.Add("Wittgenstein, Ludwig", "Biography"); subj2dict.Add("Wittgenstein, Ludwig", "Philosophers and social scientists");
            wddict.Add("Zhu Xi", "Q9397"); subj1dict.Add("Zhu Xi", "Biography"); subj2dict.Add("Zhu Xi", "Philosophers and social scientists");



            wddict.Add("Akbar", "Q8597"); subj1dict.Add("Akbar", "Biography"); subj2dict.Add("Akbar", "Political leaders");
            wddict.Add("Alexander the Great", "Q8409"); subj1dict.Add("Alexander the Great", "Biography"); subj2dict.Add("Alexander the Great", "Political leaders");
            wddict.Add("Ashoka", "Q8589"); subj1dict.Add("Ashoka", "Biography"); subj2dict.Add("Ashoka", "Political leaders");
            wddict.Add("Atatürk, Mustafa Kemal", "Q5152"); subj1dict.Add("Atatürk, Mustafa Kemal", "Biography"); subj2dict.Add("Atatürk, Mustafa Kemal", "Political leaders");
            wddict.Add("Augustus", "Q1405"); subj1dict.Add("Augustus", "Biography"); subj2dict.Add("Augustus", "Political leaders");
            wddict.Add("von Bismarck, Otto", "Q8442"); subj1dict.Add("von Bismarck, Otto", "Biography"); subj2dict.Add("von Bismarck, Otto", "Political leaders");
            wddict.Add("Bolívar, Simón", "Q8605"); subj1dict.Add("Bolívar, Simón", "Biography"); subj2dict.Add("Bolívar, Simón", "Political leaders");
            wddict.Add("Bonaparte, Napoleon", "Q517"); subj1dict.Add("Bonaparte, Napoleon", "Biography"); subj2dict.Add("Bonaparte, Napoleon", "Political leaders");
            wddict.Add("Caesar, Julius", "Q1048"); subj1dict.Add("Caesar, Julius", "Biography"); subj2dict.Add("Caesar, Julius", "Political leaders");
            wddict.Add("Charlemagne", "Q3044"); subj1dict.Add("Charlemagne", "Biography"); subj2dict.Add("Charlemagne", "Political leaders");
            wddict.Add("Churchill, Winston", "Q8016"); subj1dict.Add("Churchill, Winston", "Biography"); subj2dict.Add("Churchill, Winston", "Political leaders");
            wddict.Add("Constantine the Great", "Q8413"); subj1dict.Add("Constantine the Great", "Biography"); subj2dict.Add("Constantine the Great", "Political leaders");
            wddict.Add("Cyrus the Great", "Q8423"); subj1dict.Add("Cyrus the Great", "Biography"); subj2dict.Add("Cyrus the Great", "Political leaders");
            wddict.Add("de Gaulle, Charles", "Q2042"); subj1dict.Add("de Gaulle, Charles", "Biography"); subj2dict.Add("de Gaulle, Charles", "Political leaders");
            wddict.Add("Elizabeth I of England", "Q7207"); subj1dict.Add("Elizabeth I of England", "Biography"); subj2dict.Add("Elizabeth I of England", "Political leaders");
            wddict.Add("Gandhi, Mohandas Karamchand", "Q1001"); subj1dict.Add("Gandhi, Mohandas Karamchand", "Biography"); subj2dict.Add("Gandhi, Mohandas Karamchand", "Political leaders");
            wddict.Add("Genghis Khan", "Q720"); subj1dict.Add("Genghis Khan", "Biography"); subj2dict.Add("Genghis Khan", "Political leaders");
            wddict.Add("Guevara, Che", "Q5809"); subj1dict.Add("Guevara, Che", "Biography"); subj2dict.Add("Guevara, Che", "Political leaders");
            wddict.Add("Hitler, Adolf", "Q352"); subj1dict.Add("Hitler, Adolf", "Biography"); subj2dict.Add("Hitler, Adolf", "Political leaders");
            wddict.Add("Joan of Arc", "Q7226"); subj1dict.Add("Joan of Arc", "Biography"); subj2dict.Add("Joan of Arc", "Political leaders");
            wddict.Add("King, Martin Luther, Jr.", "Q8027"); subj1dict.Add("King, Martin Luther, Jr.", "Biography"); subj2dict.Add("King, Martin Luther, Jr.", "Political leaders");
            wddict.Add("Lenin, Vladimir", "Q1394"); subj1dict.Add("Lenin, Vladimir", "Biography"); subj2dict.Add("Lenin, Vladimir", "Political leaders");
            wddict.Add("Lincoln, Abraham", "Q91"); subj1dict.Add("Lincoln, Abraham", "Biography"); subj2dict.Add("Lincoln, Abraham", "Political leaders");
            wddict.Add("Louis XIV", "Q7742"); subj1dict.Add("Louis XIV", "Biography"); subj2dict.Add("Louis XIV", "Political leaders");
            wddict.Add("Luxemburg, Rosa", "Q7231"); subj1dict.Add("Luxemburg, Rosa", "Biography"); subj2dict.Add("Luxemburg, Rosa", "Political leaders");
            wddict.Add("Mandela, Nelson", "Q8023"); subj1dict.Add("Mandela, Nelson", "Biography"); subj2dict.Add("Mandela, Nelson", "Political leaders");
            wddict.Add("Mao Zedong", "Q5816"); subj1dict.Add("Mao Zedong", "Biography"); subj2dict.Add("Mao Zedong", "Political leaders");
            wddict.Add("Nehru, Jawaharlal", "Q1047"); subj1dict.Add("Nehru, Jawaharlal", "Biography"); subj2dict.Add("Nehru, Jawaharlal", "Political leaders");
            wddict.Add("Nkrumah, Kwame", "Q8620"); subj1dict.Add("Nkrumah, Kwame", "Biography"); subj2dict.Add("Nkrumah, Kwame", "Political leaders");
            wddict.Add("Peter the Great", "Q8479"); subj1dict.Add("Peter the Great", "Biography"); subj2dict.Add("Peter the Great", "Political leaders");
            wddict.Add("Qin Shi Huang", "Q7192"); subj1dict.Add("Qin Shi Huang", "Biography"); subj2dict.Add("Qin Shi Huang", "Political leaders");
            wddict.Add("Roosevelt, Franklin D.", "Q8007"); subj1dict.Add("Roosevelt, Franklin D.", "Biography"); subj2dict.Add("Roosevelt, Franklin D.", "Political leaders");
            wddict.Add("Saladin", "Q8581"); subj1dict.Add("Saladin", "Biography"); subj2dict.Add("Saladin", "Political leaders");
            wddict.Add("Stalin, Joseph", "Q855"); subj1dict.Add("Stalin, Joseph", "Biography"); subj2dict.Add("Stalin, Joseph", "Political leaders");
            wddict.Add("Suleiman the Magnificent", "Q8474"); subj1dict.Add("Suleiman the Magnificent", "Biography"); subj2dict.Add("Suleiman the Magnificent", "Political leaders");
            wddict.Add("Sun Yat-sen", "Q8573"); subj1dict.Add("Sun Yat-sen", "Biography"); subj2dict.Add("Sun Yat-sen", "Political leaders");
            wddict.Add("Tamerlane", "Q8462"); subj1dict.Add("Tamerlane", "Biography"); subj2dict.Add("Tamerlane", "Political leaders");
            wddict.Add("Umar", "Q8467"); subj1dict.Add("Umar", "Biography"); subj2dict.Add("Umar", "Political leaders");
            wddict.Add("Washington, George", "Q23"); subj1dict.Add("Washington, George", "Biography"); subj2dict.Add("Washington, George", "Political leaders");



            wddict.Add("Abraham", "Q9181"); subj1dict.Add("Abraham", "Biography"); subj2dict.Add("Abraham", "Religious figures and theologians");
            wddict.Add("Aquinas, Thomas", "Q9438"); subj1dict.Add("Aquinas, Thomas", "Biography"); subj2dict.Add("Aquinas, Thomas", "Religious figures and theologians");
            wddict.Add("Augustine of Hippo", "Q8018"); subj1dict.Add("Augustine of Hippo", "Biography"); subj2dict.Add("Augustine of Hippo", "Religious figures and theologians");
            wddict.Add("Buddha", "Q9441"); subj1dict.Add("Buddha", "Biography"); subj2dict.Add("Buddha", "Religious figures and theologians");
            wddict.Add("Al-Ghazali", "Q9546"); subj1dict.Add("Al-Ghazali", "Biography"); subj2dict.Add("Al-Ghazali", "Religious figures and theologians");
            wddict.Add("Jesus", "Q302"); subj1dict.Add("Jesus", "Biography"); subj2dict.Add("Jesus", "Religious figures and theologians");
            wddict.Add("Luther, Martin", "Q9554"); subj1dict.Add("Luther, Martin", "Biography"); subj2dict.Add("Luther, Martin", "Religious figures and theologians");
            wddict.Add("Moses", "Q9077"); subj1dict.Add("Moses", "Biography"); subj2dict.Add("Moses", "Religious figures and theologians");
            wddict.Add("Muhammad", "Q9458"); subj1dict.Add("Muhammad", "Biography"); subj2dict.Add("Muhammad", "Religious figures and theologians");
            wddict.Add("Paul the Apostle", "Q9200"); subj1dict.Add("Paul the Apostle", "Biography"); subj2dict.Add("Paul the Apostle", "Religious figures and theologians");





            wddict.Add("Beauty", "Q7242"); subj1dict.Add("Beauty", "Philosophy and psychology"); subj2dict.Add("Beauty", "Philosophy");
            wddict.Add("Dialectic", "Q9453"); subj1dict.Add("Dialectic", "Philosophy and psychology"); subj2dict.Add("Dialectic", "Philosophy");
            wddict.Add("Ethics", "Q9465"); subj1dict.Add("Ethics", "Philosophy and psychology"); subj2dict.Add("Ethics", "Philosophy");
            wddict.Add("Epistemology", "Q9471"); subj1dict.Add("Epistemology", "Philosophy and psychology"); subj2dict.Add("Epistemology", "Philosophy");
            wddict.Add("Feminism", "Q7252"); subj1dict.Add("Feminism", "Philosophy and psychology"); subj2dict.Add("Feminism", "Philosophy");
            wddict.Add("Free will", "Q9476"); subj1dict.Add("Free will", "Philosophy and psychology"); subj2dict.Add("Free will", "Philosophy");
            wddict.Add("Knowledge", "Q9081"); subj1dict.Add("Knowledge", "Philosophy and psychology"); subj2dict.Add("Knowledge", "Philosophy");
            wddict.Add("Logic", "Q8078"); subj1dict.Add("Logic", "Philosophy and psychology"); subj2dict.Add("Logic", "Philosophy");
            wddict.Add("Mind", "Q450"); subj1dict.Add("Mind", "Philosophy and psychology"); subj2dict.Add("Mind", "Philosophy");
            wddict.Add("Philosophy", "Q5891"); subj1dict.Add("Philosophy", "Philosophy and psychology"); subj2dict.Add("Philosophy", "Philosophy");
            wddict.Add("Probability", "Q9492"); subj1dict.Add("Probability", "Philosophy and psychology"); subj2dict.Add("Probability", "Philosophy");
            wddict.Add("Reality", "Q9510"); subj1dict.Add("Reality", "Philosophy and psychology"); subj2dict.Add("Reality", "Philosophy");
            wddict.Add("Truth", "Q7949"); subj1dict.Add("Truth", "Philosophy and psychology"); subj2dict.Add("Truth", "Philosophy");


            wddict.Add("Behavior", "Q9332"); subj1dict.Add("Behavior", "Philosophy and psychology"); subj2dict.Add("Behavior", "Psychology");
            wddict.Add("Emotion", "Q9415"); subj1dict.Add("Emotion", "Philosophy and psychology"); subj2dict.Add("Emotion", "Psychology");
            wddict.Add("Love", "Q316"); subj1dict.Add("Love", "Philosophy and psychology"); subj2dict.Add("Love", "Psychology");
            wddict.Add("Psychology", "Q9418"); subj1dict.Add("Psychology", "Philosophy and psychology"); subj2dict.Add("Psychology", "Psychology");
            wddict.Add("Thought", "Q9420"); subj1dict.Add("Thought", "Philosophy and psychology"); subj2dict.Add("Thought", "Psychology");





            wddict.Add("God", "Q190"); subj1dict.Add("God", "Religion"); subj2dict.Add("God", "World view and religion");
            wddict.Add("Mythology", "Q9134"); subj1dict.Add("Mythology", "Religion"); subj2dict.Add("Mythology", "World view and religion");

            wddict.Add("Atheism", "Q7066"); subj1dict.Add("Atheism", "Religion"); subj2dict.Add("Atheism", "World view and religion");
            wddict.Add("Fundamentalism", "Q9149"); subj1dict.Add("Fundamentalism", "Religion"); subj2dict.Add("Fundamentalism", "World view and religion");
            wddict.Add("Materialism", "Q7081"); subj1dict.Add("Materialism", "Religion"); subj2dict.Add("Materialism", "World view and religion");
            wddict.Add("Monotheism", "Q9159"); subj1dict.Add("Monotheism", "Religion"); subj2dict.Add("Monotheism", "World view and religion");
            wddict.Add("Polytheism", "Q9163"); subj1dict.Add("Polytheism", "Religion"); subj2dict.Add("Polytheism", "World view and religion");
            wddict.Add("Soul", "Q9165"); subj1dict.Add("Soul", "Religion"); subj2dict.Add("Soul", "World view and religion");
            wddict.Add("Religion", "Q9174"); subj1dict.Add("Religion", "Religion"); subj2dict.Add("Religion", "World view and religion");

            wddict.Add("Buddhism", "Q748"); subj1dict.Add("Buddhism", "Religion"); subj2dict.Add("Buddhism", "World view and religion");
            wddict.Add("Christianity", "Q5043"); subj1dict.Add("Christianity", "Religion"); subj2dict.Add("Christianity", "World view and religion");
            wddict.Add("Catholic Church", "Q9592"); subj1dict.Add("Catholic Church", "Religion"); subj2dict.Add("Catholic Church", "World view and religion");
            wddict.Add("Confucianism", "Q9581"); subj1dict.Add("Confucianism", "Religion"); subj2dict.Add("Confucianism", "World view and religion");
            wddict.Add("Hinduism", "Q9089"); subj1dict.Add("Hinduism", "Religion"); subj2dict.Add("Hinduism", "World view and religion");
            wddict.Add("Trimurti", "Q9595"); subj1dict.Add("Trimurti", "Religion"); subj2dict.Add("Trimurti", "World view and religion");
            wddict.Add("Islam", "Q432"); subj1dict.Add("Islam", "Religion"); subj2dict.Add("Islam", "World view and religion");
            wddict.Add("Shia Islam", "Q9585"); subj1dict.Add("Shia Islam", "Religion"); subj2dict.Add("Shia Islam", "World view and religion");
            wddict.Add("Jainism", "Q9232"); subj1dict.Add("Jainism", "Religion"); subj2dict.Add("Jainism", "World view and religion");
            wddict.Add("Judaism", "Q9268"); subj1dict.Add("Judaism", "Religion"); subj2dict.Add("Judaism", "World view and religion");
            wddict.Add("Sikhism", "Q9316"); subj1dict.Add("Sikhism", "Religion"); subj2dict.Add("Sikhism", "World view and religion");
            wddict.Add("Taoism", "Q9598"); subj1dict.Add("Taoism", "Religion"); subj2dict.Add("Taoism", "World view and religion");
            wddict.Add("Zoroastrianism", "Q9601"); subj1dict.Add("Zoroastrianism", "Religion"); subj2dict.Add("Zoroastrianism", "World view and religion");

            wddict.Add("Sufism", "Q9603"); subj1dict.Add("Sufism", "Religion"); subj2dict.Add("Sufism", "World view and religion");
            wddict.Add("Yoga", "Q9350"); subj1dict.Add("Yoga", "Religion"); subj2dict.Add("Yoga", "World view and religion");
            wddict.Add("Zen", "Q7953"); subj1dict.Add("Zen", "Religion"); subj2dict.Add("Zen", "World view and religion");



            wddict.Add("Society", "Q8425"); subj1dict.Add("Society", "Social sciences"); subj2dict.Add("Society", "Social sciences");
            wddict.Add("Civilization", "Q8432"); subj1dict.Add("Civilization", "Social sciences"); subj2dict.Add("Civilization", "Social sciences");
            wddict.Add("Education", "Q8434"); subj1dict.Add("Education", "Social sciences"); subj2dict.Add("Education", "Social sciences");



            wddict.Add("Family", "Q8436"); subj1dict.Add("Family", "Social sciences"); subj2dict.Add("Family", "Family and relationships");
            wddict.Add("Child", "Q7569"); subj1dict.Add("Child", "Social sciences"); subj2dict.Add("Child", "Family and relationships");
            wddict.Add("Man", "Q8441"); subj1dict.Add("Man", "Social sciences"); subj2dict.Add("Man", "Family and relationships");
            wddict.Add("Marriage", "Q8445"); subj1dict.Add("Marriage", "Social sciences"); subj2dict.Add("Marriage", "Family and relationships");
            wddict.Add("Woman", "Q467"); subj1dict.Add("Woman", "Social sciences"); subj2dict.Add("Woman", "Family and relationships");



            wddict.Add("Politics", "Q7163"); subj1dict.Add("Politics", "Social sciences"); subj2dict.Add("Politics", "Politics");
            wddict.Add("Anarchism", "Q6199"); subj1dict.Add("Anarchism", "Social sciences"); subj2dict.Add("Anarchism", "Politics");
            wddict.Add("Colonialism", "Q7167"); subj1dict.Add("Colonialism", "Social sciences"); subj2dict.Add("Colonialism", "Politics");
            wddict.Add("Communism", "Q6186"); subj1dict.Add("Communism", "Social sciences"); subj2dict.Add("Communism", "Politics");
            wddict.Add("Conservatism", "Q7169"); subj1dict.Add("Conservatism", "Social sciences"); subj2dict.Add("Conservatism", "Politics");
            wddict.Add("Democracy", "Q7174"); subj1dict.Add("Democracy", "Social sciences"); subj2dict.Add("Democracy", "Politics");
            wddict.Add("Dictatorship", "Q317"); subj1dict.Add("Dictatorship", "Social sciences"); subj2dict.Add("Dictatorship", "Politics");
            wddict.Add("Diplomacy", "Q1889"); subj1dict.Add("Diplomacy", "Social sciences"); subj2dict.Add("Diplomacy", "Politics");
            wddict.Add("Fascism", "Q6223"); subj1dict.Add("Fascism", "Social sciences"); subj2dict.Add("Fascism", "Politics");
            wddict.Add("Globalization", "Q7181"); subj1dict.Add("Globalization", "Social sciences"); subj2dict.Add("Globalization", "Politics");
            wddict.Add("Government", "Q7188"); subj1dict.Add("Government", "Social sciences"); subj2dict.Add("Government", "Politics");
            wddict.Add("Ideology", "Q7257"); subj1dict.Add("Ideology", "Social sciences"); subj2dict.Add("Ideology", "Politics");
            wddict.Add("Imperialism", "Q7260"); subj1dict.Add("Imperialism", "Social sciences"); subj2dict.Add("Imperialism", "Politics");
            wddict.Add("Liberalism", "Q6216"); subj1dict.Add("Liberalism", "Social sciences"); subj2dict.Add("Liberalism", "Politics");
            wddict.Add("Marxism", "Q7264"); subj1dict.Add("Marxism", "Social sciences"); subj2dict.Add("Marxism", "Politics");
            wddict.Add("Monarchy", "Q7269"); subj1dict.Add("Monarchy", "Social sciences"); subj2dict.Add("Monarchy", "Politics");
            wddict.Add("Nationalism", "Q6235"); subj1dict.Add("Nationalism", "Social sciences"); subj2dict.Add("Nationalism", "Politics");
            wddict.Add("Republic", "Q7270"); subj1dict.Add("Republic", "Social sciences"); subj2dict.Add("Republic", "Politics");
            wddict.Add("Socialism", "Q7272"); subj1dict.Add("Socialism", "Social sciences"); subj2dict.Add("Socialism", "Politics");
            wddict.Add("State (polity)", "Q7275"); subj1dict.Add("State (polity)", "Social sciences"); subj2dict.Add("State (polity)", "Politics");
            wddict.Add("Political party", "Q7278"); subj1dict.Add("Political party", "Social sciences"); subj2dict.Add("Political party", "Politics");
            wddict.Add("Propaganda", "Q7281"); subj1dict.Add("Propaganda", "Social sciences"); subj2dict.Add("Propaganda", "Politics");
            wddict.Add("Terrorism", "Q7283"); subj1dict.Add("Terrorism", "Social sciences"); subj2dict.Add("Terrorism", "Politics");



            wddict.Add("Economics", "Q8134"); subj1dict.Add("Economics", "Social sciences"); subj2dict.Add("Economics", "Business and economics");
            wddict.Add("Capital", "Q8137"); subj1dict.Add("Capital", "Social sciences"); subj2dict.Add("Capital", "Business and economics");
            wddict.Add("Capitalism", "Q6206"); subj1dict.Add("Capitalism", "Social sciences"); subj2dict.Add("Capitalism", "Business and economics");
            wddict.Add("Currency", "Q8142"); subj1dict.Add("Currency", "Social sciences"); subj2dict.Add("Currency", "Business and economics");
            wddict.Add("Euro", "Q4916"); subj1dict.Add("Euro", "Social sciences"); subj2dict.Add("Euro", "Business and economics");
            wddict.Add("Japanese yen", "Q8146"); subj1dict.Add("Japanese yen", "Social sciences"); subj2dict.Add("Japanese yen", "Business and economics");
            wddict.Add("United States dollar", "Q4917"); subj1dict.Add("United States dollar", "Social sciences"); subj2dict.Add("United States dollar", "Business and economics");
            wddict.Add("Industry", "Q8148"); subj1dict.Add("Industry", "Social sciences"); subj2dict.Add("Industry", "Business and economics");
            wddict.Add("Money", "Q1368"); subj1dict.Add("Money", "Social sciences"); subj2dict.Add("Money", "Business and economics");
            wddict.Add("Tax", "Q8161"); subj1dict.Add("Tax", "Social sciences"); subj2dict.Add("Tax", "Business and economics");



            wddict.Add("Law", "Q7748"); subj1dict.Add("Law", "Social sciences"); subj2dict.Add("Law", "Law");
            wddict.Add("Constitution", "Q7755"); subj1dict.Add("Constitution", "Social sciences"); subj2dict.Add("Constitution", "Law");



            wddict.Add("African Union", "Q7159"); subj1dict.Add("African Union", "Social sciences"); subj2dict.Add("African Union", "International organizations");
            wddict.Add("Arab League", "Q7172"); subj1dict.Add("Arab League", "Social sciences"); subj2dict.Add("Arab League", "International organizations");
            wddict.Add("ASEAN", "Q7768"); subj1dict.Add("ASEAN", "Social sciences"); subj2dict.Add("ASEAN", "International organizations");
            wddict.Add("Commonwealth of Independent States", "Q7779"); subj1dict.Add("Commonwealth of Independent States", "Social sciences"); subj2dict.Add("Commonwealth of Independent States", "International organizations");
            wddict.Add("Commonwealth of Nations", "Q7785"); subj1dict.Add("Commonwealth of Nations", "Social sciences"); subj2dict.Add("Commonwealth of Nations", "International organizations");
            wddict.Add("European Union", "Q458"); subj1dict.Add("European Union", "Social sciences"); subj2dict.Add("European Union", "International organizations");
            wddict.Add("International Red Cross and Red Crescent Movement", "Q7178"); subj1dict.Add("International Red Cross and Red Crescent Movement", "Social sciences"); subj2dict.Add("International Red Cross and Red Crescent Movement", "International organizations");
            wddict.Add("NATO", "Q7184"); subj1dict.Add("NATO", "Social sciences"); subj2dict.Add("NATO", "International organizations");
            wddict.Add("Nobel Prize", "Q7191"); subj1dict.Add("Nobel Prize", "Social sciences"); subj2dict.Add("Nobel Prize", "International organizations");
            wddict.Add("OPEC", "Q7795"); subj1dict.Add("OPEC", "Social sciences"); subj2dict.Add("OPEC", "International organizations");
            wddict.Add("United Nations", "Q1065"); subj1dict.Add("United Nations", "Social sciences"); subj2dict.Add("United Nations", "International organizations");
            wddict.Add("International Court of Justice", "Q7801"); subj1dict.Add("International Court of Justice", "Social sciences"); subj2dict.Add("International Court of Justice", "International organizations");
            wddict.Add("International Monetary Fund", "Q7804"); subj1dict.Add("International Monetary Fund", "Social sciences"); subj2dict.Add("International Monetary Fund", "International organizations");
            wddict.Add("UNESCO", "Q7809"); subj1dict.Add("UNESCO", "Social sciences"); subj2dict.Add("UNESCO", "International organizations");
            wddict.Add("Universal Declaration of Human Rights", "Q7813"); subj1dict.Add("Universal Declaration of Human Rights", "Social sciences"); subj2dict.Add("Universal Declaration of Human Rights", "International organizations");
            wddict.Add("World Health Organization", "Q7817"); subj1dict.Add("World Health Organization", "Social sciences"); subj2dict.Add("World Health Organization", "International organizations");
            wddict.Add("World Bank", "Q7164"); subj1dict.Add("World Bank", "Social sciences"); subj2dict.Add("World Bank", "International organizations");
            wddict.Add("World Trade Organization", "Q7825"); subj1dict.Add("World Trade Organization", "Social sciences"); subj2dict.Add("World Trade Organization", "International organizations");



            wddict.Add("Civil war", "Q8465"); subj1dict.Add("Civil war", "Social sciences"); subj2dict.Add("Civil war", "War and military");
            wddict.Add("Military", "Q8473"); subj1dict.Add("Military", "Social sciences"); subj2dict.Add("Military", "War and military");
            wddict.Add("Peace", "Q454"); subj1dict.Add("Peace", "Social sciences"); subj2dict.Add("Peace", "War and military");
            wddict.Add("War", "Q198"); subj1dict.Add("War", "Social sciences"); subj2dict.Add("War", "War and military");



            wddict.Add("Abortion", "Q8452"); subj1dict.Add("Abortion", "Social sciences"); subj2dict.Add("Abortion", "Social issues");
            wddict.Add("Capital punishment", "Q8454"); subj1dict.Add("Capital punishment", "Social sciences"); subj2dict.Add("Capital punishment", "Social issues");
            wddict.Add("Human rights", "Q8458"); subj1dict.Add("Human rights", "Social sciences"); subj2dict.Add("Human rights", "Social issues");
            wddict.Add("Racism", "Q8461"); subj1dict.Add("Racism", "Social sciences"); subj2dict.Add("Racism", "Social issues");
            wddict.Add("Slavery", "Q8463"); subj1dict.Add("Slavery", "Social sciences"); subj2dict.Add("Slavery", "Social issues");



            wddict.Add("Language", "Q315"); subj1dict.Add("Language", "Language and literature"); subj2dict.Add("Language", "Language and literature");

            wddict.Add("Arabic", "Q13955"); subj1dict.Add("Arabic", "Language and literature"); subj2dict.Add("Arabic", "Language and literature");
            wddict.Add("Bengali", "Q9610"); subj1dict.Add("Bengali", "Language and literature"); subj2dict.Add("Bengali", "Language and literature");
            wddict.Add("Chinese", "Q7850"); subj1dict.Add("Chinese", "Language and literature"); subj2dict.Add("Chinese", "Language and literature");
            wddict.Add("English", "Q1860"); subj1dict.Add("English", "Language and literature"); subj2dict.Add("English", "Language and literature");
            wddict.Add("Esperanto", "Q143"); subj1dict.Add("Esperanto", "Language and literature"); subj2dict.Add("Esperanto", "Language and literature");
            wddict.Add("French", "Q150"); subj1dict.Add("French", "Language and literature"); subj2dict.Add("French", "Language and literature");
            wddict.Add("German", "Q188"); subj1dict.Add("German", "Language and literature"); subj2dict.Add("German", "Language and literature");
            wddict.Add("Greek", "Q9129"); subj1dict.Add("Greek", "Language and literature"); subj2dict.Add("Greek", "Language and literature");
            wddict.Add("Hebrew", "Q9288"); subj1dict.Add("Hebrew", "Language and literature"); subj2dict.Add("Hebrew", "Language and literature");
            wddict.Add("Hindi-Urdu", "Q11051"); subj1dict.Add("Hindi-Urdu", "Language and literature"); subj2dict.Add("Hindi-Urdu", "Language and literature");
            wddict.Add("Japanese", "Q5287"); subj1dict.Add("Japanese", "Language and literature"); subj2dict.Add("Japanese", "Language and literature");
            wddict.Add("Latin", "Q397"); subj1dict.Add("Latin", "Language and literature"); subj2dict.Add("Latin", "Language and literature");
            wddict.Add("Persian", "Q9168"); subj1dict.Add("Persian", "Language and literature"); subj2dict.Add("Persian", "Language and literature");
            wddict.Add("Portuguese", "Q5146"); subj1dict.Add("Portuguese", "Language and literature"); subj2dict.Add("Portuguese", "Language and literature");
            wddict.Add("Russian", "Q7737"); subj1dict.Add("Russian", "Language and literature"); subj2dict.Add("Russian", "Language and literature");
            wddict.Add("Sanskrit", "Q11059"); subj1dict.Add("Sanskrit", "Language and literature"); subj2dict.Add("Sanskrit", "Language and literature");
            wddict.Add("Spanish", "Q1321"); subj1dict.Add("Spanish", "Language and literature"); subj2dict.Add("Spanish", "Language and literature");
            wddict.Add("Swahili", "Q7838"); subj1dict.Add("Swahili", "Language and literature"); subj2dict.Add("Swahili", "Language and literature");
            wddict.Add("Turkish", "Q256"); subj1dict.Add("Turkish", "Language and literature"); subj2dict.Add("Turkish", "Language and literature");
            wddict.Add("Linguistics", "Q8162"); subj1dict.Add("Linguistics", "Language and literature"); subj2dict.Add("Linguistics", "Language and literature");
            wddict.Add("Grammar", "Q8091"); subj1dict.Add("Grammar", "Language and literature"); subj2dict.Add("Grammar", "Language and literature");
            wddict.Add("Word", "Q8171"); subj1dict.Add("Word", "Language and literature"); subj2dict.Add("Word", "Language and literature");
            wddict.Add("Phoneme", "Q8183"); subj1dict.Add("Phoneme", "Language and literature"); subj2dict.Add("Phoneme", "Language and literature");
            wddict.Add("Syllable", "Q8188"); subj1dict.Add("Syllable", "Language and literature"); subj2dict.Add("Syllable", "Language and literature");
            wddict.Add("Writing system", "Q8192"); subj1dict.Add("Writing system", "Language and literature"); subj2dict.Add("Writing system", "Language and literature");
            wddict.Add("Arabic alphabet", "Q8196"); subj1dict.Add("Arabic alphabet", "Language and literature"); subj2dict.Add("Arabic alphabet", "Language and literature");
            wddict.Add("Chinese characters", "Q8201"); subj1dict.Add("Chinese characters", "Language and literature"); subj2dict.Add("Chinese characters", "Language and literature");
            wddict.Add("Cyrillic script", "Q8209"); subj1dict.Add("Cyrillic script", "Language and literature"); subj2dict.Add("Cyrillic script", "Language and literature");
            wddict.Add("Greek alphabet", "Q8216"); subj1dict.Add("Greek alphabet", "Language and literature"); subj2dict.Add("Greek alphabet", "Language and literature");
            wddict.Add("Hangul", "Q8222"); subj1dict.Add("Hangul", "Language and literature"); subj2dict.Add("Hangul", "Language and literature");
            //wddict.Add("Latin alphabet", "Q8229"); subj1dict.Add("Latin alphabet", "Language and literature"); subj2dict.Add("Latin alphabet", "Language and literature");
            wddict.Add("Literacy", "Q8236"); subj1dict.Add("Literacy", "Language and literature"); subj2dict.Add("Literacy", "Language and literature");
            wddict.Add("Translation", "Q7553"); subj1dict.Add("Translation", "Language and literature"); subj2dict.Add("Translation", "Language and literature");
            wddict.Add("Literature", "Q8242"); subj1dict.Add("Literature", "Language and literature"); subj2dict.Add("Literature", "Language and literature");
            wddict.Add("Prose", "Q676"); subj1dict.Add("Prose", "Language and literature"); subj2dict.Add("Prose", "Language and literature");
            wddict.Add("The Art of War", "Q8251"); subj1dict.Add("The Art of War", "Language and literature"); subj2dict.Add("The Art of War", "Language and literature");
            wddict.Add("Fiction", "Q8253"); subj1dict.Add("Fiction", "Language and literature"); subj2dict.Add("Fiction", "Language and literature");
            wddict.Add("One Thousand and One Nights", "Q8258"); subj1dict.Add("One Thousand and One Nights", "Language and literature"); subj2dict.Add("One Thousand and One Nights", "Language and literature");
            wddict.Add("Novel", "Q8261"); subj1dict.Add("Novel", "Language and literature"); subj2dict.Add("Novel", "Language and literature");
            wddict.Add("Dream of the Red Chamber", "Q8265"); subj1dict.Add("Dream of the Red Chamber", "Language and literature"); subj2dict.Add("Dream of the Red Chamber", "Language and literature");
            wddict.Add("The Tale of Genji", "Q8269"); subj1dict.Add("The Tale of Genji", "Language and literature"); subj2dict.Add("The Tale of Genji", "Language and literature");
            wddict.Add("Poetry", "Q482"); subj1dict.Add("Poetry", "Language and literature"); subj2dict.Add("Poetry", "Language and literature");
            wddict.Add("Epic of Gilgamesh", "Q8272"); subj1dict.Add("Epic of Gilgamesh", "Language and literature"); subj2dict.Add("Epic of Gilgamesh", "Language and literature");
            wddict.Add("Iliad", "Q8275"); subj1dict.Add("Iliad", "Language and literature"); subj2dict.Add("Iliad", "Language and literature");
            wddict.Add("Mahābhārata", "Q8276"); subj1dict.Add("Mahābhārata", "Language and literature"); subj2dict.Add("Mahābhārata", "Language and literature");
            wddict.Add("Shāhnāma", "Q8279"); subj1dict.Add("Shāhnāma", "Language and literature"); subj2dict.Add("Shāhnāma", "Language and literature");



            wddict.Add("Science", "Q336"); subj1dict.Add("Science", "Science"); subj2dict.Add("Science", "Science");
            wddict.Add("Nature", "Q7860"); subj1dict.Add("Nature", "Science"); subj2dict.Add("Nature", "Science");



            wddict.Add("Astronomy", "Q333"); subj1dict.Add("Astronomy", "Science"); subj2dict.Add("Astronomy", "Astronomy");
            wddict.Add("Asteroid", "Q3863"); subj1dict.Add("Asteroid", "Science"); subj2dict.Add("Asteroid", "Astronomy");
            wddict.Add("Big Bang", "Q323"); subj1dict.Add("Big Bang", "Science"); subj2dict.Add("Big Bang", "Astronomy");
            wddict.Add("Black hole", "Q589"); subj1dict.Add("Black hole", "Science"); subj2dict.Add("Black hole", "Astronomy");
            wddict.Add("Comet", "Q3559"); subj1dict.Add("Comet", "Science"); subj2dict.Add("Comet", "Astronomy");
            wddict.Add("Galaxy", "Q318"); subj1dict.Add("Galaxy", "Science"); subj2dict.Add("Galaxy", "Astronomy");
            wddict.Add("Milky Way", "Q321"); subj1dict.Add("Milky Way", "Science"); subj2dict.Add("Milky Way", "Astronomy");
            wddict.Add("Moon", "Q405"); subj1dict.Add("Moon", "Science"); subj2dict.Add("Moon", "Astronomy");
            wddict.Add("Planet", "Q634"); subj1dict.Add("Planet", "Science"); subj2dict.Add("Planet", "Astronomy");
            wddict.Add("Earth", "Q2"); subj1dict.Add("Earth", "Science"); subj2dict.Add("Earth", "Astronomy");
            wddict.Add("Jupiter", "Q319"); subj1dict.Add("Jupiter", "Science"); subj2dict.Add("Jupiter", "Astronomy");
            wddict.Add("Mars", "Q111"); subj1dict.Add("Mars", "Science"); subj2dict.Add("Mars", "Astronomy");
            wddict.Add("Mercury", "Q308"); subj1dict.Add("Mercury", "Science"); subj2dict.Add("Mercury", "Astronomy");
            wddict.Add("Neptune", "Q332"); subj1dict.Add("Neptune", "Science"); subj2dict.Add("Neptune", "Astronomy");
            wddict.Add("Saturn", "Q193"); subj1dict.Add("Saturn", "Science"); subj2dict.Add("Saturn", "Astronomy");
            wddict.Add("Uranus", "Q324"); subj1dict.Add("Uranus", "Science"); subj2dict.Add("Uranus", "Astronomy");
            wddict.Add("Venus", "Q313"); subj1dict.Add("Venus", "Science"); subj2dict.Add("Venus", "Astronomy");
            wddict.Add("Solar System", "Q544"); subj1dict.Add("Solar System", "Science"); subj2dict.Add("Solar System", "Astronomy");
            wddict.Add("Spaceflight", "Q5916"); subj1dict.Add("Spaceflight", "Science"); subj2dict.Add("Spaceflight", "Astronomy");
            wddict.Add("Star", "Q523"); subj1dict.Add("Star", "Science"); subj2dict.Add("Star", "Astronomy");
            wddict.Add("Sun", "Q525"); subj1dict.Add("Sun", "Science"); subj2dict.Add("Sun", "Astronomy");
            wddict.Add("Universe", "Q1"); subj1dict.Add("Universe", "Science"); subj2dict.Add("Universe", "Astronomy");



            wddict.Add("Biology", "Q420"); subj1dict.Add("Biology", "Science"); subj2dict.Add("Biology", "Biology");

            wddict.Add("DNA", "Q7430"); subj1dict.Add("DNA", "Science"); subj2dict.Add("DNA", "Biology");
            wddict.Add("Enzyme", "Q8047"); subj1dict.Add("Enzyme", "Science"); subj2dict.Add("Enzyme", "Biology");
            wddict.Add("Protein", "Q8054"); subj1dict.Add("Protein", "Science"); subj2dict.Add("Protein", "Biology");
            wddict.Add("Botany", "Q441"); subj1dict.Add("Botany", "Science"); subj2dict.Add("Botany", "Biology");
            wddict.Add("Death", "Q4"); subj1dict.Add("Death", "Science"); subj2dict.Add("Death", "Biology");
            wddict.Add("Suicide", "Q10737"); subj1dict.Add("Suicide", "Science"); subj2dict.Add("Suicide", "Biology");
            wddict.Add("Ecology", "Q7150"); subj1dict.Add("Ecology", "Science"); subj2dict.Add("Ecology", "Biology");
            wddict.Add("Endangered species", "Q11394"); subj1dict.Add("Endangered species", "Science"); subj2dict.Add("Endangered species", "Biology");
            wddict.Add("Domestication", "Q11395"); subj1dict.Add("Domestication", "Science"); subj2dict.Add("Domestication", "Biology");
            wddict.Add("Life", "Q3"); subj1dict.Add("Life", "Science"); subj2dict.Add("Life", "Biology");
            wddict.Add("Biological classification", "Q11398"); subj1dict.Add("Biological classification", "Science"); subj2dict.Add("Biological classification", "Biology");
            wddict.Add("Species", "Q7432"); subj1dict.Add("Species", "Science"); subj2dict.Add("Species", "Biology");



            wddict.Add("Metabolism", "Q1057"); subj1dict.Add("Metabolism", "Science"); subj2dict.Add("Metabolism", "Biological processes");
            wddict.Add("Digestion", "Q11978"); subj1dict.Add("Digestion", "Science"); subj2dict.Add("Digestion", "Biological processes");
            wddict.Add("Photosynthesis", "Q11982"); subj1dict.Add("Photosynthesis", "Science"); subj2dict.Add("Photosynthesis", "Biological processes");
            wddict.Add("Breathing", "Q9530"); subj1dict.Add("Breathing", "Science"); subj2dict.Add("Breathing", "Biological processes");
            wddict.Add("Evolution", "Q1063"); subj1dict.Add("Evolution", "Science"); subj2dict.Add("Evolution", "Biological processes");
            wddict.Add("Reproduction", "Q11990"); subj1dict.Add("Reproduction", "Science"); subj2dict.Add("Reproduction", "Biological processes");
            wddict.Add("Pregnancy", "Q11995"); subj1dict.Add("Pregnancy", "Science"); subj2dict.Add("Pregnancy", "Biological processes");
            wddict.Add("Sex", "Q290"); subj1dict.Add("Sex", "Science"); subj2dict.Add("Sex", "Biological processes");



            wddict.Add("Anatomy", "Q514"); subj1dict.Add("Anatomy", "Science"); subj2dict.Add("Anatomy", "Anatomy");
            wddict.Add("Breast", "Q9103"); subj1dict.Add("Breast", "Science"); subj2dict.Add("Breast", "Anatomy");
            wddict.Add("Cell", "Q7868"); subj1dict.Add("Cell", "Science"); subj2dict.Add("Cell", "Anatomy");
            wddict.Add("Circulatory system", "Q11068"); subj1dict.Add("Circulatory system", "Science"); subj2dict.Add("Circulatory system", "Anatomy");
            wddict.Add("Blood", "Q7873"); subj1dict.Add("Blood", "Science"); subj2dict.Add("Blood", "Anatomy");
            wddict.Add("Heart", "Q1072"); subj1dict.Add("Heart", "Science"); subj2dict.Add("Heart", "Anatomy");
            wddict.Add("Endocrine system", "Q11078"); subj1dict.Add("Endocrine system", "Science"); subj2dict.Add("Endocrine system", "Anatomy");
            wddict.Add("Human gastrointestinal tract", "Q9649"); subj1dict.Add("Human gastrointestinal tract", "Science"); subj2dict.Add("Human gastrointestinal tract", "Anatomy");
            wddict.Add("Large intestine", "Q11083"); subj1dict.Add("Large intestine", "Science"); subj2dict.Add("Large intestine", "Anatomy");
            wddict.Add("Small intestine", "Q11090"); subj1dict.Add("Small intestine", "Science"); subj2dict.Add("Small intestine", "Anatomy");
            wddict.Add("Liver", "Q9368"); subj1dict.Add("Liver", "Science"); subj2dict.Add("Liver", "Anatomy");
            wddict.Add("Muscle", "Q7365"); subj1dict.Add("Muscle", "Science"); subj2dict.Add("Muscle", "Anatomy");
            wddict.Add("Nervous system", "Q9404"); subj1dict.Add("Nervous system", "Science"); subj2dict.Add("Nervous system", "Anatomy");
            wddict.Add("Brain", "Q1073"); subj1dict.Add("Brain", "Science"); subj2dict.Add("Brain", "Anatomy");
            wddict.Add("Sensory system", "Q11101"); subj1dict.Add("Sensory system", "Science"); subj2dict.Add("Sensory system", "Anatomy");
            wddict.Add("Ear", "Q7362"); subj1dict.Add("Ear", "Science"); subj2dict.Add("Ear", "Anatomy");
            wddict.Add("Nose", "Q7363"); subj1dict.Add("Nose", "Science"); subj2dict.Add("Nose", "Anatomy");
            wddict.Add("Eye", "Q7364"); subj1dict.Add("Eye", "Science"); subj2dict.Add("Eye", "Anatomy");
            wddict.Add("Reproductive system", "Q7895"); subj1dict.Add("Reproductive system", "Science"); subj2dict.Add("Reproductive system", "Anatomy");
            wddict.Add("Respiratory system", "Q7891"); subj1dict.Add("Respiratory system", "Science"); subj2dict.Add("Respiratory system", "Anatomy");
            wddict.Add("Lung", "Q7886"); subj1dict.Add("Lung", "Science"); subj2dict.Add("Lung", "Anatomy");
            wddict.Add("Skeleton", "Q7881"); subj1dict.Add("Skeleton", "Science"); subj2dict.Add("Skeleton", "Anatomy");
            wddict.Add("Skin", "Q1074"); subj1dict.Add("Skin", "Science"); subj2dict.Add("Skin", "Anatomy");



            wddict.Add("Medicine", "Q11190"); subj1dict.Add("Medicine", "Science"); subj2dict.Add("Medicine", "Health and medicine");
            wddict.Add("Addiction", "Q12029"); subj1dict.Add("Addiction", "Science"); subj2dict.Add("Addiction", "Health and medicine");
            wddict.Add("Alzheimer's disease", "Q11081"); subj1dict.Add("Alzheimer's disease", "Science"); subj2dict.Add("Alzheimer's disease", "Health and medicine");
            wddict.Add("Cancer", "Q12078"); subj1dict.Add("Cancer", "Science"); subj2dict.Add("Cancer", "Health and medicine");
            wddict.Add("Cholera", "Q12090"); subj1dict.Add("Cholera", "Science"); subj2dict.Add("Cholera", "Health and medicine");
            wddict.Add("Common cold", "Q12125"); subj1dict.Add("Common cold", "Science"); subj2dict.Add("Common cold", "Health and medicine");
            wddict.Add("Dentistry", "Q12128"); subj1dict.Add("Dentistry", "Science"); subj2dict.Add("Dentistry", "Health and medicine");
            wddict.Add("Disability", "Q12131"); subj1dict.Add("Disability", "Science"); subj2dict.Add("Disability", "Health and medicine");
            wddict.Add("Blindness", "Q10874"); subj1dict.Add("Blindness", "Science"); subj2dict.Add("Blindness", "Health and medicine");
            wddict.Add("Deafness", "Q12133"); subj1dict.Add("Deafness", "Science"); subj2dict.Add("Deafness", "Health and medicine");
            wddict.Add("Mental disorder", "Q12135"); subj1dict.Add("Mental disorder", "Science"); subj2dict.Add("Mental disorder", "Health and medicine");
            wddict.Add("Disease", "Q12136"); subj1dict.Add("Disease", "Science"); subj2dict.Add("Disease", "Health and medicine");
            wddict.Add("Pharmaceutical drug", "Q12140"); subj1dict.Add("Pharmaceutical drug", "Science"); subj2dict.Add("Pharmaceutical drug", "Health and medicine");
            wddict.Add("Ethanol", "Q153"); subj1dict.Add("Ethanol", "Science"); subj2dict.Add("Ethanol", "Health and medicine");
            wddict.Add("Nicotine", "Q12144"); subj1dict.Add("Nicotine", "Science"); subj2dict.Add("Nicotine", "Health and medicine");
            wddict.Add("Tobacco", "Q1566"); subj1dict.Add("Tobacco", "Science"); subj2dict.Add("Tobacco", "Health and medicine");
            wddict.Add("Health", "Q12147"); subj1dict.Add("Health", "Science"); subj2dict.Add("Health", "Health and medicine");
            wddict.Add("Headache", "Q86"); subj1dict.Add("Headache", "Science"); subj2dict.Add("Headache", "Health and medicine");
            wddict.Add("Immune system", "Q1059"); subj1dict.Add("Immune system", "Science"); subj2dict.Add("Immune system", "Health and medicine");
            wddict.Add("Heart attack", "Q12152"); subj1dict.Add("Heart attack", "Science"); subj2dict.Add("Heart attack", "Health and medicine");
            wddict.Add("Malaria", "Q12156"); subj1dict.Add("Malaria", "Science"); subj2dict.Add("Malaria", "Health and medicine");
            wddict.Add("Malnutrition", "Q12167"); subj1dict.Add("Malnutrition", "Science"); subj2dict.Add("Malnutrition", "Health and medicine");
            wddict.Add("Menstruation", "Q12171"); subj1dict.Add("Menstruation", "Science"); subj2dict.Add("Menstruation", "Health and medicine");
            wddict.Add("Obesity", "Q12174"); subj1dict.Add("Obesity", "Science"); subj2dict.Add("Obesity", "Health and medicine");
            wddict.Add("Pandemic", "Q12184"); subj1dict.Add("Pandemic", "Science"); subj2dict.Add("Pandemic", "Health and medicine");
            wddict.Add("Antibacterial", "Q12187"); subj1dict.Add("Antibacterial", "Science"); subj2dict.Add("Antibacterial", "Health and medicine");
            wddict.Add("Penicillin", "Q12190"); subj1dict.Add("Penicillin", "Science"); subj2dict.Add("Penicillin", "Health and medicine");
            wddict.Add("Pneumonia", "Q12192"); subj1dict.Add("Pneumonia", "Science"); subj2dict.Add("Pneumonia", "Health and medicine");
            wddict.Add("Poliomyelitis", "Q12195"); subj1dict.Add("Poliomyelitis", "Science"); subj2dict.Add("Poliomyelitis", "Health and medicine");
            wddict.Add("Sexually transmitted disease", "Q12198"); subj1dict.Add("Sexually transmitted disease", "Science"); subj2dict.Add("Sexually transmitted disease", "Health and medicine");
            wddict.Add("AIDS", "Q12199"); subj1dict.Add("AIDS", "Science"); subj2dict.Add("AIDS", "Health and medicine");
            wddict.Add("Stroke", "Q12202"); subj1dict.Add("Stroke", "Science"); subj2dict.Add("Stroke", "Health and medicine");
            wddict.Add("Tuberculosis", "Q12204"); subj1dict.Add("Tuberculosis", "Science"); subj2dict.Add("Tuberculosis", "Health and medicine");
            wddict.Add("Diabetes mellitus", "Q12206"); subj1dict.Add("Diabetes mellitus", "Science"); subj2dict.Add("Diabetes mellitus", "Health and medicine");
            wddict.Add("Virus", "Q808"); subj1dict.Add("Virus", "Science"); subj2dict.Add("Virus", "Health and medicine");
            wddict.Add("Influenza", "Q2840"); subj1dict.Add("Influenza", "Science"); subj2dict.Add("Influenza", "Health and medicine");
            wddict.Add("Smallpox", "Q12214"); subj1dict.Add("Smallpox", "Science"); subj2dict.Add("Smallpox", "Health and medicine");



            wddict.Add("Organism", "Q7239"); subj1dict.Add("Organism", "Science"); subj2dict.Add("Organism", "Organisms");
            wddict.Add("Animal", "Q729"); subj1dict.Add("Animal", "Science"); subj2dict.Add("Animal", "Organisms");
            wddict.Add("Nematode", "Q5185"); subj1dict.Add("Nematode", "Science"); subj2dict.Add("Nematode", "Organisms");
            wddict.Add("Arthropod", "Q1360"); subj1dict.Add("Arthropod", "Science"); subj2dict.Add("Arthropod", "Organisms");
            wddict.Add("Insect", "Q1390"); subj1dict.Add("Insect", "Science"); subj2dict.Add("Insect", "Organisms");
            wddict.Add("Ant", "Q7386"); subj1dict.Add("Ant", "Science"); subj2dict.Add("Ant", "Organisms");
            wddict.Add("Bee", "Q7391"); subj1dict.Add("Bee", "Science"); subj2dict.Add("Bee", "Organisms");
            wddict.Add("Mosquito", "Q7367"); subj1dict.Add("Mosquito", "Science"); subj2dict.Add("Mosquito", "Organisms");
            wddict.Add("Spider", "Q1357"); subj1dict.Add("Spider", "Science"); subj2dict.Add("Spider", "Organisms");
            wddict.Add("Chordate", "Q10915"); subj1dict.Add("Chordate", "Science"); subj2dict.Add("Chordate", "Organisms");
            wddict.Add("Amphibian", "Q10908"); subj1dict.Add("Amphibian", "Science"); subj2dict.Add("Amphibian", "Organisms");
            wddict.Add("Bird", "Q5113"); subj1dict.Add("Bird", "Science"); subj2dict.Add("Bird", "Organisms");
            wddict.Add("Chicken", "Q780"); subj1dict.Add("Chicken", "Science"); subj2dict.Add("Chicken", "Organisms");
            wddict.Add("Pigeons and doves", "Q10856"); subj1dict.Add("Pigeons and doves", "Science"); subj2dict.Add("Pigeons and doves", "Organisms");
            wddict.Add("Fish", "Q152"); subj1dict.Add("Fish", "Science"); subj2dict.Add("Fish", "Organisms");
            wddict.Add("Shark", "Q7372"); subj1dict.Add("Shark", "Science"); subj2dict.Add("Shark", "Organisms");
            wddict.Add("Mammal", "Q7377"); subj1dict.Add("Mammal", "Science"); subj2dict.Add("Mammal", "Organisms");
            wddict.Add("Camel", "Q7375"); subj1dict.Add("Camel", "Science"); subj2dict.Add("Camel", "Organisms");
            wddict.Add("Cat", "Q146"); subj1dict.Add("Cat", "Science"); subj2dict.Add("Cat", "Organisms");
            wddict.Add("Cattle", "Q830"); subj1dict.Add("Cattle", "Science"); subj2dict.Add("Cattle", "Organisms");
            wddict.Add("Dog", "Q144"); subj1dict.Add("Dog", "Science"); subj2dict.Add("Dog", "Organisms");
            wddict.Add("Elephant", "Q7378"); subj1dict.Add("Elephant", "Science"); subj2dict.Add("Elephant", "Organisms");
            wddict.Add("Horse", "Q726"); subj1dict.Add("Horse", "Science"); subj2dict.Add("Horse", "Organisms");
            wddict.Add("Sheep", "Q7368"); subj1dict.Add("Sheep", "Science"); subj2dict.Add("Sheep", "Organisms");
            wddict.Add("Rodent", "Q10850"); subj1dict.Add("Rodent", "Science"); subj2dict.Add("Rodent", "Organisms");
            wddict.Add("Domestic pig", "Q787"); subj1dict.Add("Domestic pig", "Science"); subj2dict.Add("Domestic pig", "Organisms");
            wddict.Add("Primate", "Q7380"); subj1dict.Add("Primate", "Science"); subj2dict.Add("Primate", "Organisms");
            wddict.Add("Human", "Q5"); subj1dict.Add("Human", "Science"); subj2dict.Add("Human", "Organisms");
            wddict.Add("Whales, dolphins and porpoises", "Q160"); subj1dict.Add("Whales, dolphins and porpoises", "Science"); subj2dict.Add("Whales, dolphins and porpoises", "Organisms");
            wddict.Add("Reptile", "Q10811"); subj1dict.Add("Reptile", "Science"); subj2dict.Add("Reptile", "Organisms");
            wddict.Add("Dinosaur", "Q430"); subj1dict.Add("Dinosaur", "Science"); subj2dict.Add("Dinosaur", "Organisms");
            wddict.Add("Snake", "Q2102"); subj1dict.Add("Snake", "Science"); subj2dict.Add("Snake", "Organisms");
            wddict.Add("Archaea", "Q10872"); subj1dict.Add("Archaea", "Science"); subj2dict.Add("Archaea", "Organisms");
            wddict.Add("Bacteria", "Q10876"); subj1dict.Add("Bacteria", "Science"); subj2dict.Add("Bacteria", "Organisms");
            wddict.Add("Fungus", "Q764"); subj1dict.Add("Fungus", "Science"); subj2dict.Add("Fungus", "Organisms");
            wddict.Add("Plant", "Q756"); subj1dict.Add("Plant", "Science"); subj2dict.Add("Plant", "Organisms");
            wddict.Add("Flower", "Q506"); subj1dict.Add("Flower", "Science"); subj2dict.Add("Flower", "Organisms");
            wddict.Add("Tree", "Q10884"); subj1dict.Add("Tree", "Science"); subj2dict.Add("Tree", "Organisms");
            wddict.Add("Protist", "Q10892"); subj1dict.Add("Protist", "Science"); subj2dict.Add("Protist", "Organisms");



            wddict.Add("Chemistry", "Q2329"); subj1dict.Add("Chemistry", "Science"); subj2dict.Add("Chemistry", "Chemistry");
            wddict.Add("Inorganic chemistry", "Q11165"); subj1dict.Add("Inorganic chemistry", "Science"); subj2dict.Add("Inorganic chemistry", "Chemistry");
            wddict.Add("Biochemistry", "Q7094"); subj1dict.Add("Biochemistry", "Science"); subj2dict.Add("Biochemistry", "Chemistry");
            wddict.Add("Chemical compound", "Q11173"); subj1dict.Add("Chemical compound", "Science"); subj2dict.Add("Chemical compound", "Chemistry");
            wddict.Add("Acid", "Q11158"); subj1dict.Add("Acid", "Science"); subj2dict.Add("Acid", "Chemistry");
            wddict.Add("Base (chemistry)", "Q11193"); subj1dict.Add("Base (chemistry)", "Science"); subj2dict.Add("Base (chemistry)", "Chemistry");
            wddict.Add("Salt", "Q12370"); subj1dict.Add("Salt", "Science"); subj2dict.Add("Salt", "Chemistry");
            wddict.Add("Chemical element", "Q11344"); subj1dict.Add("Chemical element", "Science"); subj2dict.Add("Chemical element", "Chemistry");
            wddict.Add("Periodic table", "Q10693"); subj1dict.Add("Periodic table", "Science"); subj2dict.Add("Periodic table", "Chemistry");
            wddict.Add("Aluminium", "Q663"); subj1dict.Add("Aluminium", "Science"); subj2dict.Add("Aluminium", "Chemistry");
            wddict.Add("Carbon", "Q623"); subj1dict.Add("Carbon", "Science"); subj2dict.Add("Carbon", "Chemistry");
            wddict.Add("Copper", "Q753"); subj1dict.Add("Copper", "Science"); subj2dict.Add("Copper", "Chemistry");
            wddict.Add("Gold", "Q897"); subj1dict.Add("Gold", "Science"); subj2dict.Add("Gold", "Chemistry");
            wddict.Add("Hydrogen", "Q556"); subj1dict.Add("Hydrogen", "Science"); subj2dict.Add("Hydrogen", "Chemistry");
            wddict.Add("Iron", "Q677"); subj1dict.Add("Iron", "Science"); subj2dict.Add("Iron", "Chemistry");
            wddict.Add("Nitrogen", "Q627"); subj1dict.Add("Nitrogen", "Science"); subj2dict.Add("Nitrogen", "Chemistry");
            wddict.Add("Oxygen", "Q629"); subj1dict.Add("Oxygen", "Science"); subj2dict.Add("Oxygen", "Chemistry");
            wddict.Add("Silver", "Q1090"); subj1dict.Add("Silver", "Science"); subj2dict.Add("Silver", "Chemistry");
            wddict.Add("Tin", "Q1096"); subj1dict.Add("Tin", "Science"); subj2dict.Add("Tin", "Chemistry");
            wddict.Add("Organic chemistry", "Q11351"); subj1dict.Add("Organic chemistry", "Science"); subj2dict.Add("Organic chemistry", "Chemistry");
            wddict.Add("Alcohol", "Q156"); subj1dict.Add("Alcohol", "Science"); subj2dict.Add("Alcohol", "Chemistry");
            wddict.Add("Carbohydrate", "Q11358"); subj1dict.Add("Carbohydrate", "Science"); subj2dict.Add("Carbohydrate", "Chemistry");
            wddict.Add("Hormone", "Q11364"); subj1dict.Add("Hormone", "Science"); subj2dict.Add("Hormone", "Chemistry");
            wddict.Add("Lipid", "Q11367"); subj1dict.Add("Lipid", "Science"); subj2dict.Add("Lipid", "Chemistry");
            wddict.Add("Molecule", "Q11369"); subj1dict.Add("Molecule", "Science"); subj2dict.Add("Molecule", "Chemistry");
            wddict.Add("Analytical chemistry", "Q2346"); subj1dict.Add("Analytical chemistry", "Science"); subj2dict.Add("Analytical chemistry", "Chemistry");
            wddict.Add("Physical chemistry", "Q11372"); subj1dict.Add("Physical chemistry", "Science"); subj2dict.Add("Physical chemistry", "Chemistry");



            wddict.Add("Avalanche", "Q7935"); subj1dict.Add("Avalanche", "Science"); subj2dict.Add("Avalanche", "Earth science");
            wddict.Add("Climate", "Q7937"); subj1dict.Add("Climate", "Science"); subj2dict.Add("Climate", "Earth science");
            wddict.Add("El Niño–Southern Oscillation", "Q7939"); subj1dict.Add("El Niño–Southern Oscillation", "Science"); subj2dict.Add("El Niño–Southern Oscillation", "Earth science");
            wddict.Add("Global warming", "Q7942"); subj1dict.Add("Global warming", "Science"); subj2dict.Add("Global warming", "Earth science");
            wddict.Add("Earthquake", "Q7944"); subj1dict.Add("Earthquake", "Science"); subj2dict.Add("Earthquake", "Earth science");
            wddict.Add("Geology", "Q1069"); subj1dict.Add("Geology", "Science"); subj2dict.Add("Geology", "Earth science");
            wddict.Add("Mineral", "Q7946"); subj1dict.Add("Mineral", "Science"); subj2dict.Add("Mineral", "Earth science");
            wddict.Add("Diamond", "Q5283"); subj1dict.Add("Diamond", "Science"); subj2dict.Add("Diamond", "Earth science");
            wddict.Add("Plate tectonics", "Q7950"); subj1dict.Add("Plate tectonics", "Science"); subj2dict.Add("Plate tectonics", "Earth science");
            wddict.Add("Rock", "Q8063"); subj1dict.Add("Rock", "Science"); subj2dict.Add("Rock", "Earth science");
            wddict.Add("Natural disaster", "Q8065"); subj1dict.Add("Natural disaster", "Science"); subj2dict.Add("Natural disaster", "Earth science");
            wddict.Add("Flood", "Q8068"); subj1dict.Add("Flood", "Science"); subj2dict.Add("Flood", "Earth science");
            wddict.Add("Tsunami", "Q8070"); subj1dict.Add("Tsunami", "Science"); subj2dict.Add("Tsunami", "Earth science");
            wddict.Add("Volcano", "Q8072"); subj1dict.Add("Volcano", "Science"); subj2dict.Add("Volcano", "Earth science");
            wddict.Add("Weather", "Q11663"); subj1dict.Add("Weather", "Science"); subj2dict.Add("Weather", "Earth science");
            wddict.Add("Cloud", "Q8074"); subj1dict.Add("Cloud", "Science"); subj2dict.Add("Cloud", "Earth science");
            wddict.Add("Rain", "Q7925"); subj1dict.Add("Rain", "Science"); subj2dict.Add("Rain", "Earth science");
            wddict.Add("Snow", "Q7561"); subj1dict.Add("Snow", "Science"); subj2dict.Add("Snow", "Earth science");
            wddict.Add("Tornado", "Q8081"); subj1dict.Add("Tornado", "Science"); subj2dict.Add("Tornado", "Earth science");
            wddict.Add("Tropical cyclone", "Q8092"); subj1dict.Add("Tropical cyclone", "Science"); subj2dict.Add("Tropical cyclone", "Earth science");
            wddict.Add("Wind", "Q8094"); subj1dict.Add("Wind", "Science"); subj2dict.Add("Wind", "Earth science");


            wddict.Add("Physics", "Q413"); subj1dict.Add("Physics", "Science"); subj2dict.Add("Physics", "Physics");
            wddict.Add("Acceleration", "Q11376"); subj1dict.Add("Acceleration", "Science"); subj2dict.Add("Acceleration", "Physics");
            wddict.Add("Atom", "Q9121"); subj1dict.Add("Atom", "Science"); subj2dict.Add("Atom", "Physics");
            wddict.Add("Energy", "Q11379"); subj1dict.Add("Energy", "Science"); subj2dict.Add("Energy", "Physics");
            wddict.Add("Conservation of energy", "Q11382"); subj1dict.Add("Conservation of energy", "Science"); subj2dict.Add("Conservation of energy", "Physics");
            wddict.Add("Electromagnetic radiation", "Q11386"); subj1dict.Add("Electromagnetic radiation", "Science"); subj2dict.Add("Electromagnetic radiation", "Physics");
            wddict.Add("Infrared", "Q11388"); subj1dict.Add("Infrared", "Science"); subj2dict.Add("Infrared", "Physics");
            wddict.Add("Ultraviolet", "Q11391"); subj1dict.Add("Ultraviolet", "Science"); subj2dict.Add("Ultraviolet", "Physics");
            wddict.Add("Light", "Q9128"); subj1dict.Add("Light", "Science"); subj2dict.Add("Light", "Physics");
            wddict.Add("Color", "Q1075"); subj1dict.Add("Color", "Science"); subj2dict.Add("Color", "Physics");
            wddict.Add("Classical mechanics", "Q11397"); subj1dict.Add("Classical mechanics", "Science"); subj2dict.Add("Classical mechanics", "Physics");
            wddict.Add("Force", "Q11402"); subj1dict.Add("Force", "Science"); subj2dict.Add("Force", "Physics");
            wddict.Add("Electromagnetism", "Q11406"); subj1dict.Add("Electromagnetism", "Science"); subj2dict.Add("Electromagnetism", "Physics");
            wddict.Add("Magnetic field", "Q11408"); subj1dict.Add("Magnetic field", "Science"); subj2dict.Add("Magnetic field", "Physics");
            wddict.Add("Gravitation", "Q11412"); subj1dict.Add("Gravitation", "Science"); subj2dict.Add("Gravitation", "Physics");
            wddict.Add("Strong interaction", "Q11415"); subj1dict.Add("Strong interaction", "Science"); subj2dict.Add("Strong interaction", "Physics");
            wddict.Add("Weak interaction", "Q11418"); subj1dict.Add("Weak interaction", "Science"); subj2dict.Add("Weak interaction", "Physics");
            wddict.Add("Magnet", "Q11421"); subj1dict.Add("Magnet", "Science"); subj2dict.Add("Magnet", "Physics");
            wddict.Add("Mass", "Q11423"); subj1dict.Add("Mass", "Science"); subj2dict.Add("Mass", "Physics");
            wddict.Add("Metal", "Q11426"); subj1dict.Add("Metal", "Science"); subj2dict.Add("Metal", "Physics");
            wddict.Add("Steel", "Q11427"); subj1dict.Add("Steel", "Science"); subj2dict.Add("Steel", "Physics");
            wddict.Add("Nuclear fission", "Q11429"); subj1dict.Add("Nuclear fission", "Science"); subj2dict.Add("Nuclear fission", "Physics");
            wddict.Add("State of matter", "Q11430"); subj1dict.Add("State of matter", "Science"); subj2dict.Add("State of matter", "Physics");
            wddict.Add("Gas", "Q11432"); subj1dict.Add("Gas", "Science"); subj2dict.Add("Gas", "Physics");
            wddict.Add("Liquid", "Q11435"); subj1dict.Add("Liquid", "Science"); subj2dict.Add("Liquid", "Physics");
            wddict.Add("Plasma", "Q10251"); subj1dict.Add("Plasma", "Science"); subj2dict.Add("Plasma", "Physics");
            wddict.Add("Solid", "Q11438"); subj1dict.Add("Solid", "Science"); subj2dict.Add("Solid", "Physics");
            wddict.Add("Quantum mechanics", "Q944"); subj1dict.Add("Quantum mechanics", "Science"); subj2dict.Add("Quantum mechanics", "Physics");
            wddict.Add("Radioactive decay", "Q11448"); subj1dict.Add("Radioactive decay", "Science"); subj2dict.Add("Radioactive decay", "Physics");
            wddict.Add("General relativity", "Q11452"); subj1dict.Add("General relativity", "Science"); subj2dict.Add("General relativity", "Physics");
            wddict.Add("Special relativity", "Q11455"); subj1dict.Add("Special relativity", "Science"); subj2dict.Add("Special relativity", "Physics");
            wddict.Add("Semiconductor", "Q11456"); subj1dict.Add("Semiconductor", "Science"); subj2dict.Add("Semiconductor", "Physics");
            wddict.Add("Sound", "Q11461"); subj1dict.Add("Sound", "Science"); subj2dict.Add("Sound", "Physics");
            wddict.Add("Velocity", "Q11465"); subj1dict.Add("Velocity", "Science"); subj2dict.Add("Velocity", "Physics");
            wddict.Add("Speed of light", "Q2111"); subj1dict.Add("Speed of light", "Science"); subj2dict.Add("Speed of light", "Physics");
            wddict.Add("Temperature", "Q11466"); subj1dict.Add("Temperature", "Science"); subj2dict.Add("Temperature", "Physics");
            wddict.Add("Time", "Q11471"); subj1dict.Add("Time", "Science"); subj2dict.Add("Time", "Physics");
            wddict.Add("Thermodynamics", "Q11473"); subj1dict.Add("Thermodynamics", "Science"); subj2dict.Add("Thermodynamics", "Physics");
            wddict.Add("Vacuum", "Q11475"); subj1dict.Add("Vacuum", "Science"); subj2dict.Add("Vacuum", "Physics");



            wddict.Add("Measurement", "Q12453"); subj1dict.Add("Measurement", "Science"); subj2dict.Add("Measurement", "Measurement and units");
            wddict.Add("Kilogram", "Q11570"); subj1dict.Add("Kilogram", "Science"); subj2dict.Add("Kilogram", "Measurement and units");
            wddict.Add("Litre", "Q11582"); subj1dict.Add("Litre", "Science"); subj2dict.Add("Litre", "Measurement and units");
            wddict.Add("Metre", "Q11573"); subj1dict.Add("Metre", "Science"); subj2dict.Add("Metre", "Measurement and units");
            wddict.Add("International System of Units", "Q12457"); subj1dict.Add("International System of Units", "Science"); subj2dict.Add("International System of Units", "Measurement and units");
            wddict.Add("Second", "Q11574"); subj1dict.Add("Second", "Science"); subj2dict.Add("Second", "Measurement and units");



            wddict.Add("Calendar", "Q12132"); subj1dict.Add("Calendar", "Science"); subj2dict.Add("Calendar", "Timekeeping");
            wddict.Add("Gregorian calendar", "Q12138"); subj1dict.Add("Gregorian calendar", "Science"); subj2dict.Add("Gregorian calendar", "Timekeeping");
            wddict.Add("Clock", "Q376"); subj1dict.Add("Clock", "Science"); subj2dict.Add("Clock", "Timekeeping");
            wddict.Add("Day", "Q573"); subj1dict.Add("Day", "Science"); subj2dict.Add("Day", "Timekeeping");
            wddict.Add("Time zone", "Q12143"); subj1dict.Add("Time zone", "Science"); subj2dict.Add("Time zone", "Timekeeping");
            wddict.Add("Year", "Q577"); subj1dict.Add("Year", "Science"); subj2dict.Add("Year", "Timekeeping");



            wddict.Add("Food", "Q2095"); subj1dict.Add("Food", "Science"); subj2dict.Add("Food", "Foodstuffs");
            wddict.Add("Bread", "Q7802"); subj1dict.Add("Bread", "Science"); subj2dict.Add("Bread", "Foodstuffs");
            wddict.Add("Cereal", "Q12117"); subj1dict.Add("Cereal", "Science"); subj2dict.Add("Cereal", "Foodstuffs");
            wddict.Add("Barley", "Q11577"); subj1dict.Add("Barley", "Science"); subj2dict.Add("Barley", "Foodstuffs");
            wddict.Add("Maize", "Q11575"); subj1dict.Add("Maize", "Science"); subj2dict.Add("Maize", "Foodstuffs");
            wddict.Add("Oat", "Q12104"); subj1dict.Add("Oat", "Science"); subj2dict.Add("Oat", "Foodstuffs");
            wddict.Add("Rice", "Q5090"); subj1dict.Add("Rice", "Science"); subj2dict.Add("Rice", "Foodstuffs");
            wddict.Add("Rye", "Q12099"); subj1dict.Add("Rye", "Science"); subj2dict.Add("Rye", "Foodstuffs");
            wddict.Add("Sorghum", "Q12111"); subj1dict.Add("Sorghum", "Science"); subj2dict.Add("Sorghum", "Foodstuffs");
            wddict.Add("Wheat", "Q12106"); subj1dict.Add("Wheat", "Science"); subj2dict.Add("Wheat", "Foodstuffs");
            wddict.Add("Cheese", "Q10943"); subj1dict.Add("Cheese", "Science"); subj2dict.Add("Cheese", "Foodstuffs");
            wddict.Add("Chocolate", "Q195"); subj1dict.Add("Chocolate", "Science"); subj2dict.Add("Chocolate", "Foodstuffs");
            wddict.Add("Honey", "Q10987"); subj1dict.Add("Honey", "Science"); subj2dict.Add("Honey", "Foodstuffs");
            wddict.Add("Fruit", "Q1364"); subj1dict.Add("Fruit", "Science"); subj2dict.Add("Fruit", "Foodstuffs");
            wddict.Add("Apple", "Q89"); subj1dict.Add("Apple", "Science"); subj2dict.Add("Apple", "Foodstuffs");
            wddict.Add("Banana", "Q503"); subj1dict.Add("Banana", "Science"); subj2dict.Add("Banana", "Foodstuffs");
            wddict.Add("Grape", "Q10978"); subj1dict.Add("Grape", "Science"); subj2dict.Add("Grape", "Foodstuffs");
            wddict.Add("Soybean", "Q11006"); subj1dict.Add("Soybean", "Science"); subj2dict.Add("Soybean", "Foodstuffs");
            wddict.Add("Lemon", "Q500"); subj1dict.Add("Lemon", "Science"); subj2dict.Add("Lemon", "Foodstuffs");
            wddict.Add("Nut (fruit)", "Q11009"); subj1dict.Add("Nut (fruit)", "Science"); subj2dict.Add("Nut (fruit)", "Foodstuffs");
            wddict.Add("Meat", "Q10990"); subj1dict.Add("Meat", "Science"); subj2dict.Add("Meat", "Foodstuffs");
            wddict.Add("Sugar", "Q11002"); subj1dict.Add("Sugar", "Science"); subj2dict.Add("Sugar", "Foodstuffs");
            wddict.Add("Vegetable", "Q11004"); subj1dict.Add("Vegetable", "Science"); subj2dict.Add("Vegetable", "Foodstuffs");
            wddict.Add("Potato", "Q10998"); subj1dict.Add("Potato", "Science"); subj2dict.Add("Potato", "Foodstuffs");



            wddict.Add("Beer", "Q44"); subj1dict.Add("Beer", "Science"); subj2dict.Add("Beer", "Beverages");
            wddict.Add("Coffee", "Q8486"); subj1dict.Add("Coffee", "Science"); subj2dict.Add("Coffee", "Beverages");
            wddict.Add("Juice", "Q8492"); subj1dict.Add("Juice", "Science"); subj2dict.Add("Juice", "Beverages");
            wddict.Add("Milk", "Q8495"); subj1dict.Add("Milk", "Science"); subj2dict.Add("Milk", "Beverages");
            wddict.Add("Tea", "Q6097"); subj1dict.Add("Tea", "Science"); subj2dict.Add("Tea", "Beverages");
            wddict.Add("Water", "Q283"); subj1dict.Add("Water", "Science"); subj2dict.Add("Water", "Beverages");
            wddict.Add("Wine", "Q282"); subj1dict.Add("Wine", "Science"); subj2dict.Add("Wine", "Beverages");



            wddict.Add("Mathematics", "Q395"); subj1dict.Add("Mathematics", "Science"); subj2dict.Add("Mathematics", "Mathematics");
            wddict.Add("Algebra", "Q3968"); subj1dict.Add("Algebra", "Science"); subj2dict.Add("Algebra", "Mathematics");
            wddict.Add("Linear algebra", "Q82571"); subj1dict.Add("Linear algebra", "Science"); subj2dict.Add("Linear algebra", "Mathematics");
            wddict.Add("Arithmetic", "Q11205"); subj1dict.Add("Arithmetic", "Science"); subj2dict.Add("Arithmetic", "Mathematics");
            wddict.Add("Logarithm", "Q11197"); subj1dict.Add("Logarithm", "Science"); subj2dict.Add("Logarithm", "Mathematics");
            wddict.Add("Coordinate system", "Q11210"); subj1dict.Add("Coordinate system", "Science"); subj2dict.Add("Coordinate system", "Mathematics");
            wddict.Add("Mathematical analysis", "Q7754"); subj1dict.Add("Mathematical analysis", "Science"); subj2dict.Add("Mathematical analysis", "Mathematics");
            wddict.Add("Differential equation", "Q11214"); subj1dict.Add("Differential equation", "Science"); subj2dict.Add("Differential equation", "Mathematics");
            wddict.Add("Numerical analysis", "Q11216"); subj1dict.Add("Numerical analysis", "Science"); subj2dict.Add("Numerical analysis", "Mathematics");
            wddict.Add("Equation", "Q11345"); subj1dict.Add("Equation", "Science"); subj2dict.Add("Equation", "Mathematics");
            wddict.Add("Function (mathematics)", "Q11348"); subj1dict.Add("Function (mathematics)", "Science"); subj2dict.Add("Function (mathematics)", "Mathematics");
            wddict.Add("Geometry", "Q8087"); subj1dict.Add("Geometry", "Science"); subj2dict.Add("Geometry", "Mathematics");
            wddict.Add("Angle", "Q11352"); subj1dict.Add("Angle", "Science"); subj2dict.Add("Angle", "Mathematics");
            wddict.Add("Area", "Q11500"); subj1dict.Add("Area", "Science"); subj2dict.Add("Area", "Mathematics");
            wddict.Add("Pi", "Q167"); subj1dict.Add("Pi", "Science"); subj2dict.Add("Pi", "Mathematics");
            wddict.Add("Pythagorean theorem", "Q11518"); subj1dict.Add("Pythagorean theorem", "Science"); subj2dict.Add("Pythagorean theorem", "Mathematics");
            wddict.Add("Symmetry", "Q12485"); subj1dict.Add("Symmetry", "Science"); subj2dict.Add("Symmetry", "Mathematics");
            wddict.Add("Trigonometry", "Q8084"); subj1dict.Add("Trigonometry", "Science"); subj2dict.Add("Trigonometry", "Mathematics");
            wddict.Add("Mathematical proof", "Q11538"); subj1dict.Add("Mathematical proof", "Science"); subj2dict.Add("Mathematical proof", "Mathematics");
            wddict.Add("Number", "Q11563"); subj1dict.Add("Number", "Science"); subj2dict.Add("Number", "Mathematics");
            wddict.Add("Complex number", "Q11567"); subj1dict.Add("Complex number", "Science"); subj2dict.Add("Complex number", "Mathematics");
            wddict.Add("Number theory", "Q12479"); subj1dict.Add("Number theory", "Science"); subj2dict.Add("Number theory", "Mathematics");
            wddict.Add("Infinity", "Q205"); subj1dict.Add("Infinity", "Science"); subj2dict.Add("Infinity", "Mathematics");
            wddict.Add("Set theory", "Q12482"); subj1dict.Add("Set theory", "Science"); subj2dict.Add("Set theory", "Mathematics");
            wddict.Add("Statistics", "Q12483"); subj1dict.Add("Statistics", "Science"); subj2dict.Add("Statistics", "Mathematics");



            wddict.Add("Technology", "Q11016"); subj1dict.Add("Technology", "Technology"); subj2dict.Add("Technology", "Technology");
            wddict.Add("Biotechnology", "Q7108"); subj1dict.Add("Biotechnology", "Technology"); subj2dict.Add("Biotechnology", "Technology");
            wddict.Add("Clothing", "Q11460"); subj1dict.Add("Clothing", "Technology"); subj2dict.Add("Clothing", "Technology");
            wddict.Add("Cotton", "Q11457"); subj1dict.Add("Cotton", "Technology"); subj2dict.Add("Cotton", "Technology");
            wddict.Add("Engineering", "Q11023"); subj1dict.Add("Engineering", "Technology"); subj2dict.Add("Engineering", "Technology");
            wddict.Add("Machine", "Q11019"); subj1dict.Add("Machine", "Technology"); subj2dict.Add("Machine", "Technology");
            wddict.Add("Robot", "Q11012"); subj1dict.Add("Robot", "Technology"); subj2dict.Add("Robot", "Technology");
            wddict.Add("Screw", "Q11022"); subj1dict.Add("Screw", "Technology"); subj2dict.Add("Screw", "Technology");
            wddict.Add("Wheel", "Q446"); subj1dict.Add("Wheel", "Technology"); subj2dict.Add("Wheel", "Technology");
            wddict.Add("Agriculture", "Q11451"); subj1dict.Add("Agriculture", "Technology"); subj2dict.Add("Agriculture", "Technology");
            wddict.Add("Irrigation", "Q11453"); subj1dict.Add("Irrigation", "Technology"); subj2dict.Add("Irrigation", "Technology");
            wddict.Add("Plough", "Q11464"); subj1dict.Add("Plough", "Technology"); subj2dict.Add("Plough", "Technology");
            wddict.Add("Metallurgy", "Q11467"); subj1dict.Add("Metallurgy", "Technology"); subj2dict.Add("Metallurgy", "Technology");
            wddict.Add("Nanotechnology", "Q11468"); subj1dict.Add("Nanotechnology", "Technology"); subj2dict.Add("Nanotechnology", "Technology");



            wddict.Add("Communication", "Q11024"); subj1dict.Add("Communication", "Technology"); subj2dict.Add("Communication", "Communication");
            wddict.Add("Book", "Q571"); subj1dict.Add("Book", "Technology"); subj2dict.Add("Book", "Communication");
            wddict.Add("Information", "Q11028"); subj1dict.Add("Information", "Technology"); subj2dict.Add("Information", "Communication");
            wddict.Add("Encyclopedia", "Q5292"); subj1dict.Add("Encyclopedia", "Technology"); subj2dict.Add("Encyclopedia", "Communication");
            wddict.Add("Journalism", "Q11030"); subj1dict.Add("Journalism", "Technology"); subj2dict.Add("Journalism", "Communication");
            wddict.Add("Newspaper", "Q11032"); subj1dict.Add("Newspaper", "Technology"); subj2dict.Add("Newspaper", "Communication");
            wddict.Add("Mass media", "Q11033"); subj1dict.Add("Mass media", "Technology"); subj2dict.Add("Mass media", "Communication");
            wddict.Add("Printing", "Q11034"); subj1dict.Add("Printing", "Technology"); subj2dict.Add("Printing", "Communication");
            wddict.Add("Telephone", "Q11035"); subj1dict.Add("Telephone", "Technology"); subj2dict.Add("Telephone", "Communication");



            wddict.Add("Electronics", "Q11650"); subj1dict.Add("Electronics", "Technology"); subj2dict.Add("Electronics", "Electronics");
            wddict.Add("Electric current", "Q11651"); subj1dict.Add("Electric current", "Technology"); subj2dict.Add("Electric current", "Electronics");
            wddict.Add("Frequency", "Q11652"); subj1dict.Add("Frequency", "Technology"); subj2dict.Add("Frequency", "Electronics");

            wddict.Add("Capacitor", "Q5322"); subj1dict.Add("Capacitor", "Technology"); subj2dict.Add("Capacitor", "Electronics");
            wddict.Add("Inductor", "Q5325"); subj1dict.Add("Inductor", "Technology"); subj2dict.Add("Inductor", "Electronics");
            wddict.Add("Transistor", "Q5339"); subj1dict.Add("Transistor", "Technology"); subj2dict.Add("Transistor", "Electronics");
            wddict.Add("Diode", "Q11656"); subj1dict.Add("Diode", "Technology"); subj2dict.Add("Diode", "Electronics");
            wddict.Add("Resistor", "Q5321"); subj1dict.Add("Resistor", "Technology"); subj2dict.Add("Resistor", "Electronics");
            wddict.Add("Transformer", "Q11658"); subj1dict.Add("Transformer", "Technology"); subj2dict.Add("Transformer", "Electronics");



            wddict.Add("Computer", "Q68"); subj1dict.Add("Computer", "Technology"); subj2dict.Add("Computer", "Computers and Internet");
            wddict.Add("Hard disk drive", "Q4439"); subj1dict.Add("Hard disk drive", "Technology"); subj2dict.Add("Hard disk drive", "Computers and Internet");
            wddict.Add("Central processing unit", "Q5300"); subj1dict.Add("Central processing unit", "Technology"); subj2dict.Add("Central processing unit", "Computers and Internet");
            wddict.Add("Artificial intelligence", "Q11660"); subj1dict.Add("Artificial intelligence", "Technology"); subj2dict.Add("Artificial intelligence", "Computers and Internet");
            wddict.Add("Information technology", "Q11661"); subj1dict.Add("Information technology", "Technology"); subj2dict.Add("Information technology", "Computers and Internet");
            wddict.Add("Algorithm", "Q8366"); subj1dict.Add("Algorithm", "Technology"); subj2dict.Add("Algorithm", "Computers and Internet");
            wddict.Add("Internet", "Q75"); subj1dict.Add("Internet", "Technology"); subj2dict.Add("Internet", "Computers and Internet");
            wddict.Add("Email", "Q9158"); subj1dict.Add("Email", "Technology"); subj2dict.Add("Email", "Computers and Internet");
            wddict.Add("World Wide Web", "Q466"); subj1dict.Add("World Wide Web", "Technology"); subj2dict.Add("World Wide Web", "Computers and Internet");
            wddict.Add("Operating system", "Q9135"); subj1dict.Add("Operating system", "Technology"); subj2dict.Add("Operating system", "Computers and Internet");
            wddict.Add("Programming language", "Q9143"); subj1dict.Add("Programming language", "Technology"); subj2dict.Add("Programming language", "Computers and Internet");
            wddict.Add("Software", "Q7397"); subj1dict.Add("Software", "Technology"); subj2dict.Add("Software", "Computers and Internet");



            wddict.Add("Renewable energy", "Q12705"); subj1dict.Add("Renewable energy", "Technology"); subj2dict.Add("Renewable energy", "Energy and fuels");
            wddict.Add("Electricity", "Q12725"); subj1dict.Add("Electricity", "Technology"); subj2dict.Add("Electricity", "Energy and fuels");
            wddict.Add("Nuclear power", "Q12739"); subj1dict.Add("Nuclear power", "Technology"); subj2dict.Add("Nuclear power", "Energy and fuels");
            wddict.Add("Fossil fuel", "Q12748"); subj1dict.Add("Fossil fuel", "Technology"); subj2dict.Add("Fossil fuel", "Energy and fuels");
            wddict.Add("Internal combustion engine", "Q12757"); subj1dict.Add("Internal combustion engine", "Technology"); subj2dict.Add("Internal combustion engine", "Energy and fuels");
            wddict.Add("Steam engine", "Q12760"); subj1dict.Add("Steam engine", "Technology"); subj2dict.Add("Steam engine", "Energy and fuels");
            wddict.Add("Fire", "Q3196"); subj1dict.Add("Fire", "Technology"); subj2dict.Add("Fire", "Energy and fuels");


            wddict.Add("Glass", "Q11469"); subj1dict.Add("Glass", "Technology"); subj2dict.Add("Glass", "Materials");
            wddict.Add("Paper", "Q11472"); subj1dict.Add("Paper", "Technology"); subj2dict.Add("Paper", "Materials");
            wddict.Add("Plastic", "Q11474"); subj1dict.Add("Plastic", "Technology"); subj2dict.Add("Plastic", "Materials");
            wddict.Add("Wood", "Q287"); subj1dict.Add("Wood", "Technology"); subj2dict.Add("Wood", "Materials");



            wddict.Add("Transport", "Q7590"); subj1dict.Add("Transport", "Technology"); subj2dict.Add("Transport", "Transportation");
            wddict.Add("Aircraft", "Q11436"); subj1dict.Add("Aircraft", "Technology"); subj2dict.Add("Aircraft", "Transportation");
            wddict.Add("Automobile", "Q1420"); subj1dict.Add("Automobile", "Technology"); subj2dict.Add("Automobile", "Transportation");
            wddict.Add("Bicycle", "Q11442"); subj1dict.Add("Bicycle", "Technology"); subj2dict.Add("Bicycle", "Transportation");
            wddict.Add("Submarine", "Q2811"); subj1dict.Add("Submarine", "Technology"); subj2dict.Add("Submarine", "Transportation");
            wddict.Add("Ship", "Q11446"); subj1dict.Add("Ship", "Technology"); subj2dict.Add("Ship", "Transportation");
            wddict.Add("Train", "Q870"); subj1dict.Add("Train", "Technology"); subj2dict.Add("Train", "Transportation");



            wddict.Add("Weapon", "Q728"); subj1dict.Add("Weapon", "Technology"); subj2dict.Add("Weapon", "Weapons");
            wddict.Add("Sword", "Q12791"); subj1dict.Add("Sword", "Technology"); subj2dict.Add("Sword", "Weapons");
            wddict.Add("Firearm", "Q12796"); subj1dict.Add("Firearm", "Technology"); subj2dict.Add("Firearm", "Weapons");
            wddict.Add("Machine gun", "Q12800"); subj1dict.Add("Machine gun", "Technology"); subj2dict.Add("Machine gun", "Weapons");
            wddict.Add("Nuclear weapon", "Q12802"); subj1dict.Add("Nuclear weapon", "Technology"); subj2dict.Add("Nuclear weapon", "Weapons");
            wddict.Add("Tank", "Q12876"); subj1dict.Add("Tank", "Technology"); subj2dict.Add("Tank", "Weapons");
            wddict.Add("Explosive material", "Q12870"); subj1dict.Add("Explosive material", "Technology"); subj2dict.Add("Explosive material", "Weapons");
            wddict.Add("Gunpowder", "Q12861"); subj1dict.Add("Gunpowder", "Technology"); subj2dict.Add("Gunpowder", "Weapons");



            wddict.Add("Culture", "Q11042"); subj1dict.Add("Culture", "Arts and recreation"); subj2dict.Add("Culture", "Arts and recreation");
            wddict.Add("Art", "Q735"); subj1dict.Add("Art", "Arts and recreation"); subj2dict.Add("Art", "Arts and recreation");
            wddict.Add("Comics", "Q1004"); subj1dict.Add("Comics", "Arts and recreation"); subj2dict.Add("Comics", "Arts and recreation");
            wddict.Add("Painting", "Q11629"); subj1dict.Add("Painting", "Arts and recreation"); subj2dict.Add("Painting", "Arts and recreation");
            wddict.Add("Photography", "Q11633"); subj1dict.Add("Photography", "Arts and recreation"); subj2dict.Add("Photography", "Arts and recreation");
            wddict.Add("Sculpture", "Q11634"); subj1dict.Add("Sculpture", "Arts and recreation"); subj2dict.Add("Sculpture", "Arts and recreation");
            wddict.Add("Pottery", "Q11642"); subj1dict.Add("Pottery", "Arts and recreation"); subj2dict.Add("Pottery", "Arts and recreation");
            wddict.Add("Dance", "Q11639"); subj1dict.Add("Dance", "Arts and recreation"); subj2dict.Add("Dance", "Arts and recreation");
            wddict.Add("Fashion", "Q12684"); subj1dict.Add("Fashion", "Arts and recreation"); subj2dict.Add("Fashion", "Arts and recreation");
            wddict.Add("Theatre", "Q11635"); subj1dict.Add("Theatre", "Arts and recreation"); subj2dict.Add("Theatre", "Arts and recreation");
            wddict.Add("Calligraphy", "Q12681"); subj1dict.Add("Calligraphy", "Arts and recreation"); subj2dict.Add("Calligraphy", "Arts and recreation");



            wddict.Add("Architecture", "Q12271"); subj1dict.Add("Architecture", "Arts and recreation"); subj2dict.Add("Architecture", "Architecture and civil engineering");
            wddict.Add("Arch", "Q12277"); subj1dict.Add("Arch", "Arts and recreation"); subj2dict.Add("Arch", "Architecture and civil engineering");
            wddict.Add("Bridge", "Q12280"); subj1dict.Add("Bridge", "Arts and recreation"); subj2dict.Add("Bridge", "Architecture and civil engineering");
            wddict.Add("Canal", "Q12284"); subj1dict.Add("Canal", "Arts and recreation"); subj2dict.Add("Canal", "Architecture and civil engineering");
            wddict.Add("Dam", "Q12323"); subj1dict.Add("Dam", "Arts and recreation"); subj2dict.Add("Dam", "Architecture and civil engineering");
            wddict.Add("Dome", "Q12493"); subj1dict.Add("Dome", "Arts and recreation"); subj2dict.Add("Dome", "Architecture and civil engineering");
            wddict.Add("House", "Q3947"); subj1dict.Add("House", "Arts and recreation"); subj2dict.Add("House", "Architecture and civil engineering");

            wddict.Add("Angkor Wat", "Q43473"); subj1dict.Add("Angkor Wat", "Arts and recreation"); subj2dict.Add("Angkor Wat", "Architecture and civil engineering");
            wddict.Add("Colosseum", "Q10285"); subj1dict.Add("Colosseum", "Arts and recreation"); subj2dict.Add("Colosseum", "Architecture and civil engineering");
            wddict.Add("Great Wall of China", "Q12501"); subj1dict.Add("Great Wall of China", "Arts and recreation"); subj2dict.Add("Great Wall of China", "Architecture and civil engineering");
            wddict.Add("Eiffel Tower", "Q243"); subj1dict.Add("Eiffel Tower", "Arts and recreation"); subj2dict.Add("Eiffel Tower", "Architecture and civil engineering");
            wddict.Add("Empire State Building", "Q9188"); subj1dict.Add("Empire State Building", "Arts and recreation"); subj2dict.Add("Empire State Building", "Architecture and civil engineering");
            wddict.Add("Hagia Sophia", "Q12506"); subj1dict.Add("Hagia Sophia", "Arts and recreation"); subj2dict.Add("Hagia Sophia", "Architecture and civil engineering");
            wddict.Add("Parthenon", "Q10288"); subj1dict.Add("Parthenon", "Arts and recreation"); subj2dict.Add("Parthenon", "Architecture and civil engineering");
            wddict.Add("Giza Necropolis", "Q13217298"); subj1dict.Add("Giza Necropolis", "Arts and recreation"); subj2dict.Add("Giza Necropolis", "Architecture and civil engineering");
            wddict.Add("St. Peter's Basilica", "Q12512"); subj1dict.Add("St. Peter's Basilica", "Arts and recreation"); subj2dict.Add("St. Peter's Basilica", "Architecture and civil engineering");
            wddict.Add("Statue of Liberty", "Q9202"); subj1dict.Add("Statue of Liberty", "Arts and recreation"); subj2dict.Add("Statue of Liberty", "Architecture and civil engineering");
            wddict.Add("Taj Mahal", "Q9141"); subj1dict.Add("Taj Mahal", "Arts and recreation"); subj2dict.Add("Taj Mahal", "Architecture and civil engineering");
            wddict.Add("Three Gorges Dam", "Q12514"); subj1dict.Add("Three Gorges Dam", "Arts and recreation"); subj2dict.Add("Three Gorges Dam", "Architecture and civil engineering");
            wddict.Add("Pyramid", "Q12516"); subj1dict.Add("Pyramid", "Arts and recreation"); subj2dict.Add("Pyramid", "Architecture and civil engineering");
            wddict.Add("Tower", "Q12518"); subj1dict.Add("Tower", "Arts and recreation"); subj2dict.Add("Tower", "Architecture and civil engineering");



            wddict.Add("Film", "Q11424"); subj1dict.Add("Film", "Arts and recreation"); subj2dict.Add("Film", "Film, radio and television");
            wddict.Add("Animation", "Q11425"); subj1dict.Add("Animation", "Arts and recreation"); subj2dict.Add("Animation", "Film, radio and television");
            wddict.Add("Anime", "Q1107"); subj1dict.Add("Anime", "Arts and recreation"); subj2dict.Add("Anime", "Film, radio and television");
            wddict.Add("Radio", "Q872"); subj1dict.Add("Radio", "Arts and recreation"); subj2dict.Add("Radio", "Film, radio and television");
            wddict.Add("Television", "Q289"); subj1dict.Add("Television", "Arts and recreation"); subj2dict.Add("Television", "Film, radio and television");



            wddict.Add("Music", "Q638"); subj1dict.Add("Music", "Arts and recreation"); subj2dict.Add("Music", "Music");
            wddict.Add("Song", "Q7366"); subj1dict.Add("Song", "Arts and recreation"); subj2dict.Add("Song", "Music");

            wddict.Add("Blues", "Q9759"); subj1dict.Add("Blues", "Arts and recreation"); subj2dict.Add("Blues", "Music");
            wddict.Add("Classical music", "Q9730"); subj1dict.Add("Classical music", "Arts and recreation"); subj2dict.Add("Classical music", "Music");
            wddict.Add("Opera", "Q1344"); subj1dict.Add("Opera", "Arts and recreation"); subj2dict.Add("Opera", "Music");
            wddict.Add("Symphony", "Q9734"); subj1dict.Add("Symphony", "Arts and recreation"); subj2dict.Add("Symphony", "Music");
            wddict.Add("Electronic music", "Q9778"); subj1dict.Add("Electronic music", "Arts and recreation"); subj2dict.Add("Electronic music", "Music");
            wddict.Add("Flamenco", "Q9764"); subj1dict.Add("Flamenco", "Arts and recreation"); subj2dict.Add("Flamenco", "Music");
            wddict.Add("Hip hop", "Q11401"); subj1dict.Add("Hip hop", "Arts and recreation"); subj2dict.Add("Hip hop", "Music");
            wddict.Add("Jazz", "Q8341"); subj1dict.Add("Jazz", "Arts and recreation"); subj2dict.Add("Jazz", "Music");
            wddict.Add("Reggae", "Q9794"); subj1dict.Add("Reggae", "Arts and recreation"); subj2dict.Add("Reggae", "Music");
            wddict.Add("Rock music", "Q11399"); subj1dict.Add("Rock music", "Arts and recreation"); subj2dict.Add("Rock music", "Music");
            wddict.Add("Samba", "Q11403"); subj1dict.Add("Samba", "Arts and recreation"); subj2dict.Add("Samba", "Music");

            wddict.Add("Drum", "Q11404"); subj1dict.Add("Drum", "Arts and recreation"); subj2dict.Add("Drum", "Music");
            wddict.Add("Flute", "Q11405"); subj1dict.Add("Flute", "Arts and recreation"); subj2dict.Add("Flute", "Music");
            wddict.Add("Guitar", "Q6607"); subj1dict.Add("Guitar", "Arts and recreation"); subj2dict.Add("Guitar", "Music");
            wddict.Add("Piano", "Q5994"); subj1dict.Add("Piano", "Arts and recreation"); subj2dict.Add("Piano", "Music");
            wddict.Add("Trumpet", "Q8338"); subj1dict.Add("Trumpet", "Arts and recreation"); subj2dict.Add("Trumpet", "Music");
            wddict.Add("Violin", "Q8355"); subj1dict.Add("Violin", "Arts and recreation"); subj2dict.Add("Violin", "Music");



            wddict.Add("Game", "Q11410"); subj1dict.Add("Game", "Arts and recreation"); subj2dict.Add("Game", "Recreation");
            wddict.Add("Backgammon", "Q11411"); subj1dict.Add("Backgammon", "Arts and recreation"); subj2dict.Add("Backgammon", "Recreation");
            wddict.Add("Chess", "Q718"); subj1dict.Add("Chess", "Arts and recreation"); subj2dict.Add("Chess", "Recreation");
            wddict.Add("Go (game)", "Q11413"); subj1dict.Add("Go (game)", "Arts and recreation"); subj2dict.Add("Go (game)", "Recreation");
            wddict.Add("Gambling", "Q11416"); subj1dict.Add("Gambling", "Arts and recreation"); subj2dict.Add("Gambling", "Recreation");
            wddict.Add("Martial arts", "Q11417"); subj1dict.Add("Martial arts", "Arts and recreation"); subj2dict.Add("Martial arts", "Recreation");
            wddict.Add("Karate", "Q11419"); subj1dict.Add("Karate", "Arts and recreation"); subj2dict.Add("Karate", "Recreation");
            wddict.Add("Judo", "Q11420"); subj1dict.Add("Judo", "Arts and recreation"); subj2dict.Add("Judo", "Recreation");
            wddict.Add("Olympic Games", "Q5389"); subj1dict.Add("Olympic Games", "Arts and recreation"); subj2dict.Add("Olympic Games", "Recreation");
            wddict.Add("Sport", "Q349"); subj1dict.Add("Sport", "Arts and recreation"); subj2dict.Add("Sport", "Recreation");
            wddict.Add("Athletics", "Q542"); subj1dict.Add("Athletics", "Arts and recreation"); subj2dict.Add("Athletics", "Recreation");
            wddict.Add("Auto racing", "Q5386"); subj1dict.Add("Auto racing", "Arts and recreation"); subj2dict.Add("Auto racing", "Recreation");
            wddict.Add("Baseball", "Q5369"); subj1dict.Add("Baseball", "Arts and recreation"); subj2dict.Add("Baseball", "Recreation");
            wddict.Add("Basketball", "Q5372"); subj1dict.Add("Basketball", "Arts and recreation"); subj2dict.Add("Basketball", "Recreation");
            wddict.Add("Cricket", "Q5375"); subj1dict.Add("Cricket", "Arts and recreation"); subj2dict.Add("Cricket", "Recreation");
            wddict.Add("Association football", "Q2736"); subj1dict.Add("Association football", "Arts and recreation"); subj2dict.Add("Association football", "Recreation");
            wddict.Add("Golf", "Q5377"); subj1dict.Add("Golf", "Arts and recreation"); subj2dict.Add("Golf", "Recreation");
            wddict.Add("Rugby", "Q5378"); subj1dict.Add("Rugby", "Arts and recreation"); subj2dict.Add("Rugby", "Recreation");
            wddict.Add("Tennis", "Q847"); subj1dict.Add("Tennis", "Arts and recreation"); subj2dict.Add("Tennis", "Recreation");
            wddict.Add("Toy", "Q11422"); subj1dict.Add("Toy", "Arts and recreation"); subj2dict.Add("Toy", "Recreation");







            wddict.Add("History", "Q309"); subj1dict.Add("History", "History and geography"); subj2dict.Add("History", "History");



            wddict.Add("Prehistory", "Q11756"); subj1dict.Add("Prehistory", "History and geography"); subj2dict.Add("Prehistory", "Prehistory and ancient world");
            wddict.Add("Stone Age", "Q11759"); subj1dict.Add("Stone Age", "History and geography"); subj2dict.Add("Stone Age", "Prehistory and ancient world");
            wddict.Add("Bronze Age", "Q11761"); subj1dict.Add("Bronze Age", "History and geography"); subj2dict.Add("Bronze Age", "Prehistory and ancient world");
            wddict.Add("Iron Age", "Q11764"); subj1dict.Add("Iron Age", "History and geography"); subj2dict.Add("Iron Age", "Prehistory and ancient world");
            wddict.Add("Mesopotamia", "Q11767"); subj1dict.Add("Mesopotamia", "History and geography"); subj2dict.Add("Mesopotamia", "Prehistory and ancient world");
            wddict.Add("Ancient Egypt", "Q11768"); subj1dict.Add("Ancient Egypt", "History and geography"); subj2dict.Add("Ancient Egypt", "Prehistory and ancient world");
            wddict.Add("Ancient Greece", "Q11772"); subj1dict.Add("Ancient Greece", "History and geography"); subj2dict.Add("Ancient Greece", "Prehistory and ancient world");
            wddict.Add("Roman Empire", "Q2277"); subj1dict.Add("Roman Empire", "History and geography"); subj2dict.Add("Roman Empire", "Prehistory and ancient world");
            wddict.Add("Han Dynasty", "Q7209"); subj1dict.Add("Han Dynasty", "History and geography"); subj2dict.Add("Han Dynasty", "Prehistory and ancient world");
            wddict.Add("Gupta Empire", "Q11774"); subj1dict.Add("Gupta Empire", "History and geography"); subj2dict.Add("Gupta Empire", "Prehistory and ancient world");



            wddict.Add("Abbasid Caliphate", "Q12536"); subj1dict.Add("Abbasid Caliphate", "History and geography"); subj2dict.Add("Abbasid Caliphate", "Middle Ages and Early Modern");
            wddict.Add("Age of Enlightenment", "Q12539"); subj1dict.Add("Age of Enlightenment", "History and geography"); subj2dict.Add("Age of Enlightenment", "Middle Ages and Early Modern");
            wddict.Add("Aztec", "Q12542"); subj1dict.Add("Aztec", "History and geography"); subj2dict.Add("Aztec", "Middle Ages and Early Modern");
            wddict.Add("Byzantine Empire", "Q12544"); subj1dict.Add("Byzantine Empire", "History and geography"); subj2dict.Add("Byzantine Empire", "Middle Ages and Early Modern");
            wddict.Add("Crusades", "Q12546"); subj1dict.Add("Crusades", "History and geography"); subj2dict.Add("Crusades", "Middle Ages and Early Modern");
            wddict.Add("Holy Roman Empire", "Q12548"); subj1dict.Add("Holy Roman Empire", "History and geography"); subj2dict.Add("Holy Roman Empire", "Middle Ages and Early Modern");
            wddict.Add("Hundred Years' War", "Q12551"); subj1dict.Add("Hundred Years' War", "History and geography"); subj2dict.Add("Hundred Years' War", "Middle Ages and Early Modern");
            wddict.Add("Middle Ages", "Q12554"); subj1dict.Add("Middle Ages", "History and geography"); subj2dict.Add("Middle Ages", "Middle Ages and Early Modern");
            wddict.Add("Mongol Empire", "Q12557"); subj1dict.Add("Mongol Empire", "History and geography"); subj2dict.Add("Mongol Empire", "Middle Ages and Early Modern");
            wddict.Add("Ming Dynasty", "Q9903"); subj1dict.Add("Ming Dynasty", "History and geography"); subj2dict.Add("Ming Dynasty", "Middle Ages and Early Modern");
            wddict.Add("Ottoman Empire", "Q12560"); subj1dict.Add("Ottoman Empire", "History and geography"); subj2dict.Add("Ottoman Empire", "Middle Ages and Early Modern");
            wddict.Add("Protestant Reformation", "Q12562"); subj1dict.Add("Protestant Reformation", "History and geography"); subj2dict.Add("Protestant Reformation", "Middle Ages and Early Modern");
            wddict.Add("Thirty Years' War", "Q2487"); subj1dict.Add("Thirty Years' War", "History and geography"); subj2dict.Add("Thirty Years' War", "Middle Ages and Early Modern");
            wddict.Add("Renaissance", "Q4692"); subj1dict.Add("Renaissance", "History and geography"); subj2dict.Add("Renaissance", "Middle Ages and Early Modern");
            wddict.Add("Tang Dynasty", "Q9683"); subj1dict.Add("Tang Dynasty", "History and geography"); subj2dict.Add("Tang Dynasty", "Middle Ages and Early Modern");
            wddict.Add("Vikings", "Q12567"); subj1dict.Add("Vikings", "History and geography"); subj2dict.Add("Vikings", "Middle Ages and Early Modern");



            wddict.Add("Arab-Israeli conflict", "Q8669"); subj1dict.Add("Arab-Israeli conflict", "History and geography"); subj2dict.Add("Arab-Israeli conflict", "Modern");
            wddict.Add("American Civil War", "Q8676"); subj1dict.Add("American Civil War", "History and geography"); subj2dict.Add("American Civil War", "Modern");
            wddict.Add("Apartheid", "Q11409"); subj1dict.Add("Apartheid", "History and geography"); subj2dict.Add("Apartheid", "Modern");
            wddict.Add("British Empire", "Q8680"); subj1dict.Add("British Empire", "History and geography"); subj2dict.Add("British Empire", "Modern");
            wddict.Add("Cold War", "Q8683"); subj1dict.Add("Cold War", "History and geography"); subj2dict.Add("Cold War", "Modern");
            wddict.Add("Cultural Revolution", "Q8690"); subj1dict.Add("Cultural Revolution", "History and geography"); subj2dict.Add("Cultural Revolution", "Modern");
            wddict.Add("French Revolution", "Q6534"); subj1dict.Add("French Revolution", "History and geography"); subj2dict.Add("French Revolution", "Modern");
            wddict.Add("Great Depression", "Q8698"); subj1dict.Add("Great Depression", "History and geography"); subj2dict.Add("Great Depression", "Modern");
            wddict.Add("The Holocaust", "Q2763"); subj1dict.Add("The Holocaust", "History and geography"); subj2dict.Add("The Holocaust", "Modern");
            wddict.Add("Industrial Revolution", "Q2269"); subj1dict.Add("Industrial Revolution", "History and geography"); subj2dict.Add("Industrial Revolution", "Modern");
            wddict.Add("Nazi Germany", "Q7318"); subj1dict.Add("Nazi Germany", "History and geography"); subj2dict.Add("Nazi Germany", "Modern");
            wddict.Add("Meiji Restoration", "Q8707"); subj1dict.Add("Meiji Restoration", "History and geography"); subj2dict.Add("Meiji Restoration", "Modern");
            wddict.Add("Russian Revolution", "Q8729"); subj1dict.Add("Russian Revolution", "History and geography"); subj2dict.Add("Russian Revolution", "Modern");
            wddict.Add("Qing Dynasty", "Q8733"); subj1dict.Add("Qing Dynasty", "History and geography"); subj2dict.Add("Qing Dynasty", "Modern");
            wddict.Add("Treaty of Versailles", "Q8736"); subj1dict.Add("Treaty of Versailles", "History and geography"); subj2dict.Add("Treaty of Versailles", "Modern");
            wddict.Add("Vietnam War", "Q8740"); subj1dict.Add("Vietnam War", "History and geography"); subj2dict.Add("Vietnam War", "Modern");
            wddict.Add("World War I", "Q361"); subj1dict.Add("World War I", "History and geography"); subj2dict.Add("World War I", "Modern");
            wddict.Add("World War II", "Q362"); subj1dict.Add("World War II", "History and geography"); subj2dict.Add("World War II", "Modern");





            wddict.Add("Geography", "Q1071"); subj1dict.Add("Geography", "History and geography"); subj2dict.Add("Geography", "Geography");
            wddict.Add("City", "Q515"); subj1dict.Add("City", "History and geography"); subj2dict.Add("City", "Geography");
            wddict.Add("Continent", "Q5107"); subj1dict.Add("Continent", "History and geography"); subj2dict.Add("Continent", "Geography");
            wddict.Add("Forest", "Q4421"); subj1dict.Add("Forest", "History and geography"); subj2dict.Add("Forest", "Geography");
            wddict.Add("Mountain", "Q8502"); subj1dict.Add("Mountain", "History and geography"); subj2dict.Add("Mountain", "Geography");
            wddict.Add("Desert", "Q8514"); subj1dict.Add("Desert", "History and geography"); subj2dict.Add("Desert", "Geography");
            wddict.Add("North Pole", "Q934"); subj1dict.Add("North Pole", "History and geography"); subj2dict.Add("North Pole", "Geography");
            wddict.Add("Ocean", "Q9430"); subj1dict.Add("Ocean", "History and geography"); subj2dict.Add("Ocean", "Geography");
            wddict.Add("River", "Q4022"); subj1dict.Add("River", "History and geography"); subj2dict.Add("River", "Geography");
            wddict.Add("Sea", "Q165"); subj1dict.Add("Sea", "History and geography"); subj2dict.Add("Sea", "Geography");
            wddict.Add("South Pole", "Q933"); subj1dict.Add("South Pole", "History and geography"); subj2dict.Add("South Pole", "Geography");



            wddict.Add("Africa", "Q15"); subj1dict.Add("Africa", "History and geography"); subj2dict.Add("Africa", "Continents and major regions");
            wddict.Add("Antarctica", "Q51"); subj1dict.Add("Antarctica", "History and geography"); subj2dict.Add("Antarctica", "Continents and major regions");
            wddict.Add("Asia", "Q48"); subj1dict.Add("Asia", "History and geography"); subj2dict.Add("Asia", "Continents and major regions");
            wddict.Add("Europe", "Q46"); subj1dict.Add("Europe", "History and geography"); subj2dict.Add("Europe", "Continents and major regions");
            wddict.Add("Middle East", "Q7204"); subj1dict.Add("Middle East", "History and geography"); subj2dict.Add("Middle East", "Continents and major regions");
            wddict.Add("North America", "Q49"); subj1dict.Add("North America", "History and geography"); subj2dict.Add("North America", "Continents and major regions");
            wddict.Add("Oceania", "Q538"); subj1dict.Add("Oceania", "History and geography"); subj2dict.Add("Oceania", "Continents and major regions");
            wddict.Add("South America", "Q18"); subj1dict.Add("South America", "History and geography"); subj2dict.Add("South America", "Continents and major regions");


            wddict.Add("List of countries", ""); subj1dict.Add("List of countries", "History and geography"); subj2dict.Add("List of countries", "Countries");

            wddict.Add("Afghanistan", "Q889"); subj1dict.Add("Afghanistan", "History and geography"); subj2dict.Add("Afghanistan", "Countries");
            wddict.Add("Algeria", "Q262"); subj1dict.Add("Algeria", "History and geography"); subj2dict.Add("Algeria", "Countries");
            wddict.Add("Argentina", "Q414"); subj1dict.Add("Argentina", "History and geography"); subj2dict.Add("Argentina", "Countries");
            wddict.Add("Australia", "Q408"); subj1dict.Add("Australia", "History and geography"); subj2dict.Add("Australia", "Countries");
            wddict.Add("Austria", "Q40"); subj1dict.Add("Austria", "History and geography"); subj2dict.Add("Austria", "Countries");
            wddict.Add("Bangladesh", "Q902"); subj1dict.Add("Bangladesh", "History and geography"); subj2dict.Add("Bangladesh", "Countries");
            wddict.Add("Brazil", "Q155"); subj1dict.Add("Brazil", "History and geography"); subj2dict.Add("Brazil", "Countries");
            wddict.Add("Canada", "Q16"); subj1dict.Add("Canada", "History and geography"); subj2dict.Add("Canada", "Countries");
            wddict.Add("China", "Q148"); subj1dict.Add("China", "History and geography"); subj2dict.Add("China", "Countries");
            wddict.Add("Democratic Republic of the Congo", "Q974"); subj1dict.Add("Democratic Republic of the Congo", "History and geography"); subj2dict.Add("Democratic Republic of the Congo", "Countries");
            wddict.Add("Cuba", "Q241"); subj1dict.Add("Cuba", "History and geography"); subj2dict.Add("Cuba", "Countries");
            wddict.Add("Egypt", "Q79"); subj1dict.Add("Egypt", "History and geography"); subj2dict.Add("Egypt", "Countries");
            wddict.Add("Ethiopia", "Q115"); subj1dict.Add("Ethiopia", "History and geography"); subj2dict.Add("Ethiopia", "Countries");
            wddict.Add("France", "Q142"); subj1dict.Add("France", "History and geography"); subj2dict.Add("France", "Countries");
            wddict.Add("Germany", "Q183"); subj1dict.Add("Germany", "History and geography"); subj2dict.Add("Germany", "Countries");
            wddict.Add("India", "Q668"); subj1dict.Add("India", "History and geography"); subj2dict.Add("India", "Countries");
            wddict.Add("Indonesia", "Q252"); subj1dict.Add("Indonesia", "History and geography"); subj2dict.Add("Indonesia", "Countries");
            wddict.Add("Iran", "Q794"); subj1dict.Add("Iran", "History and geography"); subj2dict.Add("Iran", "Countries");
            wddict.Add("Iraq", "Q796"); subj1dict.Add("Iraq", "History and geography"); subj2dict.Add("Iraq", "Countries");
            wddict.Add("Israel", "Q801"); subj1dict.Add("Israel", "History and geography"); subj2dict.Add("Israel", "Countries");
            wddict.Add("Italy", "Q38"); subj1dict.Add("Italy", "History and geography"); subj2dict.Add("Italy", "Countries");
            wddict.Add("Japan", "Q17"); subj1dict.Add("Japan", "History and geography"); subj2dict.Add("Japan", "Countries");
            wddict.Add("Mexico", "Q96"); subj1dict.Add("Mexico", "History and geography"); subj2dict.Add("Mexico", "Countries");
            wddict.Add("Netherlands", "Q55"); subj1dict.Add("Netherlands", "History and geography"); subj2dict.Add("Netherlands", "Countries");
            wddict.Add("New Zealand", "Q664"); subj1dict.Add("New Zealand", "History and geography"); subj2dict.Add("New Zealand", "Countries");
            wddict.Add("Nigeria", "Q1033"); subj1dict.Add("Nigeria", "History and geography"); subj2dict.Add("Nigeria", "Countries");
            wddict.Add("Pakistan", "Q843"); subj1dict.Add("Pakistan", "History and geography"); subj2dict.Add("Pakistan", "Countries");
            wddict.Add("Poland", "Q36"); subj1dict.Add("Poland", "History and geography"); subj2dict.Add("Poland", "Countries");
            wddict.Add("Portugal", "Q45"); subj1dict.Add("Portugal", "History and geography"); subj2dict.Add("Portugal", "Countries");
            wddict.Add("Russia", "Q159"); subj1dict.Add("Russia", "History and geography"); subj2dict.Add("Russia", "Countries");
            wddict.Add("Saudi Arabia", "Q851"); subj1dict.Add("Saudi Arabia", "History and geography"); subj2dict.Add("Saudi Arabia", "Countries");
            wddict.Add("Singapore", "Q334"); subj1dict.Add("Singapore", "History and geography"); subj2dict.Add("Singapore", "Countries");
            wddict.Add("South Africa", "Q258"); subj1dict.Add("South Africa", "History and geography"); subj2dict.Add("South Africa", "Countries");
            wddict.Add("South Korea", "Q884"); subj1dict.Add("South Korea", "History and geography"); subj2dict.Add("South Korea", "Countries");
            wddict.Add("Spain", "Q29"); subj1dict.Add("Spain", "History and geography"); subj2dict.Add("Spain", "Countries");
            wddict.Add("Sudan", "Q1049"); subj1dict.Add("Sudan", "History and geography"); subj2dict.Add("Sudan", "Countries");
            wddict.Add("Switzerland", "Q39"); subj1dict.Add("Switzerland", "History and geography"); subj2dict.Add("Switzerland", "Countries");
            wddict.Add("Tanzania", "Q924"); subj1dict.Add("Tanzania", "History and geography"); subj2dict.Add("Tanzania", "Countries");
            wddict.Add("Thailand", "Q869"); subj1dict.Add("Thailand", "History and geography"); subj2dict.Add("Thailand", "Countries");
            wddict.Add("Turkey", "Q43"); subj1dict.Add("Turkey", "History and geography"); subj2dict.Add("Turkey", "Countries");
            wddict.Add("Ukraine", "Q212"); subj1dict.Add("Ukraine", "History and geography"); subj2dict.Add("Ukraine", "Countries");
            wddict.Add("United Kingdom", "Q145"); subj1dict.Add("United Kingdom", "History and geography"); subj2dict.Add("United Kingdom", "Countries");
            wddict.Add("United States", "Q30"); subj1dict.Add("United States", "History and geography"); subj2dict.Add("United States", "Countries");
            wddict.Add("Vatican City", "Q237"); subj1dict.Add("Vatican City", "History and geography"); subj2dict.Add("Vatican City", "Countries");
            wddict.Add("Venezuela", "Q717"); subj1dict.Add("Venezuela", "History and geography"); subj2dict.Add("Venezuela", "Countries");
            wddict.Add("Vietnam", "Q881"); subj1dict.Add("Vietnam", "History and geography"); subj2dict.Add("Vietnam", "Countries");


            wddict.Add("Amsterdam", "Q727"); subj1dict.Add("Amsterdam", "History and geography"); subj2dict.Add("Amsterdam", "Cities");
            wddict.Add("Athens", "Q1524"); subj1dict.Add("Athens", "History and geography"); subj2dict.Add("Athens", "Cities");
            wddict.Add("Baghdad", "Q1530"); subj1dict.Add("Baghdad", "History and geography"); subj2dict.Add("Baghdad", "Cities");
            wddict.Add("Bangkok", "Q1861"); subj1dict.Add("Bangkok", "History and geography"); subj2dict.Add("Bangkok", "Cities");
            wddict.Add("Beijing", "Q956"); subj1dict.Add("Beijing", "History and geography"); subj2dict.Add("Beijing", "Cities");
            wddict.Add("Berlin", "Q64"); subj1dict.Add("Berlin", "History and geography"); subj2dict.Add("Berlin", "Cities");
            wddict.Add("Bogotá", "Q2841"); subj1dict.Add("Bogotá", "History and geography"); subj2dict.Add("Bogotá", "Cities");
            wddict.Add("Brussels", "Q240"); subj1dict.Add("Brussels", "History and geography"); subj2dict.Add("Brussels", "Cities");
            wddict.Add("Buenos Aires", "Q1486"); subj1dict.Add("Buenos Aires", "History and geography"); subj2dict.Add("Buenos Aires", "Cities");
            wddict.Add("Cairo", "Q85"); subj1dict.Add("Cairo", "History and geography"); subj2dict.Add("Cairo", "Cities");
            wddict.Add("Cape Town", "Q5465"); subj1dict.Add("Cape Town", "History and geography"); subj2dict.Add("Cape Town", "Cities");
            wddict.Add("Damascus", "Q3766"); subj1dict.Add("Damascus", "History and geography"); subj2dict.Add("Damascus", "Cities");
            wddict.Add("Delhi", "Q1353"); subj1dict.Add("Delhi", "History and geography"); subj2dict.Add("Delhi", "Cities");
            wddict.Add("Dhaka", "Q1354"); subj1dict.Add("Dhaka", "History and geography"); subj2dict.Add("Dhaka", "Cities");
            wddict.Add("Dubai", "Q613"); subj1dict.Add("Dubai", "History and geography"); subj2dict.Add("Dubai", "Cities");
            wddict.Add("Hong Kong", "Q8646"); subj1dict.Add("Hong Kong", "History and geography"); subj2dict.Add("Hong Kong", "Cities");
            wddict.Add("Istanbul", "Q406"); subj1dict.Add("Istanbul", "History and geography"); subj2dict.Add("Istanbul", "Cities");
            wddict.Add("Jakarta", "Q3630"); subj1dict.Add("Jakarta", "History and geography"); subj2dict.Add("Jakarta", "Cities");
            wddict.Add("Jerusalem", "Q1218"); subj1dict.Add("Jerusalem", "History and geography"); subj2dict.Add("Jerusalem", "Cities");
            wddict.Add("Karachi", "Q8660"); subj1dict.Add("Karachi", "History and geography"); subj2dict.Add("Karachi", "Cities");
            wddict.Add("Kinshasa", "Q3838"); subj1dict.Add("Kinshasa", "History and geography"); subj2dict.Add("Kinshasa", "Cities");
            wddict.Add("Kolkata", "Q1348"); subj1dict.Add("Kolkata", "History and geography"); subj2dict.Add("Kolkata", "Cities");
            wddict.Add("Lagos", "Q8673"); subj1dict.Add("Lagos", "History and geography"); subj2dict.Add("Lagos", "Cities");
            wddict.Add("London", "Q84"); subj1dict.Add("London", "History and geography"); subj2dict.Add("London", "Cities");
            wddict.Add("Los Angeles", "Q65"); subj1dict.Add("Los Angeles", "History and geography"); subj2dict.Add("Los Angeles", "Cities");
            wddict.Add("Madrid", "Q2807"); subj1dict.Add("Madrid", "History and geography"); subj2dict.Add("Madrid", "Cities");
            wddict.Add("Mecca", "Q5806"); subj1dict.Add("Mecca", "History and geography"); subj2dict.Add("Mecca", "Cities");
            wddict.Add("Mexico City", "Q1489"); subj1dict.Add("Mexico City", "History and geography"); subj2dict.Add("Mexico City", "Cities");
            wddict.Add("Moscow", "Q649"); subj1dict.Add("Moscow", "History and geography"); subj2dict.Add("Moscow", "Cities");
            wddict.Add("Mumbai", "Q1156"); subj1dict.Add("Mumbai", "History and geography"); subj2dict.Add("Mumbai", "Cities");
            wddict.Add("Nairobi", "Q3870"); subj1dict.Add("Nairobi", "History and geography"); subj2dict.Add("Nairobi", "Cities");
            wddict.Add("New York City", "Q60"); subj1dict.Add("New York City", "History and geography"); subj2dict.Add("New York City", "Cities");
            wddict.Add("Paris", "Q90"); subj1dict.Add("Paris", "History and geography"); subj2dict.Add("Paris", "Cities");
            wddict.Add("Rio de Janeiro", "Q8678"); subj1dict.Add("Rio de Janeiro", "History and geography"); subj2dict.Add("Rio de Janeiro", "Cities");
            wddict.Add("Rome", "Q220"); subj1dict.Add("Rome", "History and geography"); subj2dict.Add("Rome", "Cities");
            wddict.Add("Saint Petersburg", "Q656"); subj1dict.Add("Saint Petersburg", "History and geography"); subj2dict.Add("Saint Petersburg", "Cities");
            wddict.Add("São Paulo", "Q174"); subj1dict.Add("São Paulo", "History and geography"); subj2dict.Add("São Paulo", "Cities");
            wddict.Add("Seoul", "Q8684"); subj1dict.Add("Seoul", "History and geography"); subj2dict.Add("Seoul", "Cities");
            wddict.Add("Shanghai", "Q8686"); subj1dict.Add("Shanghai", "History and geography"); subj2dict.Add("Shanghai", "Cities");
            wddict.Add("Sydney", "Q3130"); subj1dict.Add("Sydney", "History and geography"); subj2dict.Add("Sydney", "Cities");
            wddict.Add("Tehran", "Q3616"); subj1dict.Add("Tehran", "History and geography"); subj2dict.Add("Tehran", "Cities");
            wddict.Add("Tokyo", "Q1490"); subj1dict.Add("Tokyo", "History and geography"); subj2dict.Add("Tokyo", "Cities");
            wddict.Add("Vienna", "Q1741"); subj1dict.Add("Vienna", "History and geography"); subj2dict.Add("Vienna", "Cities");
            wddict.Add("Washington, D.C.", "Q61"); subj1dict.Add("Washington, D.C.", "History and geography"); subj2dict.Add("Washington, D.C.", "Cities");



            wddict.Add("Amazon River", "Q3783"); subj1dict.Add("Amazon River", "History and geography"); subj2dict.Add("Amazon River", "Bodies of water");
            wddict.Add("Arctic Ocean", "Q788"); subj1dict.Add("Arctic Ocean", "History and geography"); subj2dict.Add("Arctic Ocean", "Bodies of water");
            wddict.Add("Atlantic Ocean", "Q97"); subj1dict.Add("Atlantic Ocean", "History and geography"); subj2dict.Add("Atlantic Ocean", "Bodies of water");
            wddict.Add("Baltic Sea", "Q545"); subj1dict.Add("Baltic Sea", "History and geography"); subj2dict.Add("Baltic Sea", "Bodies of water");
            wddict.Add("Black Sea", "Q166"); subj1dict.Add("Black Sea", "History and geography"); subj2dict.Add("Black Sea", "Bodies of water");
            wddict.Add("Caribbean Sea", "Q1247"); subj1dict.Add("Caribbean Sea", "History and geography"); subj2dict.Add("Caribbean Sea", "Bodies of water");
            wddict.Add("Caspian Sea", "Q5484"); subj1dict.Add("Caspian Sea", "History and geography"); subj2dict.Add("Caspian Sea", "Bodies of water");
            wddict.Add("Congo River", "Q3503"); subj1dict.Add("Congo River", "History and geography"); subj2dict.Add("Congo River", "Bodies of water");
            wddict.Add("Danube", "Q1653"); subj1dict.Add("Danube", "History and geography"); subj2dict.Add("Danube", "Bodies of water");
            wddict.Add("Ganges", "Q5089"); subj1dict.Add("Ganges", "History and geography"); subj2dict.Add("Ganges", "Bodies of water");
            wddict.Add("Great Barrier Reef", "Q7343"); subj1dict.Add("Great Barrier Reef", "History and geography"); subj2dict.Add("Great Barrier Reef", "Bodies of water");
            wddict.Add("Great Lakes", "Q7347"); subj1dict.Add("Great Lakes", "History and geography"); subj2dict.Add("Great Lakes", "Bodies of water");
            wddict.Add("Indian Ocean", "Q1239"); subj1dict.Add("Indian Ocean", "History and geography"); subj2dict.Add("Indian Ocean", "Bodies of water");
            wddict.Add("Indus River", "Q7348"); subj1dict.Add("Indus River", "History and geography"); subj2dict.Add("Indus River", "Bodies of water");
            wddict.Add("Lake Baikal", "Q5513"); subj1dict.Add("Lake Baikal", "History and geography"); subj2dict.Add("Lake Baikal", "Bodies of water");
            wddict.Add("Lake Tanganyika", "Q5511"); subj1dict.Add("Lake Tanganyika", "History and geography"); subj2dict.Add("Lake Tanganyika", "Bodies of water");
            wddict.Add("Lake Victoria", "Q5505"); subj1dict.Add("Lake Victoria", "History and geography"); subj2dict.Add("Lake Victoria", "Bodies of water");
            wddict.Add("Mediterranean Sea", "Q4918"); subj1dict.Add("Mediterranean Sea", "History and geography"); subj2dict.Add("Mediterranean Sea", "Bodies of water");
            wddict.Add("Mississippi River", "Q1497"); subj1dict.Add("Mississippi River", "History and geography"); subj2dict.Add("Mississippi River", "Bodies of water");
            wddict.Add("Niger River", "Q3542"); subj1dict.Add("Niger River", "History and geography"); subj2dict.Add("Niger River", "Bodies of water");
            wddict.Add("Nile", "Q3392"); subj1dict.Add("Nile", "History and geography"); subj2dict.Add("Nile", "Bodies of water");
            wddict.Add("North Sea", "Q1693"); subj1dict.Add("North Sea", "History and geography"); subj2dict.Add("North Sea", "Bodies of water");
            wddict.Add("Pacific Ocean", "Q98"); subj1dict.Add("Pacific Ocean", "History and geography"); subj2dict.Add("Pacific Ocean", "Bodies of water");
            wddict.Add("Panama Canal", "Q7350"); subj1dict.Add("Panama Canal", "History and geography"); subj2dict.Add("Panama Canal", "Bodies of water");
            wddict.Add("Rhine", "Q584"); subj1dict.Add("Rhine", "History and geography"); subj2dict.Add("Rhine", "Bodies of water");
            wddict.Add("Suez Canal", "Q899"); subj1dict.Add("Suez Canal", "History and geography"); subj2dict.Add("Suez Canal", "Bodies of water");
            wddict.Add("Southern Ocean", "Q7354"); subj1dict.Add("Southern Ocean", "History and geography"); subj2dict.Add("Southern Ocean", "Bodies of water");
            wddict.Add("Volga River", "Q626"); subj1dict.Add("Volga River", "History and geography"); subj2dict.Add("Volga River", "Bodies of water");
            wddict.Add("Yangtze River", "Q5413"); subj1dict.Add("Yangtze River", "History and geography"); subj2dict.Add("Yangtze River", "Bodies of water");
            wddict.Add("Yellow River", "Q7355"); subj1dict.Add("Yellow River", "History and geography"); subj2dict.Add("Yellow River", "Bodies of water");



            wddict.Add("Alps", "Q1286"); subj1dict.Add("Alps", "History and geography"); subj2dict.Add("Alps", "Mountains and deserts");
            wddict.Add("Andes", "Q5456"); subj1dict.Add("Andes", "History and geography"); subj2dict.Add("Andes", "Mountains and deserts");
            wddict.Add("Himalayas", "Q5451"); subj1dict.Add("Himalayas", "History and geography"); subj2dict.Add("Himalayas", "Mountains and deserts");
            wddict.Add("Mount Everest", "Q513"); subj1dict.Add("Mount Everest", "History and geography"); subj2dict.Add("Mount Everest", "Mountains and deserts");
            wddict.Add("Mount Kilimanjaro", "Q7296"); subj1dict.Add("Mount Kilimanjaro", "History and geography"); subj2dict.Add("Mount Kilimanjaro", "Mountains and deserts");
            wddict.Add("Rocky Mountains", "Q5463"); subj1dict.Add("Rocky Mountains", "History and geography"); subj2dict.Add("Rocky Mountains", "Mountains and deserts");
            wddict.Add("Sahara", "Q6583"); subj1dict.Add("Sahara", "History and geography"); subj2dict.Add("Sahara", "Mountains and deserts");

            foreach (string w in wddict.Keys)
            {
                wdqdict.Add(wddict[w], w);
            }
        }

        
        public static void read_countries()
        {
            using (StreamReader sr = new StreamReader(dumpfolder + "wikidata-list-country.txt"))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    if (String.IsNullOrEmpty(s))
                        continue;
                    string[] words = s.Split('\t');
                    countrynamedict.Add(words[0], words[1]);
                }

            }

        }

        public static bool human_properties(JObject wd)
        {
            int nprop = 0;

            Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();

            for (int i = 0; i < humanprops.Length; i++)
            {
                if (get_claims(wd, i, dictObj).Count > 0)
                    nprop++;
            }

            Console.WriteLine(nprop + " human properties");
            return (nprop >= 3);
        }




        public static bool is_latin(string name)
        {
            return (get_alphabet(name) == "latin");
        }

        public static string get_alphabet(string name)
        {
            char[] letters = name.ToCharArray();
            int n = 0;
            int sum = 0;
            //int nlatin = 0;
            Dictionary<string, int> alphdir = new Dictionary<string, int>();
            foreach (char c in letters)
            {
                int uc = Convert.ToInt32(c);
                sum += uc;
                string alphabet = "none";
                if (uc <= 0x0040) alphabet = "none";
                //else if ((uc >= 0x0030) && (uc <= 0x0039)) alphabet = "number";
                //else if ((uc >= 0x0020) && (uc <= 0x0040)) alphabet = "punctuation";
                else if ((uc >= 0x0041) && (uc <= 0x007F)) alphabet = "latin";
                else if ((uc >= 0x00A0) && (uc <= 0x00FF)) alphabet = "latin";
                else if ((uc >= 0x0100) && (uc <= 0x017F)) alphabet = "latin";
                else if ((uc >= 0x0180) && (uc <= 0x024F)) alphabet = "latin";
                else if ((uc >= 0x0250) && (uc <= 0x02AF)) alphabet = "phonetic";
                else if ((uc >= 0x02B0) && (uc <= 0x02FF)) alphabet = "spacing modifier letters";
                else if ((uc >= 0x0300) && (uc <= 0x036F)) alphabet = "combining diacritical marks";
                else if ((uc >= 0x0370) && (uc <= 0x03FF)) alphabet = "greek and coptic";
                else if ((uc >= 0x0400) && (uc <= 0x04FF)) alphabet = "cyrillic";
                else if ((uc >= 0x0500) && (uc <= 0x052F)) alphabet = "cyrillic";
                else if ((uc >= 0x0530) && (uc <= 0x058F)) alphabet = "armenian";
                else if ((uc >= 0x0590) && (uc <= 0x05FF)) alphabet = "hebrew";
                else if ((uc >= 0x0600) && (uc <= 0x06FF)) alphabet = "arabic";
                else if ((uc >= 0x0700) && (uc <= 0x074F)) alphabet = "syriac";
                else if ((uc >= 0x0780) && (uc <= 0x07BF)) alphabet = "thaana";
                else if ((uc >= 0x0900) && (uc <= 0x097F)) alphabet = "devanagari";
                else if ((uc >= 0x0980) && (uc <= 0x09FF)) alphabet = "bengali";
                else if ((uc >= 0x0A00) && (uc <= 0x0A7F)) alphabet = "gurmukhi";
                else if ((uc >= 0x0A80) && (uc <= 0x0AFF)) alphabet = "gujarati";
                else if ((uc >= 0x0B00) && (uc <= 0x0B7F)) alphabet = "oriya";
                else if ((uc >= 0x0B80) && (uc <= 0x0BFF)) alphabet = "tamil";
                else if ((uc >= 0x0C00) && (uc <= 0x0C7F)) alphabet = "telugu";
                else if ((uc >= 0x0C80) && (uc <= 0x0CFF)) alphabet = "kannada";
                else if ((uc >= 0x0D00) && (uc <= 0x0D7F)) alphabet = "malayalam";
                else if ((uc >= 0x0D80) && (uc <= 0x0DFF)) alphabet = "sinhala";
                else if ((uc >= 0x0E00) && (uc <= 0x0E7F)) alphabet = "thai";
                else if ((uc >= 0x0E80) && (uc <= 0x0EFF)) alphabet = "lao";
                else if ((uc >= 0x0F00) && (uc <= 0x0FFF)) alphabet = "tibetan";
                else if ((uc >= 0x1000) && (uc <= 0x109F)) alphabet = "myanmar";
                else if ((uc >= 0x10A0) && (uc <= 0x10FF)) alphabet = "georgian";
                else if ((uc >= 0x1100) && (uc <= 0x11FF)) alphabet = "korean";
                else if ((uc >= 0x1200) && (uc <= 0x137F)) alphabet = "ethiopic";
                else if ((uc >= 0x13A0) && (uc <= 0x13FF)) alphabet = "cherokee";
                else if ((uc >= 0x1400) && (uc <= 0x167F)) alphabet = "unified canadian aboriginal syllabics";
                else if ((uc >= 0x1680) && (uc <= 0x169F)) alphabet = "ogham";
                else if ((uc >= 0x16A0) && (uc <= 0x16FF)) alphabet = "runic";
                else if ((uc >= 0x1700) && (uc <= 0x171F)) alphabet = "tagalog";
                else if ((uc >= 0x1720) && (uc <= 0x173F)) alphabet = "hanunoo";
                else if ((uc >= 0x1740) && (uc <= 0x175F)) alphabet = "buhid";
                else if ((uc >= 0x1760) && (uc <= 0x177F)) alphabet = "tagbanwa";
                else if ((uc >= 0x1780) && (uc <= 0x17FF)) alphabet = "khmer";
                else if ((uc >= 0x1800) && (uc <= 0x18AF)) alphabet = "mongolian";
                else if ((uc >= 0x1900) && (uc <= 0x194F)) alphabet = "limbu";
                else if ((uc >= 0x1950) && (uc <= 0x197F)) alphabet = "tai le";
                else if ((uc >= 0x19E0) && (uc <= 0x19FF)) alphabet = "khmer";
                else if ((uc >= 0x1D00) && (uc <= 0x1D7F)) alphabet = "phonetic";
                else if ((uc >= 0x1E00) && (uc <= 0x1EFF)) alphabet = "latin";
                else if ((uc >= 0x1F00) && (uc <= 0x1FFF)) alphabet = "greek and coptic";
                else if ((uc >= 0x2000) && (uc <= 0x206F)) alphabet = "none";
                else if ((uc >= 0x2070) && (uc <= 0x209F)) alphabet = "none";
                else if ((uc >= 0x20A0) && (uc <= 0x20CF)) alphabet = "none";
                else if ((uc >= 0x20D0) && (uc <= 0x20FF)) alphabet = "combining diacritical marks for symbols";
                else if ((uc >= 0x2100) && (uc <= 0x214F)) alphabet = "letterlike symbols";
                else if ((uc >= 0x2150) && (uc <= 0x218F)) alphabet = "none";
                else if ((uc >= 0x2190) && (uc <= 0x21FF)) alphabet = "none";
                else if ((uc >= 0x2200) && (uc <= 0x22FF)) alphabet = "none";
                else if ((uc >= 0x2300) && (uc <= 0x23FF)) alphabet = "none";
                else if ((uc >= 0x2400) && (uc <= 0x243F)) alphabet = "none";
                else if ((uc >= 0x2440) && (uc <= 0x245F)) alphabet = "optical character recognition";
                else if ((uc >= 0x2460) && (uc <= 0x24FF)) alphabet = "enclosed alphanumerics";
                else if ((uc >= 0x2500) && (uc <= 0x257F)) alphabet = "none";
                else if ((uc >= 0x2580) && (uc <= 0x259F)) alphabet = "none";
                else if ((uc >= 0x25A0) && (uc <= 0x25FF)) alphabet = "none";
                else if ((uc >= 0x2600) && (uc <= 0x26FF)) alphabet = "none";
                else if ((uc >= 0x2700) && (uc <= 0x27BF)) alphabet = "none";
                else if ((uc >= 0x27C0) && (uc <= 0x27EF)) alphabet = "none";
                else if ((uc >= 0x27F0) && (uc <= 0x27FF)) alphabet = "none";
                else if ((uc >= 0x2800) && (uc <= 0x28FF)) alphabet = "braille";
                else if ((uc >= 0x2900) && (uc <= 0x297F)) alphabet = "none";
                else if ((uc >= 0x2980) && (uc <= 0x29FF)) alphabet = "none";
                else if ((uc >= 0x2A00) && (uc <= 0x2AFF)) alphabet = "none";
                else if ((uc >= 0x2B00) && (uc <= 0x2BFF)) alphabet = "none";
                else if ((uc >= 0x2E80) && (uc <= 0x2EFF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x2F00) && (uc <= 0x2FDF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x2FF0) && (uc <= 0x2FFF)) alphabet = "none";
                else if ((uc >= 0x3000) && (uc <= 0x303F)) alphabet = "chinese/japanese";
                else if ((uc >= 0x3040) && (uc <= 0x309F)) alphabet = "chinese/japanese";
                else if ((uc >= 0x30A0) && (uc <= 0x30FF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x3100) && (uc <= 0x312F)) alphabet = "bopomofo";
                else if ((uc >= 0x3130) && (uc <= 0x318F)) alphabet = "korean";
                else if ((uc >= 0x3190) && (uc <= 0x319F)) alphabet = "chinese/japanese";
                else if ((uc >= 0x31A0) && (uc <= 0x31BF)) alphabet = "bopomofo";
                else if ((uc >= 0x31F0) && (uc <= 0x31FF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x3200) && (uc <= 0x32FF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x3300) && (uc <= 0x33FF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x3400) && (uc <= 0x4DBF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x4DC0) && (uc <= 0x4DFF)) alphabet = "none";
                else if ((uc >= 0x4E00) && (uc <= 0x9FFF)) alphabet = "chinese/japanese";
                else if ((uc >= 0xA000) && (uc <= 0xA48F)) alphabet = "chinese/japanese";
                else if ((uc >= 0xA490) && (uc <= 0xA4CF)) alphabet = "chinese/japanese";
                else if ((uc >= 0xAC00) && (uc <= 0xD7AF)) alphabet = "korean";
                else if ((uc >= 0xD800) && (uc <= 0xDB7F)) alphabet = "high surrogates";
                else if ((uc >= 0xDB80) && (uc <= 0xDBFF)) alphabet = "high private use surrogates";
                else if ((uc >= 0xDC00) && (uc <= 0xDFFF)) alphabet = "low surrogates";
                else if ((uc >= 0xE000) && (uc <= 0xF8FF)) alphabet = "private use area";
                else if ((uc >= 0xF900) && (uc <= 0xFAFF)) alphabet = "chinese/japanese";
                else if ((uc >= 0xFB00) && (uc <= 0xFB4F)) alphabet = "alphabetic presentation forms";
                else if ((uc >= 0xFB50) && (uc <= 0xFDFF)) alphabet = "arabic";
                else if ((uc >= 0xFE00) && (uc <= 0xFE0F)) alphabet = "variation selectors";
                else if ((uc >= 0xFE20) && (uc <= 0xFE2F)) alphabet = "combining half marks";
                else if ((uc >= 0xFE30) && (uc <= 0xFE4F)) alphabet = "chinese/japanese";
                else if ((uc >= 0xFE50) && (uc <= 0xFE6F)) alphabet = "small form variants";
                else if ((uc >= 0xFE70) && (uc <= 0xFEFF)) alphabet = "arabic";
                else if ((uc >= 0xFF00) && (uc <= 0xFFEF)) alphabet = "halfwidth and fullwidth forms";
                else if ((uc >= 0xFFF0) && (uc <= 0xFFFF)) alphabet = "specials";
                else if ((uc >= 0x10000) && (uc <= 0x1007F)) alphabet = "linear b";
                else if ((uc >= 0x10080) && (uc <= 0x100FF)) alphabet = "linear b";
                else if ((uc >= 0x10100) && (uc <= 0x1013F)) alphabet = "aegean numbers";
                else if ((uc >= 0x10300) && (uc <= 0x1032F)) alphabet = "old italic";
                else if ((uc >= 0x10330) && (uc <= 0x1034F)) alphabet = "gothic";
                else if ((uc >= 0x10380) && (uc <= 0x1039F)) alphabet = "ugaritic";
                else if ((uc >= 0x10400) && (uc <= 0x1044F)) alphabet = "deseret";
                else if ((uc >= 0x10450) && (uc <= 0x1047F)) alphabet = "shavian";
                else if ((uc >= 0x10480) && (uc <= 0x104AF)) alphabet = "osmanya";
                else if ((uc >= 0x10800) && (uc <= 0x1083F)) alphabet = "cypriot syllabary";
                else if ((uc >= 0x1D000) && (uc <= 0x1D0FF)) alphabet = "byzantine musical symbols";
                else if ((uc >= 0x1D100) && (uc <= 0x1D1FF)) alphabet = "musical symbols";
                else if ((uc >= 0x1D300) && (uc <= 0x1D35F)) alphabet = "tai xuan jing symbols";
                else if ((uc >= 0x1D400) && (uc <= 0x1D7FF)) alphabet = "none";
                else if ((uc >= 0x20000) && (uc <= 0x2A6DF)) alphabet = "chinese/japanese";
                else if ((uc >= 0x2F800) && (uc <= 0x2FA1F)) alphabet = "chinese/japanese";
                else if ((uc >= 0xE0000) && (uc <= 0xE007F)) alphabet = "none";

                bool ucprint = false;
                if (alphabet != "none")
                {
                    n++;
                    if (!alphdir.ContainsKey(alphabet))
                        alphdir.Add(alphabet, 0);
                    alphdir[alphabet]++;
                }
                else if (uc != 0x0020)
                {
                    //Console.Write("c=" + c.ToString() + ", uc=0x" + uc.ToString("x5") + "|");
                    //ucprint = true;
                }
                if (ucprint)
                    Console.WriteLine();
            }

            int nmax = 0;
            string alphmax = "none";
            foreach (string alph in alphdir.Keys)
            {
                //Console.WriteLine("ga:" + alph + " " + alphdir[alph].ToString());
                if (alphdir[alph] > nmax)
                {
                    nmax = alphdir[alph];
                    alphmax = alph;
                }
            }

            if (letters.Length > 2 * n) //mostly non-alphabetic
                return "none";
            else if (nmax > n / 2) //mostly same alphabet
                return alphmax;
            else
                return "mixed"; //mixed alphabets
        }

        public static string get_best_label(JObject wd)
        {
            string label = "(no label)";
            if (wd["labels"].HasValues)
            {
                Dictionary<string, object> dictObj = wd["labels"].ToObject<Dictionary<string, object>>();
                for (int ilang = 0; ilang < preferred_languages.Length; ilang++)
                    if (dictObj.ContainsKey(preferred_languages[ilang]))
                    {
                        return wd["labels"][preferred_languages[ilang]]["value"].ToString();
                    }
                foreach (string lang in dictObj.Keys)
                {
                    label = wd["labels"][lang]["value"].ToString();
                    if (is_latin(label))
                        return label;
                }
                foreach (string lang in dictObj.Keys)
                {
                    label = wd["labels"][lang]["value"].ToString();
                    return label;
                }
            }
            return label;
        }

        public static void separate_properties(string dumpfn, string propfn)
        {
            char[] trimchars = "[,]".ToCharArray();

            int nlines = 0;
            using (StreamWriter sw = new StreamWriter(propfn))
            using (StreamReader sr = new StreamReader(dumpfn))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim(trimchars);
                    if (String.IsNullOrEmpty(s))
                        continue;
                    nlines++;
                    JObject wd = JObject.Parse(s);
                    if (nlines % 1000 == 0)
                    {
                        Console.WriteLine(nlines + " " + wd["id"] + " " + get_best_label(wd));// +" "+wd.labels["en"].value);
                    }
                    if (wd["type"].ToString() != "item")
                    {
                        Console.WriteLine(wd["id"]);// +" "+wd.labels["en"].value);
                        //Console.ReadLine();
                        sw.WriteLine(s);
                    }
                }
            }
        }

        public static List<DateTime> get_time_claims(JObject wd, string prop, Dictionary<string, object> dictObj)
        {
            List<DateTime> rl = new List<DateTime>();
            if (dictObj.ContainsKey(prop))
            {
                string bestrank = "";
                for (int iclaim = 0; iclaim < wd["claims"][prop].Count(); iclaim++)
                {
                    string rank = wd["claims"][prop][iclaim]["rank"].ToString();
                    if (rank == "preferred")
                    {
                        bestrank = "preferred";
                        break;
                    }
                    else if (rank == "normal")
                        bestrank = "normal";
                }
                for (int iclaim = 0; iclaim < wd["claims"][prop].Count(); iclaim++)
                {
                    string rank = wd["claims"][prop][iclaim]["rank"].ToString();
                    if (rank != bestrank)
                        continue;
                    string wdtype = wd["claims"][prop][iclaim]["mainsnak"]["datavalue"]["type"].ToString();
                    //Console.WriteLine(prop + ":" + wdtype);
                    if (wdtype == "time")
                    {
                        string rs = wd["claims"][prop][iclaim]["mainsnak"]["datavalue"]["value"]["time"].ToString();
                        if (String.IsNullOrEmpty(rs))
                            continue;
                        string precision = wd["claims"][prop][iclaim]["mainsnak"]["datavalue"]["value"]["precision"].ToString();
                        DateTime rt = parse_time(rs, precision);
                        if (rt.Year != 9999)
                        {
                            rl.Add(rt);
                        }
                    }
                }

            }

            return rl;
        }

        public static DateTime parse_time(string rs,string precision)
        {
            //Console.WriteLine("get_wd_year:rs: " + rs);

            bool bc = (rs[0] == '-');
            //Console.WriteLine("bc = " + bc);

            rs = rs.Remove(0, 1);
            if (rs.Contains("-00-00"))
                rs = rs.Replace("-00-00", "-01-01");

            DateTime rt = new DateTime(9999, 1, 1);
            try
            {
                rt = DateTime.Parse(rs);
                if (bc)
                    rt = rt.AddYears(BCoffset);

                double iprec = util.tryconvert(precision);
                //Console.WriteLine("iprec =" + iprec);
                if (iprec < 0)
                    iprec = 0;
                //Console.WriteLine("precision =" + precision);
                rt = rt.AddSeconds(iprec);
                //rl.Add(rt);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return rt;
        }
        public static string get_datavalue(JToken jd)
        {
            string s = "";
            string wdtype = "";
            if (jd["datavalue"] != null)
                wdtype = jd["datavalue"]["type"].ToString();
            //Console.WriteLine(prop + ":" + wdtype);
            switch (wdtype)
            {
                case "string":
                    s = jd["datavalue"]["value"].ToString();
                    break;
                case "wikibase-entityid":
                    s = "Q" + jd["datavalue"]["value"]["numeric-id"].ToString();
                    break;
                case "quantity":
                    s = jd["datavalue"]["value"]["amount"].ToString();
                    break;
                case "globecoordinate":
                    s = jd["datavalue"]["value"]["latitude"].ToString() + "|" + jd["datavalue"]["value"]["longitude"].ToString();
                    break;
                case "time":
                    string rs = jd["datavalue"]["value"]["time"].ToString();
                    string precision = jd["datavalue"]["value"]["precision"].ToString();
                    DateTime rt = parse_time(rs, precision);
                    s = rt.Year.ToString();
                    break;
            }

            return s;
        }

        public static List<string> get_claims(JObject wd, int iprop, Dictionary<string, object> dictObj)
        {
            string prop = "P" + iprop.ToString();
            return get_claims(wd, prop, dictObj);
        }

        public static List<string> get_claims(JObject wd, string prop)
        {
            Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();
            return get_claims(wd, prop, dictObj);
        }

        public static List<string> get_claims(JObject wd, string prop, Dictionary<string, object> dictObj)
        {
            List<string> rl = new List<string>();

            if (dictObj.ContainsKey(prop))
            {
                string bestrank = "";
                for (int iclaim = 0; iclaim < wd["claims"][prop].Count(); iclaim++)
                {
                    string rank = wd["claims"][prop][iclaim]["rank"].ToString();
                    if (rank == "preferred")
                    {
                        bestrank = "preferred";
                        break;
                    }
                    else if (rank == "normal")
                        bestrank = "normal";
                }
                for (int iclaim = 0; iclaim < wd["claims"][prop].Count(); iclaim++)
                {
                    string rank = wd["claims"][prop][iclaim]["rank"].ToString();
                    if (rank != bestrank)
                        continue;
                    string datavalue = get_datavalue(wd["claims"][prop][iclaim]["mainsnak"]);
                    if (!String.IsNullOrEmpty(datavalue))
                        rl.Add(datavalue);
                    //    Console.WriteLine(key + ":" + wd["claims"][key][iclaim]["mainsnak"]["datavalue"]["value"]);
                }

            }
            return rl;
        }

        public static void map_subclasses(JObject wd)
        {
            if (wd["type"].ToString() != "item")
                return;

            int iq = util.qtoint(wd["id"].ToString());
            if (iq < 0)
                return;

            Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();

            List<string> instancelist = get_claims(wd, propdict["instance_of"], dictObj);
            List<string> subclasslist = get_claims(wd, propdict["subclass_of"], dictObj);

            foreach (string qi in instancelist)
            {
                int iqi = util.qtoint(qi);
                if (!subclassdict.ContainsKey(iqi))
                {
                    subclassclass sc = new subclassclass();
                    subclassdict.Add(iqi, sc);
                }
                subclassdict[iqi].instances++;
            }

            if (subclasslist.Count > 0)
            {
                if (!subclassdict.ContainsKey(iq))
                {
                    subclassclass sc = new subclassclass();
                    subclassdict.Add(iq, sc);
                }
            }

            foreach (string qi in subclasslist)
            {
                int iqi = util.qtoint(qi);
                if (!subclassdict.ContainsKey(iqi))
                {
                    subclassclass sc = new subclassclass();
                    subclassdict.Add(iqi, sc);
                }
                if (!subclassdict[iqi].down.Contains(iq))
                    subclassdict[iqi].down.Add(iq);
                if (!subclassdict[iq].up.Contains(iqi))
                    subclassdict[iq].up.Add(iqi);
            }

            if (subclassdict.ContainsKey(iq))
            {
                subclassdict[iq].label = get_best_label(wd);
                Console.WriteLine(subclassdict.Count + " subclasses. " + subclassdict[iq].label);
            }


        }

        public static void print_subclasses()
        {
            using (StreamWriter sw = new StreamWriter(dumpfolder + "wikidata-subclasses-new.txt"))
                foreach (int iq in subclassdict.Keys)
                {
                    Console.WriteLine(iq + " " + subclassdict[iq].label + " Up:" + subclassdict[iq].up.Count + " Down:" + subclassdict[iq].down.Count + " Instances:" + subclassdict[iq].instances);
                    sw.Write(iq + "\t" + subclassdict[iq].label + "\t" + subclassdict[iq].up.Count + "\t" + subclassdict[iq].down.Count + "\t" + subclassdict[iq].instances);
                    foreach (int us in subclassdict[iq].up)
                        sw.Write("\t" + us);
                    foreach (int us in subclassdict[iq].down)
                        sw.Write("\t" + us);
                    sw.WriteLine();
                }
        }
        
        public void print_subclass_tree(int subclass, int level, List<int> donelist)
        {
            //Console.WriteLine("print_subclass_tree");
            string ts = "".PadRight(level, '\t');

            if (level > util.tryconvert(TB_maxlevels.Text))
                return;

            if (donelist.Contains(subclass))
            {
                memo(ts + "Q" + subclass + " looping");
            }
            else if (subclassdict.ContainsKey(subclass))
            {
                donelist.Add(subclass);
                memo(ts + "Q" + subclass + " " + subclassdict[subclass].label + ": " + subclassdict[subclass].instances);
                foreach (int subsub in subclassdict[subclass].down)
                {
                    print_subclass_tree(subsub, level + 1,donelist);
                }
            }
            else
                memo(ts + "Q" + subclass + " not found");
        }

        public static List<int> donedown = new List<int>();
        public static List<int> doneup = new List<int>();
        public static int sum_down(int iq)
        {
            int isum = 0;
            donedown.Add(iq);
            foreach (int iqd in subclassdict[iq].down)
                if (!donedown.Contains(iqd))
                    isum += sum_down(iqd);
            //Console.WriteLine(iq+" "+isum);
            return isum;
        }

        public static void follow_up(int iq, string prefix)
        {
            if (doneup.Contains(iq))
                return;

            doneup.Add(iq);
            Console.WriteLine(prefix + subclassdict[iq].label);
            foreach (int iqu in subclassdict[iq].up)
            {
                follow_up(iqu, prefix + "--");
            }
        }

        public static void list_up(JObject wd)
        {
            Console.WriteLine(wd["id"].ToString() + ": " + get_best_label(wd));

            Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();

            List<string> instancelist = get_claims(wd, propdict["instance_of"], dictObj);

            foreach (string sqi in instancelist)
            {
                int iqi = util.qtoint(sqi);
                follow_up(iqi, "--");
            }
        }

        public static bool search_up(int iq, int target)
        {
            bool found = false;

            if (iq == target)
                return true;

            if (doneup.Contains(iq))
                return false;

            doneup.Add(iq);

            if (!subclassdict.ContainsKey(iq))
                return false;

            foreach (int iqi in subclassdict[iq].up)
            {
                found = search_up(iqi, target);
                if (found)
                    break;
            }

            return found;
        }

        public static bool search_up(JObject wd, int target)
        {
            return search_up(wd, target, false);
        }

        public static bool search_up(JObject wd, int target, bool debug)
        {
            bool found = false;

            doneup.Clear();

            int iq = util.qtoint(wd["id"].ToString());
            if (iq == target)
                return true;

            if (subclassdict.ContainsKey(iq))
            {
                if (debug)
                    Console.WriteLine("scd contains iq");
                found = search_up(iq, target);
            }

            if (found)
                return true;

            doneup.Add(iq);

            Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();

            List<string> instancelist = get_claims(wd, propdict["instance_of"], dictObj);

            foreach (string sqi in instancelist)
            {
                if (debug)
                    Console.WriteLine("sqi = " + sqi);

                int iqi = util.qtoint(sqi);
                found = search_up(iqi, target);
                if (found)
                    break;
            }
            if (debug)
                Console.WriteLine("found = " + found);

            return found;
        }

        public static void read_subclasses()
        {
            Console.WriteLine("Reading subclass file");
            int nlines = 0;
            string fn = dumpfolder + "wikidata-subclasses.txt";
            if (!File.Exists(fn))
                fn = fn.Replace(dumpfolder, outfolder);
            using (StreamReader sr = new StreamReader(fn))
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    if (String.IsNullOrEmpty(s))
                        continue;
                    string[] words = s.Split('\t');
                    if (words.Length < 5)
                        continue;
                    int iq = util.tryconvert(words[0]);
                    if (iq < 0)
                        continue;
                    if (subclassdict.ContainsKey(iq))
                        continue;

                    subclassclass sc = new subclassclass();
                    sc.label = words[1];
                    sc.instances = util.tryconvert(words[4]);
                    int nup = util.tryconvert(words[2]);
                    int ndown = util.tryconvert(words[3]);
                    if (nup > 0)
                        for (int i = 0; i < nup; i++)
                        {
                            int iup = util.tryconvert(words[5 + i]);
                            if (iup != iqentity)
                                sc.up.Add(iup);
                        }
                    if (ndown > 0)
                        for (int i = 0; i < ndown; i++)
                        {
                            sc.down.Add(util.tryconvert(words[5 + nup + i]));
                        }

                    subclassdict.Add(iq, sc);

                    nlines++;

                    if (nlines < 10)
                        Console.WriteLine(iq + " " + subclassdict[iq].label + " Up:" + subclassdict[iq].up.Count + " Down:" + subclassdict[iq].down.Count + " Instances:" + subclassdict[iq].instances);
                }

            Console.WriteLine("Done reading " + nlines);
            Console.WriteLine("subclassdict.Count = " + subclassdict.Count);
        }

        public static void analyze_subclasses()
        {

            foreach (int iq in subclassdict.Keys)
            {
                donedown.Clear();
                if ((subclassdict[iq].up.Count == 0) && (subclassdict[iq].down.Count > 0))
                {
                    Console.WriteLine(subclassdict[iq].label);
                    int idown = sum_down(iq);
                    if ((idown > 10) && !String.IsNullOrEmpty(subclassdict[iq].label))
                    {
                        Console.WriteLine(idown + ": " + subclassdict[iq].label);
                    }
                }
            }
        }

        public static void make_small_dump(int n) //first n items
        {
            char[] trimchars = "[,]".ToCharArray();
            int nlines = 0;
            using (StreamWriter sw = new StreamWriter(dumpfolder + "wikidata-" + n.ToString() + "dump.json"))
            using (StreamReader sr = new StreamReader(dumpfolder + "wikidata-dump.json"))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim(trimchars);
                    if (String.IsNullOrEmpty(s))
                        continue;
                    sw.WriteLine(s);
                    nlines++;
                    if (nlines > n)
                        break;

                }
            }
        }

        public static void make_low_dump(int n) //all Qxxx < n
        {
            char[] trimchars = "[,]".ToCharArray();
            int nlines = 0;
            using (StreamWriter sw = new StreamWriter(dumpfolder + "wikidata-Q" + n.ToString() + "dump.json"))
            using (StreamReader sr = new StreamReader(dumpfolder + "wikidata-dump.json"))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim(trimchars);
                    if (String.IsNullOrEmpty(s))
                        continue;
                    JObject wd = JObject.Parse(s);
                    int iq = util.qtoint(wd["id"].ToString());

                    if (iq < 0)
                        continue;

                    if (iq < n)
                        sw.WriteLine(s);
                    nlines++;
                    if (nlines % 1000 == 0)
                    {
                        Console.WriteLine(nlines + " ============================= " + wd["id"] + " " + get_best_label(wd));// +" "+wd.labels["en"].value);
                    }

                }
            }
        }

        public static void get_claim_statistics(JObject wd, Dictionary<string, object> dictObj)
        {
            foreach (string prop in dictObj.Keys)
            {
                if (!claimstatdict.ContainsKey(prop))
                    claimstatdict.Add(prop, 0);
                claimstatdict[prop]++;
            }
            //propdict.Add("citizenship", 27);
            //propdict.Add("birthdate", 569);
            //propdict.Add("occupation", 106);
            //propdict.Add("position_held", 39);

            foreach (string ccc in get_claims(wd, propdict["citizenship"], dictObj))
            {
                if (!countrystatdict.ContainsKey(ccc))
                    countrystatdict.Add(ccc, 0);
                countrystatdict[ccc]++;
            }
            foreach (string ccc in get_claims(wd, propdict["occupation"], dictObj))
            {
                if (!occupationstatdict.ContainsKey(ccc))
                    occupationstatdict.Add(ccc, 0);
                occupationstatdict[ccc]++;
            }
            foreach (string ccc in get_claims(wd, propdict["position_held"], dictObj))
            {
                if (!positionstatdict.ContainsKey(ccc))
                    positionstatdict.Add(ccc, 0);
                positionstatdict[ccc]++;
            }

        }

        public static void save_statdict(string filename, Dictionary<string, int> dict)
        {
            using (StreamWriter sw = new StreamWriter(filename))
                foreach (string cc in dict.Keys)
                {
                    string ccc = "";
                    if (labeldict.ContainsKey(cc))
                        ccc = labeldict[cc];
                    else if (propnamedict.ContainsKey(cc))
                        ccc = propnamedict[cc];
                    sw.WriteLine(cc + "\t" + ccc + "\t" + dict[cc]);
                }

        }

        public static void read_labels()
        {
            //Console.WriteLine("Reading wanted labels");
            //using (StreamReader sr = new StreamReader(dumpfolder+"wikidata-wanted-labels.txt"))
            //{

            //    while (!sr.EndOfStream)
            //    {
            //        string s = sr.ReadLine();
            //        if (String.IsNullOrEmpty(s))
            //            continue;
            //        if ( !wanted_labels.Contains(s))
            //            wanted_labels.Add(s);
            //    }
            //}

            //Console.WriteLine("wanted labels " + wanted_labels.Count);
            Console.WriteLine("Getting labels");
            int n = 0;
            using (StreamReader sr = new StreamReader(dumpfolder + "wikidata-labels-humans.txt"))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    if (String.IsNullOrEmpty(s))
                        continue;
                    string[] words = s.Split('\t');
                    //if (( words[0].Length < 8) || (wanted_labels.Contains(words[0])))
                    labeldict.Add(words[0], words[1]);
                    n++;
                    if (n % 100000 == 0)
                        Console.WriteLine("n=" + n);
                }

            }

            //using (StreamWriter sw = new StreamWriter(dumpfolder+"wikidata-labels-humans")) 
            //{
            //    foreach (string s in labeldict.Keys)
            //    {
            //        sw.WriteLine(s + "\t" + labeldict[s]);
            //      }
            //}      

            Console.WriteLine("Done labels");
            //Console.ReadLine();
        }

        public void subdump_category(Dictionary<string, List<string>> catlist, string fulldumpfile)
        {
            int nlines = 0;
            int nfound = 0;
            memo("Starting subdump");
            Dictionary<string, StreamWriter> swdict = new Dictionary<string, StreamWriter>();
            foreach (string cat in catlist.Keys)
                swdict.Add(cat, new StreamWriter(outfolder + "wikidata-dump-" + cat + ".json"));
            //using (StreamWriter sw = new StreamWriter(subdumpfile))
            //dumpfolder+"wikidata-dump-" + catlist[0] + ".json"
            using (StreamReader sr = new StreamReader(fulldumpfile))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim(trimchars);
                    if (String.IsNullOrEmpty(s))
                        continue;
                    nlines++;
                    //Console.WriteLine(s);
                    //sw.WriteLine(s);
                    //var wd = serializer.Deserialize<wdtopclass2>(s);
                    //Dictionary<string, string> wddict = JsonConvert.DeserializeObject<Dictionary<string, string>>(s);
                    //wdtopclass wddict = JsonConvert.DeserializeObject<wdtopclass>(s);
                    JObject wd = JObject.Parse(s);

                    //sw.WriteLine(wd["id"].ToString() + "\t" + get_best_label(wd));
                    int iq = util.qtoint(wd["id"].ToString());
                    if (iq < 0)
                        continue;

                    //Console.WriteLine(wd["id"].ToString() + "\t" + get_best_label(wd));

                    if (iq == 17)
                        Console.WriteLine(wd["id"].ToString() + "\t" + get_best_label(wd));
                    //doneup.Clear();
                    //list_up(wd);

                    //map_subclasses(wd);
                    foreach (string cat in catlist.Keys)
                    {
                        bool found = false;
                        string catfound = "";
                        foreach (string category in catlist[cat])
                        {
                            if (search_up(wd, classdict[category], (iq == 17)))
                            {
                                found = true;
                                catfound = category;
                                break;
                            }
                        }
                        if (found)
                        {
                            if (nfound % 10 == 0)
                                memo(get_best_label(wd) + " is category " + catfound);
                            swdict[cat].WriteLine(s);
                            nfound++;
                        }
                    }
                    //Console.WriteLine(wd["id"] + " " + get_best_label(wd));// +" "+wd.labels["en"].value);
                    //sw.WriteLine(wd["id"] + "\t" + get_best_label(wd));
                    if (nlines % 1000 == 0)
                    {
                        memo(nlines + " ============================= ");// + wd["id"] + " " + get_best_label(wd));// +" "+wd.labels["en"].value);
                    }

                    //continue;
                    //Console.WriteLine("<cr>");
                    //Console.ReadLine();

                    //continue;

                    //if (wd["sitelinks"].Contains("frwiki"))
                    //    Console.WriteLine(wd["sitelinks"]["frwiki"]["title"]);
                    //else
                    //    Console.WriteLine("no french");
                    //if (iq == 17)
                    //    Console.ReadLine();
                }
            }

            foreach (string cat in catlist.Keys)
                swdict[cat].Dispose();

            memo("End of subdump. Found " + nfound);

        }

        public void get_iw_statistics(string dumpfile)
        {
            int nlines = 0;
            int nfound = 0;
            if (!File.Exists(dumpfile))
            {
                memo("File not found! " + dumpfile);
                return;
            }
            memo("Starting get_iw_statistics from "+dumpfile);
            using (StreamReader sr = new StreamReader(dumpfile))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim(trimchars);
                    if (String.IsNullOrEmpty(s))
                        continue;
                    nlines++;

                    JObject wd;
                    try
                    {
                        wd = JObject.Parse(s);
                    }
                    catch (Exception e)
                    {
                        memo(e.Message);
                        continue;
                    }
                    Dictionary<int, string> propvalues = new Dictionary<int, string>();
                    Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();

                    nfound++;
                    //sw.Write(wd["id"].ToString() + "\t" + get_best_label(wd));


                    Dictionary<string, object> sitedict = wd["sitelinks"].ToObject<Dictionary<string, object>>();
                    //foreach (string sl in sitedict.Keys)
                    //    Console.WriteLine(sl);

                    //sw.Write("\tsitedict");
                    //sw.Write("\t" + sitedict.Count);
                    nlanghist.Add(sitedict.Count);
                    foreach (string lang in sitedict.Keys)
                    {
                        //sw.Write("\t");
                        if (lang.EndsWith("wiki"))
                        {
                            nitemhist.Add(lang.Replace("wiki",""));
                        }
                        //if (sitedict.ContainsKey(lang))
                        //{

                        //    //sw.Write(lang.Replace("wiki", "") + ":" + wd["sitelinks"][lang]["title"]);
                        //}

                    }



                    //foreach (int prop in proplist)
                    //{
                    //    sw.Write("\t");
                    //    if (propvalues.ContainsKey(prop))
                    //        sw.Write(propvalues[prop]);
                    //}
                    //sw.WriteLine();

                    if (nlines % 1000 == 0)
                    {
                        Console.WriteLine(nlines + " ============================= " + wd["id"] + " " + get_best_label(wd));// +" "+wd.labels["en"].value);
                    }


                }
            }

            memo("#iw-languages per item");
            memo(nlanghist.GetIHist());
            memo("#items per language");
            memo(nitemhist.GetSHist());
            memo("End of get_iw_statistics. Found " + nfound);

        }

        public static void list_country_prop(List<int> proplist, List<string> langlist, string dumpfile, string outfile, int testprop, double minvalue)
        {
            list_country_prop(proplist, langlist, dumpfile, outfile, testprop, 0);
        }
        public static void list_country_prop(List<int> proplist, List<string> langlist, string dumpfile, string outfile, int testprop, double minvalue,int nmax)
        {
            int nlines = 0;
            int nfound = 0;
            Console.WriteLine("Starting list_country_prop");
            using (StreamWriter sw = new StreamWriter(outfile))
            using (StreamReader sr = new StreamReader(dumpfile))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim(trimchars);
                    if (String.IsNullOrEmpty(s))
                        continue;
                    nlines++;

                    JObject wd = JObject.Parse(s);

                    Dictionary<int, string> propvalues = new Dictionary<int, string>();
                    Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();

                    foreach (int prop in proplist)
                    {
                        foreach (string ccc in get_claims(wd, prop, dictObj))
                        {
                            propvalues.Add(prop, ccc);
                            break;
                        }
                    }

                    if (testprop > 0)
                    {
                        bool ok = false;
                        if (propvalues.ContainsKey(testprop))
                        {
                            if (util.tryconvertdouble(propvalues[testprop]) > minvalue)
                                ok = true;
                        }
                        if (!ok)
                            continue;
                    }

                    nfound++;
                    sw.Write(wd["id"].ToString() + "\t" + get_best_label(wd));


                    Dictionary<string, object> sitedict = wd["sitelinks"].ToObject<Dictionary<string, object>>();
                    //foreach (string sl in sitedict.Keys)
                    //    Console.WriteLine(sl);

                    if (langlist != null)
                    {
                        sw.Write("\tlanglist");
                        sw.Write("\t" + langlist.Count);
                        foreach (string lang in langlist)
                        {
                            sw.Write("\t");
                            if (sitedict.ContainsKey(lang))
                                sw.Write(wd["sitelinks"][lang]["title"]);
                        }
                    }
                    else
                    {
                        sw.Write("\tsitedict");
                        sw.Write("\t" + sitedict.Count);
                        foreach (string lang in sitedict.Keys)
                        {
                            sw.Write("\t");
                            if (sitedict.ContainsKey(lang))
                                sw.Write(lang.Replace("wiki", "") + ":" + wd["sitelinks"][lang]["title"]);
                        }
                    }


                    sw.Write("\t");
                    foreach (string ccc in get_claims(wd, propdict["country"], dictObj))
                    {
                        if (countrynamedict.ContainsKey(ccc))
                        {
                            sw.Write(countrynamedict[ccc]);
                            break;
                        }
                    }


                    foreach (int prop in proplist)
                    {
                        sw.Write("\t");
                        if (propvalues.ContainsKey(prop))
                            sw.Write(propvalues[prop]);
                    }
                    sw.WriteLine();

                    if (nlines % 1000 == 0)
                    {
                        Console.WriteLine(nlines + " ============================= " + wd["id"] + " " + get_best_label(wd));// +" "+wd.labels["en"].value);
                    }

                    if (nmax > 0)
                    {
                        if (nlines > nmax)
                            break;
                    }
                }
            }

            Console.WriteLine("End of list_country_prop. Found " + nfound);
        }

        public void make_subdump()
        {
            memo("make subdump");
            Dictionary<string, List<string>> catlist = new Dictionary<string, List<string>>();

            List<string> clist1 = new List<string>();
            List<string> clist2 = new List<string>();
            List<string> clist3 = new List<string>();
            List<string> clist4 = new List<string>();
            List<string> clist5 = new List<string>();
            List<string> clist6 = new List<string>();
            List<string> clist7 = new List<string>();
            List<string> clist8 = new List<string>();
            List<string> clist9 = new List<string>();
            List<string> clist10 = new List<string>();
            List<string> clist11 = new List<string>();
            List<string> clist12 = new List<string>();
            List<string> clist13 = new List<string>();

            if (CB_categories.CheckedItems.Contains("mountain"))
            {
                //mountains:
                clist1.Add("mountain");
                clist1.Add("volcano");
                clist1.Add("massif");
                catlist.Add("mountain", clist1);
            }
            ////cities:
            if (CB_categories.CheckedItems.Contains("human settlement"))
            {
                clist2.Add("human settlement");
                catlist.Add("city", clist2);
            }
            if (CB_categories.CheckedItems.Contains("lake"))
            {
                ////lakes:
                clist3.Add("lake");
                catlist.Add("lake", clist3);
            }
            if (CB_categories.CheckedItems.Contains("island"))
            {
                ////islands:
                clist4.Add("island");
                clist4.Add("island nation");
                catlist.Add("island", clist4);
            }
            if (CB_categories.CheckedItems.Contains("language"))
            {
                ////languages:
                clist5.Add("language");
                catlist.Add("language", clist5);
            }
            if (CB_categories.CheckedItems.Contains("country"))
            {
                //////countries:
                clist6.Add("country");
                clist6.Add("sovereign state");
                clist6.Add("island nation");
                catlist.Add("country", clist6);
            }
            if (CB_categories.CheckedItems.Contains("human"))
            {
                //humans:
                clist7.Add("human");
                catlist.Add("human", clist7);
            }
            if (CB_categories.CheckedItems.Contains("color"))
            {
                //Colors:
                clist8.Add("color");
                catlist.Add("color", clist8);
            }
            if (CB_categories.CheckedItems.Contains("food"))
            {
                clist9.Add("food");
                catlist.Add("food", clist9);
            }
            if (CB_categories.CheckedItems.Contains("body part"))
            {

            clist10.Add("body part");
            catlist.Add("body part", clist10);
            }
            if (CB_categories.CheckedItems.Contains("container"))
            {
            clist11.Add("container");
            catlist.Add("container", clist11);
            }
            if (CB_categories.CheckedItems.Contains("clothing"))
            {
                clist12.Add("clothing");
                catlist.Add("clothing", clist12);
            }

            foreach (string s in catlist.Keys)
                memo(s);
            memo(catlist.Count+" categories");
            string dumpsize = "";
            if (catlist.Count > 0)
                subdump_category(catlist, dumpfolder + "wikidata-" + dumpsize + "dump.json");

        }

        public static void make_proptable(string kind, int testprop, double minvalue)
        {
            List<int> proplist = new List<int>();
            //proplist.Add(propdict["country"]);
            proplist.Add(propdict["instance_of"]);
            proplist.Add(propdict["subclass_of"]);

            List<string> langlist = null;
            //langlist = new List<string>();
            //langlist.Add("svwiki");
            //langlist.Add("enwiki");
            //langlist.Add("nowiki");

            //string kind = "country";
            list_country_prop(proplist, langlist, outfolder + "wikidata-dump-" + kind + ".json", outfolder + "wikidata-list-" + kind + ".txt", testprop, minvalue);
        }

        public static void make_proptable_language(string kind, int testprop, double minvalue)
        {
            List<int> proplist = new List<int>();
            //proplist.Add(propdict["country"]);
            proplist.Add(propdict["iso1"]);
            proplist.Add(propdict["iso2"]);
            proplist.Add(propdict["iso3"]);
            proplist.Add(propdict["script"]);
            proplist.Add(propdict["wikipedia"]);
            proplist.Add(propdict["distribution_map"]);

            List<string> langlist = new List<string>();
            langlist.Add("svwiki");
            langlist.Add("enwiki");
            langlist.Add("nowiki");

            //string kind = "country";
            list_country_prop(proplist, langlist, dumpfolder + "wikidata-dump-" + kind + ".json", dumpfolder + "wikidata-list-" + kind + ".txt", testprop, minvalue);
        }

        public static void make_proptable_geography(string kind, int testprop, double minvalue)
        {
            List<int> proplist = new List<int>();
            //proplist.Add(propdict["country"]);
            proplist.Add(propdict["gnid"]);
            proplist.Add(propdict["coordinates"]);
            proplist.Add(propdict["area"]);
            proplist.Add(propdict["elevation"]);
            proplist.Add(propdict["population"]);

            List<string> langlist = new List<string>();
            langlist.Add("svwiki");
            langlist.Add("enwiki");
            langlist.Add("nowiki");

            //string kind = "country";
            list_country_prop(proplist, langlist, outfolder + "wikidata-dump-" + kind + ".json", outfolder + "wikidata-list-" + kind + ".txt", testprop, minvalue);
        }

        public static void get_scripts()
        {
            string fulldumpfile = dumpfolder + "wikidata-dump.json";
            string outfile = dumpfolder + "wikidata-list-scripts.txt";
            List<string> qlist = new List<string>();
            qlist.Add("Q966893");
            qlist.Add("Q935857");
            qlist.Add("Q933796");
            qlist.Add("Q877906");
            qlist.Add("Q855624");
            qlist.Add("Q854968");
            qlist.Add("Q839666");
            qlist.Add("Q83942");
            qlist.Add("Q82996");
            qlist.Add("Q829464");
            qlist.Add("Q82946");
            qlist.Add("Q82772");
            qlist.Add("Q8229");
            qlist.Add("Q822836");
            qlist.Add("Q8222");
            qlist.Add("Q8216");
            qlist.Add("Q8209");
            qlist.Add("Q8201");
            qlist.Add("Q8196");
            qlist.Add("Q806024");
            qlist.Add("Q804984");
            qlist.Add("Q792216");
            qlist.Add("Q79199");
            qlist.Add("Q790681");
            qlist.Add("Q756802");
            qlist.Add("Q7563292");
            qlist.Add("Q754989");
            qlist.Add("Q754673");
            qlist.Add("Q752854");
            qlist.Add("Q744068");
            qlist.Add("Q733944");
            qlist.Add("Q689894");
            qlist.Add("Q637405");
            qlist.Add("Q622712");
            qlist.Add("Q606972");
            qlist.Add("Q570450");
            qlist.Add("Q559173");
            qlist.Add("Q550383");
            qlist.Add("Q540987");
            qlist.Add("Q5287");
            qlist.Add("Q51592");
            qlist.Add("Q5058305");
            qlist.Add("Q4891256");
            qlist.Add("Q48332");
            qlist.Add("Q473725");
            qlist.Add("Q46861");
            qlist.Add("Q467784");
            qlist.Add("Q467037");
            qlist.Add("Q4494892");
            qlist.Add("Q442244");
            qlist.Add("Q4301931");
            qlist.Add("Q41670");
            qlist.Add("Q4062913");
            qlist.Add("Q401");
            qlist.Add("Q3944022");
            qlist.Add("Q387875");
            qlist.Add("Q38592");
            qlist.Add("Q33513");
            qlist.Add("Q332652");
            qlist.Add("Q3322653");
            qlist.Add("Q3297091");
            qlist.Add("Q321083");
            qlist.Add("Q285126");
            qlist.Add("Q27898535");
            qlist.Add("Q271565");
            qlist.Add("Q26978");
            qlist.Add("Q26803");
            qlist.Add("Q26752");
            qlist.Add("Q26567");
            qlist.Add("Q26549");
            qlist.Add("Q2613755");
            qlist.Add("Q2599096");
            qlist.Add("Q257634");
            qlist.Add("Q2472605");
            qlist.Add("Q2470116");
            qlist.Add("Q236376");
            qlist.Add("Q2274646");
            qlist.Add("Q209764");
            qlist.Add("Q2092163");
            qlist.Add("Q208503");
            qlist.Add("Q201688");
            qlist.Add("Q2003257");
            qlist.Add("Q1992372");
            qlist.Add("Q19816791");
            qlist.Add("Q191272");
            qlist.Add("Q187846");
            qlist.Add("Q185614");
            qlist.Add("Q185083");
            qlist.Add("Q184661");
            qlist.Add("Q1828555");
            qlist.Add("Q1815229");
            qlist.Add("Q18130932");
            qlist.Add("Q178528");
            qlist.Add("Q1771716");
            qlist.Add("Q1760127");
            qlist.Add("Q17538100");
            qlist.Add("Q164436");
            qlist.Add("Q161428");
            qlist.Add("Q1574992");
            qlist.Add("Q1497335");
            qlist.Add("Q1481626");
            qlist.Add("Q1471822");
            qlist.Add("Q13405127");
            qlist.Add("Q133800");
            qlist.Add("Q132659");
            qlist.Add("Q13203898");
            qlist.Add("Q1314503");
            qlist.Add("Q122888");
            qlist.Add("Q1197646");
            qlist.Add("Q11932");
            qlist.Add("Q11818518");
            qlist.Add("Q11818517");
            qlist.Add("Q1164129");
            qlist.Add("Q1110078");
            qlist.Add("Q1090055");
            qlist.Add("Q1089000");
            qlist.Add("Q1062587");
            qlist.Add("Q1054190");
            qlist.Add("Q1035956");

            int nlines = 0;
            int nfound = 0;
            using (StreamReader sr = new StreamReader(fulldumpfile))
            using (StreamWriter sw = new StreamWriter(outfile))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim(trimchars);
                    if (String.IsNullOrEmpty(s))
                        continue;
                    nlines++;
                    //Console.WriteLine(s);
                    //sw.WriteLine(s);
                    //var wd = serializer.Deserialize<wdtopclass2>(s);
                    //Dictionary<string, string> wddict = JsonConvert.DeserializeObject<Dictionary<string, string>>(s);
                    //wdtopclass wddict = JsonConvert.DeserializeObject<wdtopclass>(s);
                    JObject wd = JObject.Parse(s);

                    if (nlines % 1000 == 0)
                    {
                        Console.WriteLine(nlines + " ============================= ");// + wd["id"] + " " + get_best_label(wd));// +" "+wd.labels["en"].value);
                    }
                    string q = wd["id"].ToString();
                    if (!qlist.Contains(q))
                        continue;

                    Dictionary<string, object> sitedict = wd["sitelinks"].ToObject<Dictionary<string, object>>();
                    //foreach (string sl in sitedict.Keys)
                    //    Console.WriteLine(sl);

                    //foreach (string ss in sitedict.Keys)
                    //{
                    //    Console.WriteLine(ss + ":" + sitedict[ss].ToString());
                    //    //Console.WriteLine(ss + ":" + wd["sitelinks"][ss]["title"]);
                    //}

                    string[] langlist = new string[] { "nowiki", "nnwiki", "svwiki", "enwiki" };

                    //foreach (string lang in langlist)
                    //{
                    //    sw.Write("\t");
                    //    if (sitedict.ContainsKey(lang))
                    //        sw.Write(wd["sitelinks"][lang]["title"]);
                    //}

                    nfound++;
                    string title = get_best_label(wd);
                    if (sitedict.ContainsKey("nowiki"))
                        title = wd["sitelinks"]["nowiki"]["title"].ToString();
                    else if (sitedict.ContainsKey("nnwiki"))
                        title = wd["sitelinks"]["nnwiki"]["title"].ToString();

                    Console.WriteLine(wd["id"].ToString() + "\t" + title);
                    sw.WriteLine(wd["id"].ToString() + "\t" + title);

                    //continue;
                    //Console.WriteLine("<cr>");
                    //Console.ReadLine();

                    //continue;

                    //if (wd["sitelinks"].Contains("frwiki"))
                    //    Console.WriteLine(wd["sitelinks"]["frwiki"]["title"]);
                    //else
                    //    Console.WriteLine("no french");
                    //if (iq == 17)
                    //    Console.ReadLine();
                }
            }

            Console.WriteLine("End of scriptdump. Found " + nfound);


        }

        public void memo(string s)
        {
            richTextBox1.AppendText(s + "\n");
            richTextBox1.ScrollToCaret();
        }

        private void subclassbutton_Click(object sender, EventArgs e)
        {
            foreach (string s in CB_categories.CheckedItems)
                print_subclass_tree(classdict[s], 0, new List<int>());
        }

        private void iwstatbutton_Click(object sender, EventArgs e)
        {
            foreach (string s in CB_categories.CheckedItems)
                get_iw_statistics(outfolder + "wikidata-dump-" + s + ".json");
            iwtablebutton.Enabled = true;

        }

        private bool whitecheck(string category, int item,Dictionary<int,List<string>> propvalues)
        {
            if (whitelist[category].Contains(item))
                return true;
            if (blacklist[category].Contains(item))
                return false;

            if (propvalues.ContainsKey(propdict["instance_of"]))
            {
                foreach (int inst in blackinstance[category])
                {
                    string qinst = "Q" + inst;
                    if (propvalues[propdict["instance_of"]].Contains(qinst))
                        return false;
                }
            }

            if (propvalues.ContainsKey(propdict["subclass_of"]))
            {
                foreach (int inst in blacksubclass[category])
                {
                    string qinst = "Q" + inst;
                    if (propvalues[propdict["subclass_of"]].Contains(qinst))
                        return false;
                }
            }

            if (whiteinstance[category].Count == 0)
                return true;

            if (propvalues.ContainsKey(propdict["instance_of"]))
            {
                foreach (int inst in whiteinstance[category])
                {
                    string qinst = "Q" + inst;
                    if (propvalues[propdict["instance_of"]].Contains(qinst))
                        return true;
                }
            }

            if (propvalues.ContainsKey(propdict["subclass_of"]))
            {
                foreach (int inst in whitesubclass[category])
                {
                    string qinst = "Q" + inst;
                    if (propvalues[propdict["subclass_of"]].Contains(qinst))
                        return true;
                }
            }

            return false;
        }

        private void make_iw_table(string dumpfile, string outfile, List<int> proplist,string category)
        {
            int nlines = 0;
            int nfound = 0;
            int nbad = 0;
            int ndouble = 0;
            int nbelow = 0;
            int miniw = util.tryconvert(TB_miniw.Text);
            List<int> donelist = new List<int>();

            memo("Starting make_iw_table");

            using (StreamWriter sw = new StreamWriter(outfile))
            using (StreamReader sr = new StreamReader(dumpfile))
            {
                StringBuilder sbhead = new StringBuilder("\t\t# lang");
                List<string> langlist = nitemhist.GetKeys();
                foreach (int i in proplist)
                    sbhead.Append("\t"+propnamedict["P"+i]);
                foreach (string s in langlist)
                    sbhead.Append("\t" + s);
                sw.WriteLine(sbhead.ToString());

                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim(trimchars);
                    if (String.IsNullOrEmpty(s))
                        continue;
                    nlines++;

                    JObject wd;
                    try
                    {
                        wd = JObject.Parse(s);
                    }
                    catch (Exception e)
                    {
                        memo(e.Message);
                        continue;
                    }
                    Dictionary<int, List<string>> propvalues = new Dictionary<int, List<string>>();
                    Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();

                    foreach (int prop in proplist)
                    {
                        if (!propvalues.ContainsKey(prop))
                            propvalues.Add(prop, new List<string>());
                        foreach (string ccc in get_claims(wd, prop, dictObj))
                        {
                            propvalues[prop].Add(ccc);
                            //break;
                        }
                    }

                    if (nlines % 1000 == 0)
                    {
                        memo(nlines + " ============================= " + wd["id"] + " " + get_best_label(wd));// +" "+wd.labels["en"].value);
                    }

                    int qint = util.qtoint(wd["id"].ToString());

                    if (CBskip1000.Checked) //skip "1000 articles every Wikipedia should have"
                        if (wdqdict.ContainsKey(wd["id"].ToString()))
                            continue;

                    if (donelist.Contains(qint))
                    {
                        ndouble++;
                        continue;
                    }
                    else
                        donelist.Add(qint);

                    if (!whitecheck(category, qint, propvalues))
                    {
                        nbad++;
                        continue;
                    }

                    StringBuilder sb = new StringBuilder(wd["id"].ToString() + "\t" + get_best_label(wd));
                    
                    Dictionary<string, object> sitedict = wd["sitelinks"].ToObject<Dictionary<string, object>>();
                    //foreach (string sl in sitedict.Keys)
                    //    Console.WriteLine(sl);

                    //sw.Write("\tsitedict");
                    //sw.Write("\t" + sitedict.Count);
                    //nlanghist.Add(sitedict.Count);

                    if (sitedict.Count < miniw)
                    {
                        nbelow++;
                        continue;
                    }
                    nfound++;

                    sb.Append("\t" + sitedict.Count);
                    foreach (int prop in proplist)
                    {
                        sb.Append("\t");
                        foreach (string pp in propvalues[prop])
                        {
                            sb.Append(pp+";");
                        }
                    }

                    foreach (string lang in langlist)
                    {
                        //sw.Write("\t");

                        nitemhist.Add(lang);
                        if (sitedict.ContainsKey(lang+"wiki"))
                        {
                            sb.Append("\t1");
                            //sw.Write(lang.Replace("wiki", "") + ":" + wd["sitelinks"][lang]["title"]);
                        }
                        else
                        {
                            sb.Append("\t");
                        }

                    }

                    sw.WriteLine(sb.ToString());
                    //foreach (int prop in proplist)
                    //{
                    //    sw.Write("\t");
                    //    if (propvalues.ContainsKey(prop))
                    //        sw.Write(propvalues[prop]);
                    //}
                    //sw.WriteLine();



                }
            }

            memo("End of make_iw_table. Found " + nfound + ", black: " + nbad + ", double: " + ndouble + ", belowlimit: " + nbelow); 


        }



        private void iwtablebutton_Click(object sender, EventArgs e)
        {

            List<int> proplist = new List<int>();
            foreach (string s in CB_props.CheckedItems)
                proplist.Add(propdict[s]);

            
            foreach (string s in CB_categories.CheckedItems)
            {
                string fn = outfolder + "wd-table-" + s + ".v1.txt";
                if (CBskip1000.Checked)
                    fn = fn.Replace("-table-", "-skip1000table-");
                int version = 1;
                while (File.Exists(fn))
                {
                    fn = fn.Replace("v" + version + ".txt", "v" + (version + 1) + ".txt");
                    version++;
                }

                make_iw_table(outfolder + "wikidata-dump-" + s + ".json", fn, proplist,s);
            }


        }

        private void Subdump_button_Click(object sender, EventArgs e)
        {
            make_subdump();
        }

        private void Normalizebutton_Click(object sender, EventArgs e)
        {
        //            public float[,] norm_matrix = new float[maxlang, maxlang];
        //public int[,] hit_matrix = new int[maxlang, maxlang];
        //public int[,] miss_matrix = new int[maxlang, maxlang];

            int[] nitem = new int[maxlang];

            memo("Clearing arrays");
            //Array.Clear(norm_matrix, 0,maxlang*maxlang);
            //Array.Clear(hit_matrix, 0,maxlang*maxlang);
            //Array.Clear(miss_matrix, 0, maxlang*maxlang);
            for (int i = 0; i < maxlang; i++)
            {
                nitem[i] = 0;
                for (int j = 0; j < maxlang; j++)
                {
                    norm_matrix[i, j] = 0;
                    hit_matrix[i, j] = 0;
                    miss_matrix[i, j] = 0;
                }
            }
            langindex.Clear();
            indexlang.Clear();
            //langstats.Clear();
            memo("Arrays cleared");

            int nline = 0;


            openFileDialog1.InitialDirectory = outfolder;
            if ( openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fn = openFileDialog1.FileName;
                string fnout = unusedfn(outfolder + "wikidata-norm-XXX.txt");
                using (StreamWriter sw = new StreamWriter(fnout))
                {
                    using (StreamReader sr = new StreamReader(fn))
                    {
                        string header = sr.ReadLine();
                        string[] hwords = header.Split('\t');
                        int offset = 4;

                        StringBuilder sb = new StringBuilder();
                        for (int i = 1; i < hwords.Length - offset; i++)
                        {
                            langindex.Add(hwords[i + offset], i);
                            indexlang.Add(i, hwords[i + offset]);
                            sb.Append("\t" + hwords[i + offset]);
                        }
                        memo(sb.ToString());
                        sw.WriteLine(sb.ToString());


                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            nline++;
                            if (nline % 1000 == 0)
                                memo("nline = " + nline);

                            string[] words = line.Split('\t');

                            for (int i = 1; i < words.Length - offset; i++)
                            {
                                if (!String.IsNullOrEmpty(words[i + offset]))
                                {
                                    nitem[i]++;
                                    for (int j = 1; j < words.Length - offset; j++)
                                    {
                                        if (!String.IsNullOrEmpty(words[j + offset]))
                                        {
                                            hit_matrix[i, j]++;
                                        }
                                        else
                                        {
                                            miss_matrix[i, j]++;
                                        }

                                    }

                                }

                            }


                        }
                    }

                    float totalitem = nline;

                    for (int i = 1; i < maxlang; i++)
                    {
                        if (!indexlang.ContainsKey(i))
                            continue;
                        StringBuilder sb = new StringBuilder(indexlang[i]);
                        for (int j = 1; j < maxlang; j++)
                        {
                            if (hit_matrix[i, j] + miss_matrix[i, j] > 0)
                            {
                                if (hit_matrix[i, j] == 0)
                                    norm_matrix[i, j] = nitem[j] / totalitem;
                                else
                                    norm_matrix[i, j] = (float)hit_matrix[i, j] / (hit_matrix[i, j] + miss_matrix[i, j]);
                                sb.Append("\t" + norm_matrix[i, j].ToString("N6"));
                            }
                            else
                                sb.Append("\t");
                        }
                        //memo(sb.ToString());
                        sw.WriteLine(sb.ToString());

                    }

                }

                string fnout2 = unusedfn(outfolder + "wikidata-normcount-XXX.txt");
                using (StreamWriter sw = new StreamWriter(fnout2))
                {
                    sw.WriteLine("total\t" + nline);
                    for (int i=1;i<maxlang; i++)
                    {
                        if ( indexlang.ContainsKey(i))
                        {
                            sw.WriteLine(indexlang[i] + "\t" + nitem[i]);
                        }
                    }
                }
            }
        }

        private void read_normalization_matrix()
        {
            normdict.Clear();
            normdict = read_matrix("Normalization matrix file");
        }

        private string insertbeforesuffix(string fnbase, string infix)
        {
            string suffix = "." + fnbase.Split('.').Last();
            return fnbase.Replace(suffix, infix + suffix);
        }

        private string insertafterpath(string fnbase, string infix)
        {
            string suffix = fnbase.Split('\\').Last();
            return fnbase.Replace(suffix, infix + suffix);
        }

        private string unusedfn(string fnbase)
        {
            string suffix = ".txt";
            if ( !fnbase.Contains(suffix))
            {
                suffix = "." + fnbase.Split('.').Last();
            }
            memo(fnbase + " suffix:" + suffix);
            int i = 1;
            string fn = fnbase.Replace(suffix,i+suffix);
            while (File.Exists(fn))
            {
                i++;
                fn = fnbase.Replace(suffix, i + suffix);
            }
            return fn;
        }

        private void write_matrix(string fnbase, Dictionary<string,Dictionary<string,double>> dict)
        {
            bool nocheck = (langstats.Count == 0);
            string fn = unusedfn(fnbase);
            memo("Writing matrix to " + fn);
            langindex.Clear();
            indexlang.Clear();
            using (StreamWriter sw = new StreamWriter(fn))
            {
                StringBuilder sbhead = new StringBuilder();
                int i = 0;
                foreach (string s in dict.Keys)
                {
                    if (dict[s].Count == 0)
                    {
                        continue;
                    }
                    i++;
                    if (nocheck || (langstats.ContainsKey(s) && inrange(langstats[s])))
                        sbhead.Append("\t" + s);
                    langindex.Add(s, i);
                    indexlang.Add(i, s);
                }
                sw.WriteLine(sbhead.ToString());

                foreach (string s in dict.Keys)
                {
                    if (dict[s].Count == 0)
                        continue;
                    if (!nocheck && !(langstats.ContainsKey(s) && inrange(langstats[s])))
                        continue;
                    StringBuilder sb = new StringBuilder(s);
                    for (int j=1;j<=i;j++)
                    {
                        if ( indexlang.ContainsKey(j) && (nocheck || inrange(langstats[indexlang[j]])))
                        {
                            if (dict[s].ContainsKey(indexlang[j]))
                            {
                                sb.Append("\t" + dict[s][indexlang[j]].ToString());
                            }
                            else
                                sb.Append("\t-");

                        }
                    }
                    sw.WriteLine(sb.ToString());
                }

            }
        }

        private Dictionary<string,Dictionary<string,double>> read_matrix(string opendialogheader)
        {
            if (blacklang.Count == 0)
                fill_whitelists();

            memo("read_matrix " + opendialogheader);
            Console.WriteLine("read_matrix " + opendialogheader);
            Dictionary<string, Dictionary<string, double>> returndict = new Dictionary<string, Dictionary<string, double>>();

            openFileDialog1.InitialDirectory = outfolder;
            openFileDialog1.Title = opendialogheader;
            Console.WriteLine("opendialog1.Show:");
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fn = openFileDialog1.FileName;
                recentfile = fn;
                memo("Reading matrix from " + fn);
                using (StreamReader sr = new StreamReader(fn))
                {
                    string header = sr.ReadLine();
                    string[] hwords = header.Split('\t');
                    int offset = 0;

                    langindex.Clear();
                    indexlang.Clear();

                    StringBuilder sb = new StringBuilder();
                    for (int i = 1; i < hwords.Length - offset; i++)
                    {
                        langindex.Add(hwords[i + offset], i);
                        indexlang.Add(i, hwords[i + offset]);
                        if (!blacklang.Contains(hwords[i + offset]))
                            sb.Append("\t" + hwords[i + offset]);
                    }
                    memo(sb.ToString());

                    int nline = 0;

                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        nline++;
                        if (nline % 100 == 0)
                            memo("nline = " + nline);

                        string[] words = line.Split('\t');

                        if (!langindex.ContainsKey(words[0]))
                            continue;
                        if (blacklang.Contains(words[0]))
                            continue;
                        
                        int i = langindex[words[0]];
                        if (!returndict.ContainsKey(words[0]))
                        {
                            Dictionary<string, double> dd = new Dictionary<string, double>();
                            returndict.Add(words[0], dd);
                        }
                        for (int j=1;j<words.Length;j++)
                        {
                            if ( !indexlang.ContainsKey(j))
                                continue;
                            string jlang = indexlang[j];
                            if (blacklang.Contains(jlang))
                                continue;

                            returndict[words[0]].Add(jlang, util.tryconvertfloat(words[j]));
                            //norm_matrix[i, j] = util.tryconvertfloat(words[j]);

                        }

                    }
                }


            }

            memo("read_matrix done");
            return returndict;
        }

        private string normalize_distances()
        {
            //            public float[,] norm_matrix = new float[maxlang, maxlang];
            //public int[,] hit_matrix = new int[maxlang, maxlang];
            //public int[,] miss_matrix = new int[maxlang, maxlang];
            //Dictionary<int, int> normindex = new Dictionary<int, int>(); //from langtable-index to norm matrix index
            Dictionary<int, string> ltabdict = new Dictionary<int, string>(); //from j to iso

            memo("Clearing arrays");
            for (int i = 0; i < maxlang; i++)
                for (int j = 0; j < maxlang; j++)
                {
                    dist_matrix[i, j] = 0;
                }
            //iwdistdict.Clear();
            langstats.Clear();
            memo("Arrays cleared");
            string fn = "";

            openFileDialog1.InitialDirectory = outfolder;
            openFileDialog1.Title = "Domain language table";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fn = openFileDialog1.FileName;
                using (StreamReader sr = new StreamReader(fn))
                {
                    string header = sr.ReadLine();
                    string[] hwords = header.Split('\t');
                    int offset = 4;

                    StringBuilder sb = new StringBuilder();
                    for (int i = 1; i < hwords.Length - offset; i++)
                    {
                        if (normdict.ContainsKey(hwords[i + offset]))
                        {
                            //normindex.Add(i, langindex[hwords[i + offset]]);
                            sb.Append("\t" + hwords[i + offset]);
                            ltabdict.Add(i, hwords[i + offset]);
                            langstats.Add(hwords[i + offset], 0);
                        }
                    }
                    memo(sb.ToString());


                    int nline = 0;

                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        nline++;
                        if (nline % 100 == 0)
                            memo("nline = " + nline);

                        string[] words = line.Split('\t');

                        for (int i = 1; i < words.Length - offset; i++)
                        {
                            if (!ltabdict.ContainsKey(i))
                                continue;
                            if (!iwdistdict.ContainsKey(ltabdict[i]))
                            {
                                Dictionary<string, double> dd = new Dictionary<string, double>();
                                iwdistdict.Add(ltabdict[i], dd);
                            }
                            if (!String.IsNullOrEmpty(words[i + offset]))
                            {
                                //Count articles in language i:
                                langstats[ltabdict[i]]++;
                                //Loop over all other languages and count coincidences:
                                for (int j = 1; j < words.Length - offset; j++)
                                {
                                    if (!ltabdict.ContainsKey(j))
                                        continue;
                                    if (!iwdistdict.ContainsKey(ltabdict[j]))
                                    {
                                        Dictionary<string, double> dd = new Dictionary<string, double>();
                                        iwdistdict.Add(ltabdict[j], dd);
                                    }
                                    if (!iwdistdict[ltabdict[i]].ContainsKey(ltabdict[j]))
                                        iwdistdict[ltabdict[i]].Add(ltabdict[j], 0);
                                    if (String.IsNullOrEmpty(words[j + offset]))
                                    {
                                        if (normdict[ltabdict[i]][ltabdict[j]] + normdict[ltabdict[j]][ltabdict[i]] > 0)
                                            //iwdistdict[ltabdict[i]][ltabdict[j]] += normdict[ltabdict[i]][ltabdict[j]];
                                            iwdistdict[ltabdict[i]][ltabdict[j]] += normdict[ltabdict[i]][ltabdict[j]] * normdict[ltabdict[i]][ltabdict[j]];
                                        else
                                            iwdistdict[ltabdict[i]][ltabdict[j]] = -1;
                                        //dist_matrix[i, j] += norm_matrix[normindex[i], normindex[j]];
                                    }
                                }


                            }

                        }


                    }
                }

                for (int i = 1; i < maxlang; i++)
                {
                    if (!ltabdict.ContainsKey(i))
                        continue;
                    if (!normdict.ContainsKey(ltabdict[i]))
                        continue;
                    if (iwdistdict[ltabdict[i]].Count == 0)
                        continue;
                    if (!inrange(langstats[ltabdict[i]]))
                        continue;
                    StringBuilder sb = new StringBuilder(ltabdict[i]);
                    for (int j = 1; j < maxlang; j++)
                    {
                        if (!ltabdict.ContainsKey(j))
                            continue;
                        if (!inrange(langstats[ltabdict[j]]))
                            continue;
                        iwdistdict[ltabdict[i]][ltabdict[j]] = Math.Sqrt(iwdistdict[ltabdict[i]][ltabdict[j]]);
                        sb.Append("\t" + iwdistdict[ltabdict[i]][ltabdict[j]].ToString("N4"));
                    }
                    memo(sb.ToString());
                }


            }
            return fn;
        }

        private bool inrange(int n)
        {
            if (n < util.tryconvert(TB_minrange.Text))
                return false;
            if (n > util.tryconvert(TB_maxrange.Text))
                return false;
            return true;
        }

        private Dictionary<string,Dictionary<string,double>> make_symmetric(Dictionary<string,Dictionary<string,double>> dict)
        {
            Dictionary<string, Dictionary<string, double>> symdict = new Dictionary<string, Dictionary<string, double>>();
            foreach (string s1 in dict.Keys)
            {
                Dictionary<string, double> dd = new Dictionary<string, double>();
                symdict.Add(s1, dd);
                foreach (string s2 in dict[s1].Keys)
                {
                    if (dict.ContainsKey(s2) && dict[s2].ContainsKey(s1))
                        symdict[s1].Add(s2, 0.5 * (dict[s1][s2] + dict[s2][s1]));
                }
            }

            return symdict;
        }

        public string get_random_key(Dictionary<string,double> dict)
        {
            if (dict.Count == 0)
                return "XX";
            else
            {
                Random rand = new Random();
                return dict.ElementAt(rand.Next(0, dict.Count)).Key;
            }
        }

        private Dictionary<string, Dictionary<string, double>> make_random(Dictionary<string, Dictionary<string, double>> dict)
        {
            Random rand = new Random();
            memo("make_random");
            Dictionary<string, Dictionary<string, double>> symdict = make_symmetric(dict);
            List<double> values = new List<double>();
            foreach (string s1 in symdict.Keys)
                foreach (string s2 in symdict[s1].Keys)
                {
                    if (s1 != s2)
                        values.Add(symdict[s1][s2]);
                }

            Dictionary<string, Dictionary<string, double>> randomdict = new Dictionary<string, Dictionary<string, double>>(symdict);
            foreach (string s1 in symdict.Keys)
            {
                randomdict[s1] = new Dictionary<string, double>(symdict[s1]);
            }

            foreach (string s1 in symdict.Keys)
                foreach (string s2 in symdict[s1].Keys)
                {
                    if (s1 != s2)
                    {
                        randomdict[s1][s2] = values.ElementAt(rand.Next(values.Count));
                        randomdict[s2][s1] = randomdict[s1][s2];
                    }
                }


            //foreach (string s1 in dict.Keys)
            //{
            //    memo(s1);
            //    Dictionary<string, double> dd = new Dictionary<string, double>();
            //    symdict.Add(s1, dd);
            //    foreach (string s2 in dict[s1].Keys)
            //    {
            //        string s3 = get_random_key(dict[s1]);
            //        string s4 = s3;
            //        int j = 0;
            //        while ((s4 == s3) && ( j<10))
            //        {
            //            s4 = get_random_key(dict[s2]);
            //            j++;
            //        }

            //        if (dict.ContainsKey(s3) && dict[s3].ContainsKey(s4))
            //            if (dict.ContainsKey(s4) && dict[s4].ContainsKey(s3))
            //                symdict[s1].Add(s2, 0.5 * (dict[s3][s4] + dict[s4][s3]));
            //    }
            //}
            memo("make_random done");
            return randomdict;
        }

        private void print_matrix_value_pairs(Dictionary<string, Dictionary<string, double>> m1, Dictionary<string, Dictionary<string, double>> m2, string label1, string label2)
        {
            int i = 0;
            memo("Language\t" + label1 + "\t" + label2);
            string fn = unusedfn(outfolder + label1 + "-" + label2 + "-valuepairs.txt");
            using (StreamWriter sw = new StreamWriter(fn))
            {
                foreach (string s1 in m1.Keys)
                {
                    if (!m2.ContainsKey(s1))
                        continue;
                    foreach (string s2 in m1[s1].Keys)
                    {
                        if (s2 == s1)
                            continue;
                        if (!m2[s1].ContainsKey(s2))
                            continue;
                        i++;
                        if (i % 1000 == 0)
                            memo(i.ToString());
                        if ((m1[s1][s2] > 0) && (m2[s1][s2] > 0))
                            sw.WriteLine(s1 + "/" + s2 + "\t" + m1[s1][s2] + "\t" + m2[s1][s2]);
                    }
                }
            }
            memo("Done "+i);
        }

        private System.Windows.Forms.DataVisualization.Charting.Chart chart_from_pca(double[][] data,string label1, string label2, string title, int[] langclass)
        {
            Dictionary<string, int> wingdict = new Dictionary<string, int>();

            System.Windows.Forms.DataVisualization.Charting.Chart chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            chart1.ChartAreas.Add(new ChartArea());

            chart1.Titles.Add("Title1");
            chart1.Titles["Title1"].Docking = Docking.Top;
            chart1.Titles["Title1"].Font = new Font("Arial", 20);
            chart1.Titles["Title1"].Text = title;


            Color[] classcolor = new Color[3] { Color.Blue, Color.Red, Color.Green };

            for (int i = 0; i < 3; i++)
            {

                Series ss = new Series(i.ToString());
                ss.ChartType = SeriesChartType.Point;
                ss.MarkerSize = TBmarkersize.Value;
                ss.MarkerColor = classcolor[i];

                //chart1.Series.Add(sslop);

                int j = 0;
                foreach (double[] dd in data)
                {
                    if (langclass[j] == i)
                        ss.Points.AddXY(dd[0], dd[1]);
                    j++;
                }

                chart1.Series.Add(ss);
            }
            chart1.ChartAreas[0].AxisX.Title = label1;
            chart1.ChartAreas[0].AxisY.Title = label2;
            double chartlimit = util.tryconvertdouble(TBpcachartmax.Text);

            chart1.ChartAreas[0].AxisX.Maximum =  chartlimit;
            chart1.ChartAreas[0].AxisX.Minimum = -chartlimit;
            chart1.ChartAreas[0].AxisY.Maximum =  chartlimit;
            chart1.ChartAreas[0].AxisY.Minimum = -chartlimit;

            chart1.ChartAreas[0].AxisX.TitleFont = new Font(FontFamily.GenericSansSerif, 20);
            chart1.ChartAreas[0].AxisY.TitleFont = new Font(FontFamily.GenericSansSerif, 20);
            
            return chart1;

        }



        private bool islopsided(string domain1,string domain2,string l1,string l2)
        {
            if (lopsideddict.ContainsKey(domain1))
            {
                if (lopsideddict[domain1].Contains(l1))
                    return true;
                if (lopsideddict[domain1].Contains(l2))
                    return true;
            }
            if (lopsideddict.ContainsKey(domain2))
            {
                if (lopsideddict[domain2].Contains(l1))
                    return true;
                if (lopsideddict[domain2].Contains(l2))
                    return true;
            }
            return false;
        }

        private System.Windows.Forms.DataVisualization.Charting.Chart chart_matrix_value_pairs(Dictionary<string, Dictionary<string, double>> m1, Dictionary<string, Dictionary<string, double>> m2, string label1, string label2)
        {
            return chart_matrix_value_pairs(m1, m2, label1, label2, true, true, true, true);
        }

        private System.Windows.Forms.DataVisualization.Charting.Chart chart_matrix_value_pairs(Dictionary<string, Dictionary<string, double>> m1, Dictionary<string, Dictionary<string, double>> m2, string label1, string label2, bool doll, bool dols, bool dosl, bool doss)
        {
            Dictionary<string, int> wingdict = new Dictionary<string, int>();

            System.Windows.Forms.DataVisualization.Charting.Chart chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            chart1.ChartAreas.Add(new ChartArea());
            Series ss = new Series("XXX");
            ss.ChartType = SeriesChartType.Point;
            Series ssll = new Series("Large-large");
            ssll.ChartType = SeriesChartType.Point;
            Series ssls = new Series("Large-small");
            ssls.ChartType = SeriesChartType.Point;
            Series ssss = new Series("Small-small");
            ssss.ChartType = SeriesChartType.Point;
            Series sssl = new Series("Small-large");
            sssl.ChartType = SeriesChartType.Point;
            Series sslop = new Series("Lopsided");
            sslop.ChartType = SeriesChartType.Point;

            


            double largelimit = util.tryconvertdouble(TBlargelimit.Text);
            double winglimit = 1.5;

            int i = 0;
            int nwing = 0;
            memo("Language\t" + label1 + "\t" + label2);
            List<string> s1done = new List<string>();
            //string fn = unusedfn(outfolder + label1 + "-" + label2 + "-valuepairs.txt");
            //using (StreamWriter sw = new StreamWriter(fn))
            {
                foreach (string s1 in m1.Keys)
                {
                    s1done.Add(s1);
                    if (!m2.ContainsKey(s1))
                        continue;
                    foreach (string s2 in m1[s1].Keys)
                    {
                        if (s2 == s1)
                            continue;
                        if (s1done.Contains(s2))
                            continue;
                        if (!m2[s1].ContainsKey(s2))
                            continue;
                        if ((m1[s1][s2] > 0) && (m2[s1][s2] > 0))
                        {
                            i++;
                            if (i % 1000 == 0)
                                memo(i.ToString());
                            //sw.WriteLine(s1 + "/" + s2 + "\t" + m1[s1][s2] + "\t" + m2[s1][s2]);
                            if (islopsided(label1,label2,s1,s2))//(lopsided.Contains(s1) || lopsided.Contains(s2))
                                sslop.Points.AddXY(m1[s1][s2], m2[s1][s2]);
                            else
                                ss.Points.AddXY(m1[s1][s2], m2[s1][s2]);
                            double wing = Math.Abs(Math.Log(m1[s1][s2]) - Math.Log(m2[s1][s2]));
                            if (wing > winglimit)
                            {
                                nwing++;
                                if (!wingdict.ContainsKey(s1))
                                    wingdict.Add(s1, 0);
                                if (!wingdict.ContainsKey(s2))
                                    wingdict.Add(s2, 0);
                                wingdict[s1]++;
                                wingdict[s2]++;
                            }
                            //else
                            //    continue;

                            //if ( coverdict[s1] > largelimit)
                            //{
                            //    if ( coverdict[s2] > largelimit)
                            //        ssll.Points.AddXY(m1[s1][s2], m2[s1][s2]);
                            //    else
                            //        ssls.Points.AddXY(m1[s1][s2], m2[s1][s2]);
                            //}
                            //else
                            //{
                            //    if (coverdict[s2] > largelimit)
                            //        sssl.Points.AddXY(m1[s1][s2], m2[s1][s2]);
                            //    else
                            //        ssss.Points.AddXY(m1[s1][s2], m2[s1][s2]);

                            //}
                        }
                    }
                }
            }
            ss.MarkerSize = TBmarkersize.Value;
            ss.MarkerColor = Color.Blue;
            sslop.MarkerSize = TBmarkersize.Value;
            sslop.MarkerColor = Color.Red;
            ssll.MarkerSize = TBmarkersize.Value;
            ssll.MarkerColor = Color.Red;
            ssls.MarkerSize = TBmarkersize.Value;
            ssls.MarkerColor = Color.Green;
            sssl.MarkerSize = TBmarkersize.Value;
            sssl.MarkerColor = Color.Orange;
            ssss.MarkerSize = TBmarkersize.Value;
            ssss.MarkerColor = Color.Black;
            //if (doll)
            //    chart1.Series.Add(ssll);
            //if (dols)
            //    chart1.Series.Add(ssls);
            //if (dosl)
            //    chart1.Series.Add(sssl);
            //if (doss)
            //    chart1.Series.Add(ssss);
            chart1.Series.Add(ss);
            //chart1.Series.Add(sslop);

            chart1.ChartAreas[0].AxisX.IsLogarithmic = true;
            chart1.ChartAreas[0].AxisY.IsLogarithmic = true;
            chart1.ChartAreas[0].AxisX.Title = label1;
            chart1.ChartAreas[0].AxisY.Title = label2;
            chart1.ChartAreas[0].AxisX.TitleFont = new Font(FontFamily.GenericSansSerif,20);
            chart1.ChartAreas[0].AxisY.TitleFont = new Font(FontFamily.GenericSansSerif, 20);
            
            memo("Done " + i);
            memo("Wing fraction = " + 100 * nwing / (double)i + " %");
            foreach (string s in wingdict.Keys)
            {
                if (wingdict[s] > 50)
                    memo(s + "\t" + wingdict[s]);
            }
            return chart1;

        }

        
        private void treebutton_Click(object sender, EventArgs e)
        {
            read_normalization_matrix();
            string fn = normalize_distances();
            write_matrix(insertafterpath(fn,"distmatrix-"), iwdistdict);
            write_matrix(insertafterpath(fn, "symdistmatrix-"), make_symmetric(iwdistdict));
            memo("Done");
        }

        private string get_domain_from_fn(string fn)
        {
            foreach (string dd in domainlist)
                if (fn.Contains(dd))
                    return dd;
            return fn;
        }

        private void Comparebutton_Click(object sender, EventArgs e)
        {
            Dictionary<string, Dictionary<string, double>> matrix1 = read_matrix("Matrix 1");
            string m1fn = recentfile;
            Dictionary<string, Dictionary<string, double>> matrix2 = read_matrix("Matrix 2");
            string m2fn = recentfile;
            //print_matrix_value_pairs(matrix1, matrix2, m1fn,m1fn);

            ChartForm cf = new ChartForm(chart_matrix_value_pairs(matrix1, matrix2, get_domain_from_fn(m1fn), get_domain_from_fn(m2fn), true, true, true, true));
            cf.Show();
            //ChartForm cf1 = new ChartForm(chart_matrix_value_pairs(matrix1, matrix2, m1fn, m1fn,true,false,false,false));
            //cf1.Show();
            //ChartForm cf2 = new ChartForm(chart_matrix_value_pairs(matrix1, matrix2, m1fn, m1fn, false, true, false, false));
            //cf2.Show();
            //ChartForm cf3 = new ChartForm(chart_matrix_value_pairs(matrix1, matrix2, m1fn, m1fn, false, false, true, false));
            //cf3.Show();
            //ChartForm cf4 = new ChartForm(chart_matrix_value_pairs(matrix1, matrix2, m1fn, m1fn, false, false, false, true));
            //cf4.Show();

        }

        private void neighborjoinbutton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("nj-click");
            Dictionary<string, Dictionary<string, double>> matrix1 = read_matrix("Matrix to neighbor-join");
            memo("nj");
            NeighborJoining nj = new NeighborJoining();
            memo("BuildTree");
            string fn = unusedfn(outfolder + "njtree.txt");
            nj.BuildTree(matrix1,fn);
            memo("Done");
        }

        private void testbutton_Click(object sender, EventArgs e)
        {
            // Below is the same data used on the excellent paper "Tutorial
            //   On Principal Component Analysis", by Lindsay Smith (2002).
            double[][] data =
{
    new double[] { 2.5,  2.4 },
    new double[] { 0.5,  0.7 },
    new double[] { 2.2,  2.9 },
    new double[] { 1.9,  2.2 },
    new double[] { 3.1,  3.0 },
    new double[] { 2.3,  2.7 },
    new double[] { 2.0,  1.6 },
    new double[] { 1.0,  1.1 },
    new double[] { 1.5,  1.6 },
    new double[] { 1.1,  0.9 }
};

            Console.WriteLine(data.ToString(" 000.00"));
            //Scatterplot sc = new Scatterplot("Data");
            //sc.Compute(data);
            ScatterplotBox.Show(data);
            
            // Let's create an analysis with centering (covariance method)
            // but no standardization (correlation method) and whitening:
            var pca = new PrincipalComponentAnalysis()
            {
                Method = PrincipalComponentMethod.Center,
                Whiten = true
            };

            // Now we can learn the linear projection from the data
            MultivariateLinearRegression transform = pca.Learn(data);
            //Console.WriteLine(transform.ToString());

            // Finally, we can project all the data
            double[][] output1 = pca.Transform(data);
            Console.WriteLine(output1.ToString(" 0.000"));

            ScatterplotBox.Show("output1",output1);
            Console.WriteLine("Cumulative proportions:");
            foreach (double d in pca.CumulativeProportions)
                Console.Write(d+" ");
            Console.WriteLine();
            Console.WriteLine("Eigenvalues:");
            foreach (double d in pca.Eigenvalues)
                Console.Write(d + " ");
            Console.WriteLine();


            // Or just its first components by setting 
            // NumberOfOutputs to the desired components:
            pca.NumberOfOutputs = 1;

            // And then calling transform again:
            double[][] output2 = pca.Transform(data);
            //ScatterplotBox.Show("Single component",output2);
            //HistogramBox.Show(output2);

            // We can also limit to 80% of explained variance:
            pca.ExplainedVariance = 0.8;

            // And then call transform again:
            double[][] output3 = pca.Transform(data);
            //ScatterplotBox.Show("Variance 80%",output3);

            Console.WriteLine("Done");
        }

        private void print_components(domaintableclass dc, double[][] output, int ncomponents)
        {
            foreach (int ilang in dc.langnames.Keys)
            {
                StringBuilder sb = new StringBuilder(dc.langnames[ilang] + "\t" + dc.langstat[ilang]);
                for (int i = 0; i < ncomponents; i++)
                    sb.Append("\t" + output[ilang][i]);
                memo(sb.ToString());
            }

        }

        private double[][] select_components(domaintableclass dc, double[][] pcdata, int pc1, int pc2)
        {
            double[][] selecteddata = new double[dc.langnames.Count][];

            foreach (int ilang in dc.langnames.Keys)
            {
                selecteddata[ilang] = new double[2];
                selecteddata[ilang][0] = pcdata[ilang][pc1 - 1];
                selecteddata[ilang][1] = pcdata[ilang][pc2 - 1];
            }
            return selecteddata;
        }

        //public delegate void Del();

        //public static void DelegateSetSize()
        //{
        //    SetSize()
        //}

        private void plot_pcarray(domaintableclass dc, double[][] pcdata, int maxpc, int[] langclass)
        {
            int nplot = maxpc*(maxpc-1)/2;
            ScatterplotBox[] sb = new ScatterplotBox[nplot];
            ScatterplotBox.CheckForIllegalCrossThreadCalls = false;
            Thread[] formThread = new Thread[nplot];
            AutoResetEvent stopWaitHandle = new AutoResetEvent(false);

            int iplot = 0;
            for (int p1 = 1;p1<maxpc;p1++)
            {
                for (int p2=p1+1;p2<=maxpc;p2++)
                {
                    //formThread[iplot] = new Thread(() =>
                    //{
                        sb[iplot] = ScatterplotBox.Show("PC" + p1 + " vs. PC" + p2, select_components(dc, pcdata, p1, p2), langclass);
                    

                        sb[iplot].SetSize(800, 800);
                        sb[iplot].SetSymbolSize(TBmarkersize.Value);
                 
                        //sb[iplot] = ScatterplotBox.SetSize(600, 600);

                        //sb[iplot]. .formThread = formThread[iplot];
                        //stopWaitHandle.Set();
                        //Application.Run(sb[iplot]);
                    //});

                    //formThread[iplot].SetApartmentState(ApartmentState.STA);

                    //formThread[iplot].Start();

                    //stopWaitHandle.WaitOne();

                    //if (!hold)
                    //    formThread[iplot].Join();


                    iplot++;
                }
            }
        }

        private void plot_pcarray2(domaintableclass dc, double[][] pcdata, int maxpc, int[] langclass, string title)
        {

            int nplot = maxpc * (maxpc - 1) / 2;

            int iplot = 0;
            for (int p1 = 1; p1 < maxpc; p1++)
            {
                for (int p2 = p1 + 1; p2 <= maxpc; p2++)
                {
                    //sb[iplot] = ScatterplotBox.Show("PC" + p1 + " vs. PC" + p2, select_components(dc, pcdata, p1, p2), langclass);

                    ChartForm cf = new ChartForm(chart_from_pca(select_components(dc,pcdata,p1,p2),"PC"+p1,"PC"+p2,title,langclass)); 
                    cf.Show();


                    iplot++;
                }
            }
        }

        private void PCAbutton_Click(object sender, EventArgs e)
        {
            domaintableclass dc = new domaintableclass();
            //dc.maxdim = 20;
            string fn = dc.readfile(outfolder,CB_normalize.Checked);

            string domain = get_domain_from_fn(fn);

            var pca = new PrincipalComponentAnalysis()
            {
                Method = PrincipalComponentMethod.Center,
                Whiten = true
            };

            // Now we can learn the linear projection from the data
            MultivariateLinearRegression transform = pca.Learn(dc.data);
            //Console.WriteLine(transform.ToString());

            // Finally, we can project all the data
            double[][] output1 = pca.Transform(dc.data);
            //Console.WriteLine(output1.ToString(" 0.000"));


            //ScatterplotBox.Show("output1", output1);
            memo("Cumulative proportions:");
            foreach (double d in pca.CumulativeProportions)
                memo(d + " ");
            //Console.WriteLine();
            memo("Eigenvalues:");
            foreach (double d in pca.Eigenvalues)
                memo(d + " ");
            


            // Or just its first components by setting 
            // NumberOfOutputs to the desired components:
            pca.NumberOfOutputs = 2;

            // And then calling transform again:
            double[][] output2 = pca.Transform(dc.data);

            int[] langclass = new int[dc.langstat.Count];
            foreach (int n in dc.langstat.Keys)
            {
                if (dc.langstat[n] > 20)
                    langclass[n] = 0;
                else if (dc.langstat[n] > 5)
                    langclass[n] = 1;
                else
                    langclass[n] = 2;
            }
            int[] langclasscount = new int[3]{0,0,0};
            foreach (int n in langclass)
                langclasscount[n]++;
            for (int i = 0; i < 3; i++)
                memo(i + ":" + langclasscount[i]);

            //plot_pcarray(dc, output1, 3, langclass);
            plot_pcarray2(dc, output1, 3, langclass,domain);

            //ScatterplotBox sb = ScatterplotBox.Show("PC1 vs PC2", output2, langclass);
            //sb.SetSize(600, 600);
            //sb.SetSymbolSize(2);
            ////HistogramBox.Show(output2);
            ////Console.WriteLine(output2.ToString(" 0.000"));

            //ScatterplotBox.Show("select_components",select_components(dc,output1,1,2));

            // We can also limit to 80% of explained variance:
            pca.ExplainedVariance = 0.8;

            // And then call transform again:
            double[][] output3 = pca.Transform(dc.data);
            //ScatterplotBox.Show("Variance 80%",output3);

            //print_components(dc, output1, 5);

            memo("Done");
        }

        private void nexusbutton_Click(object sender, EventArgs e)
        {
            Dictionary<string, Dictionary<string, double>> distdict = read_matrix("Distance matrix file");
            make_nexus_file(distdict);

        }

        private void make_nexus_file(Dictionary<string, Dictionary<string, double>> distdict)
        {
            string domain = get_domain_from_fn(recentfile);
            string fn = unusedfn(outfolder + "distmatrix.nex");
            memo("Writing " + fn);
            using (StreamWriter sw = new StreamWriter(fn))
            {
                sw.WriteLine("#nexus");
                sw.WriteLine();
                sw.WriteLine("BEGIN Taxa;");
                int ntax = 0;
                foreach (string s in distdict.Keys)
                {
                    if (islopsided(domain, "", s, ""))
                        continue;
                    ntax++;
                }
                sw.WriteLine("DIMENSIONS ntax=" + ntax + ";");
                sw.WriteLine("TAXLABELS");
                int k = 1;
                foreach (string s in distdict.Keys)
                {
                    if (islopsided(domain, "", s, ""))
                        continue;
                    sw.WriteLine("[" + k + "] '" + s + "'");
                    k++;
                }
                sw.WriteLine(";");
                sw.WriteLine("END; [Taxa]");
                sw.WriteLine("");
                sw.WriteLine("BEGIN Distances;");
                sw.WriteLine("DIMENSIONS ntax=" + ntax + ";");
                sw.WriteLine("FORMAT labels=left diagonal triangle=both;");
                sw.WriteLine("MATRIX");
                k = 1;
                foreach (string s1 in distdict.Keys)
                {
                    if (islopsided(domain, "", s1, ""))
                        continue;
                    StringBuilder sb = new StringBuilder(("[" + k + "] '" + s1 + "'").PadRight(12));
                    foreach (string s2 in distdict[s1].Keys)
                    {
                        if (islopsided(domain, "", s2, ""))
                            continue;
                        sb.Append(" " + distdict[s1][s2].ToString("N8", new CultureInfo("en-US")));
                    }
                    sw.WriteLine(sb);
                    k++;
                }
                sw.WriteLine(";");
                sw.WriteLine("END; [Distance]");
                sw.WriteLine("");
                sw.WriteLine("BEGIN st_Assumptions;");
                sw.WriteLine("uptodate;");
                sw.WriteLine("chartransform=.Uncorrected_P ;");
                sw.WriteLine("disttransform=NeighborNet ;");
                sw.WriteLine("splitstransform=.EqualAngle ;");
                sw.WriteLine("SplitsPostProcess filter=dimension value=4;");
                sw.WriteLine(" exclude  no missing;");
                sw.WriteLine("autolayoutnodelabels;");
                sw.WriteLine("END; [st_Assumptions]");
                sw.WriteLine("");


            }
            memo("NEXUS file done");
        }

       

        private void logtransformbutton_Click(object sender, EventArgs e)
        {
            Dictionary<string, Dictionary<string, double>> distdict = read_matrix("Distance matrix file to transform");
            string fn = openFileDialog1.FileName;
            string fnlog = insertbeforesuffix(fn, ".log");
            double min = Double.MaxValue;
            foreach (string s1 in distdict.Keys)
                foreach (string s2 in distdict[s1].Keys)
                {
                    if (distdict[s1][s2] > 0 && distdict[s1][s2] < min)
                        min = distdict[s1][s2];
                }
            double logmin = Math.Log(min)-0.1;
            memo("logmin = " + logmin);
            List<string> s1keys = distdict.Keys.ToList();
            foreach (string s1 in s1keys)
            {
                List<string> s2keys = distdict[s1].Keys.ToList();
                foreach (string s2 in s2keys)
                {
                    if (distdict[s1][s2] > 0)
                        distdict[s1][s2] = Math.Log(distdict[s1][s2]) - logmin;
                }
            }
            write_matrix(fnlog, distdict);

        }

        private void randombutton_Click(object sender, EventArgs e)
        {
            read_normalization_matrix();
            string fn = normalize_distances();
            //write_matrix(insertafterpath(fn, "distmatrix-"), iwdistdict);
            write_matrix(insertafterpath(fn, "random-distmatrix-"), make_random(iwdistdict));
            memo("Done");

        }

        private void Kmeansbutton_Click(object sender, EventArgs e)
        {
            domaintableclass dc = new domaintableclass();
            //dc.maxdim = 20;
            dc.readfile(outfolder, CB_normalize.Checked);

            // Create a new K-Means algorithm
            KMeans kmeans = new KMeans(k: TBcluster.Value);

            // Compute and retrieve the data centroids
            var clusters = kmeans.Learn(dc.data);

            memo("Learn done");

            // Use the centroids to parition all the data
            int[] labels = clusters.Decide(dc.data);

            //memo(clusters.ToString());

            Dictionary<int, int> clustersize = new Dictionary<int, int>();
            Dictionary<int, List<string>> clusterlist = new Dictionary<int, List<string>>();

            for (int i = 0; i < labels.Length; i++)
            {
                memo(i + "\t" + dc.langnames[i] + "\t" + labels[i]);
                if (!clustersize.ContainsKey(labels[i]))
                {
                    clustersize.Add(labels[i], 0);
                    clusterlist.Add(labels[i], new List<string>());
                }
                clustersize[labels[i]]++;
                clusterlist[labels[i]].Add(dc.langnames[i]);
            }

            foreach (int i in clustersize.Keys)
            {
                string s = "";
                foreach (string ss in clusterlist[i])
                    s += " " + ss;
                memo(i + ": " + clustersize[i] + ";" + s);
            }
        }

        private void TBcluster_Scroll(object sender, EventArgs e)
        {
            memo(TBcluster.Value + " clusters");
        }

        private void meanshiftbutton_Click(object sender, EventArgs e)
        {
            domaintableclass dc = new domaintableclass();
            //dc.maxdim = 20;
            dc.readfile(outfolder, CB_normalize.Checked);

            memo("Done reading");
            this.Refresh();

            // Create a new Mean-Shift algorithm for 3 dimensional samples
            MeanShift meanShift = new MeanShift()
            {
                // Use a uniform kernel density
                Kernel = new UniformKernel(),
                Bandwidth = util.tryconvertdouble(TBmeanshift.Text)
            };

            memo("Learning...");
            this.Refresh();

            // Learn a data partitioning using the Mean Shift algorithm
            MeanShiftClusterCollection clustering = meanShift.Learn(dc.data);

            memo("Learn done");

            // Predict group labels for each point
            int[] labels = clustering.Decide(dc.data);

            Dictionary<int, int> clustersize = new Dictionary<int, int>();
            Dictionary<int, List<string>> clusterlist = new Dictionary<int, List<string>>();
 
            for (int i = 0; i < labels.Length; i++)
            {
                memo(i + "\t" + dc.langnames[i] + "\t" + labels[i]);
                if ( !clustersize.ContainsKey(labels[i]))
                {
                    clustersize.Add(labels[i], 0);
                    clusterlist.Add(labels[i], new List<string>());
                }
                clustersize[labels[i]]++;
                clusterlist[labels[i]].Add(dc.langnames[i]);
            }

            foreach (int i in clustersize.Keys)
            {
                string s = "";
                foreach (string ss in clusterlist[i])
                    s += " "+ss;
                memo(i + ": " + clustersize[i] + ";" + s);
            }

        }

        private void domaincoverbutton_Click(object sender, EventArgs e)
        {
            Dictionary<string, domaintableclass> domaintabledict = new Dictionary<string, domaintableclass>();
            
            hbookclass relcoverhist = new hbookclass("coverage in each domain relative to people coverage");
            relcoverhist.SetBins(0, 1.2, 30);

            string fn0 = "";
            string domain0 = "";
            foreach (string domain in domainlist)
            {
                domaintableclass dtc = new domaintableclass();
                if ( String.IsNullOrEmpty(fn0))
                {
                    domain0 = domain;
                    fn0 = dtc.readfile(outfolder, "Table for " + domain + " domain:", false);
                }
                else
                {
                    string fn = fn0.Replace(domain0, domain);
                    memo("Reading " + fn);
                    dtc.readnamedfile(fn);
                }
                domaintabledict.Add(domain, dtc);
                double nconcept = dtc.conceptnames.Count;
                memo("Domain "+domain+": "+nconcept+" concepts");
                foreach (int ilang in dtc.langstat.Keys)
                {
                    double cc = dtc.langstat[ilang] / nconcept;
                    relcoverhist.Add(cc);
                }
            }
            memo(relcoverhist.GetDHist());

            hbookclass ndomainhist = new hbookclass("# domains where a language has entries");
            hbookclass outlierhist = new hbookclass("Largest domain coverage relative coverage sum");
            outlierhist.SetBins(0, 1, 20);

            foreach (string lang in coverdict.Keys)
            {
                int nfound = 0;
                double coversum = 0;
                List<double> domaincover = new List<double>();
                Dictionary<string, double> domaincoverdict = new Dictionary<string, double>();
                foreach (string domain in domainlist)
                {
                    if (domaintabledict[domain].langcolumns.ContainsKey(lang))
                        if (domaintabledict[domain].langstat[domaintabledict[domain].langcolumns[lang]] > 0)
                        {
                            nfound++;
                            double nconcept = domaintabledict[domain].conceptnames.Count;
                            double cc = domaintabledict[domain].langstat[domaintabledict[domain].langcolumns[lang]] / nconcept;
                            coversum += cc;
                            domaincover.Add(cc);
                            domaincoverdict.Add(domain, cc);
                        }
                }
                ndomainhist.Add(nfound);
                if (nfound >= 3)
                {
                    foreach (string domain in domaincoverdict.Keys)
                    {
                        if ((domaincoverdict[domain] / coversum > 0.6) || (domaincoverdict[domain]/coversum < 0.1))
                            memo(lang + "\t" + domain + "\t" + domaincoverdict[domain] / coversum);
                    }
                    double maxfraction = domaincover.Max() / coversum;
                    outlierhist.Add(maxfraction);
                    //if (maxfraction > 0.7)
                    //{
                    //    memo(lang + "\t" + maxfraction);
                    //}
                }
            }

            memo(ndomainhist.GetIHist());
            memo(outlierhist.GetDHist());
        }

        Dictionary<int,double> fill_centuryfreq()
        {
            Dictionary<int, double> centuryfreq = new Dictionary<int, double>();
            centuryfreq.Add(-99900, 0.422198683549089);
            centuryfreq.Add(-6000, 2.12869833396361E-07);
            centuryfreq.Add(-5000, 2.12869833396361E-07);
            centuryfreq.Add(-4900, 1.0643491669818E-07);
            centuryfreq.Add(-4200, 3.19304750094541E-07);
            centuryfreq.Add(-4100, 1.0643491669818E-07);
            centuryfreq.Add(-4000, 4.25739666792721E-07);
            centuryfreq.Add(-3500, 1.17078408367998E-06);
            centuryfreq.Add(-3200, 5.32174583490901E-07);
            centuryfreq.Add(-3100, 1.0643491669818E-06);
            centuryfreq.Add(-3000, 2.44800308405815E-06);
            centuryfreq.Add(-2900, 1.5965237504727E-06);
            centuryfreq.Add(-2800, 2.44800308405815E-06);
            centuryfreq.Add(-2700, 3.72522208443631E-06);
            centuryfreq.Add(-2600, 8.40835841915624E-06);
            centuryfreq.Add(-2500, 1.11756662533089E-05);
            centuryfreq.Add(-2400, 4.68313633471993E-06);
            centuryfreq.Add(-2300, 5.96035533509809E-06);
            centuryfreq.Add(-2200, 5.00244108481447E-06);
            centuryfreq.Add(-2100, 4.78957125141811E-06);
            centuryfreq.Add(-2000, 9.36627266943986E-06);
            centuryfreq.Add(-1900, 6.81183466868354E-06);
            centuryfreq.Add(-1800, 9.79201233623258E-06);
            centuryfreq.Add(-1700, 8.94053300264714E-06);
            centuryfreq.Add(-1600, 1.19207106701962E-05);
            centuryfreq.Add(-1500, 1.79875009219925E-05);
            centuryfreq.Add(-1400, 1.60716724214252E-05);
            centuryfreq.Add(-1300, 1.95840246724652E-05);
            centuryfreq.Add(-1200, 9.68557741953441E-06);
            centuryfreq.Add(-1100, 7.45044416887262E-06);
            centuryfreq.Add(-1000, 7.13113941877808E-06);
            centuryfreq.Add(-900, 9.04696791934532E-06);
            centuryfreq.Add(-800, 2.61829895077523E-05);
            centuryfreq.Add(-700, 3.34205638432286E-05);
            centuryfreq.Add(-600, 7.37593972718389E-05);
            centuryfreq.Add(-500, 0.000128041204787911);
            centuryfreq.Add(-400, 0.000134640169623198);
            centuryfreq.Add(-300, 0.000122932328786398);
            centuryfreq.Add(-200, 0.000129424858704987);
            centuryfreq.Add(-100, 0.000143048528042354);
            centuryfreq.Add(0, 0.00027609217391508);
            centuryfreq.Add(100, 0.000236179080153262);
            centuryfreq.Add(200, 0.000236391949986658);
            centuryfreq.Add(300, 0.000248951270157044);
            centuryfreq.Add(400, 0.000301742988839341);
            centuryfreq.Add(500, 0.000343571911101726);
            centuryfreq.Add(600, 0.000355279751938526);
            centuryfreq.Add(700, 0.000417118438540168);
            centuryfreq.Add(800, 0.000457670141802175);
            centuryfreq.Add(900, 0.000543137379910814);
            centuryfreq.Add(1000, 0.000733017271300368);
            centuryfreq.Add(1100, 0.00112501706949977);
            centuryfreq.Add(1200, 0.00159354357280516);
            centuryfreq.Add(1300, 0.00210315395395604);
            centuryfreq.Add(1400, 0.00366402200733486);
            centuryfreq.Add(1500, 0.00814312260674438);
            centuryfreq.Add(1600, 0.0133227778278613);
            centuryfreq.Add(1700, 0.0282317552192756);
            centuryfreq.Add(1800, 0.125102323868041);
            centuryfreq.Add(1900, 0.371356320366119);
            centuryfreq.Add(2000, 0.0141719155932794);
            centuryfreq.Add(2100, 9.57914250283622E-07);
            centuryfreq.Add(2900, 1.0643491669818E-07);
            centuryfreq.Add(9900, 0.00385752068589215);
            return centuryfreq;
        }

        Dictionary<string,Dictionary<int,int>> fill_countrytimedict()
        {
            Dictionary<string, Dictionary<int, int>> countrytimedict = new Dictionary<string, Dictionary<int, int>>();
            countrytimedict.Add("United States of America", new Dictionary<int, int>());
            countrytimedict.Add("China", new Dictionary<int, int>());
            countrytimedict.Add("Germany", new Dictionary<int, int>());
            countrytimedict.Add("France", new Dictionary<int, int>());
            countrytimedict.Add("Japan", new Dictionary<int, int>());
            countrytimedict.Add("United Kingdom", new Dictionary<int, int>());
            countrytimedict.Add("Spain", new Dictionary<int, int>());
            countrytimedict.Add("Italy", new Dictionary<int, int>());
            countrytimedict.Add("Russia", new Dictionary<int, int>());
            countrytimedict.Add("Poland", new Dictionary<int, int>());
            countrytimedict.Add("Czech Republic", new Dictionary<int, int>());
            countrytimedict.Add("Sweden", new Dictionary<int, int>());
            countrytimedict.Add("Netherlands", new Dictionary<int, int>());
            countrytimedict.Add("Brazil", new Dictionary<int, int>());
            countrytimedict.Add("India", new Dictionary<int, int>());
            countrytimedict.Add("Canada", new Dictionary<int, int>());
            countrytimedict.Add("Ukraine", new Dictionary<int, int>());
            countrytimedict.Add("Finland", new Dictionary<int, int>());
            countrytimedict.Add("Norway", new Dictionary<int, int>());
            countrytimedict.Add("Belgium", new Dictionary<int, int>());
            countrytimedict.Add("Austria", new Dictionary<int, int>());
            countrytimedict.Add("Australia", new Dictionary<int, int>());
            countrytimedict.Add("Romania", new Dictionary<int, int>());
            countrytimedict.Add("Switzerland", new Dictionary<int, int>());
            countrytimedict.Add("Denmark", new Dictionary<int, int>());
            countrytimedict.Add("Hungary", new Dictionary<int, int>());
            countrytimedict.Add("Turkey", new Dictionary<int, int>());
            countrytimedict.Add("Argentina", new Dictionary<int, int>());
            countrytimedict.Add("Korea", new Dictionary<int, int>());
            countrytimedict.Add("Greece", new Dictionary<int, int>());
            countrytimedict.Add("Indonesia", new Dictionary<int, int>());
            countrytimedict.Add("Mexico", new Dictionary<int, int>());
            countrytimedict.Add("Israel", new Dictionary<int, int>());
            countrytimedict.Add("New Zealand", new Dictionary<int, int>());
            countrytimedict.Add("Iran", new Dictionary<int, int>());
            countrytimedict.Add("Portugal", new Dictionary<int, int>());
            countrytimedict.Add("Slovenia", new Dictionary<int, int>());
            countrytimedict.Add("Ireland", new Dictionary<int, int>());
            countrytimedict.Add("Bulgaria", new Dictionary<int, int>());
            countrytimedict.Add("Estonia", new Dictionary<int, int>());
            countrytimedict.Add("Slovakia", new Dictionary<int, int>());
            countrytimedict.Add("Ancient Rome", new Dictionary<int, int>());
            countrytimedict.Add("Belarus", new Dictionary<int, int>());
            countrytimedict.Add("South Africa", new Dictionary<int, int>());
            countrytimedict.Add("Uruguay", new Dictionary<int, int>());
            countrytimedict.Add("Azerbaijan", new Dictionary<int, int>());
            countrytimedict.Add("Taiwan", new Dictionary<int, int>());
            countrytimedict.Add("Lithuania", new Dictionary<int, int>());
            countrytimedict.Add("Serbia", new Dictionary<int, int>());
            countrytimedict.Add("Chile", new Dictionary<int, int>());
            countrytimedict.Add("Soviet Union", new Dictionary<int, int>());
            countrytimedict.Add("Latvia", new Dictionary<int, int>());
            countrytimedict.Add("Colombia", new Dictionary<int, int>());
            countrytimedict.Add("Egypt", new Dictionary<int, int>());
            countrytimedict.Add("Croatia", new Dictionary<int, int>());
            countrytimedict.Add("Nigeria", new Dictionary<int, int>());
            countrytimedict.Add("Thailand", new Dictionary<int, int>());
            countrytimedict.Add("Armenia", new Dictionary<int, int>());
            countrytimedict.Add("Vietnam", new Dictionary<int, int>());
            countrytimedict.Add("Kazakhstan", new Dictionary<int, int>());
            countrytimedict.Add("England", new Dictionary<int, int>());
            countrytimedict.Add("Peru", new Dictionary<int, int>());
            countrytimedict.Add("Philippines", new Dictionary<int, int>());
            countrytimedict.Add("Georgia", new Dictionary<int, int>());
            countrytimedict.Add("Pakistan", new Dictionary<int, int>());
            countrytimedict.Add("Venezuela", new Dictionary<int, int>());
            countrytimedict.Add("Malaysia", new Dictionary<int, int>());
            countrytimedict.Add("Algeria", new Dictionary<int, int>());
            countrytimedict.Add("Iceland", new Dictionary<int, int>());
            countrytimedict.Add("Cuba", new Dictionary<int, int>());
            countrytimedict.Add("Czechoslovakia", new Dictionary<int, int>());
            countrytimedict.Add("Albania", new Dictionary<int, int>());
            countrytimedict.Add("Morocco", new Dictionary<int, int>());
            countrytimedict.Add("Luxembourg", new Dictionary<int, int>());
            countrytimedict.Add("Polish–Lithuanian Commonwealth", new Dictionary<int, int>());
            countrytimedict.Add("Tunisia", new Dictionary<int, int>());
            countrytimedict.Add("Bangladesh", new Dictionary<int, int>());
            countrytimedict.Add("North Macedonia", new Dictionary<int, int>());
            countrytimedict.Add("Saudi Arabia", new Dictionary<int, int>());
            countrytimedict.Add("Iraq", new Dictionary<int, int>());
            countrytimedict.Add("Ghana", new Dictionary<int, int>());
            countrytimedict.Add("Bosnia and Herzegovina", new Dictionary<int, int>());
            countrytimedict.Add("Sri Lanka", new Dictionary<int, int>());
            countrytimedict.Add("Lebanon", new Dictionary<int, int>());
            countrytimedict.Add("Kenya", new Dictionary<int, int>());
            countrytimedict.Add("Jamaica", new Dictionary<int, int>());
            countrytimedict.Add("Moldova", new Dictionary<int, int>());
            countrytimedict.Add("Ecuador", new Dictionary<int, int>());
            countrytimedict.Add("Uzbekistan", new Dictionary<int, int>());
            countrytimedict.Add("Syria", new Dictionary<int, int>());
            countrytimedict.Add("Dominican Republic", new Dictionary<int, int>());
            countrytimedict.Add("Haiti", new Dictionary<int, int>());
            countrytimedict.Add("Cyprus", new Dictionary<int, int>());
            countrytimedict.Add("Cameroon", new Dictionary<int, int>());
            countrytimedict.Add("Senegal", new Dictionary<int, int>());
            countrytimedict.Add("Paraguay", new Dictionary<int, int>());
            countrytimedict.Add("Nepal", new Dictionary<int, int>());
            countrytimedict.Add("Democratic Republic of the Congo", new Dictionary<int, int>());
            countrytimedict.Add("Byzantine Empire", new Dictionary<int, int>());
            countrytimedict.Add("Singapore", new Dictionary<int, int>());
            countrytimedict.Add("Afghanistan", new Dictionary<int, int>());
            countrytimedict.Add("Bolivia", new Dictionary<int, int>());
            countrytimedict.Add("Costa Rica", new Dictionary<int, int>());
            countrytimedict.Add("Uganda", new Dictionary<int, int>());
            countrytimedict.Add("Kyrgyzstan", new Dictionary<int, int>());
            countrytimedict.Add("Joseon", new Dictionary<int, int>());
            countrytimedict.Add("Yugoslavia", new Dictionary<int, int>());
            countrytimedict.Add("Cote d'Ivoire", new Dictionary<int, int>());
            countrytimedict.Add("Zimbabwe", new Dictionary<int, int>());
            countrytimedict.Add("Myanmar", new Dictionary<int, int>());
            countrytimedict.Add("Prussia", new Dictionary<int, int>());
            countrytimedict.Add("Montenegro", new Dictionary<int, int>());
            countrytimedict.Add("Ethiopia", new Dictionary<int, int>());
            countrytimedict.Add("Western Han", new Dictionary<int, int>());
            countrytimedict.Add("Kuwait", new Dictionary<int, int>());
            countrytimedict.Add("Malta", new Dictionary<int, int>());
            countrytimedict.Add("Trinidad and Tobago", new Dictionary<int, int>());
            countrytimedict.Add("Guatemala", new Dictionary<int, int>());
            countrytimedict.Add("Tanzania", new Dictionary<int, int>());
            countrytimedict.Add("Suriname", new Dictionary<int, int>());
            countrytimedict.Add("El Salvador", new Dictionary<int, int>());
            countrytimedict.Add("Panama", new Dictionary<int, int>());
            countrytimedict.Add("Tajikistan", new Dictionary<int, int>());
            countrytimedict.Add("South Korea", new Dictionary<int, int>());
            countrytimedict.Add("Yemen", new Dictionary<int, int>());
            countrytimedict.Add("Mongolia", new Dictionary<int, int>());
            countrytimedict.Add("United Arab Emirates", new Dictionary<int, int>());
            countrytimedict.Add("Palestine", new Dictionary<int, int>());
            countrytimedict.Add("African Americans", new Dictionary<int, int>());
            countrytimedict.Add("Honduras", new Dictionary<int, int>());
            countrytimedict.Add("Angola", new Dictionary<int, int>());
            countrytimedict.Add("Jordan", new Dictionary<int, int>());
            countrytimedict.Add("San Marino", new Dictionary<int, int>());
            countrytimedict.Add("Faroe Islands", new Dictionary<int, int>());
            countrytimedict.Add("Mali", new Dictionary<int, int>());
            countrytimedict.Add("Ottoman Empire", new Dictionary<int, int>());
            countrytimedict.Add("Kosovo", new Dictionary<int, int>());
            countrytimedict.Add("Zambia", new Dictionary<int, int>());
            countrytimedict.Add("Fiji", new Dictionary<int, int>());
            countrytimedict.Add("Bahrain", new Dictionary<int, int>());
            countrytimedict.Add("Manchu", new Dictionary<int, int>());
            countrytimedict.Add("Namibia", new Dictionary<int, int>());
            countrytimedict.Add("Barbados", new Dictionary<int, int>());
            countrytimedict.Add("The Gambia", new Dictionary<int, int>());
            countrytimedict.Add("Madagascar", new Dictionary<int, int>());
            countrytimedict.Add("Guinea", new Dictionary<int, int>());
            countrytimedict.Add("Libya", new Dictionary<int, int>());
            countrytimedict.Add("Nicaragua", new Dictionary<int, int>());
            countrytimedict.Add("Austria-Hungary", new Dictionary<int, int>());
            countrytimedict.Add("Qatar", new Dictionary<int, int>());
            countrytimedict.Add("Cambodia", new Dictionary<int, int>());
            countrytimedict.Add("Turkmenistan", new Dictionary<int, int>());
            countrytimedict.Add("Papua New Guinea", new Dictionary<int, int>());
            countrytimedict.Add("Somalia", new Dictionary<int, int>());
            countrytimedict.Add("Guyana", new Dictionary<int, int>());
            countrytimedict.Add("Mozambique", new Dictionary<int, int>());
            countrytimedict.Add("Classical Athens", new Dictionary<int, int>());
            countrytimedict.Add("Sudan", new Dictionary<int, int>());
            countrytimedict.Add("Republic of the Congo", new Dictionary<int, int>());
            countrytimedict.Add("Burkina Faso", new Dictionary<int, int>());
            countrytimedict.Add("Liechtenstein", new Dictionary<int, int>());
            countrytimedict.Add("Scotland", new Dictionary<int, int>());
            countrytimedict.Add("Mauritius", new Dictionary<int, int>());
            countrytimedict.Add("Tibet", new Dictionary<int, int>());
            countrytimedict.Add("Holy Roman Empire", new Dictionary<int, int>());
            countrytimedict.Add("Rwanda", new Dictionary<int, int>());
            countrytimedict.Add("Sierra Leone", new Dictionary<int, int>());
            countrytimedict.Add("Greenland", new Dictionary<int, int>());
            countrytimedict.Add("Benin", new Dictionary<int, int>());
            countrytimedict.Add("Malawi", new Dictionary<int, int>());
            countrytimedict.Add("Grand Duchy of Lithuania", new Dictionary<int, int>());
            countrytimedict.Add("Liberia", new Dictionary<int, int>());
            countrytimedict.Add("Chad", new Dictionary<int, int>());
            countrytimedict.Add("Timor-Leste", new Dictionary<int, int>());
            countrytimedict.Add("Tonga", new Dictionary<int, int>());
            countrytimedict.Add("The Bahamas", new Dictionary<int, int>());
            countrytimedict.Add("Niger", new Dictionary<int, int>());
            countrytimedict.Add("Samoa", new Dictionary<int, int>());
            countrytimedict.Add("Gabon", new Dictionary<int, int>());
            countrytimedict.Add("Togo", new Dictionary<int, int>());
            countrytimedict.Add("Sui dynasty", new Dictionary<int, int>());
            countrytimedict.Add("Eritrea", new Dictionary<int, int>());
            countrytimedict.Add("Cape Verde", new Dictionary<int, int>());
            countrytimedict.Add("Botswana", new Dictionary<int, int>());
            countrytimedict.Add("Austrian Empire", new Dictionary<int, int>());
            countrytimedict.Add("Monaco", new Dictionary<int, int>());
            countrytimedict.Add("Andorra", new Dictionary<int, int>());
            countrytimedict.Add("Liao dynasty", new Dictionary<int, int>());
            countrytimedict.Add("Republic of Venice", new Dictionary<int, int>());
            countrytimedict.Add("Laos", new Dictionary<int, int>());
            countrytimedict.Add("Burundi", new Dictionary<int, int>());
            countrytimedict.Add("Ancient Egypt", new Dictionary<int, int>());
            countrytimedict.Add("Mauritania", new Dictionary<int, int>());
            countrytimedict.Add("Kingdom of Sardinia (1720-1861)", new Dictionary<int, int>());
            countrytimedict.Add("Antigua and Barbuda", new Dictionary<int, int>());
            countrytimedict.Add("Guinea-Bissau", new Dictionary<int, int>());
            countrytimedict.Add("Crown of Castile", new Dictionary<int, int>());
            countrytimedict.Add("Dominica", new Dictionary<int, int>());
            countrytimedict.Add("Grenada", new Dictionary<int, int>());
            countrytimedict.Add("Oman", new Dictionary<int, int>());
            countrytimedict.Add("Saint Kitts and Nevis", new Dictionary<int, int>());
            countrytimedict.Add("Bulgarians", new Dictionary<int, int>());
            countrytimedict.Add("Central African Republic", new Dictionary<int, int>());
            countrytimedict.Add("Mongols", new Dictionary<int, int>());
            countrytimedict.Add("German", new Dictionary<int, int>());
            countrytimedict.Add("Saint Lucia", new Dictionary<int, int>());
            countrytimedict.Add("Serbs", new Dictionary<int, int>());
            countrytimedict.Add("Belize", new Dictionary<int, int>());
            countrytimedict.Add("Abbasid Caliphate", new Dictionary<int, int>());
            countrytimedict.Add("Equatorial Guinea", new Dictionary<int, int>());
            countrytimedict.Add("Jersey", new Dictionary<int, int>());
            countrytimedict.Add("Babylon", new Dictionary<int, int>());
            countrytimedict.Add("Maldives", new Dictionary<int, int>());
            countrytimedict.Add("Lesotho", new Dictionary<int, int>());
            countrytimedict.Add("Umayyad Caliphate", new Dictionary<int, int>());
            countrytimedict.Add("Second Polish Republic", new Dictionary<int, int>());
            countrytimedict.Add("Brunei", new Dictionary<int, int>());
            countrytimedict.Add("Solomon Islands", new Dictionary<int, int>());
            countrytimedict.Add("Saint Vincent and the Grenadines", new Dictionary<int, int>());
            countrytimedict.Add("Seychelles", new Dictionary<int, int>());
            countrytimedict.Add("Crown of Aragon", new Dictionary<int, int>());
            countrytimedict.Add("Isle of Man", new Dictionary<int, int>());
            countrytimedict.Add("Southern Netherlands", new Dictionary<int, int>());
            countrytimedict.Add("Bhutan", new Dictionary<int, int>());
            countrytimedict.Add("Southern Tang", new Dictionary<int, int>());
            countrytimedict.Add("Vanuatu", new Dictionary<int, int>());
            countrytimedict.Add("Eswatini", new Dictionary<int, int>());
            countrytimedict.Add("Assyrian Empire", new Dictionary<int, int>());
            countrytimedict.Add("São Tomé and Príncipe", new Dictionary<int, int>());
            countrytimedict.Add("Cook Islands", new Dictionary<int, int>());
            countrytimedict.Add("Liang dynasty", new Dictionary<int, int>());
            countrytimedict.Add("Cao Wei", new Dictionary<int, int>());
            countrytimedict.Add("Guernsey", new Dictionary<int, int>());
            countrytimedict.Add("Ukrainians", new Dictionary<int, int>());
            countrytimedict.Add("English", new Dictionary<int, int>());
            countrytimedict.Add("Portuguese Empire", new Dictionary<int, int>());
            countrytimedict.Add("Eastern Wu", new Dictionary<int, int>());
            countrytimedict.Add("Goryeo", new Dictionary<int, int>());
            countrytimedict.Add("Ancient Greek", new Dictionary<int, int>());
            countrytimedict.Add("Republic of Artsakh", new Dictionary<int, int>());
            countrytimedict.Add("Arabic", new Dictionary<int, int>());
            countrytimedict.Add("Mongol Empire", new Dictionary<int, int>());
            countrytimedict.Add("Nguyen dynasty", new Dictionary<int, int>());
            countrytimedict.Add("Comoros", new Dictionary<int, int>());
            countrytimedict.Add("Western Zhou Dynasty", new Dictionary<int, int>());
            countrytimedict.Add("South Sudan", new Dictionary<int, int>());
            countrytimedict.Add("Cisleithania", new Dictionary<int, int>());
            countrytimedict.Add("Czechs", new Dictionary<int, int>());
            countrytimedict.Add("Somaliland", new Dictionary<int, int>());
            countrytimedict.Add("Djibouti", new Dictionary<int, int>());
            countrytimedict.Add("Greeks", new Dictionary<int, int>());
            countrytimedict.Add("Navajo", new Dictionary<int, int>());
            countrytimedict.Add("Nauru", new Dictionary<int, int>());
            countrytimedict.Add("Tanganyika Territory", new Dictionary<int, int>());
            countrytimedict.Add("Sinhala people", new Dictionary<int, int>());
            countrytimedict.Add("Kiribati", new Dictionary<int, int>());
            countrytimedict.Add("Northern Zhou", new Dictionary<int, int>());
            countrytimedict.Add("Irish Free State", new Dictionary<int, int>());
            countrytimedict.Add("Tuvalu", new Dictionary<int, int>());
            countrytimedict.Add("Wuyue", new Dictionary<int, int>());
            countrytimedict.Add("Kingdom of Aragon", new Dictionary<int, int>());
            countrytimedict.Add("White Americans", new Dictionary<int, int>());
            countrytimedict.Add("French", new Dictionary<int, int>());
            countrytimedict.Add("Turkish Republic of Northern Cyprus", new Dictionary<int, int>());
            countrytimedict.Add("Habsburg Netherlands", new Dictionary<int, int>());
            countrytimedict.Add("Republic of Ragusa", new Dictionary<int, int>());
            countrytimedict.Add("Simbirsk Governorate", new Dictionary<int, int>());
            countrytimedict.Add("Federated States of Micronesia", new Dictionary<int, int>());
            countrytimedict.Add("Later Yan", new Dictionary<int, int>());
            countrytimedict.Add("Macedonia", new Dictionary<int, int>());
            countrytimedict.Add("Later Shu", new Dictionary<int, int>());
            countrytimedict.Add("Uyghur people", new Dictionary<int, int>());
            countrytimedict.Add("Former Yan", new Dictionary<int, int>());
            countrytimedict.Add("Union between Sweden and Norway", new Dictionary<int, int>());
            countrytimedict.Add("Grand Duchy of Moscow", new Dictionary<int, int>());
            countrytimedict.Add("Jewish people", new Dictionary<int, int>());
            countrytimedict.Add("Hopi people", new Dictionary<int, int>());
            countrytimedict.Add("Principality of Moldavia", new Dictionary<int, int>());
            countrytimedict.Add("Ancient Greece", new Dictionary<int, int>());
            countrytimedict.Add("Bengali", new Dictionary<int, int>());
            countrytimedict.Add("Palau", new Dictionary<int, int>());
            countrytimedict.Add("Kingdom of Naples", new Dictionary<int, int>());
            countrytimedict.Add("Bohemia", new Dictionary<int, int>());
            countrytimedict.Add("Livonia", new Dictionary<int, int>());
            countrytimedict.Add("Eastern Wei", new Dictionary<int, int>());
            countrytimedict.Add("Min", new Dictionary<int, int>());
            countrytimedict.Add("Kingdom of León", new Dictionary<int, int>());
            countrytimedict.Add("Kingdom of Navarre", new Dictionary<int, int>());
            countrytimedict.Add("Principality of Wallachia", new Dictionary<int, int>());
            countrytimedict.Add("Northern Qi", new Dictionary<int, int>());
            countrytimedict.Add("Demerara", new Dictionary<int, int>());
            countrytimedict.Add("Sioux", new Dictionary<int, int>());
            countrytimedict.Add("Wu guo", new Dictionary<int, int>());
            countrytimedict.Add("Kingdom of Hawaiʻi", new Dictionary<int, int>());
            countrytimedict.Add("Irish Republic", new Dictionary<int, int>());
            countrytimedict.Add("Crown of the Kingdom of Poland", new Dictionary<int, int>());
            countrytimedict.Add("Kingdom of Castile", new Dictionary<int, int>());
            countrytimedict.Add("Mughal Empire", new Dictionary<int, int>());
            countrytimedict.Add("Germans", new Dictionary<int, int>());
            countrytimedict.Add("Western Wei", new Dictionary<int, int>());
            countrytimedict.Add("Chen dynasty", new Dictionary<int, int>());
            countrytimedict.Add("Qi", new Dictionary<int, int>());
            countrytimedict.Add("Habsburg Monarchy", new Dictionary<int, int>());
            countrytimedict.Add("Indians", new Dictionary<int, int>());
            countrytimedict.Add("Spanish", new Dictionary<int, int>());
            countrytimedict.Add("British Cyprus", new Dictionary<int, int>());
            countrytimedict.Add("Rhodesia", new Dictionary<int, int>());
            countrytimedict.Add("Shu Han", new Dictionary<int, int>());
            countrytimedict.Add("Southern Qi", new Dictionary<int, int>());
            countrytimedict.Add("Papal States", new Dictionary<int, int>());
            countrytimedict.Add("Serbia and Montenegro", new Dictionary<int, int>());
            countrytimedict.Add("Ashikaga shogunate", new Dictionary<int, int>());
            countrytimedict.Add("Republic of Abkhazia", new Dictionary<int, int>());
            countrytimedict.Add("Odrysian kingdom", new Dictionary<int, int>());
            countrytimedict.Add("Later Tang Dynasty", new Dictionary<int, int>());
            countrytimedict.Add("Zaire", new Dictionary<int, int>());
            countrytimedict.Add("Angles", new Dictionary<int, int>());
            countrytimedict.Add("Jemez Puebloans", new Dictionary<int, int>());
            countrytimedict.Add("Zuni people", new Dictionary<int, int>());
            countrytimedict.Add("Kingdom of Wessex", new Dictionary<int, int>());
            return countrytimedict;
        }

        private List<int> nameprops()
        {
            List<int> proplist = new List<int>();
            proplist.Add(734);
            proplist.Add(735);
            proplist.Add(742);
            proplist.Add(1448);
            proplist.Add(1477);
            proplist.Add(1449);
            proplist.Add(1559);
            proplist.Add(1635);
            proplist.Add(1782);
            proplist.Add(1785);
            proplist.Add(1786);
            proplist.Add(1787);
            proplist.Add(1813);
            proplist.Add(1950);
            proplist.Add(2358);
            proplist.Add(2359);
            proplist.Add(2365);
            proplist.Add(2366);
            proplist.Add(2561);
            proplist.Add(2562);
            proplist.Add(5056);
            proplist.Add(6978);
            proplist.Add(8500);
            proplist.Add(8927);
            proplist.Add(9139);
            proplist.Add(172);
            proplist.Add(27);
            proplist.Add(19);
            proplist.Add(103);
            proplist.Add(569);
            proplist.Add(570);
            proplist.Add(21);
            return proplist;
        }

        public bool goodplace(Dictionary<int, List<string>> propvalues)
        {
            if (propvalues[19].Count > 0)
                return true;
            if (propvalues[27].Count > 0)
                return true;
            if (propvalues[172].Count > 0)
                return true;
            if (propvalues[103].Count > 0)
                return true;
            return false;
        }

        public List<string> firstnames(Dictionary<int, List<string>> propvalues)
        {
            List<string> nn = new List<string>();
            List<string> nameproplist = new List<string>() { "given name", "Roman praenomen" };
            foreach (string propname in nameproplist)
            foreach (string qss in propvalues[propdict[propname]])
                {
                    int q = util.qtoint(qss);
                    if (qlabeldict.ContainsKey(q))
                        nn.Add(qlabeldict[q]);
                    else
                        nn.Add(qss);
                }
            return nn;
        }

        public List<string> familynames(Dictionary<int, List<string>> propvalues)
        {
            List<string> nn = new List<string>();
            List<string> nameproplist = new List<string>() { "family name", "first family name in Portuguese name", "second family name in Spanish name", "Roman nomen gentilicium" , "Roman cognomen" , "Roman agnomen" };
            foreach (string propname in nameproplist)
                foreach (string qss in propvalues[propdict[propname]])
                {
                    int q = util.qtoint(qss);
                    if (qlabeldict.ContainsKey(q))
                        nn.Add(qlabeldict[q]);
                    else
                        nn.Add(qss);
                }
            return nn;
        }

        public Tuple<int,double,int> parseyearstring(string yearstring, Dictionary<int,int> yeardict, Dictionary<int, double> centuryfreq, bool decade)
        {
            Tuple<int, double, int> mxc;
            string[] years = yearstring.Trim(';').Split(';');
            int nysum = 0;
            double maxfreq = -1;
            int badcent = -999999;
            int maxfreqcent = badcent;

            foreach (string yy in years)
            {
                int ny = util.tryconvert(yy.Split(':')[1]);
                nysum += ny;
                int nc = util.tryconvert(yy.Split(':')[0]);
                if (decade)
                {
                    if (nc <= 0)
                        continue;
                    if (nc > 2100)
                        continue;
                }
                double freq = ny / get_centuryfreq(nc, centuryfreq,decade);
                if (freq > maxfreq)
                {
                    maxfreq = freq;
                    if (ny > 2)
                        maxfreqcent = nc;
                    else
                        maxfreqcent = badcent;
                }
                yeardict.Add(nc, ny);
            }

            mxc = new Tuple<int, double, int>(maxfreqcent, maxfreq, nysum);

            return mxc;
        }

        public double get_centuryfreq(int year,Dictionary<int,double> centuryfreq, bool decade)
        {
            int yy = (year / 100) * 100;
            double freq = centuryfreq[yy];
            if (decade && yy == 2000)
                freq *= 5;
            return freq;
        }

        public void read_namedicts(string folder, int fileorder,Dictionary<int,double> centuryfreq)
        {
            string fn1 = folder + "firstnames" + fileorder + ".txt";
            memo("reading " + fn1);
            int nlines = 0;
            using (StreamReader srq = new StreamReader(fn1))
            {
                while (!srq.EndOfStream)
                {
                    string line = srq.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        continue;
                    string[] words = line.Split('\t');

                    string label = words[0];
                    firstnamecenturydict.Add(label, new Dictionary<int, int>());
                    firstnamecountrydict.Add(label, new Dictionary<string, int>());

                    Tuple<int, double,int> mxc = parseyearstring(words[1], firstnamecenturydict[label], centuryfreq,false);
                    //string[] years = words[1].Trim(';').Split(';');
                    //int nysum = 0;
                    //double maxfreq = -1;
                    //int badcent = -999999;
                    //int maxfreqcent = badcent;

                    //foreach (string yy in years)
                    //{
                    //    int ny = util.tryconvert(yy.Split(':')[1]);
                    //    nysum += ny;
                    //    int nc = util.tryconvert(yy.Split(':')[0]);
                    //    double freq = ny / centuryfreq[nc];
                    //    if (freq > maxfreq)
                    //    {
                    //        maxfreq = freq;
                    //        if (ny > 2)
                    //            maxfreqcent = nc;
                    //        else
                    //            maxfreqcent = badcent;
                    //    }
                    //    firstnamecenturydict[label].Add(nc, ny);
                    //}

                    int badcent = -999999;
                    //if ((mxc.Item3 >= 100) & (mxc.Item1 != badcent))
                    //    memo(label + "\t" + mxc.Item1 + "\t" + mxc.Item2 + "\t" + mxc.Item2 * centuryfreq[mxc.Item1]);

                    firstnamedecadedict.Add(label, new Dictionary<int, int>());

                    mxc = parseyearstring(words[2], firstnamedecadedict[label], centuryfreq,true);

                    //if ((mxc.Item3 >= 100) & (mxc.Item1 != badcent))
                    //    memo(label + "\t" + mxc.Item1 + "\t" + mxc.Item2 + "\t" + mxc.Item2 * get_centuryfreq(mxc.Item1,centuryfreq,true));

                    if (words.Length > 3)
                    {
                        int nsum = 0;
                        int nmax = 0;
                        string maxplace = "";
                        for (int i=3;i<words.Length;i++)
                        {
                            int nn = util.tryconvert(words[i].Split(':')[1]);
                            string place = words[i].Split(':')[0];
                            if (firstnamecountrydict[label].ContainsKey(place))
                                firstnamecountrydict[label][place] += nn;
                            else
                                firstnamecountrydict[label].Add(place, nn);
                            nsum += nn;
                            if (nn > nmax)
                            {
                                nmax = nn;
                                maxplace = place;
                            }
                        }
                        if (!string.IsNullOrEmpty(maxplace))
                        {
                            if (nsum >= 3)
                            {
                                if (nsum == nmax)
                                {
                                    nameplacedict.Add(label, maxplace);
                                    //memo(label + "\t" + maxplace + "\t" + nsum);
                                }
                                else if (nsum >= 10)
                                {
                                    if (3 * nmax > 2 * nsum)
                                    {
                                        nameplacedict.Add(label, maxplace);
                                        //memo(label + "\t" + maxplace + "\t" + nsum);
                                    }
                                }
                                else if (nsum >= 6 && nsum - nmax == 1)
                                {
                                    nameplacedict.Add(label, maxplace);
                                    //memo(label + "\t" + maxplace + "\t" + nsum);
                                }
                            }
                        }
                    }
                    nlines++;
                    if (nlines % 10000 == 0)
                        memo("nlines=" + nlines);
                }

            }
            string fn2 = folder + "familynames" + fileorder + ".txt";
            memo("reading " + fn2);
            nlines = 0;
            using (StreamReader srf = new StreamReader(fn2))
            {
                while (!srf.EndOfStream)
                {
                    string line = srf.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        continue;
                    string[] words = line.Split('\t');

                    string label = words[0];
                    familynamecenturydict.Add(label, new Dictionary<int, int>());
                    familynamecountrydict.Add(label, new Dictionary<string, int>());
                    Tuple<int, double, int> mxc = parseyearstring(words[1], familynamecenturydict[label], centuryfreq, false);
                    //string[] years = words[1].Trim(';').Split(';');
                    //int nysum = 0;
                    //double maxfreq = -1;
                    //int badcent = -999999;
                    //int maxfreqcent = badcent;
                    //foreach (string yy in years)
                    //{
                    //    int ny = util.tryconvert(yy.Split(':')[1]);
                    //    nysum += ny;
                    //    int nc = util.tryconvert(yy.Split(':')[0]);
                    //    double freq = ny / centuryfreq[nc];
                    //    if (freq > maxfreq)
                    //    {
                    //        maxfreq = freq;
                    //        if (ny > 2)
                    //            maxfreqcent = nc;
                    //        else
                    //            maxfreqcent = badcent;
                    //    }
                    //    familynamecenturydict[label].Add(util.tryconvert(yy.Split(':')[0]), util.tryconvert(yy.Split(':')[1]));
                    //}
                    //if ((nysum >= 100) & (maxfreqcent != badcent))
                    //    memo(label + "\t" + maxfreqcent + "\t" + maxfreq + "\t" + maxfreq * centuryfreq[maxfreqcent]);

                    int badcent = -999999;
                    //if ((mxc.Item3 >= 100) & (mxc.Item1 != badcent))
                    //    memo(label + "\t" + mxc.Item1 + "\t" + mxc.Item2 + "\t" + mxc.Item2 * centuryfreq[mxc.Item1],false);

                    familynamedecadedict.Add(label, new Dictionary<int, int>());

                    mxc = parseyearstring(words[2], familynamedecadedict[label], centuryfreq, true);

                    if ((mxc.Item3 >= 100) & (mxc.Item1 != badcent))
                        memo(label + "\t" + mxc.Item1 + "\t" + mxc.Item2 + "\t" + mxc.Item2 * get_centuryfreq(mxc.Item1, centuryfreq, true));

                    if (words.Length > 3)
                    {
                        int nsum = 0;
                        int nmax = 0;
                        string maxplace = "";
                        for (int i = 3; i < words.Length; i++)
                        {
                            int nn = util.tryconvert(words[i].Split(':')[1]);
                            string place = words[i].Split(':')[0];
                            if (familynamecountrydict[label].ContainsKey(place))
                                familynamecountrydict[label][place] += nn;
                            else
                                familynamecountrydict[label].Add(place, nn);
                            nsum += nn;
                            if (nn > nmax)
                            {
                                nmax = nn;
                                maxplace = place;
                            }
                        }
                        if (!string.IsNullOrEmpty(maxplace))
                        {
                            if (nsum >= 3 && !nameplacedict.ContainsKey(label))
                            {
                                if (nsum == nmax)
                                {
                                    nameplacedict.Add(label, maxplace);
                                    //memo(label + "\t" + maxplace + "\t" + nsum);
                                }
                                else if (nsum >= 10)
                                {
                                    if (3 * nmax > 2 * nsum)
                                    {
                                        nameplacedict.Add(label, maxplace);
                                        //memo(label + "\t" + maxplace + "\t" + nsum);
                                    }
                                }
                                else if (nsum >= 6 && nsum - nmax == 1)
                                {
                                    nameplacedict.Add(label, maxplace);
                                    //memo(label + "\t" + maxplace + "\t" + nsum);
                                }
                            }
                        }
                    }
                    nlines++;
                    if (nlines % 10000 == 0)
                        memo("nlines=" + nlines);
                }

            }
        }

        public bool goodname(Dictionary<int, List<string>> propvalues)
        {
            //propdict.Add("family name", 734);
            //propdict.Add("given name", 735);
            //propdict.Add("pseudonym", 742);
            //propdict.Add("official name", 1448);
            //propdict.Add("birth name", 1477);
            //propdict.Add("nickname", 1449);
            //propdict.Add("name in native language", 1559);
            //propdict.Add("religious name", 1635);
            //propdict.Add("courtesy name", 1782);
            //propdict.Add("temple name", 1785);
            //propdict.Add("posthumous name", 1786);
            //propdict.Add("art-name", 1787);
            //propdict.Add("short name", 1813);
            //propdict.Add("second family name in Spanish name", 1950);
            //propdict.Add("Roman praenomen", 2358);
            //propdict.Add("Roman nomen gentilicium", 2359);
            //propdict.Add("Roman cognomen", 2365);
            //propdict.Add("Roman agnomen", 2366);
            //propdict.Add("name", 2561);
            //propdict.Add("married name", 2562);
            //propdict.Add("patronym or matronym", 5056);
            //propdict.Add("Scandinavian middle name", 6978);
            //propdict.Add("Vietnamese middle name", 8500);
            //propdict.Add("Kunya", 8927);
            //propdict.Add("first family name in Portuguese name", 9139);
            //propdict.Add("ethnic group", 172);
            //propdict.Add("place of birth", 19);
            //propdict.Add("native language", 103);

            List<int> roman = new List<int>() { 2358, 2359, 2365, 2366 };
            bool ok = false;
            if (propvalues[735].Count > 0 && (propvalues[734].Count > 0)||(propvalues[5056].Count>0))
                ok = true;
            else
            {
                int nrom = 0;
                foreach (int rr in roman)
                    if (propvalues[rr].Count > 0)
                        nrom++;
                if (nrom >= 2)
                    ok = true;
            }
            return ok;
        }

        public string place_from_alphabet(string alphabet)
        {
            switch (alphabet)
            {
                case "armenian":
                    return "Armenia";
                case "devanagari":
                case "telugu":
                case "tamil":
                    return "India";
                case "georgian":
                    return "Georgia";
                case "greek and coptic":
                    return "Greece";
                case "hebrew":
                    return "Israel";
                case "korean":
                    return "Korea";


            }

            return "";
        }
        public string bestplace(Dictionary<int, List<string>> propvalues)
        {
            string[] proporder = new string[] { "place of birth", "citizenship", "ethnic group", "native language" };

            foreach (string prop in proporder)
                if (propvalues[propdict[prop]].Count > 0)
                {
                    foreach (string qss in propvalues[propdict[prop]])
                    {
                        int qs = util.qtoint(qss);
                        if (qlabeldict.ContainsKey(qs))
                            return qlabeldict[qs];
                    }
                }

            foreach (string name in firstnames(propvalues))
            {
                if (nameplacedict.ContainsKey(name))
                    return nameplacedict[name];
                string script = util.get_alphabet(name);
                string place = place_from_alphabet(script);
                if (!String.IsNullOrEmpty(place))
                    return place;
            }
            foreach (string name in familynames(propvalues))
            {
                if (nameplacedict.ContainsKey(name))
                    return nameplacedict[name];
                string script = util.get_alphabet(name);
                string place = place_from_alphabet(script);
                if (!String.IsNullOrEmpty(place))
                    return place;
            }
            return "";
        }

        public int bestyear(Dictionary<int, List<string>> propvalues)
        {
            int year = -99999;
            foreach (string s in propvalues[propdict["birthdate"]])
            {
                year = util.tryconvert(s);
                if (year > 0 && year < 9999)
                    break;
            }
            if (year > 0)
            {
                if (year > BCoffset && year < 9999)
                    return -(year - BCoffset);
                else
                    return year;
            }
            foreach (string s in propvalues[propdict["deathdate"]])
            {
                year = util.tryconvert(s);
                if (year > 0 && year < 9999)
                    break;
            }
            if (year > BCoffset && year < 9999)
                return -(year - BCoffset)-60; //birth 60 years before death
            else
                return year;
            //return -99999;
        }

        public int bestcentury(Dictionary<int, List<string>> propvalues)
        {
            return bestperiod(100, propvalues);
        }

        public int bestdecade(Dictionary<int, List<string>> propvalues)
        {
            return bestperiod(10, propvalues);
        }

        public int bestperiod(int period,Dictionary<int, List<string>> propvalues)
        {
            int year = bestyear(propvalues);

            double cen = year / period;
            return period * (int)Math.Truncate(cen);
        }

        public void list_human_prop(List<int> proplist, List<string> langlist, string dumpfile, string outfile, int testprop, double minvalue, int nmax,bool statsonly)
        {
            int nlines = 0;
            int nfound = 0;
            int ngoodname = 0;
            int ngoodplace = 0;

            bool read_or_write_namedicts = true; //true means read, false means write


            Dictionary<string, Dictionary<int, int>> countrytimedict = fill_countrytimedict();
            Dictionary<int, double> centuryfreq = fill_centuryfreq();

            read_qlabels(outfolder + "qlabels-humans.txt");
            if (read_or_write_namedicts)
                read_namedicts(outfolder, 18, centuryfreq);
            
            hbookclass iwhist = new hbookclass("#iw links");
            
            Dictionary<int, hbookclass> prophistdict = new Dictionary<int, hbookclass>();
            Dictionary<string, hbookclass> countrynamehistdict = new Dictionary<string, hbookclass>();
            foreach (int iprop in proplist)
            {
                string sprop = intpropdict[iprop];
                prophistdict.Add(iprop, new hbookclass(sprop+"\t========================================="));
            }

            hbookclass countryhist = new hbookclass("Country");
            hbookclass centuryhist = new hbookclass("Century");
            hbookclass decadehist = new hbookclass("Decade");
            hbookclass scripthist = new hbookclass("Scripts used in names");
            hbookclass country_timelesshist = new hbookclass("Country for people without century");

            string fn = util.unusedfilename(outfile);
            Console.WriteLine("Starting list_human_prop to "+fn);
            using (StreamWriter sw = new StreamWriter(fn))
            using (StreamReader sr = new StreamReader(dumpfile))
            {
                if (!statsonly)
                {
                    StringBuilder sbhead = new StringBuilder("Id\tLabel\t#IW\tCountry");
                    foreach (int ip in proplist)
                        sbhead.Append("\t" + intpropdict[ip]);
                    sw.WriteLine(sbhead.ToString());
                }
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim(trimchars);
                    if (String.IsNullOrEmpty(s))
                        continue;
                    nlines++;

                    JObject wd = JObject.Parse(s);

                    Dictionary<int, List<string>> propvalues = new Dictionary<int, List<string>>();
                    Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();

                    foreach (int prop in proplist)
                    {
                        propvalues.Add(prop, new List<string>());
                        foreach (string ccc in get_claims(wd, prop, dictObj))
                        {
                            propvalues[prop].Add(ccc);
                        }
                    }

                    if (!goodname(propvalues))
                        parse_name(get_best_label(wd), propvalues);

                    if (goodname(propvalues))
                        ngoodname++;
                    if (goodplace(propvalues))
                        ngoodplace++;

                    string country = bestplace(propvalues);
                    countryhist.Add(country);
                    int century = bestcentury(propvalues);
                    int decade = bestdecade(propvalues);
                    if (decade < 1700)
                        decade = 0;
                    centuryhist.Add(century);
                    decadehist.Add(decade);
                    if (century < -10000)
                        country_timelesshist.Add(country);
                    else
                    {
                        if (countrytimedict.ContainsKey(country))
                        {
                            if (countrytimedict[country].ContainsKey(century))
                                countrytimedict[country][century]++;
                            else
                                countrytimedict[country].Add(century, 1);
                        }
                    }



                    foreach (string fs in firstnames(propvalues))
                    {
                        if (!firstnamecountrydict.ContainsKey(fs))
                        {
                            firstnamecountrydict.Add(fs, new Dictionary<string, int>());
                            firstnamecenturydict.Add(fs, new Dictionary<int, int>());
                        }
                        if (!firstnamedecadedict.ContainsKey(fs))
                        { 
                            firstnamedecadedict.Add(fs, new Dictionary<int, int>());
                        }
                        if (!firstnamecountrydict[fs].ContainsKey(country))
                            firstnamecountrydict[fs].Add(country, 1);
                        else
                            firstnamecountrydict[fs][country]++;
                        if (!firstnamecenturydict[fs].ContainsKey(century))
                            firstnamecenturydict[fs].Add(century, 1);
                        else
                            firstnamecenturydict[fs][century]++;
                        if (!firstnamedecadedict[fs].ContainsKey(decade))
                            firstnamedecadedict[fs].Add(decade, 1);
                        else
                            firstnamedecadedict[fs][decade]++;

                        scripthist.Add(util.get_alphabet(fs));
                    }

                    foreach (string fs in familynames(propvalues))
                    {
                        if (!familynamecountrydict.ContainsKey(fs))
                        {
                            familynamecountrydict.Add(fs, new Dictionary<string, int>());
                            familynamecenturydict.Add(fs, new Dictionary<int, int>());
                        }
                        if (!familynamedecadedict.ContainsKey(fs))
                        {
                            familynamedecadedict.Add(fs, new Dictionary<int, int>());
                        }
                        if (!familynamecountrydict[fs].ContainsKey(country))
                            familynamecountrydict[fs].Add(country, 1);
                        else
                            familynamecountrydict[fs][country]++;
                        if (!familynamecenturydict[fs].ContainsKey(century))
                            familynamecenturydict[fs].Add(century, 1);
                        else
                            familynamecenturydict[fs][century]++;
                        if (!familynamedecadedict[fs].ContainsKey(decade))
                            familynamedecadedict[fs].Add(decade, 1);
                        else
                            familynamedecadedict[fs][decade]++;
                        scripthist.Add(util.get_alphabet(fs));
                    }


                    if (testprop > 0)
                    {
                        bool ok = false;
                        if (propvalues.ContainsKey(testprop))
                        {
                            foreach (string stest in propvalues[testprop])
                                if (util.tryconvertdouble(stest) > minvalue)
                                    ok = true;
                        }
                        if (!ok)
                            continue;
                    }

                    nfound++;
                    StringBuilder sb = new StringBuilder(wd["id"].ToString() + "\t" + get_best_label(wd));
                    //sw.Write();


                    Dictionary<string, object> sitedict = wd["sitelinks"].ToObject<Dictionary<string, object>>();
                    //foreach (string sl in sitedict.Keys)
                    //    Console.WriteLine(sl);

                    if (langlist != null)
                    {
                        if (!statsonly)
                        {
                            sb.Append("\tlanglist");
                            sb.Append("\t" + langlist.Count);
                            foreach (string lang in langlist)
                            {
                                sb.Append("\t");
                                if (sitedict.ContainsKey(lang))
                                    sb.Append(wd["sitelinks"][lang]["title"]);
                            }
                        }
                    }
                    else
                    {
                        //sw.Write("\tsitedict");
                        if (!statsonly)
                            sb.Append("\t" + sitedict.Count);
                        iwhist.Add(sitedict.Count);
                        //foreach (string lang in sitedict.Keys)
                        //{
                        //    sb.Append("\t");
                        //    if (sitedict.ContainsKey(lang))
                        //        sb.Append(lang.Replace("wiki", "") + ":" + wd["sitelinks"][lang]["title"]);
                        //}
                    }


                    if (!statsonly)
                    {
                        sb.Append("\t");
                        foreach (string ccc in get_claims(wd, propdict["country"], dictObj))
                        {
                            if (countrynamedict.ContainsKey(ccc))
                            {
                                sb.Append(countrynamedict[ccc]);
                                break;
                            }
                        }
                    }


                    foreach (int prop in proplist)
                    {
                        if (!statsonly)
                            sb.Append("\t");
                        if (propvalues.ContainsKey(prop))
                        {
                            prophistdict[prop].Add(propvalues[prop].Count);
                            foreach (string sprop in propvalues[prop])
                            {
                                if (!statsonly)
                                    sb.Append(sprop + " ");
                                int q = util.qtoint(sprop);
                                if (qlabeldict.ContainsKey(q))
                                    prophistdict[prop].Add(qlabeldict[q]);
                                else
                                    prophistdict[prop].Add(sprop);
                            }

                        }
                    }
                    if (!statsonly)
                        sw.WriteLine(sb.ToString());

                    if (nlines % 1000 == 0)
                    {
                        memo(nlines + " ============================= " + wd["id"] + " " + get_best_label(wd));// +" "+wd.labels["en"].value);
                    }

                    if (nmax > 0)
                    {
                        if (nlines > nmax)
                            break;
                    }
                }

                sw.WriteLine(iwhist.GetIHist());
                foreach (int prop in proplist)
                {
                    sw.WriteLine(prophistdict[prop].GetIHist());
                    sw.WriteLine(prophistdict[prop].GetSHist());
                }
                sw.WriteLine(countryhist.GetSHist());
                sw.WriteLine(centuryhist.GetIHist());
                sw.WriteLine(decadehist.GetIHist());
                sw.WriteLine(country_timelesshist.GetSHist());
                sw.WriteLine(scripthist.GetSHist());

                //StringBuilder sbh = new StringBuilder();
                //for (int ic = -4000; ic < 2100; ic += 100)
                //    sbh.Append("\t" + ic);
                //memo(sbh.ToString());
                //foreach (string cc in countrytimedict.Keys)
                //{
                //    StringBuilder sb = new StringBuilder(cc);
                //    for (int ic = -4000; ic < 2100; ic += 100)
                //    {
                //        if (countrytimedict[cc].ContainsKey(ic))
                //            sb.Append("\t" + countrytimedict[cc][ic]/centuryfreq[ic]);
                //        else
                //            sb.Append("\t0");
                //    }
                //    memo(sb.ToString());
                //}

                sw.WriteLine("ngoodname\t" + ngoodname);
                sw.WriteLine("ngoodplace\t" + ngoodplace);
                sw.WriteLine("nfound\t" + nfound);


            }

            if (!read_or_write_namedicts)
            {
                string firstfile = util.unusedfilename(outfolder + "firstnames.txt");
                memo("Writing " + firstfile);
                using (StreamWriter sw1 = new StreamWriter(firstfile))
                {
                    foreach (string fs in firstnamecenturydict.Keys)
                    {
                        StringBuilder sb = new StringBuilder(fs + "\t");
                        foreach (int cent in firstnamecenturydict[fs].Keys)
                        {
                            sb.Append(cent + ":" + firstnamecenturydict[fs][cent] + ";");
                        }
                        sb.Append("\t");
                        if (firstnamedecadedict.ContainsKey(fs))
                        {
                            foreach (int cent in firstnamedecadedict[fs].Keys)
                            {
                                sb.Append(cent + ":" + firstnamedecadedict[fs][cent] + ";");
                            }
                        }
                        foreach (string country in firstnamecountrydict[fs].Keys)
                        {
                            sb.Append("\t" + country + ":" + firstnamecountrydict[fs][country]);
                        }
                        sw1.WriteLine(sb.ToString());
                    }
                }
                string familyfile = util.unusedfilename(outfolder + "familynames.txt");
                memo("Writing " + familyfile);
                using (StreamWriter sw2 = new StreamWriter(familyfile))
                {
                    foreach (string fs in familynamecenturydict.Keys)
                    {
                        StringBuilder sb = new StringBuilder(fs + "\t");
                        foreach (int cent in familynamecenturydict[fs].Keys)
                        {
                            sb.Append(cent + ":" + familynamecenturydict[fs][cent] + ";");
                        }
                        sb.Append("\t");
                        if (familynamedecadedict.ContainsKey(fs))
                        {
                            foreach (int cent in familynamedecadedict[fs].Keys)
                            {
                                sb.Append(cent + ":" + familynamedecadedict[fs][cent] + ";");
                            }
                        }
                        foreach (string country in familynamecountrydict[fs].Keys)
                        {
                            sb.Append("\t" + country + ":" + familynamecountrydict[fs][country]);
                        }
                        sw2.WriteLine(sb.ToString());
                    }
                }
            }

            Console.WriteLine("End of list_human_prop. Found " + nfound);
        }

        private void parse_name(string label, Dictionary<int, List<string>> propvalues)
        {
            List<string> fn = firstnames(propvalues);
            List<string> fm = familynames(propvalues);
            List<char> badchars = new List<char>() { '=','(', '[', '?', '{', '.', '@', '_', '-', '/', '“', '"','1','2','3','4','5','6','7','8','9','0' };
            string[] ss = label.Split();
            if (ss.Length == 1)
            {
                if (fn.Count + fm.Count == 0) //assume single name is first name
                    if (util.is_latin(label)) //other alphabets may not split properly
                        propvalues[propdict["given name"]].Add(label);
                return;
            }
            int nfound = 0;
            string unfound = "";
            int ngood = 0;

            foreach (string sss in ss)
            {
                string s = sss.Trim('.');
                if (s.Length < 2)
                    continue;
                if (badchars.Contains(s[0]))
                    continue;
                ngood++;

                if (fn.Contains(s))
                {
                    nfound++;
                    continue;
                }
                if (fm.Contains(s))
                {
                    nfound++;
                    continue;
                }
                if (firstnamecountrydict.ContainsKey(s))
                {
                    if (familynamecountrydict.ContainsKey(s))
                    {
                        int nf = firstnamecountrydict[s].Values.ToList().Sum();
                        int nm = familynamecountrydict[s].Values.ToList().Sum();
                        if (nf > 3 * nm)
                        {
                            nfound++;
                            propvalues[propdict["given name"]].Add(s);
                        }
                        else if (nm > 3 * nf)
                        {
                            propvalues[propdict["family name"]].Add(s);
                            nfound++;
                        }
                        else
                            unfound = s;
                    }
                    else
                    {
                        nfound++;
                        propvalues[propdict["given name"]].Add(s);
                    }
                }
                else if (familynamecountrydict.ContainsKey(s))
                {
                    nfound++;
                    propvalues[propdict["family name"]].Add(s);
                }
                else
                    unfound = s;
            }
            if (ngood - nfound == 1)
            {
                if (!String.IsNullOrEmpty(unfound))
                {
                    if (fn.Count > 0)
                    {
                        if (fm.Count == 0)
                        {
                            propvalues[propdict["family name"]].Add(unfound);
                        }
                    }
                    else if (fm.Count > 0)
                    {
                        propvalues[propdict["given name"]].Add(unfound);
                    }
                }
            }
        }

        private void namebutton_Click(object sender, EventArgs e)
        {
            string dumpfile = outfolder + "wikidata-dump-human.json";
            string outfile = outfolder + "humanprops.txt";

            DateTime starttime = DateTime.Now;
            list_human_prop(nameprops(), null, dumpfile, outfile, 0, 0, util.tryconvert(TB_break.Text),true);
            memo("Time elapsed = " + (DateTime.Now - starttime).ToString());
        }

        private void read_qlabels(string fn)
        {
            int nlines = 0;
            using (StreamReader srq = new StreamReader(fn))
            {
                while (!srq.EndOfStream)
                {
                    string line = srq.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        continue;
                    string[] words = line.Split('\t');

                    int q = util.qtoint(words[0]);
                    string label = words[1];
                    if (words.Length > 2)
                    {
                        if (!string.IsNullOrEmpty(words[2].Trim()))
                        {
                            int q2 = util.qtoint(words[2]);
                            if (q2 != q)
                            {
                                if (qlabeldict.ContainsKey(q2))
                                    label = qlabeldict[q2];
                                else
                                    label = words[2];
                            }
                        }
                    }
                    qlabeldict.Add(q, label);
                    nlines++;
                    if (nlines % 10000 == 0)
                        memo("nlines=" + nlines);
                }

            }

            memo("2nd pass...");

            List<int> dummy = qlabeldict.Keys.ToList();
            foreach (int q in dummy)
            {
                if (qlabeldict[q][0] == 'Q')
                {
                    int q2 = util.qtoint(qlabeldict[q]);
                    if (qlabeldict.ContainsKey(q2))
                        qlabeldict[q] = qlabeldict[q2];
                    else if (q2 > 0)
                        memo("Q" + q + "\t" + qlabeldict[q]);
                }
            }
            memo("... done reading qlabels");
        }



        private void Qlistbutton_Click(object sender, EventArgs e)
        {
            Dictionary<string, bool> qdict = new Dictionary<string, bool>(); //key is Qxxx, value is true if a place
            string fnq = outfolder + "qlist-humans.txt";
            int nq = 0;
            using (StreamReader srq = new StreamReader(fnq))
            {
                while (!srq.EndOfStream)
                {
                    string line = srq.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        continue;
                    string[] words = line.Split('\t');
                    bool place = false;
                    if (words.Length > 2 && words[2] == "country")
                        place = true;
                    if (qdict.ContainsKey(words[0]))
                    {
                        if (place)
                            qdict[words[0]] = true;
                    }
                    else
                        qdict.Add(words[0], place);
                    nq++;
                    if (nq % 10000 == 0)
                        memo("nq=" + nq);
                }
            }
            memo("# Q = " + qdict.Count);

            int nfound = 0;
            int nlines = 0;
            int nmax = util.tryconvert(TB_break.Text);
            int countryprop = propdict["country"];

            string dumpfile = outfolder + "wikidata-dump.json";
            string outfile = outfolder + "qlabels.txt";
            string fn = util.unusedfilename(outfile);
            memo("Starting list_human_prop to " + fn);
            using (StreamWriter sw = new StreamWriter(fn))
            using (StreamReader sr = new StreamReader(dumpfile))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim(trimchars);
                    if (String.IsNullOrEmpty(s))
                        continue;
                    nlines++;


                    JObject wd = JObject.Parse(s);
                    string qid = wd["id"].ToString();

                    if (nlines % 1000 == 0)
                    {
                        memo(nlines + " ============================= " + qid + " " + get_best_label(wd));// +" "+wd.labels["en"].value);
                    }

                    if (nmax > 0)
                    {
                        if (nlines > nmax)
                            break;
                    }

                    if (!qdict.ContainsKey(qid))
                        continue;

                    nfound++;

                    string qlabel = get_best_label(wd);
                    string qcountry = "";

                    if (qdict[qid])
                    {
                        qcountry = qid;
                        Dictionary<string, object> dictObj = wd["claims"].ToObject<Dictionary<string, object>>();
                        foreach (string ccc in get_claims(wd, countryprop, dictObj))
                        {
                            qcountry = ccc;
                            break;
                        }
                    }

                    sw.WriteLine(qid + "\t" + qlabel + "\t" + qcountry);

                }

            }


            Console.WriteLine("End of qbutton. Found " + nfound);

        }
    }
}
