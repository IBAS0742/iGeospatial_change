using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDAL = OSGeo.GDAL;

namespace GdalUtils.Utils
{
        public class CreateRaster
        {
                private static string tmpRasterPath = null;
                public static string driverName = "GTiff";
                public static GDAL.Dataset CreateTmpRaster(int bands,int xsize,int yszie,GDAL.DataType type = GDAL.DataType.GDT_Float64)
                {
                        tmpRasterPath = System.Environment.GetEnvironmentVariable("TMP") + '\\' + DateTimeOffset.Now.ToUnixTimeMilliseconds() + ".tif";
                        Console.WriteLine("创建一个临时栅格文件，位于\n" + tmpRasterPath);
                        return ToCreateRaster(tmpRasterPath, bands, xsize, yszie, type);
                }
                public static GDAL.Dataset ToCreateRaster(string path, int bands, GDAL.Dataset ds,GDAL.DataType type = GDAL.DataType.GDT_Unknown)
                {
                        double[] transform = new double[6];
                        ds.GetGeoTransform(transform);
                        return ToCreateRaster(
                                path,
                                bands,
                                ds.RasterXSize,
                                ds.RasterYSize,
                                type == GDAL.DataType.GDT_Unknown ? ds.GetRasterBand(1).DataType : type,
                                ds.GetProjection(),
                                transform
                        );
                }
                public static GDAL.Dataset ToCreateRaster(
                        string path,
                        int bands,
                        int xsize,
                        int ysize,
                        GDAL.DataType type,
                        string projection = null,
                        double[] transform = null) {
                        if (String.IsNullOrEmpty(projection)) {
                                projection = Prj.getPrjString(Program.SingtonSetting.RasterProject);
                        }
                        if (transform == null)
                        {
                                transform = SettingUtils.getTransform();
                        }
                        OSGeo.GDAL.Driver driver = GDAL.Gdal.GetDriverByName(driverName);
                        GDAL.Dataset nds = driver.Create(
                                path,
                                xsize,
                                ysize,
                                bands,
                                type,
                                null);
                        nds.SetGeoTransform(transform);
                        nds.SetProjection(projection);
                        return nds;
                }
        }

        public class CopyRasterBand
        {
                public static Func<short[], int, short[]> ShortFunc { get; set; } = null;
                public static Func<int[], int, int[]> IntFunc { get; set; } = null;
                public static Func<float[], int, float[]> FloatFunc { get; set; } = null;
                public static Func<double[], int, double[]> DoubleFunc { get; set; } = null;
                public static Func<byte[], int, byte[]> ByteFunc { get; set; } = null;
                public static void ToCopyRasterBand(
                        GDAL.Dataset srcDs,
                        GDAL.Dataset distDs,
                        int srcBandInd,
                        int distBandInd)
                {
                        GDAL.Band srcBand = srcDs.GetRasterBand(srcBandInd);
                        GDAL.Band discBand = distDs.GetRasterBand(distBandInd);
                        int xsize = srcBand.XSize, ysize = srcBand.YSize;
                        switch (srcBand.DataType)
                        {
                                case GDAL.DataType.GDT_Byte:
                                        CopyRasterBandByte(srcBand, discBand, xsize, ysize);
                                        break;
                                case GDAL.DataType.GDT_CFloat32:
                                case GDAL.DataType.GDT_Float32:
                                        CopyRasterBandFloat(srcBand, discBand, xsize, ysize);
                                        break;
                                case GDAL.DataType.GDT_CFloat64:
                                case GDAL.DataType.GDT_Float64:
                                        CopyRasterBandDouble(srcBand, discBand, xsize, ysize);
                                        break;
                                case GDAL.DataType.GDT_CInt16:
                                case GDAL.DataType.GDT_Int16:
                                case GDAL.DataType.GDT_UInt16:
                                        CopyRasterBandShort(srcBand, discBand, xsize, ysize);
                                        break;
                                case GDAL.DataType.GDT_CInt32:
                                case GDAL.DataType.GDT_Int32:
                                case GDAL.DataType.GDT_UInt32:
                                        CopyRasterBandInt(srcBand, discBand, xsize, ysize);
                                        break;
                        }
                }
                private static void CopyRasterBandInt(GDAL.Band src, GDAL.Band dist, int xsize, int ysize)
                {
                        int[] buf = new int[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                if (IntFunc != null) {
                                        buf = IntFunc(buf, xsize);
                                }
                                dist.WriteRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                        }
                }
                private static void CopyRasterBandShort(GDAL.Band src, GDAL.Band dist, int xsize, int ysize)
                {
                        short[] buf = new short[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                if (ShortFunc != null)
                                {
                                        buf = ShortFunc(buf, xsize);
                                }
                                dist.WriteRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                        }
                }
                private static void CopyRasterBandFloat(GDAL.Band src, GDAL.Band dist, int xsize, int ysize)
                {
                        float[] buf = new float[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                if (FloatFunc != null)
                                {
                                        buf = FloatFunc(buf, xsize);
                                }
                                dist.WriteRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                        }
                }
                private static void CopyRasterBandDouble(GDAL.Band src, GDAL.Band dist, int xsize, int ysize)
                {
                        double[] buf = new double[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                if (DoubleFunc != null)
                                {
                                        buf = DoubleFunc(buf, xsize);
                                }
                                dist.WriteRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                        }
                }
                private static void CopyRasterBandByte(GDAL.Band src, GDAL.Band dist, int xsize, int ysize)
                {
                        byte[] buf = new byte[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                if (ByteFunc != null)
                                {
                                        buf = ByteFunc(buf, xsize);
                                }
                                dist.WriteRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                        }
                }
        }

        public class CopyRasterBandFromDouble
        {
                public enum ResultType
                {
                        Double = GDAL.DataType.GDT_Float64,
                        Float = GDAL.DataType.GDT_Float32,
                        Int = GDAL.DataType.GDT_Int16,
                        Short = GDAL.DataType.GDT_Int32,
                        CDouble = GDAL.DataType.GDT_CFloat64,
                        CFloat = GDAL.DataType.GDT_CFloat32,
                        CInt = GDAL.DataType.GDT_CInt16,
                        CShort = GDAL.DataType.GDT_CInt32,
                        Byte = GDAL.DataType.GDT_Byte
                }
                static Func<double[], byte[]> fromDoubleToByte = (input) =>
                {
                        byte[] output = new byte[input.Length];
                        for (int i = 0; i < input.Length; i++)
                        {
                                output[i] = (byte)input[i];
                        }
                        return output;
                };
                static Func<double[], int[]> fromDoubleToInt = (input) =>
                {
                        int[] output = new int[input.Length];
                        for (int i = 0; i < input.Length; i++)
                        {
                                output[i] = (int)input[i];
                        }
                        return output;
                };
                static Func<double[], short[]> fromDoubleToShort = (input) =>
                {
                        short[] output = new short[input.Length];
                        for (int i = 0; i < input.Length; i++)
                        {
                                output[i] = (short)input[i];
                        }
                        return output;
                };
                static Func<double[], float[]> fromDoubleToFloat = (input) =>
                {
                        float[] output = new float[input.Length];
                        for (int i = 0; i < input.Length; i++)
                        {
                                output[i] = (float)input[i];
                        }
                        return output;
                };
                static Func<double[], double[]> fromDoubleToDouble = (input) =>
                {
                        return input;
                };
                public static void ToCopyRasterBandFromDouble(
                        GDAL.Dataset srcDs,
                        GDAL.Dataset distDs,
                        int srcBandInd,
                        int distBandInd,
                        ResultType type
                        )
                {
                        GDAL.Band srcBand = srcDs.GetRasterBand(srcBandInd);
                        GDAL.Band discBand = distDs.GetRasterBand(distBandInd);
                        int xsize = srcBand.XSize, ysize = srcBand.YSize;
                        double[] buf = new double[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                srcBand.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                switch (type)
                                {
                                        case ResultType.Byte:
                                                discBand.WriteRaster(0, i, xsize, 1, fromDoubleToByte(buf), xsize, 1, 0, 0);
                                                break;
                                        case ResultType.Short:
                                        case ResultType.CShort:
                                                discBand.WriteRaster(0, i, xsize, 1, fromDoubleToShort(buf), xsize, 1, 0, 0);
                                                break;
                                        case ResultType.Int:
                                        case ResultType.CInt:
                                                discBand.WriteRaster(0, i, xsize, 1, fromDoubleToInt(buf), xsize, 1, 0, 0);
                                                break;
                                        case ResultType.Float:
                                        case ResultType.CFloat:
                                                discBand.WriteRaster(0, i, xsize, 1, fromDoubleToFloat(buf), xsize, 1, 0, 0);
                                                break;
                                        case ResultType.Double:
                                        case ResultType.CDouble:
                                                discBand.WriteRaster(0, i, xsize, 1, fromDoubleToDouble(buf), xsize, 1, 0, 0);
                                                break;
                                }
                        }
                }
        }

        public class CopyRasterBandDouble
        {
                public static void CopyRasterBandByteToDouble(GDAL.Band src, GDAL.Band dist, int xsize, int ysize)
                {
                        byte[] buf = new byte[xsize];
                        double[] dbuf = new double[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                for (int j = 0; j < xsize; j++)
                                {
                                        dbuf[i] = buf[i];
                                }
                                dist.WriteRaster(0, i, xsize, 1, dbuf, xsize, 1, 0, 0);
                        }
                }
                public static void CopyRasterBandDoubleToDouble(GDAL.Band src, GDAL.Band dist, int xsize, int ysize)
                {
                        double[] buf = new double[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                dist.WriteRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                        }
                }
                public static void CopyRasterBandFloatToDouble(GDAL.Band src, GDAL.Band dist, int xsize, int ysize)
                {
                        float[] buf = new float[xsize];
                        double[] dbuf = new double[xsize];
                        double max = Double.MinValue, min = Double.MaxValue;
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                for (int j = 0; j < xsize; j++)
                                {
                                        dbuf[i] = buf[i];
                                }
                                dist.WriteRaster(0, i, xsize, 1, dbuf, xsize, 1, 0, 0);
                        }
                }
                public static void CopyRasterBandShortToDouble(GDAL.Band src, GDAL.Band dist, int xsize, int ysize)
                {
                        short[] buf = new short[xsize];
                        double[] dbuf = new double[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                for (int j = 0; j < xsize; j++)
                                {
                                        dbuf[i] = buf[i];
                                }
                                dist.WriteRaster(0, i, xsize, 1, dbuf, xsize, 1, 0, 0);
                        }
                }
                public static void CopyRasterBandIntToDouble(GDAL.Band src, GDAL.Band dist, int xsize, int ysize)
                {
                        int[] buf = new int[xsize];
                        double[] dbuf = new double[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                for (int j = 0; j < xsize; j++)
                                {
                                        dbuf[i] = buf[i];
                                }
                                dist.WriteRaster(0, i, xsize, 1, dbuf, xsize, 1, 0, 0);
                        }
                }
                public static void ToCopyRasterBandToDouble(
                        GDAL.Dataset srcDs,
                        GDAL.Dataset distDs,
                        int srcBandInd,
                        int distBandInd)
                {
                        GDAL.Band srcBand = srcDs.GetRasterBand(srcBandInd);
                        GDAL.Band discBand = distDs.GetRasterBand(distBandInd);
                        int xsize = srcBand.XSize, ysize = srcBand.YSize;
                        switch (srcBand.DataType)
                        {
                                case GDAL.DataType.GDT_Byte:
                                        Console.WriteLine("type = byte");
                                        CopyRasterBandByteToDouble(srcBand, discBand, xsize, ysize);
                                        break;
                                case GDAL.DataType.GDT_CFloat32:
                                case GDAL.DataType.GDT_Float32:
                                        Console.WriteLine("type = float32");
                                        CopyRasterBandFloatToDouble(srcBand, discBand, xsize, ysize);
                                        break;
                                case GDAL.DataType.GDT_CFloat64:
                                case GDAL.DataType.GDT_Float64:
                                        Console.WriteLine("type = float64");
                                        CopyRasterBandDoubleToDouble(srcBand, discBand, xsize, ysize);
                                        break;
                                case GDAL.DataType.GDT_CInt16:
                                case GDAL.DataType.GDT_Int16:
                                case GDAL.DataType.GDT_UInt16:
                                        Console.WriteLine("type = int16");
                                        CopyRasterBandShortToDouble(srcBand, discBand, xsize, ysize);
                                        break;
                                case GDAL.DataType.GDT_CInt32:
                                case GDAL.DataType.GDT_Int32:
                                case GDAL.DataType.GDT_UInt32:
                                        Console.WriteLine("type = int32");
                                        CopyRasterBandIntToDouble(srcBand, discBand, xsize, ysize);
                                        break;
                        }
                }
        }
}
 