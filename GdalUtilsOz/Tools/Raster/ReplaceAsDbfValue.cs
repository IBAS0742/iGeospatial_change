using GdalUtilsOz.Utils.Dbf;
using GDAL = OSGeo.GDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GdalUtilsOz.Utils.RasterOperation;

namespace GdalUtilsOz.Tools.Raster {
        class ReplaceAsDbfValue {
                static void help(string commandName) {
                        Console.WriteLine("程序名 " + commandName + " tif outtif dbf valueField replaceField");
                        Console.WriteLine("tif 和 outtif 和 dbf 是文件名");
                        Console.WriteLine("outtif 是生成的文件");
                        Console.WriteLine("valueField 是 dbf 上的一个字段的名字，用来对应dbf和tif的值");
                        Console.WriteLine("replaceField 是 dbf 上的一个字段的名字，指将tif的值改成该字段对应的值");
                }

                public static void ToReplaceAsDbfValue(string[] args,string commandName) { 
                        if (args.Length == 6) {
                                ToReplaceAsDbfValue(args[1], args[2], args[3], args[4], args[5]);
                        } else {
                                help(commandName);
                        }
                }

                static void ToReplaceAsDbfValue(string tif,string outTif,string dbf,string valueField,string replaceField) {
                        ReadDbf rdbf = new ReadDbf(dbf);
                        GDAL.Dataset ds = GDAL.Gdal.Open(tif, GDAL.Access.GA_ReadOnly);
                        GDAL.Dataset ods = Create.CreateRaster.ToCreateRaster(outTif, 1, ds, GDAL.DataType.GDT_Float64);
                        GDAL.Band band = ds.GetRasterBand(1);
                        if (band.DataType == GDAL.DataType.GDT_Byte) {
                                ToReplaceAsDbfValueByte(band, ods.GetRasterBand(1), rdbf, valueField, replaceField);
                        } else if (band.DataType == GDAL.DataType.GDT_CInt16 ||
                                band.DataType == GDAL.DataType.GDT_Int16 ||
                                band.DataType == GDAL.DataType.GDT_UInt16) {
                                ToReplaceAsDbfValueInt16(band, ods.GetRasterBand(1), rdbf, valueField, replaceField);
                        } else if (band.DataType == GDAL.DataType.GDT_CInt32 ||
                                band.DataType == GDAL.DataType.GDT_Int32 ||
                                band.DataType == GDAL.DataType.GDT_UInt32) {
                                ToReplaceAsDbfValueInt32(band, ods.GetRasterBand(1), rdbf, valueField, replaceField);
                        } else if (band.DataType == GDAL.DataType.GDT_CFloat32 ||
                                  band.DataType == GDAL.DataType.GDT_Float32) {
                                ToReplaceAsDbfValueFloat(band, ods.GetRasterBand(1), rdbf, valueField, replaceField);
                        } else if (band.DataType == GDAL.DataType.GDT_CFloat64 ||
                                 band.DataType == GDAL.DataType.GDT_Float64) {
                                ToReplaceAsDbfValueDouble(band, ods.GetRasterBand(1), rdbf, valueField, replaceField);
                        }
                }

                static void ToReplaceAsDbfValueByte(GDAL.Band srcBand, GDAL.Band distBand,ReadDbf rdf, string valueField, string replaceField) {
                        List<byte> value = rdf.ReadByte(valueField);
                        List<double> rep = rdf.ReadDouble(replaceField);
                        int xsize = srcBand.XSize, ysize = srcBand.YSize;
                        byte[] buf = new byte[xsize];
                        double[] obuf = new double[xsize];
                        for (int i = 0; i < ysize; i++) {
                                srcBand.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                for (int j = 0;j < xsize;j++) {
                                        obuf[j] = rep[value.IndexOf(buf[j])];
                                }
                                distBand.WriteRaster(0, i, xsize, 1, obuf, xsize, 1, 0, 0);
                        }
                }
                static void ToReplaceAsDbfValueInt16(GDAL.Band srcBand, GDAL.Band distBand,ReadDbf rdf, string valueField, string replaceField) {
                        List<Int16> value = rdf.ReadInt16(valueField);
                        List<double> rep = rdf.ReadDouble(replaceField);
                        int xsize = srcBand.XSize, ysize = srcBand.YSize;
                        Int16[] buf = new Int16[xsize];
                        double[] obuf = new double[xsize];
                        for (int i = 0; i < ysize; i++) {
                                srcBand.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                for (int j = 0; j < xsize; j++) {
                                        obuf[j] = rep[value.IndexOf(buf[j])];
                                }
                                distBand.WriteRaster(0, i, xsize, 1, obuf, xsize, 1, 0, 0);
                        }
                }
                static void ToReplaceAsDbfValueInt32(GDAL.Band srcBand, GDAL.Band distBand,ReadDbf rdf, string valueField, string replaceField) {
                        List<Int32> value = rdf.ReadInt32(valueField);
                        List<double> rep = rdf.ReadDouble(replaceField);
                        int xsize = srcBand.XSize, ysize = srcBand.YSize;
                        Int32[] buf = new Int32[xsize];
                        double[] obuf = new double[xsize];
                        for (int i = 0; i < ysize; i++) {
                                srcBand.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                for (int j = 0; j < xsize; j++) {
                                        obuf[j] = rep[value.IndexOf(buf[j])];
                                }
                                distBand.WriteRaster(0, i, xsize, 1, obuf, xsize, 1, 0, 0);
                        }
                }
                static void ToReplaceAsDbfValueFloat(GDAL.Band srcBand, GDAL.Band distBand, ReadDbf rdf, string valueField, string replaceField) {
                        List<double> value = rdf.ReadDouble(valueField);
                        List<double> rep = rdf.ReadDouble(replaceField);
                        int xsize = srcBand.XSize, ysize = srcBand.YSize;
                        double[] buf = new double[xsize];
                        double[] obuf = new double[xsize];
                        for (int i = 0; i < ysize; i++) {
                                srcBand.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                for (int j = 0; j < xsize; j++) {
                                        obuf[j] = rep[value.IndexOf(buf[j])];
                                }
                                distBand.WriteRaster(0, i, xsize, 1, obuf, xsize, 1, 0, 0);
                        }
                }
                static void ToReplaceAsDbfValueDouble(GDAL.Band srcBand, GDAL.Band distBand, ReadDbf rdf, string valueField, string replaceField) {
                        List<float> value = rdf.ReadFloat(valueField);
                        List<double> rep = rdf.ReadDouble(replaceField);
                        int xsize = srcBand.XSize, ysize = srcBand.YSize;
                        float[] buf = new float[xsize];
                        double[] obuf = new double[xsize];
                        for (int i = 0; i < ysize; i++) {
                                srcBand.ReadRaster(0, i, xsize, 1, buf, xsize, 1, 0, 0);
                                for (int j = 0; j < xsize; j++) {
                                        obuf[j] = rep[value.IndexOf(buf[j])];
                                }
                                distBand.WriteRaster(0, i, xsize, 1, obuf, xsize, 1, 0, 0);
                        }
                }
        }
}
