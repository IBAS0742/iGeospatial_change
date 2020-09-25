using RDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageR.Op {
        class ClipTifByShp {
                static void help(string commandName) {
                        Console.WriteLine("程序名 " + commandName + " intif shp outtif bands extent");
                        Console.WriteLine("bands 需要裁剪的波段 1#2");
                        Console.WriteLine("extent = true or false");
                        Console.WriteLine("extent = 默认为 true");
                        Console.WriteLine("extent = true 表示根据shp的extent进行裁剪");
                }
                static public void ToClipTifByShp(REngine eng, string commandName, string[] args) {
                        if (args.Length == 6) {
                                ToClipTifByShp(eng,args[1], args[2], args[3],args[4], args[5].ToLower() == "false");
                        } else {
                                eng.Dispose();
                                help(commandName);
                        }
                }
                static void ToClipTifByShp(REngine eng, string inTif,string shp,string outTif,string bands,bool mark) {
                        inTif = inTif.Replace("\\", "\\\\");
                        shp = shp.Replace("\\", "\\\\");
                        outTif = outTif.Replace("\\", "\\\\");
                        List<string> bandList = new List<string>();
                        StringBuilder sb = new StringBuilder();
                        bands.Split('#').ToList().ForEach(a => {
                                bandList.Add(a.Trim());
                        });
                        DebugForR dfr = new DebugForR(eng);
                        dfr.Command = "library(maptools)";
                        dfr.Command = "library(raster)";
                        dfr.Command = "library(rgdal)";
                        dfr.Command = "shp = readOGR('" + shp + "')";
                        int bCount = 0;
                        bandList.ForEach(b => {
                                sb.Append(bCount == 0 ? "":",");
                                sb.Append("cr" + bCount);
                                dfr.Command = "r" + bCount + " = raster('" + inTif + "',band=" + (bCount + 1) + ")";
                                dfr.Command = "cr" + bCount + " = crop(r" + bCount + ",shp)";
                                if (mark) {
                                        dfr.Command = "cr" + bCount + " = mask(cr" + bCount + ",shp);";
                                }
                                bCount++;
                        });

                        // dfr.Command = "writeRaster(stack(" + sb.ToString() + "),\"" + outTif + "\",overwrite=TRUE);";
                        if (mark) {
                                dfr.Command = "writeRaster(stack(" + sb.ToString() + "),\"" + outTif + "\",overwrite=TRUE);";
                        } else {
                                dfr.Command = "writeRaster(cr,'" + outTif + "',overwrite=TRUE)";
                        }
                        eng.Dispose();
                }
        }
}
