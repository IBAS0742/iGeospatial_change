using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDAL = OSGeo.GDAL;
using OGR = OSGeo.OGR;
using OSGeo.OSR;
using iGeospatial.Geometries;
using iGeospatial.Coordinates;
using System.Collections;
using iGeospatial.Collections.Sets;

namespace GdalUtils.Utils
{
        class UPoint
        {
                double x;
                double y;
                public UPoint(double _x, double _y)
                {
                        x = _x;
                        y = _y;
                }
                public Coordinate coordinate {
                        get {
                                return new Coordinate(this.x, this.y);
                        }
                }
        }
        class ULine
        {
                List<UPoint> uPoints = new List<UPoint>();
                public ULine addPoint(double x, double y)
                {
                        uPoints.Add(new UPoint(x, y));
                        return this;
                }
                public LineString line {
                        get {
                                List<Coordinate> coordinates = new List<Coordinate>();
                                uPoints.ForEach(p => coordinates.Add(p.coordinate));
                                return new LineString(coordinates.ToArray(), CreateShpFile.GeometryFactory);
                        }
                }
        }
        class CreateShpFile
        {
                static public GeometryFactory GeometryFactory = new GeometryFactory();
                static string driverName = Program.SingtonSetting.ShpDriver;
                public static OGR.DataSource ToCreateShpFile(string filename, Prj.Prjection prj, Dictionary<string, OGR.wkbGeometryType> layerNameAndType)
                {
                        OGR.Driver odriver = OGR.Ogr.GetDriverByName(driverName);
                        OGR.DataSource dataSource = odriver.CreateDataSource(filename, null);
                        SpatialReference sr = new SpatialReference(Prj.getPrjString(prj));
                        foreach (KeyValuePair<string, OGR.wkbGeometryType> kvp in layerNameAndType)
                        {
                                OGR.Layer lay = dataSource.CreateLayer(
                                        kvp.Key,
                                        sr,
                                        kvp.Value,
                                        null
                                );
                        }
                        return dataSource;
                }
                #region 这里的方法其实都是通过层层调用实现将数据存储在shp文件中，数据都是 geos 类型的
                public static void SavePointsToShpFile(List<UPoint> points, string filename)
                {
                        GeometryList geometryList = new GeometryList();
                        points.ForEach(p => {
                                geometryList.Add(new Point(p.coordinate, GeometryFactory));
                        });
                        SaveGeometryListToShpFile(geometryList, filename, OGR.wkbGeometryType.wkbMultiPoint);
                }
                public static void SaveHashedSetLineStringToShpFile(HashedSet set, string filename)
                {
                        GeometryList geometryList = new GeometryList();
                        for (IEnumerator i = set.GetEnumerator(); i.MoveNext();)
                        {
                                LineString line = (LineString)i.Current;
                                geometryList.Add(line);
                        }
                        SaveGeometryListToShpFile(geometryList, filename, OGR.wkbGeometryType.wkbLineString);
                }
                public static void SaveArrLineStringToShpFile(ArrayList array, string filename)
                {
                        GeometryList geometryList = new GeometryList();
                        for (int i = 0; i < array.Count; i++)
                        {
                                geometryList.Add((LineString)array[i]);
                        }
                        SaveGeometryListToShpFile(geometryList, filename, OGR.wkbGeometryType.wkbLineString);
                }
                public static void SaveLinesToShpFile(List<ULine> lines, string filename)
                {
                        GeometryList geometryList = new GeometryList();
                        lines.ForEach(l => {
                                geometryList.Add(l.line);
                        });
                        SaveGeometryListToShpFile(geometryList, filename, OGR.wkbGeometryType.wkbMultiPoint);
                }
                /***
                 * geometryList 数据集（可以是面、线、点）
                 * filename     文件名（保存shp文件的完整或相对路径）
                 * type         类型（OGR.wkbGeometryType）
                 * */
                public static void SaveGeometryListToShpFile(GeometryList geometryList, string filename, OGR.wkbGeometryType type)
                {
                        SaveGeometryListToDS(
                                geometryList,
                                ToCreateShpFile(filename,Prj.getPrj(Program.SingtonSetting.ShpProject),
                                new Dictionary<string, OGR.wkbGeometryType> {
                                        { Program.SingtonSetting.ShpLayName, type}
                                }),
                                0);
                }
                /**
                 * geometryList 是 geos 内的一种类型，需要转换为 ogr 类型才能存储到 ds 中
                 * ds 是 ogr 的一种类型
                 */
                public static void SaveGeometryListToDS(GeometryList geometryList, OGR.DataSource ds, int layerIndex)
                {
                        OGR.Layer layer = ds.GetLayerByIndex(layerIndex);
                        for (int i = 0; i < geometryList.Count; i++)
                        {
                                OGR.Feature feature = new OGR.Feature(layer.GetLayerDefn());
                                OSGeo.OGR.Geometry geometry = OSGeo.OGR.Geometry.CreateFromWkt(geometryList[i].ToString());
                                feature.SetGeometry(geometry);
                                layer.CreateFeature(feature);
                        }
                }
                #endregion

                #region 和上面的区别是，这里的数据都是 ogr 类型的
                public static void SaveOGRGeometryListToShpFile(List<OGR.Geometry> geoList, string filename, OGR.wkbGeometryType type)
                {
                        SaveOGRGeometryListToDS(geoList,
                                ToCreateShpFile(filename, Prj.Prjection.EPSG4326,
                                new Dictionary<string, OGR.wkbGeometryType> {
                                        {"lay0", type }
                                }), 0);
                }
                public static void SaveOGRGeometryListToDS(List<OGR.Geometry> geoList, OGR.DataSource ds, int layerIndex)
                {
                        OGR.Layer layer = ds.GetLayerByIndex(layerIndex);
                        geoList.ForEach(geo => {
                                OGR.Feature feature = new OGR.Feature(layer.GetLayerDefn());
                                feature.SetGeometry(geo);
                                layer.CreateFeature(feature);
                        });
                }
                #endregion
        }
        public class Field
        {
                string _fileName = "";
                OSGeo.OGR.FieldDefn pFieldDefn;
                int intValue;
                string strValue;
                double doubleValue;
                public delegate void SetFieldDelegate(OGR.Layer lay);  //返回值为空
                public SetFieldDelegate sfd;
                public Field(string fieldName, OGR.FieldType type, int value)
                {
                        intValue = value;
                        pFieldDefn = new OSGeo.OGR.FieldDefn(fieldName, type);
                        _fileName = fieldName;
                        sfd = SetIntField;
                }
                public Field(string fieldName, OGR.FieldType type, double value)
                {
                        doubleValue = value;
                        pFieldDefn = new OSGeo.OGR.FieldDefn(fieldName, type);
                        _fileName = fieldName;
                        sfd = SetDoubleField;
                }
                public Field(string fieldName, OGR.FieldType type, string value)
                {
                        strValue = value;
                        pFieldDefn = new OSGeo.OGR.FieldDefn(fieldName, type);
                        _fileName = fieldName;
                        sfd = SetStringField;
                }
                public Field(string fieldName, OGR.FieldType type, SetFieldDelegate _sfd = null)
                {
                        pFieldDefn = new OSGeo.OGR.FieldDefn(fieldName, type);
                        _fileName = fieldName;
                        if (_sfd == null)
                        {
                                sfd = SetFiledAsIndex;
                        }
                        else
                        {
                                sfd = _sfd;
                        }
                }


                void SetFiledAsIndex(OGR.Layer lay)
                {
                        //第二个参数如果为TRUE，则根据格式驱动程序的限制，可能以略有不同的形式创建该字段。
                        lay.CreateField(pFieldDefn, 1);
                        long fcount = lay.GetFeatureCount(1);
                        for (long i = 0; i < fcount; i++)
                        {
                                lay.GetFeature(i).SetField(_fileName, (int)i);
                        }
                }
                void SetDoubleField(OGR.Layer lay)
                {
                        //第二个参数如果为TRUE，则根据格式驱动程序的限制，可能以略有不同的形式创建该字段。
                        lay.CreateField(pFieldDefn, 1);
                        long fcount = lay.GetFeatureCount(1);
                        for (long i = 0; i < fcount; i++)
                        {
                                lay.GetFeature(i).SetField(_fileName, doubleValue);
                        }
                }
                void SetIntField(OGR.Layer lay)
                {
                        //第二个参数如果为TRUE，则根据格式驱动程序的限制，可能以略有不同的形式创建该字段。
                        lay.CreateField(pFieldDefn, 1);
                        long fcount = lay.GetFeatureCount(1);
                        for (long i = 0; i < fcount; i++)
                        {
                                lay.GetFeature(i).SetField(_fileName, intValue);
                        }
                }
                void SetStringField(OGR.Layer lay)
                {
                        if (!CheckFieldExist(lay))
                        {
                                //第二个参数如果为TRUE，则根据格式驱动程序的限制，可能以略有不同的形式创建该字段。
                                lay.CreateField(pFieldDefn, 1);
                        } else
                        {
                                Console.WriteLine(_fileName + " is exist");
                        }
                        long fcount = lay.GetFeatureCount(1);
                        for (long i = 0; i < fcount; i++)
                        {
                                OGR.Feature f = lay.GetFeature(i);
                                f.SetField(_fileName, strValue);
                                lay.SetFeature(f);
                        }
                }

                public bool CheckFieldExist(OGR.Layer lay) {
                        // 第二个参数为false好像有点像不存在即创建的感觉
                        return lay.FindFieldIndex(_fileName, 1) != -1;
                }
                public bool CheckFieldTypeRight(OGR.Layer lay) {
                        bool ret = CheckFieldExist(lay);
                        if (ret) {
                                return lay.GetFeature(0).GetFieldType(_fileName) == pFieldDefn.GetFieldType();
                        } else
                        {
                                return true;
                        }
                }
        }
        class ShpFileOP {
                public static void AddField(OGR.DataSource ds,Field field) {
                        int lcount = ds.GetLayerCount();
                        for (int i = 0;i < lcount;i++)
                        {
                                field.sfd(ds.GetLayerByIndex(i));
                        }
                }
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
                        double[] defaultGeoT = SettingUtils.getTransform(); 
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
                        if (spatialRefrence != null) {
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
                        } else {
                                geoTransform[1] = rasterCellSize;
                                geoTransform[5] = -rasterCellSize;
                                geoTransform[2] = geoTransform[4] = 0;
                        }

                        GDAL.Dataset ds = CreateRaster.ToCreateRaster(
                                tifPath, 1, x_res, y_res,
                                type,inputShapeSrs, geoTransform);
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
                                layer, IntPtr.Zero, IntPtr.Zero,1,
                                new double[] { burnValue }, options, 
                                new GDAL.Gdal.GDALProgressFuncDelegate(ProgressFunc), "Raster conversion");
                }
        }
}
