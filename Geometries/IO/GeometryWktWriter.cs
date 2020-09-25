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
using System.Globalization;

using iGeospatial.Texts;
using iGeospatial.Exceptions;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.IO
{
	/// <summary> 
	/// Outputs the textual representation of a <see cref="Geometry"/> in a 
	/// Well-Known Text format.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The GeometryWktWriter outputs coordinates rounded to the precision
	/// model. No more than the maximum number of necessary decimal places will be
	/// output.
	/// </para>
	/// <para>
	/// The Well-known Text format is defined in the 
	/// <see href="http://www.opengis.org/techno/specs.htm">OpenGIS Simple Features
	/// Specification for SQL</see>.
	/// </para>
	/// A non-standard "LINEARRING" tag is used for LinearRings. The WKT spec does
	/// not define a special tag for LinearRings. The standard tag to use is
	/// "LINESTRING".
	/// </remarks>
	public class GeometryWktWriter : GeometryTextWriter
	{
        #region Private Members

        private static readonly string WktEmpty  = "EMPTY";
        private static readonly string WktComma  = ", ";
        private static readonly string WktLParan = "(";
        private static readonly string WktRParan = ")";
		
        #endregion

        #region Constructors and Destructor
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryWktWriter"/> class.
        /// </summary>
        public GeometryWktWriter() : base()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryWktWriter"/> class.
        /// </summary>
        public GeometryWktWriter(PrecisionModel customPrecision)
            : base(customPrecision)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryWktWriter"/> class.
        /// </summary>
        public GeometryWktWriter(PrecisionModel customPrecision, int indent)
            : base(customPrecision, indent)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryWktWriter"/> class.
        /// </summary>
        public GeometryWktWriter(CultureInfo culture) : base(culture)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryWktWriter"/> class.
        /// </summary>
        public GeometryWktWriter(CultureInfo culture, int indent)
            : base(culture, indent)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryWktWriter"/> class.
        /// </summary>
        public GeometryWktWriter(CultureInfo culture, 
            PrecisionModel customPrecision) : base(culture, customPrecision)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryWktWriter"/> class.
        /// </summary>
        public GeometryWktWriter(CultureInfo culture, 
            PrecisionModel customPrecision, int indent) 
            : base(culture, customPrecision, indent)
        {
        }

        #endregion
		
        #region Public Methods

		/// <summary>  
		/// Converts a Geometry to its well-known text representation.
		/// </summary>
		/// <param name="geometry"> a Geometry to process
		/// </param>
		/// <returns>           a "Geometry Tagged Text" string (see the OpenGIS Simple
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

                UpdateFormatter(precision);

                StringWriter sw = new StringWriter(m_objProvider);

                WriteFormatted(geometry, false, sw);

                return sw.ToString();
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
		/// <returns>           a "Geometry Tagged Text" string (see the OpenGIS Simple
		/// Features Specification)
		/// </returns>
		public override void Write(Geometry geometry, TextWriter writer)
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

                WriteFormatted(geometry, false, writer);
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

                StringWriter sw = new StringWriter(m_objProvider);
				WriteFormatted(geometry, true, sw);

                return sw.ToString();
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

                WriteFormatted(geometry, true, writer);
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

                WriteFormatted(geometry, true, writer);
            
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
        
        #endregion

        #region Public Static Methods

        /// <summary>
        /// Generates the WKT for a <see cref="Point"/>.
        /// </summary>
        /// <param name="p0">The point coordinate.</param>
        /// <returns>
        /// A string containing the WKT of the Point.
        /// </returns>
        public static String ToPoint(Coordinate p0)
        {
            return "POINT ( " + p0.X + " " + p0.Y  + " )";
        }

        /// <summary>
        /// Generates the WKT for a N-point <see cref="LineString"/>.
        /// </summary>
        /// <param name="seq">The sequence to outpout</param>
        /// <returns>
        /// A string containing the WKT of the LineString.
        /// </returns>
        public static string ToLineString(ICoordinateList seq)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("LINESTRING ");

            if (seq == null || seq.Count == 0)
            {
                buf.Append(" EMPTY");
            }
            else 
            {
                buf.Append(WktLParan);

                int nCount = seq.Count;

                for (int i = 0; i < nCount; i++) 
                {
                    if (i > 0)
                    {
                        buf.Append(WktComma);
                    }

                    Coordinate coord = seq[i];

                    buf.Append(coord.X + " " + coord.Y);
                }

                buf.Append(WktRParan);
            }

            return buf.ToString();
        }

        /// <summary>
        /// Generates the WKT for a 2-point <see cref="LineString"/>.
        /// </summary>
        /// <param name="p0">The first coordinate</param>
        /// <param name="p1">The second coordinate</param>
        /// <returns>
        /// A string containing the WKT of the LineString.
        /// </returns>
        public static string ToLineString(Coordinate p0, Coordinate p1)
        {
            return "LINESTRING ( " + p0.X + " " + p0.Y + WktComma + 
                p1.X + " " + p1.Y + " )";
        }
        
        #endregion

        #region Private Methods

		/// <summary>  
		/// Converts a Geometry to its well-known text representation.
		/// </summary>
		/// <param name="geometry"> a Geometry to process
		/// </param>
		/// <returns>           
		/// A &lt;Geometry Tagged Text&gt; string (see the OpenGIS Simple
		/// Features Specification)
		/// </returns>
		private void WriteFormatted(Geometry geometry, bool isFormatted, 
            TextWriter writer)
		{
			m_bIsFormatted = isFormatted;

            if (m_bIncludeSRID)
            {
                // try writing..."SRID=4326;GEOMETRY(...)"
                IGeometryProperties properties = geometry.Properties;
                if (properties != null && properties.Count > 0 
                    && properties.Contains("SRID"))
                {
                    int nSRID = (int)properties["SRID"];
                    if (nSRID > 0)
                    {
                        writer.Write("SRID=");
                        writer.Write(nSRID.ToString());
                        writer.Write(";");
                    }
                }
            }

            WriteGeometry(geometry, 0, writer);
		}
		
		/// <summary>  
		/// Converts a Geometry to &lt;Geometry Tagged Text&gt; format,
		/// then appends it to the writer.
		/// </summary>
		/// <param name="geometry"> the Geometry to process
		/// </param>
		/// <param name="writer">   the output writer to append to
		/// </param>
		private void WriteGeometry(Geometry geometry, int level, TextWriter writer)
		{
			Indent(level, writer);
			
            GeometryType geomType = geometry.GeometryType;

            if (geomType == GeometryType.Point)
			{
				Point point = (Point)geometry;
				WritePoint(point.Coordinate, level, writer);
			}
			else if (geomType == GeometryType.LinearRing)
			{
				WriteLinearRing((LinearRing)geometry, level, writer);
			}
			else if (geomType == GeometryType.LineString)
			{
				WriteLineString((LineString)geometry, level, writer);
			}
			else if (geomType == GeometryType.Polygon)
			{
				WritePolygon((Polygon)geometry, level, writer);
			}
			else if (geomType == GeometryType.MultiPoint)
			{
				WriteMultiPoint((MultiPoint)geometry, level, writer);
			}
			else if (geomType == GeometryType.MultiLineString)
			{
				WriteMultiLineString((MultiLineString)geometry, level, writer);
			}
			else if (geomType == GeometryType.MultiPolygon)
			{
				WriteMultiPolygon((MultiPolygon)geometry, level, writer);
			}
			else if (geomType == GeometryType.GeometryCollection)
			{
				WriteGeometryCollection((GeometryCollection)geometry, 
                    level, writer);
			}
            else
            {
                throw new ArgumentException("Unsupported Geometry implementation:" 
                    + geometry.Name);
            }
		}
		
		/// <summary>  
		/// Converts a Coordinate to &lt;Point Tagged Text&gt; format,
		/// then appends it to the writer.
		/// 
		/// </summary>
		/// <param name="coordinate">     the Coordinate to process
		/// </param>
		/// <param name="writer">         the output writer to append to
		/// </param>
		private void WritePoint(Coordinate coordinate, int level, 
            TextWriter writer)
		{
            if (m_bIsMeasured)
            {
                writer.Write("POINTM ");
            }
            else
            {
                writer.Write("POINT ");
            }
			AppendPointText(coordinate, level, writer);
		}
		
		/// <summary>  Converts a LineString to &lt;LineString Tagged Text&gt;
		/// format, then appends it to the writer.
		/// 
		/// </summary>
		/// <param name="lineString"> the LineString to process
		/// </param>
		/// <param name="writer">     the output writer to append to
		/// </param>
		private void WriteLineString(LineString lineString, 
            int level, TextWriter writer)
		{
            if (m_bIsMeasured)
            {
                writer.Write("LINESTRINGM ");
            }
            else
            {
                writer.Write("LINESTRING ");
            }
			AppendLineStringText(lineString, level, false, writer);
		}
		
		/// <summary>  Converts a LinearRing to &lt;LinearRing Tagged Text&gt;
		/// format, then appends it to the writer.
		/// 
		/// </summary>
		/// <param name="linearRing"> the LinearRing to process
		/// </param>
		/// <param name="writer">     the output writer to append to
		/// </param>
		private void WriteLinearRing(LinearRing linearRing, 
            int level, TextWriter writer)
		{
            if (m_bIsMeasured)
            {
                writer.Write("LINEARRINGM ");
            }
            else
            {
                writer.Write("LINEARRING ");
            }
			AppendLineStringText(linearRing, level, false, writer);
		}
		
		/// <summary>  Converts a Polygon to &lt;Polygon Tagged Text&gt; format,
		/// then appends it to the writer.
		/// 
		/// </summary>
		/// <param name="polygon"> the Polygon to process
		/// </param>
		/// <param name="writer">  the output writer to append to
		/// </param>
		private void WritePolygon(Polygon polygon, int level, 
            TextWriter writer)
		{
            if (m_bIsMeasured)
            {
                writer.Write("POLYGONM ");
            }
            else
            {
                writer.Write("POLYGON ");
            }
			AppendPolygonText(polygon, level, false, writer);
		}
		
		/// <summary>  
		/// Converts a MultiPoint to &lt;MultiPoint Tagged Text&gt; format, then 
		/// appends it to the writer.
		/// </summary>
		/// <param name="multipoint"> the MultiPoint to process
		/// </param>
		/// <param name="writer">     the output writer to append to
		/// </param>
		private void WriteMultiPoint(MultiPoint multipoint, 
            int level, TextWriter writer)
		{
            if (m_bIsMeasured)
            {
                writer.Write("MULTIPOINTM ");
            }
            else
            {
                writer.Write("MULTIPOINT ");
            }
			AppendMultiPointText(multipoint, level, writer);
		}
		
		/// <summary>  
		/// Converts a MultiLineString to &lt;MultiLineString Tagged Text&gt; 
		/// format, then appends it to the writer.
		/// </summary>
		/// <param name="multiLineString"> the MultiLineString to process
		/// </param>
		/// <param name="writer">          the output writer to append to
		/// </param>
		private void WriteMultiLineString(
            MultiLineString multiLineString, int level, TextWriter writer)
		{
            if (m_bIsMeasured)
            {
                writer.Write("MULTILINESTRINGM ");
            }
            else
            {
                writer.Write("MULTILINESTRING ");
            }
			AppendMultiLineStringText(multiLineString, level, false, writer);
		}
		
		/// <summary>  
		/// Converts a MultiPolygon to &lt;MultiPolygon Tagged Text&gt;
		/// format, then appends it to the writer.
		/// </summary>
		/// <param name="multiPolygon"> the MultiPolygon to process
		/// </param>
		/// <param name="writer">       the output writer to append to
		/// </param>
		private void WriteMultiPolygon(MultiPolygon multiPolygon, 
            int level, TextWriter writer)
		{
            if (m_bIsMeasured)
            {
                writer.Write("MULTIPOLYGONM ");
            }
            else
            {
                writer.Write("MULTIPOLYGON ");
            }
			AppendMultiPolygonText(multiPolygon, level, writer);
		}
		
		/// <summary>  
		/// Converts a GeometryCollection to &lt;GeometryCollection
		/// Tagged Text&gt; format, then appends it to the writer.
		/// </summary>
		/// <param name="geometryCollection"> the GeometryCollection to process
		/// </param>
		/// <param name="writer">             the output writer to append to
		/// </param>
		private void WriteGeometryCollection(
            GeometryCollection geometryCollection, int level, TextWriter writer)
		{
            if (m_bIsMeasured)
            {
                writer.Write("GEOMETRYCOLLECTIONM ");
            }
            else
            {
                writer.Write("GEOMETRYCOLLECTION ");
            }
			AppendGeometryCollectionText(geometryCollection, level, writer);
		}
		
		/// <summary>  
		/// Converts a Coordinate to &lt;Point Text&gt; format, then appends it 
		/// to the writer.
		/// </summary>
		/// <param name="coordinate">     the Coordinate to process
		/// </param>
		/// <param name="writer">         the output writer to append to
		/// </param>
		private void AppendPointText(Coordinate coordinate, int level, 
            TextWriter writer)
		{
			if (coordinate == null)
			{
				writer.Write(WktEmpty);
			}
			else
			{
				writer.Write(WktLParan);
				AppendCoordinate(coordinate, writer);
				writer.Write(WktRParan);
			}
		}
		
		/// <summary>  
		/// Converts a Coordinate to &lt;Point&gt; format, then appends
		/// it to the writer.
		/// </summary>
		/// <param name="coordinate">The Coordinate to process.</param>
		/// <param name="writer">The output writer to append to.</param>
		private void AppendCoordinate(Coordinate coordinate, TextWriter writer)
		{
            if (m_bIsMeasured)
            {
                if (coordinate.Dimension > 2)
                {
                    Coordinate3DM measured = (Coordinate3DM)(coordinate);

                    writer.Write(WriteNumber(measured.X) + " " 
                        + WriteNumber(measured.Y) + " " 
                        + WriteNumber(measured.Z) + " " 
                        + WriteNumber(measured.Measure));
                }
                else
                {
                    CoordinateM measured = (CoordinateM)(coordinate);

                    writer.Write(WriteNumber(measured.X) + " " 
                        + WriteNumber(measured.Y) + " " 
                        + WriteNumber(measured.Measure));
                }
            }
            else
            {
                if (coordinate.Dimension > 2)
                {
                    double dZ = coordinate.GetOrdinate(2);

                    writer.Write(WriteNumber(coordinate.X) + " " 
                        + WriteNumber(coordinate.Y) + " " 
                        + WriteNumber(dZ));
                }
                else
                {
                    writer.Write(WriteNumber(coordinate.X) + " " 
                        + WriteNumber(coordinate.Y));
                }
            }
		}
		
		/// <summary>  
		/// Converts a LineString to &lt;LineString Text&gt; format, then
		/// appends it to the writer.
		/// </summary>
		/// <param name="lineString"> the LineString to process
		/// </param>
		/// <param name="writer">     the output writer to append to
		/// </param>
		private void AppendLineStringText(LineString lineString, 
            int level, bool doIndent, TextWriter writer)
		{
			if (lineString.IsEmpty)
			{
				writer.Write(WktEmpty);
			}
			else
			{
				if (doIndent)
                {
                    Indent(level, writer);
                }

				writer.Write(WktLParan);
				for (int i = 0; i < lineString.NumPoints; i++)
				{
					if (i > 0)
					{
						writer.Write(WktComma);
						if (i % 10 == 0)
                        {
                            Indent(level + 2, writer);
                        }
					}
					AppendCoordinate(lineString.GetCoordinate(i), writer);
				}
				writer.Write(WktRParan);
			}
		}
		
		/// <summary>  
		/// Converts a Polygon to &lt;Polygon Text&gt; format, then
		/// appends it to the writer.
		/// </summary>
		/// <param name="polygon"> the Polygon to process
		/// </param>
		/// <param name="writer">  the output writer to append to
		/// </param>
		private void AppendPolygonText(Polygon polygon, int level, 
            bool indentFirst, TextWriter writer)
		{
			if (polygon.IsEmpty)
			{
				writer.Write(WktEmpty);
			}
			else
			{
				if (indentFirst)
                {
                    Indent(level, writer);
                }

				writer.Write(WktLParan);
				AppendLineStringText(polygon.ExteriorRing, level, false, writer);
				for (int i = 0; i < polygon.NumInteriorRings; i++)
				{
					writer.Write(WktComma);
					AppendLineStringText(polygon.InteriorRing(i), 
                        level + 1, true, writer);
				}
				writer.Write(WktRParan);
			}
		}
		
		/// <summary>  Converts a MultiPoint to &lt;MultiPoint Text&gt; format, then
		/// appends it to the writer.
		/// 
		/// </summary>
		/// <param name="multiPoint"> the MultiPoint to process
		/// </param>
		/// <param name="writer">     the output writer to append to
		/// </param>
		private void AppendMultiPointText(MultiPoint multiPoint, 
            int level, TextWriter writer)
		{
			if (multiPoint.IsEmpty)
			{
				writer.Write(WktEmpty);
			}
			else
			{
				writer.Write(WktLParan);

                int nCount = multiPoint.NumGeometries;
				for (int i = 0; i < nCount; i++)
				{
					if (i > 0)
					{
						writer.Write(WktComma);
					}
					AppendCoordinate(((Point)multiPoint.GetGeometry(i)).Coordinate, 
                        writer);
				}
				writer.Write(WktRParan);
			}
		}
		
		/// <summary>  Converts a MultiLineString to &lt;MultiLineString Text&gt;
		/// format, then appends it to the writer.
		/// 
		/// </summary>
		/// <param name="multiLineString"> the MultiLineString to process
		/// </param>
		/// <param name="writer">          the output writer to append to
		/// </param>
		private void AppendMultiLineStringText(MultiLineString multiLineString, 
            int level, bool indentFirst, TextWriter writer)
		{
			if (multiLineString.IsEmpty)
			{
				writer.Write(WktEmpty);
			}
			else
			{
				int level2 = level;
				bool doIndent = indentFirst;
				writer.Write(WktLParan);

                int nCount = multiLineString.NumGeometries;
				for (int i = 0; i < nCount; i++)
				{
					if (i > 0)
					{
						writer.Write(WktComma);
						level2 = level + 1;
						doIndent = true;
					}
					AppendLineStringText((LineString)multiLineString.GetGeometry(i), 
                        level2, doIndent, writer);
				}
				writer.Write(WktRParan);
			}
		}
		
		/// <summary>  Converts a MultiPolygon to &lt;MultiPolygon Text&gt; format,
		/// then appends it to the writer.
		/// 
		/// </summary>
		/// <param name="multiPolygon"> the MultiPolygon to process
		/// </param>
		/// <param name="writer">       the output writer to append to
		/// </param>
		private void AppendMultiPolygonText(MultiPolygon multiPolygon, 
            int level, TextWriter writer)
		{
			if (multiPolygon.IsEmpty)
			{
				writer.Write(WktEmpty);
			}
			else
			{
				int level2    = level;
				bool doIndent = false;
				writer.Write(WktLParan);

                int nCount = multiPolygon.NumGeometries;
				for (int i = 0; i < nCount; i++)
				{
					if (i > 0)
					{
						writer.Write(WktComma);
						level2 = level + 1;
						doIndent = true;
					}

					AppendPolygonText((Polygon) multiPolygon.GetGeometry(i), 
                        level2, doIndent, writer);
				}
				writer.Write(WktRParan);
			}
		}
		
		/// <summary>
		/// Converts a GeometryCollection to &lt;GeometryCollectionText&gt;
		/// format, then appends it to the writer.
		/// </summary>
		/// <param name="geometryCollection">
		/// The GeometryCollection to process.
		/// </param>
		/// <param name="writer">The output writer to append to.</param>
		private void AppendGeometryCollectionText(
            GeometryCollection geometryCollection, int level, TextWriter writer)
		{
			if (geometryCollection.IsEmpty)
			{
				writer.Write(WktEmpty);
			}
			else
			{
				int level2 = level;
				writer.Write(WktLParan);

                int nCount = geometryCollection.NumGeometries;
				for (int i = 0; i < nCount; i++)
				{
					if (i > 0)
					{
						writer.Write(WktComma);
						level2 = level + 1;
					}

					WriteGeometry(geometryCollection.GetGeometry(i), 
                        level2, writer);
				}
				writer.Write(WktRParan);
			}
		}
        
        #endregion
	}
}