using RDotNet;
using System;
using REngineAddon.SailMethod;

namespace PackageR.Op
{
        class GetInfo
        {
                static public void DoOp(REngine eng, string file)
                {
                        DebugForR dbr = new DebugForR(eng);
                        dbr.Command = "library('raster')";
                        dbr.Command = "library('rgdal')";
                        dbr.Command = "library('ncdf4')";
                        GetInfoNc4 info = new GetInfoNc4(eng, file);
                        Console.WriteLine(info);
                        eng.Dispose();
                        Console.ReadKey();
                }

                static public void DoOpHelp(REngine eng, string commandName, string[] args)
                {

                        if (args.Length == 2)
                        {
                                if (AboutFile.Exists(args[1]) == -1)
                                {
                                        DoOp(eng, args[1].Replace("\\","\\\\"));
                                }
                                else
                                {
                                        Help(commandName);
                                        eng.Dispose();
                                }
                        }
                        else
                        {
                                Help(commandName);
                                eng.Dispose();
                        }
                }

                static public void Help(string commandName)
                {
                        Console.WriteLine("usage :");
                        Console.WriteLine("program.exe " + commandName + " file");
                        Console.WriteLine("file 是要获取信息的文件，可以是");
                        Console.WriteLine("\t1.\ttif 文件");
                        Console.WriteLine("\t2.\tenvi .dat 文件");
                        Console.WriteLine("\t3.\thdf 文件");
                }
        }
}
