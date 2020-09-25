using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iGeospatial.Geometries.Operations;
using iGeospatial.Geometries;
using iGeospatial.Coordinates;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using sds = Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;
using NetCDFInterop;
using RDotNet;
using REngineAddon.Extension;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Data;
using SocialExplorer.IO.FastDBF;

namespace testGeos
{
        class Program
        {
                static REngine GetRInt()
                {
                        REngine.SetEnvironmentVariables();
                        return REngine.GetInstance();
                }
                static void Main(string[] args)
                {
                        //Int64 time;
                        //GetTimeStamp(out time);
                        //Console.WriteLine(time);
                        //Console.ReadKey();
                        // TestTimer();
                        //TestReadNC4();
                        //TestSubString();
                        //TestRAddon();
                        //TestHdf();
                        //TestDbf();
                        //string a = "2.34";
                        //int dotInd = a.IndexOf('.');
                        //Console.WriteLine(a.Substring(dotInd));
                        Console.WriteLine("top");
                        TestColor("center");
                        Console.WriteLine("bottom");
                        Console.ReadKey();
                }

                static void TestColor(string value) {
                        Console.BackgroundColor = ConsoleColor.Blue;
                        Console.ForegroundColor = ConsoleColor.White;

                        Console.WriteLine("R run >> " + value);

                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                }

                static void TestDbf() {
                        string dbfFile = @"C:\Users\HUZENGYUN\Desktop\test\c.dbf";
                        var odbf = new DbfFile(Encoding.GetEncoding(65001));
                        odbf.Open(dbfFile, System.IO.FileMode.Open);

                        var header = odbf.Header;
                        for (int i = 0; i < header.ColumnCount; i++) {
                                Console.WriteLine(header[i].Name);
                        }
                        DbfRecord record = odbf.Read(0);
                        int ind = 0;
                        while (record != null && ind < 10) {
                                ind++;
                                var a = float.Parse(record["s_type"]);
                                Console.WriteLine(a);
                                record = odbf.ReadNext();
                        }
                        odbf.Close();
                }

                static void TestHdf() {
                        GdalConfiguration.ConfigureGdal();
                        GdalConfiguration.ConfigureOgr();
                        string hdf = @"D:\testhdf\MOD09A1.hdf";
                        var ds = Gdal.Open(hdf,Access.GA_ReadOnly);
                        ds.Dispose();
                }
                static void TestRAddon()
                {
                        REngine eng = GetRInt();
                        double d = eng.GetReal("1.1");
                        List<double> ld = eng.GetRealList("c(1,2.2,3.2)");
                        Console.WriteLine(d);
                        ld.ForEach(_d => Console.WriteLine(_d));
                        int i = eng.GetInt("1");
                        List<int> li = eng.GetIntList("c(1,2,3,4)");
                        Console.WriteLine("e = 1,r = " + i);
                        li.ForEach(_i => Console.WriteLine(_i));
                        string s = eng.GetString("'ibas'");
                        Console.WriteLine(s);
                }
                static void TestSubString()
                {
                        string a = "hahahah";
                        Console.WriteLine(a.Substring(0, 20));
                }
                static void TestReadNC4()
                {
                        string nc4 = @"C:\Users\HUZENGYUN\Desktop\GLDAS_NOAH025_3H.A20191106.1200.021.nc4";
                        Dataset ds = Gdal.Open(nc4, Access.GA_ReadOnly);
                        ds.Dispose();
                }
                static void TestTimer()
                {
                        System.Timers.Timer t = new System.Timers.Timer(1000);
                        t.Elapsed += new System.Timers.ElapsedEventHandler((obj, tar) => {
                                Console.WriteLine("do once");
                        });
                        t.Enabled = true;
                        t.AutoReset = true;
                        t.Start();
                }
                static void TestGeo()
                {
                        Console.WriteLine("请通过 [程序名] [模块名] 获取相关帮助文档");
                        Console.WriteLine("模块如下:");
                        List<string> helpList = new List<string>() {
                                "vector.polygonize", "矢量要素转面(GEOS实现)",
                                "raster.Polygonize","栅格矢量化(GDAL实现)",
                                "mergeBand", "合并多个波段到一个文件(GDAL实现)",
                                "calcBand","波段计算(GDAL实现)",
                                "vecotr.rasterize","矢量栅格化(GDAL实现)",
                                "changeField","修改矢量属性表信息"
                        };
                        StringBuilder sb = new StringBuilder();
                        int len = 0;
                        for (int i = 0; i < helpList.Count; i += 2)
                        {
                                len = len > helpList[i].Length ? len : helpList[i].Length;
                        }
                        len += 8;
                        for (int i = 0; i < len; i++) sb.Append(" ");
                        for (int i = 0; i < helpList.Count; i += 2)
                        {
                                Console.WriteLine(helpList[i] + sb.ToString().Substring(0, len - helpList[i].Length) + helpList[i + 1]);
                        }
                        Console.ReadKey();
                        }
                static void GetTimeStamp(out Int64 time)
                {
                        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        time = Convert.ToInt64(ts.TotalSeconds);
                }
        }

}
