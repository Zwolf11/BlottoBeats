using System;
using System.Collections.Generic;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;

namespace BlottoBeats
{
    public delegate void ClickedEventHandler(object sender, MouseEventArgs e);

    public class Button
    {
        public Point[] ClickLocation
        {
            get
            {
                List<Point> points = new List<Point>();
                foreach (Point pt in button)
                    points.Add(new Point(pt.X + loc.X, pt.Y + loc.Y));
                return points.ToArray();
            }
        }

        public Point[] ImgLocation
        {
            get
            {
                if (img == null || img.Count == 0)
                    return null;
                List<Point> points = new List<Point>();
                foreach (Point pt in img)
                    points.Add(new Point(pt.X + loc.X, pt.Y + loc.Y));
                return points.ToArray();
            }
        }

        public Point loc;
        public List<Point> button;
        public SolidBrush inside;
        public Pen stroke;
        public event ClickedEventHandler Clicked;
        public List<Point> img;

        public Button(List<Point> button, Point loc, SolidBrush inside, Pen stroke, List<Point> img)
        {
            this.button = button;
            this.loc = loc;
            this.inside = inside;
            this.stroke = stroke;
            this.img = img;
        }

        public virtual void onClicked(MouseEventArgs e) { if (Clicked != null) Clicked(this, e); }
    }
}
