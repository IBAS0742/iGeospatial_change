using System;
using System.Linq;
using GDAL = OSGeo.GDAL;

namespace GdalUtilsOz.Utils.RasterOperation
{
        class RasterBandOperation
        {
                // 仅使用一个文件完成计算
                public class SingleDoubleFile
                {
                        public class Op
                        {
                                enum calcType
                                {
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
                                public Op(GDAL.Dataset ds)
                                {
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
                                                }
                                                else
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
                                                }
                                                else
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

                                private void bbop(Func<double[], double[], double[]> func)
                                {
                                        double[] buf1 = new double[xsize];
                                        double[] buf2 = new double[xsize];
                                        for (int i = 0; i < ysize; i++)
                                        {
                                                DS.GetRasterBand(inBand1).ReadRaster(0, i, xsize, 1, buf1, xsize, 1, 0, 0);
                                                DS.GetRasterBand(inBand2).ReadRaster(0, i, xsize, 1, buf2, xsize, 1, 0, 0);
                                                DS.GetRasterBand(outBand).WriteRaster(0, i, xsize, 1, func(buf1, buf2), xsize, 1, 0, 0);
                                        }
                                }
                                private void bcop(Func<double[], double, double[]> func)
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
                                private void setBand(string exp, int n)
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

                        public class OpDouble
                        {
                                enum calcType
                                {
                                        BandBand,       // 条带与条带操作
                                        BandConst       // 条带和常数操作
                                }
                                calcType type;
                                int inBand1 = -1;
                                int inBand2 = -1;
                                int outBand = -1;
                                Double constNum = -1;
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
                                double nodata;
                                public OpDouble(GDAL.Dataset ds,double _nodata)
                                {
                                        DS = ds;
                                        nodata = _nodata;
                                }

                                Func<Double[], Double[], Double[]> plusBBOP = (inp1, inp2) =>
                                {
                                        Double[] output = new Double[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = inp1[i] + inp2[i];
                                        }
                                        return output;
                                };
                                Func<Double[], Double[], Double[]> cutBBOP = (inp1, inp2) =>
                                {
                                        Double[] output = new Double[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = inp1[i] - inp2[i];
                                        }
                                        return output;
                                };
                                Func<Double[], Double[], Double[]> multiBBOP = (inp1, inp2) =>
                                {
                                        Double[] output = new Double[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = inp1[i] * inp2[i];
                                        }
                                        return output;
                                };
                                Func<Double[], Double[], Double[]> deviceBBOP = (inp1, inp2) =>
                                {
                                        Double[] output = new Double[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = inp1[i] / inp2[i];
                                        }
                                        return output;
                                };
                                Func<Double[], Double, Double[]> plusBCOP = (inp1, inp2) =>
                                {
                                        Double[] output = new Double[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp1[i] + inp2;
                                        }
                                        return output;
                                };
                                Func<Double[], Double, Double[]> cutBCOP = (inp1, inp2) =>
                                {
                                        Double[] output = new Double[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp1[i] - inp2;
                                        }
                                        return output;
                                };
                                Func<Double[], Double, Double[]> cutBCOPRev = (inp1, inp2) =>
                                {
                                        Double[] output = new Double[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp2 - inp1[i];
                                        }
                                        return output;
                                };
                                Func<Double[], Double, Double[]> multiBCOP = (inp1, inp2) =>
                                {
                                        Double[] output = new Double[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp1[i] * inp2;
                                        }
                                        return output;
                                };
                                Func<Double[], Double, Double[]> deviceBCOP = (inp1, inp2) =>
                                {
                                        Double[] output = new Double[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp1[i] / inp2;
                                        }
                                        return output;
                                };
                                Func<Double[], Double, Double[]> deviceBCOPRev = (inp1, inp2) =>
                                {
                                        Double[] output = new Double[inp1.Length];
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
                                                }
                                                else
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
                                                }
                                                else
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

                                private void bbop(Func<Double[], Double[], Double[]> func)
                                {
                                        Double[] buf1 = new Double[xsize];
                                        Double[] buf2 = new Double[xsize];
                                        for (int i = 0; i < ysize; i++)
                                        {
                                                DS.GetRasterBand(inBand1).ReadRaster(0, i, xsize, 1, buf1, xsize, 1, 0, 0);
                                                DS.GetRasterBand(inBand2).ReadRaster(0, i, xsize, 1, buf2, xsize, 1, 0, 0);
                                                DS.GetRasterBand(outBand).WriteRaster(0, i, xsize, 1, func(buf1, buf2), xsize, 1, 0, 0);
                                        }
                                }
                                private void bcop(Func<Double[], Double, Double[]> func)
                                {
                                        Double[] buf1 = new Double[xsize];
                                        for (int i = 0; i < ysize; i++)
                                        {
                                                DS.GetRasterBand(inBand1).ReadRaster(0, i, xsize, 1, buf1, xsize, 1, 0, 0);
                                                DS.GetRasterBand(outBand).WriteRaster(0, i, xsize, 1, func(buf1, constNum), xsize, 1, 0, 0);
                                        }
                                }
                                // n = 1 => outBand
                                // n = 2 => inBand1
                                // n = 3 => inBand2
                                private void setBand(string exp, int n)
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
                        public class OpFloat
                        {
                                enum calcType
                                {
                                        BandBand,       // 条带与条带操作
                                        BandConst       // 条带和常数操作
                                }
                                calcType type;
                                int inBand1 = -1;
                                int inBand2 = -1;
                                int outBand = -1;
                                float constNum = -1;
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
                                static float nodata;
                                public OpFloat(GDAL.Dataset ds,double _nodata)
                                {
                                        DS = ds;
                                        // 需要将nodata转换为对应的类型
                                        nodata = (float)_nodata;
                                }

                                Func<float[], float[], float[]> plusBBOP = (inp1, inp2) =>
                                {
                                        float[] output = new float[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                if (inp1[i] == nodata || inp2[i] == nodata) {
                                                        output[i] = nodata;
                                                }
                                                else
                                                {
                                                        output[i] = inp1[i] + inp2[i];
                                                }
                                        }
                                        return output;
                                };
                                Func<float[], float[], float[]> cutBBOP = (inp1, inp2) =>
                                {
                                        float[] output = new float[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                if (inp1[i] == nodata || inp2[i] == nodata)
                                                {
                                                        output[i] = nodata;
                                                }
                                                else
                                                {
                                                        output[i] = inp1[i] - inp2[i];
                                                }
                                        }
                                        return output;
                                };
                                Func<float[], float[], float[]> multiBBOP = (inp1, inp2) =>
                                {
                                        float[] output = new float[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                if (inp1[i] == nodata || inp2[i] == nodata)
                                                {
                                                        output[i] = nodata;
                                                }
                                                else
                                                {
                                                        output[i] = inp1[i] * inp2[i];
                                                }
                                        }
                                        return output;
                                };
                                Func<float[], float[], float[]> deviceBBOP = (inp1, inp2) =>
                                {
                                        float[] output = new float[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                if (inp1[i] == nodata || inp2[i] == nodata)
                                                {
                                                        output[i] = nodata;
                                                }
                                                else
                                                {
                                                        output[i] = inp1[i] / inp2[i];
                                                }
                                        }
                                        return output;
                                };
                                Func<float[], float, float[]> plusBCOP = (inp1, inp2) =>
                                {
                                        float[] output = new float[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                if (inp1[i] == nodata)
                                                {
                                                        output[i] = nodata;
                                                }
                                                else
                                                {
                                                        output[i] = inp1[i] + inp2;
                                                }
                                        }
                                        return output;
                                };
                                Func<float[], float, float[]> cutBCOP = (inp1, inp2) =>
                                {
                                        float[] output = new float[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                if (inp1[i] == nodata)
                                                {
                                                        output[i] = nodata;
                                                }
                                                else
                                                {
                                                        output[i] = inp1[i] - inp2;
                                                }
                                        }
                                        return output;
                                };
                                Func<float[], float, float[]> cutBCOPRev = (inp1, inp2) =>
                                {
                                        float[] output = new float[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                if (inp1[i] == nodata)
                                                {
                                                        output[i] = nodata;
                                                }
                                                else
                                                {
                                                        output[i] = inp2 - inp1[i];
                                                }
                                        }
                                        return output;
                                };
                                Func<float[], float, float[]> multiBCOP = (inp1, inp2) =>
                                {
                                        float[] output = new float[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                if (inp1[i] == nodata)
                                                {
                                                        output[i] = nodata;
                                                }
                                                else
                                                {
                                                        output[i] = inp1[i] * inp2;
                                                }
                                        }
                                        return output;
                                };
                                Func<float[], float, float[]> deviceBCOP = (inp1, inp2) =>
                                {
                                        float[] output = new float[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                if (inp1[i] == nodata)
                                                {
                                                        output[i] = nodata;
                                                }
                                                else
                                                {
                                                        output[i] = inp1[i] / inp2;
                                                }
                                        }
                                        return output;
                                };
                                Func<float[], float, float[]> deviceBCOPRev = (inp1, inp2) =>
                                {
                                        float[] output = new float[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                if (inp1[i] == nodata)
                                                {
                                                        output[i] = nodata;
                                                }
                                                else
                                                {
                                                        output[i] = inp2 / inp1[i];
                                                }
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
                                                constNum = float.Parse(_exp[1]);
                                        }
                                        // 获取计算方法
                                        if (exp.Contains("+"))
                                        {
                                                if (type == calcType.BandBand)
                                                {
                                                        bbop(plusBBOP);
                                                }
                                                else
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
                                                }
                                                else
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

                                private void bbop(Func<float[], float[], float[]> func)
                                {
                                        float[] buf1 = new float[xsize];
                                        float[] buf2 = new float[xsize];
                                        for (int i = 0; i < ysize; i++)
                                        {
                                                DS.GetRasterBand(inBand1).ReadRaster(0, i, xsize, 1, buf1, xsize, 1, 0, 0);
                                                DS.GetRasterBand(inBand2).ReadRaster(0, i, xsize, 1, buf2, xsize, 1, 0, 0);
                                                DS.GetRasterBand(outBand).WriteRaster(0, i, xsize, 1, func(buf1, buf2), xsize, 1, 0, 0);
                                        }
                                }
                                private void bcop(Func<float[], float, float[]> func)
                                {
                                        float[] buf1 = new float[xsize];
                                        for (int i = 0; i < ysize; i++)
                                        {
                                                DS.GetRasterBand(inBand1).ReadRaster(0, i, xsize, 1, buf1, xsize, 1, 0, 0);
                                                DS.GetRasterBand(outBand).WriteRaster(0, i, xsize, 1, func(buf1, constNum), xsize, 1, 0, 0);
                                        }
                                }
                                // n = 1 => outBand
                                // n = 2 => inBand1
                                // n = 3 => inBand2
                                private void setBand(string exp, int n)
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
                        public class OpInt16
                        {
                                enum calcType
                                {
                                        BandBand,       // 条带与条带操作
                                        BandConst       // 条带和常数操作
                                }
                                calcType type;
                                int inBand1 = -1;
                                int inBand2 = -1;
                                int outBand = -1;
                                Int16 constNum = -1;
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
                                Int16 nodata;
                                public OpInt16(GDAL.Dataset ds,double _nodata)
                                {
                                        DS = ds;
                                        nodata = (Int16)_nodata;
                                }

                                Func<Int16[], Int16[], Int16[]> plusBBOP = (inp1, inp2) =>
                                {
                                        Int16 a = 1, b = 2;
                                        Int16[] output = new Int16[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = (Int16)(inp1[i] + inp2[i]);
                                        }
                                        return output;
                                };
                                Func<Int16[], Int16[], Int16[]> cutBBOP = (inp1, inp2) =>
                                {
                                        Int16[] output = new Int16[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = (Int16)(inp1[i] - inp2[i]);
                                        }
                                        return output;
                                };
                                Func<Int16[], Int16[], Int16[]> multiBBOP = (inp1, inp2) =>
                                {
                                        Int16[] output = new Int16[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = (Int16)(inp1[i] * inp2[i]);
                                        }
                                        return output;
                                };
                                Func<Int16[], Int16[], Int16[]> deviceBBOP = (inp1, inp2) =>
                                {
                                        Int16[] output = new Int16[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = (Int16)(inp1[i] / inp2[i]);
                                        }
                                        return output;
                                };
                                Func<Int16[], Int16, Int16[]> plusBCOP = (inp1, inp2) =>
                                {
                                        Int16[] output = new Int16[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = (Int16)(inp1[i] + inp2);
                                        }
                                        return output;
                                };
                                Func<Int16[], Int16, Int16[]> cutBCOP = (inp1, inp2) =>
                                {
                                        Int16[] output = new Int16[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = (Int16)(inp1[i] - inp2);
                                        }
                                        return output;
                                };
                                Func<Int16[], Int16, Int16[]> cutBCOPRev = (inp1, inp2) =>
                                {
                                        Int16[] output = new Int16[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = (Int16)(inp2 - inp1[i]);
                                        }
                                        return output;
                                };
                                Func<Int16[], Int16, Int16[]> multiBCOP = (inp1, inp2) =>
                                {
                                        Int16[] output = new Int16[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = (Int16)(inp1[i] * inp2);
                                        }
                                        return output;
                                };
                                Func<Int16[], Int16, Int16[]> deviceBCOP = (inp1, inp2) =>
                                {
                                        Int16[] output = new Int16[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = (Int16)(inp1[i] / inp2);
                                        }
                                        return output;
                                };
                                Func<Int16[], Int16, Int16[]> deviceBCOPRev = (inp1, inp2) =>
                                {
                                        Int16[] output = new Int16[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = (Int16)(inp2 / inp1[i]);
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
                                                constNum = Int16.Parse(_exp[1]);
                                        }
                                        // 获取计算方法
                                        if (exp.Contains("+"))
                                        {
                                                if (type == calcType.BandBand)
                                                {
                                                        bbop(plusBBOP);
                                                }
                                                else
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
                                                }
                                                else
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

                                private void bbop(Func<Int16[], Int16[], Int16[]> func)
                                {
                                        Int16[] buf1 = new Int16[xsize];
                                        Int16[] buf2 = new Int16[xsize];
                                        for (int i = 0; i < ysize; i++)
                                        {
                                                DS.GetRasterBand(inBand1).ReadRaster(0, i, xsize, 1, buf1, xsize, 1, 0, 0);
                                                DS.GetRasterBand(inBand2).ReadRaster(0, i, xsize, 1, buf2, xsize, 1, 0, 0);
                                                DS.GetRasterBand(outBand).WriteRaster(0, i, xsize, 1, func(buf1, buf2), xsize, 1, 0, 0);
                                        }
                                }
                                private void bcop(Func<Int16[], Int16, Int16[]> func)
                                {
                                        Int16[] buf1 = new Int16[xsize];
                                        for (int i = 0; i < ysize; i++)
                                        {
                                                DS.GetRasterBand(inBand1).ReadRaster(0, i, xsize, 1, buf1, xsize, 1, 0, 0);
                                                DS.GetRasterBand(outBand).WriteRaster(0, i, xsize, 1, func(buf1, constNum), xsize, 1, 0, 0);
                                        }
                                }
                                // n = 1 => outBand
                                // n = 2 => inBand1
                                // n = 3 => inBand2
                                private void setBand(string exp, int n)
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

                        public class OpInt32
                        {
                                enum calcType
                                {
                                        BandBand,       // 条带与条带操作
                                        BandConst       // 条带和常数操作
                                }
                                calcType type;
                                int inBand1 = -1;
                                int inBand2 = -1;
                                int outBand = -1;
                                Int32 constNum = -1;
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
                                Int32 nodata;
                                public OpInt32(GDAL.Dataset ds,double _nodata)
                                {
                                        DS = ds;
                                        nodata = (Int32)_nodata;
                                }

                                Func<Int32[], Int32[], Int32[]> plusBBOP = (inp1, inp2) =>
                                {
                                        Int32[] output = new Int32[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = inp1[i] + inp2[i];
                                        }
                                        return output;
                                };
                                Func<Int32[], Int32[], Int32[]> cutBBOP = (inp1, inp2) =>
                                {
                                        Int32[] output = new Int32[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = inp1[i] - inp2[i];
                                        }
                                        return output;
                                };
                                Func<Int32[], Int32[], Int32[]> multiBBOP = (inp1, inp2) =>
                                {
                                        Int32[] output = new Int32[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = inp1[i] * inp2[i];
                                        }
                                        return output;
                                };
                                Func<Int32[], Int32[], Int32[]> deviceBBOP = (inp1, inp2) =>
                                {
                                        Int32[] output = new Int32[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = inp1[i] / inp2[i];
                                        }
                                        return output;
                                };
                                Func<Int32[], Int32, Int32[]> plusBCOP = (inp1, inp2) =>
                                {
                                        Int32[] output = new Int32[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp1[i] + inp2;
                                        }
                                        return output;
                                };
                                Func<Int32[], Int32, Int32[]> cutBCOP = (inp1, inp2) =>
                                {
                                        Int32[] output = new Int32[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp1[i] - inp2;
                                        }
                                        return output;
                                };
                                Func<Int32[], Int32, Int32[]> cutBCOPRev = (inp1, inp2) =>
                                {
                                        Int32[] output = new Int32[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp2 - inp1[i];
                                        }
                                        return output;
                                };
                                Func<Int32[], Int32, Int32[]> multiBCOP = (inp1, inp2) =>
                                {
                                        Int32[] output = new Int32[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp1[i] * inp2;
                                        }
                                        return output;
                                };
                                Func<Int32[], Int32, Int32[]> deviceBCOP = (inp1, inp2) =>
                                {
                                        Int32[] output = new Int32[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = inp1[i] / inp2;
                                        }
                                        return output;
                                };
                                Func<Int32[], Int32, Int32[]> deviceBCOPRev = (inp1, inp2) =>
                                {
                                        Int32[] output = new Int32[inp1.Length];
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
                                                constNum = Int32.Parse(_exp[1]);
                                        }
                                        // 获取计算方法
                                        if (exp.Contains("+"))
                                        {
                                                if (type == calcType.BandBand)
                                                {
                                                        bbop(plusBBOP);
                                                }
                                                else
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
                                                }
                                                else
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

                                private void bbop(Func<Int32[], Int32[], Int32[]> func)
                                {
                                        Int32[] buf1 = new Int32[xsize];
                                        Int32[] buf2 = new Int32[xsize];
                                        for (int i = 0; i < ysize; i++)
                                        {
                                                DS.GetRasterBand(inBand1).ReadRaster(0, i, xsize, 1, buf1, xsize, 1, 0, 0);
                                                DS.GetRasterBand(inBand2).ReadRaster(0, i, xsize, 1, buf2, xsize, 1, 0, 0);
                                                DS.GetRasterBand(outBand).WriteRaster(0, i, xsize, 1, func(buf1, buf2), xsize, 1, 0, 0);
                                        }
                                }
                                private void bcop(Func<Int32[], Int32, Int32[]> func)
                                {
                                        Int32[] buf1 = new Int32[xsize];
                                        for (int i = 0; i < ysize; i++)
                                        {
                                                DS.GetRasterBand(inBand1).ReadRaster(0, i, xsize, 1, buf1, xsize, 1, 0, 0);
                                                DS.GetRasterBand(outBand).WriteRaster(0, i, xsize, 1, func(buf1, constNum), xsize, 1, 0, 0);
                                        }
                                }
                                // n = 1 => outBand
                                // n = 2 => inBand1
                                // n = 3 => inBand2
                                private void setBand(string exp, int n)
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

                        public class OpByte
                        {
                                enum calcType
                                {
                                        BandBand,       // 条带与条带操作
                                        BandConst       // 条带和常数操作
                                }
                                calcType type;
                                int inBand1 = -1;
                                int inBand2 = -1;
                                int outBand = -1;
                                Byte constNum = 1;
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
                                byte nodata;
                                public OpByte(GDAL.Dataset ds,double _nodata)
                                {
                                        DS = ds;
                                        nodata = (byte)_nodata;
                                }

                                Func<Byte[], Byte[], Byte[]> plusBBOP = (inp1, inp2) =>
                                {
                                        Byte[] output = new Byte[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = (byte)(inp1[i] + inp2[i]);
                                        }
                                        return output;
                                };
                                Func<Byte[], Byte[], Byte[]> cutBBOP = (inp1, inp2) =>
                                {
                                        Byte[] output = new Byte[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = (byte)(inp1[i] - inp2[i]);
                                        }
                                        return output;
                                };
                                Func<Byte[], Byte[], Byte[]> multiBBOP = (inp1, inp2) =>
                                {
                                        Byte[] output = new Byte[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = (byte)(inp1[i] * inp2[i]);
                                        }
                                        return output;
                                };
                                Func<Byte[], Byte[], Byte[]> deviceBBOP = (inp1, inp2) =>
                                {
                                        Byte[] output = new Byte[inp1.Length];
                                        for (int i = 0; i < inp2.Length; i++)
                                        {
                                                output[i] = (byte)(inp1[i] / inp2[i]);
                                        }
                                        return output;
                                };
                                Func<Byte[], Byte, Byte[]> plusBCOP = (inp1, inp2) =>
                                {
                                        Byte[] output = new Byte[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = (byte)(inp1[i] + inp2);
                                        }
                                        return output;
                                };
                                Func<Byte[], Byte, Byte[]> cutBCOP = (inp1, inp2) =>
                                {
                                        Byte[] output = new Byte[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = (byte)(inp1[i] - inp2);
                                        }
                                        return output;
                                };
                                Func<Byte[], Byte, Byte[]> cutBCOPRev = (inp1, inp2) =>
                                {
                                        Byte[] output = new Byte[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = (byte)(inp2 - inp1[i]);
                                        }
                                        return output;
                                };
                                Func<Byte[], Byte, Byte[]> multiBCOP = (inp1, inp2) =>
                                {
                                        Byte[] output = new Byte[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = (byte)(inp1[i] * inp2);
                                        }
                                        return output;
                                };
                                Func<Byte[], Byte, Byte[]> deviceBCOP = (inp1, inp2) =>
                                {
                                        Byte[] output = new Byte[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = (byte)(inp1[i] / inp2);
                                        }
                                        return output;
                                };
                                Func<Byte[], Byte, Byte[]> deviceBCOPRev = (inp1, inp2) =>
                                {
                                        Byte[] output = new Byte[inp1.Length];
                                        for (int i = 0; i < inp1.Length; i++)
                                        {
                                                output[i] = (byte)(inp2 / inp1[i]);
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
                                                constNum = Byte.Parse(_exp[1]);
                                        }
                                        // 获取计算方法
                                        if (exp.Contains("+"))
                                        {
                                                if (type == calcType.BandBand)
                                                {
                                                        bbop(plusBBOP);
                                                }
                                                else
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
                                                }
                                                else
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

                                private void bbop(Func<Byte[], Byte[], Byte[]> func)
                                {
                                        Byte[] buf1 = new Byte[xsize];
                                        Byte[] buf2 = new Byte[xsize];
                                        for (int i = 0; i < ysize; i++)
                                        {
                                                DS.GetRasterBand(inBand1).ReadRaster(0, i, xsize, 1, buf1, xsize, 1, 0, 0);
                                                DS.GetRasterBand(inBand2).ReadRaster(0, i, xsize, 1, buf2, xsize, 1, 0, 0);
                                                DS.GetRasterBand(outBand).WriteRaster(0, i, xsize, 1, func(buf1, buf2), xsize, 1, 0, 0);
                                        }
                                }
                                private void bcop(Func<Byte[], Byte, Byte[]> func)
                                {
                                        Byte[] buf1 = new Byte[xsize];
                                        for (int i = 0; i < ysize; i++)
                                        {
                                                DS.GetRasterBand(inBand1).ReadRaster(0, i, xsize, 1, buf1, xsize, 1, 0, 0);
                                                DS.GetRasterBand(outBand).WriteRaster(0, i, xsize, 1, func(buf1, constNum), xsize, 1, 0, 0);
                                        }
                                }
                                // n = 1 => outBand
                                // n = 2 => inBand1
                                // n = 3 => inBand2
                                private void setBand(string exp, int n)
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
