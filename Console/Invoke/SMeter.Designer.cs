using System;
using System.Windows.Forms;
using System.Drawing;

namespace PowerSDR.Invoke
{
    partial class SMeter
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.progressSWR = new ProgressODoom.ProgressBarEx();
            this.fruityLoopsBackgroundPainterSWRLine = new ProgressODoom.FruityLoopsBackgroundPainter();
            this.fruityLoopsProgressPainterSWRLine = new ProgressODoom.FruityLoopsProgressPainter();
            this.fruityLoopsBackgroundPainterSigLine = new ProgressODoom.FruityLoopsBackgroundPainter();
            this.fruityLoopsProgressPainterSigLine = new ProgressODoom.FruityLoopsProgressPainter();
            this.progressSignal = new ProgressODoom.ProgressBarEx();
            this.labelTS13 = new System.Windows.Forms.LabelTS();
            this.labelTS11 = new System.Windows.Forms.LabelTS();
            this.labelTS6 = new System.Windows.Forms.LabelTS();
            this.labelTS12 = new System.Windows.Forms.LabelTS();
            this.labelTS14 = new System.Windows.Forms.LabelTS();
            this.labelTS15 = new System.Windows.Forms.LabelTS();
            this.labelTS17 = new System.Windows.Forms.LabelTS();
            this.lblPwr5 = new System.Windows.Forms.LabelTS();
            this.lblPwr4 = new System.Windows.Forms.LabelTS();
            this.lblPwr3 = new System.Windows.Forms.LabelTS();
            this.lblPwr2 = new System.Windows.Forms.LabelTS();
            this.lblPwr1 = new System.Windows.Forms.LabelTS();
            this.labelTS10 = new System.Windows.Forms.LabelTS();
            this.labelTS9 = new System.Windows.Forms.LabelTS();
            this.labelTS8 = new System.Windows.Forms.LabelTS();
            this.labelTS7 = new System.Windows.Forms.LabelTS();
            this.labelTS5 = new System.Windows.Forms.LabelTS();
            this.labelTS4 = new System.Windows.Forms.LabelTS();
            this.labelTS3 = new System.Windows.Forms.LabelTS();
            this.labelTS2 = new System.Windows.Forms.LabelTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.labelTS16 = new System.Windows.Forms.LabelTS();
            this.SuspendLayout();
            // 
            // progressSWR
            // 
            this.progressSWR.BackgroundPainter = this.fruityLoopsBackgroundPainterSWRLine;
            this.progressSWR.ForeColor = System.Drawing.Color.BlueViolet;
            this.progressSWR.Location = new System.Drawing.Point(8, 90);
            this.progressSWR.MarqueePercentage = 50;
            this.progressSWR.MarqueeSpeed = 30;
            this.progressSWR.MarqueeStep = 1;
            this.progressSWR.Maximum = 200;
            this.progressSWR.Minimum = 0;
            this.progressSWR.Name = "progressSWR";
            this.progressSWR.ProgressPadding = 0;
            this.progressSWR.ProgressPainter = this.fruityLoopsProgressPainterSWRLine;
            this.progressSWR.ProgressType = ProgressODoom.ProgressType.Smooth;
            this.progressSWR.ShowPercentage = false;
            this.progressSWR.Size = new System.Drawing.Size(225, 15);
            this.progressSWR.TabIndex = 1;
            this.progressSWR.Value = 0;
            // 
            // fruityLoopsBackgroundPainterSWRLine
            // 
            this.fruityLoopsBackgroundPainterSWRLine.FruityType = ProgressODoom.FruityLoopsProgressPainter.FruityLoopsProgressType.DoubleLayer;
            // 
            // fruityLoopsProgressPainterSWRLine
            // 
            this.fruityLoopsProgressPainterSWRLine.FruityType = ProgressODoom.FruityLoopsProgressPainter.FruityLoopsProgressType.DoubleLayer;
            this.fruityLoopsProgressPainterSWRLine.ProgressBorderPainter = null;
            // 
            // fruityLoopsBackgroundPainterSigLine
            // 
            this.fruityLoopsBackgroundPainterSigLine.FruityType = ProgressODoom.FruityLoopsProgressPainter.FruityLoopsProgressType.DoubleLayer;
            // 
            // fruityLoopsProgressPainterSigLine
            // 
            this.fruityLoopsProgressPainterSigLine.FruityType = ProgressODoom.FruityLoopsProgressPainter.FruityLoopsProgressType.DoubleLayer;
            this.fruityLoopsProgressPainterSigLine.ProgressBorderPainter = null;
            // 
            // progressSignal
            // 
            this.progressSignal.BackColor = System.Drawing.Color.Black;
            this.progressSignal.BackgroundPainter = this.fruityLoopsBackgroundPainterSigLine;
            this.progressSignal.ForeColor = System.Drawing.Color.DeepPink;
            this.progressSignal.Location = new System.Drawing.Point(8, 44);
            this.progressSignal.MarqueePercentage = 50;
            this.progressSignal.MarqueeSpeed = 30;
            this.progressSignal.MarqueeStep = 1;
            this.progressSignal.Maximum = 200;
            this.progressSignal.Minimum = 0;
            this.progressSignal.Name = "progressSignal";
            this.progressSignal.ProgressPadding = 0;
            this.progressSignal.ProgressPainter = this.fruityLoopsProgressPainterSigLine;
            this.progressSignal.ProgressType = ProgressODoom.ProgressType.Smooth;
            this.progressSignal.ShowPercentage = false;
            this.progressSignal.Size = new System.Drawing.Size(225, 15);
            this.progressSignal.TabIndex = 23;
            this.progressSignal.Value = 0;
            // 
            // labelTS13
            // 
            this.labelTS13.AutoSize = true;
            this.labelTS13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS13.ForeColor = System.Drawing.Color.White;
            this.labelTS13.Image = null;
            this.labelTS13.Location = new System.Drawing.Point(45, 60);
            this.labelTS13.Name = "labelTS13";
            this.labelTS13.Size = new System.Drawing.Size(14, 13);
            this.labelTS13.TabIndex = 33;
            this.labelTS13.Text = "3";
            // 
            // labelTS11
            // 
            this.labelTS11.AutoSize = true;
            this.labelTS11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS11.ForeColor = System.Drawing.Color.White;
            this.labelTS11.Image = null;
            this.labelTS11.Location = new System.Drawing.Point(102, 74);
            this.labelTS11.Name = "labelTS11";
            this.labelTS11.Size = new System.Drawing.Size(36, 13);
            this.labelTS11.TabIndex = 32;
            this.labelTS11.Text = "SWR";
            // 
            // labelTS6
            // 
            this.labelTS6.AutoSize = true;
            this.labelTS6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS6.ForeColor = System.Drawing.Color.White;
            this.labelTS6.Image = null;
            this.labelTS6.Location = new System.Drawing.Point(205, 60);
            this.labelTS6.Name = "labelTS6";
            this.labelTS6.Size = new System.Drawing.Size(33, 13);
            this.labelTS6.TabIndex = 31;
            this.labelTS6.Text = "20W";
            // 
            // labelTS12
            // 
            this.labelTS12.AutoSize = true;
            this.labelTS12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS12.ForeColor = System.Drawing.Color.White;
            this.labelTS12.Image = null;
            this.labelTS12.Location = new System.Drawing.Point(142, 60);
            this.labelTS12.Name = "labelTS12";
            this.labelTS12.Size = new System.Drawing.Size(21, 13);
            this.labelTS12.TabIndex = 29;
            this.labelTS12.Text = "10";
            // 
            // labelTS14
            // 
            this.labelTS14.AutoSize = true;
            this.labelTS14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS14.ForeColor = System.Drawing.Color.White;
            this.labelTS14.Image = null;
            this.labelTS14.Location = new System.Drawing.Point(74, 60);
            this.labelTS14.Name = "labelTS14";
            this.labelTS14.Size = new System.Drawing.Size(14, 13);
            this.labelTS14.TabIndex = 27;
            this.labelTS14.Text = "5";
            // 
            // labelTS15
            // 
            this.labelTS15.AutoSize = true;
            this.labelTS15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS15.ForeColor = System.Drawing.Color.White;
            this.labelTS15.Image = null;
            this.labelTS15.Location = new System.Drawing.Point(19, 60);
            this.labelTS15.Name = "labelTS15";
            this.labelTS15.Size = new System.Drawing.Size(14, 13);
            this.labelTS15.TabIndex = 26;
            this.labelTS15.Text = "1";
            // 
            // labelTS17
            // 
            this.labelTS17.AutoSize = true;
            this.labelTS17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS17.ForeColor = System.Drawing.Color.White;
            this.labelTS17.Image = null;
            this.labelTS17.Location = new System.Drawing.Point(4, 60);
            this.labelTS17.Name = "labelTS17";
            this.labelTS17.Size = new System.Drawing.Size(14, 13);
            this.labelTS17.TabIndex = 24;
            this.labelTS17.Text = "0";
            // 
            // lblPwr5
            // 
            this.lblPwr5.AutoSize = true;
            this.lblPwr5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblPwr5.ForeColor = System.Drawing.Color.White;
            this.lblPwr5.Image = null;
            this.lblPwr5.Location = new System.Drawing.Point(207, 107);
            this.lblPwr5.Name = "lblPwr5";
            this.lblPwr5.Size = new System.Drawing.Size(26, 13);
            this.lblPwr5.TabIndex = 19;
            this.lblPwr5.Text = "Inf.";
            // 
            // lblPwr4
            // 
            this.lblPwr4.AutoSize = true;
            this.lblPwr4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblPwr4.ForeColor = System.Drawing.Color.White;
            this.lblPwr4.Image = null;
            this.lblPwr4.Location = new System.Drawing.Point(153, 107);
            this.lblPwr4.Name = "lblPwr4";
            this.lblPwr4.Size = new System.Drawing.Size(21, 13);
            this.lblPwr4.TabIndex = 18;
            this.lblPwr4.Text = "50";
            // 
            // lblPwr3
            // 
            this.lblPwr3.AutoSize = true;
            this.lblPwr3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblPwr3.ForeColor = System.Drawing.Color.White;
            this.lblPwr3.Image = null;
            this.lblPwr3.Location = new System.Drawing.Point(107, 107);
            this.lblPwr3.Name = "lblPwr3";
            this.lblPwr3.Size = new System.Drawing.Size(25, 13);
            this.lblPwr3.TabIndex = 17;
            this.lblPwr3.Text = "3.0";
            // 
            // lblPwr2
            // 
            this.lblPwr2.AutoSize = true;
            this.lblPwr2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblPwr2.ForeColor = System.Drawing.Color.White;
            this.lblPwr2.Image = null;
            this.lblPwr2.Location = new System.Drawing.Point(59, 107);
            this.lblPwr2.Name = "lblPwr2";
            this.lblPwr2.Size = new System.Drawing.Size(25, 13);
            this.lblPwr2.TabIndex = 16;
            this.lblPwr2.Text = "1.5";
            // 
            // lblPwr1
            // 
            this.lblPwr1.AutoSize = true;
            this.lblPwr1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblPwr1.ForeColor = System.Drawing.Color.White;
            this.lblPwr1.Image = null;
            this.lblPwr1.Location = new System.Drawing.Point(2, 107);
            this.lblPwr1.Name = "lblPwr1";
            this.lblPwr1.Size = new System.Drawing.Size(25, 13);
            this.lblPwr1.TabIndex = 15;
            this.lblPwr1.Text = "1.0";
            // 
            // labelTS10
            // 
            this.labelTS10.AutoSize = true;
            this.labelTS10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS10.ForeColor = System.Drawing.Color.White;
            this.labelTS10.Image = null;
            this.labelTS10.Location = new System.Drawing.Point(90, 8);
            this.labelTS10.Name = "labelTS10";
            this.labelTS10.Size = new System.Drawing.Size(60, 13);
            this.labelTS10.TabIndex = 11;
            this.labelTS10.Text = "Sig/PWR";
            // 
            // labelTS9
            // 
            this.labelTS9.AutoSize = true;
            this.labelTS9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS9.ForeColor = System.Drawing.Color.White;
            this.labelTS9.Image = null;
            this.labelTS9.Location = new System.Drawing.Point(199, 28);
            this.labelTS9.Name = "labelTS9";
            this.labelTS9.Size = new System.Drawing.Size(43, 13);
            this.labelTS9.TabIndex = 10;
            this.labelTS9.Text = "+60dB";
            // 
            // labelTS8
            // 
            this.labelTS8.AutoSize = true;
            this.labelTS8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS8.ForeColor = System.Drawing.Color.White;
            this.labelTS8.Image = null;
            this.labelTS8.Location = new System.Drawing.Point(172, 28);
            this.labelTS8.Name = "labelTS8";
            this.labelTS8.Size = new System.Drawing.Size(28, 13);
            this.labelTS8.TabIndex = 9;
            this.labelTS8.Text = "+40";
            // 
            // labelTS7
            // 
            this.labelTS7.AutoSize = true;
            this.labelTS7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS7.ForeColor = System.Drawing.Color.White;
            this.labelTS7.Image = null;
            this.labelTS7.Location = new System.Drawing.Point(144, 28);
            this.labelTS7.Name = "labelTS7";
            this.labelTS7.Size = new System.Drawing.Size(28, 13);
            this.labelTS7.TabIndex = 8;
            this.labelTS7.Text = "+20";
            // 
            // labelTS5
            // 
            this.labelTS5.AutoSize = true;
            this.labelTS5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS5.ForeColor = System.Drawing.Color.White;
            this.labelTS5.Image = null;
            this.labelTS5.Location = new System.Drawing.Point(113, 28);
            this.labelTS5.Name = "labelTS5";
            this.labelTS5.Size = new System.Drawing.Size(14, 13);
            this.labelTS5.TabIndex = 6;
            this.labelTS5.Text = "9";
            // 
            // labelTS4
            // 
            this.labelTS4.AutoSize = true;
            this.labelTS4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS4.ForeColor = System.Drawing.Color.White;
            this.labelTS4.Image = null;
            this.labelTS4.Location = new System.Drawing.Point(77, 28);
            this.labelTS4.Name = "labelTS4";
            this.labelTS4.Size = new System.Drawing.Size(14, 13);
            this.labelTS4.TabIndex = 5;
            this.labelTS4.Text = "5";
            // 
            // labelTS3
            // 
            this.labelTS3.AutoSize = true;
            this.labelTS3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS3.ForeColor = System.Drawing.Color.White;
            this.labelTS3.Image = null;
            this.labelTS3.Location = new System.Drawing.Point(46, 28);
            this.labelTS3.Name = "labelTS3";
            this.labelTS3.Size = new System.Drawing.Size(14, 13);
            this.labelTS3.TabIndex = 4;
            this.labelTS3.Text = "3";
            // 
            // labelTS2
            // 
            this.labelTS2.AutoSize = true;
            this.labelTS2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS2.ForeColor = System.Drawing.Color.White;
            this.labelTS2.Image = null;
            this.labelTS2.Location = new System.Drawing.Point(21, 28);
            this.labelTS2.Name = "labelTS2";
            this.labelTS2.Size = new System.Drawing.Size(14, 13);
            this.labelTS2.TabIndex = 3;
            this.labelTS2.Text = "1";
            // 
            // labelTS1
            // 
            this.labelTS1.AutoSize = true;
            this.labelTS1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS1.ForeColor = System.Drawing.Color.White;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(5, 28);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(14, 13);
            this.labelTS1.TabIndex = 2;
            this.labelTS1.Text = "0";
            // 
            // labelTS16
            // 
            this.labelTS16.AutoSize = true;
            this.labelTS16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTS16.ForeColor = System.Drawing.Color.White;
            this.labelTS16.Image = null;
            this.labelTS16.Location = new System.Drawing.Point(28, 107);
            this.labelTS16.Name = "labelTS16";
            this.labelTS16.Size = new System.Drawing.Size(25, 13);
            this.labelTS16.TabIndex = 34;
            this.labelTS16.Text = "1.2";
            // 
            // SMeter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.labelTS16);
            this.Controls.Add(this.labelTS13);
            this.Controls.Add(this.labelTS11);
            this.Controls.Add(this.labelTS6);
            this.Controls.Add(this.labelTS12);
            this.Controls.Add(this.labelTS14);
            this.Controls.Add(this.labelTS15);
            this.Controls.Add(this.labelTS17);
            this.Controls.Add(this.progressSignal);
            this.Controls.Add(this.lblPwr5);
            this.Controls.Add(this.lblPwr4);
            this.Controls.Add(this.lblPwr3);
            this.Controls.Add(this.lblPwr2);
            this.Controls.Add(this.lblPwr1);
            this.Controls.Add(this.labelTS10);
            this.Controls.Add(this.labelTS9);
            this.Controls.Add(this.labelTS8);
            this.Controls.Add(this.labelTS7);
            this.Controls.Add(this.labelTS5);
            this.Controls.Add(this.labelTS4);
            this.Controls.Add(this.labelTS3);
            this.Controls.Add(this.labelTS2);
            this.Controls.Add(this.labelTS1);
            this.Controls.Add(this.progressSWR);
            this.MaximumSize = new System.Drawing.Size(240, 133);
            this.MinimumSize = new System.Drawing.Size(240, 133);
            this.Name = "SMeter";
            this.Size = new System.Drawing.Size(240, 133);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ProgressODoom.ProgressBarEx progressSWR;
        private System.Windows.Forms.LabelTS labelTS1;
        private System.Windows.Forms.LabelTS labelTS2;
        private System.Windows.Forms.LabelTS labelTS3;
        private System.Windows.Forms.LabelTS labelTS4;
        private System.Windows.Forms.LabelTS labelTS5;
        private System.Windows.Forms.LabelTS labelTS7;
        private System.Windows.Forms.LabelTS labelTS8;
        private System.Windows.Forms.LabelTS labelTS9;
        private System.Windows.Forms.LabelTS labelTS10;
        private System.Windows.Forms.LabelTS lblPwr1;
        private System.Windows.Forms.LabelTS lblPwr2;
        private System.Windows.Forms.LabelTS lblPwr3;
        private System.Windows.Forms.LabelTS lblPwr4;
        private System.Windows.Forms.LabelTS lblPwr5;
        private ProgressODoom.ProgressBarEx progressSignal;
        public ProgressODoom.FruityLoopsProgressPainter fruityLoopsProgressPainterSigLine;
        private ProgressODoom.FruityLoopsBackgroundPainter fruityLoopsBackgroundPainterSigLine;
        public ProgressODoom.FruityLoopsProgressPainter fruityLoopsProgressPainterSWRLine;
        private ProgressODoom.FruityLoopsBackgroundPainter fruityLoopsBackgroundPainterSWRLine;
        private LabelTS labelTS6;
        private LabelTS labelTS12;
        private LabelTS labelTS14;
        private LabelTS labelTS15;
        private LabelTS labelTS17;
        private LabelTS labelTS11;
        private LabelTS labelTS13;
        private LabelTS labelTS16;
    }
}
