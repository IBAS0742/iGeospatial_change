using RDotNet;
using System;
using System.Text;
using System.IO;

namespace PackageR.Op {
        class CheckNcFilesInFolder {
                static void Help(string actionName) {
                        Console.WriteLine("program.exe " + actionName + " folderName outFileName");
                }
                static public void ToChek(REngine eng, string commandName, string[] args) {
                        if (args.Length == 3) {
                                if (File.Exists(args[1])) {
                                        ToCheck(eng, args[1]);
                                } else if (Directory.Exists(args[1])) {
                                        ToCheck(eng, args[1], args[2]);
                                }
                        } else {
                                Help(commandName);
                                eng.Dispose();
                        }
                }

                static void ToCheck(REngine eng, string folder,string outpath) {
                        DebugForR dfr = new DebugForR(eng);
                        StringBuilder sb = new StringBuilder();
                        sb.Clear();
                        dfr.Command = "library('ncdf4')";
                        //eng.Evaluate("nc4 = ncdf4::nc_open('" + nc4 + "');");
                        DirectoryInfo TheFolder = new DirectoryInfo(folder);
                        foreach (FileInfo NextFile in TheFolder.GetFiles()) {
                                if (NextFile.Name.EndsWith(".nc4")) {
                                        string nc4 = NextFile.FullName.Replace("\\", "\\\\");
                                        try {
                                                Console.WriteLine($"checkFile {NextFile.Name}");
                                                eng.Evaluate("nc = ncdf4::nc_open('" + nc4 + "');");
                                        } catch(Exception e) {
                                                sb.Append(NextFile.Name);
                                                sb.Append("\r\n");
                                        } finally {
                                                eng.Evaluate("nc = 0");
                                        }
                                }
                        }
                        eng.Dispose();
                        File.WriteAllText(outpath, sb.ToString());
                }
                static void ToCheck(REngine eng, string file) {
                        DebugForR dfr = new DebugForR(eng,false);
                        StringBuilder sb = new StringBuilder();
                        sb.Clear();
                        dfr.Command = "library('ncdf4')";
                        //eng.Evaluate("nc4 = ncdf4::nc_open('" + nc4 + "');");
                        string nc4 = file.Replace("\\", "\\\\");
                        try {
                                //Console.WriteLine($"checkFile {file}");
                                eng.Evaluate("nc = ncdf4::nc_open('" + nc4 + "');");
                        } catch (Exception e) {
                                Console.WriteLine($"[chck_error] {file}");
                        } finally {
                                eng.Evaluate("nc = 0");
                        }
                        eng.Dispose();
                }
        }
}
