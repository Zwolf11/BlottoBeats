using System;
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

        public AdvancedSettings(MainForm form)
        {
            InitializeComponent();
            this.textBox1.Text = form.server.ip;
            this.label3.Text = Convert.ToString(form.backlog.Count);
        }
    }
}
