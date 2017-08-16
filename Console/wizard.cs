//=================================================================
// wizard.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004, 2005, 2006  FlexRadio Systems
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// You may contact us via email at: sales@flex-radio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    12100 Technology Blvd.
//    Austin, TX 78727
//    USA
//=================================================================

/*
 *  Changes for GenesisRadio
 *  Copyright (C)2009,2010,2011,2012 YT7PWR Goran Radivojevic
 *  contact via email at: yt7pwr@ptt.rs or yt7pwr2002@yahoo.com
*/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace PowerSDR
{
    public class SetupWizard : System.Windows.Forms.Form
    {
        #region Variable Declaration

        System.Resources.ResourceManager resource;

        private enum Page
        {
            WELCOME,
            MODEL,
            USB,
            SOUND_CARD,
            FINISHED
        }

        private bool usb_present = false;
        private bool Si570_present;
        private int sound_card_index;
        private Model model;

        Console console;
        private System.Windows.Forms.ButtonTS btnPrevious;
        private System.Windows.Forms.ButtonTS btnNext;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.LabelTS lblMessage1;
        private System.Windows.Forms.RadioButtonTS radYes;
        private System.Windows.Forms.RadioButtonTS radNo;
        private System.Windows.Forms.LabelTS lblMessage2;
        private System.Windows.Forms.ComboBoxTS comboBox1;
        private System.Windows.Forms.LabelTS lblCombo;
        private System.Windows.Forms.ButtonTS btnFinished;
        private System.Windows.Forms.ComboBoxTS comboBox2;
        private System.Windows.Forms.ButtonTS button1;
        private System.Windows.Forms.ComboBoxTS comboBox3;
        private System.Windows.Forms.RadioButtonTS radModelG40;
        private System.Windows.Forms.RadioButtonTS radModelG3020;
        private System.Windows.Forms.RadioButtonTS radModelG59;
        private System.Windows.Forms.GroupBox grpModel;
        private RadioButtonTS radModelG160;
        private RadioButtonTS radModelG80;
        private RadioButtonTS radModelNetBox;
        private RadioButtonTS radModelQRP2000;
        private RadioButtonTS radModelG500;
        private RadioButtonTS radModelG137;
        private RadioButtonTS radModelG11;
        private System.ComponentModel.Container components = null;

        #endregion

        #region Constructor and Destructor

        public SetupWizard(Console c, int sound_card_index)
        {
            this.AutoScaleMode = AutoScaleMode.Inherit;
            InitializeComponent();
            float dpi = this.CreateGraphics().DpiX;
            float ratio = dpi / 96.0f;
            string font_name = this.Font.Name;
            float size = (float)(8.25 / ratio);
            System.Drawing.Font new_font = new System.Drawing.Font(font_name, size);
            this.Font = new_font;
            this.PerformAutoScale();
            this.PerformLayout();

            console = c;

            resource = new System.Resources.ResourceManager(typeof(SetupWizard));

            CurPage = Page.WELCOME;
            usb_present = false;
            Si570_present = false;

            model = console.CurrentModel;
            switch (model)
            {
                case Model.GENESIS_G11:
                    radModelG11.Checked = true;
                    break;
                case Model.GENESIS_G59USB:
                    radModelG59.Checked = true;
                    break;
                case Model.GENESIS_G3020:
                    radModelG3020.Checked = true;
                    break;
                case Model.GENESIS_G40:
                    radModelG40.Checked = true;
                    break;
                case Model.GENESIS_G80:
                    radModelG80.Checked = true;
                    break;
                case Model.GENESIS_G160:
                    radModelG160.Checked = true;
                    break;
                case Model.GENESIS_G137:
                    radModelG137.Checked = true;
                    break;
                case Model.GENESIS_G500:
                    radModelG500.Checked = true;
                    break;
                case Model.QRP2000:
                    radModelQRP2000.Checked = true;
                    break;
            }

            comboBox3.SelectedIndex = sound_card_index;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupWizard));
            this.btnPrevious = new System.Windows.Forms.ButtonTS();
            this.btnNext = new System.Windows.Forms.ButtonTS();
            this.btnFinished = new System.Windows.Forms.ButtonTS();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblMessage1 = new System.Windows.Forms.LabelTS();
            this.radYes = new System.Windows.Forms.RadioButtonTS();
            this.radNo = new System.Windows.Forms.RadioButtonTS();
            this.lblMessage2 = new System.Windows.Forms.LabelTS();
            this.comboBox1 = new System.Windows.Forms.ComboBoxTS();
            this.lblCombo = new System.Windows.Forms.LabelTS();
            this.comboBox2 = new System.Windows.Forms.ComboBoxTS();
            this.button1 = new System.Windows.Forms.ButtonTS();
            this.comboBox3 = new System.Windows.Forms.ComboBoxTS();
            this.grpModel = new System.Windows.Forms.GroupBox();
            this.radModelG11 = new System.Windows.Forms.RadioButtonTS();
            this.radModelQRP2000 = new System.Windows.Forms.RadioButtonTS();
            this.radModelG500 = new System.Windows.Forms.RadioButtonTS();
            this.radModelG137 = new System.Windows.Forms.RadioButtonTS();
            this.radModelNetBox = new System.Windows.Forms.RadioButtonTS();
            this.radModelG160 = new System.Windows.Forms.RadioButtonTS();
            this.radModelG80 = new System.Windows.Forms.RadioButtonTS();
            this.radModelG40 = new System.Windows.Forms.RadioButtonTS();
            this.radModelG3020 = new System.Windows.Forms.RadioButtonTS();
            this.radModelG59 = new System.Windows.Forms.RadioButtonTS();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.grpModel.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPrevious
            // 
            this.btnPrevious.Image = null;
            this.btnPrevious.Location = new System.Drawing.Point(145, 360);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(75, 23);
            this.btnPrevious.TabIndex = 0;
            this.btnPrevious.Text = "Previous";
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnNext
            // 
            this.btnNext.Image = null;
            this.btnNext.Location = new System.Drawing.Point(261, 360);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 23);
            this.btnNext.TabIndex = 1;
            this.btnNext.Text = "Next";
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnFinished
            // 
            this.btnFinished.Enabled = false;
            this.btnFinished.Image = null;
            this.btnFinished.Location = new System.Drawing.Point(377, 360);
            this.btnFinished.Name = "btnFinished";
            this.btnFinished.Size = new System.Drawing.Size(75, 23);
            this.btnFinished.TabIndex = 2;
            this.btnFinished.Text = "Finish";
            this.btnFinished.Click += new System.EventHandler(this.btnFinished_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
            this.pictureBox1.Location = new System.Drawing.Point(42, 62);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(245, 167);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Visible = false;
            // 
            // lblMessage1
            // 
            this.lblMessage1.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage1.Image = null;
            this.lblMessage1.Location = new System.Drawing.Point(16, 8);
            this.lblMessage1.Name = "lblMessage1";
            this.lblMessage1.Size = new System.Drawing.Size(456, 136);
            this.lblMessage1.TabIndex = 4;
            this.lblMessage1.Text = "lblMessage1";
            // 
            // radYes
            // 
            this.radYes.Image = null;
            this.radYes.Location = new System.Drawing.Point(375, 118);
            this.radYes.Name = "radYes";
            this.radYes.Size = new System.Drawing.Size(48, 16);
            this.radYes.TabIndex = 5;
            this.radYes.Text = "Yes";
            this.radYes.Visible = false;
            this.radYes.CheckedChanged += new System.EventHandler(this.radYes_CheckedChanged);
            // 
            // radNo
            // 
            this.radNo.Image = null;
            this.radNo.Location = new System.Drawing.Point(431, 118);
            this.radNo.Name = "radNo";
            this.radNo.Size = new System.Drawing.Size(48, 16);
            this.radNo.TabIndex = 6;
            this.radNo.Text = "No";
            this.radNo.Visible = false;
            this.radNo.CheckedChanged += new System.EventHandler(this.radNo_CheckedChanged);
            // 
            // lblMessage2
            // 
            this.lblMessage2.Image = null;
            this.lblMessage2.Location = new System.Drawing.Point(24, 299);
            this.lblMessage2.Name = "lblMessage2";
            this.lblMessage2.Size = new System.Drawing.Size(464, 48);
            this.lblMessage2.TabIndex = 7;
            this.lblMessage2.Text = "lblMessage2";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.DropDownWidth = 136;
            this.comboBox1.Items.AddRange(new object[] {
            "DEMI144-28FRS",
            "DEMI144-28 (25w)"});
            this.comboBox1.Location = new System.Drawing.Point(429, 123);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(136, 21);
            this.comboBox1.TabIndex = 8;
            this.comboBox1.Visible = false;
            // 
            // lblCombo
            // 
            this.lblCombo.Image = null;
            this.lblCombo.Location = new System.Drawing.Point(363, 27);
            this.lblCombo.Name = "lblCombo";
            this.lblCombo.Size = new System.Drawing.Size(192, 72);
            this.lblCombo.TabIndex = 9;
            this.lblCombo.Text = "lblCombo";
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.DropDownWidth = 56;
            this.comboBox2.Items.AddRange(new object[] {
            "10",
            "20"});
            this.comboBox2.Location = new System.Drawing.Point(457, 123);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(56, 21);
            this.comboBox2.TabIndex = 10;
            // 
            // button1
            // 
            this.button1.Image = null;
            this.button1.Location = new System.Drawing.Point(429, 150);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "Select File ...";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // comboBox3
            // 
            this.comboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3.DropDownWidth = 184;
            this.comboBox3.Items.AddRange(new object[] {
            "M-Audio Delta 44 (PCI)",
            "PreSonus FireBox (FireWire)",
            "Edirol FA-66 (FireWire)",
            "SB Audigy (PCI)",
            "SB Audigy 2 (PCI)",
            "SB Audigy 2 ZS (PCI)",
            "Sound Blaster Extigy (USB)",
            "Sound Blaster MP3+ (USB)",
            "Turtle Beach Santa Cruz (PCI)",
            "Realtek HD audio",
            "Unsupported Card"});
            this.comboBox3.Location = new System.Drawing.Point(356, 62);
            this.comboBox3.MaxDropDownItems = 10;
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(184, 21);
            this.comboBox3.TabIndex = 12;
            this.comboBox3.SelectedIndexChanged += new System.EventHandler(this.comboBox3_SelectedIndexChanged);
            // 
            // grpModel
            // 
            this.grpModel.Controls.Add(this.radModelG11);
            this.grpModel.Controls.Add(this.radModelQRP2000);
            this.grpModel.Controls.Add(this.radModelG500);
            this.grpModel.Controls.Add(this.radModelG137);
            this.grpModel.Controls.Add(this.radModelNetBox);
            this.grpModel.Controls.Add(this.radModelG160);
            this.grpModel.Controls.Add(this.radModelG80);
            this.grpModel.Controls.Add(this.radModelG40);
            this.grpModel.Controls.Add(this.radModelG3020);
            this.grpModel.Controls.Add(this.radModelG59);
            this.grpModel.Location = new System.Drawing.Point(350, 42);
            this.grpModel.Name = "grpModel";
            this.grpModel.Size = new System.Drawing.Size(167, 254);
            this.grpModel.TabIndex = 20;
            this.grpModel.TabStop = false;
            this.grpModel.Text = "Radio model";
            this.grpModel.Visible = false;
            // 
            // radModelG11
            // 
            this.radModelG11.Image = null;
            this.radModelG11.Location = new System.Drawing.Point(39, 231);
            this.radModelG11.Name = "radModelG11";
            this.radModelG11.Size = new System.Drawing.Size(88, 17);
            this.radModelG11.TabIndex = 12;
            this.radModelG11.Text = "G11";
            this.radModelG11.CheckedChanged += new System.EventHandler(this.radModelG11_CheckedChanged);
            // 
            // radModelQRP2000
            // 
            this.radModelQRP2000.AutoSize = true;
            this.radModelQRP2000.Image = null;
            this.radModelQRP2000.Location = new System.Drawing.Point(39, 210);
            this.radModelQRP2000.Name = "radModelQRP2000";
            this.radModelQRP2000.Size = new System.Drawing.Size(72, 17);
            this.radModelQRP2000.TabIndex = 11;
            this.radModelQRP2000.Text = "QRP2000";
            this.radModelQRP2000.UseVisualStyleBackColor = true;
            this.radModelQRP2000.CheckedChanged += new System.EventHandler(this.radModelQRP2000_CheckedChanged);
            // 
            // radModelG500
            // 
            this.radModelG500.AutoSize = true;
            this.radModelG500.Image = null;
            this.radModelG500.Location = new System.Drawing.Point(39, 162);
            this.radModelG500.Name = "radModelG500";
            this.radModelG500.Size = new System.Drawing.Size(51, 17);
            this.radModelG500.TabIndex = 10;
            this.radModelG500.Text = "G500";
            this.radModelG500.UseVisualStyleBackColor = true;
            this.radModelG500.CheckedChanged += new System.EventHandler(this.radModelG500_CheckedChanged);
            // 
            // radModelG137
            // 
            this.radModelG137.AutoSize = true;
            this.radModelG137.Image = null;
            this.radModelG137.Location = new System.Drawing.Point(39, 138);
            this.radModelG137.Name = "radModelG137";
            this.radModelG137.Size = new System.Drawing.Size(51, 17);
            this.radModelG137.TabIndex = 9;
            this.radModelG137.Text = "G137";
            this.radModelG137.UseVisualStyleBackColor = true;
            this.radModelG137.CheckedChanged += new System.EventHandler(this.radModelG137_CheckedChanged);
            // 
            // radModelNetBox
            // 
            this.radModelNetBox.AutoSize = true;
            this.radModelNetBox.Image = null;
            this.radModelNetBox.Location = new System.Drawing.Point(39, 186);
            this.radModelNetBox.Name = "radModelNetBox";
            this.radModelNetBox.Size = new System.Drawing.Size(68, 17);
            this.radModelNetBox.TabIndex = 8;
            this.radModelNetBox.Text = "NET Box";
            this.radModelNetBox.UseVisualStyleBackColor = true;
            this.radModelNetBox.CheckedChanged += new System.EventHandler(this.radModelNetBox_CheckedChanged);
            // 
            // radModelG160
            // 
            this.radModelG160.AutoSize = true;
            this.radModelG160.Image = null;
            this.radModelG160.Location = new System.Drawing.Point(39, 114);
            this.radModelG160.Name = "radModelG160";
            this.radModelG160.Size = new System.Drawing.Size(51, 17);
            this.radModelG160.TabIndex = 7;
            this.radModelG160.Text = "G160";
            this.radModelG160.UseVisualStyleBackColor = true;
            this.radModelG160.CheckedChanged += new System.EventHandler(this.radModelG160_CheckedChanged);
            // 
            // radModelG80
            // 
            this.radModelG80.AutoSize = true;
            this.radModelG80.Image = null;
            this.radModelG80.Location = new System.Drawing.Point(39, 90);
            this.radModelG80.Name = "radModelG80";
            this.radModelG80.Size = new System.Drawing.Size(45, 17);
            this.radModelG80.TabIndex = 6;
            this.radModelG80.Text = "G80";
            this.radModelG80.UseVisualStyleBackColor = true;
            this.radModelG80.CheckedChanged += new System.EventHandler(this.radModelG80_CheckedChanged);
            // 
            // radModelG40
            // 
            this.radModelG40.Image = null;
            this.radModelG40.Location = new System.Drawing.Point(39, 66);
            this.radModelG40.Name = "radModelG40";
            this.radModelG40.Size = new System.Drawing.Size(88, 17);
            this.radModelG40.TabIndex = 5;
            this.radModelG40.Text = "G40";
            this.radModelG40.CheckedChanged += new System.EventHandler(this.radModelG40_CheckedChanged);
            // 
            // radModelG3020
            // 
            this.radModelG3020.Image = null;
            this.radModelG3020.Location = new System.Drawing.Point(39, 42);
            this.radModelG3020.Name = "radModelG3020";
            this.radModelG3020.Size = new System.Drawing.Size(88, 17);
            this.radModelG3020.TabIndex = 4;
            this.radModelG3020.Text = "G3020";
            this.radModelG3020.CheckedChanged += new System.EventHandler(this.radModelG3020_CheckedChanged);
            // 
            // radModelG59
            // 
            this.radModelG59.Image = null;
            this.radModelG59.Location = new System.Drawing.Point(39, 18);
            this.radModelG59.Name = "radModelG59";
            this.radModelG59.Size = new System.Drawing.Size(88, 17);
            this.radModelG59.TabIndex = 3;
            this.radModelG59.Text = "G59";
            this.radModelG59.CheckedChanged += new System.EventHandler(this.radModelG59_CheckedChanged);
            // 
            // SetupWizard
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(607, 413);
            this.Controls.Add(this.grpModel);
            this.Controls.Add(this.comboBox3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.radYes);
            this.Controls.Add(this.radNo);
            this.Controls.Add(this.lblMessage2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnFinished);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnPrevious);
            this.Controls.Add(this.lblCombo);
            this.Controls.Add(this.lblMessage1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupWizard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Genesis Setup Wizard - Welcome";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.grpModel.ResumeLayout(false);
            this.grpModel.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        #region Misc Routines

        private Stream GetResource(string name)
        {
            return this.GetType().Assembly.GetManifestResourceStream(name);
        }

        private void SwitchPage(Page p)
        {
            switch (p)
            {
                case Page.WELCOME:
                    this.Text = "Genesis Setup Wizard - Welcome";
                    grpModel.Visible = false;
                    btnFinished.Enabled = false;
                    btnNext.Enabled = true;
                    btnPrevious.Enabled = false;		// first screen			
                    button1.Visible = false;
                    comboBox1.Visible = false;
                    comboBox2.Visible = false;
                    comboBox3.Visible = false;
                    lblCombo.Visible = false;
                    lblMessage1.Text = "Welcome to the Genesis Setup Wizard.  This Setup Wizard is " +
                        "intended to simplify the setup process by providing you with an easy-to-use " +
                        "question/answer format.  Suggestions to improve this wizard are enouraged " +
                        "and can be emailed to support@genesisradio.com.au or posted on our forums at " +
                        "Yahoo";
                    lblMessage2.Text = "Note: More experieced users may perform these setup steps " +
                        "manually by closing this Wizard and opening the Setup Form.";
                    pictureBox1.Image = null;
                    pictureBox1.Visible = false;
                    radYes.Visible = false;
                    radNo.Visible = false;
                    break;
                case Page.MODEL:
                    this.Text = "Genesis Setup Wizard - Radio Model";
                    btnFinished.Enabled = false;
                    btnNext.Enabled = true;
                    btnPrevious.Enabled = true;
                    button1.Visible = false;
                    comboBox1.Visible = false;
                    comboBox2.Visible = false;
                    comboBox3.Visible = false;
                    lblCombo.Visible = false;
                    grpModel.Visible = true;
                    lblMessage1.Text = "Please select the model of the radio you will be using.";
                    radModelG59_CheckedChanged(this, EventArgs.Empty);
                    radModelG3020_CheckedChanged(this, EventArgs.Empty);
                    radModelG40_CheckedChanged(this, EventArgs.Empty);
                    radModelG11_CheckedChanged(this, EventArgs.Empty);
                    pictureBox1.Visible = true;
                    radNo.Visible = false;
                    radYes.Visible = false;
                    break;
                case Page.USB:
                    this.Text = "Genesis Setup Wizard - USB Setup";
                    btnFinished.Enabled = false;
                    btnNext.Enabled = true;
                    btnPrevious.Enabled = true;
                    button1.Visible = false;
                    comboBox1.Visible = false;
                    comboBox2.Visible = false;
                    comboBox3.Visible = false;
                    grpModel.Visible = false;
                    lblCombo.Visible = false;
                    lblMessage1.Text = "Is the external USB Si570 signal generator included in your hardware configuration?";
                    lblMessage2.Text = "For more information, see http://www.genesisradio.com.au";
                    pictureBox1.Image = new Bitmap(GetResource("PowerSDR.images.Si570.jpg"));
                    pictureBox1.Visible = true;
                    if (usb_present)
                        radYes.Select();
                    else
                        radNo.Select();
                    radYes.Visible = true;
                    radNo.Visible = true;
                    break;
                case Page.SOUND_CARD:
                    this.Text = "Genesis Setup Wizard - Sound Card Setup";
                    btnFinished.Enabled = false;
                    btnNext.Enabled = true;
                    btnPrevious.Enabled = true;
                    button1.Visible = false;
                    comboBox1.Visible = false;
                    comboBox2.Visible = false;
                    comboBox3.Visible = true;
                    grpModel.Visible = false;
                    lblCombo.Visible = false;
                    lblMessage1.Text = "Please select your sound card";
                    lblMessage2.Text = "If you don't see your card in the list, select Unsupported Card.\n" +
                        "If using an Unsupported Card, you will need to modify the settings in the Audio " +
                        "Tab of the Setup Form when finished with this wizard";
                    pictureBox1.Image = new Bitmap(GetResource("PowerSDR.images.soundcard.jpg"));
                    pictureBox1.Visible = true;
                    radYes.Visible = false;
                    radNo.Visible = false;
                    break;
                case Page.FINISHED:
                    this.Text = "Genesis Setup Wizard - Finished";
                    btnFinished.Enabled = true;
                    btnNext.Enabled = false;
                    btnPrevious.Enabled = true;
                    button1.Visible = false;
                    comboBox1.Visible = false;
                    comboBox2.Visible = false;
                    comboBox3.Visible = false;
                    grpModel.Visible = false;
                    lblCombo.Visible = false;
                    lblMessage1.Text = "Setup is now complete.  To run this wizard again, select Setup " +
                        "from the main form and click the wizard button.  To close the wizard, click " +
                        "the Finish button.";
                    lblMessage2.Visible = false;
                    pictureBox1.Image = null;
                    pictureBox1.Visible = false;
                    radYes.Visible = false;
                    radNo.Visible = false;
                    break;
            }
        }

        #endregion

        #region Properties

        private Page current_page = Page.WELCOME;
        private Page CurPage
        {
            get { return current_page; }
            set
            {
                current_page = value;
                SwitchPage(current_page);
            }
        }

        #endregion

        #region Event Handlers

        private void btnNext_Click(object sender, System.EventArgs e)
        {
            switch (current_page)
            {
                case Page.WELCOME:
                    CurPage = Page.MODEL;
                    btnNext.Focus();
                    break;
                case Page.MODEL:
                    CurPage = Page.USB;
                    btnNext.Focus();
                    break;
                case Page.USB:
                    CurPage = Page.SOUND_CARD;
                    btnNext.Focus();
                    break;
                case Page.SOUND_CARD:
                    sound_card_index = comboBox3.SelectedIndex;
                    CurPage = Page.FINISHED;
                    btnFinished.Focus();
                    break;
                case Page.FINISHED:
                    break;
            }
        }

        private void btnPrevious_Click(object sender, System.EventArgs e)
        {
            switch (current_page)
            {
                case Page.WELCOME:
                    break;
                case Page.MODEL:
                    CurPage = Page.WELCOME;
                    btnPrevious.Focus();
                    break;
                case Page.USB:
                    CurPage = Page.MODEL;
                    btnPrevious.Focus();
                    break;
                case Page.SOUND_CARD:
                    sound_card_index = comboBox3.SelectedIndex;
                    CurPage = Page.USB;
                    btnPrevious.Focus();
                    break;
                case Page.FINISHED:
                    CurPage = Page.SOUND_CARD;
                    btnPrevious.Focus();
                    break;
            }
        }

        private void radYes_CheckedChanged(object sender, System.EventArgs e)
        {
            Si570_present = true;
        }

        private void radNo_CheckedChanged(object sender, System.EventArgs e)
        {
            Si570_present = false; ;
        }

        private void btnFinished_Click(object sender, System.EventArgs e)
        {
            if (sound_card_index >= 0)
            {
                SoundCard card = SoundCard.FIRST;
                switch (comboBox3.Text)
                {
                    case "M-Audio Delta 44 (PCI)":
                        card = SoundCard.DELTA_44;
                        break;
                    case "PreSonus FireBox (FireWire)":
                        card = SoundCard.FIREBOX;
                        break;
                    case "Edirol FA-66 (FireWire)":
                        card = SoundCard.EDIROL_FA_66;
                        break;
                    case "SB Audigy (PCI)":
                        card = SoundCard.AUDIGY;
                        break;
                    case "SB Audigy 2 (PCI)":
                        card = SoundCard.AUDIGY_2;
                        break;
                    case "SB Audigy 2 ZS (PCI)":
                        card = SoundCard.AUDIGY_2_ZS;
                        break;
                    case "Sound Blaster Extigy (USB)":
                        card = SoundCard.EXTIGY;
                        break;
                    case "Sound Blaster MP3+ (USB)":
                        card = SoundCard.MP3_PLUS;
                        break;
                    case "Turtle Beach Santa Cruz (PCI)":
                        card = SoundCard.SANTA_CRUZ;
                        break;
                    case "No Mixer Audio Card":
                        card = SoundCard.NO_MIXER_AUDIO_CARD;
                        break;
                    case "Realtek HD audio":
                        card = SoundCard.REALTEK_HD_AUDIO;
                        break;
                    case "Unsupported Card":
                        card = SoundCard.UNSUPPORTED_CARD;
                        break;
                }
                console.CurrentSoundCard = card;
            }

            console.SetupForm.CurrentModel = model;

            if (Si570_present)
            {
                console.SetupForm.chkGeneralUSBPresent.Checked = true;
            }
            else
            {
                console.SetupForm.chkGeneralUSBPresent.Checked = false;
            }

            console.run_setup_wizard = false;
            this.Close();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            string path = Application.StartupPath;
            path = path.Substring(0, path.LastIndexOf("\\"));
        }

        private void comboBox3_SelectedIndexChanged(object sender, System.EventArgs e)
        {

        }

        #endregion

        #region Radio model selection

        private void radModelG59_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radModelG59.Checked)
            {
                model = Model.GENESIS_G59USB;
                console.SetupForm.radGenModelGenesisG59.Checked = true;

                if (grpModel.Visible)
                    pictureBox1.Image = new Bitmap(GetResource("PowerSDR.images.genesisG59.jpg"));
            }
        }

        private void radModelG3020_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radModelG3020.Checked)
            {
                model = Model.GENESIS_G3020;
                console.SetupForm.radGenModelGenesisG3020.Checked = true;

                if (grpModel.Visible)
                    pictureBox1.Image = new Bitmap(GetResource("PowerSDR.images.genesisGYY.jpg"));
            }
        }

        private void radModelG40_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radModelG40.Checked)
            {
                model = Model.GENESIS_G40;
                console.SetupForm.radGenModelGenesisG40.Checked = true;

                if (grpModel.Visible)
                    pictureBox1.Image = new Bitmap(GetResource("PowerSDR.images.genesisGXX.jpg"));
            }
        }

        private void radModelG80_CheckedChanged(object sender, EventArgs e)
        {
            if (radModelG80.Checked)
            {
                model = Model.GENESIS_G80;
                console.SetupForm.radGenModelGenesisG80.Checked = true;

                if (grpModel.Visible)
                    pictureBox1.Image = new Bitmap(GetResource("PowerSDR.images.genesisGXX.jpg"));
            }
        }

        private void radModelG160_CheckedChanged(object sender, EventArgs e)
        {
            if (radModelG160.Checked)
            {
                model = Model.GENESIS_G160;
                console.SetupForm.radGenModelGenesisG160.Checked = true;

                if (grpModel.Visible)
                    pictureBox1.Image = new Bitmap(GetResource("PowerSDR.images.genesisGXX.jpg"));
            }
        }

        private void radModelNetBox_CheckedChanged(object sender, EventArgs e)
        {
            if (radModelNetBox.Checked)
            {
                model = Model.GENESIS_G59NET;
                console.SetupForm.radGenModelGenesisNET.Checked = true;

                if (grpModel.Visible)
                    pictureBox1.Image = new Bitmap(GetResource("PowerSDR.images.genesisGXX.jpg"));
            }
        }

        private void radModelG137_CheckedChanged(object sender, EventArgs e)
        {
            if (radModelG137.Checked)
            {
                model = Model.GENESIS_G137;
                console.SetupForm.radGenModelGenesisG137.Checked = true;

                if (grpModel.Visible)
                    pictureBox1.Image = new Bitmap(GetResource("PowerSDR.images.genesisGXX.jpg"));
            }
        }

        private void radModelG500_CheckedChanged(object sender, EventArgs e)
        {
            if (radModelG500.Checked)
            {
                model = Model.GENESIS_G500;
                console.SetupForm.radGenModelGenesisG500.Checked = true;

                if (grpModel.Visible)
                    pictureBox1.Image = new Bitmap(GetResource("PowerSDR.images.genesisGXX.jpg"));
            }
        }

        private void radModelQRP2000_CheckedChanged(object sender, EventArgs e)
        {
            if (radModelQRP2000.Checked)
            {
                model = Model.QRP2000;
                console.SetupForm.radGenModelQRP2000.Checked = true;

                if (grpModel.Visible)
                    pictureBox1.Image = new Bitmap(GetResource("PowerSDR.images.Si570.jpg"));
            }
        }

        private void radModelG11_CheckedChanged(object sender, EventArgs e)
        {
            if (radModelG11.Checked)
            {
                model = Model.GENESIS_G11;
                console.SetupForm.radGenModelGenesisG11.Checked = true;

                if (grpModel.Visible)
                    pictureBox1.Image = new Bitmap(GetResource("PowerSDR.images.genesisG11.jpg"));                    
            }
        }

        #endregion
    }
}
