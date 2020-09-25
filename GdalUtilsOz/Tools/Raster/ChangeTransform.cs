using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDAL = OSGeo.GDAL;
using GdalUtilsOz.Utils.RasterOperation;

namespace GdalUtilsOz.Tools.Raster {
        class ChangeTransform {
                public static void ToChangeTransform(string commandName) {
                        Console.WriteLine("程序名 " + commandName + " inpath outif t1 t2 t3 t4 t5 t6");
                }
                public static void ToChangeTransform(string[] args,string commandName) {
                        if (args.Length == 9) {
                                double[] transform = new double[6];
                                for (int i = 0;i < 6;i++) {
                                        transform[i] = double.Parse(args[3 + i]);
                                }
                                ToChangeTransform(args[1], args[2], transform);
                        } else {
                                ToChangeTransform(commandName);
                        }
                }

                public static void ToChangeTransform(string inTif,string outTif,double[] transform) {
                        GDAL.Dataset ds = GDAL.Gdal.Open(inTif, GDAL.Access.GA_ReadOnly);
                        GDAL.Band b = ds.GetRasterBand(1);
                        int xsize = b.XSize;
                        int ysize = b.YSize;
                        GDAL.DataType type = b.DataType;
                        GDAL.Dataset ods = Create.CreateRaster.ToCreateRaster(outTif, 1, xsize, ysize, type, null, transform);
                        CopyRasterBandToBand.CopyHelp(ds, ods, 1, 1);
                        ds.Dispose();
                        ods.Dispose();
                }
        }
}
