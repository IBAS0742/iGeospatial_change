using SocialExplorer.IO.FastDBF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GdalUtilsOz.Utils.Dbf {
        // https://www.cnblogs.com/yzhyingcool/p/10657350.html
        class ReadDbf {
                DbfFile dbfFile;
                public ReadDbf(string filename) {
                        dbfFile = new DbfFile(Encoding.GetEncoding(65001));
                        dbfFile.Open(filename, System.IO.FileMode.Open);
                }
                public static long Convert(string a) {
                        long result = -1;
                        if (a.ToUpper().Contains("E")) {
                                string z = a.ToUpper().Split('E')[0].ToString();//整数部分
                                long c = long.Parse(a.ToUpper().Split('E')[1].ToString());//指数部分
                                int dotInd = z.IndexOf('.');
                                StringBuilder sb = new StringBuilder();
                                if (dotInd == -1) {
                                        return long.Parse(z) * 10 ^ c;
                                } else {
                                        // z = 2.34   c = 5
                                        if (z.Substring(dotInd + 1).Length < c) {
                                                int deta = (int)c - z.Substring(dotInd + 1).Length;
                                                for (int i = 0;i < deta;i++) {
                                                        z += "0";
                                                }
                                        }
                                        if (c < 0) {
                                                c = -c;
                                                z = z.Substring(0, dotInd);
                                                if (z.Length < c) {
                                                        return 0;
                                                } else {
                                                        long.Parse(z.Substring(0, z.Length - (int)c));
                                                }
                                        } else {
                                                int i;
                                                for (i = 0;i < dotInd;i++) {
                                                        sb.Append(z[i]);
                                                }
                                                for (i = 0;i < c;i++) {
                                                        sb.Append(z[i + dotInd + 1]);
                                                }
                                                return long.Parse(sb.ToString());
                                        }
                                }
                        } else {
                                result = long.Parse(a);
                        }
                        return result;
                }
                public List<double> ReadDouble(string fildName) {
                        List<double> list = new List<double>();
                        DbfRecord recoder = dbfFile.Read(0);
                        while (recoder != null) {
                                list.Add(double.Parse(recoder[fildName]));
                                recoder = dbfFile.ReadNext();
                        }
                        return list;
                }
                public List<float> ReadFloat(string fildName) {
                        List<float> list = new List<float>();
                        DbfRecord recoder = dbfFile.Read(0);
                        while (recoder != null) {
                                list.Add(float.Parse(recoder[fildName]));
                                recoder = dbfFile.ReadNext();
                        }
                        return list;
                }
                public List<Int32> ReadInt32(string fildName) {
                        List<Int32> list = new List<Int32>();
                        DbfRecord recoder = dbfFile.Read(0);
                        while (recoder != null) {
                                list.Add((Int32)Convert(recoder[fildName]));
                                recoder = dbfFile.ReadNext();
                        }
                        return list;
                }
                public List<Int16> ReadInt16(string fildName) {
                        List<Int16> list = new List<Int16>();
                        DbfRecord recoder = dbfFile.Read(0);
                        int c = 0;
                        while (recoder != null) {
                                if (c == 959) {
                                        Console.WriteLine("a");
                                }
                                c++;
                                list.Add((Int16)Convert(recoder[fildName]));
                                recoder = dbfFile.ReadNext();
                        }
                        return list;
                }
                public List<byte> ReadByte(string fildName) {
                        List<byte> list = new List<byte>();
                        DbfRecord recoder = dbfFile.Read(0);
                        while (recoder != null) {
                                list.Add(byte.Parse(recoder[fildName]));
                                recoder = dbfFile.ReadNext();
                        }
                        return list;
                }
                public List<string> ReadString(string fildName) {
                        List<string> list = new List<string>();
                        DbfRecord recoder = dbfFile.Read(0);
                        while (recoder != null) {
                                list.Add(recoder[fildName]);
                                recoder = dbfFile.ReadNext();
                        }
                        return list;
                }

                public void Dispose() {
                        dbfFile.Close();
                }
        }
}
