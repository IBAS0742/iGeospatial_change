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

using System;
using System.IO;
using System.Text;
using System.Diagnostics;

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.IO
{
	/// <summary>
	/// Summary description for GeometryWkbWriter. 
	/// </summary>
	public class GeometryWkbWriter : MarshalByRefObject
	{
        #region Private Fields

        private BytesWriter m_objWriter;
        
        #endregion

        #region Constructors and Destructor

        /// <summary>
        /// ?
        /// </summary>
		public GeometryWkbWriter()
		{
            m_objWriter = new BytesWriter();
		}
        
        #endregion

        #region Public Methods

        public byte[] Write(Geometry geometry)
        {
            BytesOrder order = m_objWriter.Order;

            return Write(geometry, order);
        }

        public byte[] Write(Geometry geometry, BytesOrder order)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            m_objWriter.Initialize();
            m_objWriter.Order = order;

            GeometryWkbMode mode    = GeometryWkbMode.Standard;

            GeometryFactory factory = geometry.Factory;
            if (factory != null)
            {
                CoordinateType coordType = factory.CoordinateType;
                if (coordType == CoordinateType.Measured)
                {
                    mode = GeometryWkbMode.PostGIS;
                }
                else
                {
                    IGeometryProperties properties = geometry.Properties;
                    if (properties != null && properties.Contains("SRID"))
                    {
                        mode = GeometryWkbMode.PostGIS;
                    }
                    else
                    {
                        int dim = factory.CoordinateDimension;
                        if (dim == 3)
                        {
                            mode = GeometryWkbMode.Proposed;
                        }
                    }
                }
            }

            bool bResult     = false;
            byte[] geomBytes = null;

            switch (mode) 
            {
                case GeometryWkbMode.Standard:
                    bResult = WriteStandard(geometry);
                    break;
                case GeometryWkbMode.Proposed:
                    bResult = WriteProposed(geometry);
                    break;
                case GeometryWkbMode.PostGIS:
                    bResult = WritePostGIS(geometry);
                    break;
                case GeometryWkbMode.Custom:
                    bResult = WriteCustom(geometry);
                    break;
            }

            if (bResult)
            {
                geomBytes = m_objWriter.GetBuffer();
            }

            m_objWriter.Uninitialize(); // reset the buffer

            return geomBytes;
        }

        public byte[] Write(Geometry geometry, GeometryWkbMode wkbMode, 
            BytesOrder order)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            m_objWriter.Initialize();
            m_objWriter.Order = order;

            GeometryWkbMode mode    = wkbMode;

            bool bResult     = false;
            byte[] geomBytes = null;

            switch (mode) 
            {
                case GeometryWkbMode.Standard:
                    bResult = WriteStandard(geometry);
                    break;
                case GeometryWkbMode.Proposed:
                    bResult = WriteProposed(geometry);
                    break;
                case GeometryWkbMode.PostGIS:
                    bResult = WritePostGIS(geometry);
                    break;
                case GeometryWkbMode.Custom:
                    bResult = WriteCustom(geometry);
                    break;
            }

            if (bResult)
            {
                geomBytes = m_objWriter.GetBuffer();
            }

            m_objWriter.Uninitialize(); // reset the buffer

            return geomBytes;
        }
        
        #endregion

        #region Public Static Methods
        
        public static string BytesToHex(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            StringBuilder buf = new StringBuilder();
            int nLength = bytes.Length;

            for (int i = 0; i < nLength; i++) 
            {
                byte b = bytes[i];
                buf.Append(ToHexDigit((b >> 4) & 0x0F));
                buf.Append(ToHexDigit(b & 0x0F));
            }

            return buf.ToString();
        }

        private static char ToHexDigit(int n)
        {
            if (n < 0 || n > 15)
                throw new ArgumentException("Nibble value out of range: " + n);
            if (n <= 9)
                return (char) ('0' + n);
            return (char) ('A' + (n - 10));
        }

        #endregion

        #region Private Methods

        private GeometryWkbType GetWkbType(GeometryType geomType)
        {
            if (geomType == GeometryType.Point)
            {
                return GeometryWkbType.Point;
            }
            else if (geomType == GeometryType.LineString)
            {
                return GeometryWkbType.LineString;
            }
            else if (geomType == GeometryType.Polygon)
            {
                return GeometryWkbType.Polygon;
            }
            else if (geomType == GeometryType.MultiPoint)
            {
                return GeometryWkbType.MultiPoint;
            }
            else if (geomType == GeometryType.MultiLineString)
            {
                return GeometryWkbType.MultiLineString;
            }
            else if (geomType == GeometryType.MultiPolygon)
            {
                return GeometryWkbType.MultiPolygon;
            }
            else if (geomType == GeometryType.GeometryCollection)
            {
                return GeometryWkbType.GeometryCollection;
            }
            else 
            {
                throw new GeometryIOException("The well-known binary format for " 
                    + "this Geometry does not exist: " + geomType.ToString());
            }
        }

        private void WriteByteOrder()
        {
            m_objWriter.WriteByte((byte)(int)m_objWriter.Order);
        }

        private void WriteInt(int value)
        {
            m_objWriter.WriteInt32(value);
        }

        private void WriterGeometryType(int value)
        {
            m_objWriter.WriteInt32(value);
        }
        
        #endregion

        #region Handling Standard WKB

        private bool WriteStandard(Geometry geometry)
        {
            GeometryType geomType = geometry.GeometryType;

            if (geomType == GeometryType.Point)
            {
                return WriteStandard((Point)geometry);
            }
            else if (geomType == GeometryType.LineString)
            {
                return WriteStandard((LineString)geometry);
            }
            else if (geomType == GeometryType.Polygon)
            {
                return WriteStandard((Polygon)geometry);
            }
            else if (geomType == GeometryType.MultiPoint)
            {
                return WriteStandard((MultiPoint)geometry);
            }
            else if (geomType == GeometryType.MultiLineString)
            {
                return WriteStandard((MultiLineString)geometry);
            }
            else if (geomType == GeometryType.MultiPolygon)
            {
                return WriteStandard((MultiPolygon)geometry);
            }
            else if (geomType == GeometryType.GeometryCollection)
            {
                return WriteStandard((GeometryCollection)geometry);
            }
            else 
            {
                throw new GeometryIOException("The well-known binary format for " 
                    + "this Geometry does not exist: " + geometry.Name);
            }
        }

        private void WriterStandardType(GeometryWkbType value)
        {
            m_objWriter.WriteInt32((int)value);
        }

        private void WriteStandard(Coordinate coord)
        {
            m_objWriter.WriteDouble(coord.X);
            m_objWriter.WriteDouble(coord.Y);
        }

        private void WriteStandard(ICoordinateList coords)
        {
            int nCount = coords.Count;
            m_objWriter.WriteInt32(nCount);

            for (int i = 0; i < nCount; i++)
            {
                WriteStandard(coords[i]);
            }
        }

        private bool WriteStandard(Point point)
        {
            WriteByteOrder();
            WriterStandardType(GeometryWkbType.Point);
            
            WriteStandard(point.Coordinate);

            return true;
        }

        private bool WriteStandard(LineString lineString)
        {
            WriteByteOrder();
            WriterStandardType(GeometryWkbType.LineString);
            
            WriteStandard(lineString.Coordinates);

            return true;
        }

        private bool WriteStandard(Polygon polygon)
        {
            WriteByteOrder();
            WriterStandardType(GeometryWkbType.Polygon);

            int interiorRings = polygon.NumInteriorRings;
            if (interiorRings <= 0)
            {                  
                m_objWriter.WriteInt32(0);

                return true;
            }

            m_objWriter.WriteInt32(interiorRings + 1);

            WriteStandard(polygon.ExteriorRing.Coordinates);

            for (int i = 0; i < interiorRings; i++)
            {
                WriteStandard(polygon.InteriorRings[i].Coordinates);
            }

            return true;
        }

        private bool WriteStandard(MultiPoint multiPoint)
        {
            WriteByteOrder();
            WriterStandardType(GeometryWkbType.MultiPoint);

            int numGeometries = multiPoint.NumGeometries;

            m_objWriter.WriteInt32(numGeometries);

            for (int i = 0; i < numGeometries; i++)
            {
                WriteStandard(multiPoint[i]);
            }

            return true;
        }

        private bool WriteStandard(MultiLineString multiLineString)
        {
            WriteByteOrder();
            WriterStandardType(GeometryWkbType.MultiLineString);

            int numGeometries = multiLineString.NumGeometries;

            m_objWriter.WriteInt32(numGeometries);

            for (int i = 0; i < numGeometries; i++)
            {
                WriteStandard(multiLineString[i]);
            }

            return true;
        }

        private bool WriteStandard(MultiPolygon multiPolygon)
        {
            WriteByteOrder();
            WriterStandardType(GeometryWkbType.MultiPolygon);

            int numGeometries = multiPolygon.NumGeometries;

            m_objWriter.WriteInt32(numGeometries);

            for (int i = 0; i < numGeometries; i++)
            {
                WriteStandard(multiPolygon[i]);
            }

            return true;
        }

        private bool WriteStandard(GeometryCollection geomCollection)
        {
            WriteByteOrder();
            WriterStandardType(GeometryWkbType.GeometryCollection);

            int numGeometries = geomCollection.NumGeometries;

            m_objWriter.WriteInt32(numGeometries);

            for (int i = 0; i < numGeometries; i++)
            {
                WriteStandard(geomCollection[i]);
            }

            return true;
        }
        
        #endregion

        #region Handling Proposed WKB

        private bool WriteProposed(Geometry geometry)
        {
            GeometryType geomType   = geometry.GeometryType;

            if (geomType == GeometryType.Point)
            {
                return WriteProposed((Point)geometry);
            }
            else if (geomType == GeometryType.LineString)
            {
                return WriteProposed((LineString)geometry);
            }
            else if (geomType == GeometryType.Polygon)
            {
                return WriteProposed((Polygon)geometry);
            }
            else if (geomType == GeometryType.MultiPoint)
            {
                return WriteProposed((MultiPoint)geometry);
            }
            else if (geomType == GeometryType.MultiLineString)
            {
                return WriteProposed((MultiLineString)geometry);
            }
            else if (geomType == GeometryType.MultiPolygon)
            {
                return WriteProposed((MultiPolygon)geometry);
            }
            else if (geomType == GeometryType.GeometryCollection)
            {
                return WriteProposed((GeometryCollection)geometry);
            }
            else 
            {
                throw new GeometryIOException("The well-known binary format for " 
                    + "this Geometry does not exist: " + geometry.Name);
            }
        }

        private void WriteProposed(Coordinate coord)
        {
            m_objWriter.WriteDouble(coord.X);
            m_objWriter.WriteDouble(coord.Y);
            if (coord.Dimension == 3)
            {
                m_objWriter.WriteDouble(coord.GetOrdinate(2));
            }
        }

        private void WriteProposed(ICoordinateList coords)
        {
            int nCount = coords.Count;
            m_objWriter.WriteInt32(nCount);

            for (int i = 0; i < nCount; i++)
            {
                WriteProposed(coords[i]);
            }
        }

        private bool WriteProposed(Point point)
        {
            WriteByteOrder();
            
            int geometryType   = (int)GeometryWkbType.Point;
            int coordDimension = point.CoordinateDimension;

            int nFlag3D  = (int)((coordDimension == 3) ? 0x80000000 : 0);
            int nTypeInt = geometryType | nFlag3D;

            WriterGeometryType(nTypeInt);
            
            WriteProposed(point.Coordinate);

            return true;
        }

        private bool WriteProposed(LineString lineString)
        {
            WriteByteOrder();
             
            int geometryType   = (int)GeometryWkbType.LineString;
            int coordDimension = lineString.CoordinateDimension;

            int nFlag3D  = (int)((coordDimension == 3) ? 0x80000000 : 0);
            int nTypeInt = geometryType | nFlag3D;

            WriterGeometryType(nTypeInt);
            
            WriteProposed(lineString.Coordinates);

            return true;
        }

        private bool WriteProposed(Polygon polygon)
        {
            WriteByteOrder();
            
            int geometryType   = (int)GeometryWkbType.Polygon;
            int coordDimension = polygon.CoordinateDimension;

            int nFlag3D  = (int)((coordDimension == 3) ? 0x80000000 : 0);
            int nTypeInt = geometryType | nFlag3D;

            WriterGeometryType(nTypeInt);

            int interiorRings = polygon.NumInteriorRings;
            if (interiorRings <= 0)
            {                  
                m_objWriter.WriteInt32(0);

                return true;
            }

            m_objWriter.WriteInt32(interiorRings + 1);

            WriteProposed(polygon.ExteriorRing.Coordinates);

            for (int i = 0; i < interiorRings; i++)
            {
                WriteProposed(polygon.InteriorRings[i].Coordinates);
            }

            return true;
        }

        private bool WriteProposed(MultiPoint multiPoint)
        {
            WriteByteOrder();
            
            int geometryType   = (int)GeometryWkbType.MultiPoint;
            int coordDimension = multiPoint.CoordinateDimension;

            int nFlag3D  = (int)((coordDimension == 3) ? 0x80000000 : 0);
            int nTypeInt = geometryType | nFlag3D;

            WriterGeometryType(nTypeInt);

            int numGeometries = multiPoint.NumGeometries;

            m_objWriter.WriteInt32(numGeometries);

            for (int i = 0; i < numGeometries; i++)
            {
                WriteProposed(multiPoint[i]);
            }

            return true;
        }

        private bool WriteProposed(MultiLineString multiLineString)
        {
            WriteByteOrder();
            
            int geometryType   = (int)GeometryWkbType.MultiLineString;
            int coordDimension = multiLineString.CoordinateDimension;

            int nFlag3D  = (int)((coordDimension == 3) ? 0x80000000 : 0);
            int nTypeInt = geometryType | nFlag3D;

            WriterGeometryType(nTypeInt);

            int numGeometries = multiLineString.NumGeometries;

            m_objWriter.WriteInt32(numGeometries);

            for (int i = 0; i < numGeometries; i++)
            {
                WriteProposed(multiLineString[i]);
            }

            return true;
        }

        private bool WriteProposed(MultiPolygon multiPolygon)
        {
            WriteByteOrder();
            
            int geometryType   = (int)GeometryWkbType.MultiPolygon;
            int coordDimension = multiPolygon.CoordinateDimension;

            int nFlag3D  = (int)((coordDimension == 3) ? 0x80000000 : 0);
            int nTypeInt = geometryType | nFlag3D;

            WriterGeometryType(nTypeInt);

            int numGeometries = multiPolygon.NumGeometries;

            m_objWriter.WriteInt32(numGeometries);

            for (int i = 0; i < numGeometries; i++)
            {
                WriteProposed(multiPolygon[i]);
            }

            return true;
        }

        private bool WriteProposed(GeometryCollection geomCollection)
        {
            WriteByteOrder();
            
            int geometryType   = (int)GeometryWkbType.GeometryCollection;
            int coordDimension = geomCollection.CoordinateDimension;

            int nFlag3D  = (int)((coordDimension == 3) ? 0x80000000 : 0);
            int nTypeInt = geometryType | nFlag3D;

            WriterGeometryType(nTypeInt);

            int numGeometries = geomCollection.NumGeometries;

            m_objWriter.WriteInt32(numGeometries);

            for (int i = 0; i < numGeometries; i++)
            {
                WriteProposed(geomCollection[i]);
            }

            return true;
        }
        
        #endregion

        #region Handling PostGIS WKB

        private bool WritePostGIS(Geometry geometry)
        {
            // 1. Determine the geometry parameters
            CoordinateType coordType     = CoordinateType.Default;
            IGeometryProperties geomProp = geometry.Properties;            
            GeometryFactory geomFactory  = geometry.Factory;

            int nDimension          = geometry.CoordinateDimension;
            GeometryType geomType   = geometry.GeometryType;

            if (geomFactory != null)
            {
                coordType = geomFactory.CoordinateType;
            }
            int nSRID = -1;

            if (geomProp != null && geomProp.Contains("SRID"))
            {
                nSRID = Convert.ToInt32(geomProp["SRID"]);
            }

            // 2. Write the bye order
            WriteByteOrder();

            // 3. Write the geometry type word
            int geomWkbType   = (int)GetWkbType(geomType);
            uint geomTypeWord = (uint)geomWkbType;
            if (nDimension > 2) 
            {
                geomTypeWord |= 0x80000000;
            }
            if (coordType == CoordinateType.Measured) 
            {
                geomTypeWord |= 0x40000000;
            }
            if (nSRID > 0) 
            {
                geomTypeWord |= 0x20000000;
            }

            WriterGeometryType((int)geomTypeWord);

            // 4. Write the spatial reference ID, if any
            if (nSRID > 0) 
            {
                WriteInt(nSRID);
            }

            if (geomType == GeometryType.Point)
            {
                return WritePostGIS((Point)geometry);
            }
            else if (geomType == GeometryType.LineString)
            {
                return WritePostGIS((LineString)geometry);
            }
            else if (geomType == GeometryType.Polygon)
            {
                return WritePostGIS((Polygon)geometry);
            }
            else if (geomType == GeometryType.MultiPoint)
            {
                return WritePostGIS((MultiPoint)geometry);
            }
            else if (geomType == GeometryType.MultiLineString)
            {
                return WritePostGIS((MultiLineString)geometry);
            }
            else if (geomType == GeometryType.MultiPolygon)
            {
                return WritePostGIS((MultiPolygon)geometry);
            }
            else if (geomType == GeometryType.GeometryCollection)
            {
                return WritePostGIS((GeometryCollection)geometry);
            }
            else 
            {
                throw new GeometryIOException("The well-known binary format for " 
                    + "this Geometry does not exist: " + geometry.Name);
            }
        }

        private void WritePostGIS(Coordinate coord )
        {
            m_objWriter.WriteDouble(coord.X);
            m_objWriter.WriteDouble(coord.Y);

            CoordinateType CoordType = coord.CoordinateType;

            if (coord.Dimension == 2)
            {
                if (CoordType == CoordinateType.Measured)
                {
                    CoordinateM measured = (CoordinateM)coord;

                    m_objWriter.WriteDouble(measured.Measure);
                }
            }
            else
            {
                // write the Z-value
                m_objWriter.WriteDouble(coord.GetOrdinate(2));

                // check and write the Measure
                if (CoordType == CoordinateType.Measured)
                {
                    Coordinate3DM measured = (Coordinate3DM)coord;

                    m_objWriter.WriteDouble(measured.Measure);
                }
            }
        }

        private void WritePostGIS(ICoordinateList coords)
        {
            int nCount = coords.Count;
            m_objWriter.WriteInt32(nCount);

            for (int i = 0; i < nCount; i++)
            {
                WritePostGIS(coords[i]);
            }
        }

        private bool WritePostGIS(Point point)
        {
            WritePostGIS(point.Coordinate);

            return true;
        }

        private bool WritePostGIS(LineString lineString)
        {
            WritePostGIS(lineString.Coordinates);

            return true;
        }

        private bool WritePostGIS(Polygon polygon)
        {
            int interiorRings = polygon.NumInteriorRings;
            if (interiorRings <= 0)
            {                  
                m_objWriter.WriteInt32(0);

                return true;
            }

            m_objWriter.WriteInt32(interiorRings + 1);

            WritePostGIS(polygon.ExteriorRing.Coordinates);

            LinearRing[] arrRings = polygon.InteriorRings;

            for (int i = 0; i < interiorRings; i++)
            {
                WritePostGIS(arrRings[i].Coordinates);
            }

            return true;
        }

        private bool WritePostGIS(MultiPoint multiPoint)
        {
            int numGeometries = multiPoint.NumGeometries;

            m_objWriter.WriteInt32(numGeometries);

            for (int i = 0; i < numGeometries; i++)
            {
                Point point = multiPoint[i];
                WritePostGIS(point);
            }

            return true;
        }

        private bool WritePostGIS(MultiLineString multiLineString)
        {
            int numGeometries = multiLineString.NumGeometries;

            m_objWriter.WriteInt32(numGeometries);

            for (int i = 0; i < numGeometries; i++)
            {
                LineString lineRing = multiLineString[i];
                WritePostGIS(lineRing);
            }

            return true;
        }

        private bool WritePostGIS(MultiPolygon multiPolygon)
        {
            int numGeometries = multiPolygon.NumGeometries;

            m_objWriter.WriteInt32(numGeometries);

            for (int i = 0; i < numGeometries; i++)
            {            
                Polygon polygon = multiPolygon[i];
                WritePostGIS(polygon);
            }

            return true;
        }

        private bool WritePostGIS(GeometryCollection geomCollection)
        {
            int numGeometries = geomCollection.NumGeometries;

            m_objWriter.WriteInt32(numGeometries);

            for (int i = 0; i < numGeometries; i++)
            {
                WritePostGIS(geomCollection[i]);
            }

            return true;
        }
        
        #endregion

        #region Handling Custom WKB

        private bool WriteCustom(Geometry geometry)
        {
            throw new NotImplementedException();
        }
        
        #endregion
    }
}
