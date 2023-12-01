using System;
using System.Reflection;
using System.Windows.Forms;

using GumpStudio.Properties;

namespace GumpStudio.Forms
{
	public class AboutBox : Form
	{
		private Label Label1;
		private LinkLabel lblHomepage;
		private Label lblVersion;
		private PictureBox PictureBox1;
		private FlowLayoutPanel flowLayoutPanel1;
		private TextBox txtAbout;

		public AboutBox()
		{
			Load += FrmAboutBox_Load;
			InitializeComponent();
		}

		private void CmdClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void FrmAboutBox_Load(object sender, EventArgs e)
		{
			lblVersion.Text = Resources.Core_Version__ + Assembly.GetExecutingAssembly().GetName().Version;
		}

		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
            PictureBox1 = new PictureBox();
            txtAbout = new TextBox();
            Label1 = new Label();
            lblVersion = new Label();
            lblHomepage = new LinkLabel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // PictureBox1
            // 
            PictureBox1.Dock = DockStyle.Top;
            PictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("PictureBox1.Image")));
            PictureBox1.Location = new System.Drawing.Point(0, 0);
            PictureBox1.Name = "PictureBox1";
            PictureBox1.Size = new System.Drawing.Size(454, 158);
            PictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            PictureBox1.TabIndex = 0;
            PictureBox1.TabStop = false;
            PictureBox1.Click += new EventHandler(PictureBox1_Click);
            // 
            // txtAbout
            // 
            txtAbout.BorderStyle = BorderStyle.FixedSingle;
            txtAbout.Dock = DockStyle.Fill;
            txtAbout.Location = new System.Drawing.Point(0, 158);
            txtAbout.Multiline = true;
            txtAbout.Name = "txtAbout";
            txtAbout.ReadOnly = true;
            txtAbout.ScrollBars = ScrollBars.Vertical;
            txtAbout.Size = new System.Drawing.Size(454, 150);
            txtAbout.TabIndex = 1;
            txtAbout.TabStop = false;
            txtAbout.Text = "Gump Studio Version 2.0.0";
            txtAbout.TextChanged += new EventHandler(txtAbout_TextChanged);
            // 
            // Label1
            // 
            Label1.Anchor = AnchorStyles.None;
            Label1.AutoSize = true;
            Label1.Location = new System.Drawing.Point(6, 3);
            Label1.Name = "Label1";
            Label1.Size = new System.Drawing.Size(162, 17);
            Label1.TabIndex = 3;
            Label1.Text = "(C) Dragon Slayer";
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Location = new System.Drawing.Point(180, 3);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new System.Drawing.Size(56, 17);
            lblVersion.TabIndex = 4;
            lblVersion.Text = "Version";
            // 
            // lblHomepage
            // 
            lblHomepage.Anchor = AnchorStyles.None;
            lblHomepage.AutoSize = true;
            lblHomepage.Location = new System.Drawing.Point(174, 3);
            lblHomepage.Name = "lblHomepage";
            lblHomepage.Size = new System.Drawing.Size(0, 17);
            lblHomepage.TabIndex = 5;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(Label1);
            flowLayoutPanel1.Controls.Add(lblHomepage);
            flowLayoutPanel1.Controls.Add(lblVersion);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.Location = new System.Drawing.Point(0, 308);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(3);
            flowLayoutPanel1.Size = new System.Drawing.Size(454, 23);
            flowLayoutPanel1.TabIndex = 6;
            flowLayoutPanel1.WrapContents = false;
            // 
            // AboutBox
            // 
            AutoScaleMode = AutoScaleMode.None;
            AutoSize = true;
            ClientSize = new System.Drawing.Size(454, 331);
            Controls.Add(txtAbout);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(PictureBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "AboutBox";
            Text = "About Gump Studio.NET";
            Load += new EventHandler(FrmAboutBox_Load);
            ((System.ComponentModel.ISupportInitialize)PictureBox1).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

		}

		private void LblHomepage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			/*Process.Start(new ProcessStartInfo
			{
				UseShellExecute = true,
				FileName = "http://www.uox3.org"
			});*/
		}

		private const string _Text = @"
Gump Studio being redesigned to work with UOX3 Emulator.
http://www.uox3.org
Version 2.0.0
";

		public void SetText(string text)
		{
			txtAbout.Text = $"{_Text}{Environment.NewLine}{Environment.NewLine}====Plugin Specific Information===={Environment.NewLine}{Environment.NewLine}" + text;
		}

		private void txtAbout_TextChanged(object sender, EventArgs e)
		{

		}

		private void PictureBox1_Click(object sender, EventArgs e)
		{

		}
	}
}