using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageR.Op
{
        class AboutFile
        {
                /**
                 * 文件返回-1 文件夹返回 1 不存在返回 0
                 */
                public static int Exists(string path)
                {
                        if (File.Exists(path))
                        {
                                return -1;
                        } else if (Directory.Exists(path))
                        {
                                return 1;
                        } else
                        {
                                return 0;
                        }
                }

                public static void Delete(string path) {
                        int e = Exists(path);
                        if (e == -1)
                        {
                                File.Delete(path);
                        } else if (e == 1)
                        {
                                Directory.Delete(path);
                        } else
                        {
                                Console.WriteLine(path + " => not exist");
                        }
                }
        }
}
