using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GisForm.shapefile
{
        public partial class ShowShapeFile : Form
        {
                UIExtent.DrawFeature.ManagerMap mmap;
                UIExtent.UI.ToggleButton dragToggleBtn;
                UIExtent.UI.ToggleButton selectToggleBtn;
                UIExtent.UI.ToggleButton infoToggleBtn;
                public ShowShapeFile()
                {
                        InitializeComponent();
                        mmap = new UIExtent.DrawFeature.ManagerMap(pictureBox1);
                        mmap.ToggleMouseMove((x, y) => {
                                textBox1.Text = "(x,y) = (" + x + ',' + y + ")"; 
                        });
                        // mmap.ToggleMouseDown();

                        // dragBtn 时按钮的变量名
                        // Color.Aqua 功能激活颜色
                        // infoBtn.Text 功能激活显示的按钮文字
                        // 返回的这个类有一个 toggle 方法,用于激活和关闭改功能
                        dragToggleBtn = new UIExtent.UI.ToggleButton(dragBtn, Color.Aqua, dragBtn.Text, () => {
                                mmap.ToggleDrag();
                        });
                        // selectBtn 时按钮的变量名
                        // Color.Aqua 功能激活颜色
                        // infoBtn.Text 功能激活显示的按钮文字
                        selectToggleBtn = new UIExtent.UI.ToggleButton(selectBtn, Color.Aqua, selectBtn.Text, () => {
                                mmap.ToggleMouseDown();
                        });
                        //selectToggleBtn.Toggle();
                        // infoBtn 时按钮的变量名
                        // Color.Aqua 功能激活颜色
                        // infoBtn.Text 功能激活显示的按钮文字
                        infoToggleBtn = new UIExtent.UI.ToggleButton(infoBtn, Color.Aqua, infoBtn.Text, () => {
                                // 开关功能
                                mmap.ToggleShowInfo(str => {
                                        // str 是信息文本
                                        textBox2.Text = str;
                                });
                        });
                }

                private void ShowShapeFile_Load(object sender, EventArgs e)
                {
                        this.SizeChanged += new EventHandler((obj, ev) => {
                                pictureBox1.Height = this.Size.Height - 70;
                                pictureBox1.Width = this.Size.Width - 230;
                                textBox2.Height = pictureBox1.Height;
                                mmap.OnlyDrawTime.Do((int)UIExtent.DrawFeature.ManagerMap.ToDraw.NoUpdate);
                        });
                }
                bool drawed = false;
                private void button1_Click(object sender, EventArgs e)
                {
                        if (!drawed)
                        {
                                string path = @"C:\Users\HUZENGYUN\Documents\git\gis相关代码\中国地图 Shapefile\test\gadm36_CHN_3.shp";
                                OSGeo.OGR.DataSource ds = OSGeo.OGR.Ogr.Open(path, 1);
                                mmap.AddShpDataSource(ds,true);
                                drawed = true;
                        }
                        mmap.OnlyDrawTime.DoNotWait((int)UIExtent.DrawFeature.ManagerMap.ToDraw.Upadte);
                }
                private void dragBtn_Click(object sender, EventArgs e) {
                        dragToggleBtn.Toggle();
                }

                private void selectBtn_Click(object sender, EventArgs e) {
                        selectToggleBtn.Toggle();
                }

                private void ZoomInBtn_Click(object sender, EventArgs e) {
                        mmap.Zoom(true);
                }

                private void zoomOutBtn_Click(object sender, EventArgs e) {
                        mmap.Zoom(false);
                }

                private void infoBtn_Click(object sender, EventArgs e) {
                        infoToggleBtn.Toggle();
                }
        }
}
