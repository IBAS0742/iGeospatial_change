using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UIExtent.UI
{
        public class ToggleButton
        {
                Button btn;
                public delegate void ToggleMethod();
                ToggleMethod toggleMethod;
                Color toggleColor;
                string toggleText;
                public ToggleButton(Button _btn,Color _toggleColor,string _toggleText,ToggleMethod _toggleMethod) {
                        btn = _btn;
                        toggleColor = _toggleColor;
                        toggleText = _toggleText;
                        toggleMethod = _toggleMethod;
                }
                public void Toggle() {
                        Color tmpColor = btn.BackColor;
                        string tmpText = btn.Text;
                        btn.BackColor = toggleColor;
                        btn.Text = toggleText;
                        toggleColor = tmpColor;
                        toggleText = tmpText;
                        toggleMethod();
                }
        }
}
