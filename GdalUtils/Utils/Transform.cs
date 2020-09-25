using iGeospatial.Coordinates;
using iGeospatial.Geometries;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GdalUtils.Utils
{
        class Transform
        {
                static public List<Point> OgrFeatureToGeosPoint(Feature feature)
                {
                        List<Point> point = new List<Point>();
                        int pcount = feature.GetGeometryRef().GetPointCount();
                        for (int i = 0;i < pcount;i++)
                        {
                                point.Add(
                                        new Point(
                                                new Coordinate(feature.GetGeometryRef().GetX(i), feature.GetGeometryRef().GetY(i)), 
                                                CreateShpFile.GeometryFactory
                                        ));
                        }
                        return point;
                }
                static public Polygon OgrFeatureToGeosPolygon(Feature feature) {
                        return CreateShpFile.GeometryFactory.CreatePolygon(OgrFeatureToGeosLinearRing(feature),null);
                }
                static public LinearRing OgrFeatureToGeosLinearRing(Feature feature)
                {
                        long pcount = feature.GetGeometryRef().GetPointCount();
                        CoordinateCollection coordinate = new CoordinateCollection();
                        for (int j = 0; j < pcount; j++)
                        {
                                coordinate.Add(
                                        new Coordinate(
                                                feature.GetGeometryRef().GetX(j),
                                                feature.GetGeometryRef().GetY(j)
                                        )
                                );
                        }
                        return new LinearRing(coordinate, CreateShpFile.GeometryFactory);
                }
                static public LineString OgrFeatureToGeosLineString(Feature feature)
                {
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
                        return new LineString(coordinates.ToArray(),CreateShpFile.GeometryFactory);
                }
        }
}
