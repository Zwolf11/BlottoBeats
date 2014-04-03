﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private void button1_Click(object sender, EventArgs e)
        {
            form.server.ip = this.textBox1.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            form.accountForm.textBox1.Clear();
            form.accountForm.textBox2.Clear();
            form.accountForm.ShowDialog();
        }
    }
}
