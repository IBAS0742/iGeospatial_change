using System;
using System.Collections.Generic;
using OSGeo.OGR;
using OSGeo.GDAL;
using iGeospatial.Geometries.Operations;
using iGeospatial.Geometries;
using iGeospatial.Collections.Sets;
using System.Collections;

namespace GdalUtilsOz.Tools.Vector
{
        class Polygonize
        {
                private static void help(string commandName)
                {
                        Console.WriteLine("★ 程序功能，将矢量转为面矢量，并将相接的多个线矢量尽可能转化为一个面");
                        Console.WriteLine("");
                        Console.WriteLine("程序名 " + commandName + " line.shp [-p poly.shp] [-d dangle.shp] [-c cut.shp] [-ps poly.ser]");
                        Console.WriteLine("line.shp 是需要转换的文件名（路径）");
                        Console.WriteLine("[-p poly.shp] -p 是指 poly.shp 是保存【面】文件的文件名（路径）");
                        Console.WriteLine("[-d dangle.shp] -d 是指 dangle.shp 是保存【悬挂线】文件的文件名（路径）");
                        Console.WriteLine("[-c cut.shp] -c 是指 cut.shp 是保存【排除的线】文件的文件名（路径）");
                        Console.WriteLine("[-ps poly.ser] 将 poly 计算结果序列化，用于下次使用 geos 库计算时不需要转化为 geos 对象");
                        Console.WriteLine("");
                        Console.WriteLine("例如：");
                        Console.WriteLine("程序名 vector.polygonize line.shp -p poly.shp | 表示只要保存面文件");
                        Console.WriteLine("程序名 vector.polygonize line.shp -c cut.shp | 表示只要保存排除的线文件");
                        Console.WriteLine("程序名 vector.polygonize line.shp -p poly.shp -c cut.shp");
                }
                /**
                 * args = ['vector.polygonize','line.shp','-p','poly.shp','-d','dangle.shp','-c','cut.shp','-ps','poly.ser']
                 */
                public static void ToPolygonize(string[] args,string commandName)
                {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        if (args.Length == 2)
                        {
                                help(commandName);
                                return;
                        }
                        else if (args.Length > 2 && args.Length % 2 == 0)
                        {
                                for (int i = 2; i < args.Length; i += 2)
                                {
                                        if (dic.ContainsKey(args[i]))
                                        {
                                                dic[args[i]] = args[i + 1];
                                        }
                                        else
                                        {
                                                dic.Add(args[i], args[i + 1]);
                                        }
                                }
                                string polyPath = null;
                                string danglesPath = null;
                                string cutEdgePath = null;
                                string polyserPath = null;

                                dic.TryGetValue("-p", out polyPath);
                                dic.TryGetValue("-d", out danglesPath);
                                dic.TryGetValue("-c", out cutEdgePath);
                                dic.TryGetValue("-ps", out polyserPath);
                                if (
                                        polyPath == null ||
                                        danglesPath == null ||
                                        cutEdgePath == null ||
                                        polyserPath == null)
                                {
                                        ToPolygonize(args[1], polyPath, danglesPath, cutEdgePath, polyserPath);
                                }
                                else
                                {
                                        help(commandName);
                                }
                        }
                        else
                        {
                                help(commandName);
                        }
                }
                private static void ToPolygonize(
                        string linePath,
                        string polyPath,
                        string danglesPath,
                        string cutEdgePath,
                        string polyPathSer)
                {
                        GdalConfiguration.ConfigureGdal();
                        GdalConfiguration.ConfigureOgr();
                        Ogr.RegisterAll();
                        Gdal.AllRegister();

                        DataSource ds = Ogr.Open(linePath, 0);
                        Layer layer = ds.GetLayerByIndex(0);
                        long count = layer.GetFeatureCount(0);
                        Polygonizer polygonizer = new Polygonizer();

                        for (int i = 0; i < count; i++)
                        {
                                polygonizer.Add(Utils.ShiftGeosOgr.FromOgrToGeos.OgrFeatureToGeosLineString(layer.GetFeature(i)));
                        }

                        polygonizer.Polygonize();
                        if (polyPath != null || polyPathSer != null)
                        {
                                GeometryList list = (GeometryList)polygonizer.Polygons;
                                if (polyPath != null)
                                        Utils.VectorOperation.Create.SaveGeometryListToShpFile(list, polyPath, wkbGeometryType.wkbCurvePolygon);
                                if (polyPathSer != null)
                                        Utils.SerializeObject.ToSerialize(list, polyPathSer);
                        }

                        if (danglesPath != null)
                        {
                                Utils.VectorOperation.Create.SaveHashedSetLineStringToShpFile((HashedSet)polygonizer.Dangles, danglesPath);
                        }
                        if (cutEdgePath != null)
                        {
                                Utils.VectorOperation.Create.SaveArrLineStringToShpFile((ArrayList)polygonizer.CutEdges, cutEdgePath);
                        }

                        ds.Dispose();
                        layer.Dispose();

                        Gdal.GDALDestroyDriverManager();
                }
        }
}
