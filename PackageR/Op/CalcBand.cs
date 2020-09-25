using RDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageR.Op
{
        class CalcBand
        {
                public class GetAverage
                {
                        public static void Help(string commandName) {
                                Console.WriteLine("usage :");
                                Console.WriteLine("program.exe " + commandName + " out.tif bands in.tif1 in.tif2 ...");
                                Console.WriteLine("out.tif 保存结果的tif路径，如果文件存在将被覆盖");
                                Console.WriteLine("bands 参与计算的波段 1#2#3 表示每个输入tif都取1 2 3 三个波段");
                                Console.WriteLine("in.tif1 int.tif2 ... 要参与计算的tif文件");
                                Console.WriteLine("");
                                Console.WriteLine("example:");
                                Console.WriteLine("program.exe " + commandName + " out.tif 1#3#2 1.tif 2.tif");
                                Console.WriteLine("表示将 1.tif 和 2.tif 的 1 波段加和取平均数存到 out.tif 的 1 波段");
                                Console.WriteLine("　再将 1.tif 和 2.tif 的 3 波段加和取平均数存到 out.tif 的 2 波段");
                                Console.WriteLine("　再将 1.tif 和 2.tif 的 2 波段加和取平均数存到 out.tif 的 3 波段");
                        }
                        // args[1,2,3,4,...] = [out.tif bands in.tif1 in.tif2 ...]
                        static public void GetAverageHelp(REngine eng, string commandName, string[] args)
                        {
                                // 如果只有一个输入波段就显得很奇怪
                                if (args.Length > 4)
                                {
                                        List<int> bands = new List<int>();
                                        List< string > inTifs = new List<string>();
                                        args[2].Split('#').ToList().ForEach(sp => {
                                                bands.Add(Int16.Parse(sp.Trim()));
                                        });
                                        for (int i = 3;i < args.Length;i++)
                                        {
                                                inTifs.Add(args[i]);
                                        }
                                        DoGetAverage(eng, args[1], bands, inTifs);
                                } else
                                {
                                        Help(commandName);
                                        eng.Dispose();
                                }
                        }
                        static public void DoGetAverage(
                                REngine eng,
                                string outTif,List<int> bands,List<string> inTifs) {
                                DebugForR dfr = new DebugForR(eng);
                                StringBuilder sb_produce = new StringBuilder();
                                StringBuilder sb_all = new StringBuilder();
                                int c = 0;
                                dfr.Command = "library('raster');";
                                dfr.Command = "library('rgdal');";
                                Console.WriteLine("band.Count = " + bands.Count);
                                bands.ForEach(band => {
                                        int t = 0;
                                        sb_produce.Clear();
                                        dfr.Command = "# 开始计算【波段" + band + "】的平均值";
                                        inTifs.ForEach(tif => {
                                                sb_produce.Append((t == 0 ? "" : "+") + "t" + t);
                                                dfr.Command = "t" + t + " = raster(\"" 
                                                                + tif.Replace("\\","\\\\") + 
                                                                "\",band=" + band + ")";
                                                t++;
                                        });
                                        dfr.Command = "o" + c + " = (" + sb_produce.ToString() + ") / " + inTifs.Count;
                                        sb_all.Append((c == 0 ? "" : ",") + "o" + c);
                                        c++;
                                });
                                dfr.Command = "writeRaster(stack(" + sb_all.ToString() + "),\"" + outTif.Replace("\\", "\\\\") + "\",overwrite=TRUE);";
                                dfr.Command = "# 计算完成";
                                eng.Dispose();
                        }
                }
                public class GetSum
                {
                        public static void Help(string commandName)
                        {
                                Console.WriteLine("usage :");
                                Console.WriteLine("program.exe " + commandName + " out.tif bands in.tif1 in.tif2 ...");
                                Console.WriteLine("out.tif 保存结果的tif路径，如果文件存在将被覆盖");
                                Console.WriteLine("bands 参与计算的波段 1#2#3 表示每个输入tif都取1 2 3 三个波段");
                                Console.WriteLine("in.tif1 int.tif2 ... 要参与计算的tif文件");
                                Console.WriteLine("");
                                Console.WriteLine("example:");
                                Console.WriteLine("program.exe " + commandName + " out.tif 1#3#2 1.tif 2.tif");
                                Console.WriteLine("表示将 1.tif 和 2.tif 的 1 波段加和取平均数存到 out.tif 的 1 波段");
                                Console.WriteLine("　再将 1.tif 和 2.tif 的 3 波段加和取平均数存到 out.tif 的 2 波段");
                                Console.WriteLine("　再将 1.tif 和 2.tif 的 2 波段加和取平均数存到 out.tif 的 3 波段");
                        }
                        // args[1,2,3,4,...] = [out.tif bands in.tif1 in.tif2 ...]
                        static public void GetSumHelp(REngine eng, string commandName, string[] args)
                        {
                                // 如果只有一个输入波段就显得很奇怪
                                if (args.Length > 4)
                                {
                                        List<int> bands = new List<int>();
                                        List<string> inTifs = new List<string>();
                                        args[2].Split('#').ToList().ForEach(sp => {
                                                bands.Add(Int16.Parse(sp.Trim()));
                                        });
                                        for (int i = 3; i < args.Length; i++)
                                        {
                                                inTifs.Add(args[i]);
                                        }
                                        DoGetSum(eng, args[1], bands, inTifs);
                                }
                                else
                                {
                                        Help(commandName);
                                        eng.Dispose();
                                }
                        }
                        static public void DoGetSum(
                                REngine eng,
                                string outTif, List<int> bands, List<string> inTifs)
                        {
                                DebugForR dfr = new DebugForR(eng);
                                StringBuilder sb_produce = new StringBuilder();
                                StringBuilder sb_all = new StringBuilder();
                                int c = 0;
                                dfr.Command = "library('raster');";
                                Console.WriteLine("band.Count = " + bands.Count);
                                bands.ForEach(band => {
                                        int t = 0;
                                        sb_produce.Clear();
                                        dfr.Command = "# 开始计算【波段" + band + "】的和";
                                        inTifs.ForEach(tif => {
                                                sb_produce.Append((t == 0 ? "" : "+") + "t" + t);
                                                dfr.Command = "t" + t + " = raster(\""
                                                                + tif.Replace("\\", "\\\\") +
                                                                "\",band=" + band + ")";
                                                t++;
                                        });
                                        dfr.Command = "o" + c + " = (" + sb_produce.ToString() + ")";
                                        sb_all.Append((c == 0 ? "" : ",") + "o" + c);
                                        c++;
                                });
                                dfr.Command = "writeRaster(stack(" + sb_all.ToString() + "),\"" + outTif.Replace("\\", "\\\\") + "\",overwrite=TRUE);";
                                dfr.Command = "# 计算完成";
                                eng.Dispose();
                        }
                }
        }
}
