using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDAL = OSGeo.GDAL;

namespace GdalUtilsOz.Tools.Raster {
        class Static {
                delegate void writeOutFile();
                static writeOutFile wof = () => { };
                static char sep = '\t';
                static List<string> lines = new List<string>();
                class statistic {
                        #region 基础数据
                        // 所有的数据点
                        public int PixelCount { get; set; } = 0;
                        // 非空数据点
                        public int noNaNPixelCount { get; set; } = 0;
                        public double Sum { get; set; } = 0;
                        public double Max { get; set; } = 0;
                        public double Min { get; set; } = 0;
                        // 平均值
                        public double Mean { get; set; } = 0;
                        // 众数
                        public List<Double> Mode { get; set; } = new List<Double>();
                        // 数据类别（例如有1、2、3就是3）
                        public double Case { get; set; } = 0;
                        // 标准差
                        public double Std { get; set; } = 0;
                        List<double> datas = new List<double>();
                        #endregion
                        #region 栅格数据汇总统计
                        public delegate void CalcRasterBand(GDAL.Band b);
                        CalcRasterBand calcRasterBand;
                        void calcRasterBandByte(GDAL.Band b) {
                                int xsize = b.XSize;
                                int ysize = b.YSize;
                                getNoData(b);
                                byte[] buf = new byte[xsize];
                                double d = 0;
                                for (int i = 0; i < ysize; i++) {
                                        b.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                        for (int j = 0; j < xsize; j++) {
                                                d = (double)buf[j];
                                                if (!CheckNoData(d)) {
                                                        noNaNPixelCount++;
                                                        datas.Add(d);
                                                        Sum += d;
                                                        if (d > Max) Max = d;
                                                        if (d < Min) Min = d;
                                                }
                                        }
                                }
                                Summary();
                        }
                        void calcRasterBandDouble(GDAL.Band b) {
                                int xsize = b.XSize;
                                int ysize = b.YSize;
                                getNoData(b);
                                double[] buf = new double[xsize];
                                double d = 0;
                                for (int i = 0; i < ysize; i++) {
                                        b.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                        for (int j = 0; j < xsize; j++) {
                                                d = (double)buf[j];
                                                if (!CheckNoData(d)) {
                                                        noNaNPixelCount++;
                                                        datas.Add(d);
                                                        Sum += d;
                                                        if (d > Max) Max = d;
                                                        if (d < Min) Min = d;
                                                }
                                        }
                                }
                                Summary();
                        }
                        void calcRasterBandFloat(GDAL.Band b) {
                                int xsize = b.XSize;
                                int ysize = b.YSize;
                                getNoData(b);
                                float[] buf = new float[xsize];
                                double d = 0;
                                for (int i = 0; i < ysize; i++) {
                                        b.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                        for (int j = 0; j < xsize; j++) {
                                                d = (double)buf[j];
                                                if (!CheckNoData(d)) {
                                                        noNaNPixelCount++;
                                                        datas.Add(d);
                                                        Sum += d;
                                                        if (d > Max) Max = d;
                                                        if (d < Min) Min = d;
                                                }
                                        }
                                }
                                Summary();
                        }
                        void calcRasterBandInt(GDAL.Band b) {
                                int xsize = b.XSize;
                                int ysize = b.YSize;
                                getNoData(b);
                                Int16[] buf = new Int16[xsize];
                                double d = 0;
                                for (int i = 0; i < ysize; i++) {
                                        b.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                        for (int j = 0; j < xsize; j++) {
                                                d = (double)buf[j];
                                                if (!CheckNoData(d)) {
                                                        noNaNPixelCount++;
                                                        datas.Add(d);
                                                        Sum += d;
                                                        if (d > Max) Max = d;
                                                        if (d < Min) Min = d;
                                                }
                                        }
                                }
                                Summary();
                        }
                        void calcRasterBandLong(GDAL.Band b) {
                                int xsize = b.XSize;
                                int ysize = b.YSize;
                                getNoData(b);
                                Int32[] buf = new Int32[xsize];
                                double d = 0;
                                for (int i = 0; i < ysize; i++) {
                                        b.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                        for (int j = 0; j < xsize; j++) {
                                                d = (double)buf[j];
                                                if (!CheckNoData(d)) {
                                                        noNaNPixelCount++;
                                                        datas.Add(d);
                                                        Sum += d;
                                                        if (d > Max) Max = d;
                                                        if (d < Min) Min = d;
                                                }
                                        }
                                }
                                Summary();
                        }
                        double nd;
                        int hnd;
                        void getNoData(GDAL.Band b) {
                                b.GetNoDataValue(out nd, out hnd);
                        }
                        bool CheckNoData(double v) {
                                if (hnd == 1) {
                                        if (v == nd) {
                                                return true;
                                        }
                                        return double.IsNaN(v);
                                } else {
                                        return double.IsNaN(v);
                                }
                        }
                        #endregion
                        void Summary() {
                                // 已经完成对 noNaNPixelCount、Sum、Min、Max、datas 的汇总
                                // 这里需要计算 Mode、Mean、Case、Std
                                // 求平均数 Mean
                                Mean = Sum / noNaNPixelCount;
                                // 求标准差 Std
                                double std = 0;
                                Dictionary<double, int> types = new Dictionary<double, int>();
                                datas.ForEach(d => {
                                        if (types.ContainsKey(d)) {
                                                types[d]++;
                                        } else {
                                                types.Add(d, 1);
                                        }
                                        std += (d - Mean) * (d - Mean) / noNaNPixelCount;
                                });
                                Std = Math.Sqrt(std);
                                // 求众数和类别 Mode Case
                                int maxTypeCount = 0;
                                //List<double> maxTypeKeys = new List<double>();
                                foreach (var kv in types) {
                                        Case++;
                                        if (maxTypeCount < types[kv.Key]) {
                                                Mode.Clear();
                                                Mode.Add(kv.Key);
                                        } else if (maxTypeCount == types[kv.Key]) {
                                                Mode.Add(kv.Key);
                                        }
                                }
                        }
                        // 第三步，写出
                        public string getLine(string filename = "", int band = 0) {
                                if (string.IsNullOrEmpty(filename)) {
                                        string title = "filename,band,totalPixel,noNaNPixel,Sum,Min,Max,Mode,Mean,Case,Std";
                                        return title.Replace(',', sep);
                                } else {
                                        StringBuilder mode = new StringBuilder();
                                        Mode.ForEach(m => {
                                                mode.Append(m);
                                                mode.Append("#");
                                        });
                                        string modeStr = mode.ToString();
                                        return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                                                filename, band,
                                                PixelCount, noNaNPixelCount,
                                                Sum, Min, Max, modeStr.Substring(0, modeStr.Length - 1), Mean, Case, Std).Replace(',', sep);
                                }
                        }
                        // 第一步，初始化
                        public void init(GDAL.DataType dt) {
                                Max = double.MinValue;
                                Min = double.MaxValue;
                                Sum = 0;
                                noNaNPixelCount = 0;
                                datas.Clear();
                                switch (dt) {
                                        case GDAL.DataType.GDT_Byte:
                                                calcRasterBand = calcRasterBandByte;
                                                break;
                                        case GDAL.DataType.GDT_CFloat32:
                                                calcRasterBand = calcRasterBandFloat;
                                                break;
                                        case GDAL.DataType.GDT_Float32:
                                                calcRasterBand = calcRasterBandFloat;
                                                break;
                                        case GDAL.DataType.GDT_CFloat64:
                                                calcRasterBand = calcRasterBandDouble;
                                                break;
                                        case GDAL.DataType.GDT_Float64:
                                                calcRasterBand = calcRasterBandDouble;
                                                break;
                                        case GDAL.DataType.GDT_CInt16:
                                                calcRasterBand = calcRasterBandInt;
                                                break;
                                        case GDAL.DataType.GDT_Int16:
                                                calcRasterBand = calcRasterBandInt;
                                                break;
                                        case GDAL.DataType.GDT_UInt16:
                                                calcRasterBand = calcRasterBandInt;
                                                break;
                                        case GDAL.DataType.GDT_CInt32:
                                                calcRasterBand = calcRasterBandLong;
                                                break;
                                        case GDAL.DataType.GDT_Int32:
                                                calcRasterBand = calcRasterBandLong;
                                                break;
                                        case GDAL.DataType.GDT_UInt32:
                                                calcRasterBand = calcRasterBandLong;
                                                break;
                                }
                        }
                        // 第二步，计算
                        public void calc(GDAL.Band b) {
                                PixelCount = b.XSize * b.YSize;
                                calcRasterBand(b);
                        }
                }
                static statistic sta;
                public static void ToRasterStaticHelp(string commandName) {
                        Console.WriteLine("程序名 " + commandName + " inpath [output] [sep]");
                        Console.WriteLine("\toutput 是输出文件，可以将结果写道指定的文件中");
                        Console.WriteLine("\tsep 是分割，默认是 制表符[\\t]");
                }
                public static void ToRasterStatic(string[] args, string commandName) {
                        if (args.Length >= 2) {
                                if (args.Length == 3) {
                                        wof = () => {
                                                using (System.IO.StreamWriter file = new System.IO.StreamWriter(args[2])) {
                                                        lines.ForEach(line => file.WriteLine(line));
                                                }
                                        };
                                }
                                if (args.Length == 4) {
                                        sep = args[3][0];
                                }
                                sta = new statistic();
                                if (File.Exists(args[1])) {
                                        doFile(args[1]);
                                } else if (Directory.Exists(args[1])) {
                                        doDir(args[1]);
                                }
                                wof();
                                doDir(args[1]);
                        } else {
                                ToRasterStaticHelp(commandName);
                        }
                }
                static void doFile(string tifPath) {
                        Console.WriteLine(sta.getLine());
                        lines.Add(sta.getLine());
                        ToRasterStatic(Path.GetDirectoryName(tifPath), Path.GetFileName(tifPath));
                }
                static void doDir(string path) {
                        DirectoryInfo root = new DirectoryInfo(path);
                        List<String> files = new List<string>();
                        root.GetFiles().ToList().ForEach(f => {
                                files.Add(f.Name);
                        });
                        files.Sort();
                        Console.WriteLine(sta.getLine());
                        lines.Add(sta.getLine());
                        files.ForEach(f => {
                                if (f.Substring(f.Length - 4) == ".tif") {
                                        ToRasterStatic(path,f);
                                }
                        });
                        Console.WriteLine("结束");
                }
                public static void ToRasterStatic(string path,string tifName) {
                        // string tifPath = args[1];
                        GDAL.Dataset ds = GDAL.Gdal.Open(path + "\\" + tifName, GDAL.Access.GA_ReadOnly);
                        for (int i = 0;i < ds.RasterCount;i++) {
                                GDAL.Band b = ds.GetRasterBand(1 + i);
                                sta.init(b.DataType);
                                sta.calc(b);
                                Console.WriteLine(sta.getLine(tifName,1 + i));
                                lines.Add(sta.getLine(tifName, 1 + i));
                        }
                }
        }
}
