using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace BlottoBeats
{
    public class MainForm : Form
    {
        private Point dragPos;
        private int size = 80;

        public MainForm()
        {
            this.Text = "Blotto Beats";
            this.Size = new Size(size * 5, size + 1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MouseDown += this.mouseDown;
            this.MouseUp += this.mouseUp;
            this.MouseWheel += this.mouseWheel;
            this.Paint += this.paint;
            this.DoubleBuffered = true;
            this.BackColor = Color.Turquoise;
            this.TransparencyKey = Color.Turquoise;
        }

        private void mouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
                size--;
            else if (e.Delta > 0)
                size++;

            this.Size = new Size(size * 5, size + 1);

            Invalidate();
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            this.MouseMove -= this.mouseMove;
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            this.MouseMove += this.mouseMove;
            dragPos = e.Location;
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            this.Location = new Point(Cursor.Position.X - dragPos.X, Cursor.Position.Y - dragPos.Y);
        }

        private void paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Pen lightOutline = new Pen(Color.FromArgb(130, 130, 130), size / 40);
            lightOutline.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
            Pen lightOutline2 = new Pen(Color.FromArgb(130, 130, 130), size / 40);
            lightOutline2.Alignment = System.Drawing.Drawing2D.PenAlignment.Outset;
            SolidBrush medGrey = new SolidBrush(Color.FromArgb(90, 90, 90));
            SolidBrush darkGrey = new SolidBrush(Color.FromArgb(50, 50, 50));
            SolidBrush paleBlue = new SolidBrush(Color.FromArgb(75, 108, 124));

            for (int i = 0; i < 6; i++)
            {
                Point[] button = new Point[4];
                button[0] = new Point((size / 2) + ((size / 2) + (size / 8)) * i, size - (size / 8));
                button[1] = new Point(size + ((size / 2) + (size / 8)) * i, (size / 2));
                button[2] = new Point(size + (size / 2) + (size / 8) + ((size / 2) + (size / 8)) * i, (size / 2));
                button[3] = new Point((size / 2) + (size / 2) + (size / 8) + ((size / 2) + (size / 8)) * i, size - (size / 8));

                g.FillPolygon(darkGrey, button);
                g.DrawPolygon(lightOutline2, button);
            }

            g.FillEllipse(darkGrey, (int)(size * 4.5), (size / 8) - 1, (size / 2), (size / 2) - (size / 8));
            g.FillRectangle(darkGrey, (size / 2), (size / 8), (int)((size * 4.5) - (size / 4)), (size / 2) - (size / 8));

            g.FillRectangle(medGrey, (size / 2), (size / 8), 2 * size, (size / 2) - (size / 8));
            g.DrawRectangle(lightOutline, (size / 2), (size / 8), 2 * size, (size / 2) - (size / 8));

            Point[] slider = new Point[4];
            slider[0] = new Point((size / 2) + 2 * size, (size / 8));
            slider[1] = new Point((size / 2) + 2 * size + (size / 8), (size / 8) + ((size / 2) - (size / 8)) / 2);
            slider[2] = new Point((size / 2) + 2 * size, (size / 2));
            slider[3] = new Point((size / 2) + 2 * size - (size / 8), (size / 8) + ((size / 2) - (size / 8)) / 2);

            g.FillPolygon(paleBlue, slider);
            g.DrawPolygon(lightOutline, slider);

            g.FillEllipse(medGrey, 0, 0, size, size);
            g.DrawEllipse(lightOutline, 0, 0, size, size);
        }
    }
}