using System;

namespace BlottoBeats.Client
{
    public interface Setting
    {
        int getIntValue();
        string getStringValue();
        bool isChecked();
        void setChecked(bool check);
        void setValue(String value);
        void init(int size);
        void setVisible(bool visible);
        void checkboxChanged(object sender, EventArgs e);
        void randomize();
    }
}
