using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Networking;

namespace BlottoBeats
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
            UserToken token;

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

                this.Close();
            }

        }

        //Register button
        private void button2_Click(object sender, EventArgs e)
        {
            UserToken token;

            token = form.server.Authenticate(new Credentials(textBox2.Text, textBox1.Text), true);

            if (token == null)
            {
                MessageBox.Show("Error occurred. Username might already be taken");
                form.currentUser = token;
            }
            else
            {
                this.Close();
            }
        }
    }
}
