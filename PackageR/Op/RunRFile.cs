using RDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageR.Op
{
        class RunRFile
        {
                static private void RunRFileUsage(string commandName)
                {
                        Console.WriteLine("usage :");
                        Console.WriteLine("program.exe " + commandName + " filepath.nc4 filepath.tif ds1 ds2 ...");
                        Console.WriteLine("filepath.nc4 是要读取的 nc4 文件");
                        Console.WriteLine("filepath.tif 是要写出的 tif 文件，如果文件存在会先被删除");
                        Console.WriteLine("ds1 ds2 ...  是 nc4 文件的数据集名称");
                }
                // args = [RFile]
                static public void RunRFileHelp(REngine eng, string commandName, string[] args)
                {
                        if (args.Length == 2)
                        {
                                if (AboutFile.Exists(args[1]) == -1)
                                {
                                        DoRunRFile(eng, args[1]);
                                } else
                                {
                                        RunRFileUsage(commandName);
                                        eng.Dispose();
                                }
                        }
                        else
                        {
                                RunRFileUsage(commandName);
                                eng.Dispose();
                        }
                }
                static private void DoRunRFile(REngine eng, string rFile)
                {
                        DebugForR dbr = new DebugForR(eng);

                        StreamReader sr = new StreamReader(rFile, Encoding.Default);
                        while ((dbr.Command = sr.ReadLine()) != null) {}

                        //eng.Evaluate(dbr.Command);
                        eng.Dispose();
                        Console.WriteLine("执行成功");
                }
        }
}
