namespace UIExtent.Event {
        class MouseMove {
                public delegate void MouseMoveEvent(double x, double y);
                bool active = false;
                public MouseMove(System.Windows.Forms.Control control, MouseMoveEvent moveEvent) {
                        control.MouseMove += new System.Windows.Forms.MouseEventHandler((obj, e) => {
                                if (!active) return;
                                moveEvent(e.X, e.Y);
                        });
                }
                public void ToggleMouseMove(bool off = false) {
                        if (off == true) {
                                active = false;
                        } else {
                                active = !active;
                        }
                }
        }
}
