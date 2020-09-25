using RDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageR.Op
{
        class ChangeDataType
        {
                static Dictionary<string, string> typeMapping = new Dictionary<string, string>() {
                        { "bool", "LOG1S"},
                        { "int16", "INT1S"},
                        { "uint16", "INT1U"},
                        { "int32", "INT2S"},
                        { "uint32", "INT2U"},
                        { "int64", "INT4S"},
                        { "uint64", "INT4U"},
                        { "float", "FLT4S"},
                        { "double", "FLT8S"},
                };
                static void Help(string commandName) {
                        Console.WriteLine("使用方法");
                        Console.WriteLine("program.exe " + commandName + " inTif outTif bands type");
                        Console.WriteLine("type 可以是 [bool,int16,uint16,int32,uint32,int64,uint64,float,double] ");
                }
                static public void ChangeDataTypeHelp(REngine eng, string commandName, string[] args) {
                        if (args.Length == 5) {
                                List<int> bands = new List<int>();
                                args[3].Split('#').ToList().ForEach(b => {
                                        bands.Add(int.Parse(b));
                                });
                                if (typeMapping.ContainsKey(args[4])) {
                                        DoChangeDataType(eng, args[1], args[2], bands, typeMapping[args[4]]);
                                }
                        } else {
                                Help(commandName);
                        }
                }
                static void DoChangeDataType(REngine eng, string inTif,string outTif,List<int> bands,string oType)
                {
                        DebugForR dfr = new DebugForR(eng);
                        StringBuilder sb = new StringBuilder();
                        int t = 0;
                        inTif = inTif.Replace("\\", "\\\\");
                        outTif = outTif.Replace("\\", "\\\\");
                        sb.Clear();
                        dfr.Command = "library('raster');";
                        bands.ForEach(b => {
                                sb.Append((t == 0 ? "" : ",") + "t" + t);
                                dfr.Command = "t" + t + "=raster('" + inTif + "',band=" + b + ")";
                                t++;
                        });
                        dfr.Command = "writeRaster(stack(" + sb.ToString() + "),\"" + outTif + "\",overwrite=TRUE,dataType='" + oType + "');";
                        dfr.Command = "#over";
                        eng.Dispose();
                }
        }
}
