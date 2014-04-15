using BlottoBeats.Library.Networking;
using System;
using System.Windows.Forms;
using System.Threading;

namespace BlottoBeats.Client
{
    public partial class AdvancedSettings : Form
    {
        MainForm form;

        public AdvancedSettings(MainForm form)
        {
            InitializeComponent();
            this.form = form;
            this.textBox1.Text = Properties.Settings.Default.lastIP;
            this.textBox2.Text = Properties.Settings.Default.maxSongs + "";
            this.label3.Text = Convert.ToString(form.backlog.Count);
        }

        //update ip button
        private void button1_Click(object sender, EventArgs e)
        {
            Thread iPThread = new Thread(new ThreadStart(testNewIP));
            iPThread.Start();

            
        }

        //update max backlog button
        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.maxSongs = form.maxSongs;
        }

        //account management button
        private void button3_Click(object sender, EventArgs e)
        {
            form.accountForm.pass.Clear();
            if (form.currentUser != null)
            {
                form.accountForm.user.Text = form.currentUser.username;
            }
            
            form.accountForm.ShowDialog();
        }

        private void testNewIP()
        {

            BBServerConnection newServer = new BBServerConnection(this.textBox1.Text, 3000);

            if (newServer.Test())
            {
                form.server.ip = this.textBox1.Text;
                Properties.Settings.Default.lastIP = form.server.ip;
                MessageBox.Show("Success!", "Change IP Success");
            }
            else
            {
                MessageBox.Show("Server is not connected. Try again later", "Change IP failed");
            }

        }
    }
}
