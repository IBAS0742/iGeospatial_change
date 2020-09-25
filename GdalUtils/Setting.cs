using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GdalUtils
{
        public class SettingUtils
        {
                public static double[] getTransform() {
                        double[] transform = new double[6];
                        int count = 0;
                        Program.SingtonSetting.Transform.Split(' ').ToList().ForEach(tr => {
                                transform[count] = Double.Parse(tr);
                                count++;
                        });
                        return transform;
                }
        }
        [Serializable]
        public class Setting
        {
                [NonSerialized]
                private string _xmlPath = System.Environment.CurrentDirectory + "\\setting.xml";
                #region public 属性
                //private string shpProject = "EPSG4326";
                public string ShpProject { get; set; } = "EPSG4326";
                public string RasterProject { get; set; } = "EPSG4326";
                public string ShpDriver { get; set; } = "ESRI Shapefile";
                public string ShpLayName { get; set; } = "lay0";
                public string RasterDriver { get; set; } = "GTiff";
                public string Transform { get; set; } = "38 0.00833333 0 42 0 -0.00833333";
                #endregion

                #region public 方法
                public Setting() {
                        //init();
                }
                public void help()
                {
                        Console.WriteLine("ShpProject/RasterProject 保存默认投影，当前是" + ShpProject);
                        Console.WriteLine("可选类型有 EPSG4326");
                        Console.WriteLine("");
                        Console.WriteLine("ShpDriver 保存 矢量 的驱动器，当前是" + ShpDriver);
                        Console.WriteLine("可选有 [ESRI Shapefile] [GeoJSON] [CDA] [CSV]");
                        Console.WriteLine("更多可选参数可以到 https://gdal.org/drivers/vector/ 获取");
                        Console.WriteLine("");
                        Console.WriteLine("ShpLayName 创建新图层时的名称，可以改成任意内容，但不能为空");
                        Console.WriteLine("");
                        Console.WriteLine("RasterDriver 保存 栅格 文件的驱动，当前是" + RasterDriver);
                        Console.WriteLine("可选有 [GTiff] [JPEG] ...");
                        Console.WriteLine("更多可选可以到 https://gdal.org/drivers/raster/ 获取");
                        Console.ReadKey();
                        Console.WriteLine("Transform 保存 栅格 文件的默认Transform，当前是" + RasterDriver);
                        Console.WriteLine("格式是 [左上角经度 偏向 横向像素大小 左上角维度 偏向 纵向像素大小]");
                        Console.WriteLine("更多帮助 https://gdal.org/user/raster_data_model.html#affine-geotransform");
                        Console.ReadKey();
                }
                public void init()
                {
                        Console.WriteLine("Configure file is exist ? " + System.IO.File.Exists(_xmlPath));
                        if (System.IO.File.Exists(_xmlPath) == false)
                        {
                                Utils.SerializeObject.ToXMLSerialize(this, _xmlPath, typeof(Setting));
                        }
                        else
                        {
                                Setting set = (Setting)Utils.SerializeObject.FromXMLSerialize(_xmlPath, typeof(Setting));

                                foreach (PropertyInfo pi in set.GetType().GetProperties())
                                {
                                        if (pi.Name.StartsWith("_"))
                                        {
                                                continue;
                                        }
                                        Console.WriteLine(pi.Name + " = " + pi.GetValue(set));
                                        pi.SetValue(this, pi.GetValue(set));
                                }
                        }
                }
                #endregion

                #region private 方法
                #endregion
        }
}
