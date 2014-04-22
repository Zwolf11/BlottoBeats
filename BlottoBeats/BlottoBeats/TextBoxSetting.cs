using System;
using System.Windows.Forms;
using System.Drawing;

namespace BlottoBeats.Client
{
    public class TextBoxSetting : Setting
    {
        public int pos;
        public MainForm parent;
        public Point loc;
        public Label label;
        public TextBox text;
        public CheckBox checkbox;
        private int minRand;
        private int maxRand;

        public int getIntValue() { return int.Parse(text.Text); }
        public string getStringValue() { return text.Text; }
        public bool isChecked() { return checkbox.Checked; }
        public void setChecked(bool check) { checkbox.Checked = check; }
        public void setValue(String value) { text.Text = value; }

        public TextBoxSetting(int pos, String name, MainForm parent, int minRand, int maxRand, int size)
        {
            this.pos = pos;
            this.parent = parent;
            this.minRand = minRand;
            this.maxRand = maxRand;
            label = new Label();
            label.Text = name;
            label.BackColor = Color.Transparent;
            text = new TextBox();
            text.Text = "1";
            checkbox = new CheckBox();
            checkbox.BackColor = Color.Transparent;
            checkbox.CheckedChanged += this.checkboxChanged;
            checkbox.Checked = true;
            parent.Controls.Add(label);
            parent.Controls.Add(text);
            parent.Controls.Add(checkbox);
            init(size);
        }

        public void init(int size)
        {
            Graphics g = parent.CreateGraphics();
            this.loc = new Point(3 * size / 4, (int)(20 * size / 16 + pos * g.MeasureString(label.Text, label.Font).Height + pos * size / 8));
            label.Font = new Font("Arial", 3 * size / 20);
            SizeF labelSize = g.MeasureString(label.Text, label.Font);
            label.Location = loc;
            label.Size = new Size((int)labelSize.Width + 1, (int)labelSize.Height);
            label.ForeColor = parent.textColor.Color;
            text.Width = 11 * size / 4 - label.Width;
            text.Location = new Point(loc.X + label.Width, loc.Y);
            text.Font = label.Font;
            checkbox.Location = new Point(loc.X + text.Width + label.Width + size / 32, loc.Y);
        }

        public void setVisible(bool visible)
        {
            label.Visible = visible;
            text.Visible = visible;
            checkbox.Visible = visible;
        }

        public void checkboxChanged(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            text.Enabled = !box.Checked;
        }

        public void randomize()
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            text.Text = "" + rand.Next(minRand, maxRand);
        }
    }
}
