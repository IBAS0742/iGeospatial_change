using System;

namespace GdalUtilsOz.Tools.Vector
{
        class ShiftVectorSer
        {
                public class SaveAsSer
                {
                        public static void help(string commandName)
                        {
                                Console.WriteLine("程序名 " + commandName + " shpPath serPath type");
                                Console.WriteLine("shpPath 为矢量文件(路径)");
                                Console.WriteLine("serPath 为序列化文件(路径)");
                                Console.WriteLine("type 是类型，可选有 [point] [line] [ring] [polygon]");
                                Console.WriteLine("如果一个矢量文件需要被反复读出进行计算，可以将其序列化提高读取速度");
                        }
                        public static void ToSaveAsSer(string[] args,string commandName)
                        {
                                if (args.Length == 4)
                                {
                                        OSGeo.OGR.DataSource dataSource = OSGeo.OGR.Ogr.Open(args[1], 0);
                                        if (dataSource == null) {
                                                Console.WriteLine("打开文件[" + args[1] + "]失败\r\n");
                                                help(commandName);
                                                return;
                                        }
                                        switch (args[3])
                                        {
                                                case "point":
                                                        Utils.SerializeObject.ToSerialize(
                                                                Utils.ShiftGeosOgr.FromOgrToGeos.OgrFeatureToGeosPoint(dataSource),
                                                                args[2]);
                                                        break;
                                                case "line":
                                                        Utils.SerializeObject.ToSerialize(
                                                                Utils.ShiftGeosOgr.FromOgrToGeos.OgrFeatureToGeosLineString(dataSource),
                                                                args[2]);
                                                        break;
                                                case "ring":
                                                        Utils.SerializeObject.ToSerialize(
                                                                Utils.ShiftGeosOgr.FromOgrToGeos.OgrFeatureToGeosLinearRing(dataSource),
                                                                args[2]);
                                                        break;
                                                case "polygon":
                                                        Utils.SerializeObject.ToSerialize(
                                                                Utils.ShiftGeosOgr.FromOgrToGeos.OgrFeatureToGeosPolygon(dataSource),
                                                                args[2]);
                                                        break;
                                                default:
                                                        help(commandName);
                                                        break;
                                        }
                                        dataSource.Dispose();
                                }
                                else
                                {
                                        help(commandName);
                                }
                        }
                }

                public class SaveAsVector
                {
                        public static void help(string commandName)
                        {
                                Console.WriteLine("程序名 " + commandName + " serPath shpPath");
                                Console.WriteLine("shpPath 为矢量文件(路径)");
                                Console.WriteLine("serPath 为序列化文件(路径)");
                                Console.WriteLine("");
                        }
                        public static void ToSaveAsVector(string[] args,string commandName)
                        {
                                if (args.Length == 3)
                                {
                                        iGeospatial.Geometries.GeometryList list = (iGeospatial.Geometries.GeometryList)Utils.SerializeObject.FromSerialize(args[1]);
                                        if (list.Count > 0)
                                        {
                                                Utils.VectorOperation.Create.SaveGeometryListToShpFile(
                                                        list,
                                                        args[2],
                                                        Utils.ShiftGeosOgr.ShiftTypes.FromGeosToOgr(list[0].GeometryType));
                                        } else
                                        {
                                                Console.WriteLine("没有要素可以转换");
                                                Console.WriteLine("");
                                                Console.WriteLine("");
                                                Console.WriteLine("");
                                                help(commandName);
                                        }
                                }
                                else
                                {
                                        help(commandName);
                                }
                        }
                }
        }
}
