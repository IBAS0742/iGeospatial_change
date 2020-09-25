using RDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PackageR.Op
{
        class InterAct
        {
                static public void DoInterAct(REngine eng, string commandName, string[] args)
                {
                        Console.WriteLine("已经进入交互模式，输入 exit 退出");
                        DebugForR dbr = new DebugForR(eng,false,true);
                        do
                        {
                                Console.Write(">>> ");
                                dbr.Command = Console.ReadLine();
                        } while (dbr.Command != "exit");
                        eng.Dispose();
                }
        }
}
