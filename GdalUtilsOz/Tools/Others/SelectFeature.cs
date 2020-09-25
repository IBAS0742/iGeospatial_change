using OGR = OSGeo.OGR;
using iGeospatial.Geometries;
using System;

namespace GdalUtilsOz.Tools.Others
{
        public enum FeatureType
        {
                Point = 1,
                LineString = 2,
                Polygon = 3
        }
        class SelectFeature
        {
                public static void SelectHelp(string commandName) {
                        Console.WriteLine("程序名 " + commandName + " <type> inpath <type> outpath exp1 exp2");
                        Console.WriteLine("type 是[inpath 和 outpath]文件类型，可选有 [ser] 和 [file]");
                        Console.WriteLine("\tser 表示序列化文件 file 表示普通矢量文件(不一定是 shp)");
                        Console.WriteLine("inpath 是要筛选的文件");
                        Console.WriteLine("outpath 是保存筛选结果的文件");
                        Console.WriteLine("exp 是表达式，格式为 [ABC]");
                        Console.WriteLine("A 可选为 [length] [area] [sq]");
                        Console.WriteLine("\tsq 是长宽比，恒小于 1，改比值由单个要素的 extent 进行计算");
                        Console.WriteLine("B 可选为 [>] [<] [==] [!=] [>=] [<=]");
                        Console.WriteLine("▲注意条件需要加 [双引号] 因为 > 是定向符");
                        Console.WriteLine("C 是一个浮点数");
                        Console.WriteLine("例如:");
                        Console.WriteLine("程序名 selectFeature ser a.ser ser b.ser \"length>=10\" \"length<20\" \"sq>0.8\"");
                        Console.WriteLine("▲为了简化代码，[exp] 在不要包含空格，以下例子放弃识别");
                        Console.WriteLine("程序名 selectFeature ser a.ser ser b.ser length >= 10 length < 20 sq > 0.8");
                }
                public static void ShowInfoHelp(string commandName)
                {
                        Console.WriteLine("程序名 " + commandName + " <type> inpath");
                        Console.WriteLine("type 是[inpath]文件类型，可选有 [ser] 和 [file]");
                        Console.WriteLine("\tser 表示序列化文件 file 表示普通矢量文件(不一定是 shp)");
                        Console.WriteLine("inpath 是要显示信息的文件");
                        Console.WriteLine("例如:");
                        Console.WriteLine("程序名 selectFeature ser a.ser");
                }
                public static void ToSelectFeature(string[] args,string commandName) {
                        if (args.Length < 6)
                        {
                                SelectHelp(commandName);
                        } else
                        {
                                if ((args[1] == "file" || args[1] == "ser") && (args[3] == "file" || args[3] == "ser"))
                                {
                                        GeometryList list,olist;
                                        if (args[1] == "file")
                                        {
                                                OGR.DataSource dataSource = OGR.Ogr.Open(args[2], 0);
                                                list = Utils.ShiftGeosOgr.FromOgrToGeos.OgrFeatureToGeoAuto(dataSource);
                                                dataSource.Dispose();
                                        } else
                                        {
                                                list = (GeometryList)Utils.SerializeObject.FromSerialize(args[1]);
                                        }
                                        Utils.VectorOperation.SelectFeature.Exp exp = new Utils.VectorOperation.SelectFeature.Exp();
                                        for (int i = 5;i < args.Length;i++)
                                        {
                                                exp.AddExp(args[i]);
                                        }
                                        olist = exp.ToCalc(list);
                                        if (olist.Count > 0)
                                        {
                                                if (args[3] == "file")
                                                {
                                                        Utils.ShiftGeosOgr.FromGeosToOgr.SaveGeoGeometryListToOgrDS(
                                                                olist,
                                                                Utils.VectorOperation.Create.ToCreateShpFile(
                                                                        args[4], 
                                                                        Utils.Prj.getPrj(Program.setting.ShpProject), 
                                                                        new System.Collections.Generic.Dictionary<string, OGR.wkbGeometryType>() {
                                                                                {"lay", Utils.ShiftGeosOgr.ShiftTypes.FromGeosToOgr(olist[0].GeometryType)}
                                                                        })
                                                                );
                                                } else
                                                {
                                                        Utils.SerializeObject.ToSerialize(olist, args[4]);
                                                }
                                        }
                                } else
                                {
                                        Console.WriteLine("类型必须是 file 或 ser");
                                        SelectHelp(commandName);
                                }
                        }
                }
                public static void ToShowFeatureInfo(string[] args,string commandName) {
                        if (args.Length == 3)
                        {
                                GeometryList list;
                                switch (args[1])
                                {
                                        case "file":
                                                OGR.DataSource dataSource = OGR.Ogr.Open(args[2], 0);
                                                list = Utils.ShiftGeosOgr.FromOgrToGeos.OgrFeatureToGeoAuto(dataSource);
                                                dataSource.Dispose();
                                                break;
                                        case "ser":
                                                list = (GeometryList)Utils.SerializeObject.FromSerialize(args[1]);
                                                break;
                                        default:
                                                ShowInfoHelp(commandName);
                                                return;
                                }
                                Utils.VectorOperation.SelectFeature.Exp.ShowInfo(list);
                        } else
                        {
                                ShowInfoHelp(commandName);
                        }
                }
        }
}
