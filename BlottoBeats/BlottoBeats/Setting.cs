using System;
using System.Windows.Forms;
using System.Drawing;

namespace BlottoBeats
{
    public class Setting
    {
        public int pos;
        public Form parent;
        public Point loc;
        public Label label;
        public TextBox text;
        public CheckBox checkbox;

        public Setting(int pos, String name, Form parent, int size)
        {
            this.pos = pos;
            this.parent = parent;
            label = new Label();
            label.Text = name;
            label.BackColor = Color.Transparent;
            text = new TextBox();
            checkbox = new CheckBox();
            checkbox.BackColor = Color.Transparent;
            checkbox.CheckedChanged += this.checkboxChanged;
            parent.Controls.Add(label);
            parent.Controls.Add(text);
            parent.Controls.Add(checkbox);
            init(size);
        }

        public void init(int size)
        {
            Graphics g = parent.CreateGraphics();
            this.loc = new Point(3 * size / 4, (int)(19 * size / 16 + pos * g.MeasureString(label.Text, label.Font).Height + pos * size / 8));
            label.Font = new Font("Arial", 3 * size / 20);
            SizeF labelSize = g.MeasureString(label.Text, label.Font);
            label.Location = loc;
            label.Size = new Size((int)labelSize.Width + 1, (int)labelSize.Height);
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
    }
}
