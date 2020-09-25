using RDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageR.Op {
        class DoForOneNC4 {
                static void Help(string actionName) {
                        Console.WriteLine("program.exe " + actionName + " tplFile nc4File");
                        Console.WriteLine("tplFile 是模板文件，其中模板文件中用");
                        Console.WriteLine("$0$ $1$ $2$ 分别表示 nc4 的完整路径、路径、文件名");
                        Console.WriteLine("$3$ $4$ 分别表示 nc4 的不含后缀文件名、后缀名");
                }
                // args[1] = 模板文件
                // args[2] = NC4文件
                static public void ToDoForOneNC4(REngine eng, string commandName, string[] args) {
                        if (args.Length == 3) {
                                ToDoForOneNC4(eng, args[1], args[2]);
                        } else {
                                Help(commandName);
                                eng.Dispose();
                        }
                }

                static void ToDoForOneNC4(REngine eng, string nc4File, string tplFile) {
                        DebugForR debugForR = new DebugForR(eng);
                        string dirName = Path.GetDirectoryName(nc4File);
                        string filename = Path.GetFileName(nc4File);
                        string fullPath = Path.GetFullPath(nc4File);
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(nc4File);
                        string extension = Path.GetExtension(nc4File);
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(tplFile)) {
                                string line;
                                while ((line = sr.ReadLine()) != null) {
                                        line = line
                                                .Replace("$4$", extension)
                                                .Replace("$3$", fileNameWithoutExtension)
                                                .Replace("$0$", fullPath)
                                                .Replace("$1$", dirName)
                                                .Replace("$2$", filename);
                                        debugForR.Command = line;
                                }
                        }
                }
        }
}
