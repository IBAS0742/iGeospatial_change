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
using System.Text;
using System.Collections;
using System.Diagnostics;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Texts;
using iGeospatial.Geometries.Editors;

namespace iGeospatial.Geometries
{
	/// <summary> 
	/// Supplies a set of utility methods for building Geometry objects 
	/// from lists of coordinates.
	/// </summary>
	[Serializable]
    public class GeometryFactory : IGeometryFactory
	{
        #region Public Static Fields

        internal static readonly GeometryFactory m_objFactory = 
            new GeometryFactory();
        
        #endregion

        #region Private Members

        private PrecisionModel      m_objPrecisionModel;
        private CoordinateType      m_enumCoordinateType;
        private int                 m_nCoordinateDimension;
        private IGeometryProperties m_objProperties;

        #endregion

        #region Constructors and Destructor

		/// <summary> 
		/// Constructs a GeometryFactory that generates Geometries having a floating
		/// PrecisionModel.
		/// </summary>
		public GeometryFactory() : this(new PrecisionModel())
		{
		}
		
		/// <summary> 
		/// Constructs a GeometryFactory that generates Geometries having the given
		/// PrecisionModel and the default CoordinateCollection
		/// implementation.
		/// </summary>
		/// <param name="precisionModel">
		/// The <see cref="PrecisionModel"> precision mode</see> to use.
		/// </param>
		public GeometryFactory(PrecisionModel precisionModel) 
		{
            m_objPrecisionModel    = precisionModel;
            m_enumCoordinateType   = CoordinateType.Default;
            m_nCoordinateDimension = 2;
		}
		
		public GeometryFactory(PrecisionModel precisionModel, 
            CoordinateType type, int dimension) 
		{
            m_objPrecisionModel    = precisionModel;
            m_enumCoordinateType   = type;
            if (dimension < 2)
            {
                dimension = 2;
            }
            m_nCoordinateDimension = dimension;
		}
		
		public GeometryFactory(PrecisionModel precisionModel, 
            CoordinateType type, int dimension, IGeometryProperties properties) 
		{
            m_objPrecisionModel    = precisionModel;
            m_objProperties        = properties;
            m_enumCoordinateType   = type;
            if (dimension < 2)
            {
                dimension = 2;
            }

            m_nCoordinateDimension = dimension;
		}
		
        #endregion

        #region Public Properties

		/// <summary> 
		/// Returns the PrecisionModel that geometries created by this factory
		/// will be associated with.
		/// </summary>
		public PrecisionModel PrecisionModel
		{
			get
			{
				return m_objPrecisionModel;
			}
		}

        public CoordinateType CoordinateType
        {
            get
            {
                return m_enumCoordinateType;
            }
        }

        public int CoordinateDimension 
        { 
            get
            {
                return m_nCoordinateDimension;
            }
        }

        public IGeometryProperties Properties
        {
            get 
            {
                return m_objProperties;
            }
        }
		
        #endregion

        #region Public Methods

        public void CreateProperties()
        {
            if (m_objProperties == null)
            {
                m_objProperties = new GeometryProperties();
            }                                              
        }

        public void CreateProperties(IComparer comparer)
        {
            if (m_objProperties == null)
            {
                m_objProperties = new GeometryProperties(comparer);
            }                                              
        }

		/// <summary>  If the Envelope is a null Envelope, returns an
		/// empty Point. If the Envelope is a point, returns
		/// a non-empty Point. If the Envelope is a
		/// rectangle, returns a Polygon whose points are (minx, miny),
		/// (maxx, miny), (maxx, maxy), (minx, maxy), (minx, miny).
		/// 
		/// </summary>
		/// <param name="envelope">       the Envelope to convert to a Geometry
		/// </param>
		/// <returns>                 an empty Point (for null Envelope
		/// s), a Point (when min x = max x and min y = max y) or a
		/// Polygon (in all other cases)
		/// @throws           GeometryException if coordinates
		/// is not a closed linestring, that is, if the first and last coordinates
		/// are not equal
		/// </returns>
		public virtual Geometry ToGeometry(Envelope envelope)
		{
			if (envelope == null || envelope.IsEmpty)
			{
				return CreatePoint((ICoordinateList) null);
			}
			if (envelope.MinX == envelope.MaxX && envelope.MinY == envelope.MaxY)
			{
				return CreatePoint(new Coordinate(envelope.MinX, envelope.MinY));
			}
			return CreatePolygon(CreateLinearRing(
                new Coordinate[]{new Coordinate(envelope.MinX, envelope.MinY), 
                                 new Coordinate(envelope.MaxX, envelope.MinY), 
                                 new Coordinate(envelope.MaxX, envelope.MaxY), 
                                 new Coordinate(envelope.MinX, envelope.MaxY), 
                                 new Coordinate(envelope.MinX, envelope.MinY)}), 
                null);
		}
		
		/// <summary> Creates a Point using the given Coordinate; a null Coordinate will create
		/// an empty Geometry.
		/// </summary>
		public virtual Point CreatePoint(Coordinate coordinate)
		{
			return CreatePoint(coordinate != null ? new CoordinateCollection(
                new Coordinate[]{coordinate}) : null);
		}
		
		/// <summary> Creates a Point using the given CoordinateCollection; a null or empty
		/// CoordinateCollection will create an empty Point.
		/// </summary>
		public virtual Point CreatePoint(ICoordinateList coordinates)
		{
			return new Point(coordinates, this);
		}
		
		/// <summary> Creates a MultiLineString using the given LineStrings; a null or empty
		/// array will create an empty MultiLineString.
		/// </summary>
		/// <param name="lineStrings">LineStrings, each of which may be empty but not null
		/// </param>
		public virtual MultiLineString CreateMultiLineString(LineString[] lineStrings)
		{
			return new MultiLineString(lineStrings, this);
		}
		
		/// <summary> Creates a GeometryCollection using the given Geometries; a null or empty
		/// array will create an empty GeometryCollection.
		/// </summary>
		/// <param name="geometries">Geometries, each of which may be empty but not null
		/// </param>
		public virtual GeometryCollection CreateGeometryCollection(Geometry[] geometries)
		{
			return new GeometryCollection(geometries, this);
		} 		
		
		/// <summary> Creates a MultiPolygon using the given Polygons; a null or empty array
		/// will create an empty Polygon. The polygons must conform to the
		/// assertions specified in the <A
		/// HREF="http://www.opengis.org/techno/specs.htm">OpenGIS Simple Features
		/// Specification for SQL</A>.
		/// 
		/// </summary>
		/// <param name="">polygons
		/// Polygons, each of which may be empty but not null
		/// </param>
		public virtual MultiPolygon CreateMultiPolygon(Polygon[] polygons)
		{
			return new MultiPolygon(polygons, this);
		}
		
		/// <summary> Creates a LinearRing using the given Coordinates; a null or empty array will
		/// create an empty LinearRing. The points must form a closed and simple
		/// linestring. Consecutive points must not be equal.
		/// </summary>
		/// <param name="coordinates">an array without null elements, or an empty array, or null
		/// </param>
		public virtual LinearRing CreateLinearRing(Coordinate[] coordinates)
		{
			return CreateLinearRing(coordinates != null ? 
                new CoordinateCollection(coordinates) : null);
		}
		
		/// <summary> Creates a LinearRing using the given CoordinateCollection; a null or empty CoordinateCollection will
		/// create an empty LinearRing. The points must form a closed and simple
		/// linestring. Consecutive points must not be equal.
		/// </summary>
		/// <param name="coordinates">a CoordinateCollection possibly empty, or null
		/// </param>
		public virtual LinearRing CreateLinearRing(ICoordinateList coordinates)
		{
			return new LinearRing(coordinates, this);
		}
		
		/// <summary> Creates a MultiPoint using the given Points; a null or empty array will
		/// create an empty MultiPoint.
		/// </summary>
		/// <param name="coordinates">an array without null elements, or an empty array, or null
		/// </param>
		public virtual MultiPoint CreateMultiPoint(Point[] point)
		{
			return new MultiPoint(point, this);
		}
		
		/// <summary> 
		/// Creates a MultiPoint using the given Coordinates; a null or empty array
		/// will create an empty MultiPoint.
		/// </summary>
		/// <param name="">coordinates
		/// an array without null elements, or an empty array, or null
		/// </param>
		public virtual MultiPoint CreateMultiPoint(Coordinate[] coordinates)
		{
            if (coordinates == null)
            {
                coordinates = new Coordinate[]{};
            }

            int nPoints      = coordinates.Length;
            Point[] arrPoint = new Point[nPoints];
            for (int i = 0; i < nPoints; i++)
            {
                arrPoint[i] = CreatePoint(coordinates[i]);
            }

            return CreateMultiPoint(arrPoint);
		}
		
		/// <summary> Creates a MultiPoint using the given CoordinateCollection; a null or empty CoordinateCollection will
		/// create an empty MultiPoint.
		/// </summary>
		/// <param name="coordinates">a CoordinateCollection possibly empty, or null
		/// </param>
		public virtual MultiPoint CreateMultiPoint(ICoordinateList coordinates)
		{
			if (coordinates == null)
			{
				coordinates = new CoordinateCollection(new Coordinate[]{});
			}

            int nPoints      = coordinates.Count;
            Point[] arrPoint = new Point[nPoints];
			for (int i = 0; i < nPoints; i++)
			{
				arrPoint[i] = CreatePoint(coordinates[i]);
			}

			return CreateMultiPoint(arrPoint);
		}
		
		/// <summary> 
		/// Constructs a Polygon with the given exterior boundary and
		/// interior boundaries.
		/// 
		/// </summary>
		/// <param name="">shell
		/// the outer boundary of the new Polygon, or
		/// null or an empty LinearRing if
		/// the empty geometry is to be created.
		/// </param>
		/// <param name="">holes
		/// the inner boundaries of the new Polygon, or
		/// null or empty LinearRing s if
		/// the empty geometry is to be created.
		/// </param>
		public virtual Polygon CreatePolygon(LinearRing shell, LinearRing[] holes)
		{
			return new Polygon(shell, holes, this);
		}
		
		/// <summary>  
		/// Build an appropriate Geometry, MultiGeometry, or GeometryCollection 
		/// to contain the <see cref="Geometry"/> instances in it.
		/// </summary>
		/// <param name="geometryList"> the <see cref="Geometry"/> instances to combine
		/// </param>
		/// <returns>           a Geometry of the "smallest", "most
		/// type-specific" class that can contain the elements of geomList.
		/// </returns>
		/// <remarks>
		/// For example:
		/// <list type="number">
		/// <item>
		/// <description>
		/// If geomList Contains a single Polygon, the Polygon is returned.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// If geomList Contains several Polygons, a MultiPolygon is returned.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// If geomList Contains some Polygons and some LineStrings, a GeometryCollection is
		/// returned.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// If geomList is empty, an empty GeometryCollection is returned.
		/// </description>
		/// </item>
		/// </list>
		/// Note that this method does not "flatten" Geometries in the input, and hence if
		/// any MultiGeometries are contained in the input a GeometryCollection containing
		/// them will be returned.
		/// </remarks>
		public virtual Geometry BuildGeometry(GeometryList geometryList)
		{
            if (geometryList == null)
            {
                throw new ArgumentNullException("geometryList");
            }

            GeometryType geomClass = GeometryType.None;
			bool isHeterogeneous   = false;

            int nCount = geometryList.Count;
			for (int i = 0; i < nCount; i++)
			{
				Geometry geom = geometryList[i];
				GeometryType partClass = geom.GeometryType;
				if (geomClass == GeometryType.None)
				{
					geomClass = partClass;
				}

				if (partClass != geomClass)
				{
					isHeterogeneous = true;
				}
			}

			// for the empty geometry, return an empty GeometryCollection
			if (geomClass == GeometryType.None)
			{
				return CreateGeometryCollection(null);
			}

			if (isHeterogeneous)
			{
				return CreateGeometryCollection(geometryList.ToArray());
			}
			// at this point we know the collection is hetereogenous.
			// Determine the type of the result from the first Geometry in the list
			// this should always return a geometry, since otherwise an empty collection would have already been returned
//			IGeometryEnumerator iTemp = geometryList.GetEnumerator();
//			iTemp.MoveNext();
//			Geometry geom0 = iTemp.Current;
            Geometry geom0 = geometryList[0];
			bool isCollection = nCount > 1;
			if (isCollection)
			{
                GeometryType geomType = geom0.GeometryType;

                if (geomType == GeometryType.Polygon)
				{
					return CreateMultiPolygon(geometryList.ToPolygonArray());
				}
				else if (geomType == GeometryType.LineString || 
                    geomType == GeometryType.LinearRing)
				{
					return CreateMultiLineString(geometryList.ToLineStringArray());
				}
				else if (geomType == GeometryType.Point)
				{
					return CreateMultiPoint(geometryList.ToPointArray());
				}
				Debug.Assert(false, "Should never reach here");
			}

			return geom0;
		}
		
		/// <summary> Creates a LineString using the given Coordinates; a null or empty array will
		/// create an empty LineString. Consecutive points must not be equal.
		/// </summary>
		/// <param name="coordinates">an array without null elements, or an empty array, or null
		/// </param>
		public virtual LineString CreateLineString(Coordinate[] coordinates)
		{
			return CreateLineString(coordinates != null ? new CoordinateCollection(coordinates):null);
		}
		/// <summary> Creates a LineString using the given CoordinateCollection; a null or empty CoordinateCollection will
		/// create an empty LineString. Consecutive points must not be equal.
		/// </summary>
		/// <param name="coordinates">a CoordinateCollection possibly empty, or null
		/// </param>
		public virtual LineString CreateLineString(ICoordinateList coordinates)
		{
			return new LineString(coordinates, this);
		}
		
		/// <returns> 
		/// a clone of g based on a CoordinateCollection created by this
		/// GeometryFactory's ICoordinateSequenceFactory
		/// </returns>
		public virtual Geometry CreateGeometry(Geometry g)
		{
            GeometryEditor editor  = new GeometryEditor(this);
            return editor.Edit(g, new CoordinateOperation());
		}

        public virtual Text CreateText()
        {
            return new Text(this);
        }

        public virtual Text CreateText(Coordinate position, string text)
        {
            return new Text(position, text, this);
        }

        public virtual Text CreateText(Coordinate position, string text, float rotation,
            bool isVertical, float width, float height)
        {
            return new Text(position, text, rotation, isVertical, 
                width, height, this);
        }

        public virtual Text CreateText(Coordinate position, string text, float rotation,
            bool isVertical, float width, float height, Encoding encoding)
        {
            return new Text(position, text, rotation, isVertical, 
                width, height, encoding, this);
        }

        public virtual MultiText CreateMultiText(Text[] texts)
        {
            return new MultiText(texts, this);
        }

        public virtual Rectangle CreateRectangle(Envelope envelope) 
        {
            return new Rectangle(envelope, this);
        }

        public virtual Rectangle CreateRectangle(Coordinate minPoint, 
            Coordinate maxPoint) 
        {
            return new Rectangle(minPoint, maxPoint, this);
        }

        public virtual MultiRectangle CreateMultiEllipse(Rectangle[] rectangles) 
        {
            return new MultiRectangle(rectangles, this);
        }

        public virtual Ellipse CreateEllipse(Coordinate center, 
           double majorAxis, double minorAxis)
        {
           return new Ellipse(center, majorAxis, minorAxis, this);
        }
		
        public virtual Ellipse CreateEllipse(Envelope envelope) 
        {
            return new Ellipse(envelope, this);
        }

        public virtual Ellipse CreateEllipse(Coordinate minPoint, 
            Coordinate maxPoint) 
        {
            return new Ellipse(minPoint, maxPoint, this);
        }

        public virtual MultiEllipse CreateMultiEllipse(Ellipse[] ellipses) 
        {
            return new MultiEllipse(ellipses, this);
        }

        public virtual Circle CreateCircle(Coordinate center, double radius)
        {
           return new Circle(center, radius, this);
        }
		
        public virtual Circle CreateCircle(Envelope envelope) 
        {
            return new Circle(envelope, this);
        }

        public virtual Circle CreateCircle(Coordinate minPoint, 
            Coordinate maxPoint) 
        {
            return new Circle(minPoint, maxPoint, this);
        }

        public virtual MultiCircle CreateMultiCircle(Circle[] circles) 
        {
            return new MultiCircle(circles, this);
        }

        #endregion

        #region Internal Static Methods

        internal static GeometryFactory GetInstance()
        {
            return m_objFactory;
        }
		
        #endregion

        #region ICloneable Members

        public virtual GeometryFactory Clone()
        {
            GeometryFactory objFactory = new GeometryFactory(m_objPrecisionModel,
                m_enumCoordinateType, m_nCoordinateDimension);

            if (m_objProperties != null)
            {
                objFactory.CreateProperties();
                if (m_objProperties.Count > 0)
                {
                    IDictionaryEnumerator iterator = 
                        m_objProperties.GetEnumerator();

                    while (iterator.MoveNext()) 
                    {
                        objFactory.m_objProperties[(string)iterator.Key] 
                            = iterator.Value;
                    }
                }
            }

            return objFactory;
        }

        IGeometryFactory IGeometryFactory.Clone()
        {
            return this.Clone();
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
    }
}