using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIExtent.Event {
        class Drag {
                List<System.Windows.Forms.MouseEventHandler> dragEvents
                        = new List<System.Windows.Forms.MouseEventHandler>();
                public delegate void DearLoc(int x,int y);
                DearLoc moving;
                DearLoc moved;

                bool dragFlag = false;
                bool activate = false;
                int x, y;
                System.Windows.Forms.Control target;
                public Drag(System.Windows.Forms.Control _control,DearLoc _moving,DearLoc _moved) {
                        target = _control;
                        moving = _moving;
                        moved = _moved;
                        Init();
                }
                void Init() {
                        // 点下鼠标事件
                        dragEvents.Add(new System.Windows.Forms.MouseEventHandler((obj, e) => {
                                if (!activate) return;
                                dragFlag = true;
                                x = e.X;
                                y = e.Y;
                        }));
                        // 移动结束
                        dragEvents.Add(new System.Windows.Forms.MouseEventHandler((obj, e) => {
                                if (!activate) return;
                                dragFlag = false;
                                moved(Convert.ToInt16(e.X - x), Convert.ToInt16(e.Y - y));
                        }));
                        dragEvents.Add(new System.Windows.Forms.MouseEventHandler((obj, e) => {
                                if (!activate) return;
                                if (dragFlag) {
                                        moving(Convert.ToInt16(e.X - x), Convert.ToInt16(e.Y - y));
                                }
                        }));
                        target.MouseDown += dragEvents[0];
                        target.MouseUp += dragEvents[1];
                        target.MouseMove += dragEvents[2];
                }
                public void ToggleDrag(bool off = false) {
                        if (off == true) {
                                activate = false;
                        }
                        activate = !activate;
                }
        }
}
