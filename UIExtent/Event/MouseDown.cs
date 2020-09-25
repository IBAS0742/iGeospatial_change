using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIExtent.Event {
        class MouseDown {
                public delegate void MouseDownEvent(double x,double y);
                bool active = false;
                public MouseDown(System.Windows.Forms.Control control,MouseDownEvent mde) {
                        control.MouseDown += new System.Windows.Forms.MouseEventHandler((obj, e) => { 
                                if (active) {
                                        mde(e.X, e.Y);
                                }
                        });
                }
                public void ToggleMouseDown(bool off = false) {
                        if (off == true) {
                                active = false;
                        } else {
                                active = !active;
                        }
                }
        }
}
