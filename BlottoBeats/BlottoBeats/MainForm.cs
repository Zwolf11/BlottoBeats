using BlottoBeats.Library.Authentication;
using BlottoBeats.Library.Networking;
using BlottoBeats.Library.SongData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace BlottoBeats.Client
{
    public class MainForm : Form
    {
        private int size;
        private List<Button> buttons;
        private List<Setting> settings;
        private Button sliderButton;
        private Button playButton;
        private Bitmap playImg;
        private Bitmap pauseImg;
        private Button playBarButton;
        private Button advSettingButton;
        private Button exportButton;
        private Button prevGenreButton;
        private Button nextGenreButton;
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
        private List<SongParameters>[] redditSongs;
        private Thread[] redditThreads;
        private Generator generator;
        public BBServerConnection server;
        private MediaPlayer.MediaPlayer player;
        public AdvancedSettings settingsForm;
        public AccountManagement accountForm;
        public UserToken currentUser;
        private String[] genres;
        private int curGenre;

        private Bitmap origPlayImg;
        private Bitmap origPauseImg;
        private Bitmap origBackImg;
        private Bitmap origNextImg;
        private Bitmap origUpvoteImg;
        private Bitmap origDownvoteImg;
        private Bitmap origRedditImg;
        private Bitmap origSettingsImg;
        private Bitmap origAdvSettingsImg;
        private Bitmap origExportImg;

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
            genres = new String[] { null, "Generic", "Classical", "Twelve-tone", "Jazz", "4-Chord Pop/Rock", null };
            redditSongs = new List<SongParameters>[genres.Length];
            redditThreads = new Thread[genres.Length];
            for (int i = 0; i < redditSongs.Length; i++)
            {
                redditSongs[i] = new List<SongParameters>();
                redditThreads[i] = new Thread(() => refreshReddit(i));
            }
            generator = new Generator();
            server = new BBServerConnection(Properties.Settings.Default.lastIP, 3000);
            player = new MediaPlayer.MediaPlayer();
            curGenre = 0;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 10;
            timer.Tick += this.tick;

            origPlayImg = new Bitmap("images/play.png");
            origPauseImg = new Bitmap("images/pause.png");
            origBackImg = new Bitmap("images/back.png");
            origNextImg = new Bitmap("images/next.png");
            origUpvoteImg = new Bitmap("images/upvote.png");
            origDownvoteImg = new Bitmap("images/downvote.png");
            origRedditImg = new Bitmap("images/reddit.png");
            origSettingsImg = new Bitmap("images/settings.png");
            origAdvSettingsImg = new Bitmap("images/advsettings.png");
            origExportImg = new Bitmap("images/export.png");

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

            genre = new DropDownSetting(0, "Genre", this, new string[] { "Generic", "Classical", "Twelve-tone", "Jazz", "4-Chord Pop/Rock" }, size);
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

            if (Properties.Settings.Default.username == "null") accountForm.ShowDialog();
            else new Thread(() => startUp()).Start();
        }

        public void createRedditThreads()
        {
            for (int i = 0; i < redditThreads.Length; i++)
            {
                int tempGenre = i;
                redditThreads[i] = new Thread(() => refreshReddit(tempGenre));
                redditThreads[i].Start();
            }
        }

        private void startUp()
        {
            UserToken tempToken = new UserToken(Properties.Settings.Default.username, Properties.Settings.Default.expires, Properties.Settings.Default.token);
            if (server.Test())
            {
                if (server.VerifyToken(tempToken))
                {
                    currentUser = tempToken;
                    accountForm.user.Text = currentUser.username;
                }
                else
                {
                    MessageBox.Show("Login has expired. Please log in again");
                    accountForm.ShowDialog();
                }
                createRedditThreads();
            }
            else
            {
                MessageBox.Show("Server is not connected. Try again later", "Auto-login failed");
            }
        }

        private Bitmap colorOverlay(Bitmap img, Color color)
        {
            var bitmapData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, img.PixelFormat);
            byte[] imageBytes = new byte[bitmapData.Stride * img.Height];
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, imageBytes, 0, imageBytes.Length);
            img.UnlockBits(bitmapData);
            int pixelSize = Image.GetPixelFormatSize(img.PixelFormat);

            int x = 0;
            int y = 0;
            var bitmap = new Bitmap(img.Width, img.Height);

            for (int i = 0; i < imageBytes.Length; i += pixelSize / 8)
            {
                var pixelData = new byte[4];
                Array.Copy(imageBytes, i, pixelData, 0, 4);
                var pxColor = Color.FromArgb(pixelData[0], pixelData[1], pixelData[2]);
                if (pixelData[3] != 0) bitmap.SetPixel(x, y, color);
                else bitmap.SetPixel(x, y, Color.Transparent);

                x++;
                if (x >= bitmap.Width)
                {
                    x = 0;
                    y++;
                }
            }

            return bitmap;
        }

        public void initButtons()
        {
            buttons.Clear();
            if (redditDropped) this.Size = new Size(33 * size / 8, 23 * size / 8);
            else if (settingsDropped) this.Size = new Size(33 * size / 8, 19 * size / 8);
            else this.Size = new Size(33 * size / 8, size);
            font = new Font("Arial", (float)(3.0 * size / 20));
            smallFont = new Font("Arial", (float)(3.0 * size / 35));
            lightInline.Width = size / 40;
            lightOutline.Width = size / 40;

            List<Point> playBar = new List<Point>();
            playBar.Add(new Point(0, 0));
            playBar.Add(new Point(13 * size / 4, 0));
            playBar.Add(new Point(13 * size / 4, 3 * size / 8));
            playBar.Add(new Point(0, 3 * size / 8));
            playBarButton = new Button(playBar, new Point(3 * size / 4, size / 8), darkColor, null, null, null);
            playBarButton.Clicked += playBarClicked;
            buttons.Add(playBarButton);

            List<Point> bottomButton = new List<Point>();
            bottomButton.Add(new Point(0, 3 * size / 8));
            bottomButton.Add(new Point(size / 4, 0));
            bottomButton.Add(new Point(3 * size / 4, 0));
            bottomButton.Add(new Point(size / 2, 3 * size / 8));

            Bitmap backImg = new Bitmap(origBackImg, new Size(size / 5, size / 5));
            Button backButton = new Button(bottomButton, new Point(3 * size / 4, size / 2), darkColor, lightOutline, colorOverlay(backImg, lightColor.Color), new Point(9 * size / 32, 3 * size / 32));
            backButton.Clicked += backClicked;
            buttons.Add(backButton);

            Bitmap nextImg = new Bitmap(origNextImg, new Size(size / 5, size / 5));
            Button nextButton = new Button(bottomButton, new Point(size / 2 + 3 * size / 4, size / 2), darkColor, lightOutline, colorOverlay(nextImg, lightColor.Color), new Point(9 * size / 32, 3 * size / 32));
            nextButton.Clicked += nextClicked;
            buttons.Add(nextButton);

            Bitmap upvoteImg = new Bitmap(origUpvoteImg, new Size(size / 4, size / 4));
            Button upvoteButton = new Button(bottomButton, new Point(size + 3 * size / 4, size / 2), upvoteColor, lightOutline, colorOverlay(upvoteImg, lightColor.Color), new Point(7 * size / 32, size / 16));
            upvoteButton.Clicked += upvoteClicked;
            buttons.Add(upvoteButton);

            Bitmap downvoteImg = new Bitmap(origDownvoteImg, new Size(size / 4, size / 4));
            Button downvoteButton = new Button(bottomButton, new Point(3 * size / 2 + 3 * size / 4, size / 2), downvoteColor, lightOutline, colorOverlay(downvoteImg, lightColor.Color), new Point(9 * size / 32, size / 16));
            downvoteButton.Clicked += downvoteClicked;
            buttons.Add(downvoteButton);

            Bitmap redditImg = new Bitmap(origRedditImg, new Size(size / 4, size / 4));
            Button redditButton = new Button(bottomButton, new Point(4 * size / 2 + 3 * size / 4, size / 2), darkColor, lightOutline, colorOverlay(redditImg, lightColor.Color), new Point(size / 4, size / 16));
            redditButton.Clicked += redditClicked;
            buttons.Add(redditButton);

            Bitmap settingsImg = new Bitmap(origSettingsImg, new Size(size / 4, size / 4));
            Button settingsButton = new Button(bottomButton, new Point(5 * size / 2 + 3 * size / 4, size / 2), darkColor, lightOutline, colorOverlay(settingsImg, lightColor.Color), new Point(size / 4, 3 * size / 32));
            settingsButton.Clicked += settingsClicked;
            buttons.Add(settingsButton);

            List<Point> slider = new List<Point>();
            slider.Add(new Point(0, 0));
            slider.Add(new Point(size / 8, 3 * size / 16));
            slider.Add(new Point(0, 3 * size / 8));
            slider.Add(new Point(-size / 8, 3 * size / 16));
            sliderButton = new Button(slider, new Point((int)(progress * (193 * size / 64) + size), size / 8), sliderColor, lightInline, null, null);
            sliderButton.Clicked += sliderClicked;
            buttons.Add(sliderButton);

            List<Point> play = new List<Point>();
            for (int i = 0; i < 128; i++)
                play.Add(new Point((int)(size / 2 + Math.Cos((1.0 * i / 128) * (2 * Math.PI)) * (size / 2)), (int)(size / 2 + Math.Sin((1.0 * i / 128) * (2 * Math.PI)) * (size / 2))));
            playImg = colorOverlay(new Bitmap(origPlayImg, new Size(size / 2, size / 2)), lightColor.Color);
            pauseImg = colorOverlay(new Bitmap(origPauseImg, new Size(size / 2, size / 2)), lightColor.Color);
            Bitmap playButtonImg = playImg;
            Point imgLoc = new Point(size / 3, size / 4);
            if (playing)
            {
                playButtonImg = pauseImg;
                imgLoc = new Point(size / 4, size / 4);
            }
            playButton = new Button(play, new Point(0, 0), medColor, lightInline, playButtonImg, imgLoc);
            playButton.Clicked += this.playClicked;
            buttons.Add(playButton);

            List<Point> menuButton = new List<Point>();
            menuButton.Add(new Point(0, 0));
            menuButton.Add(new Point(size / 4, 0));
            menuButton.Add(new Point(size / 4, size / 8));
            menuButton.Add(new Point(0, size / 8));

            Button minimizeButton = new Button(menuButton, new Point(7 * size / 2, 0), medColor, null, null, null);
            minimizeButton.Clicked += minimizeClicked;
            buttons.Add(minimizeButton);

            Button exitButton = new Button(menuButton, new Point(15 * size / 4, 0), downvoteColor, null, null, null);
            exitButton.Clicked += exitClicked;
            buttons.Add(exitButton);

            List<Point> buttonShape = new List<Point>();
            buttonShape.Add(new Point(0, 0));
            buttonShape.Add(new Point(size / 4, 0));
            buttonShape.Add(new Point(size / 4, size / 4));
            buttonShape.Add(new Point(0, size / 4));

            Bitmap advSettingsImg = new Bitmap(origAdvSettingsImg, new Size(size / 4, size / 4));
            advSettingButton = new Button(buttonShape, new Point(13 * size / 16, 30 * size / 32), darkColor, null, colorOverlay(advSettingsImg, lightColor.Color), new Point(0, 0));
            advSettingButton.Clicked += advSettingClicked;

            Bitmap exportImg = new Bitmap(origExportImg, new Size(size / 4, size / 4));
            exportButton = new Button(buttonShape, new Point(18 * size / 16, 30 * size / 32), darkColor, null, colorOverlay(exportImg, lightColor.Color), new Point(0, 0));
            exportButton.Clicked += exportClicked;

            List<Point> smallButtonShape = new List<Point>();
            smallButtonShape.Add(new Point(0, 0));
            smallButtonShape.Add(new Point(size / 8, 0));
            smallButtonShape.Add(new Point(size / 8, size / 8));
            smallButtonShape.Add(new Point(0, size / 8));
            
            prevGenreButton = new Button(smallButtonShape, new Point(13 * size / 16, 15 * size / 16), darkColor, null, null, null);
            prevGenreButton.Clicked += prevGenreClicked;

            nextGenreButton = new Button(smallButtonShape, new Point(57 * size / 16, 15 * size / 16), darkColor, null, null, null);
            nextGenreButton.Clicked += nextGenreClicked;

            foreach (Setting setting in settings)
                setting.init(size);
        }

        private void playSong()
        {
            playButton.img = pauseImg;
            playButton.imgLoc = new Point(size / 4, size / 4);
            playing = true;
            player.CurrentPosition = progress * songLen;
            player.Play();
            timer.Start();
        }

        private void loadSong(bool nextSong)
        {
            stopSong();
            if (songPos >= 0)
            {
                int tempScore = score;
                SongParameters tempSong = backlog[songPos];
                UserToken tempUser = currentUser;
                int tempGenre = curGenre;
                new Thread(() => sendScore(tempScore, tempSong, tempUser, tempGenre)).Start();
            }
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
            player.Open("temp.mid");
            playSong();
        }

        private void stopSong()
        {
            playButton.img = playImg;
            playButton.imgLoc = new Point(size / 3, size / 4);
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

        private void sendScore(int tempScore, SongParameters tempSong, UserToken tempUser, int tempGenre)
        {
            if (server.Test())
            {
                if (tempScore > 0) server.SendRequest(new BBRequest(tempSong, true, tempUser));
                else if (tempScore < 0) server.SendRequest(new BBRequest(tempSong, false, tempUser));

                createRedditThreads();

                if (!this.IsDisposed) this.Invoke((MethodInvoker)delegate { this.Invalidate(); });
            }
        }

        private void refreshReddit(int tempGenre)
        {
            if (!this.IsDisposed) this.Invoke((MethodInvoker) delegate { this.Invalidate(); });

            if (server.Test())
            {
                BBResponse response;
                if(tempGenre < genres.Length - 1)
                    response = server.SendRequest(new BBRequest(10, genres[tempGenre], null));
                else
                    response = server.SendRequest(new BBRequest(10, null, currentUser.username));

                if (response.responseType is BBResponse.SongList)
                {
                    BBResponse.SongList songList = (BBResponse.SongList)response.responseType;
                    redditSongs[tempGenre] = songList.songs;
                }
            }

            if(!this.IsDisposed) this.Invoke((MethodInvoker) delegate { this.Invalidate(); });
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

            if (settingsDropped) this.Size = new Size(33 * size / 8, 19 * size / 8);
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

        private void exportClicked(object sender, MouseEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "MIDI Files | *.mid";
            save.DefaultExt = "mid";

            if(save.ShowDialog() == System.Windows.Forms.DialogResult.OK && File.Exists("temp.mid"))
                File.Copy("temp.mid", save.FileName);
        }

        private void prevGenreClicked(object sender, MouseEventArgs e)
        {
            if (--curGenre < 0) curGenre = genres.Length - 1;
            Invalidate();
        }

        private void nextGenreClicked(object sender, MouseEventArgs e)
        {
            if (++curGenre >= genres.Length) curGenre = 0;
            Invalidate();
        }

        private void minimizeClicked(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void exitClicked(object sender, MouseEventArgs e)
        {
            stopSong();
            if (songPos >= 0) sendScore(score, backlog[songPos], currentUser, curGenre);
            for (int i = 0; i < redditThreads.Length; i++)
                redditThreads[i].Abort();
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

                if (settingsDropped)
                {
                    if (pointInPolygon(e.Location, advSettingButton.ClickLocation))
                        advSettingButton.onClicked(e);
                    else if(pointInPolygon(e.Location, exportButton.ClickLocation))
                        exportButton.onClicked(e);
                }

                if(redditDropped)
                {
                    if (pointInPolygon(e.Location, prevGenreButton.ClickLocation))
                        prevGenreButton.onClicked(e);
                    else if (pointInPolygon(e.Location, nextGenreButton.ClickLocation))
                        nextGenreButton.onClicked(e);
                    else
                    {
                        for (int i = 0; i < redditSongs[curGenre].Count; i++)
                            if (e.Location.X >= 3 * size / 4 && e.Location.X < 3 * size / 4 + 3 * size && e.Location.Y >= 15 * size / 16 + (i + 1) * smallFont.Size * 2 && e.Location.Y < 15 * size / 16 + (i + 2) * smallFont.Size * 2)
                            {
                                bool[] checks = new bool[settings.Count];
                                for (int j = 0; j < settings.Count; j++)
                                {
                                    checks[j] = settings[j].isChecked();
                                    settings[j].setChecked(false);
                                }

                                genre.setValue(redditSongs[curGenre][i].genre);
                                tempo.setValue(redditSongs[curGenre][i].tempo + "");
                                seed.setValue(redditSongs[curGenre][i].seed + "");

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
            progress = player.CurrentPosition / songLen;

            if (progress < 1)
                sliderButton.loc.X = (int)(progress * (193 * size / 64) + size);
            else
                loadSong(true);
            
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
                g.FillRectangle(medColor, 3 * size / 4, 7 * size / 8, 3 * size + 2, 12 * size / 8);
                g.DrawRectangle(lightInline, 3 * size / 4, 7 * size / 8, 3 * size + 2, 12 * size / 8);
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Far;
                g.DrawString("Randomized?", font, textColor, 119 * size / 32, 15 * size / 16, format);
                g.FillPolygon(advSettingButton.inside, advSettingButton.ClickLocation);
                g.DrawImage(advSettingButton.img, advSettingButton.ImgLocation.Value);
                g.FillPolygon(exportButton.inside, exportButton.ClickLocation);
                g.DrawImage(exportButton.img, exportButton.ImgLocation.Value);
            }
            else if(redditDropped)
            {
                g.FillRectangle(medColor, 3 * size / 4, 7 * size / 8, 3 * size + 2, 2 * size);
                g.DrawRectangle(lightInline, 3 * size / 4, 7 * size / 8, 3 * size + 2, 2 * size);

                StringFormat center = new StringFormat();
                center.Alignment = StringAlignment.Center;
                if (curGenre == 0)
                    g.DrawString("Top: All", smallFont, textColor, 37 * size / 16, 15 * size / 16, center);
                else if (curGenre == genres.Length - 1)
                    g.DrawString("Top: My Songs", smallFont, textColor, 37 * size / 16, 15 * size / 16, center);
                else
                    g.DrawString("Top: " + genres[curGenre], smallFont, textColor, 37 * size / 16, 15 * size / 16, center);

                g.FillPolygon(prevGenreButton.inside, prevGenreButton.ClickLocation);
                g.DrawString("<", smallFont, textColor, 13 * size / 16, 15 * size / 16);
                g.FillPolygon(nextGenreButton.inside, nextGenreButton.ClickLocation);
                g.DrawString(">", smallFont, textColor, 57 * size / 16, 15 * size / 16);

                if (redditSongs[curGenre].Count > 0)
                {
                    for (int i = 0; i < redditSongs[curGenre].Count; i++)
                    {
                        String preString = "";
                        if (redditSongs[curGenre][i].score >= 0) preString = "+";
                        String wholeString = preString + redditSongs[curGenre][i].score + " | Genre: " + redditSongs[curGenre][i].genre + " | Tempo: " + redditSongs[curGenre][i].tempo + " | Seed: " + redditSongs[curGenre][i].seed;
                        if (wholeString.Length > 50)
                            wholeString = wholeString.Substring(0, 50) + "...";
                        g.DrawString(wholeString, smallFont, textColor, 13 * size / 16, 15 * size / 16 + (i + 1) * smallFont.Size * 2);
                    }
                }
                else if (redditSongs[curGenre].Count <= 0 && redditThreads[curGenre].ThreadState == ThreadState.Running)
                    g.DrawString("Song list loading...", font, textColor, 13 * size / 16, 18 * size / 16);
            }

            foreach(Button button in buttons)
            {
                g.FillPolygon(button.inside, button.ClickLocation);
                if(button.stroke != null)
                    g.DrawPolygon(button.stroke, button.ClickLocation);
                if (button.img != null)
                    g.DrawImage(button.img, button.ImgLocation.Value);

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