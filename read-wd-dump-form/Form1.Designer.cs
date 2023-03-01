namespace read_wd_dump_form
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.quitbutton = new System.Windows.Forms.Button();
            this.Subdump_button = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.CB_props = new System.Windows.Forms.CheckedListBox();
            this.CB_categories = new System.Windows.Forms.CheckedListBox();
            this.subclassbutton = new System.Windows.Forms.Button();
            this.iwstatbutton = new System.Windows.Forms.Button();
            this.iwtablebutton = new System.Windows.Forms.Button();
            this.TB_miniw = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TB_maxlevels = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Normalizebutton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.treebutton = new System.Windows.Forms.Button();
            this.Comparebutton = new System.Windows.Forms.Button();
            this.neighborjoinbutton = new System.Windows.Forms.Button();
            this.testbutton = new System.Windows.Forms.Button();
            this.PCAbutton = new System.Windows.Forms.Button();
            this.nexusbutton = new System.Windows.Forms.Button();
            this.logtransformbutton = new System.Windows.Forms.Button();
            this.TB_minrange = new System.Windows.Forms.TextBox();
            this.TB_maxrange = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.CB_normalize = new System.Windows.Forms.CheckBox();
            this.randombutton = new System.Windows.Forms.Button();
            this.Kmeansbutton = new System.Windows.Forms.Button();
            this.TBcluster = new System.Windows.Forms.TrackBar();
            this.meanshiftbutton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.TBmeanshift = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.TBlargelimit = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.TBmarkersize = new System.Windows.Forms.TrackBar();
            this.domaincoverbutton = new System.Windows.Forms.Button();
            this.CBskip1000 = new System.Windows.Forms.CheckBox();
            this.TBpcachartmax = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.namebutton = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.TB_break = new System.Windows.Forms.TextBox();
            this.Qlistbutton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.TBcluster)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TBmarkersize)).BeginInit();
            this.SuspendLayout();
            // 
            // quitbutton
            // 
            this.quitbutton.Location = new System.Drawing.Point(466, 538);
            this.quitbutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.quitbutton.Name = "quitbutton";
            this.quitbutton.Size = new System.Drawing.Size(102, 75);
            this.quitbutton.TabIndex = 0;
            this.quitbutton.Text = "Quit";
            this.quitbutton.UseVisualStyleBackColor = true;
            this.quitbutton.Click += new System.EventHandler(this.quitbutton_Click);
            // 
            // Subdump_button
            // 
            this.Subdump_button.Location = new System.Drawing.Point(336, 11);
            this.Subdump_button.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Subdump_button.Name = "Subdump_button";
            this.Subdump_button.Size = new System.Drawing.Size(102, 39);
            this.Subdump_button.TabIndex = 2;
            this.Subdump_button.Text = "Make subdump";
            this.Subdump_button.UseVisualStyleBackColor = true;
            this.Subdump_button.Click += new System.EventHandler(this.Subdump_button_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(10, 11);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(322, 365);
            this.richTextBox1.TabIndex = 3;
            this.richTextBox1.Text = "";
            // 
            // CB_props
            // 
            this.CB_props.FormattingEnabled = true;
            this.CB_props.Location = new System.Drawing.Point(166, 524);
            this.CB_props.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.CB_props.Name = "CB_props";
            this.CB_props.Size = new System.Drawing.Size(126, 64);
            this.CB_props.TabIndex = 4;
            // 
            // CB_categories
            // 
            this.CB_categories.FormattingEnabled = true;
            this.CB_categories.Location = new System.Drawing.Point(35, 524);
            this.CB_categories.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.CB_categories.Name = "CB_categories";
            this.CB_categories.Size = new System.Drawing.Size(114, 64);
            this.CB_categories.TabIndex = 1;
            // 
            // subclassbutton
            // 
            this.subclassbutton.Location = new System.Drawing.Point(336, 54);
            this.subclassbutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.subclassbutton.Name = "subclassbutton";
            this.subclassbutton.Size = new System.Drawing.Size(102, 38);
            this.subclassbutton.TabIndex = 5;
            this.subclassbutton.Text = "Print subclass tree";
            this.subclassbutton.UseVisualStyleBackColor = true;
            this.subclassbutton.Click += new System.EventHandler(this.subclassbutton_Click);
            // 
            // iwstatbutton
            // 
            this.iwstatbutton.Location = new System.Drawing.Point(336, 97);
            this.iwstatbutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.iwstatbutton.Name = "iwstatbutton";
            this.iwstatbutton.Size = new System.Drawing.Size(102, 41);
            this.iwstatbutton.TabIndex = 6;
            this.iwstatbutton.Text = "Get IW statistics";
            this.iwstatbutton.UseVisualStyleBackColor = true;
            this.iwstatbutton.Click += new System.EventHandler(this.iwstatbutton_Click);
            // 
            // iwtablebutton
            // 
            this.iwtablebutton.Enabled = false;
            this.iwtablebutton.Location = new System.Drawing.Point(336, 141);
            this.iwtablebutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.iwtablebutton.Name = "iwtablebutton";
            this.iwtablebutton.Size = new System.Drawing.Size(102, 44);
            this.iwtablebutton.TabIndex = 7;
            this.iwtablebutton.Text = "Make IW table";
            this.iwtablebutton.UseVisualStyleBackColor = true;
            this.iwtablebutton.Click += new System.EventHandler(this.iwtablebutton_Click);
            // 
            // TB_miniw
            // 
            this.TB_miniw.Location = new System.Drawing.Point(106, 396);
            this.TB_miniw.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TB_miniw.Name = "TB_miniw";
            this.TB_miniw.Size = new System.Drawing.Size(44, 20);
            this.TB_miniw.TabIndex = 8;
            this.TB_miniw.Text = "1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 396);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Minimum #iw:";
            // 
            // TB_maxlevels
            // 
            this.TB_maxlevels.Location = new System.Drawing.Point(252, 396);
            this.TB_maxlevels.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TB_maxlevels.Name = "TB_maxlevels";
            this.TB_maxlevels.Size = new System.Drawing.Size(47, 20);
            this.TB_maxlevels.TabIndex = 10;
            this.TB_maxlevels.Text = "999";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(173, 398);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Max tree levels:";
            // 
            // Normalizebutton
            // 
            this.Normalizebutton.Location = new System.Drawing.Point(336, 189);
            this.Normalizebutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Normalizebutton.Name = "Normalizebutton";
            this.Normalizebutton.Size = new System.Drawing.Size(102, 57);
            this.Normalizebutton.TabIndex = 12;
            this.Normalizebutton.Text = "Make normalization matrix";
            this.Normalizebutton.UseVisualStyleBackColor = true;
            this.Normalizebutton.Click += new System.EventHandler(this.Normalizebutton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // treebutton
            // 
            this.treebutton.Location = new System.Drawing.Point(335, 251);
            this.treebutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.treebutton.Name = "treebutton";
            this.treebutton.Size = new System.Drawing.Size(103, 55);
            this.treebutton.TabIndex = 13;
            this.treebutton.Text = "Make semantic distance matrix";
            this.treebutton.UseVisualStyleBackColor = true;
            this.treebutton.Click += new System.EventHandler(this.treebutton_Click);
            // 
            // Comparebutton
            // 
            this.Comparebutton.Location = new System.Drawing.Point(336, 310);
            this.Comparebutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Comparebutton.Name = "Comparebutton";
            this.Comparebutton.Size = new System.Drawing.Size(102, 45);
            this.Comparebutton.TabIndex = 14;
            this.Comparebutton.Text = "Compare distance matrices";
            this.Comparebutton.UseVisualStyleBackColor = true;
            this.Comparebutton.Click += new System.EventHandler(this.Comparebutton_Click);
            // 
            // neighborjoinbutton
            // 
            this.neighborjoinbutton.Location = new System.Drawing.Point(336, 359);
            this.neighborjoinbutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.neighborjoinbutton.Name = "neighborjoinbutton";
            this.neighborjoinbutton.Size = new System.Drawing.Size(102, 46);
            this.neighborjoinbutton.TabIndex = 15;
            this.neighborjoinbutton.Text = "Neighbor joining";
            this.neighborjoinbutton.UseVisualStyleBackColor = true;
            this.neighborjoinbutton.Click += new System.EventHandler(this.neighborjoinbutton_Click);
            // 
            // testbutton
            // 
            this.testbutton.Location = new System.Drawing.Point(489, 455);
            this.testbutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.testbutton.Name = "testbutton";
            this.testbutton.Size = new System.Drawing.Size(56, 19);
            this.testbutton.TabIndex = 16;
            this.testbutton.Text = "PCA test";
            this.testbutton.UseVisualStyleBackColor = true;
            this.testbutton.Click += new System.EventHandler(this.testbutton_Click);
            // 
            // PCAbutton
            // 
            this.PCAbutton.Location = new System.Drawing.Point(335, 410);
            this.PCAbutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.PCAbutton.Name = "PCAbutton";
            this.PCAbutton.Size = new System.Drawing.Size(103, 31);
            this.PCAbutton.TabIndex = 17;
            this.PCAbutton.Text = "PCA";
            this.PCAbutton.UseVisualStyleBackColor = true;
            this.PCAbutton.Click += new System.EventHandler(this.PCAbutton_Click);
            // 
            // nexusbutton
            // 
            this.nexusbutton.Location = new System.Drawing.Point(336, 446);
            this.nexusbutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.nexusbutton.Name = "nexusbutton";
            this.nexusbutton.Size = new System.Drawing.Size(102, 37);
            this.nexusbutton.TabIndex = 18;
            this.nexusbutton.Text = "Make NEXUS file for SplitsTree";
            this.nexusbutton.UseVisualStyleBackColor = true;
            this.nexusbutton.Click += new System.EventHandler(this.nexusbutton_Click);
            // 
            // logtransformbutton
            // 
            this.logtransformbutton.Location = new System.Drawing.Point(443, 11);
            this.logtransformbutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.logtransformbutton.Name = "logtransformbutton";
            this.logtransformbutton.Size = new System.Drawing.Size(102, 39);
            this.logtransformbutton.TabIndex = 19;
            this.logtransformbutton.Text = "Log-transform distance matrix";
            this.logtransformbutton.UseVisualStyleBackColor = true;
            this.logtransformbutton.Click += new System.EventHandler(this.logtransformbutton_Click);
            // 
            // TB_minrange
            // 
            this.TB_minrange.Location = new System.Drawing.Point(145, 427);
            this.TB_minrange.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TB_minrange.Name = "TB_minrange";
            this.TB_minrange.Size = new System.Drawing.Size(51, 20);
            this.TB_minrange.TabIndex = 20;
            this.TB_minrange.Text = "1";
            // 
            // TB_maxrange
            // 
            this.TB_maxrange.Location = new System.Drawing.Point(218, 427);
            this.TB_maxrange.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TB_maxrange.Name = "TB_maxrange";
            this.TB_maxrange.Size = new System.Drawing.Size(76, 20);
            this.TB_maxrange.TabIndex = 21;
            this.TB_maxrange.Text = "999999";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 427);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "Range of word counts:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(200, 430);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(13, 13);
            this.label4.TabIndex = 23;
            this.label4.Text = "--";
            // 
            // CB_normalize
            // 
            this.CB_normalize.AutoSize = true;
            this.CB_normalize.Checked = true;
            this.CB_normalize.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CB_normalize.Location = new System.Drawing.Point(10, 457);
            this.CB_normalize.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.CB_normalize.Name = "CB_normalize";
            this.CB_normalize.Size = new System.Drawing.Size(132, 17);
            this.CB_normalize.TabIndex = 24;
            this.CB_normalize.Text = "Normalize domaintable";
            this.CB_normalize.UseVisualStyleBackColor = true;
            // 
            // randombutton
            // 
            this.randombutton.Location = new System.Drawing.Point(460, 271);
            this.randombutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.randombutton.Name = "randombutton";
            this.randombutton.Size = new System.Drawing.Size(94, 36);
            this.randombutton.TabIndex = 25;
            this.randombutton.Text = "Make random distance matrix";
            this.randombutton.UseVisualStyleBackColor = true;
            this.randombutton.Click += new System.EventHandler(this.randombutton_Click);
            // 
            // Kmeansbutton
            // 
            this.Kmeansbutton.Location = new System.Drawing.Point(336, 488);
            this.Kmeansbutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Kmeansbutton.Name = "Kmeansbutton";
            this.Kmeansbutton.Size = new System.Drawing.Size(102, 42);
            this.Kmeansbutton.TabIndex = 26;
            this.Kmeansbutton.Text = "K-means clustering";
            this.Kmeansbutton.UseVisualStyleBackColor = true;
            this.Kmeansbutton.Click += new System.EventHandler(this.Kmeansbutton_Click);
            // 
            // TBcluster
            // 
            this.TBcluster.Location = new System.Drawing.Point(165, 474);
            this.TBcluster.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TBcluster.Minimum = 2;
            this.TBcluster.Name = "TBcluster";
            this.TBcluster.Size = new System.Drawing.Size(133, 45);
            this.TBcluster.TabIndex = 27;
            this.TBcluster.Value = 2;
            this.TBcluster.Scroll += new System.EventHandler(this.TBcluster_Scroll);
            // 
            // meanshiftbutton
            // 
            this.meanshiftbutton.Location = new System.Drawing.Point(335, 538);
            this.meanshiftbutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.meanshiftbutton.Name = "meanshiftbutton";
            this.meanshiftbutton.Size = new System.Drawing.Size(103, 38);
            this.meanshiftbutton.TabIndex = 28;
            this.meanshiftbutton.Text = "Mean Shift clustering";
            this.meanshiftbutton.UseVisualStyleBackColor = true;
            this.meanshiftbutton.Click += new System.EventHandler(this.meanshiftbutton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(181, 460);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 13);
            this.label5.TabIndex = 29;
            this.label5.Text = "K-means # clusters";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 619);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(108, 13);
            this.label6.TabIndex = 30;
            this.label6.Text = "Mean-shift bandwidth";
            // 
            // TBmeanshift
            // 
            this.TBmeanshift.Location = new System.Drawing.Point(120, 617);
            this.TBmeanshift.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TBmeanshift.Name = "TBmeanshift";
            this.TBmeanshift.Size = new System.Drawing.Size(76, 20);
            this.TBmeanshift.TabIndex = 31;
            this.TBmeanshift.Text = "1.0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(1, 637);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(169, 13);
            this.label7.TabIndex = 32;
            this.label7.Text = "\"Large language\" coverage cutoff";
            // 
            // TBlargelimit
            // 
            this.TBlargelimit.Location = new System.Drawing.Point(170, 635);
            this.TBlargelimit.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TBlargelimit.Name = "TBlargelimit";
            this.TBlargelimit.Size = new System.Drawing.Size(76, 20);
            this.TBlargelimit.TabIndex = 33;
            this.TBlargelimit.Text = "0.02";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(18, 682);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 13);
            this.label8.TabIndex = 34;
            this.label8.Text = "Marker size";
            // 
            // TBmarkersize
            // 
            this.TBmarkersize.Location = new System.Drawing.Point(14, 706);
            this.TBmarkersize.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TBmarkersize.Minimum = 1;
            this.TBmarkersize.Name = "TBmarkersize";
            this.TBmarkersize.Size = new System.Drawing.Size(102, 45);
            this.TBmarkersize.TabIndex = 35;
            this.TBmarkersize.Value = 1;
            // 
            // domaincoverbutton
            // 
            this.domaincoverbutton.Location = new System.Drawing.Point(336, 580);
            this.domaincoverbutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.domaincoverbutton.Name = "domaincoverbutton";
            this.domaincoverbutton.Size = new System.Drawing.Size(102, 42);
            this.domaincoverbutton.TabIndex = 36;
            this.domaincoverbutton.Text = "Cover per domain";
            this.domaincoverbutton.UseVisualStyleBackColor = true;
            this.domaincoverbutton.Click += new System.EventHandler(this.domaincoverbutton_Click);
            // 
            // CBskip1000
            // 
            this.CBskip1000.AutoSize = true;
            this.CBskip1000.Location = new System.Drawing.Point(449, 155);
            this.CBskip1000.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.CBskip1000.Name = "CBskip1000";
            this.CBskip1000.Size = new System.Drawing.Size(129, 17);
            this.CBskip1000.TabIndex = 37;
            this.CBskip1000.Text = "Skip \"1000 articles...\"";
            this.CBskip1000.UseVisualStyleBackColor = true;
            // 
            // TBpcachartmax
            // 
            this.TBpcachartmax.Location = new System.Drawing.Point(148, 658);
            this.TBpcachartmax.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TBpcachartmax.Name = "TBpcachartmax";
            this.TBpcachartmax.Size = new System.Drawing.Size(76, 20);
            this.TBpcachartmax.TabIndex = 38;
            this.TBpcachartmax.Text = "0.1";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(19, 658);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(111, 13);
            this.label9.TabIndex = 39;
            this.label9.Text = "PCA chart max values";
            // 
            // namebutton
            // 
            this.namebutton.Location = new System.Drawing.Point(336, 627);
            this.namebutton.Name = "namebutton";
            this.namebutton.Size = new System.Drawing.Size(102, 44);
            this.namebutton.TabIndex = 40;
            this.namebutton.Text = "Person names";
            this.namebutton.UseVisualStyleBackColor = true;
            this.namebutton.Click += new System.EventHandler(this.namebutton_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(444, 637);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(59, 13);
            this.label10.TabIndex = 41;
            this.label10.Text = "Break after";
            // 
            // TB_break
            // 
            this.TB_break.Location = new System.Drawing.Point(501, 634);
            this.TB_break.Name = "TB_break";
            this.TB_break.Size = new System.Drawing.Size(44, 20);
            this.TB_break.TabIndex = 42;
            // 
            // Qlistbutton
            // 
            this.Qlistbutton.Location = new System.Drawing.Point(335, 677);
            this.Qlistbutton.Name = "Qlistbutton";
            this.Qlistbutton.Size = new System.Drawing.Size(103, 37);
            this.Qlistbutton.TabIndex = 43;
            this.Qlistbutton.Text = "Q-list to labels";
            this.Qlistbutton.UseVisualStyleBackColor = true;
            this.Qlistbutton.Click += new System.EventHandler(this.Qlistbutton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(570, 761);
            this.Controls.Add(this.Qlistbutton);
            this.Controls.Add(this.TB_break);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.namebutton);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.TBpcachartmax);
            this.Controls.Add(this.CBskip1000);
            this.Controls.Add(this.domaincoverbutton);
            this.Controls.Add(this.TBmarkersize);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.TBlargelimit);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.TBmeanshift);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.meanshiftbutton);
            this.Controls.Add(this.TBcluster);
            this.Controls.Add(this.Kmeansbutton);
            this.Controls.Add(this.randombutton);
            this.Controls.Add(this.CB_normalize);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TB_maxrange);
            this.Controls.Add(this.TB_minrange);
            this.Controls.Add(this.logtransformbutton);
            this.Controls.Add(this.nexusbutton);
            this.Controls.Add(this.PCAbutton);
            this.Controls.Add(this.testbutton);
            this.Controls.Add(this.neighborjoinbutton);
            this.Controls.Add(this.Comparebutton);
            this.Controls.Add(this.treebutton);
            this.Controls.Add(this.Normalizebutton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TB_maxlevels);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TB_miniw);
            this.Controls.Add(this.iwtablebutton);
            this.Controls.Add(this.iwstatbutton);
            this.Controls.Add(this.subclassbutton);
            this.Controls.Add(this.CB_props);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.Subdump_button);
            this.Controls.Add(this.CB_categories);
            this.Controls.Add(this.quitbutton);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.TBcluster)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TBmarkersize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button quitbutton;
        private System.Windows.Forms.Button Subdump_button;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.CheckedListBox CB_props;
        private System.Windows.Forms.CheckedListBox CB_categories;
        private System.Windows.Forms.Button subclassbutton;
        private System.Windows.Forms.Button iwstatbutton;
        private System.Windows.Forms.Button iwtablebutton;
        private System.Windows.Forms.TextBox TB_miniw;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TB_maxlevels;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Normalizebutton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button treebutton;
        private System.Windows.Forms.Button Comparebutton;
        private System.Windows.Forms.Button neighborjoinbutton;
        private System.Windows.Forms.Button testbutton;
        private System.Windows.Forms.Button PCAbutton;
        private System.Windows.Forms.Button nexusbutton;
        private System.Windows.Forms.Button logtransformbutton;
        private System.Windows.Forms.TextBox TB_minrange;
        private System.Windows.Forms.TextBox TB_maxrange;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox CB_normalize;
        private System.Windows.Forms.Button randombutton;
        private System.Windows.Forms.Button Kmeansbutton;
        private System.Windows.Forms.TrackBar TBcluster;
        private System.Windows.Forms.Button meanshiftbutton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox TBmeanshift;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox TBlargelimit;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TrackBar TBmarkersize;
        private System.Windows.Forms.Button domaincoverbutton;
        private System.Windows.Forms.CheckBox CBskip1000;
        private System.Windows.Forms.TextBox TBpcachartmax;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button namebutton;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox TB_break;
        private System.Windows.Forms.Button Qlistbutton;
    }
}

