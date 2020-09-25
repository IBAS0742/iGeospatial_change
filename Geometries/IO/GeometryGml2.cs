using System;

namespace iGeospatial.Geometries.IO
{
	/// <summary>
	/// Summary description for GeometryGml2.
	/// </summary>
	public sealed class GeometryGml2
	{                
        #region Public GML 2 Constants
        
        // Namespace constants
        public const string GmlNamespace        = "http://www.opengis.net/gml";
        public const string GmlPrefix           = "gml";
        public const string GmlGid              = "gid";
		
        // Source Coordinate System
        public const string GmlAttrSrsname      = "srsName";
        public const string GmlAttrEpsgSrsname  = "http://www.opengis.net/gml/srs/epsg.xml#";
		
        // GML associative types
        public const string GmlGeometryMember   = "geometryMember";
        public const string GmlPointMember      = "pointMember";
        public const string GmlPolygonMember    = "polygonMember";
        public const string GmlLineStringMember = "lineStringMember";
        public const string GmlOuterBoundaryIs  = "outerBoundaryIs";
        public const string GmlInnerBoundaryIs  = "innerBoundaryIs";
		
        // Primitive Geometries
        public const string GmlPoint            = "Point";
        public const string GmlLineString       = "LineString";
        public const string GmlLinearRing       = "LinearRing";
        public const string GmlPolygon          = "Polygon";
        public const string GmlBox              = "Box";
		
        // Aggregate Ggeometries
        public const string GmlMultiGeometry    = "MultiGeometry";
        public const string GmlMultiPoint       = "MultiPoint";
        public const string GmlMultiLineString  = "MultiLineString";
        public const string GmlMultiPolygon     = "MultiPolygon";
		
        // Coordinates
        public const string GmlCoordinates      = "coordinates";
        public const string GmlCoord            = "coord";
        public const string GmlCoordX           = "X";
        public const string GmlCoordY           = "Y";
        public const string GmlCoordZ           = "Z";
        public const string GmlCoordM           = "M";


        public const string GmlCoordinateSeparator = ",";
        public const string GmlTupleSeparator      = " ";

        #endregion
		
        #region Constructors and Destructor

        private GeometryGml2()
		{
		}

        #endregion
	}
}
