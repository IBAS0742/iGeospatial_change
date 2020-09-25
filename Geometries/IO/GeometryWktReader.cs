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
using System.Diagnostics;
using System.Collections;
using System.Globalization;

using iGeospatial.Texts;
using iGeospatial.Exceptions;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.IO
{
	/// <summary>  
	/// This text reader converts a Well-Known Text (WKT) string to a Geometry.
	/// </summary>
	/// <remarks>
	/// The GeometryWktReader allows extracting Geometry objects from either input streams or
	/// internal strings. This allows it to function as a parser to read Geometry
	/// objects from text blocks embedded in other data formats (e.g. XML).
	/// <para>
	/// The Well-known Text format is defined in the 
	/// <see href="http://www.opengis.org/techno/specs.htm"> OpenGIS Simple Features 
	/// Specification (SFS) for SQL</see>.
	/// </para>
	/// <para>
	/// A GeometryWktReader is parameterized by a GeometryFactory, to allow it to create 
	/// Geometry objects of the appropriate implementation. In particular, the 
	/// GeometryFactory will determine the PrecisionModel and SRID that is used.
	/// </para>
    ///
    /// The <c>GeometryWktReader</c> converts all input numbers to the precise
    /// internal representation.
    /// <para><b>Notes:</b></para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// The reader supports non-standard "LINEARRING" tags.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The reader uses Double.Parse to perform the conversion of ASCII
    /// numbers to floating point.  This means it supports the .NET
    /// syntax for floating point literals (including scientific notation).
    /// </description>
    /// </item>
    /// </list>
    ///
    /// <para><b>Syntax</b></para>
    /// The following syntax specification describes the version of Well-Known Text
    /// supported by this geometric library. 
    /// <code>
    /// WKTGeometry: one of
    ///
    ///       WKTPoint  WKTLineString  WKTLinearRing  WKTPolygon
    ///       WKTMultiPoint  WKTMultiLineString  WKTMultiPolygon
    ///       WKTGeometryCollection
    ///
    /// WKTPoint: <b>POINT ( </b>Coordinate <b>)</b>
    ///
    /// WKTLineString: <b>LINESTRING</b> CoordinateSequence
    ///
    /// WKTLinearRing: <b>LINEARRING</b> CoordinateSequence
    ///
    /// WKTPolygon: <b>POLYGON</b> CoordinateSequenceList
    ///
    /// WKTMultiPoint: <b>MULTIPOINT</b> CoordinateSequence
    ///
    /// WKTMultiLineString: 
    ///   <b>MULTILINESTRING</b> CoordinateSequenceList
    ///
    /// WKTMultiPolygon:
    ///   <b>MULTIPOLYGON (</b> CoordinateSequenceList { , CoordinateSequenceList } <b>)</b>
    ///
    /// WKTGeometryCollection: 
    ///   <b>GEOMETRYCOLLECTION (</b> WKTGeometry { , WKTGeometry } <b>)</b>
    ///
    /// CoordinateSequenceList:
    ///   <b>(</b> CoordinateSequence { <b>,</b> CoordinateSequence } <b>)</b>
    ///
    /// CoordinateSequence:
    ///   <b>(</b> Coordinate { , Coordinate } <b>)</b>
    ///
    /// Coordinate:
    ///   Number Number Number<sub>opt</sub>
    ///
    /// Number: A .NET-style floating-point number
    ///
    /// </code>
	/// The GeometryWktReader will convert the input numbers to the 
	/// precise internal representation.
	/// </remarks>
	public class GeometryWktReader : GeometryTextReader
	{
        #region Private Members

        private static readonly string WktEmpty    = "EMPTY";
        private static readonly string TokenComma  = ",";
        private static readonly string TokenLParan = "(";
        private static readonly string TokenRParan = ")";
        private static readonly string TokenEquals = "=";
        private static readonly string TokenSemi   = "=";

        private StreamTokenizer  m_objTokenizer;
		
        #endregion

        #region Constructors and Destructor

		/// <summary> 
		/// Creates a GeometryWktReader that creates objects using a basic GeometryFactory.
		/// </summary>
		public GeometryWktReader() : base()
		{
		}
		
		/// <summary>  
		/// Creates a GeometryWktReader that creates objects using the given GeometryFactory.
		/// </summary>
		/// <param name="geometryFactory"> 
		/// The factory used to create Geometrys.
		/// </param>
		public GeometryWktReader(GeometryFactory geometryFactory)
            : base(geometryFactory)
		{
		}
		
        #endregion

        #region Public Methods

		/// <summary> 
		/// Converts a Well-known Text representation to a Geometry.
		/// </summary>
		/// <param name="wellKnownText">
		/// one or more "Geometry Tagged Text" strings (see the OpenGIS
		/// Simple Features Specification) separated by whitespace
		/// </param>
		/// <returns> a Geometry specified by wellKnownText
		/// </returns>
        /// <exception cref="GeometryIOException">
        /// If a parsing problem occurs.
        /// </exception>
		public override Geometry Read(string wellKnownText)
		{
            if (m_objTokenizer == null)
            {
                m_objTokenizer = new StreamTokenizer(wellKnownText);
            }
            else
            {
                m_objTokenizer.Initialize(wellKnownText);
            }

            try
            {
                return ReadGeometry(m_objTokenizer);
            }
            catch (BaseException ex)
            {
                ExceptionManager.Publish(ex);

                throw new GeometryIOException(ex.ToString(), ex);
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);

                throw new GeometryIOException(ex.ToString(), ex);
            }
		}
		
		/// <summary>  
		/// Converts a Well-known Text representation to a Geometry.
		/// </summary>
		/// <param name="reader">A reader, which will return a "Geometry Tagged Text"
		/// string (see the OpenGIS Simple Features Specification)
		/// </param>
		/// <returns> A Geometry read from reader. </returns>  
        /// <exception cref="GeometryIOException">
        /// If a parsing problem occurs.
        /// </exception>
		public override Geometry Read(TextReader reader)
		{
            if (m_objTokenizer == null)
            {
                m_objTokenizer = new StreamTokenizer(reader);
            }
            else
            {
                m_objTokenizer.Initialize(reader);
            }

			try
			{
				return ReadGeometry(m_objTokenizer);
			}
			catch (IOException ex)
			{
                ExceptionManager.Publish(ex);

                throw new GeometryIOException(ex.ToString(), ex);
			}
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);

                throw new GeometryIOException(ex.ToString(), ex);
            }
        }
        
        #endregion
		
        #region Private Methods

		/// <summary>  
		/// Creates and returns the next array of coordinates in the stream.
		/// </summary>
		/// <param name="tokenizer"> 
		/// Tokenizer over a stream of text in Well-known Text
		/// format. The next element returned by the stream should be "(" (the
		/// beginning of "(x1 y1, x2 y2, ..., xn yn)") or "EMPTY".
		/// </param>
		/// <returns> The next array of Coordinates in the stream, or an empty 
		/// array if "EMPTY" is the next element returned by the stream.
		/// </returns>
		/// <exception cref="IOException">
		/// If an I/O error occurs.
		/// </exception>
		/// <exception cref="GeometryIOException">
		/// If an unexpected token was encountered.
		/// </exception>
		private ICoordinateList GetCoordinates(StreamTokenizer tokenizer)
		{
			string nextToken = GetNextEmptyOrOpener(tokenizer);
			if (nextToken.Equals(WktEmpty))
			{
				return new CoordinateCollection();
			}

            CoordinateCollection coordinates = new CoordinateCollection();

            if (m_bApplyPrecision)
            {
                coordinates.Add(GetPreciseCoordinate(tokenizer));
            }
            else
            {
                coordinates.Add(GetCoordinate(tokenizer));
            }

			nextToken = GetNextCloserOrComma(tokenizer);

            if (m_bApplyPrecision)
            {
                while (nextToken.Equals(TokenComma))
                {
                    coordinates.Add(GetPreciseCoordinate(tokenizer));
                    nextToken = GetNextCloserOrComma(tokenizer);
                }
            }
            else
            {
                while (nextToken.Equals(TokenComma))
                {
                    coordinates.Add(GetCoordinate(tokenizer));
                    nextToken = GetNextCloserOrComma(tokenizer);
                }
            }

			return coordinates;
		}
		
		private Coordinate GetCoordinate(StreamTokenizer tokenizer)
		{
			Coordinate coord = null;
			double dX = GetNextNumber(tokenizer);
			double dY = GetNextNumber(tokenizer);

            if (m_bMeasured)
            {
                double dT = GetNextNumber(tokenizer);
                if (IsNumberNext(tokenizer))
                {                      
                    double dM = GetNextNumber(tokenizer);
                    coord  = new Coordinate3DM(dX, dY, dT, dM);
                }  
                else
                {
                    coord = new CoordinateM(dX, dY, dT);
                }
            }
            else
            {
                if (IsNumberNext(tokenizer))
                {
                    double dZ = GetNextNumber(tokenizer);
                    coord     = new Coordinate3D(dX, dY, dZ);
                }  
                else
                {
                    coord = new Coordinate(dX, dY);
                }
            }

			return coord;
		}
		
        private Coordinate GetPreciseCoordinate(StreamTokenizer tokenizer)
        {
            Coordinate coord = null;
            double dX = GetNextNumber(tokenizer);
            double dY = GetNextNumber(tokenizer);

            if (m_bMeasured)
            {
                double dT = GetNextNumber(tokenizer);
                if (IsNumberNext(tokenizer))
                {
                    double dM = GetNextNumber(tokenizer);
                    coord     = new Coordinate3DM(dX, dY, dT, dM);
                }  
                else
                {
                    coord = new CoordinateM(dX, dY, dT);
                }
            }
            else
            {
                if (IsNumberNext(tokenizer))
                {
                    double dZ = GetNextNumber(tokenizer);
                    coord     = new Coordinate3D(dX, dY, dZ);
                }  
                else
                {
                    coord = new Coordinate(dX, dY);
                }
            }

            if (m_objPrecision != null)
            {
                coord.MakePrecise(m_objPrecision);
            }

            return coord;
        }

		private bool IsNumberNext(StreamTokenizer tokenizer)
		{
			try
			{
                Token token = null;
                if (!tokenizer.NextToken(out token))
                {
                    return false;
                }

                return (token.Type == TokenType.Float || 
                    token.Type == TokenType.Integer);
            }
			finally
			{
				tokenizer.PushBack();
			}
		}

		/// <summary>  
		/// Returns the next number in the stream.
		/// </summary>
		/// <param name="tokenizer"> 
		/// Tokenizer over a stream of text in Well-known Text format. 
		/// The next token must be a number.
		/// </param>
		/// <returns>The next number in the stream.</returns>
		/// <exception cref="GeometryIOException">
		/// If the next token is not a number.
		/// </exception>
		/// <exception cref="IOException">
		/// If an I/O error occurs.
		/// </exception>
		private double GetNextNumber(StreamTokenizer tokenizer)
		{
            Token token = null;
            if (!tokenizer.NextToken(out token))
            {
                return double.NaN;
            }

            TokenType type = token.Type;

            switch (type)
			{
				case TokenType.Eof: 
					throw new GeometryIOException("Expected number but encountered end of stream");
				
				case TokenType.Eol: 
					throw new GeometryIOException("Expected number but encountered end of line");
				
                case TokenType.Float: 
                    return Convert.ToDouble(token.Object, m_objProvider);

                case TokenType.Integer: 
                    return Convert.ToDouble(token.Object, m_objProvider);
				
				case TokenType.Word: 
					throw new GeometryIOException("Expected number but encountered word: " + token.StringValue);
				
				default:
				{
					string sVal = token.StringValue;
					if (sVal == TokenLParan)
						throw new GeometryIOException("Expected number but encountered '('");
				
					if (sVal == TokenRParan) 
						throw new GeometryIOException("Expected number but encountered ')'");
				
					if (sVal == TokenComma)
						throw new GeometryIOException("Expected number but encountered ','");
				}
				break;
			}

			Debug.Assert(false, "Should never reach here: Encountered unexpected StreamTokenizer type: " + type);
			return 0;
		}
		
		/// <summary>  
		/// Returns the next "EMPTY" or "(" in the stream as uppercase text.
		/// </summary>
		/// <param name="tokenizer">
		/// Tokenizer over a stream of text in Well-known Text
		/// format. The next token must be "EMPTY" or "(".
		/// </param>
		/// <returns>
		/// The next "EMPTY" or "(" in the stream as uppercase text.
		/// </returns>
		/// <exception cref="GeometryIOException">
		/// If the next token is not "EMPTY" or "(".
		/// </exception>
		/// <exception cref="IOException">
		/// If an I/O error occurs.
		/// </exception>
		private string GetNextEmptyOrOpener(StreamTokenizer tokenizer)
		{
			string nextWord = GetNextWord(tokenizer);
			if (nextWord.Equals(WktEmpty) || nextWord.Equals(TokenLParan))
			{
				return nextWord;
			}
			throw new GeometryIOException("Expected 'EMPTY' or '(' but encountered '" + nextWord + "'");
		}
		
		/// <summary>  
		/// Returns the next ")" or "," in the stream.
		/// </summary>
		/// <param name="tokenizer">
		/// Tokenizer over a stream of text in Well-known Text
		/// format. The next token must be ")" or ",".
		/// </param>
		/// <returns>The next ")" or "," in the stream.</returns>
		/// <exception cref="GeometryIOException">
		/// If the next token is not ")" or ",".
		/// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
		private string GetNextCloserOrComma(StreamTokenizer tokenizer)
		{
			string nextWord = GetNextWord(tokenizer);
			if (nextWord.Equals(TokenComma) || nextWord.Equals(TokenRParan))
			{
				return nextWord;
			}

			throw new GeometryIOException("Expected ')' or ',' but encountered '" + nextWord + "'");
		}
		
		/// <summary>  
		/// Returns the next ")" in the stream.
		/// </summary>
		/// <param name="tokenizer">
		/// Tokenizer over a stream of text in Well-known Text
		/// format. The next token must be ")".
		/// </param>
		/// <returns>The next ")" in the stream.</returns>
        /// <exception cref="GeometryIOException">
        /// If the next token is not ")".
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
		private string GetNextCloser(StreamTokenizer tokenizer)
		{
			string nextWord = GetNextWord(tokenizer);
			if (nextWord.Equals(TokenRParan))
			{
				return nextWord;
			}
			throw new GeometryIOException("Expected ')' but encountered '" + nextWord + "'");
		}
		
		/// <summary>  
		/// Returns the next "=" in the stream.
		/// </summary>
		/// <param name="tokenizer">
		/// Tokenizer over a stream of text in Well-known Text
		/// format. The next token must be "=".
		/// </param>
		/// <returns>The next "=" in the stream.</returns>
        /// <exception cref="GeometryIOException">
        /// If the next token is not "=".
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
		private string GetNextEquals(StreamTokenizer tokenizer)
		{
			string nextWord = GetNextWord(tokenizer);
			if (nextWord.Equals(TokenEquals))
			{
				return nextWord;
			}
			throw new GeometryIOException("Expected '=' but encountered '" + nextWord + "'");
		}
		
		/// <summary>  
		/// Returns the next ";" in the stream.
		/// </summary>
		/// <param name="tokenizer">
		/// Tokenizer over a stream of text in Well-known Text
		/// format. The next token must be ";".
		/// </param>
		/// <returns>The next ";" in the stream.</returns>
        /// <exception cref="GeometryIOException">
        /// If the next token is not ";".
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
		private string GetNextSemi(StreamTokenizer tokenizer)
		{
			string nextWord = GetNextWord(tokenizer);
			if (nextWord.Equals(TokenSemi))
			{
				return nextWord;
			}
			throw new GeometryIOException("Expected ';' but encountered '" + nextWord + "'");
		}
		
		/// <summary>  
		/// Returns the next word in the stream as uppercase text.
		/// </summary>
		/// <param name="tokenizer"> 
		/// Tokenizer over a stream of text in Well-known Text
		/// format. The next token must be a word.
		/// </param>
		/// <returns>
		/// The next word in the stream as uppercase text.
		/// </returns>
        /// <exception cref="GeometryIOException">
        /// If the next token is not a word.
        /// </exception>
        /// <exception cref="IOException">
		/// If an I/O error occurs.
        /// </exception>
        private string GetNextWord(StreamTokenizer tokenizer)
		{
            Token token = null;
            if(!tokenizer.NextToken(out token))
            {
                return null;
            }

            TokenType type = token.Type;

            switch (type)
			{
				case TokenType.Eof: 
					throw new GeometryIOException("Expected word but encountered end of stream");
				
				case TokenType.Eol: 
					throw new GeometryIOException("Expected word but encountered end of line");
				
				case TokenType.Float: 
					throw new GeometryIOException("Expected word but encountered number: " + token.StringValue);
				
                case TokenType.Integer: 
                    throw new GeometryIOException("Expected word but encountered number: " + token.StringValue);
				
				case TokenType.Word: 
					return token.StringValue.ToUpper(CultureInfo.InvariantCulture);

				default:
				{
					string sVal = token.StringValue;
					if (sVal == TokenLParan)
						return TokenLParan;
				
					if (sVal == TokenRParan) 
						return TokenRParan;
				
					if (sVal == TokenComma)
						return TokenComma;
				}
				break;
				
			}

			Debug.Assert(false, "Should never reach here: Encountered unexpected StreamTokenizer type: " + type);
			
            return null;
		}
		
		/// <summary>  
		/// Creates a Geometry using the next token in the stream.
		/// </summary>
		/// <param name="tokenizer"> tokenizer over a stream of text in Well-known Text
		/// format. The next tokens must form a &lt;Geometry Tagged Text&gt;.
		/// </param>
		/// <returns>
		/// A Geometry specified by the next token in the stream.
		/// </returns>
        /// <exception cref=GeometryIOException"">
        /// If the coordinates used to create a Polygon shell and holes do 
        /// not form closed linestrings, or if an unexpected token was 
        /// encountered.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
		private Geometry ReadGeometry(StreamTokenizer tokenizer)
		{
            m_bMeasured    = false;

            Geometry geom  = null;
            int nSRID      = -1;

			string type = GetNextWord(tokenizer);

            //TODO--PAUL When moved to .NET, Use String.Equals(string, string, StringComparison);
            if (type.Equals("SRID"))
            {
                // handle the this case "SRID=4326;GEOMETRY(...)"
                GetNextEquals(tokenizer);
                double dSRID = GetNextNumber(tokenizer);

                if (!Double.IsNaN(dSRID) && !Double.IsInfinity(dSRID))
                {
                    nSRID = Convert.ToInt32(dSRID);
                }

                GetNextSemi(tokenizer);

                type = GetNextWord(tokenizer);
            }

			if (type.Equals("POINT"))
			{
				geom = ReadPoint(tokenizer);
			}
			else if (type.Equals("LINESTRING"))
			{
				geom = ReadLineString(tokenizer);
			}
			else if (type.Equals("LINEARRING"))
			{
				geom = ReadLinearRing(tokenizer);
			}
			else if (type.Equals("POLYGON"))
			{
				geom = ReadPolygon(tokenizer);
			}
			else if (type.Equals("MULTIPOINT"))
			{
				geom = ReadMultiPoint(tokenizer);
			}
			else if (type.Equals("MULTILINESTRING"))
			{
				geom = ReadMultiLineString(tokenizer);
			}
			else if (type.Equals("MULTIPOLYGON"))
			{
				geom = ReadMultiPolygon(tokenizer);
			}
			else if (type.Equals("GEOMETRYCOLLECTION"))
			{
				geom = ReadGeometryCollection(tokenizer);
			}    // start the measured geometries...
            else if (type.Equals("POINTM"))
            {
                m_bMeasured = true;
                geom = ReadPoint(tokenizer);
            }
            else if (type.Equals("LINESTRINGM"))
            {
                m_bMeasured = true;
                geom = ReadLineString(tokenizer);
            }
            else if (type.Equals("LINEARRINGM"))
            {
                m_bMeasured = true;
                geom = ReadLinearRing(tokenizer);
            }
            else if (type.Equals("POLYGONM"))
            {
                m_bMeasured = true;
                geom = ReadPolygon(tokenizer);
            }
            else if (type.Equals("MULTIPOINTM"))
            {
                m_bMeasured = true;
                geom = ReadMultiPoint(tokenizer);
            }
            else if (type.Equals("MULTILINESTRINGM"))
            {
                m_bMeasured = true;
                geom = ReadMultiLineString(tokenizer);
            }
            else if (type.Equals("MULTIPOLYGONM"))
            {
                m_bMeasured = true;
                geom = ReadMultiPolygon(tokenizer);
            }
            else if (type.Equals("GEOMETRYCOLLECTIONM"))
            {
                m_bMeasured = true;
                geom = ReadGeometryCollection(tokenizer);
            }
            else
            {
                throw new GeometryIOException("Unknown type: " + type);
            }

            if (nSRID > -1 && geom != null)
            {              
                if (geom.Properties == null)
                {
                    geom.CreateProperties();
                }

                geom.Properties["SRID"] = nSRID;
            }

            return geom;
        }
		
		/// <summary>  
		/// Creates a Point using the next token in the stream.
		/// </summary>
		/// <param name="tokenizer">
		/// Tokenizer over a stream of text in Well-known Text
		/// format. The next tokens must form a &lt;Point Text&gt;.
		/// </param>
		/// <returns>
		/// A Point specified by the next token in the stream.
		/// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        /// <exception cref="GeometryIOException">
        /// If an unexpected token was encountered.
        /// </exception>
		private Point ReadPoint(StreamTokenizer tokenizer)
		{
			string nextToken = GetNextEmptyOrOpener(tokenizer);
			if (nextToken.Equals(WktEmpty))
			{
				return m_objFactory.CreatePoint((Coordinate) null);
			}

            Coordinate coord = null;
            if (m_bApplyPrecision)
            {
                coord = GetPreciseCoordinate(tokenizer);
            }
            else
            {
                coord = GetCoordinate(tokenizer);
            }
			Point point = m_objFactory.CreatePoint(coord);

			GetNextCloser(tokenizer);

			return point;
		}
		
		/// <summary>
		/// Creates a LineString using the next token in the stream.
		/// </summary>
		/// <param name="tokenizer">
		/// Tokenizer over a stream of text in Well-known Text
		/// format. The next tokens must form a &lt;LineString Text&gt;.
		/// </param>
		/// <returns>
		/// A LineString specified by the next token in the stream.
		/// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        /// <exception cref="GeometryIOException">
        /// If an unexpected token was encountered.
        /// </exception>
		private LineString ReadLineString(StreamTokenizer tokenizer)
		{
			return m_objFactory.CreateLineString(GetCoordinates(tokenizer));
		}
		
		/// <summary>  
		/// Creates a LinearRing using the next token in the stream.
		/// </summary>
		/// <param name="tokenizer">
		/// Tokenizer over a stream of text in Well-known Text
		/// format. The next tokens must form a &lt;LineString Text&gt;.
		/// </param>
		/// <returns>
		/// A LinearRing specified by the next token in the stream.
		/// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        /// <exception cref="GeometryIOException">
        /// If the coordinates used to create the LinearRing do not form a 
        /// closed linestring, or if an unexpected token was encountered.
        /// </exception>
		private LinearRing ReadLinearRing(StreamTokenizer tokenizer)
		{
			return m_objFactory.CreateLinearRing(GetCoordinates(tokenizer));
		}
		
		/// <summary>  
		/// Creates a MultiPoint using the next token in the stream.
		/// </summary>
		/// <param name="tokenizer"> 
		/// Tokenizer over a stream of text in Well-known Text format. 
		/// The next tokens must form a &lt;MultiPoint Text&gt;.
		/// </param>
		/// <returns>
		/// A MultiPoint specified by the next token in the stream.
		/// </returns>
		/// <exception cref="IOException">If an I/O error occurs.</exception>
		/// <exception cref="GeometryIOException">
		/// If an unexpected token was encountered.
		/// </exception>
		private MultiPoint ReadMultiPoint(StreamTokenizer tokenizer)
		{
			return m_objFactory.CreateMultiPoint(ToPoints(GetCoordinates(tokenizer)));
		}
		
		/// <summary>  
		/// Creates an array of Points having the given Coordinates.
		/// </summary>
		/// <param name="coordinates">
		/// The Coordinates with which to create the Points.
		/// </param>
		/// <returns>
		/// <see cref="Point"/>s created from the given coordinates.
		/// </returns>
		private Point[] ToPoints(Coordinate[] coordinates)
		{
            int nCount     = coordinates.Length;
            Point[] points = new Point[nCount];
			for (int i = 0; i < nCount; i++)
			{
				points[i] = m_objFactory.CreatePoint(coordinates[i]);
			}

			return points;
		}
		
        private Point[] ToPoints(ICoordinateList coordinates)
        {
            int nCount     = coordinates.Count;
            Point[] points = new Point[nCount];
            for (int i = 0; i < nCount; i++)
            {
                points[i] = m_objFactory.CreatePoint(coordinates[i]);
            }

            return points;
        }
		
		/// <summary>  
		/// Creates a Polygon using the next token in the stream.
		/// </summary>
		/// <param name="tokenizer">
		/// Tokenizer over a stream of text in Well-known Text
		/// format. The next tokens must form a &lt;Polygon Text&gt;.
		/// </param>
		/// <returns>
		/// A Polygon specified by the next token in the stream.
		/// </returns>
        /// <exception cref="GeometryIOException">
        /// If the coordinates used to create the Polygon shell and holes do 
        /// not form closed linestrings, or if an unexpected token was 
        /// encountered.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
		private Polygon ReadPolygon(StreamTokenizer tokenizer)
		{
			string nextToken = GetNextEmptyOrOpener(tokenizer);
			if (nextToken.Equals(WktEmpty))
			{
                LinearRing objRing = m_objFactory.CreateLinearRing(new Coordinate[]{});

				return m_objFactory.CreatePolygon(objRing, new LinearRing[]{});
			}

            GeometryList holes = new GeometryList();
			LinearRing shell   = ReadLinearRing(tokenizer);
			nextToken = GetNextCloserOrComma(tokenizer);

			while (nextToken.Equals(TokenComma))
			{
				LinearRing hole = ReadLinearRing(tokenizer);
				holes.Add(hole);
				nextToken = GetNextCloserOrComma(tokenizer);
			}

			return m_objFactory.CreatePolygon(shell, 
                holes.ToLinearRingArray());
		}
		
		/// <summary>  
		/// Creates a MultiLineString using the next token in the stream.
		/// </summary>
		/// <param name="tokenizer">
		/// Tokenizer over a stream of text in Well-known Text
		/// format. The next tokens must form a &lt;MultiLineString Text&gt;.
		/// </param>
		/// <returns>
		/// A MultiLineString specified by the next token in the stream.
		/// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        /// <exception cref="GeometryIOException">
        /// If an unexpected token was encountered.
        /// </exception>
		private MultiLineString ReadMultiLineString(StreamTokenizer tokenizer)
		{
			string nextToken = GetNextEmptyOrOpener(tokenizer);

			if (nextToken.Equals(WktEmpty))
			{
				return m_objFactory.CreateMultiLineString(
                    new LineString[]{});
			}

            GeometryList lineStrings = new GeometryList();
			LineString lineString    = ReadLineString(tokenizer);

            lineStrings.Add(lineString);
			nextToken = GetNextCloserOrComma(tokenizer);
			while (nextToken.Equals(TokenComma))
			{
				lineString = ReadLineString(tokenizer);
				lineStrings.Add(lineString);
				nextToken = GetNextCloserOrComma(tokenizer);
			}

			return m_objFactory.CreateMultiLineString(
                lineStrings.ToLineStringArray());
		}
		
		/// <summary>  
		/// Creates a MultiPolygon using the next token in the stream.
		/// </summary>
		/// <param name="tokenizer">
		/// Tokenizer over a stream of text in Well-known Text
		/// format. The next tokens must form a &lt;MultiPolygon Text&gt;.
		/// </param>
		/// <returns>
		/// A MultiPolygon specified by the next token in the stream, or if 
		/// the coordinates used to create the Polygon shells and holes do 
		/// not form closed linestrings.
		/// </returns>
        /// <exception cref=IOException"">
        /// If an I/O error occurs.
        /// </exception>
        /// <exception cref="GeometryIOException">
        /// if an unexpected token was encountered.
        /// </exception>
		private MultiPolygon ReadMultiPolygon(StreamTokenizer tokenizer)
		{
			string nextToken = GetNextEmptyOrOpener(tokenizer);
			if (nextToken.Equals(WktEmpty))
			{
				return m_objFactory.CreateMultiPolygon(new Polygon[]{});
			}

            GeometryList polygons = new GeometryList();
			Polygon polygon       = ReadPolygon(tokenizer);

            polygons.Add(polygon);
			nextToken = GetNextCloserOrComma(tokenizer);
			while (nextToken.Equals(TokenComma))
			{
				polygon = ReadPolygon(tokenizer);

                polygons.Add(polygon);
				nextToken = GetNextCloserOrComma(tokenizer);
			}

			return m_objFactory.CreateMultiPolygon(
                polygons.ToPolygonArray());
		}
		
		/// <summary>  
		/// Creates a GeometryCollection using the next token in the stream.
		/// </summary>
		/// <param name="tokenizer">
		/// Tokenizer over a stream of text in Well-known Text format. The 
		/// next tokens must form a &lt;GeometryCollection Text&gt;.
		/// </param>
		/// <returns>
		/// A GeometryCollection specified by the next token in the stream.
		/// </returns>
        /// <exception cref="GeometryIOException">
        /// If the coordinates used to create a Polygon shell and holes do 
        /// not form closed linestrings, or if an unexpected token was 
        /// encountered.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
		private GeometryCollection ReadGeometryCollection(StreamTokenizer tokenizer)
		{
			string nextToken = GetNextEmptyOrOpener(tokenizer);

			if (nextToken.Equals(WktEmpty))
			{
				return m_objFactory.CreateGeometryCollection(new Geometry[]{});
			}

            GeometryList geometries = new GeometryList();
			Geometry geometry       = ReadGeometry(tokenizer);

            geometries.Add(geometry);
			nextToken = GetNextCloserOrComma(tokenizer);
			while (nextToken.Equals(TokenComma))
			{
				geometry = ReadGeometry(tokenizer);

                geometries.Add(geometry);
				nextToken = GetNextCloserOrComma(tokenizer);
			}

			return m_objFactory.CreateGeometryCollection(geometries.ToArray());
		}
        
        #endregion
	}
}