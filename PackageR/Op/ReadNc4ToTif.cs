using RDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PackageR.Op
{
        class ReadNc4ToTif
        {
                // args = [nc4path outpath subdataset1 subdataset2 subdataset3 ...]
                static public void DoReadNc4ToTifHelp(REngine eng, string commandName, string[] args) { 
                        if (args.Length > 3) {
                                AboutFile.Delete(args[2]);
                                if (AboutFile.Exists(args[1]) == -1) {
                                        List<string> ds = new List<string>();
                                        for (int i = 3;i < args.Length;i++)
                                        {
                                                ds.Add(args[i]);
                                        }
                                        DoReadNc4ToTif(eng, args[1], args[2], ds);
                                }
                        } else
                        {
                                Console.WriteLine("usage :");
                                Console.WriteLine("program.exe " + commandName + " filepath.nc4 filepath.tif ds1 ds2 ...");
                                Console.WriteLine("filepath.nc4 是要读取的 nc4 文件");
                                Console.WriteLine("filepath.tif 是要写出的 tif 文件，如果文件存在会先被删除");
                                Console.WriteLine("ds1 ds2 ...  是 nc4 文件的数据集名称");
                                eng.Dispose();
                        }
                }
                static private void DoReadNc4ToTif(REngine eng,string nc4,string outPath,List<string> dataset)
                {
                        DebugForR dbr = new DebugForR(eng);
                        StringBuilder sb = new StringBuilder();
                        int count = 0;
                        nc4 = nc4.Replace("\\","\\\\");
                        outPath = outPath.Replace("\\","\\\\");
                        // 载入必要的库
                        eng.Evaluate("library(rgdal)");
                        eng.Evaluate("library(raster)");
                        eng.Evaluate("library(ncdf4)");

                        dataset.ForEach(str => {
                                sb.Append((count > 0 ? "," : "") + "r" + count);
                                dbr.Command = "r" + count + " <- raster('" + nc4 + "',varname='" + str + "');";
                                //eng.Evaluate(dbr.Command);
                                count++;
                        });
                        dbr.Command = "writeRaster(stack(" + sb.ToString() + "),'" + outPath + "')";
                        //eng.Evaluate(dbr.Command);
                        eng.Dispose();
                        Console.WriteLine("写出成功");
                }

                static public void GetNc4DatasetHelp(REngine eng,string commandName,string[] args) {
                        if (args.Length == 2) { 
                                if (AboutFile.Exists(args[1]) == -1) {
                                        GetNc4Dataset(eng,args[1]);
                                } else
                                {
                                        eng.Dispose();
                                }
                        } else
                        {
                                Console.WriteLine("usage :");
                                Console.WriteLine("program.exe " + commandName + " filepath.nc4");
                                eng.Dispose();
                        }
                }
                static private void GetNc4Dataset(REngine eng, string nc4) {
                        nc4 = nc4.Replace("\\","\\\\");
                        // 载入必要的库
                        eng.Evaluate("library(ncdf4)");

                        eng.Evaluate("nc4 = ncdf4::nc_open('" + nc4 + "');");
                        DynamicVector names = eng.Evaluate("ns <- names(nc4$var)").AsVector();
                        Console.WriteLine("names has :\n");
                        int c = 0;
                        names.ToList().ForEach(obj => {
                                Console.Write((string)obj + "\t");
                                c++;
                                if (c == 4)
                                {
                                        c = 0;
                                        Console.WriteLine();
                                }
                        });
                        Console.WriteLine("over\n\n");
                        eng.Dispose();
                }
        }
}
