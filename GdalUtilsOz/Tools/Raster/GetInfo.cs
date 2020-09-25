using OSGeo.GDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDAL = OSGeo.GDAL;

namespace GdalUtilsOz.Tools.Raster {
        class GetInfo {
                public static void ToGetInfoHelp(string commandName) {
                        Console.WriteLine("程序名 " + commandName + " inpath");
                }
                public static void ToGetInfo(string[] args,string commandName) {
                        if (args.Length == 2) {
                                string tifPath = args[1];
                                GDAL.Dataset ds = GDAL.Gdal.Open(tifPath, GDAL.Access.GA_ReadOnly);
                                GDAL.Band b = ds.GetRasterBand(1);
                                double[] transform = new double[6];
                                ds.GetGeoTransform(transform);
                                for (int i = 0;i < transform.Length;i++) {
                                        Console.WriteLine(transform[i]);
                                }
                                double nd = 0;
                                int hnd = 0;
                                b.GetNoDataValue(out nd,out hnd);
                                Console.WriteLine($"nd={nd}\thnd={hnd}");
                                ds.Dispose();
                        } else {
                                ToGetInfoHelp(commandName);
                        }
                }
        }
}
