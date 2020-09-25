using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GisForm
{
        public partial class Form1 : Form
        {
                public Form1()
                {
                        InitializeComponent();
                        int x = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width - this.Size.Width;
                        int y = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height - this.Size.Height;
                        Point p = new Point(x, y);
                        this.PointToScreen(p);
                        this.Location = p;
                }

                private void button1_Click(object sender, EventArgs e)
                {
                        // 这个是我用 Gdal 重写的
                        Form form = new shapefile.ShowShapeFile();
                        form.Show();
                }

                private void Form1_Load(object sender, EventArgs e)
                {
                        button1_Click(null, null);
                }

                private void button2_Click(object sender, EventArgs e)
                {
                        // 这个是将他的代码稍微整合的
                        Form form = new shapefileNoGdal.Form1();
                        form.Show();
                }
        }
}
