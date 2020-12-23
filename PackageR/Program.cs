using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDotNet;

// https://archive.codeplex.com/?p=rdotnet
// https://github.com/rdotnet/rdotnet
namespace PackageR
{
        class Program
        {
                public delegate void FunctionName(REngine eng,string command,string[] args);
                // args[]
                static void Main(string[] args)
                {
                        if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\R.config")) {
                                string[] lines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"\R.config");
                                if (lines.Length == 2) {
                                        InitR(lines[0],lines[1]);
                                }
                        }
                        InitR(
                                AppDomain.CurrentDomain.BaseDirectory + @"\CopyR\bin\i386",
                                AppDomain.CurrentDomain.BaseDirectory + @"\CopyR");

                        //REngine.SetEnvironmentVariables(@"C:\Program Files\R\R-3.6.2\bin\i386",
                        //        @"C:\Program Files\R\R-3.6.2");
                        REngine eng = REngine.GetInstance();
                        Help help = new Help();
                        help.AddModule("interact", "▲进入交互模式", Op.InterAct.DoInterAct);
                        help.AddModule("GetDataset", "▲获取 nc4 文件中的数据集名称", Op.ReadNc4ToTif.GetNc4DatasetHelp);
                        help.AddModule("ReadNc4ToTif", "▲读取 nc4 中的数据集写入到 tif 文件中", Op.ReadNc4ToTif.DoReadNc4ToTifHelp);
                        help.AddModule("CalcBandGetAverage", "▲计算多个tif的均值",Op.CalcBand.GetAverage.GetAverageHelp);
                        help.AddModule("CalcBandGetSum", "▲计算多个tif的和",Op.CalcBand.GetSum.GetSumHelp);
                        help.AddModule("ChangeBandDataType", "▲修改波段数据类型",Op.ChangeDataType.ChangeDataTypeHelp);
                        help.AddModule("GetBands", "▲提取波段",Op.GetBands.GetBandsHelp);
                        help.AddModule("GetInfo", "▲获取文件的信息",Op.GetInfo.DoOpHelp);
                        help.AddModule("RunRFile", "▲执行R文件",Op.RunRFile.RunRFileHelp);
                        help.AddModule("ClipTifByShp", "▲根据shp裁剪tif",Op.ClipTifByShp.ToClipTifByShp);
                        help.AddModule("CheckNC4", "▲检查一个文件夹下的所有nc4文件是否正常",Op.CheckNcFilesInFolder.ToChek);
                        help.AddModule("RunRAsTplTiff", "▲根据模板语言执行R处理tiff文件",Op.RunRAsTplTiff.ToRunAsTplTiff);
                        help.AddModule("RunRAsTplDbf", "▲根据模板语言执行R处理dbf文件",Op.RunRAsTplDbf.ToRunRAsTplDbf);
                        help.AddModule("RunRAsTplNC4", "▲根据模板计算nc4文件", Op.DoForOneNC4.ToDoForOneNC4);
                        help.Run(eng, args);
                        //help.Run(eng, new string[] {
                        //        "RunRAsTplNC4",
                        //        @"H:\temp\GMP\3B-DAY.MS.MRG.3IMERG.20191101-S000000-E235959.V06.nc4",
                        //        @"C:\Users\HUZENGYUN\Documents\git\cshap\iGeospatial-master\Version 1.1\Open\PackageR\bin\Release\tpl.txt"
                        //});
                        //help.Run(eng, new string[] {
                        //        "RunRAsTplTiff",
                        //        "convert",
                        //        @"D:\codes\cshap\iGeospatial-master\Version 1.1\Open\PackageR\bin\Release\calc.txt"
                        //});
                }
                static bool init = false;
                static void InitR(string bin,string bp) {
                        if (init) {
                                return;
                        }
                        init = true;
                        REngine.SetEnvironmentVariables(bin,bp);
                }
                private class Help
                {
                        Dictionary<string, FunctionName> funcDic = new Dictionary<string, FunctionName>();
                        List<string> helpList = new List<string>();
                        public Help() { }
                        public void AddModule(string name, string doc, FunctionName func)
                        {
                                helpList.Add(name);
                                helpList.Add(doc);
                                funcDic.Add(name, func);
                        }
                        public void Run(REngine eng,string[] args)
                        {
                                if (args.Length == 0)
                                {
                                        ShowHelp();
                                }
                                else
                                {
                                        if (funcDic.ContainsKey(args[0]))
                                        {
                                                funcDic[args[0]](eng,args[0],args);
                                        }
                                        else
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
                                Console.ReadKey();
                        }
                }
        }
}
