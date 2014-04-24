using BlottoBeats.Library.Authentication;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace BlottoBeats.Client
{
    public class AccountManagement : Form
    {
        private int size;
        private List<Button> buttons;
        private MainForm form;
        private bool dragging;
        private Point dragPos;
        public TextBox user;
        public TextBox pass;
        public CheckBox remember;

        private Font font;
        private Font smallFont;

        public AccountManagement(MainForm form)
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
            user = new TextBox();
            user.Width = 2 * size + size / 4;
            user.Font = new Font("Arial", 12);
            user.Location = new Point(2 * size - size / 4, 9 * size / 8);
            if (Properties.Settings.Default.username != "null")
            {
                user.Text = Properties.Settings.Default.username;
            }
            this.Controls.Add(user);
            pass = new TextBox();
            pass.PasswordChar = '*';
            pass.Width = 2 * size + size / 4;
            pass.Font = new Font("Arial", 12);
            pass.Location = new Point(2 * size - size / 4, 13 * size / 8);
            this.Controls.Add(pass);
            remember = new CheckBox();
            remember.BackColor = Color.Transparent;
            remember.Location = new Point(15 * size / 4 + 3, 69 * size / 32 - 3);
            remember.Checked = Properties.Settings.Default.username != "null";
            this.Controls.Add(remember);

            font = new Font("Arial", 16, FontStyle.Bold);
            smallFont = new Font("Arial", 10, FontStyle.Bold);

            initButtons();
        }

        public void initButtons()
        {
            buttons.Clear();

            List<Point> button = new List<Point>();
            button.Add(new Point(0, 0));
            button.Add(new Point(3 * size / 4, 0));
            button.Add(new Point(3 * size / 4, size / 4));
            button.Add(new Point(0, size / 4));

            Button loginButton = new Button(button, new Point(5 * size / 16, 17 * size / 8), form.medColor, null, null, null);
            loginButton.Clicked += loginClicked;
            buttons.Add(loginButton);

            Button registerButton = new Button(button, new Point(9 * size / 8, 17 * size / 8), form.medColor, null, null, null);
            registerButton.Clicked += registerClicked;
            buttons.Add(registerButton);

            Button logoutButton = new Button(button, new Point(31 * size / 16, 17 * size / 8), form.medColor, null, null, null);
            logoutButton.Clicked += logoutClicked;
            buttons.Add(logoutButton);

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

        private void loginClicked(object sender, MouseEventArgs e)
        {
            UserToken token;

            if (form.server.Test())
            {
                token = form.server.Authenticate(new Credentials(user.Text, pass.Text), false);

                if (token == null)
                {
                    MessageBox.Show("Username/Password was incorrect. Please try again", "Login failed");
                }
                else
                {
                    form.currentUser = token;
                    if (this.remember.Checked == true)
                    {
                        Properties.Settings.Default.username = form.currentUser.username;
                        Properties.Settings.Default.expires = form.currentUser.expires;
                        Properties.Settings.Default.token = form.currentUser.token;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        Properties.Settings.Default.username = "null";
                        Properties.Settings.Default.token = "null";
                        Properties.Settings.Default.Save();
                    }

                    this.Close();
                    form.createRedditThreads();
                }
            }
            else
            {
                MessageBox.Show("Server is not connected. Try again later", "Login failed");

                this.Close();
            }
        }

        private void registerClicked(object sender, MouseEventArgs e)
        {
            UserToken token;

            if (form.server.Test())
            {
                token = form.server.Authenticate(new Credentials(user.Text, pass.Text), true);

                if (token == null)
                {
                    MessageBox.Show("Error occurred. Username might already be taken");
                    form.currentUser = token;
                }
                else
                {
                    form.currentUser = token;
                    if (this.remember.Checked == true)
                    {
                        Properties.Settings.Default.username = form.currentUser.username;
                        Properties.Settings.Default.expires = form.currentUser.expires;
                        Properties.Settings.Default.token = form.currentUser.token;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        Properties.Settings.Default.username = "null";
                        Properties.Settings.Default.token = "null";
                        Properties.Settings.Default.Save();
                    }

                    MessageBox.Show("Account successfully created!");
                    this.Close();
                    form.createRedditThreads();
                }
            }
            else
            {
                MessageBox.Show("Server is not connected. Try again later", "Registration failed");

                this.Close();
            }
        }

        private void logoutClicked(object sender, MouseEventArgs e)
        {
            form.currentUser = null;
            Properties.Settings.Default.username = "null";
            Properties.Settings.Default.token = "null";
            Properties.Settings.Default.Save();
            this.user.Clear();
            this.remember.Checked = false;
            this.pass.Clear();
            MessageBox.Show("Successfully logged out");
        }

        private void minimizeClicked(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void exitClicked(object sender, MouseEventArgs e)
        {
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
                g.FillPolygon(button.inside, button.ClickLocation);

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Far;
            g.DrawString("Blotto Beats - Login", font, form.darkColor, 65 * size / 16, size / 8 + size / 18, format);
            g.DrawString("Username:", font, form.lightColor, size / 4, 9 * size / 8);
            g.DrawString("Password:", font, form.lightColor, size / 4, 13 * size / 8);
            g.DrawString("Remember:", smallFont, form.lightColor, 15 * size  / 4, 69 * size / 32, format);
            StringFormat format2 = new StringFormat();
            format2.Alignment = StringAlignment.Center;
            g.DrawString("Login", smallFont, form.textColor, 11 * size / 16, 69 * size / 32, format2);
            g.DrawString("Register", smallFont, form.textColor, 24 * size / 16, 69 * size / 32, format2);
            g.DrawString("Logout", smallFont, form.textColor, 37 * size / 16, 69 * size / 32, format2);
        }
    }
}
