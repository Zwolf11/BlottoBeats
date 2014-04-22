using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BlottoBeats.Client
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

        public Point? ImgLocation
        {
            get
            {
                if (img == null)
                    return null;
                return new Point(loc.X + imgLoc.Value.X, loc.Y + imgLoc.Value.Y);
            }
        }

        public Point loc;
        public List<Point> button;
        public SolidBrush inside;
        public Pen stroke;
        public event ClickedEventHandler Clicked;
        public Bitmap img;
        public Point? imgLoc;

        public Button(List<Point> button, Point loc, SolidBrush inside, Pen stroke, Bitmap img, Point? imgLoc)
        {
            this.button = button;
            this.loc = loc;
            this.inside = inside;
            this.stroke = stroke;
            this.img = img;
            this.imgLoc = imgLoc;
        }

        public virtual void onClicked(MouseEventArgs e) { if (Clicked != null) Clicked(this, e); }
    }
}
