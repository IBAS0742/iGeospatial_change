using RDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using REngineAddon.Extension;
using System.Text;

namespace PackageR.Op {
        class RunRAsTplTiff {
                static void help(string commandName) {
                        Console.WriteLine("program.exe " + commandName + " <target> filename");
                        Console.WriteLine("<target> 有三个选项，一个是 calc 一个是 anli 一个是 example");
                        Console.WriteLine("     calc 是计算，anli 是分析文件");
                        Console.WriteLine("             用于分析参加计算的文件的extent和大小，并且会给出一份报告");
                        Console.WriteLine("     example 将列举几个例子供参考");
                        Console.WriteLine("filename 是 代码内容");
                        Console.WriteLine("目前定义了以下关键字，（仅仅支持Tiff计算）");
                        Console.WriteLine("read v1 path");
                        Console.WriteLine("     读取 path 的第一个波段到 v1 变量，变量名用于后面计算");
                        Console.WriteLine("read v1#v2#_#v4 path");
                        Console.WriteLine("     读取 path 对应的 1 2 4 波段到 v1 v2 v4 变量中");
                        Console.WriteLine("     readRange a#_#b path");
                        Console.WriteLine("     其中 _ 表示跳过一个波段不读取，不要纠结于变量名");
                        Console.WriteLine("save v1 path");
                        Console.WriteLine("     将 v1 保存到 path 对应的文件");
                        Console.WriteLine("save v1#v2#3 path [type]");
                        Console.WriteLine("     将 v1 v2 v3 按顺序存到 path 文件中");
                        Console.WriteLine("     type 是可选的，如果没有指定就是默认类型，既计算结果的类型");
                        Console.WriteLine("     type 如果指定，可以是");
                        Console.WriteLine("        logic  0 ~ 1");
                        Console.WriteLine("        int8   -127 ~ 127  <- 不支持");
                        Console.WriteLine("        uint8  0 ~ 255");
                        Console.WriteLine("        int16  -32767 ~ 32767");
                        Console.WriteLine("        uint16 0 ~ 65535");
                        Console.WriteLine("        int32  -2,147,483,647 ~ 2,147,483,647");
                        Console.WriteLine("        uint32 0 ~ 4,294,967,296");
                        Console.WriteLine("        float  -3.4e+38 ~ 3.4e+38");
                        Console.WriteLine("        double -1.7e+308 ~ 1.7e+308");
                        //Console.WriteLine("set extAndResample minx maxx miny maxy col row method");
                        //Console.WriteLine("     其中 minx maxx miny maxy 用于裁剪，如果范围比tif大将不会裁剪");
                        //Console.WriteLine("     其中 col 和 row 为重采样大小");
                        //Console.WriteLine("     method 可以取 bilinear 或 ngb");
                        //Console.WriteLine("	method used to compute values for the new RasterLayer, should be \"bilinear\" for bilinear interpolation, or \"ngb\" for using the nearest neighbor");
                }

                static public void ToRunAsTplTiff(REngine eng, string commandName, string[] args) {
                        if (args.Length == 3) {
                                args[1] = args[1].ToLower();
                                if (args[1] == "calc") {
                                        ToRunAsTplTiffCalc(eng, args[2]);
                                } else if (args[1] == "anli") {
                                        ToRunAsTplTiffCalcAnli(eng, args[2]);
                                } else {
                                        eng.Dispose();
                                        help(commandName);
                                }
                        } else {
                                eng.Dispose();
                                help(commandName);
                        }
                }

                class TifInfo {
                        public string Path { get; set; }
                        public string BandName { get; set; }
                        public double EMaxx { get; set; }
                        public double EMaxy { get; set; }
                        public double EMinx { get; set; }
                        public double EMiny { get; set; }
                        public double Cols { get; set; }
                        public double Rows { get; set; }
                }
                static void ToRunAsTplTiffCalcAnli(REngine eng, string filename) {
                        DebugForR dfr = new DebugForR(eng);
                        string line;
                        List<TifInfo> infoList = new List<TifInfo>();
                        TifInfo recInfo = null;
                        dfr.Command = "library(rgdal)";
                        dfr.Command = "library(raster)";
                        int TifCount = 0;
                        if (File.Exists(filename)) {
                                using (System.IO.StreamReader sr = new System.IO.StreamReader(filename)) {
                                        while ((line = sr.ReadLine()) != null) {
                                                if (line.StartsWith("read ")) {
                                                        line = line.Substring("read ".Length).Trim();
                                                        ParseReadWrite(line, (tifName, bands, exist) => {
                                                                if (!exist) {
                                                                        Console.WriteLine(tifName + "文件不存在");
                                                                        throw new Exception("");
                                                                } else {
                                                                        TifCount++;
                                                                        TifInfo info = new TifInfo();
                                                                        info.BandName = "b" + TifCount;
                                                                        info.Path = tifName;
                                                                        dfr.Command = $"{info.BandName}=raster('{info.Path}',band=1);";
                                                                        infoList.Add(info);
                                                                }
                                                                return 0;
                                                        });
                                                }
                                        }
                                }
                                infoList.ForEach(info => {
                                        info.EMaxx = eng.GetReal($"{info.BandName}@extent@xmax");
                                        info.EMinx = eng.GetReal($"{info.BandName}@extent@xmin");
                                        info.EMaxy = eng.GetReal($"{info.BandName}@extent@ymax");
                                        info.EMiny = eng.GetReal($"{info.BandName}@extent@ymin");
                                        info.Cols = eng.GetReal($"{info.BandName}@ncols");
                                        info.Rows = eng.GetReal($"{info.BandName}@nrows");
                                });
                                // TifInfo tifInfo = new TifInfo();
                                Console.WriteLine("信息如下");
                                Console.WriteLine("xmin\t\txmax\t\tymin\t\tymax\t\tcols\trows\t\t文件名");
                                string formatDouble = "0.000000000";
                                infoList.ForEach(info => {
                                        if (recInfo == null) {
                                                recInfo = new TifInfo();
                                                recInfo.EMaxx = info.EMaxx;
                                                recInfo.EMinx = info.EMinx;
                                                recInfo.EMaxy = info.EMaxy;
                                                recInfo.EMiny = info.EMiny;
                                                recInfo.Rows = info.Rows;
                                                recInfo.Cols = info.Cols;
                                        } else {
                                                recInfo.EMinx = Math.Max(recInfo.EMinx, info.EMinx);
                                                recInfo.EMaxx = Math.Min(recInfo.EMaxx, info.EMaxx);
                                                recInfo.EMiny = Math.Max(recInfo.EMiny, info.EMiny);
                                                recInfo.EMaxy = Math.Min(recInfo.EMaxy, info.EMaxy);
                                                recInfo.Rows = Math.Max(recInfo.Rows, info.Rows);
                                                recInfo.Cols = Math.Max(recInfo.Cols, info.Cols);
                                        }
                                        Console.WriteLine($"{info.EMinx.ToString(formatDouble)}\t{info.EMaxx.ToString(formatDouble)}\t{info.EMiny.ToString(formatDouble)}\t{info.EMaxy.ToString(formatDouble)}\t{info.Cols}\t{info.Rows}\t{info.Path}");
                                });
                                Console.WriteLine("如果需要统一修改 extent 和 重采样");
                                Console.WriteLine("假设设置为第一个的配置");
                                Console.WriteLine($"set extAndResample {infoList[0].EMinx}  {infoList[0].EMaxx}  {infoList[0].EMiny}  {infoList[0].EMaxy}  {infoList[0].Cols}  {infoList[0].Rows} bilinear");
                                Console.WriteLine("推荐的设置，不一定是最好的");
                                Console.WriteLine($"set extAndResample {recInfo.EMinx}  {recInfo.EMaxx}  {recInfo.EMiny}  {recInfo.EMaxy}  {recInfo.Cols}  {recInfo.Rows} bilinear");
                        } else {
                                eng.Dispose();
                                Console.WriteLine(filename + " 文件不存在");
                        }
                }

                static void ToRunAsTplTiffCalc(REngine eng,string filename) {
                        DebugForR dfr = new DebugForR(eng);
                        dfr.Command = "library(rgdal)";
                        dfr.Command = "library(raster)";
                        if (File.Exists(filename)) {
                                GetLines(filename).ForEach(line => {
                                        dfr.Command = line;
                                });
                                eng.Dispose();
                        } else {
                                eng.Dispose();
                                Console.WriteLine(filename + " 文件不存在");
                        }
                }

                static List<string> GetLines(string filename) {
                        WriteObj writeObj = new WriteObj();
                        List<string> lines = new List<string>();
                        //lines.Add("setExtAndResample = function(r) { return (r) }");
                        string line;
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(filename)) {
                                while ((line = sr.ReadLine()) != null) {
                                        if (line.StartsWith("#")) {
                                        } else if (line.StartsWith("read ")) {
                                                // s + "=raster('" + tifName + "',band=" + bandIndex + ");"
                                                writeObj.ParseRead(line.Substring("read ".Length).Trim()).ForEach(L => {
                                                        lines.Add(L);
                                                });
                                        } else if (line.StartsWith("write ")) {
                                                lines.Add(writeObj.ParseWrite(line.Substring("write ".Length).Trim()));
                                        }/* else if (line.StartsWith("set extAndResample ")) {
                                                lines.Add(MakeSetExtAndResample(line.Substring("set extAndResample ".Length)));
                                        }*/ else {
                                                lines.Add(line);
                                        }
                                }
                        }
                        return lines;
                }
                static string MakeSetExtAndResample(string line) {
                        StringBuilder sb = new StringBuilder();
                        sb.Clear();
                        string[] part = line.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (part.Length == 7) {
                                if (part[6] == "bilinear" || part[6] == "ngb") { } else {
                                        Console.WriteLine("△△△△△△△△△△△△△△△△△△△△△△△△△△△△△△△△");
                                        Console.WriteLine("method 只能是 bilinear 或 ngb");
                                        Console.WriteLine("△△△△△△△△△△△△△△△△△△△△△△△△△△△△△△△△");
                                        part[6] = "bilinear";
                                }
                                sb.Append("setExtAndResample = function(r) { ");
                                sb.Append($"e = extent(c({part[0]},{part[1]},{part[2]},{part[3]})); ");
                                sb.Append("r = crop(r,e); ");
                                sb.Append($"tr = raster(ncol={part[4]},nrow={part[5]}); ");
                                sb.Append("tr@extent = r@extent; ");
                                sb.Append("tr@crs = r@crs; ");
                                sb.Append($"r = resample(r,tr,method='{part[6]}'); ");
                                sb.Append("return (r); ");
                                sb.Append("}");
                        } else {
                                Console.WriteLine("格式应该是 set extAndResample minx maxx miny maxy col row method");
                                throw new Exception("格式应该是 set extAndResample minx maxx miny maxy col row method");
                        }
                        return sb.ToString();
                }
                // Func<string,List<string>,bool,int> format => func<文件名,变量名集合（如果为空表示不取某一个波段）,文件是否存在,不得不输出>
                static void ParseReadWrite(string line, Func<string, List<string>,bool, int> format) {
                        int i = 0;
                        List<string> bands = new List<string>();
                        char[] splitChar = new char[] { '#' };
                        int bandIndex = 0;
                        for (; i < line.Length; i++) {
                                if (line[i] == ' ') {
                                        break;
                                }
                        }
                        if (i < line.Length) {
                                string tifName = line.Substring(i).Trim();
                                bool exist = File.Exists(tifName);
                                tifName = tifName.Replace("\\", "\\\\");
                                line.Substring(0, i).Trim()
                                        .Split(splitChar, StringSplitOptions.RemoveEmptyEntries)
                                        .ToList()
                                        .ForEach(s => {
                                                bandIndex++;
                                                if (s != "_") {
                                                        // lines.Add(s + "=raster('" + tifName + "',band=" + bandIndex + ");");
                                                        // lines.Add(format(tifName, s, bandIndex));
                                                        bands.Add(s);
                                                } else {
                                                        bands.Add("");
                                                }
                                        });
                                format(tifName, bands, exist);
                        }
                }

                class WriteObj {
                        public String FileName { get; set; } = "";
                        public List<String> Bands { get; set; } = new List<string>();
                        public Boolean Exit { get; set; } = false;
                        public String DType { get; set; } = "";
                        public String OverWrite { get; set; } = "";
                        public List<string> Reads { get; set; } = new List<string>();
                        // line example => v1#v2#3 path [type]
                        public String ParseWrite(String line) {
                                int i = 0;
                                Bands.Clear();
                                char[] splitChar = new char[] { '#' };
                                int bandIndex = 0;
                                // 找到分隔位置
                                for (; i < line.Length; i++) {
                                        if (line[i] == ' ') {
                                                break;
                                        }
                                }
                                if (i < line.Length) {
                                        FileName = line.Substring(i).Trim();
                                        CheckDType();
                                        OverWrite = File.Exists(FileName) ? ",overwrite=TRUE" : "";
                                        FileName = FileName.Replace("\\", "\\\\");
                                        line.Substring(0, i).Trim()
                                                .Split(splitChar, StringSplitOptions.RemoveEmptyEntries)
                                                .ToList()
                                                .ForEach(s => {
                                                        bandIndex++;
                                                        if (s != "_") {
                                                                // lines.Add(s + "=raster('" + tifName + "',band=" + bandIndex + ");");
                                                                // lines.Add(format(tifName, s, bandIndex));
                                                                Bands.Add(s);
                                                        } else {
                                                                Bands.Add("");
                                                        }
                                                });
                                        string bs = "";
                                        Bands.ForEach(b => bs += "," + b);
                                        return $"writeRaster(stack({bs.Substring(1)}),'{FileName}' {OverWrite} {DType})";
                                        // return "writeRaster(stack(" + bs.Substring(1) + "),'" + FileName + "');";
                                }
                                return "";
                        }
                        // line example => v1#v2#_#v4 path
                        public List<string> ParseRead(String line) {
                                int i = 0;
                                Bands.Clear();
                                Reads.Clear();
                                char[] splitChar = new char[] { '#' };
                                int bandIndex = 0;
                                // 找到分隔位置
                                for (; i < line.Length; i++) {
                                        if (line[i] == ' ') {
                                                break;
                                        }
                                }
                                if (i < line.Length) {
                                        FileName = line.Substring(i).Trim();
                                        CheckDType();
                                        if (!File.Exists(FileName)) {
                                                Console.WriteLine(FileName + " 文件不存在");
                                                throw new Exception("文件不存在");
                                        }
                                        FileName = FileName.Replace("\\", "\\\\");
                                        int bn = 1;
                                        line.Substring(0, i).Trim()
                                                .Split(splitChar, StringSplitOptions.RemoveEmptyEntries)
                                                .ToList()
                                                .ForEach(s => {
                                                        bandIndex++;
                                                        if (s != "_") {
                                                                // b + "=raster('" + tifName + "',band=" + bn + ");"
                                                                Reads.Add($"{s}=raster('{FileName}',band='{bn}')");
                                                        }
                                                        bn++;
                                                });
                                }
                                return Reads;
                        }
                        Dictionary<string, string> DTypeMapping = new Dictionary<string, string>() {
                                { "logic","LOG1S" },
                                { "uint8","INT1U" },
                                { "int8","INT1S" },
                                { "uint16","INT2U" },
                                { "int16","INT2S" },
                                { "uint32","INT4U" },
                                { "int32","INT4S" },
                                { "float","FLT4S" },
                                { "double","FLT8S" },
                        };
                        /*
                        Console.WriteLine("        logic  0 ~ 1");
                        Console.WriteLine("        int8   -127 ~ 127");
                        Console.WriteLine("        uint8  0 ~ 255");
                        Console.WriteLine("        int16  -32767 ~ 32767");
                        Console.WriteLine("        uint16 0 ~ 65535");
                        Console.WriteLine("        int32  -2,147,483,647 ~ 2,147,483,647");
                        Console.WriteLine("        uint32 0 ~ 4,294,967,296");
                        Console.WriteLine("        float  -3.4e+38 ~ 3.4e+38");
                        Console.WriteLine("        double -1.7e+308 ~ 1.7e+308");
                        */
                        void CheckDType() {
                                DType = "";
                                foreach (var kv in DTypeMapping) {
                                        if (kv.Key.Equals(FileName.Substring(FileName.Length - kv.Key.Length).ToLower())) {
                                                FileName = FileName.Substring(0, FileName.Length - kv.Key.Length).Trim();
                                                DType = ",datatype='" + kv.Value + "'";
                                                break;
                                        }
                                }
                        }
                }
        }
}
