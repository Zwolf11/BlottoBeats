using BlottoBeats.Library.Authentication;
using BlottoBeats.Library.Networking;
using BlottoBeats.Library.SongData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace BlottoBeats.Client
{
    public class MainForm : Form
    {
        private int size;
        private List<Button> buttons;
        private List<Setting> settings;
        private Button sliderButton;
        private Button playButton;
        private List<Point> playImg;
        private List<Point> pauseImg;
        private Button playBarButton;
        private Button advSettingButton;
        private Button refreshRedditButton;
        private Point dragPos;
        private bool dragging;
        private bool playing;
        private double progress;
        private int score;
        private bool settingsDropped;
        private bool redditDropped;
        private double songLen;
        public int maxSongs;
        private Setting genre;
        private Setting tempo;
        private Setting seed;
        private System.Windows.Forms.Timer timer;
        private int songPos;
        public List<SongParameters> backlog;
        private List<SongParameters> redditSongs;
        private Generator generator;
        public BBServerConnection server;
        private MediaPlayer.MediaPlayer player;
        public AdvancedSettings settingsForm;
        public AccountManagement accountForm;
        public UserToken currentUser;

        public Font font;
        public Font smallFont;
        public SolidBrush lightColor;
        public SolidBrush medColor;
        public SolidBrush darkColor;
        public SolidBrush sliderColor;
        public SolidBrush downvoteColor;
        public SolidBrush upvoteColor;
        public SolidBrush textColor;
        public Pen lightInline;
        public Pen lightOutline;

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
            this.TopMost = Properties.Settings.Default.alwaysOnTop;

            size = 80;
            buttons = new List<Button>();
            settings = new List<Setting>();
            dragging = false;
            playing = false;
            progress = 0;
            score = 0;
            settingsDropped = false;
            songLen = 0;
            songPos = -1;
            backlog = new List<SongParameters>();
            redditSongs = new List<SongParameters>();
            generator = new Generator();
            Console.WriteLine("Ip is: " + Properties.Settings.Default.lastIP);
            server = new BBServerConnection(Properties.Settings.Default.lastIP, 3000);
            player = new MediaPlayer.MediaPlayer();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 10;
            timer.Tick += this.tick;

            lightColor = new SolidBrush(Properties.Settings.Default.lightColor);
            medColor = new SolidBrush(Properties.Settings.Default.medColor);
            darkColor = new SolidBrush(Properties.Settings.Default.darkColor);
            sliderColor = new SolidBrush(Properties.Settings.Default.sliderColor);
            downvoteColor = new SolidBrush(Properties.Settings.Default.downvoteColor);
            upvoteColor = new SolidBrush(Properties.Settings.Default.upvoteColor);
            textColor = new SolidBrush(Properties.Settings.Default.textColor);
            lightInline = new Pen(lightColor);
            lightInline.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
            lightOutline = new Pen(lightColor);
            lightOutline.Alignment = System.Drawing.Drawing2D.PenAlignment.Outset;

            genre = new DropDownSetting(0, "Genre", this, new string[]{"Generic", "Classical", "Twelve-tone", "Jazz"}, size);
            tempo = new TextBoxSetting(1, "Tempo", this, 60, 200, size);
            seed = new TextBoxSetting(2, "Seed", this, int.MinValue, int.MaxValue, size);
            settings.Add(genre);
            settings.Add(tempo);
            settings.Add(seed);

            foreach (Setting setting in settings)
                setting.setVisible(settingsDropped);

            initButtons();

            settingsForm = new AdvancedSettings(this);
            accountForm = new AccountManagement(this);

            Thread refreshRed = new Thread(new ThreadStart(refreshReddit));
            refreshRed.Start();
            //refreshReddit();

            if (Properties.Settings.Default.username == "null")
            {
                accountForm.ShowDialog();
            }
            else
            {
                Thread startThread = new Thread(new ThreadStart(startUp));
                startThread.Start();
                //startThread.Join();
                
            }
            //refreshRed.Join();
        }

        private void startUp()
        {
            UserToken tempToken = new UserToken(Properties.Settings.Default.username, Properties.Settings.Default.expires, Properties.Settings.Default.token);
            if (server.Test())
            {

                if (server.VerifyToken(tempToken) == true)
                {
                    currentUser = tempToken;
                    accountForm.user.Text = currentUser.username;
                    MessageBox.Show("You have successfully logged in!", "Login Success");
                }
                else
                {
                    MessageBox.Show("Login has expired. Please log in again");
                    accountForm.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Server is not connected. Try again later", "Auto-login failed");
            }
        }

        public void initButtons()
        {
            buttons.Clear();
            if (settingsDropped || redditDropped) this.Size = new Size(33 * size / 8, 23 * size / 8);
            else this.Size = new Size(33 * size / 8, size);
            font = new Font("Arial", 3 * size / 20);
            smallFont = new Font("Arial", 3 * size / 35);
            lightInline.Width = size / 40;
            lightOutline.Width = size / 40;

            List<Point> playBar = new List<Point>();
            playBar.Add(new Point(0, 0));
            playBar.Add(new Point(13 * size / 4, 0));
            playBar.Add(new Point(13 * size / 4, 3 * size / 8));
            playBar.Add(new Point(0, 3 * size / 8));
            playBarButton = new Button(playBar, new Point(3 * size / 4, size / 8), darkColor, null, null);
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
            Button backButton = new Button(menuButton, new Point(3 * size / 4, size / 2), darkColor, lightOutline, backImg);
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
            Button nextButton = new Button(menuButton, new Point(size / 2 + 3 * size / 4, size / 2), darkColor, lightOutline, nextImg);
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
            Button upvoteButton = new Button(menuButton, new Point(2 * size / 2 + 3 * size / 4, size / 2), upvoteColor, lightOutline, upvoteImg);
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
            Button downvoteButton = new Button(menuButton, new Point(3 * size / 2 + 3 * size / 4, size / 2), downvoteColor, lightOutline, downvoteImg);
            downvoteButton.Clicked += downvoteClicked;
            buttons.Add(downvoteButton);

            List<Point> redditImg = new List<Point>();
            Button redditButton = new Button(menuButton, new Point(4 * size / 2 + 3 * size / 4, size / 2), darkColor, lightOutline, redditImg);
            redditButton.Clicked += redditClicked;
            buttons.Add(redditButton);

            List<Point> settingsImg = new List<Point>();
            settingsImg.Add(new Point(size / 4, size / 8));
            settingsImg.Add(new Point(size / 2, size / 8));
            settingsImg.Add(new Point((int)(3 * size / 8), 5 * size / 16));
            Button settingsButton = new Button(menuButton, new Point(5 * size / 2 + 3 * size / 4, size / 2), darkColor, lightOutline, settingsImg);
            settingsButton.Clicked += settingsClicked;
            buttons.Add(settingsButton);

            List<Point> slider = new List<Point>();
            slider.Add(new Point(0, 0));
            slider.Add(new Point(size / 8, 3 * size / 16));
            slider.Add(new Point(0, 3 * size / 8));
            slider.Add(new Point(-size / 8, 3 * size / 16));
            sliderButton = new Button(slider, new Point((int)(progress * (193 * size / 64) + size), size / 8), sliderColor, lightInline, null);
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
            else playButtonImg = playImg;
            playButton = new Button(play, new Point(0, 0), medColor, lightInline, playButtonImg);
            playButton.Clicked += this.playClicked;
            buttons.Add(playButton);

            List<Point> minimize = new List<Point>();
            minimize.Add(new Point(0, 0));
            minimize.Add(new Point(size / 4, 0));
            minimize.Add(new Point(size / 4, size / 8));
            minimize.Add(new Point(0, size / 8));
            List<Point> minimizeImg = new List<Point>();
            Button minimizeButton = new Button(minimize, new Point(7 * size / 2, 0), medColor, null, minimizeImg);
            minimizeButton.Clicked += minimizeClicked;
            buttons.Add(minimizeButton);

            List<Point> exit = new List<Point>();
            exit.Add(new Point(0, 0));
            exit.Add(new Point(size / 4, 0));
            exit.Add(new Point(size / 4, size / 8));
            exit.Add(new Point(0, size / 8));
            List<Point> exitImg = new List<Point>();
            Button exitButton = new Button(exit, new Point(15 * size / 4, 0), downvoteColor, null, exitImg);
            exitButton.Clicked += exitClicked;
            buttons.Add(exitButton);

            List<Point> buttonShape = new List<Point>();
            buttonShape.Add(new Point(0, 0));
            buttonShape.Add(new Point(size / 4, 0));
            buttonShape.Add(new Point(size / 4, size / 4));
            buttonShape.Add(new Point(0, size / 4));
            advSettingButton = new Button(buttonShape, new Point(27 * size / 8, 20 * size / 8), darkColor, lightOutline, null);
            advSettingButton.Clicked += advSettingClicked;

            refreshRedditButton = new Button(buttonShape, new Point(27 * size / 8, 20 * size / 8), darkColor, lightOutline, null);
            refreshRedditButton.Clicked += refreshRedditClicked;

            foreach (Setting setting in settings)
                setting.init(size);
        }

        private void playSong()
        {
            playButton.img = pauseImg;
            playing = true;
            player.CurrentPosition = progress * songLen;
            player.Play();
            timer.Start();
        }

        private void loadSong(bool nextSong)
        {
            stopSong();
            Thread sendSc = new Thread(new ThreadStart(sendScore));
            sendSc.Start();
            //sendScore();
            resetPlayBar();

            if (nextSong) songPos++;
            else if(songPos > 0) songPos--;

            if (songPos < 0 || songPos > backlog.Count)
                return;

            foreach (Setting setting in settings)
                if (setting.isChecked())
                    setting.randomize();

            if (songPos >= backlog.Count)
            {
                backlog.Add(new SongParameters(seed.getIntValue(), tempo.getIntValue(), genre.getStringValue()));
                if(backlog.Count > Properties.Settings.Default.maxSongs)
                {
                    backlog.RemoveAt(0);
                    songPos--;
                }
            }
            else
            {
                genre.setValue(backlog[songPos].genre);
                tempo.setValue(backlog[songPos].tempo + "");
                seed.setValue(backlog[songPos].seed + "");
            }

            player.Open("");
            songLen = generator.generate(backlog[songPos]);
            player.Open(@"C:\BlottoBeats\temp.mid");
            playSong();
            //sendSc.Join();
        }

        private void stopSong()
        {
            playButton.img = playImg;
            playing = false;
            player.Stop();
            timer.Stop();
        }

        private void resetPlayBar()
        {
            progress = 0;
            sliderButton.loc.X = size;
            score = 0;
        }

        private void sendScore()
        {
			if (server.Test()) {
				if (score > 0) server.SendRequest(new BBRequest(backlog[songPos], true, currentUser));
				else if (score < 0) server.SendRequest(new BBRequest(backlog[songPos], false, currentUser));
			}
        }

        private void refreshReddit()
        {
            if (server.Test())
            {
                BBResponse response = server.SendRequest(new BBRequest(12));

                if (response.responseType is BBResponse.SongList)
                {
                    BBResponse.SongList songList = (BBResponse.SongList)response.responseType;
                    redditSongs = songList.songs;
                }
            }

            Invalidate();
        }

        private void playClicked(object sender, MouseEventArgs e)
        {
            if (playing)
                stopSong();
            else
            {
                if (songPos >= 0) playSong();
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
                if (playing) playSong();
                Invalidate();
            }
        }

        private void backClicked(object sender, MouseEventArgs e)
        {
            loadSong(false);
        }

        private void nextClicked(object sender, MouseEventArgs e)
        {
            loadSong(true);
        }

        private void upvoteClicked(object sender, MouseEventArgs e)
        {
            if(songPos >= 0) score = score == 1 ? 0 : 1;
            Invalidate();
        }

        private void downvoteClicked(object sender, MouseEventArgs e)
        {
            if (songPos >= 0) score = score == -1 ? 0 : -1;
            Invalidate();
        }

        private void redditClicked(object sender, MouseEventArgs e)
        {
            redditDropped = !redditDropped;

            if (settingsDropped)
            {
                settingsDropped = false;
                foreach (Setting setting in settings)
                    setting.setVisible(settingsDropped);
            }

            if (redditDropped) this.Size = new Size(33 * size / 8, 23 * size / 8);
            else this.Size = new Size(33 * size / 8, size);

            Invalidate();
        }

        private void settingsClicked(object sender, MouseEventArgs e)
        {
            settingsDropped = !settingsDropped;

            if (redditDropped)
            {
                redditDropped = false;
            }

            if (settingsDropped) this.Size = new Size(33 * size / 8, 23 * size / 8);
            else this.Size = new Size(33 * size / 8, size);

            foreach (Setting setting in settings)
                setting.setVisible(settingsDropped);

            Invalidate();
        }

        private void advSettingClicked(object sender, MouseEventArgs e)
        {
            if (settingsForm == null)
                settingsForm = new AdvancedSettings(this);

            settingsForm.ShowDialog();
        }

        private void refreshRedditClicked(object sender, MouseEventArgs e)
        {
            Thread refreshRed = new Thread(new ThreadStart(refreshReddit));
            refreshRed.Start();
            //refreshReddit();
            Invalidate();
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
            if (playing)
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
            if (playing) playSong();
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            this.MouseMove -= this.mouseMove;

            if (!dragging)
            {
                for (int i = buttons.Count - 1; i >= 0; i--)
                    if (pointInPolygon(e.Location, buttons[i].ClickLocation))
                    {
                        buttons[i].onClicked(e);
                        break;
                    }

                if(settingsDropped)
                    if (pointInPolygon(e.Location, advSettingButton.ClickLocation))
                        advSettingButton.onClicked(e);

                if(redditDropped)
                {
                    if (pointInPolygon(e.Location, refreshRedditButton.ClickLocation))
                        refreshRedditButton.onClicked(e);
                    else
                    {
                        for (int i = 0; i < redditSongs.Count; i++)
                            if (e.Location.X >= 3 * size / 4 && e.Location.X < 3 * size / 4 + 3 * size && e.Location.Y >= 15 * size / 16 + i * smallFont.Size * 2 && e.Location.Y < 15 * size / 16 + (i + 1) * smallFont.Size * 2)
                            {
                                bool[] checks = new bool[settings.Count];
                                for (int j = 0; j < settings.Count; j++)
                                {
                                    checks[j] = settings[j].isChecked();
                                    settings[j].setChecked(false);
                                }

                                genre.setValue(redditSongs[i].genre);
                                tempo.setValue(redditSongs[i].tempo + "");
                                seed.setValue(redditSongs[i].seed + "");

                                loadSong(true);

                                for (int j = 0; j < settings.Count; j++)
                                    settings[j].setChecked(checks[j]);

                                break;
                            }
                    }
                }
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

            g.FillEllipse(darkColor, 31 * size / 8, size / 8 - 1, size / 4, 3 * size / 8);

            if (settingsDropped)
            {
                g.FillRectangle(medColor, 3 * size / 4, 7 * size / 8, 3 * size, 2 * size);
                g.DrawRectangle(lightInline, 3 * size / 4, 7 * size / 8, 3 * size, 2 * size);
                g.DrawString("Variable", font, lightColor, 13 * size / 16, 15 * size / 16);
                g.DrawString("Randomized?", font, lightColor, 15 * size / 4 - g.MeasureString("Randomized?", font).Width, 15 * size / 16);
                g.DrawString("Advanced Settings", font, lightColor, 13 * size / 16, 40 * size / 16);
                g.FillPolygon(advSettingButton.inside, advSettingButton.ClickLocation);
                g.DrawPolygon(advSettingButton.stroke, advSettingButton.ClickLocation);
            }
            else if(redditDropped)
            {
                g.FillRectangle(medColor, 3 * size / 4, 7 * size / 8, 3 * size, 2 * size);
                g.DrawRectangle(lightInline, 3 * size / 4, 7 * size / 8, 3 * size, 2 * size);

                if (redditSongs.Count > 0)
                    for (int i = 0; i < redditSongs.Count; i++)
                    {
                        String preString = "";
                        if (redditSongs[i].score >= 0) preString = "+";
                        g.DrawString(preString + redditSongs[i].score + " | Genre: " + redditSongs[i].genre + " | Tempo: " + redditSongs[i].tempo + " | Seed: " + redditSongs[i].seed, smallFont, lightColor, 13 * size / 16, 15 * size / 16 + i * smallFont.Size * 2);
                    }
                else
                    g.DrawString("Could not connect to server.", font, lightColor, 13 * size / 16, 15 * size / 16);

                g.FillPolygon(refreshRedditButton.inside, refreshRedditButton.ClickLocation);
                g.DrawPolygon(refreshRedditButton.stroke, refreshRedditButton.ClickLocation);
            }

            foreach(Button button in buttons)
            {
                g.FillPolygon(button.inside, button.ClickLocation);
                if(button.stroke != null)
                    g.DrawPolygon(button.stroke, button.ClickLocation);
                if(button.ImgLocation != null)
                    g.FillPolygon(lightColor, button.ImgLocation);

                if(button == playBarButton)
                {
                    SolidBrush fillProgress = medColor;
                    if (score < 0) fillProgress = downvoteColor;
                    else if (score > 0) fillProgress = upvoteColor;

                    g.FillRectangle(fillProgress, 3 * size / 4, size / 8, sliderButton.loc.X - 3 * size / 4, 3 * size / 8);
                    g.DrawRectangle(lightInline, 3 * size / 4, size / 8, sliderButton.loc.X - 3 * size / 4, 3 * size / 8);
                }
            }
        }
    }
}