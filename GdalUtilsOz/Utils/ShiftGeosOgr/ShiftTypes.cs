using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iGeospatial.Geometries;
using OSGeo.OGR;

namespace GdalUtilsOz.Utils.ShiftGeosOgr
{
        class ShiftTypes
        {
                public static wkbGeometryType FromGeosToOgr(GeometryType type) { 
                        switch (type)
                        {
                                case GeometryType.Point:
                                        return wkbGeometryType.wkbPoint;
                                case GeometryType.LinearRing:
                                        return wkbGeometryType.wkbLinearRing;
                                case GeometryType.LineString:
                                        return wkbGeometryType.wkbLineString;
                                case GeometryType.Polygon:
                                        return wkbGeometryType.wkbTriangle;
                                case GeometryType.Triangle:
                                        return wkbGeometryType.wkbTriangle;
                                case GeometryType.MultiPoint:
                                        return wkbGeometryType.wkbMultiPoint;
                                case GeometryType.MultiLineString:
                                        return wkbGeometryType.wkbMultiLineString;
                                case GeometryType.MultiPolygon:
                                        return wkbGeometryType.wkbMultiPolygon;
                                default:
                                        return wkbGeometryType.wkbUnknown;
                        }
                }
                public static GeometryType FromOgrToGeos(wkbGeometryType type)
                {
                        switch (type)
                        {
                                case wkbGeometryType.wkbPoint:
                                        return GeometryType.Point;
                                case wkbGeometryType.wkbLinearRing:
                                        return GeometryType.LinearRing;
                                case wkbGeometryType.wkbLineString:
                                        return GeometryType.LineString;
                                case wkbGeometryType.wkbTriangle:
                                        return GeometryType.Triangle;
                                case wkbGeometryType.wkbMultiPoint:
                                        return GeometryType.MultiPoint;
                                case wkbGeometryType.wkbMultiLineString:
                                        return GeometryType.MultiLineString;
                                case wkbGeometryType.wkbMultiPolygon:
                                        return GeometryType.MultiPolygon;
                                default:
                                        return GeometryType.None;
                        }
                }
        }
}
