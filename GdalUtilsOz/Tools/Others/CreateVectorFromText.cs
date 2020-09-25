using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iGeospatial.Geometries;

namespace GdalUtilsOz.Tools.Others
{
        class CreateVectorFromText
        {
                public static void help(string commandName)
                {
                        Console.WriteLine("程序名 " + commandName + " txtPath type vectorPath [fileType]");
                        Console.WriteLine("type 是 矢量 的要素类型，可选有 [line] [point] [polygon]");
                        Console.WriteLine("fileType 是文件类型，可选有 [file] [ser]，默认是 [file]");
                        Console.WriteLine("\tfile 表示矢量文件 ser 表示序列化文件");
                        Console.WriteLine("注意这里的文件格式如下");
                        Console.WriteLine("点文件为一行两个数值，表示xy，用[空格]或[英文逗号隔开]");
                        Console.WriteLine("线和面一行表示一个线或面，也是用[空格]或[英文逗号隔开]");
                        Console.WriteLine("线/面文件一行 [x1 y1 x2 y2 x3 y3 ...]");
                        Console.WriteLine("例如 point.txt 内容如下\n100 100\n100 200\n200 200\n200 100\n");
                        Console.WriteLine("line.txt内容如下\n100 100 100 200\n200 100 200 200 300 300\n");
                        Console.WriteLine("面文件要求一行至少三个点");
                } 
                public static void ToCreateVectorFromText(string[] args,string commandName) {
                        if (args.Length == 4 || args.Length == 5)
                        {
                                bool saveAsFile = true;
                                OSGeo.OGR.wkbGeometryType type;
                                GeometryList list;
                                switch(args[2].ToLower().Trim())
                                {
                                        case "point":
                                                list = ToCreateVectorFromTextPoint(args[1]);
                                                type = OSGeo.OGR.wkbGeometryType.wkbPoint;
                                                break;
                                        case "line":
                                                list = ToCreateVectorFromTextLine(args[1]);
                                                type = OSGeo.OGR.wkbGeometryType.wkbLineString;
                                                break;
                                        case "polygon":
                                                list = ToCreateVectorFromTextPolygon(args[1]);
                                                type = OSGeo.OGR.wkbGeometryType.wkbCurvePolygon;
                                                break;
                                        default:
                                                help(commandName);
                                                return;
                                }
                                if (args.Length == 5)
                                {
                                        saveAsFile = !string.Equals(args[4], "ser");
                                }

                                if (saveAsFile)
                                {
                                        Utils.VectorOperation.Create.SaveGeometryListToShpFile(list, args[3], type);
                                } else
                                {
                                        Utils.SerializeObject.ToSerialize(list, args[4]);
                                }
                        } else
                        {
                                help(commandName);
                        }
                }

                public static GeometryList ToCreateVectorFromTextPolygon(string filePath)
                {
                        Utils.VectorOperation.URing ring = new Utils.VectorOperation.URing();
                        Utils.VectorOperation.UPolygon polygon = new Utils.VectorOperation.UPolygon();
                        GeometryList list = new GeometryList();
                        foreach (string line in System.IO.File.ReadAllLines(filePath, Encoding.Default))
                        {
                                string[] xyLabel = line.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                                ring.Clear();
                                for (int i = 0; i < xyLabel.Length; i += 2)
                                {
                                        ring.addPoint(
                                                double.Parse(xyLabel[i]),
                                                double.Parse(xyLabel[i + 1])
                                        );
                                }
                                polygon.SetRing(ring);
                                list.Add(polygon.Polygon);
                        }
                        return list;
                }
                public static GeometryList ToCreateVectorFromTextLine(string filePath)
                {
                        Utils.VectorOperation.ULine lineString = new Utils.VectorOperation.ULine();
                        GeometryList list = new GeometryList();
                        foreach (string line in System.IO.File.ReadAllLines(filePath, Encoding.Default))
                        {
                                string[] xyLabel = line.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                                lineString.Clear();
                                for (int i = 0; i < xyLabel.Length; i += 2)
                                {
                                        lineString.addPoint(
                                                double.Parse(xyLabel[i]),
                                                double.Parse(xyLabel[i + 1])
                                        );
                                }
                                list.Add(lineString.Line);
                        }
                        return list;
                }
                public static GeometryList ToCreateVectorFromTextPoint(string filePath)
                {
                        Utils.VectorOperation.UPoint point = new Utils.VectorOperation.UPoint();
                        GeometryList list = new GeometryList();
                        foreach (string line in System.IO.File.ReadAllLines(filePath, Encoding.Default))
                        {
                                string[] xyLabel = line.Split(new char[] { ' ', ',' },StringSplitOptions.RemoveEmptyEntries);
                                point.SetPoint(
                                        double.Parse(xyLabel[0]),
                                        double.Parse(xyLabel[1])
                                );
                                list.Add(point.Point);
                        }
                        return list;
                }
        }
}
