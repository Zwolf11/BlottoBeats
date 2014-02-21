using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace BlottoBeats
{
    public class MainForm : Form
    {
        private Point dragPos;
        private int size;
        private List<Button> buttons;
        private Button sliderButton;
        private int score;

        public MainForm()
        {
            this.Text = "Blotto Beats";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MouseDown += this.mouseDown;
            this.MouseUp += this.mouseUp;
            this.Paint += this.paint;
            this.DoubleBuffered = true;
            this.BackColor = Color.Turquoise;
            this.TransparencyKey = Color.Turquoise;

            size = 80;
            this.Size = new Size(size * 5, size + 1);
            buttons = new List<Button>();
            score = 0;

            initButtons();
        }

        private void initButtons()
        {
            SolidBrush medGrey = new SolidBrush(Color.FromArgb(90, 90, 90));
            SolidBrush darkGrey = new SolidBrush(Color.FromArgb(50, 50, 50));
            SolidBrush paleBlue = new SolidBrush(Color.FromArgb(75, 108, 124));

            Pen lightOutline = new Pen(Color.FromArgb(130, 130, 130), size / 40);
            lightOutline.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
            Pen lightOutline2 = new Pen(Color.FromArgb(130, 130, 130), size / 40);
            lightOutline2.Alignment = System.Drawing.Drawing2D.PenAlignment.Outset;

            List<Point> menuButton = new List<Point>();
            menuButton.Add(new Point(0, 3 * size / 8));
            menuButton.Add(new Point(size / 4, 0));
            menuButton.Add(new Point(3 * size / 4, 0));
            menuButton.Add(new Point(size / 2, 3 * size / 8));

            List<Point> backImg = new List<Point>();
            Button backButton = new Button(menuButton, new Point(3 * size / 4, size / 2), darkGrey, lightOutline2, backImg);
            backButton.Clicked += backClicked;
            buttons.Add(backButton);

            List<Point> nextImg = new List<Point>();
            Button nextButton = new Button(menuButton, new Point(size / 2 + 3 * size / 4, size / 2), darkGrey, lightOutline2, nextImg);
            nextButton.Clicked += nextClicked;
            buttons.Add(nextButton);

            List<Point> upvoteImg = new List<Point>();
            Button upvoteButton = new Button(menuButton, new Point(2 * size / 2 + 3 * size / 4, size / 2), new SolidBrush(Color.FromArgb(77, 125, 74)), lightOutline2, upvoteImg);
            upvoteButton.Clicked += upvoteClicked;
            buttons.Add(upvoteButton);

            List<Point> downvoteImg = new List<Point>();
            Button downvoteButton = new Button(menuButton, new Point(3 * size / 2 + 3 * size / 4, size / 2), new SolidBrush(Color.FromArgb(107, 49, 50)), lightOutline2, downvoteImg);
            downvoteButton.Clicked += downvoteClicked;
            buttons.Add(downvoteButton);

            List<Point> redditImg = new List<Point>();
            Button redditButton = new Button(menuButton, new Point(4 * size / 2 + 3 * size / 4, size / 2), darkGrey, lightOutline2, redditImg);
            redditButton.Clicked += redditClicked;
            buttons.Add(redditButton);

            List<Point> settingsImg = new List<Point>();
            settingsImg.Add(new Point(size / 4, size / 8));
            settingsImg.Add(new Point(size / 2, size / 8));
            settingsImg.Add(new Point((int)(3 * size / 8), 5 * size / 16));
            Button settingsButton = new Button(menuButton, new Point(5 * size / 2 + 3 * size / 4, size / 2), darkGrey, lightOutline2, settingsImg);
            settingsButton.Clicked += settingsClicked;
            buttons.Add(settingsButton);

            List<Point> slider = new List<Point>();
            slider.Add(new Point(0, 0));
            slider.Add(new Point(size / 8, 3 * size / 16));
            slider.Add(new Point(0, 3 * size / 8));
            slider.Add(new Point(-size / 8, 3 * size / 16));
            sliderButton = new Button(slider, new Point(size * 2, size / 8), paleBlue, lightOutline, null);
            sliderButton.Clicked += sliderClicked;
            buttons.Add(sliderButton);

            List<Point> play = new List<Point>();
            for (int i = 0; i < 64; i++)
                play.Add(new Point((int)(size / 2 + Math.Cos((1.0 * i / 64) * (2 * Math.PI)) * (size / 2)), (int)(size / 2 + Math.Sin((1.0 * i / 64) * (2 * Math.PI)) * (size / 2))));
            List<Point> playImg = new List<Point>();
            Button playButton = new Button(play, new Point(0, 0), medGrey, lightOutline, playImg);
            playButton.Clicked += this.playClicked;
            buttons.Add(playButton);
        }

        private bool PointInPolygon(Point p, Point[] poly)
        {
            Point p1, p2;
            bool inside = false;

            if (poly.Length < 3)
                return inside;

            Point oldPoint = new Point(poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            for (int i = 0; i < poly.Length; i++)
            {
                Point newPoint = new Point(poly[i].X, poly[i].Y);

                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X) && ((long)p.Y - (long)p1.Y) * (long)(p2.X - p1.X) < ((long)p2.Y - (long)p1.Y) * (long)(p.X - p1.X))
                    inside = !inside;

                oldPoint = newPoint;
            }

            return inside;
        }

        private void playClicked(object sender, EventArgs e)
        {

        }

        private void backClicked(object sender, EventArgs e)
        {
            
        }

        private void nextClicked(object sender, EventArgs e)
        {

        }

        private void upvoteClicked(object sender, EventArgs e)
        {
            if (score == 1)
                score = 0;
            else
                score = 1;

            Invalidate();
        }

        private void downvoteClicked(object sender, EventArgs e)
        {
            if (score == -1)
                score = 0;
            else
                score = -1;

            Invalidate();
        }

        private void redditClicked(object sender, EventArgs e)
        {

        }

        private void settingsClicked(object sender, EventArgs e)
        {

        }

        private void sliderClicked(object sender, EventArgs e)
        {
            this.MouseMove += dragSlider;
            this.MouseUp += undragSlider;
            this.MouseMove -= this.mouseMove;
        }

        private void dragSlider(object sender, MouseEventArgs e)
        {
            sliderButton.loc.X = e.X;
            if (e.X < size)
                sliderButton.loc.X = size;
            else if (e.X >= 16 * size / 4)
                sliderButton.loc.X = 16 * size / 4 - 1;
            Invalidate();
        }

        private void undragSlider(object sender, MouseEventArgs e)
        {
            this.MouseMove -= dragSlider;
            this.MouseUp -= undragSlider;
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            this.MouseMove -= this.mouseMove;
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            this.MouseMove += this.mouseMove;
            dragPos = e.Location;
            foreach(Button button in buttons)
            {
                if(PointInPolygon(e.Location, button.ClickLocation))
                {
                    button.onClicked(e);
                    break;
                }
            }
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            this.Location = new Point(Cursor.Position.X - dragPos.X, Cursor.Position.Y - dragPos.Y);
        }

        private void paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 50)), 3 * size / 4, size / 8, 13 * size / 4, 3 * size / 8);
            SolidBrush inside = new SolidBrush(Color.FromArgb(90, 90, 90));
            if (score < 0)
                inside = new SolidBrush(Color.FromArgb(107, 49, 50));
            else if (score > 0)
                inside = new SolidBrush(Color.FromArgb(77, 125, 74));
            g.FillRectangle(inside, 3 * size / 4, size / 8, sliderButton.loc.X - 3 * size / 4, 3 * size / 8);
            Pen lightOutline = new Pen(Color.FromArgb(130, 130, 130), size / 40);
            lightOutline.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
            g.DrawRectangle(lightOutline, 3 * size / 4, size / 8, sliderButton.loc.X - 3 * size / 4, 3 * size / 8);

            foreach(Button button in buttons)
            {
                g.FillPolygon(button.inside, button.ClickLocation);
                g.DrawPolygon(button.stroke, button.ClickLocation);
                if(button.ImgLocation != null)
                    g.FillPolygon(new SolidBrush(Color.FromArgb(90, 90, 90)), button.ImgLocation);
            }
        }
    }
}