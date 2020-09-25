using System;
using GDAL = OSGeo.GDAL;

namespace GdalUtilsOz.Tools.Vector
{
        class Rasterize
        {
                static void help(string commandName)
                {
                        Console.WriteLine("程序名 " + commandName + " shpPath tifPath [type] [burnValue] [defaultGeoTransform] [rasterCellSize]");
                        Console.WriteLine("shpPath 矢量文件(路径)");
                        Console.WriteLine("tifPath 栅格文件(路径)");
                        Console.WriteLine("type 可选，栅格数据类型，默认为double");
                        Console.WriteLine("可选有 [byte] [uint] [int] [ulong] [long] [float] [double]");
                        Console.WriteLine("burnValue 可选，表示栅格化后的值，默认为1.0");
                        Console.WriteLine("defaultGeoTransform 可选，表示是否选用默认的 geoTransform，该值在配置中设置，默认为 true");
                        Console.WriteLine("defaultGeoTransform 可选值为 True 或 其他(其他都是false) (单词无大小写之分)");
                        Console.WriteLine("rasterCellSize 表示栅格像素大小，当 defaultGeoTransform 为 True 时，该值有 配置 设定");
                        Console.WriteLine("例子");
                        Console.WriteLine("程序名 rasterize 1.shp 2.tif double 1.0 True 0.5 // 这里的 0.5 是无效的");
                        Console.WriteLine("程序名 rasterize 1.shp 2.tif double True 0.5 // 这里的 True 和 0.5 是无效的，请将 burnValue 设置为默认值");
                        Console.WriteLine("程序名 rasterize 1.shp 2.tif double 1.0 No 0.5 // 只要不是true都会认为是false");
                        Console.WriteLine("▲注意:默认的 geotransform 只取 [1][2][4][5]，其中[0][3]是右上角坐标，由程序自动计算");
                }
                public static void ToRasterize(string[] args,string commandName)
                {
                        // string shpPath,string tifPath,double burnValue = 1.0,bool defaultGeoTransform = true,double rasterCellSize = 0.00833333
                        if (args.Length < 3 || args.Length > 7)
                        {
                                help(commandName);
                        }
                        else
                        {
                                double burnValue = 1.0;
                                bool defaultGeoTransform = true;
                                double rasterSize = 0.008333;
                                GDAL.DataType type = GDAL.DataType.GDT_Float64;
                                if (args.Length == 7) rasterSize = double.Parse(args[6]);
                                if (args.Length >= 6) defaultGeoTransform = String.IsNullOrEmpty(args[5]) ?
                                                        true : String.Equals(args[5].ToLower().Trim(), "true");
                                if (args.Length >= 5) burnValue = double.Parse(args[4]);
                                if (args.Length >= 4)
                                {
                                        switch (args[3].Trim().ToLower())
                                        {
                                                case "byte":
                                                        type = GDAL.DataType.GDT_Byte;
                                                        break;
                                                case "uint":
                                                        type = GDAL.DataType.GDT_UInt32;
                                                        break;
                                                case "int":
                                                        type = GDAL.DataType.GDT_Int16;
                                                        break;
                                                case "ulong":
                                                        type = GDAL.DataType.GDT_UInt32;
                                                        break;
                                                case "long":
                                                        type = GDAL.DataType.GDT_Int32;
                                                        break;
                                                case "float":
                                                        type = GDAL.DataType.GDT_Float32;
                                                        break;
                                                case "double":
                                                        type = GDAL.DataType.GDT_Float64;
                                                        break;
                                        }
                                }
                                Utils.VectorOperation.Utils.Rasterize(args[1], args[2], type, burnValue, defaultGeoTransform, rasterSize);
                        }
                }
        }
}
