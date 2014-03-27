using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using Networking;
using SongData;

namespace BlottoBeats
{
    public class MainForm : Form
    {
        public const int PLAYING = 1;
        public const int LOADING = 0;
        public const int PAUSED = -1;

        private int size;
        private List<Button> buttons;
        private List<Setting> settings;
        private Button sliderButton;
        private Button playButton;
        private List<Point> playImg;
        private List<Point> pauseImg;
        private List<Point> loadImg;
        private Button playBarButton;
        private Button advSettingButton;
        private Point dragPos;
        private bool dragging;
        private int playing;
        private bool songLoaded;
        private double progress;
        private int score;
        private bool menuDropped;
        private double songLen;
        private Setting genre;
        private Setting tempo;
        private Setting seed;
        private System.Windows.Forms.Timer timer;
        private int songPos;
        private List<SongParameters> backlog;
        private Generator generator;
        private BBServerConnection server;
        private MediaPlayer.MediaPlayer player;

        private Font font;
        private SolidBrush lightGrey;
        private SolidBrush medGrey;
        private SolidBrush darkGrey;
        private SolidBrush paleBlue;
        private SolidBrush paleRed;
        private SolidBrush paleGreen;
        private SolidBrush white;
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
            settings = new List<Setting>();
            dragging = false;
            playing = PAUSED;
            songLoaded = false;
            progress = 0;
            score = 0;
            menuDropped = false;
            songLen = 0;
            songPos = -1;
            backlog = new List<SongParameters>();
            generator = new Generator(this);
            server = new BBServerConnection("127.0.0.1", 3000);
            player = new MediaPlayer.MediaPlayer();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 10;
            timer.Tick += this.tick;

            lightGrey = new SolidBrush(Color.FromArgb(130, 130, 130));
            medGrey = new SolidBrush(Color.FromArgb(90, 90, 90));
            darkGrey = new SolidBrush(Color.FromArgb(50, 50, 50));
            paleBlue = new SolidBrush(Color.FromArgb(75, 108, 124));
            paleRed = new SolidBrush(Color.FromArgb(107, 49, 50));
            paleGreen = new SolidBrush(Color.FromArgb(77, 125, 74));
            white = new SolidBrush(Color.FromArgb(255, 255, 255));
            lightInline = new Pen(Color.FromArgb(130, 130, 130));
            lightInline.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
            lightOutline = new Pen(Color.FromArgb(130, 130, 130));
            lightOutline.Alignment = System.Drawing.Drawing2D.PenAlignment.Outset;

            genre = new DropDownSetting(0, "Genre", this, new string[]{"Chord Progression", "Classical"}, size);
            tempo = new TextBoxSetting(1, "Tempo", this, 60, 200, size);
            seed = new TextBoxSetting(2, "Seed", this, int.MinValue, int.MaxValue, size);
            settings.Add(genre);
            settings.Add(tempo);
            settings.Add(seed);

            foreach (Setting setting in settings)
                setting.setVisible(menuDropped);

            initButtons();
        }

        private void initButtons()
        {
            buttons.Clear();
            if (menuDropped) this.Size = new Size(33 * size / 8, 23 * size / 8);
            else this.Size = new Size(33 * size / 8, size);
            font = new Font("Arial", 3 * size / 20);
            lightInline.Width = size / 40;
            lightOutline.Width = size / 40;

            List<Point> playBar = new List<Point>();
            playBar.Add(new Point(0, 0));
            playBar.Add(new Point(13 * size / 4, 0));
            playBar.Add(new Point(13 * size / 4, 3 * size / 8));
            playBar.Add(new Point(0, 3 * size / 8));
            playBarButton = new Button(playBar, new Point(3 * size / 4, size / 8), darkGrey, null, null);
            playBarButton.Clicked += playBarClicked;
            buttons.Add(playBarButton);

            List<Point> menuButton = new List<Point>();
            menuButton.Add(new Point(0, 3 * size / 8));
            menuButton.Add(new Point(size / 4, 0));
            menuButton.Add(new Point(3 * size / 4, 0));
            menuButton.Add(new Point(size / 2, 3 * size / 8));

            List<Point> backImg = new List<Point>();
            backImg.Add(new Point(9 * size / 32 + size / 16, size / 16));
            backImg.Add(new Point(10 * size / 32 + size / 16, size / 16));
            backImg.Add(new Point(10 * size / 32 + size / 16, 3 * size / 16));
            backImg.Add(new Point(12 * size / 32 + size / 16, size / 16));
            backImg.Add(new Point(12 * size / 32 + size / 16, 5 * size / 16));
            backImg.Add(new Point(10 * size / 32 + size / 16, 3 * size / 16));
            backImg.Add(new Point(10 * size / 32 + size / 16, 5 * size / 16));
            backImg.Add(new Point(9 * size / 32 + size / 16, 5 * size / 16));
            Button backButton = new Button(menuButton, new Point(3 * size / 4, size / 2), darkGrey, lightOutline, backImg);
            backButton.Clicked += backClicked;
            buttons.Add(backButton);

            List<Point> nextImg = new List<Point>();
            nextImg.Add(new Point(11 * size / 32, size / 16));
            nextImg.Add(new Point(13 * size / 32, 3 * size / 16));
            nextImg.Add(new Point(13 * size / 32, size / 16));
            nextImg.Add(new Point(14 * size / 32, size / 16));
            nextImg.Add(new Point(14 * size / 32, 5 * size / 16));
            nextImg.Add(new Point(13 * size / 32, 5 * size / 16));
            nextImg.Add(new Point(13 * size / 32, 3 * size / 16));
            nextImg.Add(new Point(11 * size / 32, 5 * size / 16));
            Button nextButton = new Button(menuButton, new Point(size / 2 + 3 * size / 4, size / 2), darkGrey, lightOutline, nextImg);
            nextButton.Clicked += nextClicked;
            buttons.Add(nextButton);

            List<Point> upvoteImg = new List<Point>();
            upvoteImg.Add(new Point(9 * size / 32, 5 * size / 32));
            upvoteImg.Add(new Point(9 * size / 32, 9 * size / 32));
            upvoteImg.Add(new Point(7 * size / 32, 9 * size / 32));
            upvoteImg.Add(new Point(7 * size / 32, 5 * size / 32));
            upvoteImg.Add(new Point(10 * size / 32, 5 * size / 32));
            upvoteImg.Add(new Point(11 * size / 32, 3 * size / 32));
            upvoteImg.Add(new Point(11 * size / 32, 2 * size / 32));
            upvoteImg.Add(new Point(12 * size / 32, 2 * size / 32));
            upvoteImg.Add(new Point(12 * size / 32, 5 * size / 32));
            upvoteImg.Add(new Point(15 * size / 32, 5 * size / 32));
            upvoteImg.Add(new Point(15 * size / 32, 8 * size / 32));
            upvoteImg.Add(new Point(13 * size / 32, 10 * size / 32));
            upvoteImg.Add(new Point(10 * size / 32, 10 * size / 32));
            upvoteImg.Add(new Point(9 * size / 32, 9 * size / 32));
            Button upvoteButton = new Button(menuButton, new Point(2 * size / 2 + 3 * size / 4, size / 2), paleGreen, lightOutline, upvoteImg);
            upvoteButton.Clicked += upvoteClicked;
            buttons.Add(upvoteButton);

            List<Point> downvoteImg = new List<Point>();
            downvoteImg.Add(new Point(15 * size / 32, 7 * size / 32));
            downvoteImg.Add(new Point(15 * size / 32, 3 * size / 32));
            downvoteImg.Add(new Point(17 * size / 32, 3 * size / 32));
            downvoteImg.Add(new Point(17 * size / 32, 7 * size / 32));
            downvoteImg.Add(new Point(14 * size / 32, 7 * size / 32));
            downvoteImg.Add(new Point(13 * size / 32, 9 * size / 32));
            downvoteImg.Add(new Point(13 * size / 32, 10 * size / 32));
            downvoteImg.Add(new Point(12 * size / 32, 10 * size / 32));
            downvoteImg.Add(new Point(12 * size / 32, 7 * size / 32));
            downvoteImg.Add(new Point(9 * size / 32, 7 * size / 32));
            downvoteImg.Add(new Point(9 * size / 32, 4 * size / 32));
            downvoteImg.Add(new Point(11 * size / 32, 2 * size / 32));
            downvoteImg.Add(new Point(14 * size / 32, 2 * size / 32));
            downvoteImg.Add(new Point(15 * size / 32, 3 * size / 32));
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

            List<Point> buttonShape = new List<Point>();
            buttonShape.Add(new Point(0, 0));
            buttonShape.Add(new Point(3 * size / 4, 0));
            buttonShape.Add(new Point(3 * size / 4, size / 4));
            buttonShape.Add(new Point(0, size / 4));

            List<Point> advSetImg = new List<Point>();
            advSettingButton = new Button(buttonShape, new Point(22 * size / 8, 20 * size / 8), darkGrey, lightOutline, advSetImg);
            advSettingButton.Clicked += advSettingClicked;
            buttons.Add(advSettingButton);

            List<Point> slider = new List<Point>();
            slider.Add(new Point(0, 0));
            slider.Add(new Point(size / 8, 3 * size / 16));
            slider.Add(new Point(0, 3 * size / 8));
            slider.Add(new Point(-size / 8, 3 * size / 16));
            sliderButton = new Button(slider, new Point((int)(progress * (193 * size / 64) + size), size / 8), paleBlue, lightInline, null);
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
            loadImg = new List<Point>();
            List<Point> playButtonImg = playImg;
            if (playing == PLAYING) playButtonImg = pauseImg;
            else if (playing == LOADING) playButtonImg = loadImg;
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

            foreach (Setting setting in settings)
                setting.init(size);
        }

        public void songDoneLoading(double songLen)
        {
            this.songLen = songLen;
            player.Open(@"C:\BlottoBeats\temp.mid");
            songLoaded = true;
            playSong();
        }

        private void playSong()
        {
            playButton.img = pauseImg;
            playing = PLAYING;
            player.CurrentPosition = progress * songLen;
            player.Play();
            timer.Start();
        }

        private void loadSong(bool nextSong)
        {
            if (nextSong) songPos++;
            else if(songPos > 0) songPos--;

            if (songPos < 0 || songPos > backlog.Count)
                return;

            playButton.img = loadImg;
            playing = LOADING;
            foreach (Setting setting in settings)
                if (setting.isChecked())
                    setting.randomize();

            player.Open("");
            if(songPos >= backlog.Count)
                backlog.Add(new SongParameters(seed.getIntValue(), tempo.getIntValue(), genre.getStringValue()));
            else
            {
                genre.setValue(backlog[songPos].genre);
                tempo.setValue(backlog[songPos].tempo + "");
                seed.setValue(backlog[songPos].seed + "");
            }
            generator.generate(backlog[songPos]);
        }

        private void stopSong()
        {
            playButton.img = playImg;
            playing = PAUSED;
            player.Stop();
            timer.Stop();
        }

        private void resetPlayBar()
        {
            progress = 0;
            sliderButton.loc.X = size;
            score = 0;
            songLoaded = false;
        }

        private void sendScore()
        {
            if (score > 0) new Thread(() => server.SendRequest(new BBRequest(backlog[songPos], true, null))).Start();
            else if (score < 0) new Thread(() => server.SendRequest(new BBRequest(backlog[songPos], false, null))).Start();
        }

        private void playClicked(object sender, MouseEventArgs e)
        {
            if (playing == PLAYING)
                stopSong();
            else if (playing == PAUSED)
            {
                if (songLoaded) playSong();
                else loadSong(true);
            }
            Invalidate();
        }

        private void playBarClicked(object sender, MouseEventArgs e)
        {
            if (e.X >= size)
            {
                sliderButton.loc.X = e.X;
                progress = 1.0 * (sliderButton.loc.X - size) / (193 * size / 64);
                if (playing == PLAYING) playSong();
                Invalidate();
            }
        }

        private void backClicked(object sender, MouseEventArgs e)
        {
            sendScore();
            resetPlayBar();
            loadSong(false);
        }

        private void nextClicked(object sender, MouseEventArgs e)
        {
            sendScore();
            resetPlayBar();
            loadSong(true);
        }

        private void upvoteClicked(object sender, MouseEventArgs e)
        {
            score = score == 1 ? 0 : 1;
            Invalidate();
        }

        private void downvoteClicked(object sender, MouseEventArgs e)
        {
            score = score == -1 ? 0 : -1;
            Invalidate();
        }

        private void redditClicked(object sender, MouseEventArgs e)
        {

        }

        private void settingsClicked(object sender, MouseEventArgs e)
        {
            if (menuDropped) this.Size = new Size(33 * size / 8, size);
            else this.Size = new Size(33 * size / 8, 23 * size / 8);

            menuDropped = !menuDropped;
            foreach (Setting setting in settings)
                setting.setVisible(menuDropped);

            Invalidate();
        }

        private void advSettingClicked(object sender, MouseEventArgs e)
        {

        }

        private void minimizeClicked(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void exitClicked(object sender, MouseEventArgs e)
        {
            sendScore();
            this.Close();
        }

        private void sliderClicked(object sender, MouseEventArgs e)
        {
            if (playing == PLAYING)
            {
                player.Stop();
                timer.Stop();
            }
            this.MouseMove += dragSlider;
            this.MouseUp += undragSlider;
            this.MouseMove -= this.mouseMove;
            this.MouseUp -= this.mouseUp;
        }

        private void dragSlider(object sender, MouseEventArgs e)
        {
            sliderButton.loc.X = e.X;

            if (e.X < size)
                sliderButton.loc.X = size;
            else if (e.X >= 257 * size / 64)
                sliderButton.loc.X = 257 * size / 64;

            Invalidate();
        }

        private void undragSlider(object sender, MouseEventArgs e)
        {
            progress = 1.0 * (sliderButton.loc.X - size) / (193 * size / 64);
            this.MouseMove -= dragSlider;
            this.MouseUp -= undragSlider;
            this.MouseUp += this.mouseUp;
            if (playing == PLAYING) playSong();
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            this.MouseMove -= this.mouseMove;

            if (!dragging)
                for (int i = buttons.Count - 1; i >= 0; i--)
                    if (pointInPolygon(e.Location, buttons[i].ClickLocation))
                    {
                        buttons[i].onClicked(e);
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

        private void tick(object sender, EventArgs e)
        {
            if (progress < 1)
            {
                progress = player.CurrentPosition / songLen;
                sliderButton.loc.X = (int)(progress * (193 * size / 64) + size);
            }
            else
            {
                sendScore();
                resetPlayBar();
                loadSong(true);
            }
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

            if (menuDropped)
            {
                g.FillRectangle(medGrey, 3 * size / 4, 7 * size / 8, 3 * size, 2 * size);
                g.DrawRectangle(lightInline, 3 * size / 4, 7 * size / 8, 3 * size, 2 * size);
                g.DrawString("Variable", font, lightGrey, 13 * size / 16, 15 * size / 16);
                g.DrawString("Randomized?", font, lightGrey, 15 * size / 4 - g.MeasureString("Randomized?", font).Width, 15 * size / 16);
                g.DrawString("Advanced Settings", font, lightGrey, 13 * size / 16, 40 * size / 16);
                g.FillPolygon(advSettingButton.inside, advSettingButton.ClickLocation);
                if (advSettingButton.stroke != null)
                    g.DrawPolygon(advSettingButton.stroke, advSettingButton.ClickLocation);
                if (advSettingButton.ImgLocation != null)
                    g.FillPolygon(lightGrey, advSettingButton.ImgLocation);
            }

            foreach(Button button in buttons)
            {
                g.FillPolygon(button.inside, button.ClickLocation);
                if(button.stroke != null)
                    g.DrawPolygon(button.stroke, button.ClickLocation);
                if(button.ImgLocation != null)
                    g.FillPolygon(lightGrey, button.ImgLocation);

                if(button == playBarButton)
                {
                    SolidBrush fillProgress = medGrey;
                    if (score < 0) fillProgress = paleRed;
                    else if (score > 0) fillProgress = paleGreen;

                    g.FillRectangle(fillProgress, 3 * size / 4, size / 8, sliderButton.loc.X - 3 * size / 4, 3 * size / 8);
                    g.DrawRectangle(lightInline, 3 * size / 4, size / 8, sliderButton.loc.X - 3 * size / 4, 3 * size / 8);
                }
            }
        }
    }
}