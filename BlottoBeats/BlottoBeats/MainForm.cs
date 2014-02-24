using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace BlottoBeats
{
    public class MainForm : Form
    {
        private int size;
        private List<Button> buttons;
        private Button sliderButton;
        private Button playButton;
        private List<Point> playImg;
        private List<Point> pauseImg;
        private Point dragPos;
        private bool dragging;
        private bool playing;
        private double progress;
        private int score;

        private SolidBrush lightGrey;
        private SolidBrush medGrey;
        private SolidBrush darkGrey;
        private SolidBrush paleBlue;
        private SolidBrush paleRed;
        private SolidBrush paleGreen;
        private Pen lightInline;
        private Pen lightOutline;

        public MainForm()
        {
            this.Text = "Blotto Beats";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MouseDown += this.mouseDown;
            this.MouseUp += this.mouseUp;
            this.MouseWheel += this.mouseWheel;
            this.Paint += this.paint;
            this.DoubleBuffered = true;
            this.BackColor = Color.Turquoise;
            this.TransparencyKey = Color.Turquoise;

            size = 80;
            buttons = new List<Button>();
            dragging = false;
            playing = false;
            progress = 0;
            score = 0;

            lightGrey = new SolidBrush(Color.FromArgb(130, 130, 130));
            medGrey = new SolidBrush(Color.FromArgb(90, 90, 90));
            darkGrey = new SolidBrush(Color.FromArgb(50, 50, 50));
            paleBlue = new SolidBrush(Color.FromArgb(75, 108, 124));
            paleRed = new SolidBrush(Color.FromArgb(107, 49, 50));
            paleGreen = new SolidBrush(Color.FromArgb(77, 125, 74));
            lightInline = new Pen(Color.FromArgb(130, 130, 130));
            lightInline.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
            lightOutline = new Pen(Color.FromArgb(130, 130, 130));
            lightOutline.Alignment = System.Drawing.Drawing2D.PenAlignment.Outset;

            initButtons();
        }

        private void initButtons()
        {
            buttons.Clear();
            this.Size = new Size(33 * size / 8, size);
            lightInline.Width = size / 40;
            lightOutline.Width = size / 40;

            List<Point> menuButton = new List<Point>();
            menuButton.Add(new Point(0, 3 * size / 8));
            menuButton.Add(new Point(size / 4, 0));
            menuButton.Add(new Point(3 * size / 4, 0));
            menuButton.Add(new Point(size / 2, 3 * size / 8));

            List<Point> backImg = new List<Point>();
            Button backButton = new Button(menuButton, new Point(3 * size / 4, size / 2), darkGrey, lightOutline, backImg);
            backButton.Clicked += backClicked;
            buttons.Add(backButton);

            List<Point> nextImg = new List<Point>();
            Button nextButton = new Button(menuButton, new Point(size / 2 + 3 * size / 4, size / 2), darkGrey, lightOutline, nextImg);
            nextButton.Clicked += nextClicked;
            buttons.Add(nextButton);

            List<Point> upvoteImg = new List<Point>();
            Button upvoteButton = new Button(menuButton, new Point(2 * size / 2 + 3 * size / 4, size / 2), paleGreen, lightOutline, upvoteImg);
            upvoteButton.Clicked += upvoteClicked;
            buttons.Add(upvoteButton);

            List<Point> downvoteImg = new List<Point>();
            Button downvoteButton = new Button(menuButton, new Point(3 * size / 2 + 3 * size / 4, size / 2), paleRed, lightOutline, downvoteImg);
            downvoteButton.Clicked += downvoteClicked;
            buttons.Add(downvoteButton);

            List<Point> redditImg = new List<Point>();
            Button redditButton = new Button(menuButton, new Point(4 * size / 2 + 3 * size / 4, size / 2), darkGrey, lightOutline, redditImg);
            redditButton.Clicked += redditClicked;
            buttons.Add(redditButton);

            List<Point> settingsImg = new List<Point>();
            settingsImg.Add(new Point(size / 4, size / 8));
            settingsImg.Add(new Point(size / 2, size / 8));
            settingsImg.Add(new Point((int)(3 * size / 8), 5 * size / 16));
            Button settingsButton = new Button(menuButton, new Point(5 * size / 2 + 3 * size / 4, size / 2), darkGrey, lightOutline, settingsImg);
            settingsButton.Clicked += settingsClicked;
            buttons.Add(settingsButton);

            List<Point> slider = new List<Point>();
            slider.Add(new Point(0, 0));
            slider.Add(new Point(size / 8, 3 * size / 16));
            slider.Add(new Point(0, 3 * size / 8));
            slider.Add(new Point(-size / 8, 3 * size / 16));
            sliderButton = new Button(slider, new Point((int)(progress * (257 * size / 64 - size) + size), size / 8), paleBlue, lightInline, null);
            sliderButton.Clicked += sliderClicked;
            buttons.Add(sliderButton);

            List<Point> play = new List<Point>();
            for (int i = 0; i < 128; i++)
                play.Add(new Point((int)(size / 2 + Math.Cos((1.0 * i / 128) * (2 * Math.PI)) * (size / 2)), (int)(size / 2 + Math.Sin((1.0 * i / 128) * (2 * Math.PI)) * (size / 2))));
            playImg = new List<Point>();
            playImg.Add(new Point(6 * size / 16, size / 4));
            playImg.Add(new Point(3 * size / 4, size / 2));
            playImg.Add(new Point(6 * size / 16, 3 * size / 4));
            pauseImg = new List<Point>();
            pauseImg.Add(new Point(3 * size / 4, 3 * size / 4));
            pauseImg.Add(new Point(size / 4, 3 * size / 4));
            pauseImg.Add(new Point(size / 4, size / 4));
            pauseImg.Add(new Point(7 * size / 16, size / 4));
            pauseImg.Add(new Point(7 * size / 16, 3 * size / 4));
            pauseImg.Add(new Point(9 * size / 16, 3 * size / 4));
            pauseImg.Add(new Point(9 * size / 16, size / 4));
            pauseImg.Add(new Point(3 * size / 4, size / 4));
            List<Point> playButtonImg = playImg;
            if (playing) playButtonImg = pauseImg;
            playButton = new Button(play, new Point(0, 0), medGrey, lightInline, playButtonImg);
            playButton.Clicked += this.playClicked;
            buttons.Add(playButton);

            List<Point> minimize = new List<Point>();
            minimize.Add(new Point(0, 0));
            minimize.Add(new Point(size / 4, 0));
            minimize.Add(new Point(size / 4, size / 8));
            minimize.Add(new Point(0, size / 8));
            List<Point> minimizeImg = new List<Point>();
            Button minimizeButton = new Button(minimize, new Point(7 * size / 2, 0), medGrey, null, minimizeImg);
            minimizeButton.Clicked += minimizeClicked;
            buttons.Add(minimizeButton);

            List<Point> exit = new List<Point>();
            exit.Add(new Point(0, 0));
            exit.Add(new Point(size / 4, 0));
            exit.Add(new Point(size / 4, size / 8));
            exit.Add(new Point(0, size / 8));
            List<Point> exitImg = new List<Point>();
            Button exitButton = new Button(exit, new Point(15 * size / 4, 0), paleRed, null, exitImg);
            exitButton.Clicked += exitClicked;
            buttons.Add(exitButton);
        }

        private void playClicked(object sender, EventArgs e)
        {
            if (playing) playButton.img = playImg;
            else playButton.img = pauseImg;

            playing = !playing;

            Invalidate();
        }

        private void backClicked(object sender, EventArgs e)
        {
            
        }

        private void nextClicked(object sender, EventArgs e)
        {

        }

        private void upvoteClicked(object sender, EventArgs e)
        {
            score = score == 1 ? 0 : 1;
            Invalidate();
        }

        private void downvoteClicked(object sender, EventArgs e)
        {
            score = score == -1 ? 0 : -1;
            Invalidate();
        }

        private void redditClicked(object sender, EventArgs e)
        {

        }

        private void settingsClicked(object sender, EventArgs e)
        {

        }

        private void minimizeClicked(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void exitClicked(object sender, EventArgs e)
        {
            this.Close();
        }

        private void sliderClicked(object sender, EventArgs e)
        {
            this.MouseMove += dragSlider;
            this.MouseUp += undragSlider;
            this.MouseMove -= this.mouseMove;
        }

        private void dragSlider(object sender, MouseEventArgs e)
        {
            this.MouseUp -= this.mouseUp;
            sliderButton.loc.X = e.X;

            if (e.X < size)
                sliderButton.loc.X = size;
            else if (e.X >= 257 * size / 64)
                sliderButton.loc.X = 257 * size / 64;

            Invalidate();
        }

        private void undragSlider(object sender, MouseEventArgs e)
        {
            progress = 1.0 * (sliderButton.loc.X - size) / (257 * size / 64 - size);
            this.MouseMove -= dragSlider;
            this.MouseUp -= undragSlider;
            this.MouseUp += this.mouseUp;
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            this.MouseMove -= this.mouseMove;

            if(!dragging)
                foreach (Button button in buttons)
                    if (pointInPolygon(e.Location, button.ClickLocation))
                    {
                        button.onClicked(e);
                        break;
                    }

            dragging = false;
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            this.MouseMove += this.mouseMove;
            dragPos = e.Location;
            if(pointInPolygon(e.Location, sliderButton.ClickLocation))
                sliderButton.onClicked(e);
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
                this.Location = new Point(Cursor.Position.X - dragPos.X, Cursor.Position.Y - dragPos.Y);
            else
                if(Math.Sqrt(Math.Pow(e.X - dragPos.X, 2) + Math.Pow(e.Y - dragPos.Y, 2)) > 10)
                    dragging = true;
        }

        private void mouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                size++;
            else if (e.Delta < 0 && size > 80)
                size--;

            initButtons();
            Invalidate();
        }

        private bool pointInPolygon(Point p, Point[] poly)
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

        private void paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.FillEllipse(darkGrey, 31 * size / 8, size / 8 - 1, size / 4, 3 * size / 8);
            g.FillRectangle(darkGrey, 3 * size / 4, size / 8, 13 * size / 4, 3 * size / 8);

            SolidBrush fillProgress = medGrey;
            if (score < 0) fillProgress = paleRed;
            else if (score > 0) fillProgress = paleGreen;

            g.FillRectangle(fillProgress, 3 * size / 4, size / 8, sliderButton.loc.X - 3 * size / 4, 3 * size / 8);
            g.DrawRectangle(lightInline, 3 * size / 4, size / 8, sliderButton.loc.X - 3 * size / 4, 3 * size / 8);

            foreach(Button button in buttons)
            {
                g.FillPolygon(button.inside, button.ClickLocation);
                if(button.stroke != null)
                    g.DrawPolygon(button.stroke, button.ClickLocation);
                if(button.ImgLocation != null)
                    g.FillPolygon(lightGrey, button.ImgLocation);
            }
        }
    }
}