using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace draw_growing_tree
{
    public partial class MathPlay : Form
    {
        float _step = 1.0f;
        uint startColor = 0xffd36f;

        public MathPlay()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            DoubleBuffered = true;

            Timer timer = new Timer()
            {
                Interval = 100,
                Enabled = true
            };
            timer.Tick += (sender, e) =>
                {
                    startColor = (uint)((double)startColor * 1.00001);
                    Invalidate();
                };
        }

        float ToRadian(float value)
        {
            return (float)(value * (Math.PI / 180));
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Add)
            {
                ++_step;
                if (_step >= 360.0f)
                    _step = 360.0f;
                Invalidate();
            }
            else if (keyData == Keys.Subtract)
            {
                --_step;
                if (_step < 1.0f)
                    _step = 1.0f;
                Invalidate();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            const float DotSize = 8.0f;

            e.Graphics.Clear(Color.Black);

            PointF ptMiddle = new PointF((float)ClientRectangle.Width / 2.0f, (float)ClientRectangle.Height / 2.0f);

            e.Graphics.FillEllipse(Brushes.White, ptMiddle.X - (DotSize / 2.0f), ptMiddle.Y - (DotSize / 2.0f), DotSize, DotSize);

            for (float i = 0; i < 360.0f*8; i += 1)
            {
                float sin = (float)Math.Sin(ToRadian(i));
                float cos = (float)Math.Cos(ToRadian(i));

                float x = ptMiddle.X + sin * (i/4);
                float y = ptMiddle.Y - cos * (i/4/_step);

                uint color = startColor;
                //Debug.WriteLine((uint)((double)color * 0.001 * i));
                color -= 0x010101*(uint)i;
                color |= 0xff000000;

                using (var brush = new SolidBrush(Color.FromArgb((int)color)))
                    e.Graphics.FillEllipse(brush, x - (DotSize / 2.0f), y - (DotSize / 2.0f), DotSize, DotSize);
            }
        }
    }
}
