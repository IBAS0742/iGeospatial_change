using RDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageR.Op
{
        class GetBands
        {
                public static void Help(string commandName) {
                        Console.WriteLine("* args = [commandName,intif,outtif,bands]");
                        Console.WriteLine("* bands = 1#2#3");
                }
                /**
                 * args = [commandName,intif,outtif,bands]
                 * bands = 1#2#3
                 */
                static public void GetBandsHelp(REngine eng, string commandName, string[] args) {
                        if (args.Length == 4) {
                                List<int> bands = new List<int>();
                                args[3].Split('#').ToList().ForEach(b => {
                                        bands.Add(int.Parse(b));
                                });
                                DoGetBands(eng,args[1],args[2],bands);
                        } else
                        {
                                Help(commandName);
                                eng.Dispose();
                        }
                }
                static public void DoGetBands(REngine eng,string inTif,string outTif,List<int> bands) {
                        DebugForR dfr = new DebugForR(eng);
                        StringBuilder sb = new StringBuilder();
                        int t = 0;
                        inTif = inTif.Replace("\\","\\\\");
                        outTif = outTif.Replace("\\","\\\\");
                        sb.Clear();
                        dfr.Command = "library('raster');";
                        bands.ForEach(b => {
                                sb.Append((t == 0 ? "" : ",") + "t" + t);
                                dfr.Command = "t" + t + "=raster('" + inTif + "',band=" + b + ")";
                                t++;
                        });
                        dfr.Command = "writeRaster(stack(" + sb.ToString() + "),\"" + outTif + "\",overwrite=TRUE);";
                        dfr.Command = "#over";
                        eng.Dispose();
                }
        }
}
