using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace PowerSDR.Invoke
{
    public partial class SMeter : UserControl
    {
        private float signal_m_MaxValue = 200.0f;
        private float signal_m_MinValue = 0.0f;
        private float signal_m_value = 0.0f;
        private float swr_m_MaxValue = 200.0f;
        private float swr_m_MinValue = 0.0f;
        private float swr_m_value = 0.0f;

        public SMeter()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            InitializeComponent();
        }

        public Single SignalMaxValue
        {
            get
            {
                return signal_m_MaxValue;
            }
            set
            {
                if ((signal_m_MaxValue != value)
                && (value > signal_m_MinValue))
                {
                    signal_m_MaxValue = value;
                    Refresh();
                }
            }
        }

        public Single swrMaxValue
        {
            get
            {
                return swr_m_MaxValue;
            }
            set
            {
                if ((swr_m_MaxValue != value)
                && (value > swr_m_MinValue))
                {
                    swr_m_MaxValue = value;
                    Refresh();
                }
            }
        }

        public Single SignalMinValue
        {
            get
            {
                return signal_m_MinValue;
            }
            set
            {
                if ((signal_m_MinValue != value)
                && (value < signal_m_MaxValue))
                {
                    signal_m_MinValue = value;
                    Refresh();
                }
            }
        }

        public Single swrMinValue
        {
            get
            {
                return swr_m_MinValue;
            }
            set
            {
                if ((swr_m_MinValue != value)
                && (value < swr_m_MaxValue))
                {
                    swr_m_MinValue = value;
                    Refresh();
                }
            }
        }

        public Single SignalValue
        {
            get
            {
                return signal_m_value;
            }
            set
            {
                value *= 10;

                if (signal_m_value != value)
                {
                    signal_m_value = Math.Min(Math.Max(value, signal_m_MinValue), signal_m_MaxValue);
                }

                progressSignal.Value = (int)signal_m_value;
            }
        }

        public Single swrValue
        {
            get
            {
                return swr_m_value;
            }
            set
            {
                value *= 40;

                if (swr_m_value != value)
                {
                    swr_m_value = Math.Min(Math.Max(value, swr_m_MinValue), swr_m_MaxValue);
                }

                progressSWR.Value = (int)swr_m_value;
            }
        }

        public Color SignalLineColorLit
        {
            set
            {
                fruityLoopsProgressPainterSigLine.OnLit = Color.FromArgb(value.ToArgb());
                fruityLoopsProgressPainterSigLine.pOnLit = new Pen(new SolidBrush(fruityLoopsProgressPainterSigLine.OnLit), 1f);
                fruityLoopsProgressPainterSigLine.OnLitTop = Color.FromArgb(value.ToArgb());
                fruityLoopsProgressPainterSigLine.pOnLitTop = new Pen(new SolidBrush(fruityLoopsProgressPainterSigLine.OnLitTop), 1f);
                fruityLoopsProgressPainterSigLine.OnLitBot = Color.FromArgb(value.ToArgb());
                fruityLoopsProgressPainterSigLine.pOnLitBot = new Pen(new SolidBrush(fruityLoopsProgressPainterSigLine.OnLitBot), 1f);
            }
        }

        public Color SignalLineColorDrk
        {
            set
            {
                fruityLoopsProgressPainterSigLine.OnDrk = Color.FromArgb(value.ToArgb());
                fruityLoopsProgressPainterSigLine.pOnDrk = new Pen(new SolidBrush(fruityLoopsProgressPainterSigLine.OnDrk), 1f);
                fruityLoopsProgressPainterSigLine.OnDrkTop = Color.FromArgb(value.ToArgb());
                fruityLoopsProgressPainterSigLine.pOnDrkTop = new Pen(new SolidBrush(fruityLoopsProgressPainterSigLine.OnDrkTop), 1f);
                fruityLoopsProgressPainterSigLine.OnDrkBot = Color.FromArgb(value.ToArgb());
                fruityLoopsProgressPainterSigLine.pOnDrkBot = new Pen(new SolidBrush(fruityLoopsProgressPainterSigLine.OnDrkBot), 1f);

                fruityLoopsProgressPainterSWRLine.OnDrk = Color.FromArgb(value.ToArgb());
                fruityLoopsProgressPainterSWRLine.pOnDrk = new Pen(new SolidBrush(fruityLoopsProgressPainterSigLine.OnDrk), 1f);
                fruityLoopsProgressPainterSWRLine.OnDrkTop = Color.FromArgb(value.ToArgb());
                fruityLoopsProgressPainterSWRLine.pOnDrkTop = new Pen(new SolidBrush(fruityLoopsProgressPainterSigLine.OnDrkTop), 1f);
                fruityLoopsProgressPainterSWRLine.OnDrkBot = Color.FromArgb(value.ToArgb());
                fruityLoopsProgressPainterSWRLine.pOnDrkBot = new Pen(new SolidBrush(fruityLoopsProgressPainterSigLine.OnDrkBot), 1f);
            }
        }

        public Color SWRLineColor
        {
            set
            {
                progressSWR.ForeColor = value;
                fruityLoopsProgressPainterSWRLine.OnLit = Color.FromArgb(value.ToArgb());
                fruityLoopsProgressPainterSWRLine.pOnLit = new Pen(new SolidBrush(fruityLoopsProgressPainterSWRLine.OnLit), 1f);
            }
        }

        private void SMeter_Paint(object sender, PaintEventArgs e)
        {
            progressSignal.Value = (int)signal_m_value;
            progressSWR.Value = (int)swr_m_value;
        }
    }
}
