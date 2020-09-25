using GdalUtilsOz.Utils.RasterOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using GDAL = OSGeo.GDAL;

namespace GdalUtilsOz.Tools.Raster
{
        class RasterBandOp
        {
                public class MergeBand
                {
                        public static void help(string commandName)
                        {
                                Console.WriteLine("程序名 " + commandName + " 输出栅格路径 inRaster1 inBand1 inRaster2 inBand2");
                                Console.WriteLine("inRaster(n) 表示第几个输入栅格的路径");
                                Console.WriteLine("inBand(n) 表示第几个栅格要复制到条带，多个条带用#隔开");
                                Console.WriteLine("");
                                Console.WriteLine("要求每个栅格的波段数据类型都是一致的");
                                Console.WriteLine("");
                                Console.WriteLine("例如 mergeBand out.tif in1.tif 4#1#2 in2.tif 1#2");
                                Console.WriteLine("表示将 in1.tif 的 4、2、1 和 in2.tif 的 1、2 依次复制到 out.tif 内");
                        }
                        /**
                         * args[0] = "mergeBand"
                         * args[1] = "输出栅格文件的路径"
                         * args[2] = "第一个输入栅格文件的路径"
                         * args[3] = "要复制的波段格式为 1#2#3"
                         */
                        public static void ToMergeMultiBnadToOne(string[] args,string commandName)
                        {
                                Dictionary<string, int[]> dic = new Dictionary<string, int[]>();
                                if (args.Length % 2 == 0 && args.Length > 2)
                                {
                                        List<int> bands = new List<int>();
                                        int bandCount = 0;
                                        for (int i = 2; i < args.Length; i += 2)
                                        {
                                                bands.Clear();
                                                args[i + 1].Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(b => {
                                                        bands.Add(Int16.Parse(b));
                                                });
                                                if (bands.Count == 0)
                                                {
                                                        bands.Add(1);
                                                }
                                                bandCount += bands.Count;
                                                dic.Add(args[i], bands.ToArray());
                                        }
                                        ToMergeMultiBandToOne(args[1], dic, bandCount);
                                } else
                                {
                                        help(commandName);
                                }
                        }
                        public static void ToMergeMultiBandToOne(string outTif, Dictionary<string, int[]> bands, int bandCount)
                        {
                                List<CopyRasterBandInfo> tifs = new List<CopyRasterBandInfo>();
                                GDAL.Dataset ds = null;
                                foreach (KeyValuePair<string, int[]> kvp in bands)
                                {
                                        tifs.Add(new CopyRasterBandInfo(kvp.Key, kvp.Value.ToList()));
                                }
                                GetAllBandInfo gabi = new GetAllBandInfo(tifs);
                                ds = Utils.RasterOperation.Create.CreateRaster.ToCreateRaster(outTif, bandCount, gabi.GetOneDataset());
                                gabi.CopyTo(ds, bandCount);
                                gabi.Dispose();
                                ds.Dispose();
                        }
                }
                public class CalcBnad
                {
                        public static void help(string commandName)
                        {
                                Console.WriteLine("程序名 " + commandName + " out.tif outBandSize tmpBandSize calcTifCount tif1 band1 ... exp1 exp2");
                                Console.WriteLine("out.tif\t是计算结果存储到指定的文件中");
                                Console.WriteLine("outBandSize\t指定out.tif有几个波段");
                                Console.WriteLine("tmpBandSize\t需要几个临时波段（后面细讲）");
                                Console.WriteLine("calcTifCount\t有几个栅格文件会参与计算，至少一个");
                                Console.WriteLine("tif1 band1\t第一个栅格文件路径及用到的波段，后续有 tif2 band2 等等，数量由 calcTifCount 指定");
                                Console.WriteLine("\tband1\t格式为 1#2 或 2 或 3#1#2 表示用到 1 和 2 波段 或用到 2 波段 或 用到 3 和 1 和 2 波段");
                                Console.WriteLine("exp1 exp2\t这个是表达式，表达式总共有 6*2=12 种情况，如下");
                                Console.WriteLine("bn=br+bk\tbn=br-bk\tbn=br*bk\tbn=br/bk\tbn=br\\bk\tbn=br#bk");
                                Console.WriteLine("bn=br+k \tbn=br-k \tbn=br*k \tbn=br/k \tbn=br\\k \tbn=br#k");
                                Console.WriteLine("");
                                Console.WriteLine("其中带b表示第几个波段，+-*/是加减乘除，");
                                Console.WriteLine("\\是反除，例如 a\\b=b/a");
                                Console.WriteLine("#是反减，例如 a+b=b-a");
                                Console.WriteLine("");
                                Console.WriteLine("另外，其中的bn,br,bk 表示第 n,r,k 个波段，这个波段来源如下");
                                Console.WriteLine("例如 outBandSize = 2，tmpBandSize = 3，calcTifCount = 1，band1 = 1#2#3#4");
                                Console.WriteLine("那么总的条带数量为 2 + 3 + 4 = 9 个");
                                Console.WriteLine("按照顺序,b1,b2 指的是输出文件out.tif的波段");
                                Console.WriteLine("b3,b4,b5 指的是临时波段");
                                Console.WriteLine("b6,b7,b8,b9 值得是 tif1 的 1，2，3，4 四个波段");
                                Console.WriteLine("b1=b6+b7 指的是用 tif1 的 1波段加 2 波段保存到 输出文件的 1 波段");
                                Console.WriteLine("");
                                Console.WriteLine("值得说明的一点是，b6 = b6 + 1 并不会写回到 tif1 文件中");
                                Console.WriteLine("所以不用特地申请一个临时波段来存放 b6 + 1 的结果");
                                Console.WriteLine("");
                                Console.ReadKey();
                        }
                        public static void ToCalcBandHelp(string[] args, string commandName)
                        {
                                try
                                {
                                        ToCalcBand(args, commandName);
                                }
                                catch (Exception e)
                                {
                                        Console.WriteLine(e.Message);
                                        Console.WriteLine("-------------请按照帮助文档进行使用--------------");
                                        help(commandName);
                                }
                        }
                        /**
                         * 这部分是格式化输入数据和参数检查
                         * 这里所有的波段大小都是一致的否则不好运算，但是会全部和 第一个输入 栅格统一，运算过程，全部内容会转换为 double 进行计算
                         * args[0] = "calcBand"
                         * args[1] = "out.tif"
                         * args[2] = "2" // 表示输出的tif有两个波段                       数据集 1，2       // 最终会格式化为指定的格式
                         * args[3] = "3" // 表示需要额外三个临时波段做缓存                数据集 3，4，5    // 不管什么计算，这里的几个波段都是 double 类型
                         * args[4] = "2" // 表示有两个 tif 文件加入参与计算
                         * args[5] = "in1.tif" // 第一个输入栅格
                         * args[6] = "2#3" 取第二个和第三个波段参与计算                   数据集 6，7
                         * args[7] = "in2.tif" // 第二个输入栅格                          数据集 8
                         * args[8] = "1" // 取第一个波段参与计算
                         * args[9] = "b2 = b5 + b6"                                      b2 数据集由 b5 和 b6 相加而得到
                         * args[10] = "b3 = b7 + 1"                                       这里的 1 没有带 b 表示数字 1
                         * args[11] = "..."                                               可以有很多的表达式，但仅限于 加减乘除，认为规定常量必须在右边
                         */
                        public static void ToCalcBand(string[] args,string commandName)
                        {
                                if (args.Length < 8)
                                {
                                        help(commandName);
                                        return;
                                }
                                int totalBands = Int16.Parse(args[2]) + Int16.Parse(args[3]);
                                int firstIn = totalBands;
                                int inTifCount = Int16.Parse(args[4]);
                                if (Int16.Parse(args[2]) < 1)
                                {
                                        // 输出波段少于1，
                                        throw new Exception("输出波段不能少于一个");
                                }
                                if (Int16.Parse(args[4]) < 1)
                                {
                                        throw new Exception("需要的tif文件至少要有一个");
                                }
                                if (inTifCount * 2 + 4 == args.Length)
                                {
                                        throw new Exception("不能没有表达式");
                                }
                                List<int> bands = new List<int>();
                                List<string> exp = new List<string>();
                                List<CopyRasterBandInfo> crbis = new List<CopyRasterBandInfo>();
                                for (int i = 5; i < inTifCount * 2 + 5; i += 2)
                                {
                                        bands.Clear();
                                        // 这里就是分割 1#2#3 的地方
                                        args[i + 1].Split(new char[] { '#' },StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(b => {
                                                bands.Add(Int16.Parse(b));
                                        });
                                        if (bands.Count == 0)
                                        {
                                                bands.Add(1);
                                        }
                                        totalBands += bands.Count;
                                        crbis.Add(new CopyRasterBandInfo(args[i], bands));
                                }
                                for (int i = inTifCount * 2 + 5; i < args.Length; i++)
                                {
                                        exp.Add(args[i]);
                                }
                                ToCalcBand(args[1], Int16.Parse(args[2]), crbis, totalBands, firstIn + 1, exp);
                        }

                        /**
                         * outPath      输出栅格位置
                         * type         栅格格式
                         * outBands     输出栅格的条代数
                         * bands        参与计算的栅格文件及其条带
                         * tmpBands     临时条带的数量
                         * firstIn      第几个波段开始写入输入的tif，即哪里开始是输入的tif而不是临时和输出tif的波段内容
                         * exp          表达式
                         * firstInTif   第一个输入tif，用于创建临时和输出tif
                         */
                        public static void ToCalcBand(
                                string outPath,
                                int outBands, 
                                List<CopyRasterBandInfo> crbis,
                                int tmpBnds,
                                int firstIn,
                                List<string> exp) {
                                GetAllBandInfo gabi = new GetAllBandInfo(crbis);
                                GDAL.Dataset tmpTif = Utils.RasterOperation.Create.CreateRaster.CreateTmpRaster(
                                        tmpBnds, gabi.Xsize,gabi.Ysize,gabi.Type);
                                GDAL.Dataset outTif = Utils.RasterOperation.Create.CreateRaster.ToCreateRaster(outPath,
                                        outBands,gabi.Xsize,gabi.Ysize,gabi.Type,gabi.Projection,gabi.Transform);
                                if (gabi.TypeIsSame)
                                {
                                        gabi.CopyTo(tmpTif, tmpBnds, firstIn);
                                        tmpTif.Calc(exp,gabi.NoData.Nodata);
                                        gabi.CopyTo(tmpTif, outTif, 1, outBands);
                                        gabi.Dispose();
                                        tmpTif.Dispose();
                                        outTif.Dispose();
                                } else
                                {
                                        Console.WriteLine("请仔细阅读文档，文件数据类型不一致");
                                        gabi.Dispose();
                                        return;
                                }
                        }
                }
        }
}
