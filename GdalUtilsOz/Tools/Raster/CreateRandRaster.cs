using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDAL = OSGeo.GDAL;
using GdalUtilsOz.Utils.RasterOperation;

namespace GdalUtilsOz.Tools.Raster {
        class CreateRandRaster {
                public static void ToCreateRandRaster(string commandName) {
                        Console.WriteLine("程序名 " + commandName + " tifPath nd");
                        Console.WriteLine("程序名 " + commandName + " c:\\1.tif nnd <- 不设置nodata");
                }
                public static void ToCreateRandRaster(string[] args, string commandName) {
                        if (args.Length == 3) {
                                ToCreateRandRaster(args[1], args[2]);
                        } else {
                                ToCreateRandRaster(commandName);
                        }
                }
                public static void ToCreateRandRaster(string path,string nd) {
                        GDAL.Dataset ds = Create.CreateRaster.ToCreateRaster(path, 1, 5, 5, GDAL.DataType.GDT_CFloat64);
                        GDAL.Band b = ds.GetRasterBand(1);
                        if (!string.IsNullOrEmpty(nd) && !nd.Equals("nnd")) {
                                b.SetNoDataValue(double.Parse(nd));
                        }
                        b.Dispose();
                        ds.Dispose();
                }
        }
}
