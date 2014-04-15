using BlottoBeats.Library.Authentication;
using System;
using System.Windows.Forms;
using System.Threading;

namespace BlottoBeats.Client
{
    public partial class AccountCreation : Form
    {
        MainForm form;
        public AccountCreation(MainForm form)
        {
            InitializeComponent();
            this.form = form;
        }

        //login button
        private void button1_Click(object sender, EventArgs e)
        {
            Thread loginThread = new Thread(new ThreadStart(login));
            loginThread.Start();
            
        }

        private void login()
        {
            UserToken token;

            if (form.server.Test())
            {
                token = form.server.Authenticate(new Credentials(textBox2.Text, textBox1.Text), false);

                if (token == null)
                {
                    MessageBox.Show("Username/Password was incorrect. Please try again");
                }
                else
                {
                    form.currentUser = token;
                    Properties.Settings.Default.username = form.currentUser.username;
                    Properties.Settings.Default.expires = form.currentUser.expires;
                    Properties.Settings.Default.token = form.currentUser.token;
                    Properties.Settings.Default.Save();

                    //MessageBox.Show("Current User: " + form.currentUser.username + " Current Token: " + form.currentUser.token);

                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Server is not connected. Try again later", "Login failed");
                this.Close();
            }

        }

        //Register button
        private void button2_Click(object sender, EventArgs e)
        {
            Thread registerThread = new Thread(new ThreadStart(register));
            registerThread.Start();
            
        }

        private void register()
        {
            UserToken token;
            if (form.server.Test())
            {

                token = form.server.Authenticate(new Credentials(textBox2.Text, textBox1.Text), true);

                if (token == null)
                {
                    MessageBox.Show("Error occurred. Username might already be taken");
                    form.currentUser = token;
                }
                else
                {
                    form.currentUser = token;
                    Properties.Settings.Default.username = form.currentUser.username;
                    Properties.Settings.Default.expires = form.currentUser.expires;
                    Properties.Settings.Default.token = form.currentUser.token;
                    Properties.Settings.Default.Save();

                    //MessageBox.Show("Current User: " + form.currentUser.username + " Current Token: " + form.currentUser.token);
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Server is not connected. Try again later", "Registration failed");
                this.Close();
            }
        }
    }
}
