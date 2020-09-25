using System;
using GDAL = OSGeo.GDAL;

namespace GdalUtilsOz.Utils.RasterOperation
{
        class RasterBandCopy
        {
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
                                        if (IntFunc != null)
                                        {
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
                                                CopyRasterBandByteToDouble(srcBand, discBand, xsize, ysize);
                                                break;
                                        case GDAL.DataType.GDT_CFloat32:
                                        case GDAL.DataType.GDT_Float32:
                                                CopyRasterBandFloatToDouble(srcBand, discBand, xsize, ysize);
                                                break;
                                        case GDAL.DataType.GDT_CFloat64:
                                        case GDAL.DataType.GDT_Float64:
                                                CopyRasterBandDoubleToDouble(srcBand, discBand, xsize, ysize);
                                                break;
                                        case GDAL.DataType.GDT_CInt16:
                                        case GDAL.DataType.GDT_Int16:
                                        case GDAL.DataType.GDT_UInt16:
                                                CopyRasterBandShortToDouble(srcBand, discBand, xsize, ysize);
                                                break;
                                        case GDAL.DataType.GDT_CInt32:
                                        case GDAL.DataType.GDT_Int32:
                                        case GDAL.DataType.GDT_UInt32:
                                                CopyRasterBandIntToDouble(srcBand, discBand, xsize, ysize);
                                                break;
                                }
                        }
                }
        }
        public class CopyRasterBandToBand
        {
                public static void CopyHelp(
                        GDAL.Dataset srcDs,
                        GDAL.Dataset distDs,
                        int srcBandInd,
                        int distBandInd) {
                        GDAL.Band b = srcDs.GetRasterBand(srcBandInd);
                        switch (b.DataType) {
                                case GDAL.DataType.GDT_Byte:
                                        CopyByte(srcDs, distDs, srcBandInd, distBandInd);
                                        break;
                                case GDAL.DataType.GDT_CFloat32:
                                        CopyFloat(srcDs, distDs, srcBandInd, distBandInd);
                                        break;
                                case GDAL.DataType.GDT_Float32:
                                        CopyFloat(srcDs, distDs, srcBandInd, distBandInd);
                                        break;
                                case GDAL.DataType.GDT_CFloat64:
                                        CopyDouble(srcDs, distDs, srcBandInd, distBandInd);
                                        break;
                                case GDAL.DataType.GDT_Float64:
                                        CopyDouble(srcDs, distDs, srcBandInd, distBandInd);
                                        break;
                                case GDAL.DataType.GDT_CInt16:
                                        CopyInt16(srcDs, distDs, srcBandInd, distBandInd);
                                        break;
                                case GDAL.DataType.GDT_Int16:
                                        CopyInt16(srcDs, distDs, srcBandInd, distBandInd);
                                        break;
                                case GDAL.DataType.GDT_UInt16:
                                        CopyInt16(srcDs, distDs, srcBandInd, distBandInd);
                                        break;
                                case GDAL.DataType.GDT_CInt32:
                                        CopyInt32(srcDs, distDs, srcBandInd, distBandInd);
                                        break;
                                case GDAL.DataType.GDT_Int32:
                                        CopyInt32(srcDs, distDs, srcBandInd, distBandInd);
                                        break;
                                case GDAL.DataType.GDT_UInt32:
                                        CopyInt32(srcDs, distDs, srcBandInd, distBandInd);
                                        break;
                        }
                }
                /**
                 * srcDs 要复制的源
                 * distDs 要复制的目标
                 * srcBandInd 复制源的哪一层
                 * distBandInd 复制到目标的哪一层
                 */
                public static void CopyDouble(
                        GDAL.Dataset srcDs,
                        GDAL.Dataset distDs,
                        int srcBandInd,
                        int distBandInd)
                {
                        GDAL.Band src = srcDs.GetRasterBand(srcBandInd);
                        GDAL.Band dist = distDs.GetRasterBand(distBandInd);
                        int xsize = src.XSize, ysize = src.YSize;
                        double[] buf = new double[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                dist.WriteRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                        }
                }
                public static void CopyFloat(
                        GDAL.Dataset srcDs,
                        GDAL.Dataset distDs,
                        int srcBandInd,
                        int distBandInd)
                {
                        GDAL.Band src = srcDs.GetRasterBand(srcBandInd);
                        GDAL.Band dist = distDs.GetRasterBand(distBandInd);
                        int xsize = src.XSize, ysize = src.YSize;
                        float[] buf = new float[xsize];
                        Console.WriteLine("write float32");
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                dist.WriteRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                        }
                }
                public static void CopyByte(
                        GDAL.Dataset srcDs,
                        GDAL.Dataset distDs,
                        int srcBandInd,
                        int distBandInd) {
                        GDAL.Band src = srcDs.GetRasterBand(srcBandInd);
                        GDAL.Band dist = distDs.GetRasterBand(distBandInd);
                        int xsize = src.XSize, ysize = src.YSize;
                        byte[] buf = new byte[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                dist.WriteRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                        }
                }
                public static void CopyInt16(
                        GDAL.Dataset srcDs,
                        GDAL.Dataset distDs,
                        int srcBandInd,
                        int distBandInd) {
                        GDAL.Band src = srcDs.GetRasterBand(srcBandInd);
                        GDAL.Band dist = distDs.GetRasterBand(distBandInd);
                        int xsize = src.XSize, ysize = src.YSize;
                        Int16[] buf = new Int16[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                dist.WriteRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                        }
                }
                public static void CopyInt32(
                        GDAL.Dataset srcDs,
                        GDAL.Dataset distDs,
                        int srcBandInd,
                        int distBandInd) {
                        GDAL.Band src = srcDs.GetRasterBand(srcBandInd);
                        GDAL.Band dist = distDs.GetRasterBand(distBandInd);
                        int xsize = src.XSize, ysize = src.YSize;
                        Int32[] buf = new Int32[xsize];
                        for (int i = 0; i < ysize; i++)
                        {
                                src.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                dist.WriteRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                        }
                }
        }
}
