using RDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageR.Op {
        class RunRAsTplDbf {
                static void help(string commandName) {
                        Console.WriteLine("program.exe " + commandName + " filename");
                        Console.WriteLine("filename 是 代码内容");
                        Console.WriteLine("目前定义了以下关键字，（仅仅支持dbf计算）");
                        Console.WriteLine("read write set");
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("文件名不能包含中文，否则无法读写");
                        Console.ResetColor();
                        Console.WriteLine("下面直接举例\r\n\r\n");
                        Console.WriteLine(@"# # 是注释
#读取 1.dbf 到变量 d 中
read d C:\Users\Administrator\Desktop\test\1.dbf
# 读取 2.dbf 到变量 d2 中
read d2 C:\Users\Administrator\Desktop\test\2.dbf

# 计算过程（直接改不会修改源文件）
# 注意 字段 是有大小写之分的
d$Value = d$Value + 1
# 如果是不同的数据集一起运算，需要长度一致
re = d$value + d2$value
# 如果长度不一致，可以通过取一部分来解决
# 例如都取 10 个
re1 = d$Value[1:10] + d2$Value[1:10]
re2 = d$Value[11:20] + d2$Value[11:20]

# 写出 将被修改的 d 写到 2.dbf
write d C:\Users\Administrator\Desktop\test\2.dbf

# 如果需要将中间计算的部分另外存到dbf需要做以下操作
# 例如上面的 re1 和 re2 就不是 dbf 结构的数据
# value=re1 <= re1 是数据 value 是列名
# 需要注意的是各个数据都必须是同样的长度
set outDbf value=re1  data=re2
# outDbf 只是变量名，任意写

write outDbf C:\Users\Administrator\Desktop\test\3.dbf");
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("文件名不能包含中文，否则无法读写");
                        Console.ResetColor();
                }

                static public void ToRunRAsTplDbf(REngine eng, string commandName, string[] args) {
                        if (args.Length == 2) {
                                ToRunRAsTplDbf(eng, args[1]);
                        } else {
                                eng.Dispose();
                                help(commandName);
                        }
                }

                static void ToRunRAsTplDbf(REngine eng,string filename) {
                        DebugForR dfr = new DebugForR(eng);
                        dfr.Command = "library(foreign)";
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
                class VandN { 
                        public string Val { get; set; }
                        public string Name { get; set; }
                        public bool CheckExist() {
                                return File.Exists(Name);
                        }
                }
                static List<string> GetLines(string filename) {
                        List<string> lines = new List<string>();
                        StringBuilder sb = new StringBuilder();
                        string line;
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(filename)) {
                                while ((line = sr.ReadLine()) != null) {
                                        if (line.StartsWith("#")) {
                                        } else if (line.StartsWith("read ")) {
                                                line = line.Substring("read ".Length).Trim();
                                                VandN vand = ReadLine(line);
                                                if (vand.CheckExist()) {
                                                        lines.Add(vand.Val + "=read.dbf('" + vand.Name.Replace("\\","\\\\") + "')");
                                                } else {
                                                        Console.WriteLine(vand.Name + " 文件不存在");
                                                }
                                        } else if (line.StartsWith("write ")) {
                                                line = line.Substring("write ".Length).Trim();
                                                VandN vand = ReadLine(line);
                                                lines.Add("write.dbf(" + vand.Val + ",'" + vand.Name.Replace("\\", "\\\\") + "')");
                                        } else if (line.StartsWith("set ")) {
                                                line = line.Substring("set ".Length).Trim();
                                                List<string> ls = 
                                                        line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                                sb.Clear();
                                                for (int i = 1;i < ls.Count;i++) {
                                                        sb.Append("," + ls[i]);
                                                }
                                                lines.Add(ls[0] + "=data.frame(" + sb.ToString().Substring(1) + ")");
                                        } else {
                                                lines.Add(line);
                                        }
                                }
                        }
                        return lines;
                }
                static VandN ReadLine(string line) {
                        int i = 0;
                        for (;i < line.Length;i++) {
                                if (line[i] == ' ') {
                                        break;
                                }
                        }
                        if (i < line.Length) {
                                VandN vand = new VandN();
                                vand.Val = line.Substring(0, i).Trim();
                                vand.Name = line.Substring(i).Trim();
                                return vand;
                        } else {
                                Console.WriteLine(line + " 语法错误");
                                throw new Exception("error");
                        }
                }
        }
}
