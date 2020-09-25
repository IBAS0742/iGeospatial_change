using iGeospatial.Coordinates;
using iGeospatial.Geometries;
using System.Collections.Generic;
using OGR = OSGeo.OGR;
using OSGeo.OSR;
using System.Collections;
using iGeospatial.Collections.Sets;

namespace GdalUtilsOz.Utils.VectorOperation
{
        class UPoint
        {
                double x;
                double y;
                public UPoint() { }
                public UPoint(double _x, double _y)
                {
                        x = _x;
                        y = _y;
                }
                public void SetPoint(double _x, double _y)
                {
                        x = _x;
                        y = _y;
                }
                public Coordinate coordinate {
                        get {
                                return new Coordinate(this.x, this.y);
                        }
                }
                public Point Point {
                        get {
                                return new Point(coordinate, Program.GeometryFactory);
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
                public LineString Line {
                        get {
                                List<Coordinate> coordinates = new List<Coordinate>();
                                uPoints.ForEach(p => coordinates.Add(p.coordinate));
                                return new LineString(coordinates.ToArray(), Program.GeometryFactory);
                        }
                }
                public void Clear()
                {
                        uPoints.Clear();
                }
        }
        class URing
        {

                List<UPoint> uPoints = new List<UPoint>();
                public URing addPoint(double x, double y)
                {
                        uPoints.Add(new UPoint(x, y));
                        return this;
                }
                public LinearRing Ring{
                        get {
                                if (uPoints.Count > 2) { 
                                        if (!uPoints[0].coordinate.Equals(uPoints[uPoints.Count - 1].coordinate)) {
                                                uPoints.Add(uPoints[0]);
                                        }
                                        CoordinateCollection collection = new CoordinateCollection();
                                        uPoints.ForEach(point => collection.Add(point.coordinate));
                                        return new LinearRing(collection, Program.GeometryFactory);
                                } else
                                {
                                        return null;
                                }
                        }
                }
                public void Clear()
                {
                        uPoints.Clear();
                }
        }
        class UPolygon
        {
                URing ring;
                List<URing> hole = new List<URing>();
                public UPolygon addHole(URing _ring)
                {
                        hole.Add(_ring);
                        return this;
                }
                public UPolygon SetRing(URing _ring)
                {
                        ring = _ring;
                        return this;
                }
                public Polygon Polygon {
                        get {
                                List<LinearRing> holes = new List<LinearRing>();
                                hole.ForEach(hole => holes.Add(hole.Ring));
                                return new Polygon(ring.Ring, holes.ToArray(), Program.GeometryFactory);
                        }
                }

        }
        class Create
        {
                static string driverName = Program.setting.ShpDriver;
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
                                geometryList.Add(new Point(p.coordinate, Program.GeometryFactory));
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
                                geometryList.Add(l.Line);
                        });
                        SaveGeometryListToShpFile(geometryList, filename, OGR.wkbGeometryType.wkbMultiPoint);
                }
                public static void SavePolygonToShpFile(List<UPolygon> polygon,string filename)
                {
                        GeometryList geometryList = new GeometryList();
                        polygon.ForEach(l => {
                                geometryList.Add(l.Polygon);
                        });
                        SaveGeometryListToShpFile(geometryList, filename, OGR.wkbGeometryType.wkbPolygon);
                }
                /***
                 * geometryList 数据集（可以是面、线、点）
                 * filename     文件名（保存shp文件的完整或相对路径）
                 * type         类型（OGR.wkbGeometryType）
                 * */
                public static void SaveGeometryListToShpFile(GeometryList geometryList, string filename, OGR.wkbGeometryType type)
                {
                        ShiftGeosOgr.FromGeosToOgr.SaveGeoGeometryListToOgrDS(
                                geometryList,
                                ToCreateShpFile(filename, Prj.getPrj(Program.setting.ShpProject),
                                new Dictionary<string, OGR.wkbGeometryType> {
                                        { Program.setting.ShpLayName, type}
                                }),
                                0);
                }
                #endregion

                #region 和上面的区别是，这里的数据都是 ogr 类型的，不需要转换了
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
}
