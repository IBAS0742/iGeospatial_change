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
using System.Globalization;

using iGeospatial.Texts;
using iGeospatial.Exceptions;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.IO
{
	/// <summary>
	/// Summary description for GeometryGml2Writer.
	/// </summary>
	public class GeometryGml2Writer : GeometryTextWriter
	{
        #region Private Fields

        private bool   m_bUseCoord;
        private bool   m_bUsePrefix;
        private bool   m_bUseEpsgSrs;
        private string m_strPrefix;
        
        #endregion

        #region Constructors and Destructor
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml2Writer"/> class.
        /// </summary>
        public GeometryGml2Writer() : base()
        {
            Reset();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml2Writer"/> class.
        /// </summary>
        public GeometryGml2Writer(PrecisionModel customPrecision)
            : base(customPrecision)
        {
            Reset();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml2Writer"/> class.
        /// </summary>
        public GeometryGml2Writer(PrecisionModel customPrecision, int indent)
            : base(customPrecision, indent)
        {
            Reset();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml2Writer"/> class.
        /// </summary>
        public GeometryGml2Writer(CultureInfo culture) : base(culture)
        {
            Reset();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml2Writer"/> class.
        /// </summary>
        public GeometryGml2Writer(CultureInfo culture, int indent)
            : base(culture, indent)
        {
            Reset();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml2Writer"/> class.
        /// </summary>
        public GeometryGml2Writer(CultureInfo culture, 
            PrecisionModel customPrecision) : base(culture, customPrecision)
        {
            Reset();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml2Writer"/> class.
        /// </summary>
        public GeometryGml2Writer(CultureInfo culture, 
            PrecisionModel customPrecision, int indent) 
            : base(culture, customPrecision, indent)
        {
            Reset();
        }

        #endregion

        #region Public Properties

        public bool UseCoord
        {
            get
            {
                return m_bUseCoord;
            }

            set
            {
                m_bUseCoord = value;
            }
        }

        public bool UsePrefix
        {
            get
            {
                return m_bUsePrefix;
            }

            set
            {
                m_bUsePrefix = value;
            }
        }

        public bool UseEpsgSrs
        {
            get
            {
                return m_bUseEpsgSrs;
            }

            set
            {
                m_bUseEpsgSrs = value;
            }
        }

        public string Prefix
        {
            get
            {
                return m_strPrefix;
            }

            set
            {
                m_strPrefix = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>  
        /// Converts a Geometry to its well-known text representation.
        /// </summary>
        /// <param name="geometry"> a Geometry to process
        /// </param>
        /// <returns>
        /// A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification)
        /// </returns>
        public override string Write(Geometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            try
            {
                PrecisionModel precision = geometry.PrecisionModel;
                GeometryFactory factory  = geometry.Factory;
                m_bIsMeasured            = false;
                if (factory != null)
                {
                    CoordinateType coordType = factory.CoordinateType;
                    m_bIsMeasured = (coordType == CoordinateType.Measured);
                }

                StringBuilder builder = new StringBuilder();
                StringWriter  writer  = new StringWriter(builder);

                XmlTextWriter xmlWriter = new XmlTextWriter(writer);
                xmlWriter.Formatting    = Formatting.Indented;

                if (m_nIndent > 0)
                {
                    xmlWriter.Indentation = m_nIndent;
                }
			
                WriteGeometry(geometry, xmlWriter, m_bIncludeSRID);

                return builder.ToString();           
            }
            catch (IOException ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
        }

        /// <summary>  
        /// Converts a Geometry to its well-known text representation.
        /// </summary>
        /// <param name="geometry"> a Geometry to process
        /// </param>
        /// <returns>
        /// A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification)
        /// </returns>
        public override void Write(Geometry geometry, TextWriter writer)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
			
            try
            {
                GeometryFactory factory  = geometry.Factory;
                m_bIsMeasured            = false;
                if (factory != null)
                {
                    CoordinateType coordType = factory.CoordinateType;
                    m_bIsMeasured = (coordType == CoordinateType.Measured);
                }

                XmlTextWriter xmlWriter = new XmlTextWriter(writer);
                xmlWriter.Formatting    = Formatting.Indented;

                if (m_nIndent > 0)
                {
                    xmlWriter.Indentation = m_nIndent;
                }

                WriteGeometry(geometry, xmlWriter, m_bIncludeSRID); 
            }
            catch (IOException ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="stream"></param>
        public void Write(Geometry geometry, XmlWriter writer)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            try
            {
                GeometryFactory factory  = geometry.Factory;
                m_bIsMeasured            = false;
                if (factory != null)
                {
                    CoordinateType coordType = factory.CoordinateType;
                    m_bIsMeasured = (coordType == CoordinateType.Measured);
                }

                WriteGeometry(geometry, writer, m_bIncludeSRID); 
            }
            catch (IOException ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
        }
		
        /// <summary>  
        /// Same as write, but with newlines and spaces to make the
        /// well-known text more readable.
        /// </summary>
        /// <param name="geometry"> a Geometry to process
        /// </param>
        /// <returns>
        /// A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification), with newlines and spaces
        /// </returns>
        public override string WriteFormatted(Geometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            try
            {
                PrecisionModel precision = geometry.PrecisionModel;
                GeometryFactory factory  = geometry.Factory;
                m_bIsMeasured            = false;
                if (factory != null)
                {
                    CoordinateType coordType = factory.CoordinateType;
                    m_bIsMeasured = (coordType == CoordinateType.Measured);
                }

                UpdateFormatter(precision);

                StringBuilder builder = new StringBuilder();
                StringWriter  writer  = new StringWriter(builder);

                XmlTextWriter xmlWriter = new XmlTextWriter(writer);
                xmlWriter.Formatting    = Formatting.Indented;

                if (m_nIndent > 0)
                {
                    xmlWriter.Indentation = m_nIndent;
                }
			
                WriteGeometry(geometry, xmlWriter, m_bIncludeSRID);

                return builder.ToString();           
            }
            catch (IOException ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
        }

        /// <summary>  Same as write, but with newlines and spaces to make the
        /// well-known text more readable.
        /// 
        /// </summary>
        /// <param name="geometry"> a Geometry to process
        /// </param>
        /// <returns>A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification), with newlines and spaces
        /// </returns>
        public override void WriteFormatted(Geometry geometry, TextWriter writer)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            try
            {
                PrecisionModel precision = geometry.PrecisionModel;
                GeometryFactory factory  = geometry.Factory;
                m_bIsMeasured            = false;
                if (factory != null)
                {
                    CoordinateType coordType = factory.CoordinateType;
                    m_bIsMeasured = (coordType == CoordinateType.Measured);
                }

                UpdateFormatter(precision);

                XmlTextWriter xmlWriter = new XmlTextWriter(writer);
                xmlWriter.Formatting    = Formatting.Indented;

                if (m_nIndent > 0)
                {
                    xmlWriter.Indentation = m_nIndent;
                }

                WriteGeometry(geometry, xmlWriter, m_bIncludeSRID); 
            }
            catch (IOException ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
        }

        /// <summary>  
        /// Same as write, but with newlines and spaces to make the
        /// well-known text more readable.
        /// 
        /// </summary>
        /// <param name="geometry">A Geometry to process.</param>
        /// <returns>A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification), with newlines and spaces
        /// </returns>
        public override void WriteFormatted(Geometry geometry, 
            TextWriter writer, int indentation)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            try
            {
                int nIndent = m_nIndent;
                m_nIndent   = indentation;

                PrecisionModel precision = geometry.PrecisionModel;

                UpdateFormatter(precision);

                XmlTextWriter xmlWriter = new XmlTextWriter(writer);
                xmlWriter.Formatting    = Formatting.Indented;

                if (m_nIndent > 0)
                {
                    xmlWriter.Indentation = m_nIndent;
                }

                WriteGeometry(geometry, xmlWriter, m_bIncludeSRID); 
            
                m_nIndent   = nIndent;
            }
            catch (IOException ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public string Write(Envelope envelope)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException("envelope");
            }

            try
            {
                m_bIsMeasured         = false;

                StringBuilder builder = new StringBuilder();
                StringWriter  writer  = new StringWriter(builder);

                XmlTextWriter xmlWriter = new XmlTextWriter(writer);
                xmlWriter.Formatting    = Formatting.Indented;

                if (m_nIndent > 0)
                {
                    xmlWriter.Indentation = m_nIndent;
                }
			
                WriteStartElement(xmlWriter, GeometryGml2.GmlBox, 
                    null, m_bIncludeSRID);
                
                if (!envelope.IsEmpty)
                {
                    CoordinateCollection coordindates = new CoordinateCollection(2);
                    coordindates.Add(envelope.Min);
                    coordindates.Add(envelope.Max);

                    Write(coordindates, xmlWriter);
                }
                
                WriteEndElement(xmlWriter);

                return builder.ToString();           
            }
            catch (IOException ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="writer"></param>
        public void Write(Envelope envelope, TextWriter writer)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException("envelope");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
			
            try
            {
                m_bIsMeasured         = false;

                XmlTextWriter xmlWriter = new XmlTextWriter(writer);
                xmlWriter.Formatting    = Formatting.Indented;

                if (m_nIndent > 0)
                {
                    xmlWriter.Indentation = m_nIndent;
                }

                WriteStartElement(xmlWriter, GeometryGml2.GmlBox, 
                    null, m_bIncludeSRID);
                
                if (!envelope.IsEmpty)
                {
                    CoordinateCollection coordindates = new CoordinateCollection(2);
                    coordindates.Add(envelope.Min);
                    coordindates.Add(envelope.Max);

                    Write(coordindates, xmlWriter);
                }
                
                WriteEndElement(xmlWriter);
            }
            catch (IOException ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="writer"></param>
        public void Write(Envelope envelope, XmlWriter writer)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException("envelope");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            try
            {
                m_bIsMeasured         = false;

                WriteStartElement(writer, GeometryGml2.GmlBox, null, m_bIncludeSRID);
                
                if (!envelope.IsEmpty)
                {
                    CoordinateCollection coordindates = new CoordinateCollection(2);
                    coordindates.Add(envelope.Min);
                    coordindates.Add(envelope.Max);

                    Write(coordindates, writer);
                }
                
                WriteEndElement(writer);
            }
            catch (IOException ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);

                throw ex;
            }
        }
        
        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="writer"></param>        
        private void Write(Coordinate coordinate, XmlWriter writer)
        {
            bool usePrefix     = false;
            string coordPrefix = null;
            if (m_bUsePrefix && (m_strPrefix != null && m_strPrefix.Length != 0))
            {
                usePrefix = true;

                coordPrefix = m_strPrefix + ":";
            }

            // if used to use the "coord" coordinates option or we have the
            // measured coordinate type...
            if (m_bUseCoord || m_bIsMeasured)
            {
                if (usePrefix)
                {
                    writer.WriteStartElement(m_strPrefix, GeometryGml2.GmlCoord, null);
                }
                else
                {
                    writer.WriteStartElement(GeometryGml2.GmlCoord);
                }

                // write the X and Y
                if (usePrefix)
                {
                    writer.WriteElementString(coordPrefix + GeometryGml2.GmlCoordX, 
                        WriteNumber(coordinate.X));
                    writer.WriteElementString(coordPrefix + GeometryGml2.GmlCoordY, 
                        WriteNumber(coordinate.Y));
                }
                else
                {
                    writer.WriteElementString(GeometryGml2.GmlCoordX, 
                        WriteNumber(coordinate.X));
                    writer.WriteElementString(GeometryGml2.GmlCoordY, 
                        WriteNumber(coordinate.Y));
                }

                // if available, write the Z
                if (coordinate.Dimension > 2)
                {
                    double dZ = coordinate.GetOrdinate(2);

                    if (usePrefix)
                    {
                        writer.WriteElementString(coordPrefix + GeometryGml2.GmlCoordZ, 
                            WriteNumber(dZ));
                    }
                    else
                    {
                        writer.WriteElementString(GeometryGml2.GmlCoordZ, 
                            WriteNumber(dZ));
                    }
                }

                // if available, add the M
                if (m_bIsMeasured)
                {
                    if (coordinate.Dimension > 2)
                    {
                        Coordinate3DM measured = (Coordinate3DM)(coordinate);

                        if (usePrefix)
                        {
                            writer.WriteElementString(coordPrefix + 
                                GeometryGml2.GmlCoordM, WriteNumber(measured.Measure));
                        }
                        else
                        {
                            writer.WriteElementString(GeometryGml2.GmlCoordM, 
                                WriteNumber(measured.Measure));
                        }
                    }
                    else
                    {
                        CoordinateM measured = (CoordinateM)(coordinate);

                        if (usePrefix)
                        {
                            writer.WriteElementString(coordPrefix + 
                                GeometryGml2.GmlCoordM, WriteNumber(measured.Measure));
                        }
                        else
                        {
                            writer.WriteElementString(GeometryGml2.GmlCoordM, 
                                WriteNumber(measured.Measure));
                        }
                    }
                }
                      
                writer.WriteEndElement();
            }
            else
            {
                if (usePrefix)
                {
                    writer.WriteStartElement(m_strPrefix, 
                        GeometryGml2.GmlCoordinates, null);
                }
                else
                {
                    writer.WriteStartElement(GeometryGml2.GmlCoordinates);
                }

                if (coordinate.Dimension > 2)
                {
                    double dZ = coordinate.GetOrdinate(2);

                    writer.WriteString(WriteNumber(coordinate.X) + 
                        GeometryGml2.GmlCoordinateSeparator 
                        + WriteNumber(coordinate.Y) + GeometryGml2.GmlCoordinateSeparator 
                        + WriteNumber(dZ));
                }
                else
                {
                    writer.WriteString(WriteNumber(coordinate.X) + 
                        GeometryGml2.GmlCoordinateSeparator 
                        + WriteNumber(coordinate.Y));
                }
                      
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="writer"></param>         
        private void Write(ICoordinateList coordinates, XmlWriter writer)
        {
            if (m_bUseCoord || m_bIsMeasured)
            {
                WriteCoords(coordinates, writer);
            }
            else
            {   
                WriteCoordinates(coordinates, writer);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="writer"></param>        
        private void WriteCoord(Coordinate coordinate, XmlWriter writer)
        {
            bool usePrefix     = false;
            string coordPrefix = null;
            if (m_bUsePrefix && (m_strPrefix != null && m_strPrefix.Length != 0))
            {
                usePrefix = true;

                coordPrefix = m_strPrefix + ":";
            }

            WriteCoord(coordinate, writer, usePrefix, coordPrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="writer"></param>
        /// <param name="usePrefix"></param>
        /// <param name="coordPrefix"></param>
        private void WriteCoord(Coordinate coordinate, XmlWriter writer,
            bool usePrefix, string coordPrefix)
        {
            if (usePrefix)
            {
                writer.WriteStartElement(m_strPrefix, GeometryGml2.GmlCoord, null);
            }
            else
            {
                writer.WriteStartElement(GeometryGml2.GmlCoord);
            }

            // write the X and Y
            if (usePrefix)
            {
                writer.WriteElementString(coordPrefix + GeometryGml2.GmlCoordX, 
                    WriteNumber(coordinate.X));
                writer.WriteElementString(coordPrefix + GeometryGml2.GmlCoordY, 
                    WriteNumber(coordinate.Y));
            }
            else
            {
                writer.WriteElementString(GeometryGml2.GmlCoordX, 
                    WriteNumber(coordinate.X));
                writer.WriteElementString(GeometryGml2.GmlCoordY, 
                    WriteNumber(coordinate.Y));
            }

            // if available, write the Z
            if (coordinate.Dimension > 2)
            {
                double dZ = coordinate.GetOrdinate(2);

                if (usePrefix)
                {
                    writer.WriteElementString(coordPrefix + GeometryGml2.GmlCoordZ, 
                        WriteNumber(dZ));
                }
                else
                {
                    writer.WriteElementString(GeometryGml2.GmlCoordZ, 
                        WriteNumber(dZ));
                }
            }

            // if available, add the M
            if (m_bIsMeasured)
            {
                if (coordinate.Dimension > 2)
                {
                    Coordinate3DM measured = (Coordinate3DM)(coordinate);

                    if (usePrefix)
                    {
                        writer.WriteElementString(coordPrefix + 
                            GeometryGml2.GmlCoordM, WriteNumber(measured.Measure));
                    }
                    else
                    {
                        writer.WriteElementString(GeometryGml2.GmlCoordM, 
                            WriteNumber(measured.Measure));
                    }
                }
                else
                {
                    CoordinateM measured = (CoordinateM)(coordinate);

                    if (usePrefix)
                    {
                        writer.WriteElementString(coordPrefix + 
                            GeometryGml2.GmlCoordM, WriteNumber(measured.Measure));
                    }
                    else
                    {
                        writer.WriteElementString(GeometryGml2.GmlCoordM, 
                            WriteNumber(measured.Measure));
                    }
                }
            }
                    
            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="writer"></param>         
        private void WriteCoords(ICoordinateList coordinates, XmlWriter writer)
        {
            bool usePrefix     = false;
            string coordPrefix = null;
            if (m_bUsePrefix && (m_strPrefix != null && m_strPrefix.Length != 0))
            {
                usePrefix = true;

                coordPrefix = m_strPrefix + ":";
            }

            int nCount = coordinates.Count;

            for (int i = 0; i < nCount; i++)
            {
                Coordinate coordinate = coordinates[i];

                if (coordinate == null)
                {
                    continue;
                }

                WriteCoord(coordinate, writer, usePrefix, coordPrefix);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="writer"></param>         
        private void WriteCoordinates(ICoordinateList coordinates, XmlWriter writer)
        {
            bool usePrefix = false;
            if (m_bUsePrefix && (m_strPrefix != null && m_strPrefix.Length != 0))
            {
                usePrefix = true;
            }

            if (usePrefix)
            {
                writer.WriteStartElement(m_strPrefix, 
                    GeometryGml2.GmlCoordinates, null);
            }
            else
            {
                writer.WriteStartElement(GeometryGml2.GmlCoordinates);
            }

            int nCount = coordinates.Count;
            if (nCount == 0)
            {
                writer.WriteEndElement();
                return;
            }

            int i = 0;
            Coordinate coordinate = coordinates[i];
            while (coordinate == null) 
            {
                i++;
                coordinate = coordinates[i];
            }
            int nDimension = coordinate.Dimension;

            StringBuilder builder = new StringBuilder();
            for (i = 0; i < nCount; i++)
            {
                coordinate = coordinates[i];

                if (coordinate == null)
                {
                    continue;
                }

                builder.Append(WriteNumber(coordinate.X));
                builder.Append(GeometryGml2.GmlCoordinateSeparator);
                builder.Append(WriteNumber(coordinate.Y));

                if (nDimension > 2)
                {
                    double dZ = coordinate.GetOrdinate(2);
                    builder.Append(GeometryGml2.GmlCoordinateSeparator);
                    builder.Append(WriteNumber(dZ));
                }

                if ((i + 1) < nCount)
                {
                    builder.Append(GeometryGml2.GmlTupleSeparator);
                }
            }

            string strTuples = builder.ToString();
            if (strTuples != null && strTuples.Length > 0)
            {
                writer.WriteString(strTuples);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="writer"></param>
        /// <param name="addSRID"></param>
        private void WriteGeometry(Geometry geometry, XmlWriter writer, bool addSRID)
        {
            GeometryType geomType = geometry.GeometryType;

            if (geomType == GeometryType.Point)
            {
                WritePoint((Point)geometry, writer, addSRID);
            }
            else if (geomType == GeometryType.Rectangle)
            {
                WriteRectangle((Rectangle)geometry, writer, addSRID);
            }
            else if (geomType == GeometryType.LinearRing)
            {
                WriteLinearRing((LinearRing)geometry, writer, addSRID);
            }
            else if (geomType == GeometryType.LineString)
            {
                WriteLineString((LineString)geometry, writer, addSRID);
            }
            else if (geomType == GeometryType.Polygon)
            {
                WritePolygon((Polygon)geometry, writer, addSRID);
            }
            else if (geomType == GeometryType.MultiPoint)
            {
                WriteMultiPoint((MultiPoint)geometry, writer, addSRID);
            }
            else if (geomType == GeometryType.MultiLineString)
            {
                WriteMultiLineString((MultiLineString)geometry, writer, addSRID);
            }
            else if (geomType == GeometryType.MultiPolygon)
            {
                WriteMultiPolygon((MultiPolygon)geometry, writer, addSRID);
            }
            else if (geomType == GeometryType.GeometryCollection)
            {
                WriteGeometryCollection((GeometryCollection)geometry, 
                    writer, addSRID);
            }
            else
            {
                throw new ArgumentException("Unsupported Geometry implementation:" 
                    + geometry.Name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="writer"></param>
        /// <param name="addSRID"></param>
        private void WritePoint(Point point, XmlWriter writer, bool addSRID)
        {          
            bool usePrefix = WriteStartElement(writer, GeometryGml2.GmlPoint,
                point, addSRID);
 
            Write(point.Coordinate, writer);

            WriteEndElement(writer);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="writer"></param>
        /// <param name="addSRID"></param>
        private void WriteRectangle(Rectangle rect, XmlWriter writer, bool addSRID)
        {          
            bool usePrefix = WriteStartElement(writer, GeometryGml2.GmlBox,
                rect, addSRID);
 
            Write(rect.Coordinates, writer);

            WriteEndElement(writer);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineString"></param>
        /// <param name="writer"></param>
        /// <param name="addSRID"></param>
        private void WriteLineString(LineString lineString, 
            XmlWriter writer, bool addSRID)
        {            
            bool usePrefix = WriteStartElement(writer, GeometryGml2.GmlLineString,
                lineString, addSRID);

            Write(lineString.Coordinates, writer); 
   
            WriteEndElement(writer);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="linearRing"></param>
        /// <param name="writer"></param>
        /// <param name="addSRID"></param>
        private void WriteLinearRing(LinearRing linearRing, XmlWriter writer, 
            bool addSRID)
        {
            bool usePrefix = WriteStartElement(writer, GeometryGml2.GmlLinearRing,
                linearRing, addSRID);

            Write(linearRing.Coordinates, writer);

            WriteEndElement(writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="writer"></param>
        /// <param name="addSRID"></param>
        private void WritePolygon(Polygon polygon, XmlWriter writer, bool addSRID)
        {
            // 1. Start the polygon...
            bool usePrefix = WriteStartElement(writer, GeometryGml2.GmlPolygon,
                polygon, addSRID);

            // 2. Write the outer ring
            if (usePrefix)
            {
                writer.WriteStartElement(m_strPrefix, 
                    GeometryGml2.GmlOuterBoundaryIs, null);
            }
            else
            {
                writer.WriteStartElement(GeometryGml2.GmlOuterBoundaryIs);
            }
            WriteLinearRing(polygon.ExteriorRing, writer, false);
            writer.WriteEndElement();

            // 3. Write the inner rings
            int nRings = polygon.NumInteriorRings;
            for (int i = 0; i < nRings; i++)
            {
                if (usePrefix)
                {
                    writer.WriteStartElement(m_strPrefix, 
                        GeometryGml2.GmlInnerBoundaryIs, null);
                }
                else
                {
                    writer.WriteStartElement(GeometryGml2.GmlInnerBoundaryIs);
                }
                WriteLinearRing(polygon.InteriorRings[i], writer, false);
                writer.WriteEndElement();
            }

            // 4. End the polygon
            WriteEndElement(writer);
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPoint"></param>
        /// <param name="writer"></param>
        /// <param name="addSRID"></param>
        private void WriteMultiPoint(MultiPoint multiPoint, XmlWriter writer, 
            bool addSRID)
        {              
            // 1. Start the multi-point
            bool usePrefix = WriteStartElement(writer, GeometryGml2.GmlMultiPoint,
                multiPoint, addSRID);

            // 2. Write the individual points
            int nPoints = multiPoint.NumGeometries;
            for (int i = 0; i < nPoints; i++)
            {
                if (usePrefix)
                {
                    writer.WriteStartElement(m_strPrefix, 
                        GeometryGml2.GmlPointMember, null);
                }
                else
                {
                    writer.WriteStartElement(GeometryGml2.GmlPointMember);
                }
                WritePoint(multiPoint[i], writer, false);
                writer.WriteEndElement();
            }

            // 3. End the multi-point
            WriteEndElement(writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiLineString"></param>
        /// <param name="writer"></param>
        /// <param name="addSRID"></param>
        private void WriteMultiLineString(MultiLineString multiLineString, 
            XmlWriter writer, bool addSRID)
        {
            // 1. Start the multi-linestring...
            bool usePrefix = WriteStartElement(writer, 
                GeometryGml2.GmlMultiLineString, multiLineString, addSRID);

            // 2. Write the individual linestrings...
            int nLines = multiLineString.NumGeometries;
            for (int i = 0; i < nLines; i++)
            {
                if (usePrefix)
                {
                    writer.WriteStartElement(m_strPrefix, 
                        GeometryGml2.GmlLineStringMember, null);
                }
                else
                {
                    writer.WriteStartElement(GeometryGml2.GmlLineStringMember);
                }
                WriteLineString(multiLineString[i], writer, false);
                writer.WriteEndElement();
            }

            // 3. End the multi-linestring
            WriteEndElement(writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPolygon"></param>
        /// <param name="writer"></param>
        /// <param name="addSRID"></param>
        private void WriteMultiPolygon(MultiPolygon multiPolygon, 
            XmlWriter writer, bool addSRID)
        {
            // 1. Start the multi-polygon...
            bool usePrefix = WriteStartElement(writer, GeometryGml2.GmlMultiPolygon,
                multiPolygon, addSRID);

            // 2. Write the individual polygons
            int nPolygons = multiPolygon.NumGeometries;
            for (int i = 0; i < nPolygons; i++)
            {
                if (usePrefix)
                {
                    writer.WriteStartElement(m_strPrefix, 
                        GeometryGml2.GmlPolygonMember, null);
                }
                else
                {
                    writer.WriteStartElement(GeometryGml2.GmlPolygonMember);
                }
                WritePolygon(multiPolygon[i], writer, false);
                writer.WriteEndElement();
            }

            // 3. End the multi-polygon
            WriteEndElement(writer);
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometryCollection"></param>
        /// <param name="writer"></param>
        /// <param name="addSRID"></param>
        private void WriteGeometryCollection(GeometryCollection geometryCollection, 
            XmlWriter writer, bool addSRID)
        {
            // 1. Start the multi-geometry...
            bool usePrefix = WriteStartElement(writer, GeometryGml2.GmlMultiGeometry,
                geometryCollection, addSRID);

            // 2. Write the individual geometries
            int nGeometries = geometryCollection.NumGeometries;
            for (int i = 0; i < nGeometries; i++)
            {
                if (usePrefix)
                {
                    writer.WriteStartElement(m_strPrefix, 
                        GeometryGml2.GmlGeometryMember, null);
                }
                else
                {
                    writer.WriteStartElement(GeometryGml2.GmlGeometryMember);
                }
                WriteGeometry(geometryCollection[i], writer, false);
                writer.WriteEndElement();
            }

            // 3. End the multi-geometry.
            WriteEndElement(writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="strGeometry"></param>
        /// <param name="geometry"></param>
        /// <param name="addSRID"></param>
        /// <returns></returns>
        private bool WriteStartElement(XmlWriter writer, string strGeometry, 
            Geometry geometry, bool addSRID)
        {
            bool usePrefix = false;
            if (m_bUsePrefix && (m_strPrefix != null && m_strPrefix.Length != 0))
            {
                usePrefix = true;
            }
            
            if (usePrefix)
            {
                writer.WriteStartElement(m_strPrefix, strGeometry, null);
            }
            else
            {
                writer.WriteStartElement(strGeometry);
            }

            IGeometryProperties properties = null;
            if (geometry != null)
            {
                properties = geometry.Properties;
            }
            if (properties != null && properties.Count > 0)
            {
                if (properties.Contains("GID"))
                {
                    string strGID = (string)properties["GID"];
                    if (strGID != null && strGID.Length > 0)
                    {
                        if (usePrefix)
                        {
                            writer.WriteAttributeString(m_strPrefix,
                                GeometryGml2.GmlGid, null, strGID);
                        }
                        else
                        {
                            writer.WriteAttributeString(GeometryGml2.GmlGid, strGID);
                        }
                    }
                }

                if (addSRID && properties.Contains("SRID"))
                {
                    int nSRID = (int)properties["SRID"];
                    if (nSRID > 0)
                    {
                        string strSRID = m_bUseEpsgSrs ? 
                            GeometryGml2.GmlAttrEpsgSrsname + nSRID.ToString() : 
                            nSRID.ToString();
                        if (usePrefix)
                        {
                            writer.WriteAttributeString(m_strPrefix,
                                GeometryGml2.GmlAttrSrsname, null, strSRID);
                        }
                        else
                        {
                            writer.WriteAttributeString(
                                GeometryGml2.GmlAttrSrsname, strSRID);
                        }
                    }
                }
            }

            return usePrefix;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        private void WriteEndElement(XmlWriter writer)
        {
            writer.WriteEndElement();
        }

        private void Reset()
        {
            m_bUseCoord   = false;
            m_bUsePrefix  = false;
            m_bUseEpsgSrs = true;
            m_strPrefix   = GeometryGml2.GmlPrefix;
        }
        
        #endregion
    }
}
