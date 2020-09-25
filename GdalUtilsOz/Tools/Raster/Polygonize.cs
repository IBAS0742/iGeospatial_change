using System;
using System.Collections.Generic;
using OSGeo.OGR;
using OSGeo.GDAL;

namespace GdalUtilsOz.Tools.Raster
{
        class Polygonize
        {
                public static void help(string commandName)
                {
                        Console.WriteLine("程序名 " + commandName + " tifPath shpPath");
                }
                public static void ToPolygonize(string[] args,string commandName)
                {
                        Console.WriteLine("args[0] = " + args[0] + "\targs.length = " + args.Length);
                        if (args.Length == 3)
                        {
                                ToPolygonize(args[1], args[2]);
                        }
                        else
                        {
                                help(commandName);
                        }
                }
                public static void ToPolygonize(string rasterPath, string vectorPath)
                {
                        Utils.VectorOperation.Field field = new Utils.VectorOperation.Field("fid_1", FieldType.OFTInteger, 0);
                        DataSource vecotrDs = Utils.VectorOperation.Create.ToCreateShpFile(vectorPath, 
                                Utils.Prj.getPrj(Program.setting.RasterProject),
                                new Dictionary<string, wkbGeometryType>() {
                                        { "lay", wkbGeometryType.wkbCompoundCurve }
                                });
                        Utils.VectorOperation.FieldOperation.AddField(vecotrDs, field);
                        Layer lay = vecotrDs.GetLayerByIndex(0);
                        Dataset ds = Gdal.Open(rasterPath, Access.GA_ReadOnly);
                        Band band = ds.GetRasterBand(1);

                        Gdal.Polygonize(band, null, lay, -1, new string[] { }, null, null);
                        vecotrDs.Dispose();
                        ds.Dispose();
                }
        }
}
