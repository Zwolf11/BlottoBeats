using BlottoBeats.Library.Authentication;
using BlottoBeats.Library.Networking;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace BlottoBeats.Client
{
    public class AdvancedSettings : Form
    {
        private int size;
        private List<Button> buttons;
        private MainForm form;
        private bool dragging;
        private Point dragPos;
        public TextBox ip;
        public TextBox maxBacklog;
        public CheckBox onTop;

        private Font font;
        private Font smallFont;
        private Font smallestFont;

        public AdvancedSettings(MainForm form)
        {
            this.Text = "Blotto Beats";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MouseDown += this.mouseDown;
            this.MouseUp += this.mouseUp;
            this.Paint += this.paint;
            this.DoubleBuffered = true;
            this.BackColor = Color.Turquoise;
            this.TransparencyKey = Color.Turquoise;
            this.TopMost = Properties.Settings.Default.alwaysOnTop;

            size = 80;
            buttons = new List<Button>();
            this.form = form;
            this.Size = new Size(33 * size / 8, 5 * size / 2);
            ip = new TextBox();
            ip.Width = 2 * size + size / 4;
            ip.Font = new Font("Arial", 12);
            ip.Location = new Point(2 * size - size / 4, 9 * size / 8);
            ip.Text = Properties.Settings.Default.lastIP;
            this.Controls.Add(ip);
            maxBacklog = new TextBox();
            maxBacklog.Width = 2 * size - size / 4;
            maxBacklog.Font = new Font("Arial", 12);
            maxBacklog.Location = new Point(2 * size + size / 4, 13 * size / 8);
            maxBacklog.Text = Properties.Settings.Default.maxSongs + "";
            this.Controls.Add(maxBacklog);
            onTop = new CheckBox();
            onTop.BackColor = Color.Transparent;
            onTop.Location = new Point(15 * size / 4 + 3, 69 * size / 32 - 3);
            onTop.Checked = Properties.Settings.Default.alwaysOnTop;
            this.Controls.Add(onTop);

            font = new Font("Arial", 16, FontStyle.Bold);
            smallFont = new Font("Arial", 10, FontStyle.Bold);
            smallestFont = new Font("Arial", 7, FontStyle.Bold);

            initButtons();
        }

        private void initButtons()
        {
            buttons.Clear();

            List<Point> colorButton = new List<Point>();
            colorButton.Add(new Point(0, 0));
            colorButton.Add(new Point(3 * size / 16, 0));
            colorButton.Add(new Point(3 * size / 16, 3 * size / 16));
            colorButton.Add(new Point(0, 3 * size / 16));

            Button lightButton = new Button(colorButton, new Point(14 * size / 8, 5 * size / 8), form.lightColor, new Pen(Color.White), null, null);
            lightButton.Clicked += lightClicked;
            buttons.Add(lightButton);

            Button medButton = new Button(colorButton, new Point(16 * size / 8, 5 * size / 8), form.medColor, new Pen(Color.White), null, null);
            medButton.Clicked += medClicked;
            buttons.Add(medButton);

            Button darkButton = new Button(colorButton, new Point(18 * size / 8, 5 * size / 8), form.darkColor, new Pen(Color.White), null, null);
            darkButton.Clicked += darkClicked;
            buttons.Add(darkButton);

            Button upvoteButton = new Button(colorButton, new Point(20 * size / 8, 5 * size / 8), form.upvoteColor, new Pen(Color.White), null, null);
            upvoteButton.Clicked += upvoteClicked;
            buttons.Add(upvoteButton);

            Button downvoteButton = new Button(colorButton, new Point(22 * size / 8, 5 * size / 8), form.downvoteColor, new Pen(Color.White), null, null);
            downvoteButton.Clicked += downvoteClicked;
            buttons.Add(downvoteButton);

            Button sliderButton = new Button(colorButton, new Point(24 * size / 8, 5 * size / 8), form.sliderColor, new Pen(Color.White), null, null);
            sliderButton.Clicked += sliderClicked;
            buttons.Add(sliderButton);

            Button textButton = new Button(colorButton, new Point(26 * size / 8, 5 * size / 8), form.textColor, new Pen(Color.White), null, null);
            textButton.Clicked += textClicked;
            buttons.Add(textButton);

            List<Point> defButton = new List<Point>();
            defButton.Add(new Point(0, 0));
            defButton.Add(new Point(8 * size / 16, 0));
            defButton.Add(new Point(8 * size / 16, 3 * size / 16 + 1));
            defButton.Add(new Point(0, 3 * size / 16 + 1));

            Button defaultButton = new Button(defButton, new Point(28 * size / 8, 5 * size / 8), form.medColor, null, null, null);
            defaultButton.Clicked += defaultClicked;
            buttons.Add(defaultButton);

            List<Point> button = new List<Point>();
            button.Add(new Point(0, 0));
            button.Add(new Point(3 * size / 4, 0));
            button.Add(new Point(3 * size / 4, size / 4));
            button.Add(new Point(0, size / 4));

            Button OKButton = new Button(button, new Point(5 * size / 16, 17 * size / 8), form.medColor, null, null, null);
            OKButton.Clicked += OKClicked;
            buttons.Add(OKButton);

            Button cancelButton = new Button(button, new Point(9 * size / 8, 17 * size / 8), form.medColor, null, null, null);
            cancelButton.Clicked += cancelClicked;
            buttons.Add(cancelButton);

            Button loginButton = new Button(button, new Point(31 * size / 16, 17 * size / 8), form.medColor, null, null, null);
            loginButton.Clicked += loginClicked;
            buttons.Add(loginButton);

            List<Point> menuButton = new List<Point>();
            menuButton.Add(new Point(0, 0));
            menuButton.Add(new Point(size / 4, 0));
            menuButton.Add(new Point(size / 4, size / 8));
            menuButton.Add(new Point(0, size / 8));

            Button minimizeButton = new Button(menuButton, new Point(7 * size / 2, 0), form.medColor, null, null, null);
            minimizeButton.Clicked += minimizeClicked;
            buttons.Add(minimizeButton);

            Button exitButton = new Button(menuButton, new Point(15 * size / 4, 0), form.downvoteColor, null, null, null);
            exitButton.Clicked += exitClicked;
            buttons.Add(exitButton);
        }

        private void updateAllButtons()
        {
            initButtons();
            Invalidate();
            form.initButtons();
            form.Invalidate();
            form.accountForm.initButtons();
            form.accountForm.Invalidate();
        }

        private void lightClicked(object sender, MouseEventArgs e)
        {
            ColorDialog colorPicker = new ColorDialog();
            colorPicker.Color = Properties.Settings.Default.lightColor;
            if (colorPicker.ShowDialog() == DialogResult.OK)
            {
                SolidBrush newBrush = new SolidBrush(colorPicker.Color);
                Button b = (Button)sender;
                b.inside = newBrush;
                form.lightColor = newBrush;
                form.lightInline.Brush = newBrush;
                form.lightOutline.Brush = newBrush;
                updateAllButtons();
            }
        }

        private void medClicked(object sender, MouseEventArgs e)
        {
            ColorDialog colorPicker = new ColorDialog();
            colorPicker.Color = Properties.Settings.Default.medColor;
            if (colorPicker.ShowDialog() == DialogResult.OK)
            {
                SolidBrush newBrush = new SolidBrush(colorPicker.Color);
                Button b = (Button)sender;
                b.inside = newBrush;
                form.medColor = newBrush;
                updateAllButtons();
            }
        }

        private void darkClicked(object sender, MouseEventArgs e)
        {
            ColorDialog colorPicker = new ColorDialog();
            colorPicker.Color = Properties.Settings.Default.darkColor;
            if (colorPicker.ShowDialog() == DialogResult.OK)
            {
                SolidBrush newBrush = new SolidBrush(colorPicker.Color);
                Button b = (Button)sender;
                b.inside = newBrush;
                form.darkColor = newBrush;
                updateAllButtons();
            }
        }

        private void upvoteClicked(object sender, MouseEventArgs e)
        {
            ColorDialog colorPicker = new ColorDialog();
            colorPicker.Color = Properties.Settings.Default.upvoteColor;
            if (colorPicker.ShowDialog() == DialogResult.OK)
            {
                SolidBrush newBrush = new SolidBrush(colorPicker.Color);
                Button b = (Button)sender;
                b.inside = newBrush;
                form.upvoteColor = newBrush;
                updateAllButtons();
            }
        }

        private void downvoteClicked(object sender, MouseEventArgs e)
        {
            ColorDialog colorPicker = new ColorDialog();
            colorPicker.Color = Properties.Settings.Default.downvoteColor;
            if (colorPicker.ShowDialog() == DialogResult.OK)
            {
                SolidBrush newBrush = new SolidBrush(colorPicker.Color);
                Button b = (Button)sender;
                b.inside = newBrush;
                form.downvoteColor = newBrush;
                updateAllButtons();
            }
        }

        private void sliderClicked(object sender, MouseEventArgs e)
        {
            ColorDialog colorPicker = new ColorDialog();
            colorPicker.Color = Properties.Settings.Default.sliderColor;
            if (colorPicker.ShowDialog() == DialogResult.OK)
            {
                SolidBrush newBrush = new SolidBrush(colorPicker.Color);
                Button b = (Button)sender;
                b.inside = newBrush;
                form.sliderColor = newBrush;
                updateAllButtons();
            }
        }

        private void textClicked(object sender, MouseEventArgs e)
        {
            ColorDialog colorPicker = new ColorDialog();
            colorPicker.Color = Properties.Settings.Default.textColor;
            if (colorPicker.ShowDialog() == DialogResult.OK)
            {
                SolidBrush newBrush = new SolidBrush(colorPicker.Color);
                Button b = (Button)sender;
                b.inside = newBrush;
                form.textColor = newBrush;
                updateAllButtons();
            }
        }

        private void loginClicked(object sender, MouseEventArgs e)
        {
            form.accountForm.pass.Clear();
            if (form.currentUser != null)
                form.accountForm.user.Text = form.currentUser.username;
            form.accountForm.ShowDialog();
        }

        private void testNewIP(string newIP)
        {
            BBServerConnection newServer = new BBServerConnection(newIP, 3000);

            if (newServer.Test())
            {
                form.server.ip = this.ip.Text;
                Properties.Settings.Default.lastIP = form.server.ip;
            }
            else MessageBox.Show("Server is not connected. Try again later", "Change IP failed");
        }

        private void OKClicked(object sender, MouseEventArgs e)
        {
            string newIP = ip.Text;
            new Thread(() => testNewIP(newIP)).Start();

            try { Properties.Settings.Default.maxSongs = int.Parse(maxBacklog.Text); } catch (Exception) {}
            Properties.Settings.Default.alwaysOnTop = onTop.Checked;
            form.TopMost = Properties.Settings.Default.alwaysOnTop;
            Properties.Settings.Default.lightColor = buttons[0].inside.Color;
            Properties.Settings.Default.medColor = buttons[1].inside.Color;
            Properties.Settings.Default.darkColor = buttons[2].inside.Color;
            Properties.Settings.Default.upvoteColor = buttons[3].inside.Color;
            Properties.Settings.Default.downvoteColor = buttons[4].inside.Color;
            Properties.Settings.Default.sliderColor = buttons[5].inside.Color;
            Properties.Settings.Default.textColor = buttons[6].inside.Color;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void cancelClicked(object sender, MouseEventArgs e)
        {
            exitClicked(sender, e);
        }

        private void minimizeClicked(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void defaultClicked(object sender, MouseEventArgs e)
        {
            form.lightColor = new SolidBrush(Color.FromArgb(130, 130, 130));
            form.medColor = new SolidBrush(Color.FromArgb(90, 90, 90));
            form.darkColor = new SolidBrush(Color.FromArgb(50, 50, 50));
            form.upvoteColor = new SolidBrush(Color.FromArgb(77, 125, 74));
            form.downvoteColor = new SolidBrush(Color.FromArgb(107, 49, 50));
            form.sliderColor = new SolidBrush(Color.FromArgb(75, 108, 124));
            form.textColor = new SolidBrush(Color.FromArgb(255, 255, 255));
            form.lightInline.Brush = new SolidBrush(Color.FromArgb(130, 130, 130));
            form.lightOutline.Brush = new SolidBrush(Color.FromArgb(130, 130, 130));
            updateAllButtons();
        }

        private void exitClicked(object sender, MouseEventArgs e)
        {
            form.lightColor = new SolidBrush(Properties.Settings.Default.lightColor);
            form.medColor = new SolidBrush(Properties.Settings.Default.medColor);
            form.darkColor = new SolidBrush(Properties.Settings.Default.darkColor);
            form.upvoteColor = new SolidBrush(Properties.Settings.Default.upvoteColor);
            form.downvoteColor = new SolidBrush(Properties.Settings.Default.downvoteColor);
            form.sliderColor = new SolidBrush(Properties.Settings.Default.sliderColor);
            form.textColor = new SolidBrush(Properties.Settings.Default.textColor);
            form.lightInline.Brush = new SolidBrush(Properties.Settings.Default.lightColor);
            form.lightOutline.Brush = new SolidBrush(Properties.Settings.Default.lightColor);
            updateAllButtons();
            this.Close();
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
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
                this.Location = new Point(Cursor.Position.X - dragPos.X, Cursor.Position.Y - dragPos.Y);
            else
                if (Math.Sqrt(Math.Pow(e.X - dragPos.X, 2) + Math.Pow(e.Y - dragPos.Y, 2)) > 10)
                    dragging = true;
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

            g.FillRectangle(form.darkColor, size / 6, size / 8, 95 * size / 24, 19 * size / 8);
            g.DrawRectangle(form.lightInline, size / 6, size / 8, 95 * size / 24, 19 * size / 8);

            g.FillRectangle(form.medColor, size / 6, size / 8, 95 * size / 24, 3 * size / 8);
            g.DrawRectangle(form.lightInline, size / 6, size / 8, 95 * size / 24, 3 * size / 8);

            g.FillEllipse(form.medColor, 0, 0, size, size);
            g.DrawEllipse(form.lightInline, 0, 0, size, size);

            foreach (Button button in buttons)
            {
                g.FillPolygon(button.inside, button.ClickLocation);
                if (button.stroke != null)
                    g.DrawPolygon(button.stroke, button.ClickLocation);
            }

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Far;
            g.DrawString("Blotto Beats - Settings", font, form.darkColor, 65 * size / 16, size / 8 + size / 18, format);
            g.DrawString("Server IP:", font, form.lightColor, size / 4, 9 * size / 8);
            g.DrawString("Max Backlog:", font, form.lightColor, size / 4, 13 * size / 8);
            g.DrawString("Topmost:", smallFont, form.lightColor, 15 * size / 4, 69 * size / 32, format);
            g.DrawString("Theme:", smallFont, form.lightColor, size + 5, 5 * size / 8);
            g.DrawString("Default", smallestFont, form.textColor, 28 * size / 8 + 2, 5 * size / 8 + 2);
            StringFormat format2 = new StringFormat();
            format2.Alignment = StringAlignment.Center;
            g.DrawString("OK", smallFont, form.textColor, 11 * size / 16, 69 * size / 32, format2);
            g.DrawString("Cancel", smallFont, form.textColor, 24 * size / 16, 69 * size / 32, format2);
            g.DrawString("Account", smallFont, form.textColor, 37 * size / 16, 69 * size / 32, format2);
        }
    }
}
