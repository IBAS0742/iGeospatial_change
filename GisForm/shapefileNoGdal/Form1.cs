using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UIExtent.DrawFeatureNoGdal;
using static UIExtent.DrawFeatureNoGdal.GISCode;

namespace GisForm.shapefileNoGdal
{
        public partial class Form1 : Form
        {
                MapView mapview;
                List<MapLayer> layers = new List<MapLayer>();
                public Form1()
                {
                        InitializeComponent();
                }

                private void button1_Click(object sender, EventArgs e)
                {
                        string path = @"C:\Users\HUZENGYUN\Documents\git\gis相关代码\中国地图 Shapefile\test\gadm36_CHN_3.shp";
                        ShapeFileRW shp = new ShapeFileRW(path);
                        layers.Add(shp.GetLayer());
                        FullExtent();
                }
                private void FullExtent()
                {
                        if (layers.Count > 0)
                        {
                                if (layers[0].mapextent.GetWidth() / pictureBox1.Width < layers[0].mapextent.GetHeight() / pictureBox1.Height)
                                {
                                        mapview = new MapView(
                                                layers[0].mapextent.GetCenter(),
                                                layers[0].mapextent.GetHeight() / pictureBox1.Height,
                                                pictureBox1.ClientRectangle
                                        );
                                }
                                else
                                {
                                        mapview = new MapView(
                                                layers[0].mapextent.GetCenter(),
                                                layers[0].mapextent.GetWidth() / pictureBox1.Width,
                                                pictureBox1.ClientRectangle
                                        );
                                }
                                //mapview = new MapView(
                                //    layers[0].mapextent.GetCenter(),
                                //    layers[0].mapextent.GetWidth() / pictureBox1.Width,
                                //    pictureBox1.ClientRectangle);
                                DrawMap();
                        }
                        else
                        {
                                MessageBox.Show("choose shp before zoom2Extent!");
                        }
                }
                private void DrawMap()
                {
                        pictureBox1.CreateGraphics().Clear(Color.Black);
                        for (int i = 0; i < layers.Count; i++)
                        {
                                layers[i].draw(mapview, pictureBox1.CreateGraphics());
                        }
                }
        }
}
