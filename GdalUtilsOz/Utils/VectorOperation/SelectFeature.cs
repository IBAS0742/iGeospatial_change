using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using iGeospatial.Geometries;

namespace GdalUtilsOz.Utils.VectorOperation
{
        class SelectFeature
        {
                public class Exp
                {
                        class Condiction {
                                // type = "area" "length" "sq"
                                public string Type { get; set; } = "";
                                public delegate bool M(double v);
                                public M Method { get; set; } = null;
                                public double Value { get; set; } = 0;
                                public bool TestBig(double testValue)
                                {
                                        return testValue > Value;
                                }
                                public bool TestSmall(double testValue)
                                {
                                        return testValue < Value;
                                }
                                public bool TestEqual(double testValue)
                                {
                                        return testValue == Value;
                                }
                                public bool TestBigEq(double testValue)
                                {
                                        return testValue >= Value;
                                }
                                public bool TestSmallEq(double testValue)
                                {
                                        return testValue <= Value;
                                }
                                public bool TestNotEqual(double testValue) {
                                        return testValue != Value;
                                }
                        }
                        List<Condiction> condictionList = new List<Condiction>();
                        public Exp() { }
                        /**
                         * ABC
                         * A = length、area、sq
                         * B = [>、<、==、!=、>=、<=]
                         */
                        public void AddExp(string exp) {
                                Condiction cond = new Condiction();
                                exp = exp.Replace(" ", "").ToLower();
                                #region 判断类型
                                if (exp.Contains("length")) {
                                        cond.Type = "length";
                                } else if (exp.Contains("area"))
                                {
                                        cond.Type = "area";
                                } else if (exp.Contains("sq"))
                                {
                                        cond.Type = "sq";
                                } else
                                {
                                        throw new Exception("找不到对应的计算量");
                                }
                                #endregion
                                #region 判断运算符
                                if (exp.Contains("=="))
                                {
                                        cond.Method = cond.TestEqual;
                                } else if (exp.Contains(">="))
                                {
                                        cond.Method = cond.TestBigEq;
                                } else if (exp.Contains("<="))
                                {
                                        cond.Method = cond.TestSmallEq;
                                } else if (exp.Contains("!="))
                                {
                                        cond.Method = cond.TestNotEqual;
                                } else if (exp.Contains(">"))
                                {
                                        cond.Method = cond.TestBig;
                                } else if (exp.Contains("<"))
                                {
                                        cond.Method = cond.TestSmall;
                                } else
                                {
                                        throw new Exception("找不到对应的运算符");
                                }
                                #endregion
                                #region 获取比较值
                                for (int i = 0;i < exp.Length;i++)
                                {
                                        if (exp[i] >= '0' && exp[i] <= '9' )
                                        {
                                                cond.Value = double.Parse(exp.Substring(i));
                                                break;
                                        }
                                }
                                Console.WriteLine(string.Format("{0}\t=\t{1}", cond.Type, cond.Value));
                                #endregion
                                condictionList.Add(cond);
                        }
                        public static void ShowInfo(GeometryList list)
                        {
                                int count = list.Count;
                                Console.WriteLine("Index\tArea\tLength\tSq");
                                for (int i = 0; i < count; i++)
                                {
                                        Console.WriteLine(String.Format("{0}\t{1}\t{2}\t{3}",i,list[i].Area,list[i].Length,getSq(list[i])));
                                }
                        }
                        public GeometryList ToCalc(GeometryList list) {
                                GeometryList olist = new GeometryList();
                                int count = list.Count;
                                bool isOk = true;
                                for (int i = 0;i < count;i++)
                                {
                                        isOk = true;
                                        Geometry g = list[i];
                                        for (int j = 0;j < condictionList.Count;j++)
                                        {
                                                switch (condictionList[j].Type)
                                                {
                                                        case "area":
                                                                isOk &= condictionList[j].Method(g.Area);
                                                                break;
                                                        case "length":
                                                                isOk &= condictionList[j].Method(g.Length);
                                                                break;
                                                        case "sq":
                                                                isOk &= condictionList[j].Method(getSq(g));
                                                                break;
                                                }
                                                if (!isOk) {
                                                        break;
                                                }
                                        }
                                        if (isOk) {
                                                olist.Add(g);
                                        }
                                }
                                return olist;
                        }

                        static double getSq(Geometry g) {
                                double ret = g.Bounds.Width / g.Bounds.Height;
                                if (ret > 1) ret = 1 / ret;
                                return ret;
                        }
                }
        }
}
