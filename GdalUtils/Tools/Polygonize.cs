using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSGeo.OSR;
using OSGeo.OGR;
using OSGeo.GDAL;
using iGeospatial.Geometries.Operations;
using GdalUtils.Utils;
using iGeospatial.Geometries;
using System.Collections;
using iGeospatial.Collections.Sets;

namespace GdalUtils.Tools
{
        class Polygonize
        {
                private static void help() {
                        Console.WriteLine("程序名 polygonize line.shp [-p poly.shp] [-d dangle.shp] [-c cut.shp] [-ps poly.ser]");
                        Console.WriteLine("line.shp 是需要转换的文件名（路径）");
                        Console.WriteLine("[-p poly.shp] -p 是指 poly.shp 是保存【面】文件的文件名（路径）");
                        Console.WriteLine("[-d dangle.shp] -d 是指 dangle.shp 是保存【悬挂线】文件的文件名（路径）");
                        Console.WriteLine("[-c cut.shp] -c 是指 cut.shp 是保存【排除的线】文件的文件名（路径）");
                        Console.WriteLine("[-ps poly.ser] 将 poly 计算结果序列化，用于下次使用 geos 库计算时不需要转化为 geos 对象");
                        Console.WriteLine("");
                        Console.WriteLine("例如：");
                        Console.WriteLine("程序名 polygonize line.shp -p poly.shp | 表示只要保存面文件");
                        Console.WriteLine("程序名 polygonize line.shp -c cut.shp | 表示只要保存排除的线文件");
                        Console.WriteLine("程序名 polygonize line.shp -p poly.shp -c cut.shp");
                }
                /**
                 * args = ['polygonize','line.shp','-p','poly.shp','-d','dangle.shp','-c','cut.shp','-ps','poly.ser']
                 */
                public static void ToPolygonize(string[] args) {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        if (args.Length == 2)
                        {
                                help();
                                return;
                        }else if (args.Length > 2 && args.Length % 2 == 0)
                        {
                                for (int i= 2;i < args.Length;i+=2)
                                {
                                        if (dic.ContainsKey(args[i]))
                                        {
                                                dic[args[i]] = args[i + 1];
                                        } else
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
                                        ToPolygonize(args[1], polyPath, danglesPath, cutEdgePath,polyserPath);
                                } else
                                {
                                        help();
                                }
                        } else
                        {
                                help();
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
                                polygonizer.Add(Transform.OgrFeatureToGeosLineString(layer.GetFeature(i)));
                        }

                        polygonizer.Polygonize();
                        if (polyPath != null || polyPathSer != null)
                        {
                                GeometryList list = (GeometryList)polygonizer.Polygons;
                                if (polyPath != null)
                                        CreateShpFile.SaveGeometryListToShpFile(list, polyPath, wkbGeometryType.wkbCurvePolygon);
                                if (polyPathSer != null)
                                        Utils.SerializeObject.ToSerialize(list, polyPathSer);
                        }

                        if (danglesPath != null)
                        {
                                CreateShpFile.SaveHashedSetLineStringToShpFile((HashedSet)polygonizer.Dangles, danglesPath);
                        }
                        if (cutEdgePath != null)
                        {
                                CreateShpFile.SaveArrLineStringToShpFile((ArrayList)polygonizer.CutEdges, cutEdgePath);
                        }

                        ds.Dispose();
                        layer.Dispose();

                        Gdal.GDALDestroyDriverManager();
                }
        }
        class RasterPolygonize
        {
                public static void help() {
                        Console.WriteLine("程序名 rasterToPolygon tifPath shpPath");
                }
                public static void ToPolygonize(string[] args) {
                        Console.WriteLine("args[0] = " + args[0] + "\targs.length = " + args.Length);
                        if (args.Length == 3)
                        {
                                ToPolygonize(args[1], args[2]);
                        } else
                        {
                                help();
                        }
                }
                public static void ToPolygonize(string rasterPath,string vectorPath)
                {
                        Utils.Field field = new Utils.Field("fid_1", FieldType.OFTInteger, 0);
                        DataSource vecotrDs = CreateShpFile.ToCreateShpFile(vectorPath, Prj.Prjection.EPSG4326, new Dictionary<string, wkbGeometryType>() {
                                {"lay",wkbGeometryType.wkbCompoundCurve }
                        });
                        Utils.ShpFileOP.AddField(vecotrDs, field);
                        Layer lay = vecotrDs.GetLayerByIndex(0);
                        Dataset ds = Gdal.Open(rasterPath, Access.GA_ReadOnly);
                        Band band = ds.GetRasterBand(1);

                        Gdal.Polygonize(band, null, lay, -1, new string[] { }, null, null);
                        vecotrDs.Dispose();
                        ds.Dispose();
                }
        }
}
