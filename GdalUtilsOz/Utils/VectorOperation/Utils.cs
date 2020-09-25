using System;
using OGR = OSGeo.OGR;
using GDAL = OSGeo.GDAL;

namespace GdalUtilsOz.Utils.VectorOperation
{
        class Utils
        {
                /**
                 * 参考代码 https://github.com/batuZ/GetDataTools/blob/1389f461d549eda8cff5716d6a290e0bcd348378/01_提取DEM/Rasterize.cs
                 * burnValue 的意思是，将栅格化后的点赋值为该值
                 */
                public static void Rasterize(
                        string shpPath,
                        string tifPath,
                        GDAL.DataType type = GDAL.DataType.GDT_Float64,
                        double burnValue = 1.0,
                        bool defaultGeoTransform = true,
                        double rasterCellSize = 0.00833333)
                {
                        const double noDataValue = -10000;
                        double[] defaultGeoT = Program.setting.Transform;
                        OGR.DataSource dataSource = OGR.Ogr.Open(shpPath, 0);
                        OGR.Envelope envelope = new OGR.Envelope();
                        OGR.Layer layer = dataSource.GetLayerByIndex(0);
                        layer.GetExtent(envelope, 0);
                        if (defaultGeoTransform)
                        {
                                rasterCellSize = defaultGeoT[1];
                                if (rasterCellSize < 0)
                                {
                                        rasterCellSize = -rasterCellSize;
                                }
                        }
                        Console.WriteLine("rasterCellSize = " + rasterCellSize);
                        int x_res = Convert.ToInt32((envelope.MaxX - envelope.MinX) / rasterCellSize);
                        int y_res = Convert.ToInt32((envelope.MaxY - envelope.MinY) / rasterCellSize);
                        string inputShapeSrs = "";
                        OSGeo.OSR.SpatialReference spatialRefrence = layer.GetSpatialRef();
                        if (spatialRefrence != null)
                        {
                                spatialRefrence.ExportToWkt(out inputShapeSrs);
                        }
                        double[] geoTransform = new double[6];
                        geoTransform[0] = envelope.MinX;
                        geoTransform[3] = envelope.MaxY;
                        if (defaultGeoTransform)
                        {
                                geoTransform[1] = defaultGeoT[1];
                                geoTransform[5] = defaultGeoT[5];
                                geoTransform[2] = defaultGeoT[2];
                                geoTransform[4] = defaultGeoT[4];
                        }
                        else
                        {
                                geoTransform[1] = rasterCellSize;
                                geoTransform[5] = -rasterCellSize;
                                geoTransform[2] = geoTransform[4] = 0;
                        }

                        GDAL.Dataset ds = RasterOperation.Create.CreateRaster.ToCreateRaster(
                                tifPath, 1, x_res, y_res,
                                type, inputShapeSrs, geoTransform);
                        ds.GetRasterBand(1).SetNoDataValue(noDataValue);
                        string[] options = new string[] { };

                        int ProgressFunc(double complete, IntPtr message, IntPtr data)
                        {
                                Console.Write("Processing ... " + complete * 100 + "% Completed.");
                                if (message != IntPtr.Zero)
                                {
                                        Console.Write(" Message:" + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message));
                                }
                                if (data != IntPtr.Zero)
                                {
                                        Console.Write(" Data:" + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(data));
                                }
                                Console.WriteLine("");
                                return 1;
                        }
                        GDAL.Gdal.RasterizeLayer(
                                ds, 1, new int[] { 1 },
                                layer, IntPtr.Zero, IntPtr.Zero, 1,
                                new double[] { burnValue }, options,
                                new GDAL.Gdal.GDALProgressFuncDelegate(ProgressFunc), "Raster conversion");
                        ds.Dispose();
                        dataSource.Dispose();
                }
        }
}
