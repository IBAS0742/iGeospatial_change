#region License
// <copyright>
//         iGeospatial Geometries Package
//       
// This is part of the Open Geospatial Library for .NET.
// 
// Package Description:
// This is a collection of C# classes that implement the fundamental 
// operations required to validate a given geo-spatial data set to 
// a known topological specification.
// It aims to provide a complete implementation of the Open Geospatial
// Consortium (www.opengeospatial.org) specifications for Simple 
// Feature Geometry.
// 
// Contact Information:
//     Paul Selormey (paulselormey@gmail.com or paul@toolscenter.org)
//     
// Credits:
// This library is based on the JTS Topology Suite, a Java library by
// 
//     Vivid Solutions Inc. (www.vividsolutions.com)  
//     
// License:
// See the license.txt file in the package directory.   
// </copyright>
#endregion

#region WKT and WKB Formats
//-------------------------------------------------------------------------
//
//TITLE:  ZM values and SRID for Simple Features 
//
//AUTHOR:	Name: Sandro Santilli
//	Email: strk@refractions.net
//
//DATE:   27 December 2005
//
//CATEGORY: Simple Features Revision Proposal
//
//-------------------------------------------------------------------------
//
//1. Background
//
//OpenGIS document 99-402r2 introduces semantic and well-known
//representations for Z-geometries. This proposal extend the well-known
//representations to optionally also hold a misure (M) and a SRID.
//Misures, as Z values, are attributes of 2D vertexes, but their
//semantic is unspecified in this document, as they could be used
//for any kind 'misurement'. SRID is an attribute of the whole feature.
//
//This document defines how geometry can have Z,M or both values and SRID
//in a way which is compatible to the existing 2D OpenGIS Simple Features
//specification AND to the Z-Geometry documented in OpenGIS 99-402r2.
//
//2. Proposal
//
//2.1. Definition of ZM-Geometry
//
//a) A geometry can have either 2, 3 or 4 dimensions.
//b) 3rd dimension of a 3d geometry can either represent Z or M (3DZ or 3DM).
//c) 4d geometries contain both Z and M (in this order).
//d) M and Z values are associated with every vertex.
//e) M and Z values are undefined within surface interiors.
//
//Any ZM-Geometry can be converted into a 2D geometry by discarding all its
//Z and M values. The resulting 2D geometry is the "shadow" of the ZM-Geometry.
//2D geometries cannot be safely converted into ZM-Geometries, since their Z
//and M values are undefined, and not necessarily zero.
//
//2.2. Extensions to Well-Known-Binary format
//
//The 2d OpenGIS Simple Features specification has the following geometry types:
//
//enum wkbGeometryType 
//{
//	wkbPoint = 1,
//	wkbLineString = 2,
//	wkbPolygon = 3,
//	wkbMultiPoint = 4,
//	wkbMultiLineString = 5,
//	wkbMultiPolygon = 6,
//	wkbGeometryCollection = 7
//}
//
//Document 99-402r2 introduces a Z-presence flag (wkbZ) which OR'ed
//to the type specifies the presence of Z coordinate:
//
//	wkbZ = 0x80000000
//
//This proposal suggest the use of an M-presence flag (wkbM) to
//allow for XY, XYM, XYZ and XYZM geometryes, and SRID-presence
//flag to allow for embedded SRID:
//
//	wkbM = 0x40000000
//	wkbSRID = 0x20000000
//
//Possible resulting geometry types are:
//
//enum wkbGeometryTypeZ 
//{
//
//	wkbPoint = 1,
//	wkbLineString = 2,
//	wkbPolygon = 3,
//	wkbMultiPoint = 4,
//	wkbMultiLineString = 5,
//	wkbMultiPolygon = 6,
//	wkbGeometryCollection = 7,
//
//	// | 0x80000000
//	wkbPointZ = 0x80000001,
//	wkbLineStringZ = 0x80000002,
//	wkbPolygonZ = 0x80000003,
//	wkbMultiPointZ = 0x80000004,
//	wkbMultiLineStringZ = 0x80000005,
//	wkbMultiPolygonZ = 0x80000006,
//	wkbGeometryCollectionZ = 0x80000007,
//
//	// | 0x40000000
//	wkbPointM = 0x40000001,
//	wkbLineStringM = 0x40000002,
//	wkbPolygonM = 0x40000003,
//	wkbMultiPointM = 0x40000004,
//	wkbMultiLineStringM = 0x40000005,
//	wkbMultiPolygonM = 0x40000006,
//	wkbGeometryCollectionM = 0x40000007,
//
//	// | 0x40000000 | 0x80000000
//	wkbPointZM = 0xC0000001,
//	wkbLineStringZM = 0xC0000002,
//	wkbPolygonZM = 0xC0000003,
//	wkbMultiPointZM = 0xC0000004,
//	wkbMultiLineStringZM = 0xC0000005,
//	wkbMultiPolygonZM = 0xC0000006,
//	wkbGeometryCollectionZM = 0xC0000007,
//
//	// | 0x20000000 
//	wkbPointS = 0x20000001,
//	wkbLineStringS = 0x20000002,
//	wkbPolygonS = 0x20000003,
//	wkbMultiPointS = 0x20000004,
//	wkbMultiLineStringS = 0x20000005,
//	wkbMultiPolygonS = 0x20000006,
//	wkbGeometryCollectionS = 0x20000007,
//
//	// | 0x20000000 | 0x80000000
//	wkbPointZS = 0xA0000001,
//	wkbLineStringZS = 0xA0000002,
//	wkbPolygonZS = 0xA0000003,
//	wkbMultiPointZS = 0xA0000004,
//	wkbMultiLineStringZS = 0xA0000005,
//	wkbMultiPolygonZS = 0xA0000006,
//	wkbGeometryCollectionZS = 0xA0000007,
//
//	// | 0x20000000 | 0x40000000
//	wkbPointMS = 0x60000001,
//	wkbLineStringMS = 0x60000002,
//	wkbPolygonMS = 0x60000003,
//	wkbMultiPointMS = 0x60000004,
//	wkbMultiLineStringMS = 0x60000005,
//	wkbMultiPolygonMS = 0x60000006,
//	wkbGeometryCollectionMS = 0x60000007,
//
//	// | 0x20000000 | 0x40000000 | 0x80000000
//	wkbPointZMS = 0xE0000001,
//	wkbLineStringZMS = 0xE0000002,
//	wkbPolygonZMS = 0xE0000003,
//	wkbMultiPointZMS = 0xE0000004,
//	wkbMultiLineStringZMS = 0xE0000005,
//	wkbMultiPolygonZMS = 0xE0000006,
//	wkbGeometryCollectionZMS = 0xE0000007,
//}
//
//
//If the SRID flag is set it's value is encoded as a 4byte integer
//right after the type integer.
//
//If only wkbZ or wkbM flags are set Point coordinates will
//be XYZ or XYM, if both wkbZ and wkbM flags are set Point
//coordinates will be XYZM (Z first).
//
//For example, a ZM-Point geometry at the location (10,20) with Z==30,
//M==40 and SRID=4326 would be:
//
//WKBPoint 
//{
//	byte    byteOrder;      // wkbXDR or wkbNDR
//	uint32  wkbType;        // (wkbPoint+wkbZ+wkbM+wkbSRID) = 0xE0000001
//	uint32  SRID;           // 4326
//	Point 
//  {
//		Double    x;    // 10.0
//		Double    y;    // 20.0
//		Double    z;    // 30.0
//		Double    m;    // 40.0
//	}
//}
//
//
//2.3. Extensions to Well-Known-Text format
//
//Geometry SRID presence and value would be represented using a
//"SRID=#;" prefix to the WKT text:
//
//	"SRID=4326;POINT(1 2)"
//
//3DZ geometry will be represented as:
//
//	"POINT(1 2 3)"
//
//4D geometry will be represented as:
//
//	"POINT(1 2 3 4)"
//
//3DM geometry will be represented as:
//
//	"POINTM(1 2 3)"
//	or
//	"GEOMETRYCOLLECTIONM(POINTM(1 2 3), LINESTRINGM(1 2 3, 4 5 6))"
//
//Note that the coordinates structure of a geometry must be consistent,
//you can't mix dimensions in a single geometry.

#endregion

using System;
using System.Diagnostics;

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.IO
{
	/// <summary>
	/// Summary description for GeometryWkbReader.
	/// </summary>
	public class GeometryWkbReader : MarshalByRefObject
	{
        #region Private Fields

        private GeometryFactory m_objFactory;
        private BytesReader     m_objReader;
        private PrecisionModel  m_objPrecision;

        #endregion

        #region Constructors and Destructor
        
        /// <summary>
        /// ?
        /// </summary>
		public GeometryWkbReader()
		{
            m_objFactory   = new GeometryFactory();
            m_objReader    = new BytesReader();
            m_objPrecision = m_objFactory.PrecisionModel;
		}
        
        /// <summary>
        /// ?
        /// </summary>
		public GeometryWkbReader(PrecisionModel precision)
		{
            if (precision == null)
            {
                throw new ArgumentNullException("precision");
            }

            m_objFactory   = new GeometryFactory(precision);
            m_objReader    = new BytesReader();
            m_objPrecision = precision;
        }

        /// <summary>
        /// ?
        /// </summary>
		public GeometryWkbReader(GeometryFactory factory)
		{
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            m_objFactory   = factory;
            m_objReader    = new BytesReader();
            m_objPrecision = m_objFactory.PrecisionModel;
        }
        
        #endregion

        #region Public Methods

        public Geometry Read(byte[] geometryData)
        {
            if (geometryData == null)
            {
                throw new ArgumentNullException("geometryData");
            }
            if (geometryData.Length == 0)
            {
                throw new ArgumentException("geometryData");
            }

            m_objReader.Initialize(geometryData);

            Geometry geometry = ReadStandard();

            m_objReader.Uninitialize(); // reset the buffer

            return geometry;
        }

        public Geometry Read(byte[] geometryData, GeometryWkbMode mode)
        {
            if (geometryData == null || geometryData.Length == 0)
            {
                throw new ArgumentNullException("geometryData");
            }

            m_objReader.Initialize(geometryData);

            Geometry geometry = null;

            switch (mode) 
            {
                case GeometryWkbMode.Standard:
                    geometry = ReadStandard();
                    break;
                case GeometryWkbMode.Proposed:
                    geometry = ReadProposed();
                    break;
                case GeometryWkbMode.PostGIS:
                    geometry = ReadPostGIS();
                    break;
                case GeometryWkbMode.Custom:
                    geometry = ReadCustom();
                    break;
            }

            m_objReader.Uninitialize(); // reset the buffer

            return geometry;
        }

        #endregion

        #region Public Static Methods
        
        /// <summary>
        /// Converts a hexadecimal string to a byte array.
        /// </summary>
        /// <param name="hex">A string containing hex digits.</param>
        /// <returns></returns>
        public static byte[] HexToBytes(string hex)
        {
            if (hex == null || hex.Length == 0)
            {
                return null;
            }

            int nLength  = hex.Length;
            int byteLen  = nLength / 2;
            byte[] bytes = new byte[byteLen];

            for (int i = 0; i < nLength / 2; i++) 
            {
                int i2 = 2 * i;
                if (i2 + 1 > nLength)
                    throw new ArgumentException("Hex string has odd length");

                int nib1 = HexToInt(hex[i2]);
                int nib0 = HexToInt(hex[i2 + 1]);
                byte b = (byte) ((nib1 << 4) + (byte) nib0);
                bytes[i] = b;
            }

            return bytes;
        }

        private static int HexToInt(char hex)
        {
            //int nib = Character.digit(hex, 16);
            int nib = Convert.ToInt32(Convert.ToString(hex), 16);
            if (nib < 0)
                throw new ArgumentException("Invalid hex digit");

            return nib;
        }
        
        #endregion

        #region Handling Standard WKB

        private Geometry ReadStandard()
        {
            // 1. Determine the byte order of the binary data
            BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();
            
            // 2. Initialize the reader to the appropriate byte order.
            m_objReader.Order = byteOrder;

            // 3. Determine the data/geometry type of the object
            int geomType = m_objReader.ReadInt32();

            // 4. Handover to the specialized geometry handlers...
            switch (geomType)
            {
                case 1:  // Point
                    return ReadPoint();
					
                case 2:  // LineString
                    return ReadLineString();
					
                case 3:  // Polygon
                    return ReadPolygon();
					
                case 4:  // MultiPoint
                    return ReadMultiPoint();
					
                case 5:  // MultiLineString
                    return ReadMultiLineString();
					
                case 6:  // MultiPolygon
                    return ReadMultiPolygon();
					
                case 7:  // GeometryCollection
                    return ReadGeometryCollection();
					
                default:
                    throw new GeometryIOException("The geometry type is not supported.");
            }
        }

        private Coordinate ReadCoordinate()
        {
            Coordinate coord = new Coordinate(m_objReader.ReadDouble(), 
                m_objReader.ReadDouble());

            coord.MakePrecise(m_objPrecision);

            return coord;
        }

        private ICoordinateList ReadCoordinates()
        {
            int nCount             = m_objReader.ReadInt32();
            ICoordinateList coords = new CoordinateCollection(nCount);
            
            for (int i = 0; i < nCount; i++)
            {
                coords.Add(ReadCoordinate());
            }

            return coords;
        }

        private Point ReadPoint()
        {
            Coordinate coord = ReadCoordinate();

            if (coord != null)
            {
                return m_objFactory.CreatePoint(coord);
            }

            return null;
        }

        private LineString ReadLineString()
        {
            ICoordinateList coords = ReadCoordinates();
            if (coords != null)
            {
                return m_objFactory.CreateLineString(coords);
            }

            return null;
        }

        private Polygon ReadPolygon()
        {
            int nRings = m_objReader.ReadInt32();
            if (nRings <= 0)
            {
                return null;
            }
            ICoordinateList coords = ReadCoordinates();
            if (coords == null || coords.Count == 0)
            {
                return null;
            }
            LinearRing shell   = m_objFactory.CreateLinearRing(coords);
            LinearRing[] holes = null;
            if (nRings > 1) // are there holes?
            {
                int nHoles = nRings - 1;
                holes      = new LinearRing[nHoles];
                for (int i = 0; i < nHoles; i++)
                {
                    holes[i] = m_objFactory.CreateLinearRing(ReadCoordinates());
                }
            }

            return m_objFactory.CreatePolygon(shell, holes);
        }

        private MultiPoint ReadMultiPoint()
        {
            int nPoints    = m_objReader.ReadInt32();
            Point[] points = new Point[nPoints];
            for (int i = 0; i < nPoints; i++)
            {
                BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();  // handle the byte order
                m_objReader.Order    = byteOrder;

                int geomType = m_objReader.ReadInt32();
                if (geomType != 1)
                {
                    throw new GeometryIOException("The data is badly formed. " 
                        + "A Point geometry is expected");
                }
                
                points[i] = ReadPoint();
            }

            return m_objFactory.CreateMultiPoint(points);
        }

        private MultiLineString ReadMultiLineString()
        {
            int nPolylines     = m_objReader.ReadInt32();
            if (nPolylines <= 0)
            {
                return null;
            }

            LineString[] lines = new LineString[nPolylines];
            for (int i = 0; i < nPolylines; i++)
            {
                BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();  // handle the byte order
                m_objReader.Order    = byteOrder;

                int geomType = m_objReader.ReadInt32();
                if (geomType != 2)
                {
                    throw new GeometryIOException("The data is badly formed. " 
                        + "A LineString geometry is expected");
                }
                
                lines[i] = ReadLineString();
            }

            return m_objFactory.CreateMultiLineString(lines);
        }

        private MultiPolygon ReadMultiPolygon()
        {
            int nPolygons = m_objReader.ReadInt32();
            if (nPolygons <= 0)
            {
                return null;
            }

            Polygon[] polygons = new Polygon[nPolygons];
            for (int i = 0; i < nPolygons; i++)
            {
                BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();  // handle the byte order
                m_objReader.Order    = byteOrder;

                int geomType = m_objReader.ReadInt32();
                if (geomType != 3)
                {
                    throw new GeometryIOException("The data is badly formed. " 
                        + "A Polygon geometry is expected");
                }
                
                polygons[i] = ReadPolygon();
            }

            return m_objFactory.CreateMultiPolygon(polygons);
        }

        private GeometryCollection ReadGeometryCollection()
        {
            int nGeometries       = m_objReader.ReadInt32();
            if (nGeometries <= 0)
            {
                return null;
            }

            Geometry[] geometries = new Geometry[nGeometries];

            for (int i = 0; i < nGeometries; i++)
            {
                BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();  // handle the byte order
                m_objReader.Order    = byteOrder;

                int geomType = m_objReader.ReadInt32();
                switch (geomType)
                {
                    case 1:
                        geometries[i] = ReadPoint();
                        break;
                    case 2:
                        geometries[i] = ReadLineString();
                        break;
                    case 3:
                        geometries[i] = ReadPolygon();
                        break;
                    case 4:
                        geometries[i] = ReadMultiPoint();
                        break;
                    case 5:
                        geometries[i] = ReadMultiLineString();
                        break;
                    case 6:
                        geometries[i] = ReadMultiPolygon();
                        break;
                    case 7:
                        geometries[i] = ReadGeometryCollection();
                        break;
                    default:
                        throw new GeometryIOException("The geometry type is not supported.");
                }                
            }

            return m_objFactory.CreateGeometryCollection(geometries);
        }
        
        #endregion

        #region Handling Proposed WKB

        private Geometry ReadProposed()
        {
            // 1. Determine the byte order of the binary data
            BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();
            
            // 2. Initialize the reader to the appropriate byte order.
            m_objReader.Order    = byteOrder;

            // 3. Determine the data type of the object
            int dataType = m_objReader.ReadInt32();

            // 4. Remove high flag bits, if any to obtain the geometry type
            int geomType = dataType & 0xFF; 

            bool hasZ = (dataType & 0x80000000) != 0;

            // 5. Handover to the specialized geometry handlers...
            switch (geomType)
            {
                case 1:  // Point
                    return ReadPoint(hasZ);
					
                case 2:  // LineString
                    return ReadLineString(hasZ);
					
                case 3:  // Polygon
                    return ReadPolygon(hasZ);
					
                case 4:  // MultiPoint
                    return ReadMultiPoint(hasZ);
					
                case 5:  // MultiLineString
                    return ReadMultiLineString(hasZ);
					
                case 6:  // MultiPolygon
                    return ReadMultiPolygon(hasZ);
					
                case 7:  // GeometryCollection
                    return ReadGeometryCollection(hasZ);
					
                default:
                    throw new GeometryIOException("The geometry type is not supported.");
            }
        }

        private Coordinate ReadCoordinate(bool hasZ)
        {
            double x = m_objReader.ReadDouble();
            double y = m_objReader.ReadDouble();
            Coordinate coord = null;
            if (hasZ) 
            {
                double z = m_objReader.ReadDouble();
                coord    = new Coordinate3D(x, y, z);
            } 
            else 
            {
                coord = new Coordinate(x, y);
            }

            coord.MakePrecise(m_objPrecision);

            return coord;
        }

        private ICoordinateList ReadCoordinates(bool hasZ)
        {
            int nCount             = m_objReader.ReadInt32();
            ICoordinateList coords = new CoordinateCollection(nCount);
            
            for (int i = 0; i < nCount; i++)
            {
                coords.Add(ReadCoordinate(hasZ));
            }

            return coords;
        }

        private Point ReadPoint(bool hasZ)
        {
            Coordinate coord = ReadCoordinate(hasZ);

            if (coord != null)
            {
                return m_objFactory.CreatePoint(coord);
            }

            return null;
        }

        private LineString ReadLineString(bool hasZ)
        {
            ICoordinateList coords = ReadCoordinates(hasZ);
            if (coords != null)
            {
                return m_objFactory.CreateLineString(coords);
            }

            return null;
        }

        private Polygon ReadPolygon(bool hasZ)
        {
            int nRings = m_objReader.ReadInt32();
            if (nRings <= 0)
            {
                return null;
            }
            ICoordinateList coords = ReadCoordinates(hasZ);
            if (coords == null || coords.Count == 0)
            {
                return null;
            }
            LinearRing shell   = m_objFactory.CreateLinearRing(coords);
            LinearRing[] holes = null;
            if (nRings > 1) // are there holes?
            {
                int nHoles = nRings - 1;
                holes      = new LinearRing[nHoles];
                for (int i = 0; i < nHoles; i++)
                {
                    holes[i] = m_objFactory.CreateLinearRing(
                        ReadCoordinates(hasZ));
                }
            }

            return m_objFactory.CreatePolygon(shell, holes);
        }

        private MultiPoint ReadMultiPoint(bool hasZ)
        {
            int nPoints    = m_objReader.ReadInt32();
            Point[] points = new Point[nPoints];
            for (int i = 0; i < nPoints; i++)
            {
                BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();  // handle the byte order
                m_objReader.Order    = byteOrder;

                int dataType = m_objReader.ReadInt32();
                int geomType = dataType & 0xFF; 
                bool bHasZ   = (dataType & 0x80000000) != 0;
                if (geomType != 1)
                {
                    throw new GeometryIOException("The data is badly formed. " 
                        + "A Point geometry is expected");
                }
                
                points[i] = ReadPoint(bHasZ);
            }

            return m_objFactory.CreateMultiPoint(points);
        }

        private MultiLineString ReadMultiLineString(bool hasZ)
        {
            int nPolylines     = m_objReader.ReadInt32();
            if (nPolylines <= 0)
            {
                return null;
            }

            LineString[] lines = new LineString[nPolylines];
            for (int i = 0; i < nPolylines; i++)
            {
                BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();  // handle the byte order
                m_objReader.Order    = byteOrder;

                int dataType = m_objReader.ReadInt32();
                int geomType = dataType & 0xFF; 
                bool bHasZ   = (dataType & 0x80000000) != 0;
                if (geomType != 2)
                {
                    throw new GeometryIOException("The data is badly formed. " 
                        + "A LineString geometry is expected");
                }
                
                lines[i] = ReadLineString(bHasZ);
            }

            return m_objFactory.CreateMultiLineString(lines);
        }

        private MultiPolygon ReadMultiPolygon(bool hasZ)
        {
            int nPolygons = m_objReader.ReadInt32();
            if (nPolygons <= 0)
            {
                return null;
            }

            Polygon[] polygons = new Polygon[nPolygons];
            for (int i = 0; i < nPolygons; i++)
            {
                BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();  // handle the byte order
                m_objReader.Order    = byteOrder;

                int dataType = m_objReader.ReadInt32();
                int geomType = dataType & 0xFF; 
                bool bHasZ   = (dataType & 0x80000000) != 0;
                if (geomType != 3)
                {
                    throw new GeometryIOException("The data is badly formed. " 
                        + "A Polygon geometry is expected");
                }
                
                polygons[i] = ReadPolygon(bHasZ);
            }

            return m_objFactory.CreateMultiPolygon(polygons);
        }

        private GeometryCollection ReadGeometryCollection(bool hasZ)
        {
            int nGeometries       = m_objReader.ReadInt32();
            if (nGeometries <= 0)
            {
                return null;
            }

            Geometry[] geometries = new Geometry[nGeometries];

            for (int i = 0; i < nGeometries; i++)
            {
                BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();  // handle the byte order
                m_objReader.Order    = byteOrder;

                int dataType = m_objReader.ReadInt32();
                int geomType = dataType & 0xFF; 
                bool bHasZ   = (dataType & 0x80000000) != 0;
                switch (geomType)
                {
                    case 1:
                        geometries[i] = ReadPoint(bHasZ);
                        break;
                    case 2:
                        geometries[i] = ReadLineString(bHasZ);
                        break;
                    case 3:
                        geometries[i] = ReadPolygon(bHasZ);
                        break;
                    case 4:
                        geometries[i] = ReadMultiPoint(bHasZ);
                        break;
                    case 5:
                        geometries[i] = ReadMultiLineString(bHasZ);
                        break;
                    case 6:
                        geometries[i] = ReadMultiPolygon(bHasZ);
                        break;
                    case 7:
                        geometries[i] = ReadGeometryCollection(bHasZ);
                        break;
                    default:
                        throw new GeometryIOException("The geometry type is not supported.");
                }                
            }

            return m_objFactory.CreateGeometryCollection(geometries);
        }
        
        #endregion

        #region Handling PostGIS WKB

        private Geometry ReadPostGIS()
        {
            // 1. Determine the byte order of the binary data
            BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();
            
            // 2. Initialize the reader to the appropriate byte order.
            m_objReader.Order    = byteOrder;

            // 3. Determine the data type of the object
            int dataType = m_objReader.ReadInt32();

            // 4. Remove high flag bits, if any to obtain the geometry type
            int geomType = dataType & 0x1FFFFFFF; 

            bool hasZ = (dataType & 0x80000000) != 0;
            bool hasM = (dataType & 0x40000000) != 0;
            bool hasS = (dataType & 0x20000000) != 0;

            int nSRID = -1;
            if (hasS) 
            {
                nSRID = m_objReader.ReadInt32();
            }

            Geometry geometry = null;

            // 5. Handover to the specialized geometry handlers...
            switch (geomType)
            {
                case 1:  // Point
                    geometry = ReadPoint(hasZ, hasM);
                    break;
					
                case 2:  // LineString
                    geometry = ReadLineString(hasZ, hasM);
                    break;
					
                case 3:  // Polygon
                    geometry = ReadPolygon(hasZ, hasM);
                    break;
					
                case 4:  // MultiPoint
                    geometry = ReadMultiPoint(hasZ, hasM);
                    break;
					
                case 5:  // MultiLineString
                    geometry = ReadMultiLineString(hasZ, hasM);
                    break;
					
                case 6:  // MultiPolygon
                    geometry = ReadMultiPolygon(hasZ, hasM);
                    break;
					
                case 7:  // GeometryCollection
                    geometry = ReadGeometryCollection(hasZ, hasM);
                    break;
					
                default:
                    throw new GeometryIOException("The geometry type is not supported.");
            }

            if ((hasS && nSRID > -1) && geometry != null)
            {              
                if (geometry.Properties == null)
                {
                    geometry.CreateProperties();
                }
                geometry.Properties["SRID"] = nSRID;
            }

            return geometry;
        }

        private Coordinate ReadCoordinate(bool hasZ, bool hasM)
        {
            double x = m_objReader.ReadDouble();
            double y = m_objReader.ReadDouble();
            Coordinate coord = null;
            if (hasZ) 
            {
                double z = m_objReader.ReadDouble();
                if (hasM) 
                {
                    double m = m_objReader.ReadDouble();

                    coord = new Coordinate3DM(x, y, z, m);
                }
                else
                {
                    coord = new Coordinate3D(x, y, y);
                }
            } 
            else 
            {
                if (hasM) 
                {
                    double m = m_objReader.ReadDouble();
                    coord    = new CoordinateM(x, y, m);
                }
                else
                {
                    coord = new Coordinate(x, y);
                }
            }

            coord.MakePrecise(m_objPrecision);

            return coord;
        }

        private ICoordinateList ReadCoordinates(bool hasZ, bool hasM)
        {
            int nCount             = m_objReader.ReadInt32();
            ICoordinateList coords = new CoordinateCollection(nCount);
            
            for (int i = 0; i < nCount; i++)
            {
                coords.Add(ReadCoordinate(hasZ, hasM));
            }

            return coords;
        }

        private Point ReadPoint(bool hasZ, bool hasM)
        {
            Coordinate coord = ReadCoordinate(hasZ, hasM);

            if (coord != null)
            {
                return m_objFactory.CreatePoint(coord);
            }

            return null;
        }

        private LineString ReadLineString(bool hasZ, bool hasM)
        {
            ICoordinateList coords = ReadCoordinates(hasZ, hasM);
            if (coords != null)
            {
                return m_objFactory.CreateLineString(coords);
            }

            return null;
        }

        private Polygon ReadPolygon(bool hasZ, bool hasM)
        {
            int nRings = m_objReader.ReadInt32();
            if (nRings <= 0)
            {
                return null;
            }
            ICoordinateList coords = ReadCoordinates(hasZ, hasM);
            if (coords == null || coords.Count == 0)
            {
                return null;
            }
            LinearRing shell   = m_objFactory.CreateLinearRing(coords);
            LinearRing[] holes = null;
            if (nRings > 1) // are there holes?
            {
                int nHoles = nRings - 1;
                holes      = new LinearRing[nHoles];
                for (int i = 0; i < nHoles; i++)
                {
                    holes[i] = m_objFactory.CreateLinearRing(
                        ReadCoordinates(hasZ, hasM));
                }
            }

            return m_objFactory.CreatePolygon(shell, holes);
        }

        private MultiPoint ReadMultiPoint(bool hasZ, bool hasM)
        {
            int nPoints    = m_objReader.ReadInt32();
            Point[] points = new Point[nPoints];
            for (int i = 0; i < nPoints; i++)
            {
                BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();  // handle the byte order
                m_objReader.Order    = byteOrder;

                int dataType = m_objReader.ReadInt32();
                int geomType = dataType & 0x1FFFFFFF; 

                bool bHasZ = (dataType & 0x80000000) != 0;
                bool bHasM = (dataType & 0x40000000) != 0;
                bool bHasS = (dataType & 0x20000000) != 0;

                int nSRID = -1;
                if (bHasS) 
                {
                    nSRID = m_objReader.ReadInt32();
                    if (nSRID > 0)
                    {
                    }
                }
                if (geomType != 1)
                {
                    throw new GeometryIOException("The data is badly formed. " 
                        + "A Point geometry is expected");
                }
                
                points[i] = ReadPoint(bHasZ, bHasM);
            }

            return m_objFactory.CreateMultiPoint(points);
        }

        private MultiLineString ReadMultiLineString(bool hasZ, bool hasM)
        {
            int nPolylines     = m_objReader.ReadInt32();
            if (nPolylines <= 0)
            {
                return null;
            }

            LineString[] lines = new LineString[nPolylines];
            for (int i = 0; i < nPolylines; i++)
            {
                BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();  // handle the byte order
                m_objReader.Order    = byteOrder;

                int dataType = m_objReader.ReadInt32();
                int geomType = dataType & 0x1FFFFFFF; 

                bool bHasZ = (dataType & 0x80000000) != 0;
                bool bHasM = (dataType & 0x40000000) != 0;
                bool bHasS = (dataType & 0x20000000) != 0;

                int nSRID = -1;
                if (bHasS) 
                {
                    nSRID = m_objReader.ReadInt32();
                    if (nSRID > 0)
                    {
                    }
                }
                if (geomType != 2)
                {
                    throw new GeometryIOException("The data is badly formed. " 
                        + "A LineString geometry is expected");
                }
                
                lines[i] = ReadLineString(bHasZ, bHasM);
            }

            return m_objFactory.CreateMultiLineString(lines);
        }

        private MultiPolygon ReadMultiPolygon(bool hasZ, bool hasM)
        {
            int nPolygons = m_objReader.ReadInt32();
            if (nPolygons <= 0)
            {
                return null;
            }

            Polygon[] polygons = new Polygon[nPolygons];
            for (int i = 0; i < nPolygons; i++)
            {
                BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();  // handle the byte order
                m_objReader.Order    = byteOrder;

                int dataType = m_objReader.ReadInt32();
                int geomType = dataType & 0x1FFFFFFF; 

                bool bHasZ = (dataType & 0x80000000) != 0;
                bool bHasM = (dataType & 0x40000000) != 0;
                bool bHasS = (dataType & 0x20000000) != 0;

                int nSRID = -1;
                if (bHasS) 
                {
                    nSRID = m_objReader.ReadInt32();
                    if (nSRID > 0)
                    {
                    }
                }
                if (geomType != 3)
                {
                    throw new GeometryIOException("The data is badly formed. " 
                        + "A Polygon geometry is expected");
                }
                
                polygons[i] = ReadPolygon(bHasZ, bHasM);
            }

            return m_objFactory.CreateMultiPolygon(polygons);
        }

        private GeometryCollection ReadGeometryCollection(bool hasZ, bool hasM)
        {
            int nGeometries = m_objReader.ReadInt32();
            if (nGeometries <= 0)
            {
                return null;
            }

            Geometry[] geometries = new Geometry[nGeometries];

            for (int i = 0; i < nGeometries; i++)
            {
                BytesOrder byteOrder = (BytesOrder)m_objReader.ReadByte();  // handle the byte order
                m_objReader.Order    = byteOrder;

                int dataType = m_objReader.ReadInt32();
                int geomType = dataType & 0x1FFFFFFF; 

                bool bHasZ = (dataType & 0x80000000) != 0;
                bool bHasM = (dataType & 0x40000000) != 0;
                bool bHasS = (dataType & 0x20000000) != 0;

                int nSRID = -1;
                if (bHasS) 
                {
                    nSRID = m_objReader.ReadInt32();
                    if (nSRID > 0)
                    {
                    }
                }
                switch (geomType)
                {
                    case 1:
                        geometries[i] = ReadPoint(bHasZ, bHasM);
                        break;
                    case 2:
                        geometries[i] = ReadLineString(bHasZ, bHasM);
                        break;
                    case 3:
                        geometries[i] = ReadPolygon(bHasZ, bHasM);
                        break;
                    case 4:
                        geometries[i] = ReadMultiPoint(bHasZ, bHasM);
                        break;
                    case 5:
                        geometries[i] = ReadMultiLineString(bHasZ, bHasM);
                        break;
                    case 6:
                        geometries[i] = ReadMultiPolygon(bHasZ, bHasM);
                        break;
                    case 7:
                        geometries[i] = ReadGeometryCollection(bHasZ, bHasM);
                        break;
                    default:
                        throw new GeometryIOException("The geometry type is not supported.");
                }                
            }

            return m_objFactory.CreateGeometryCollection(geometries);
        }
        
        #endregion

        #region Handling Custom WKB

        private Geometry ReadCustom()
        {
            throw new NotImplementedException();
        }

        #endregion
	}
}
