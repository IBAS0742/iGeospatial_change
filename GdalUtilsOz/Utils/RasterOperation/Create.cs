using System;
using GDAL = OSGeo.GDAL;

namespace GdalUtilsOz.Utils.RasterOperation
{
        class Create
        {
                static Random rd = new Random();
                public class CreateRaster
                {
                        private static string tmpRasterPath = null;
                        public static string driverName = "GTiff";
                        public static GDAL.Dataset CreateTmpRaster(int bands, int xsize, int yszie, GDAL.DataType type = GDAL.DataType.GDT_Float64)
                        {
                                tmpRasterPath = 
                                        System.Environment.GetEnvironmentVariable("TMP") + 
                                        '\\' + DateTimeOffset.Now.ToUnixTimeMilliseconds() + 
                                        Math.Abs(rd.Next(0,int.MaxValue)) +
                                        ".tif";
                                Console.WriteLine("创建一个临时栅格文件，位于\n" + tmpRasterPath);
                                return ToCreateRaster(tmpRasterPath, bands, xsize, yszie, type);
                        }
                        public static GDAL.Dataset ToCreateRaster(string path, int bands, GDAL.Dataset ds, GDAL.DataType type = GDAL.DataType.GDT_Unknown)
                        {
                                double[] transform = new double[6];
                                ds.GetGeoTransform(transform);
                                return ToCreateRaster(
                                        path,
                                        bands,
                                        ds.RasterXSize,
                                        ds.RasterYSize,
                                        type == GDAL.DataType.GDT_Unknown ? ds.GetRasterBand(1).DataType : type,
                                        ds.GetProjectionRef(),
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
                                double[] transform = null)
                        {
                                if (String.IsNullOrEmpty(projection))
                                {
                                        projection = Prj.getPrjString(Program.setting.RasterProject);
                                }
                                if (transform == null)
                                {
                                        transform = Program.setting.Transform;
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

        }
}
