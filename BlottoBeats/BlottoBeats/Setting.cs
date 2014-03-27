using System;
using System.Windows.Forms;

namespace BlottoBeats
{
    public interface Setting
    {
        int getIntValue();
        string getStringValue();
        bool isChecked();
        void setValue(String value);
        void init(int size);
        void setVisible(bool visible);
        void checkboxChanged(object sender, EventArgs e);
        void randomize();
    }
}
