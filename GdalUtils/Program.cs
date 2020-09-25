using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GdalUtils.Tools;

namespace GdalUtils
{
        class Program
        {
                // 全局就一个
                public static Setting SingtonSetting = new Setting();
                static void help()
                {
                        Console.WriteLine("请通过 [程序名] [模块名] 获取相关帮助文档");
                        Console.WriteLine("模块如下:");
                        Console.WriteLine("\tpolygonize\tmergeBand\trasterize");
                        Console.WriteLine("\trasterToPolygon\tchangeField\tcalcBand");
                        Console.WriteLine("按任意键继续");
                        Console.ReadKey();
                }
                static void Main(string[] args)
                {
                        SingtonSetting.init();
                        GdalConfiguration.ConfigureGdal();
                        GdalConfiguration.ConfigureOgr();
                        OSGeo.OGR.Ogr.RegisterAll();
                        OSGeo.GDAL.Gdal.AllRegister();

                        if (args.Length == 0)
                        {
                                help();
                        }
                        else
                        {
                                switch (args[0])
                                {
                                        case "polygonize":
                                                Polygonize.ToPolygonize(args);
                                                break;
                                        case "mergeBand":
                                                RasterBandOP.MergeBand.ToMergeMultiBnadToOne(args);
                                                break;
                                        case "calcBand":
                                                RasterBandOP.CalcBnad.ToCalcBandHelp(args);
                                                break;
                                        case "rasterToPolygon":
                                                RasterPolygonize.ToPolygonize(args);
                                                break;
                                        case "changeField":
                                                ShpOp.ChangeFiled.ToChangeField(args);
                                                break;
                                        case "rasterize":
                                                ShpOp.Rasterize.ToRasterize(args);
                                                break;
                                        case "SelectSomeFromAnother":
                                                SelectSomeFromAnotherHelp.SelectSomeFromAnother(args);
                                                break;
                                        default:
                                                help();
                                                break;
                                }
                        }
                }
                static void testSer()
                {
                        iGeospatial.Geometries.GeometryList list = SelectFeatureHelp.OpenFromSer(@"C:\Users\HUZENGYUN\Documents\git\cshap\iGeospatial-master\Version 1.1\Open\GdalUtils\bin\Release\test\lineToPoly\poly.ser");
                        Console.WriteLine();
                        double maxArea = list[0].Area;
                        double minArea = list[0].Area;
                        double argArea = list[0].Area;
                        for (int i = 1;i < list.Count;i++)
                        {
                                //Console.WriteLine("area = {0} width = {1} height = {2}",list[i].Area,list[i].Bounds.Width,list[i].Bounds.Height);
                                argArea = (argArea + list[i].Area) / 2;
                                maxArea = maxArea > list[i].Area ? maxArea : list[i].Area;
                                minArea = minArea < list[i].Area ? minArea : list[i].Area;
                        }
                        iGeospatial.Geometries.GeometryList list2 = new iGeospatial.Geometries.GeometryList();
                        for (int i = 0;i < list.Count;i++)
                        {
                                if (list[i].Area > argArea)
                                {
                                        list2.Add(list[i]);
                                }
                        }
                        Utils.CreateShpFile.SaveGeometryListToShpFile(list2,
                                @"C:\Users\HUZENGYUN\Documents\git\cshap\iGeospatial-master\Version 1.1\Open\GdalUtils\bin\Release\test\lineToPoly\calcout.json",
                                OSGeo.OGR.wkbGeometryType.wkbCurvePolygon);

                        Console.WriteLine("arg area = " + argArea);
                        Console.WriteLine("max area = " + maxArea);
                        Console.WriteLine("min area = " + minArea);
                        Console.ReadKey();
                }
        }
}
