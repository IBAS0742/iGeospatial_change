using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDAL = OSGeo.GDAL;

namespace GdalUtils.Tools
{
        public class RasterBandOP
        {
                public class MergeBand
                {
                        public static void help()
                        {
                                Console.WriteLine("程序名 mergeBand 输出栅格路径 inRaster1 inBand1 inRaster2 inBand2");
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
                        public static void ToMergeMultiBnadToOne(string[] args)
                        {
                                Dictionary<string, int[]> dic = new Dictionary<string, int[]>();
                                if (args.Length % 2 == 0 && args.Length > 2)
                                {
                                        List<int> bands = new List<int>();
                                        int bandCount = 0;
                                        for (int i = 2; i < args.Length; i += 2)
                                        {
                                                bands.Clear();
                                                args[i + 1].Split('#').ToList().ForEach(b => {
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
                                }
                        }
                        public static void ToMergeMultiBandToOne(string outTif, Dictionary<string, int[]> bands, int bandCount)
                        {
                                GDAL.Dataset ds = null;
                                int curBand = 1;
                                foreach (KeyValuePair<string, int[]> kvp in bands)
                                {
                                        OSGeo.GDAL.Dataset inds = OSGeo.GDAL.Gdal.Open(kvp.Key, OSGeo.GDAL.Access.GA_ReadOnly);
                                        if (ds == null)
                                        {
                                                ds = Utils.CreateRaster.ToCreateRaster(outTif, bandCount, inds);
                                        }
                                        for (int i = 0; i < kvp.Value.Length; i++)
                                        {
                                                Utils.CopyRasterBand.ToCopyRasterBand(inds, ds, kvp.Value[i], curBand);
                                                curBand++;
                                        }
                                        inds.Dispose();
                                }
                                ds.Dispose();
                        }
                }
                public class CalcBnad {
                        static Dictionary<string, GDAL.DataType> typeString = new Dictionary<string, GDAL.DataType>() {
                                {"byte", GDAL.DataType.GDT_Byte },
                                {"short", GDAL.DataType.GDT_CInt16 },
                                {"int", GDAL.DataType.GDT_CInt32 },
                                {"float", GDAL.DataType.GDT_Float32 },
                                {"double",GDAL.DataType.GDT_Float64 }
                        };
                        public static void help() {
                                Console.WriteLine("程序名 calcBand out.tif outBandSize tmpBandSize outBandType calcTifCount tif1 band1 ... exp1 exp2");
                                Console.WriteLine("out.tif\t是计算结果存储到指定的文件中");
                                Console.WriteLine("outBandSize\t指定out.tif有几个波段");
                                Console.WriteLine("tmpBandSize\t需要几个临时波段（后面细讲）");
                                Console.WriteLine("outBandType\t指定out.tif的像素类型，有byte、short、int、float、double可选");
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
                                Console.WriteLine("值得说明的一点是，b6 = bb + 1 并不会写回到 tif1 文件中");
                                Console.WriteLine("所以不用特地申请一个临时波段来存放 b6 + 1 的结果");
                                Console.WriteLine("");
                                Console.ReadKey();
                        }
                        public static void ToCalcBandHelp(string[] args)
                        {
                                try
                                {
                                        ToCalcBand(args);
                                } catch (Exception e)
                                {
                                        Console.WriteLine(e.Message);
                                        Console.WriteLine("-------------请按照帮助文档进行使用--------------");
                                        help();
                                }
                        }
                        /**
                         * 这里所有的波段大小都是一致的否则不好运算，但是会全部和 第一个输入 栅格统一，运算过程，全部内容会转换为 double 进行计算
                         * args[0] = "calcBand"
                         * args[1] = "out.tif"
                         * args[2] = "2" // 表示输出的tif有两个波段                       数据集 1，2       // 最终会格式化为指定的格式
                         * args[3] = "3" // 表示需要额外三个临时波段做缓存                数据集 3，4，5    // 不管什么计算，这里的几个波段都是 double 类型
                         * args[4] = "int" // 表示输出的 tif 是 int 类型的                只有 byte、short、int、float、double 五种类型
                         * args[5] = "2" // 表示有两个 tif 文件加入参与计算
                         * args[6] = "in1.tif" // 第一个输入栅格
                         * args[7] = "2#3" 取第二个和第三个波段参与计算                   数据集 6，7
                         * args[8] = "in2.tif" // 第二个输入栅格                          数据集 8
                         * args[9] = "1" // 取第一个波段参与计算
                         * args[10] = "b2 = b5 + b6"                                      b2 数据集由 b5 和 b6 相加而得到
                         * args[11] = "b3 = b7 + 1"                                       这里的 1 没有带 b 表示数字 1
                         * args[12] = "..."                                               可以有很多的表达式，但仅限于 加减乘除，认为规定常量必须在右边
                         */
                         public static void ToCalcBand(string[] args)
                        {
                                int totalBands = Int16.Parse(args[2]) + Int16.Parse(args[3]);
                                int firstIn = totalBands;
                                int inTifCount = Int16.Parse(args[5]);
                                if (Int16.Parse(args[2]) < 1) {
                                        // 输出波段少于1，
                                        throw new Exception("输出波段不能少于一个");
                                }
                                if (Int16.Parse(args[5]) < 1)
                                {
                                        throw new Exception("需要的tif文件至少要有一个");
                                }
                                if (inTifCount * 2 + 5 == args.Length)
                                {
                                        throw new Exception("不能没有表达式");
                                }
                                if (!typeString.ContainsKey(args[4].ToLower()))
                                {
                                        throw new Exception(args[4] + "类型不存在： byte、short、int、float、double 只有这五种类型可选");
                                }
                                Dictionary<string, int[]> dic = new Dictionary<string, int[]>();
                                List<int> bands = new List<int>();
                                List<string> exp = new List<string>();
                                for (int i = 6;i < inTifCount * 2 + 6;i+=2)
                                {
                                        bands.Clear();
                                        args[i + 1].Split('#').ToList().ForEach(b => {
                                                bands.Add(Int16.Parse(b));
                                        });
                                        if (bands.Count == 0)
                                        {
                                                bands.Add(1);
                                        }
                                        totalBands += bands.Count;
                                        dic.Add(args[i], bands.ToArray());
                                }
                                for (int i = inTifCount * 2 + 6;i < args.Length;i++) {
                                        exp.Add(args[i]);
                                }
                                ToCalcBand(args[1], typeString[args[4].ToLower()], Int16.Parse(args[2]), dic, totalBands,firstIn + 1,exp,args[6]);
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
                                GDAL.DataType type,
                                int outBands,
                                Dictionary<string,int[]> bands,
                                int tmpBnds,
                                int firstIn,
                                List<string> exp,
                                string firstInTif) {
                                // 创建输出波段和临时波段
                                Console.WriteLine(">>>>>> 创建临时文件");
                                OSGeo.GDAL.Dataset inds = OSGeo.GDAL.Gdal.Open(firstInTif, OSGeo.GDAL.Access.GA_ReadOnly);
                                GDAL.Dataset tmpTif = Utils.CreateRaster.CreateTmpRaster(tmpBnds, inds.RasterXSize, inds.RasterYSize);
                                GDAL.Dataset outTif = Utils.CreateRaster.ToCreateRaster(outPath, outBands, inds, type);
                                inds.Dispose();
                                Console.WriteLine(">>>>>> 将所有波动存入到临时文件中");
                                // 将输入文件全部转储到 临时 文件中
                                int curBand = firstIn;
                                foreach (KeyValuePair<string, int[]> kvp in bands)
                                {
                                        OSGeo.GDAL.Dataset _inds = OSGeo.GDAL.Gdal.Open(kvp.Key, OSGeo.GDAL.Access.GA_ReadOnly);
                                        for (int i = 0; i < kvp.Value.Length; i++)
                                        {
                                                Utils.CopyRasterBandDouble.ToCopyRasterBandToDouble(_inds, tmpTif, kvp.Value[i], curBand);
                                                curBand++;
                                        }
                                        _inds.Dispose();
                                }
                                Console.WriteLine(">>>>>> 开始遍历计算");
                                // 可以开始计算了
                                Utils.RasterBandOp.SingleDoubleFile.Op op = new Utils.RasterBandOp.SingleDoubleFile.Op(tmpTif);
                                for (int i = 0;i < exp.Count;i++)
                                {
                                        Console.WriteLine(">>>>>> 当前表达式:" + exp[i]);
                                        op.ToCalc(exp[i]);
                                }
                                Console.WriteLine(">>>>>> 计算完成开始准备写出");
                                // 计算完成，将条带写出
                                for (int i = 0;i < outBands;i++)
                                {
                                        Utils.CopyRasterBandFromDouble.ToCopyRasterBandFromDouble(tmpTif, outTif, i + 1, i + 1, (Utils.CopyRasterBandFromDouble.ResultType)type);
                                }
                                tmpTif.Dispose();
                                outTif.Dispose();
                        }
                }
        }
}
