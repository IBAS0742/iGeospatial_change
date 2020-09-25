using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDAL = OSGeo.GDAL;

namespace GdalUtils.Utils
{
        class RasterBandOp
        {
                // 仅使用一个文件完成计算
                public class SingleDoubleFile
                {
                        public class Op {
                                enum calcType {
                                        BandBand,       // 条带与条带操作
                                        BandConst       // 条带和常数操作
                                }
                                calcType type;
                                int inBand1 = -1;
                                int inBand2 = -1;
                                int outBand = -1;
                                double constNum = -1;
                                int xsize = 0;
                                int ysize = 0;
                                GDAL.Dataset ds;
                                GDAL.Dataset DS {
                                        get {
                                                return ds;
                                        }
                                        set {
                                                xsize = value.RasterXSize;
                                                ysize = value.RasterYSize;
                                                ds = value;
                                        } 
                                }
                                public Op(GDAL.Dataset ds) {
                                        DS = ds;
                                }

                                Func<double[], double[], double[]> plusBBOP = (inp1, inp2) =>
                                {
                                        double[] output = new double[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = inp1[i] + inp2[i];
                                        }
                                        return output;
                                };
                                Func<double[], double[], double[]> cutBBOP = (inp1, inp2) =>
                                {
                                        double[] output = new double[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = inp1[i] - inp2[i];
                                        }
                                        return output;
                                };
                                Func<double[], double[], double[]> multiBBOP = (inp1, inp2) =>
                                {
                                        double[] output = new double[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = inp1[i] * inp2[i];
                                        }
                                        return output;
                                };
                                Func<double[], double[], double[]> deviceBBOP = (inp1, inp2) =>
                                {
                                        double[] output = new double[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = inp1[i] / inp2[i];
                                        }
                                        return output;
                                };
                                Func<double[], double, double[]> plusBCOP = (inp1, inp2) =>
                                {
                                        double[] output = new double[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp1[i] + inp2;
                                        }
                                        return output;
                                };
                                Func<double[], double, double[]> cutBCOP = (inp1, inp2) =>
                                {
                                        double[] output = new double[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp1[i] - inp2;
                                        }
                                        return output;
                                };
                                Func<double[], double, double[]> cutBCOPRev = (inp1, inp2) =>
                                {
                                        double[] output = new double[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp2 - inp1[i];
                                        }
                                        return output;
                                };
                                Func<double[], double, double[]> multiBCOP = (inp1, inp2) =>
                                {
                                        double[] output = new double[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp1[i] * inp2;
                                        }
                                        return output;
                                };
                                Func<double[], double, double[]> deviceBCOP = (inp1, inp2) =>
                                {
                                        double[] output = new double[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp1[i] / inp2;
                                        }
                                        return output;
                                };
                                Func<double[], double, double[]> deviceBCOPRev = (inp1, inp2) =>
                                {
                                        double[] output = new double[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp2 / inp1[i];
                                        }
                                        return output;
                                };

                                public void ToCalc(string exp)
                                {
                                        string[] _exp = new string[2];
                                        int c = 0;
                                        exp.Split('=').ToList().ForEach(e => {
                                                _exp[c] = e.Trim();
                                                c++;
                                        });
                                        setBand(_exp[0], 1);
                                        c = 0;
                                        _exp[1].Split(new char[6] { '+', '-', '*', '/', '\\', '#' }).ToList().ForEach(e =>
                                        {
                                                _exp[c] = e.Trim();
                                                c++;
                                        });
                                        setBand(_exp[0], 2);
                                        if (_exp[1][0] == 'b')
                                        {
                                                type = calcType.BandBand;
                                                setBand(_exp[1], 3);
                                        }
                                        else
                                        {
                                                type = calcType.BandConst;
                                                constNum = Double.Parse(_exp[1]);
                                        }
                                        // 获取计算方法
                                        if (exp.Contains("+"))
                                        {
                                                if (type == calcType.BandBand)
                                                {
                                                        bbop(plusBBOP);
                                                } else
                                                {
                                                        bcop(plusBCOP);
                                                }
                                        }
                                        else if (exp.Contains("-"))
                                        {
                                                if (type == calcType.BandBand)
                                                {
                                                        bbop(cutBBOP);
                                                }
                                                else
                                                {
                                                        bcop(cutBCOP);
                                                }
                                        }
                                        else if (exp.Contains("*"))
                                        {
                                                if (type == calcType.BandBand)
                                                {
                                                        bbop(multiBBOP);
                                                }
                                                else
                                                {
                                                        bcop(multiBCOP);
                                                }
                                        }
                                        else if (exp.Contains("/"))
                                        {
                                                if (type == calcType.BandBand)
                                                {
                                                        bbop(deviceBBOP);
                                                }
                                                else
                                                {
                                                        bcop(deviceBCOP);
                                                }
                                        } 
                                        else if (exp.Contains("\\"))
                                        {
                                                if (type == calcType.BandBand)
                                                {
                                                        int a = inBand1;
                                                        inBand1 = inBand2;
                                                        inBand2 = a;
                                                        bbop(deviceBBOP);
                                                } else
                                                {
                                                        bcop(deviceBCOPRev);
                                                }
                                        }
                                        else if (exp.Contains("#"))
                                        {
                                                if (type == calcType.BandBand)
                                                {
                                                        int a = inBand1;
                                                        inBand1 = inBand2;
                                                        inBand2 = a;
                                                        bbop(cutBBOP);
                                                }
                                                else
                                                {
                                                        bcop(cutBCOPRev);
                                                }
                                        }
                                }

                                private void bbop(Func<double[],double[],double[]> func)
                                {
                                        double[] buf1 = new double[xsize];
                                        double[] buf2 = new double[xsize];
                                        for (int i = 0; i < ysize; i++)
                                        {
                                                DS.GetRasterBand(inBand1).ReadRaster(0, i, xsize, 1, buf1, xsize, 1, 0, 0);
                                                DS.GetRasterBand(inBand2).ReadRaster(0, i, xsize, 1, buf2, xsize, 1, 0, 0);
                                                DS.GetRasterBand(outBand).WriteRaster(0, i, xsize, 1, func(buf1,buf2), xsize, 1, 0, 0);
                                        }
                                }
                                private void bcop(Func<double[],double,double[]> func)
                                {
                                        double[] buf1 = new double[xsize];
                                        for (int i = 0; i < ysize; i++)
                                        {
                                                DS.GetRasterBand(inBand1).ReadRaster(0, i, xsize, 1, buf1, xsize, 1, 0, 0);
                                                DS.GetRasterBand(outBand).WriteRaster(0, i, xsize, 1, func(buf1, constNum), xsize, 1, 0, 0);
                                        }
                                }
                                // n = 1 => outBand
                                // n = 2 => inBand1
                                // n = 3 => inBand2
                                private void setBand(string exp,int n)
                                {
                                        if (exp[0] != 'b')
                                        {
                                                throw new Exception("条带计算的公式必须形如 bn=br+bk 或 bn=br+m");
                                        }
                                        else
                                        {
                                                switch (n)
                                                {
                                                        case 1:
                                                                outBand = Int16.Parse(exp.Substring(1));
                                                                break;
                                                        case 2:
                                                                inBand1 = Int16.Parse(exp.Substring(1));
                                                                break;
                                                        case 3:
                                                                inBand2 = Int16.Parse(exp.Substring(1));
                                                                break;
                                                }
                                        }
                                }
                        }
                }
        }
}
