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
using System.Diagnostics;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Visitors;

namespace iGeospatial.Geometries
{
	/// <summary> 
	/// Represents a planar triangle, and provides methods for calculating various
	/// properties of triangles.
	/// </summary>
	[Serializable]
    public class Triangle : Surface
	{
        #region Private Fields

        /// <summary>  The Coordinate wrapped by this Point.</summary>
        private ICoordinateList m_objCoordinates;
		
        [NonSerialized]
        private Coordinate m_objPoint1;
        [NonSerialized]
        private Coordinate m_objPoint2;
        [NonSerialized]
        private Coordinate m_objPoint3;

        [NonSerialized]
        private LinearRing m_objExteriorRing;
        
        #endregion
		
        #region Constructors and Destructor
		
        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle"/> geometry.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="GeometryFactory">geometry factory</see>, which 
        /// created this geometry instance.
        /// </param>
        public Triangle(GeometryFactory factory) : base(factory)
		{
            m_objCoordinates = new CoordinateCollection();
		}
		
		public Triangle(Coordinate p0, Coordinate p1, 
            Coordinate p2, GeometryFactory factory) : base(factory)
		{
            if (p0 == null)
            {
                throw new ArgumentNullException("p0");
            }
            if (p1 == null)
            {
                throw new ArgumentNullException("p1");
            }
            if (p2 == null)
            {
                throw new ArgumentNullException("p2");
            }

			m_objPoint1 = p0;
			m_objPoint2 = p1;
			m_objPoint3 = p2;

            m_objCoordinates = new CoordinateCollection();
            m_objCoordinates.Add(p0);
            m_objCoordinates.Add(p1);
            m_objCoordinates.Add(p2);
        }
		
		public Triangle(ICoordinateList coordinates, 
            GeometryFactory factory) : base(factory)
		{
            if (coordinates == null)
            {
                m_objCoordinates = new CoordinateCollection();
            }
            else
            {
                if (coordinates.Count != 3)
                {
                    throw new GeometryException("The number of coordinates for a triangle must be three.");
                }

                m_objCoordinates = coordinates;

                m_objPoint1 = coordinates[0];
                m_objPoint2 = coordinates[1];
                m_objPoint3 = coordinates[2];
            }
		}
        
        #endregion

        #region Public Properties

        public Coordinate P1
        {
            get
            {
                return m_objPoint1;
            }

            set
            {
                m_objPoint1 = value;
                if (m_objCoordinates != null && m_objCoordinates.Count == 3)
                {
                    m_objCoordinates[0] = value;
                }

                m_objExteriorRing = null;
            }
        }

        public Coordinate P2
        {
            get
            {
                return m_objPoint2;
            }
 
            set
            {
                m_objPoint2 = value;
                if (m_objCoordinates != null && m_objCoordinates.Count == 3)
                {
                    m_objCoordinates[1] = value;
                }

                m_objExteriorRing = null;
            }
        }

        public Coordinate P3
        {
            get
            {
                return m_objPoint3;
            }

            set
            {
                m_objPoint3 = value;
                if (m_objCoordinates != null && m_objCoordinates.Count == 3)
                {
                    m_objCoordinates[2] = value;
                }

                m_objExteriorRing = null;
            }
        }
			
        /// <summary> Returns the smallest of this Triangle's three heights (as measured
        /// perpendicularly from each side).
        /// </summary>
        /// <returns> the smallest of this Triangle's three altitudes
        /// </returns>
        public virtual double MinHeight
        {
            get
            {
                return (2 * this.Area) / MaxSideLength;
            }
        }
			
        /// <summary> Returns the length of this Triangle's longest side.</summary>
        /// <returns> the length of this Triangle's longest side
        /// </returns>
        public virtual double MaxSideLength
        {
            get
            {
                return Math.Min(m_objPoint1.Distance(m_objPoint2), 
                    Math.Max(m_objPoint2.Distance(m_objPoint3), 
                    m_objPoint3.Distance(m_objPoint1)));
            }
        }
        
        #endregion

        #region Public Methods

        public LinearRing ExteriorRing 
        {
            get 
            {
                if (!IsEmpty)
                {
                    if (m_objExteriorRing == null)
                    {
                        m_objExteriorRing = 
                            Factory.CreateLinearRing(m_objCoordinates);
                    }

                    return m_objExteriorRing;
                }

                return null;
           }
        }

        public Polygon ToPolygon() 
        {
            if (!IsEmpty)
            {
                return Factory.CreatePolygon(ToRing(), null);
            }

            return null;
        }

        public override string ToString()
        {
            return "TRIANGLE (" + ToString(m_objPoint1) + ", " + 
                ToString(m_objPoint2) + ", " +
                ToString(m_objPoint3) + ", " + 
                ToString(m_objPoint1) + ")";
        }
		
		/// <summary> 
		/// The inCentre of a triangle is the point which is equidistant
		/// from the sides of the triangle.  This is also the point at which the bisectors
		/// of the angles meet.
		/// 
		/// </summary>
		/// <returns> the point which is the inCentre of the triangle
		/// </returns>
		public virtual Coordinate InCentre()
		{
			// the lengths of the sides, labelled by their opposite vertex
			double len0 = m_objPoint2.Distance(m_objPoint3);
			double len1 = m_objPoint1.Distance(m_objPoint3);
			double len2 = m_objPoint1.Distance(m_objPoint2);
			double circum = len0 + len1 + len2;
			
			double inCentreX = (len0 * m_objPoint1.X + len1 * m_objPoint2.X + len2 * m_objPoint3.X) / circum;
			double inCentreY = (len0 * m_objPoint1.Y + len1 * m_objPoint2.Y + len2 * m_objPoint3.Y) / circum;

			return new Coordinate(inCentreX, inCentreY);
        }

        #endregion

        #region Private Methods

        private String ToString(Coordinate c) 
        {
            return c.X + " " + c.Y;
        }

        #endregion

        #region IGeometry Members

        public override GeometryType GeometryType
        {
            get
            {
                return GeometryType.Triangle;
            }
        }

        public override string Name
        {
            get
            {
                return "Triangle";
            }
        }

        public override bool IsEmpty
        {
            get
            {
                // if at least one vertex is null, then declare this geometry
                // as empty
                if (m_objPoint1 == null || m_objPoint2 == null ||
                    m_objPoint3 == null)
                { 
                    return true;
                }

                return false;
            }
        }

        public override bool IsSimple
        {
            get
            {
                return true;
            }
        }

        public override DimensionType Dimension
        {
            get
            {
                return DimensionType.Surface;
            }
        }

        public override Geometry Boundary
        {
            get
            {
                // TODO:  Add Rectangle.Boundary getter implementation
                return null;
            }
        }

        public override bool Equals(Geometry otherGeometry)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Equals(otherGeometry);
            }

            return false;
        }

        public override bool Disjoint(Geometry otherGeometry)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Disjoint(otherGeometry);
            }

            return false;
        }

        public override bool Intersects(Geometry otherGeometry)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Intersects(otherGeometry);
            }

            return false;
        }

        public override bool Touches(Geometry otherGeometry)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Touches(otherGeometry);
            }

            return false;
        }

        public override bool Crosses(Geometry otherGeometry)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Crosses(otherGeometry);
            }

            return false;
        }

        public override bool Within(Geometry otherGeometry)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Within(otherGeometry);
            }

            return false;
        }

        public override bool Contains(Geometry otherGeometry)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Contains(otherGeometry);
            }

            return false;
        }

        public override bool Overlaps(Geometry otherGeometry)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Overlaps(otherGeometry);
            }

            return false;
        }

        public override bool Relate(Geometry otherGeometry, string intersectionPattern)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Relate(otherGeometry, intersectionPattern);
            }

            return false;
        }

        public override IntersectionMatrix Relate(Geometry otherGeometry)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Relate(otherGeometry);
            }

            return null;
        }

        public override Geometry Buffer(double distance)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Buffer(distance);
            }

            return null;
        }

        public override Geometry ConvexHull()
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.ConvexHull();
            }

            return null;
        }

        public override Geometry Intersection(Geometry otherGeometry)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Intersection(otherGeometry);
            }

            return null;
        }

        public override Geometry Union(Geometry otherGeometry)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Union(otherGeometry);
            }

            return null;
        }

        public override Geometry Difference(Geometry otherGeometry)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.Difference(otherGeometry);
            }

            return null;
        }

        public override Geometry SymmetricDifference(Geometry otherGeometry)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.SymmetricDifference(otherGeometry);
            }

            return null;
        }

        public override bool IsWithinDistance(Geometry geom, double distance)
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = ToRing();
            }

            if (m_objExteriorRing != null)
            {
                return m_objExteriorRing.IsWithinDistance(geom, distance);
            }

            return false;
        }

        #endregion

        #region ICloneable Members

        public override Geometry Clone()
        {
            // TODO:  Add Triangle.Clone implementation
            return null;
        }

        #endregion

        #region IComparable Members

        public override int CompareTo(object obj)
        {
            // TODO:  Add Triangle.CompareTo implementation
            return 0;
        }

        #endregion

        #region IGeometryExtension Members

        public override DimensionType BoundaryDimension
        {
            get
            {
                return DimensionType.Curve;
            }
        }

        public override ICoordinateList Coordinates
        {
            get
            {
                if (!IsEmpty)
                {
                    if (m_objCoordinates != null)
                    {
                        return m_objCoordinates;
                    }

                }

                return null;
            }
        }

        public override int NumPoints
        {
            get
            {
                return 3;
            }
        }

        public override double Area
        {
            get
            {
                if (!this.IsEmpty)
                {
                    return 0.5 * Math.Abs(((m_objPoint2.X - m_objPoint1.X) * 
                        (m_objPoint3.Y - m_objPoint1.Y)) - 
                        ((m_objPoint2.Y - m_objPoint1.Y) * 
                        (m_objPoint3.X - m_objPoint1.X)));
                }

                return 0;
            }
        }

        public override Point Centroid
        {
            get
            {
                // TODO:  Add Triangle.Centroid getter implementation
                return null;
            }
        }

        public override Point InteriorPoint
        {
            get
            {
                return Factory.CreatePoint(InCentre());
            }
        }

        public override double Length
        {
            get
            {
                // TODO:  Add Triangle.Length getter implementation
                return 0;
            }
        }

        public override void Apply(ICoordinateVisitor filter)
        {
            // TODO:  Add Triangle.Apply implementation
        }

        public override void Apply(IGeometryComponentVisitor filter)
        {
            // TODO:  Add Triangle.Apply implementation
        }

        public override void Apply(IGeometryVisitor filter)
        {
            // TODO:  Add Triangle.Apply implementation
        }

        public override bool EqualsExact(Geometry other, double tolerance)
        {
            // TODO:  Add Triangle.EqualsExact implementation
            return false;
        }

        public override Coordinate Coordinate
        {
            get
            {
                return m_objPoint1;
            }
        }

        public override void Normalize()
        {
            // TODO:  Add Triangle.Normalize implementation
        }

        public override bool EqualsExact(Geometry other)
        {
            // TODO:  Add Triangle.iGeospatial.Geometries.IGeometryExtension.EqualsExact implementation
            return false;
        }

        public override void Changed()
        {
            // TODO:  Add Triangle.GeometryChanged implementation
        }
 
        protected override int CompareToGeometry(Geometry o)
        {
            // TODO:  Add Rectangle.CompareToGeometry implementation
            return 0;
        }

        protected override Envelope ComputeEnvelope()
        {
//            if (!IsEmpty)
//            {
//                Envelope envelope = new Envelope(m_objPoint1);
//                envelope.ExpandToInclude(m_objPoint2);
//                envelope.ExpandToInclude(m_objPoint3);
//                envelope.ExpandToInclude(m_objPoint4);
//
//                return envelope;
//            }

            return null;
        }

        #endregion

        #region Private Methods

        private LinearRing ToRing()
        {
            if (m_objExteriorRing == null)
            {
                m_objExteriorRing = 
                    Factory.CreateLinearRing(m_objCoordinates);
            }

            return m_objExteriorRing;
        }

        #endregion
    }
}