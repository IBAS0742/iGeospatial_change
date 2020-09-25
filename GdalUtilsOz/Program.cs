using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GdalUtilsOz
{
        class Program
        {
                public delegate void FunctionName(string[] args,string name);
                public static Utils.Setting setting = new Utils.Setting();
                static public iGeospatial.Geometries.GeometryFactory GeometryFactory = null;
                static private void Init()
                {
                        //OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "UTF - 8");
                        GdalConfiguration.ConfigureGdal();
                        GdalConfiguration.ConfigureOgr();
                        OSGeo.OGR.Ogr.RegisterAll();
                        OSGeo.GDAL.Gdal.AllRegister();
                        OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "Yes"); // 支持中文
                        GeometryFactory = new iGeospatial.Geometries.GeometryFactory();
                        setting.init();
                }
                static void Main(string[] args)
                {
                        Init();
                        Help help = new Help();
                        help.AddModule("vector.polygonize", "矢量要素转面(GEOS实现)", Tools.Vector.Polygonize.ToPolygonize);
                        help.AddModule("raster.Polygonize", "栅格矢量化(GDAL实现)", Tools.Raster.Polygonize.ToPolygonize);
                        help.AddModule("mergeBand", "合并多个波段到一个文件(GDAL实现)", Tools.Raster.RasterBandOp.MergeBand.ToMergeMultiBnadToOne);
                        help.AddModule("vecotr.rasterize", "矢量栅格化(GDAL实现)", Tools.Vector.Rasterize.ToRasterize);
                        help.AddModule("changeField", "修改矢量属性表信息", Tools.Vector.ChangeField.ToChangeField);
                        help.AddModule("SaveAsSer", "序列化矢量文件以加快读取速度", Tools.Vector.ShiftVectorSer.SaveAsSer.ToSaveAsSer);
                        help.AddModule("SaveAsVector", "将序列化文件转存为矢量文件", Tools.Vector.ShiftVectorSer.SaveAsVector.ToSaveAsVector);
                        help.AddModule("selectFeature.show", "▲显示要素信息", Tools.Others.SelectFeature.ToShowFeatureInfo);
                        help.AddModule("selectFeature", "▲筛选要素", Tools.Others.SelectFeature.ToSelectFeature);
                        help.AddModule("createVectorFromText", "从文本文件创建矢量文件", Tools.Others.CreateVectorFromText.ToCreateVectorFromText);
                        help.AddModule("calcBand", "波段计算(GDAL实现)", Tools.Raster.RasterBandOp.CalcBnad.ToCalcBand);
                        help.AddModule("getInfo", "获取tiff信息", Tools.Raster.GetInfo.ToGetInfo);
                        help.AddModule("changeTransform", "修改transform", Tools.Raster.ChangeTransform.ToChangeTransform);
                        help.AddModule("replaceDbfTiff", "将 dbf 的值赋到 tiff 上", Tools.Raster.ReplaceAsDbfValue.ToReplaceAsDbfValue);
                        help.AddModule("createRaster", "创建栅格(x double)", Tools.Raster.CreateRandRaster.ToCreateRandRaster);
                        help.AddModule("rasterStatic", "获取栅格数据(double)", Tools.Raster.Static.ToRasterStatic);
                        help.Run(args);
                        #region 测试 rasterStatic
                        //for (int i = 2018; i <= 2018; i++) {
                        //        Console.WriteLine(i);
                        //        help.Run(new string[] {
                        //                "rasterStatic",
                        //                @"U:\tmp\zsf\" + i + "\\al",
                        //                @"U:\tmp\zsf\" + i + "\\result.txt",
                        //        });
                        //}
                        //Console.ReadKey();
                        #endregion
                        #region 测试 createRaster
                        //help.Run(new string[] {
                        //        "createRaster",
                        //        @"I:\tmp\zip\a.tif",
                        //        "nnd"
                        //});
                        #endregion
                        #region 测试 replaceDbfTiff
                        //help.Run(new string[] {
                        //        "replaceDbfTiff",
                        //        @"C:\Users\HUZENGYUN\Documents\Tencent Files\1091021117\FileRecv\DATA\s_type.tif",
                        //        @"C:\Users\HUZENGYUN\Documents\Tencent Files\1091021117\FileRecv\DATA\s_type.rep.tif",
                        //        @"C:\Users\HUZENGYUN\Documents\Tencent Files\1091021117\FileRecv\DATA\s_type.tif.vat.dbf",
                        //        "Value",
                        //        "s_type"
                        //});
                        #endregion
                        #region 测试 changeTransform
                        //help.Run(new string[] {
                        //        "changeTransform",
                        //        @"D:\testhdf\test\tif\out.sur_refl_b01.tif",
                        //        @"D:\testhdf\test\tif\out1.tif",
                        //        "115.470053824",
                        //        "0.0051",
                        //        "0",
                        //        "39.999999996",
                        //        "0",
                        //        "-0.0038"
                        //});
                        #endregion
                }
                private class Help {
                        Dictionary<string, FunctionName> funcDic = new Dictionary<string, FunctionName>();
                        List<string> helpList = new List<string>();
                        public Help() { }
                        public void AddModule(string name,string doc, FunctionName func) {
                                helpList.Add(name);
                                helpList.Add(doc);
                                funcDic.Add(name, func);
                        }
                        public void Run(string[] args)
                        {
                                if (args.Length == 0) {
                                        ShowHelp();
                                } else
                                {
                                        if (funcDic.ContainsKey(args[0])) {
                                                funcDic[args[0]](args,args[0]);
                                        } else
                                        {
                                                ShowHelp();
                                        }
                                }
                        }
                        public void ShowHelp()
                        {
                                Console.WriteLine("请通过 [程序名] [模块名] 获取相关帮助文档");
                                Console.WriteLine("模块如下，带▲表示未测试:");
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
                                Console.WriteLine("");
                                Console.WriteLine("注意所有的功能，如果涉及到矢量栅格【写/创建/无中生有】操作均需要注意配置信息");
                                Console.ReadKey();
                        }
                }
        }
}
