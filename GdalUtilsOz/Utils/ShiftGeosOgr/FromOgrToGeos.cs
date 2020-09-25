using System;
using System.Collections.Generic;
using iGeospatial.Coordinates;
using iGeospatial.Geometries;
using OSGeo.OGR;

namespace GdalUtilsOz.Utils.ShiftGeosOgr
{
        class FromOgrToGeos
        {
                static public GeometryList OgrFeatureToGeoAuto(DataSource dataSource)
                {
                        GeometryList g = new GeometryList();
                        if (dataSource.GetLayerCount() > 0)
                        {
                                wkbGeometryType type = dataSource.GetLayerByIndex(0).GetGeomType();
                                switch (type)
                                {
                                        case wkbGeometryType.wkbCurvePolygon:
                                        case wkbGeometryType.wkbPolygon:
                                        case wkbGeometryType.wkbMultiPolygon:
                                                return OgrFeatureToGeosPolygon(dataSource);
                                        case wkbGeometryType.wkbPoint:
                                        case wkbGeometryType.wkbMultiPoint:
                                                return OgrFeatureToGeosPoint(dataSource);
                                        case wkbGeometryType.wkbLineString:
                                        case wkbGeometryType.wkbMultiLineString:
                                                return OgrFeatureToGeosLineString(dataSource);
                                        case wkbGeometryType.wkbLinearRing:
                                                return OgrFeatureToGeosLinearRing(dataSource);
                                }
                        }
                        return g;
                }
                #region 点集转换
                static public GeometryList OgrFeatureToGeosPoint(DataSource dataSource) {
                        GeometryList geometryList = new GeometryList();
                        int lcount = dataSource.GetLayerCount();
                        for (int i = 0; i < lcount; i++)
                        {
                                long fcount = dataSource.GetLayerByIndex(i).GetFeatureCount(1);
                                for (long j = 0; j < fcount; j++)
                                {
                                        OgrFeatureToGeosPoint(dataSource.GetLayerByIndex(i).GetFeature(j)).ForEach(point => {
                                                geometryList.Add(point);
                                        });
                                }
                        }
                        return geometryList;
                }
                static public List<Point> OgrFeatureToGeosPoint(Feature feature)
                {
                        List<Point> point = new List<Point>();
                        int pcount = feature.GetGeometryRef().GetPointCount();
                        for (int i = 0; i < pcount; i++)
                        {
                                point.Add(
                                        new Point(
                                                new Coordinate(feature.GetGeometryRef().GetX(i), feature.GetGeometryRef().GetY(i)),
                                                Program.GeometryFactory
                                        ));
                        }
                        return point;
                }
                #endregion

                #region 面集转换
                static public GeometryList OgrFeatureToGeosPolygon(DataSource dataSource)
                {
                        GeometryList points = new GeometryList();
                        int lcount = dataSource.GetLayerCount();
                        for (int i = 0; i < lcount; i++)
                        {
                                long fcount = dataSource.GetLayerByIndex(i).GetFeatureCount(1);
                                Layer lay = dataSource.GetLayerByIndex(i);
                                Feature fe = lay.GetNextFeature(); 
                                while (fe != null) {
                                        points.Add(OgrFeatureToGeosPolygon(fe));
                                        fe = lay.GetNextFeature();
                                }
                        }
                        return points;
                }
                static public Polygon OgrFeatureToGeosPolygon(Feature feature)
                {
                        return Program.GeometryFactory.CreatePolygon(OgrFeatureToGeosLinearRing(feature), null);
                }
                #endregion

                #region 环集转换
                static public GeometryList OgrFeatureToGeosLinearRing(DataSource dataSource)
                {
                        GeometryList points = new GeometryList();
                        int lcount = dataSource.GetLayerCount();
                        LinearRing ring;
                        for (int i = 0; i < lcount; i++)
                        {
                                long fcount = dataSource.GetLayerByIndex(i).GetFeatureCount(1);
                                for (long j = 0; j < fcount; j++)
                                {
                                        ring = OgrFeatureToGeosLinearRing(dataSource.GetLayerByIndex(i).GetFeature(j));
                                        if (ring != null)
                                        {
                                                points.Add(ring);
                                        }
                                }
                        }
                        return points;
                }
                /**
                 * force = true 强行成环，不成环返回null
                 * force = false 无所谓结果，强行转，报错与否不管
                 */
                static public LinearRing OgrFeatureToGeosLinearRing(Feature feature,bool force = true)
                {
                        if (feature.GetGeomFieldCount() < 1)
                        {
                                throw new Exception("无法将该要素转换为线或环，该要素没有内容");
                        }
                        OSGeo.OGR.Geometry g = feature.GetGeometryRef().GetGeometryRef(0);
                        long pcount = g.GetPointCount();
                        CoordinateCollection coordinate = new CoordinateCollection();
                        if (force && pcount < 2)
                        {
                                throw new Exception("无法将该要素转换为线或环，该要素点集小于2个");
                        }
                        for (int j = 0; j < pcount; j++)
                        {
                                coordinate.Add(
                                        new Coordinate(
                                                g.GetX(j),
                                                g.GetY(j)
                                        )
                                );
                        }
                        if (force)
                        {
                                if (
                                        g.GetX(0) != g.GetX((int)pcount - 1) ||
                                        g.GetY(0) != g.GetY((int)pcount - 1)
                                )
                                {
                                        coordinate.Add(
                                                new Coordinate(
                                                        feature.GetGeometryRef().GetX(0),
                                                        feature.GetGeometryRef().GetY(0)
                                                ));
                                }
                        }
                        return new LinearRing(coordinate, Program.GeometryFactory);
                }
                #endregion

                #region 线集转换
                // todo 这里无法解决矢量读取的问题，部分矢量可能会有嵌套，所以这里确实不知道如何完成
                static public GeometryList OgrFeatureToGeosLineString(DataSource dataSource)
                {
                        GeometryList points = new GeometryList();
                        int lcount = dataSource.GetLayerCount();
                        for (int i = 0; i < lcount; i++)
                        {
                                long fcount = dataSource.GetLayerByIndex(i).GetFeatureCount(1);
                                for (long j = 0; j < fcount; j++)
                                {
                                        points.Add(OgrFeatureToGeosLineString(dataSource.GetLayerByIndex(i).GetFeature(j)));
                                }
                        }
                        return points;
                }
                static public LineString OgrFeatureToGeosLineString(Feature feature)
                {
                        #region 不知道为什么不行
                        //if (feature.GetGeomFieldCount() < 1)
                        //{
                        //        throw new Exception("无法将该要素转换为线或环，该要素没有内容");
                        //}
                        //OSGeo.OGR.Geometry g = feature.GetGeometryRef().GetGeometryRef(0);
                        //long pcount = g.GetPointCount();
                        //List<Coordinate> coordinates = new List<Coordinate>();
                        //for (int j = 0; j < pcount; j++)
                        //{
                        //        coordinates.Add(
                        //                new Coordinate(
                        //                        g.GetX(j),
                        //                        g.GetY(j)
                        //                )
                        //        );
                        //}
                        //return new LineString(coordinates.ToArray(), Program.GeometryFactory);
                        #endregion

                        long pcount = feature.GetGeometryRef().GetPointCount();
                        List<Coordinate> coordinates = new List<Coordinate>();
                        for (int j = 0; j < pcount; j++)
                        {
                                coordinates.Add(
                                        new Coordinate(
                                                feature.GetGeometryRef().GetX(j),
                                                feature.GetGeometryRef().GetY(j)
                                        )
                                );
                        }
                        return new LineString(coordinates.ToArray(), Program.GeometryFactory);
                }
                #endregion
        }
}
