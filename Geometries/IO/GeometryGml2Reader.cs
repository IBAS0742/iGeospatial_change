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
using System.Xml;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Globalization;

using iGeospatial.Texts;
using iGeospatial.Exceptions;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.IO
{
	/// <summary>
	/// Summary description for GeometryGml2Reader.
	/// </summary>
	public class GeometryGml2Reader : GeometryTextReader
	{
        #region Private Fields

        private static readonly char[] TupleSeparator = new char[]{' '};
        private static readonly char[] Separator = new char[]{','};

        #endregion

        #region Constructors and Destructor

		public GeometryGml2Reader() : base()
		{
		}

        public GeometryGml2Reader(GeometryFactory geometryFactory)
            : base(geometryFactory)
        {
        }

        #endregion
 
        #region Public Methods

        /// <summary> 
        /// Converts a GML 2 Text representation to a Geometry.
        /// </summary>
        /// <param name="gml2Text">
        /// one or more "Geometry Tagged Text" strings (see the OpenGIS
        /// Simple Features Specification) separated by whitespace
        /// </param>
        /// <returns> a Geometry specified by GML 2
        /// </returns>
        /// <exception cref="GeometryIOException">
        /// If a parsing problem occurs.
        /// </exception>
        public override Geometry Read(string gml2Text)
        {
            if (gml2Text == null || gml2Text.Length == 0)
            {
                return null;
            }

            StringReader textReader = new StringReader(gml2Text);

            return Read(textReader);
        }
		
        /// <summary>  
        /// Converts a GML 2 Text representation to a Geometry.
        /// </summary>
        /// <param name="textReader">A reader, which will return a "Geometry Tagged Text"
        /// string (see the OpenGIS Simple Features Specification)
        /// </param>
        /// <returns> A Geometry read from reader. </returns>  
        /// <exception cref="GeometryIOException">
        /// If a parsing problem occurs.
        /// </exception>
        public override Geometry Read(TextReader textReader)
        {
            XmlTextReader reader = new XmlTextReader(textReader);

            reader.WhitespaceHandling = WhitespaceHandling.None;

            return Read(reader);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringReader"></param>
        /// <returns></returns>
        public Geometry Read(XmlTextReader reader)
        {
            do 
            {
                if (reader.IsStartElement(GeometryGml2.GmlPoint))
                {
                    return ReadPoint(reader);
                }
                else if (reader.IsStartElement(GeometryGml2.GmlLineString))
                {
                    return ReadLineString(reader);
                }
                else if (reader.IsStartElement(GeometryGml2.GmlPolygon))
                {
                    return ReadPolygon(reader);
                }
                else if (reader.IsStartElement(GeometryGml2.GmlMultiPoint))
                {
                    return ReadMultiPoint(reader);
                }
                else if (reader.IsStartElement(GeometryGml2.GmlMultiLineString))
                {
                    return ReadMultiLineString(reader);
                }
                else if (reader.IsStartElement(GeometryGml2.GmlMultiPolygon))
                {
                    return ReadMultiPolygon(reader);
                }
                else if (reader.IsStartElement(GeometryGml2.GmlMultiGeometry))
                {
                    return ReadGeometryCollection(reader);
                }
            } while(reader.Read());

            throw new GeometryIOException("No geometric object is found.");
        }        
        
        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Coordinate ReadCoord(XmlReader reader)
        {            
            Coordinate coordinate = null;

            double x = 0, y = 0, z = Double.NaN, m = Double.NaN;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == GeometryGml2.GmlCoordX)
                    {
                        x = Convert.ToDouble(reader.ReadString());
                    }
                    else if (reader.Name == GeometryGml2.GmlCoordY)
                    {
                        y = Convert.ToDouble(reader.ReadString());
                    }
                    else if (reader.Name == GeometryGml2.GmlCoordZ)
                    {
                        z = Convert.ToDouble(reader.ReadString());
                    }
                    else if (reader.Name == GeometryGml2.GmlCoordM)
                    {
                        m = Convert.ToDouble(reader.ReadString());
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == GeometryGml2.GmlCoord)
                    {
                        break;
                    }
                }
            }

            if (Double.IsNaN(z) || Double.IsInfinity(z))
            {
                if ((Double.IsNaN(m) || Double.IsInfinity(m)))
                {
                    coordinate = new Coordinate(x, y);
                }
                else
                {
                    coordinate = new CoordinateM(x, y, m);
                }
            }
            else
            {
                if ((Double.IsNaN(m) || Double.IsInfinity(m)))
                {
                    coordinate = new Coordinate3D(x, y, z);
                }
                else
                {
                    coordinate = new Coordinate3DM(x, y, z, m);
                }
            }

            return coordinate;
        }

        private Coordinate ReadCoordinate(XmlReader reader)
        {
            string strCoordinate = reader.ReadString();
            if (strCoordinate != null && strCoordinate.Length > 0)
            {
                string[] arrOrd = strCoordinate.Split(Separator);
                if (arrOrd != null && arrOrd.Length >= 2)
                {
                    int nLength = arrOrd.Length;
                    if (nLength == 2)
                    {
                        return new Coordinate(Convert.ToDouble(arrOrd[0]), 
                            Convert.ToDouble(arrOrd[1]));
                    }
                    else if (nLength == 3)
                    {
                        return new Coordinate3D(Convert.ToDouble(arrOrd[0]), 
                            Convert.ToDouble(arrOrd[1]), 
                            Convert.ToDouble(arrOrd[2]));
                    }
                }
            }

            return null;
        }

        private ICoordinateList ReadCoordinates(XmlReader reader)
        {
            string strCoordinates = reader.ReadString();
            if (strCoordinates != null && strCoordinates.Length > 0)
            {
                string[] arrOrds = strCoordinates.Split(TupleSeparator);

                if (arrOrds != null && arrOrds.Length > 0)
                {
                    int nCount = arrOrds.Length;
                    CoordinateCollection coordinates = 
                        new CoordinateCollection(nCount);

                    for (int i = 0; i < nCount; i++)
                    {
                        string strCoordinate = arrOrds[i];

                        if (strCoordinate == null || strCoordinate.Length == 0)
                        {
                            continue;
                        }

                        Coordinate coord = null;
                        string[] arrOrd  = strCoordinate.Split(Separator);
                        if (arrOrd != null && arrOrd.Length >= 2)
                        {
                            int nLength = arrOrd.Length;
                            if (nLength == 2)
                            {
                                coord = new Coordinate(Convert.ToDouble(arrOrd[0]), 
                                    Convert.ToDouble(arrOrd[1]));
                            }
                            else if (nLength == 3)
                            {
                                coord = new Coordinate3D(Convert.ToDouble(arrOrd[0]), 
                                    Convert.ToDouble(arrOrd[1]), 
                                    Convert.ToDouble(arrOrd[2]));
                            }
                        }

                        if (coord != null)
                        {
                            coordinates.Add(coord);
                        }
                    }

                    return coordinates;
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Point ReadPoint(XmlReader reader)
        {    
            string strSRID = null;
            string strGID  = null;
            int nSRID      = -1;
            if (reader.Name == GeometryGml2.GmlPoint && reader.HasAttributes)
            {
                ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                if (strSRID != null && strSRID.Length > 0)
                {
                    nSRID = Convert.ToInt32(strSRID);
                }
            }

            Coordinate coordinate = null;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == GeometryGml2.GmlPoint)
                    {
                        if (reader.HasAttributes)
                        {
                            ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                            if (strSRID != null && strSRID.Length > 0)
                            {
                                nSRID = Convert.ToInt32(strSRID);
                            }
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlCoord)
                    {
                        coordinate = ReadCoord(reader);
                    }
                    else if (reader.Name == GeometryGml2.GmlCoordinates)
                    {
                        coordinate = ReadCoordinate(reader);
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == GeometryGml2.GmlPoint)
                    {
                        break;
                    }
                }
            }

            if (coordinate != null)
            {
                Point geometry = m_objFactory.CreatePoint(coordinate);

                if (nSRID > 0 || (strGID != null && strGID.Length > 0))
                {
                    geometry.CreateProperties();
                    if (nSRID > 0)
                    {
                        geometry.Properties.Add("SRID", nSRID);
                    }
                    if (strGID != null && strGID.Length > 0)
                    {
                        geometry.Properties.Add("GID", strGID);
                    }
                }

                return geometry;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private LineString ReadLineString(XmlReader reader)
        {
            string strSRID = null;
            string strGID  = null;
            int nSRID      = -1;
            if (reader.Name == GeometryGml2.GmlLineString && reader.HasAttributes)
            {
                ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                if (strSRID != null && strSRID.Length > 0)
                {
                    nSRID = Convert.ToInt32(strSRID);
                }
            }

            ICoordinateList coordinates = null;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == GeometryGml2.GmlLineString)
                    {
                        if (reader.HasAttributes)
                        {
                            ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                            if (strSRID != null && strSRID.Length > 0)
                            {
                                nSRID = Convert.ToInt32(strSRID);
                            }
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlCoord)
                    {
                        if (coordinates == null)
                        {
                            coordinates = new CoordinateCollection();
                        }

                        coordinates.Add(ReadCoord(reader));
                    }
                    else if (reader.Name == GeometryGml2.GmlCoordinates)
                    {
                        coordinates = ReadCoordinates(reader);
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == GeometryGml2.GmlLineString)
                    {
                        break;
                    }
                }
            }

            if (coordinates != null && coordinates.Count > 1)
            {
                LineString geometry = m_objFactory.CreateLineString(coordinates);

                if (nSRID > 0 || (strGID != null && strGID.Length > 0))
                {
                    geometry.CreateProperties();
                    if (nSRID > 0)
                    {
                        geometry.Properties.Add("SRID", nSRID);
                    }
                    if (strGID != null && strGID.Length > 0)
                    {
                        geometry.Properties.Add("GID", strGID);
                    }
                }

                return geometry;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private LinearRing ReadLinearRing(XmlReader reader)
        {            
            string strSRID = null;
            string strGID  = null;
            int nSRID      = -1;
            if (reader.Name == GeometryGml2.GmlLinearRing && reader.HasAttributes)
            {
                ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                if (strSRID != null && strSRID.Length > 0)
                {
                    nSRID = Convert.ToInt32(strSRID);
                }
            }

            ICoordinateList coordinates = null;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == GeometryGml2.GmlLinearRing)
                    {
                        if (reader.HasAttributes)
                        {
                            ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                            if (strSRID != null && strSRID.Length > 0)
                            {
                                nSRID = Convert.ToInt32(strSRID);
                            }
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlCoord)
                    {
                        if (coordinates == null)
                        {
                            coordinates = new CoordinateCollection();
                        }

                        coordinates.Add(ReadCoord(reader));
                    }
                    else if (reader.Name == GeometryGml2.GmlCoordinates)
                    {
                        coordinates = ReadCoordinates(reader);
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == GeometryGml2.GmlLineString)
                    {
                        break;
                    }
                }
            }

            if (coordinates != null && coordinates.Count > 1)
            {
                LinearRing geometry = m_objFactory.CreateLinearRing(coordinates);

                if (nSRID > 0 || (strGID != null && strGID.Length > 0))
                {
                    geometry.CreateProperties();
                    if (nSRID > 0)
                    {
                        geometry.Properties.Add("SRID", nSRID);
                    }
                    if (strGID != null && strGID.Length > 0)
                    {
                        geometry.Properties.Add("GID", strGID);
                    }
                }

                return geometry;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Polygon ReadPolygon(XmlReader reader)
        {
            string strSRID = null;
            string strGID  = null;
            int nSRID      = -1;
            if (reader.Name == GeometryGml2.GmlPolygon && reader.HasAttributes)
            {
                ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                if (strSRID != null && strSRID.Length > 0)
                {
                    nSRID = Convert.ToInt32(strSRID);
                }
            }

            LinearRing exteriorRing    = null;
            GeometryList interiorRings = null;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == GeometryGml2.GmlPolygon)
                    {
                        if (reader.HasAttributes)
                        {
                            ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                            if (strSRID != null && strSRID.Length > 0)
                            {
                                nSRID = Convert.ToInt32(strSRID);
                            }
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlOuterBoundaryIs)
                    {
                        exteriorRing = ReadLinearRing(reader);
                    }
                    else if (reader.Name == GeometryGml2.GmlInnerBoundaryIs)
                    {
                        if (interiorRings == null)
                        {
                            interiorRings = new GeometryList();
                        }

                        LinearRing interiorRing = ReadLinearRing(reader);
                        if (interiorRing != null)
                        {
                            interiorRings.Add(interiorRing);
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == GeometryGml2.GmlPolygon)
                    {
                        break;
                    }
                }
            }

            if (exteriorRing != null)
            {
                Polygon geometry = m_objFactory.CreatePolygon(exteriorRing, 
                    (interiorRings == null) ? null : 
                    interiorRings.ToLinearRingArray());

                if (nSRID > 0 || (strGID != null && strGID.Length > 0))
                {
                    geometry.CreateProperties();
                    if (nSRID > 0)
                    {
                        geometry.Properties.Add("SRID", nSRID);
                    }
                    if (strGID != null && strGID.Length > 0)
                    {
                        geometry.Properties.Add("GID", strGID);
                    }
                }

                return geometry;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private MultiPoint ReadMultiPoint(XmlReader reader)
        {
            string strSRID = null;
            string strGID  = null;
            int nSRID      = -1;
            if (reader.Name == GeometryGml2.GmlMultiPoint && reader.HasAttributes)
            {
                ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                if (strSRID != null && strSRID.Length > 0)
                {
                    nSRID = Convert.ToInt32(strSRID);
                }
            }

            GeometryList points = null;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == GeometryGml2.GmlMultiPoint)
                    {
                        if (reader.HasAttributes)
                        {
                            ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                            if (strSRID != null && strSRID.Length > 0)
                            {
                                nSRID = Convert.ToInt32(strSRID);
                            }
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlPointMember)
                    {
                        if (points == null)
                        {
                            points = new GeometryList();
                        }

                        Point point = ReadPoint(reader);
                        if (point != null)
                        {
                            points.Add(point);
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == GeometryGml2.GmlMultiPoint)
                    {
                        break;
                    }
                }
            }

            if (points != null && points.Count > 0)
            {
                MultiPoint geometry = m_objFactory.CreateMultiPoint(
                    points.ToPointArray());

                if (nSRID > 0 || (strGID != null && strGID.Length > 0))
                {
                    geometry.CreateProperties();
                    if (nSRID > 0)
                    {
                        geometry.Properties.Add("SRID", nSRID);
                    }
                    if (strGID != null && strGID.Length > 0)
                    {
                        geometry.Properties.Add("GID", strGID);
                    }
                }

                return geometry;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private MultiLineString ReadMultiLineString(XmlReader reader)
        {
            string strSRID = null;
            string strGID  = null;
            int nSRID      = -1;
            if (reader.Name == GeometryGml2.GmlMultiLineString && 
                reader.HasAttributes)
            {
                ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                if (strSRID != null && strSRID.Length > 0)
                {
                    nSRID = Convert.ToInt32(strSRID);
                }
            }

            GeometryList lineStrings = null;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == GeometryGml2.GmlMultiLineString)
                    {
                        if (reader.HasAttributes)
                        {
                            ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                            if (strSRID != null && strSRID.Length > 0)
                            {
                                nSRID = Convert.ToInt32(strSRID);
                            }
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlLineStringMember)
                    {
                        if (lineStrings == null)
                        {
                            lineStrings = new GeometryList();
                        }

                        LineString lineString = ReadLineString(reader);
                        if (lineString != null)
                        {
                            lineStrings.Add(lineString);
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == GeometryGml2.GmlMultiLineString)
                    {
                        break;                       
                    }
                }
            }

            if (lineStrings != null && lineStrings.Count > 0)
            {
                MultiLineString geometry = m_objFactory.CreateMultiLineString(
                    lineStrings.ToLineStringArray());

                if (nSRID > 0 || (strGID != null && strGID.Length > 0))
                {
                    geometry.CreateProperties();
                    if (nSRID > 0)
                    {
                        geometry.Properties.Add("SRID", nSRID);
                    }
                    if (strGID != null && strGID.Length > 0)
                    {
                        geometry.Properties.Add("GID", strGID);
                    }
                }

                return geometry;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private MultiPolygon ReadMultiPolygon(XmlReader reader)
        {
            string strSRID = null;
            string strGID  = null;
            int nSRID      = -1;
            if (reader.Name == GeometryGml2.GmlMultiPolygon && reader.HasAttributes)
            {
                ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                if (strSRID != null && strSRID.Length > 0)
                {
                    nSRID = Convert.ToInt32(strSRID);
                }
            }

            GeometryList polygons = null;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == GeometryGml2.GmlMultiPolygon)
                    {
                        if (reader.HasAttributes)
                        {
                            ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                            if (strSRID != null && strSRID.Length > 0)
                            {
                                nSRID = Convert.ToInt32(strSRID);
                            }
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlPolygonMember)
                    {
                        if (polygons == null)
                        {
                            polygons = new GeometryList();
                        }

                        Polygon polygon = ReadPolygon(reader);
                        if (polygon != null)
                        {
                            polygons.Add(polygon);
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == GeometryGml2.GmlMultiPolygon)
                    {
                        break;
                    }
                }
            }

            if (polygons != null && polygons.Count > 0)
            {
                MultiPolygon geometry = m_objFactory.CreateMultiPolygon(
                    polygons.ToPolygonArray());

                if (nSRID > 0 || (strGID != null && strGID.Length > 0))
                {
                    geometry.CreateProperties();
                    if (nSRID > 0)
                    {
                        geometry.Properties.Add("SRID", nSRID);
                    }
                    if (strGID != null && strGID.Length > 0)
                    {
                        geometry.Properties.Add("GID", strGID);
                    }
                }

                return geometry;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private GeometryCollection ReadGeometryCollection(XmlReader reader)
        {
            string strSRID = null;
            string strGID  = null;
            int nSRID      = -1;
            if (reader.Name == GeometryGml2.GmlMultiGeometry && reader.HasAttributes)
            {
                ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                if (strSRID != null && strSRID.Length > 0)
                {
                    nSRID = Convert.ToInt32(strSRID);
                }
            }

            GeometryList geometries = new GeometryList();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == GeometryGml2.GmlMultiGeometry)
                    {
                        if (reader.HasAttributes)
                        {
                            ReadGeometryAttributes(reader, ref strSRID, ref strGID);
                            if (strSRID != null && strSRID.Length > 0)
                            {
                                nSRID = Convert.ToInt32(strSRID);
                            }
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlPoint)
                    {
                        Point geometry = ReadPoint(reader);
                        if (geometry != null)
                        {
                            geometries.Add(geometry);
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlLineString)
                    {
                        LineString geometry = ReadLineString(reader);
                        if (geometry != null)
                        {
                            geometries.Add(geometry);
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlPolygon)
                    {
                        Polygon geometry = ReadPolygon(reader);
                        if (geometry != null)
                        {
                            geometries.Add(geometry);
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlMultiPoint)
                    {
                        MultiPoint geometry = ReadMultiPoint(reader);
                        if (geometry != null)
                        {
                            geometries.Add(geometry);
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlMultiLineString)
                    {
                        MultiLineString geometry = ReadMultiLineString(reader);
                        if (geometry != null)
                        {
                            geometries.Add(geometry);
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlMultiPolygon)
                    {
                        MultiPolygon geometry = ReadMultiPolygon(reader);
                        if (geometry != null)
                        {
                            geometries.Add(geometry);
                        }
                    }
                    else if (reader.Name == GeometryGml2.GmlMultiGeometry)
                    {
                        GeometryCollection geometry = ReadGeometryCollection(reader);
                        if (geometry != null)
                        {
                            geometries.Add(geometry);
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == GeometryGml2.GmlMultiGeometry)
                    {
                        break;
                    }
                }
            }

            if (geometries.Count > 0)
            {
                GeometryCollection geometry = m_objFactory.CreateGeometryCollection(
                    geometries.ToArray());

                if (nSRID > 0 || (strGID != null && strGID.Length > 0))
                {
                    geometry.CreateProperties();
                    if (nSRID > 0)
                    {
                        geometry.Properties.Add("SRID", nSRID);
                    }
                    if (strGID != null && strGID.Length > 0)
                    {
                        geometry.Properties.Add("GID", strGID);
                    }
                }

                return geometry;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="srid"></param>
        /// <param name="gid"></param>
        private void ReadGeometryAttributes(XmlReader reader, 
            ref string srid, ref string gid)
        {   
            srid = reader.GetAttribute(GeometryGml2.GmlAttrSrsname);
            gid  = reader.GetAttribute(GeometryGml2.GmlGid);

            if (srid != null)
            {
                // try removing any appended string...
                if (srid.Length > 40)
                {
                    srid = srid.Replace(GeometryGml2.GmlAttrEpsgSrsname, null);
                }
                else
                {
                    // GML v1.0 supports <... srsName="EPSG:">...</...>
                    if (srid.IndexOf("EPSG:") >= 0)
                    {
                        srid = srid.Replace("EPSG:", null);
                    }
                }
            }
        }
       
        #endregion
    }
}
