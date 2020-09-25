using RDotNet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageR.Op
{
        class DebugForR
        {
                bool showCommand = true;
                bool useTask = false;
                REngine eng;
                string command;
                List<string> noOkStr = new List<string>() {
                        "?",
                        "plot("
                };
                public string Command {
                        get {
                                return command;
                        }
                        set {
                                if (string.IsNullOrEmpty(value))
                                {
                                        command = "";
                                } else {
                                        command = value.Trim();
                                }
                                if (!CheckCommandOk(command))
                                {
                                        command = "";
                                }
                                if (command != "")
                                {
                                        if (useTask)
                                        {
                                                Task t = Task.Run(() => {
                                                        try
                                                        {
                                                                eng.Evaluate(value);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                                Console.WriteLine(e.Message);
                                                        }
                                                });
                                        } else
                                        {
                                                try
                                                {
                                                        eng.Evaluate(value);
                                                }
                                                catch (Exception e)
                                                {
                                                        Console.WriteLine(e.Message);
                                                }
                                        }
                                }
                                if (showCommand) {
                                        Console.BackgroundColor = ConsoleColor.Blue;
                                        Console.ForegroundColor = ConsoleColor.White;

                                        Console.WriteLine("R run >> " + value);

                                        Console.BackgroundColor = ConsoleColor.Black;
                                        Console.ForegroundColor = ConsoleColor.White;
                                }
                        }
                }
                public DebugForR(REngine _eng,bool _showCommand = true,bool _useTask = false)
                {
                        eng = _eng;
                        showCommand = _showCommand;
                        useTask = _useTask;
                }
                private bool CheckCommandOk(string value) {
                        bool ok = true;
                        for (int i = 0;i < noOkStr.Count;i++)
                        {
                                if (noOkStr[i].Length > value.Length)
                                {
                                        continue;
                                }
                                if (string.Equals(noOkStr[i],value.Substring(0,noOkStr[i].Length))) {
                                        ok = false;
                                        Console.WriteLine("Sorry," + noOkStr[i] + " is not support now!");
                                        break;
                                }
                        }
                        return ok;
                }
        }
}
