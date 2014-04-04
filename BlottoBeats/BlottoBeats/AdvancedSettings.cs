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
    public partial class AdvancedSettings : Form
    {
        MainForm form;

        public AdvancedSettings(MainForm form)
        {
            InitializeComponent();
            this.form = form;
            this.textBox1.Text = form.server.ip;
            this.label3.Text = Convert.ToString(form.backlog.Count);
        }

        //update ip button
        private void button1_Click(object sender, EventArgs e)
        {
            BBServerConnection newServer = new BBServerConnection(this.textBox1.Text, 3000);

            if (newServer.Test())
            {
                form.server.ip = this.textBox1.Text;
                Properties.Settings.Default.lastIP = form.server.ip;
            }
            else
            {
                MessageBox.Show("Server is not connected. Try again later");
            }
        }

        //update max backlog button
        private void button2_Click(object sender, EventArgs e)
        {

        }

        //account management button
        private void button3_Click(object sender, EventArgs e)
        {
            form.accountForm.textBox1.Clear();
            if (form.currentUser != null)
            {
                form.accountForm.textBox2.Text = form.currentUser.username;
            }
            
            form.accountForm.ShowDialog();
        }
    }
}
