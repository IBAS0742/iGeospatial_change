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
using iGeospatial.Geometries.Exports;

namespace iGeospatial.Geometries
{
	/// <summary>
	/// Summary description for RoundedRectangle.
	/// </summary>
    [Serializable]
    public class RoundedRectangle : Surface
    {
        #region Private Fields

        /// <summary>  The Coordinate wrapped by this Point.</summary>
        private ICoordinateList m_objCoordinates;
		
        private Polygon         m_objCachedPolygon;

        private double          m_dCornerRadius;
        private double          m_dRotation;

        #endregion

        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundedRectangle"/> geometry.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="GeometryFactory">geometry factory</see>, which 
        /// created this geometry instance.
        /// </param>
        public RoundedRectangle(GeometryFactory factory) : base(factory) 
        {
            m_objCoordinates   = new CoordinateCollection();
        }

        public RoundedRectangle(Envelope envelope, double cornerRadius) : base(null) 
        {
            if (cornerRadius < 0)
            {
                throw new ArgumentException("The corner radius cannot be negative.");
            }

            m_objCoordinates   = new CoordinateCollection();
            m_dCornerRadius    = cornerRadius;

            if (envelope != null)
            {
                m_objCoordinates.Add(envelope.Min);
                m_objCoordinates.Add(envelope.Max);
            }
            else
            {
                throw new GeometryException("The envelope cannot be null.");
            }
        }

        public RoundedRectangle(Envelope envelope, double cornerRadius, 
            GeometryFactory factory) : base(factory) 
        {
            if (cornerRadius < 0)
            {
                throw new ArgumentException("The corner radius cannot be negative.");
            }

            m_objCoordinates   = new CoordinateCollection();
            m_dCornerRadius    = cornerRadius;

            if (envelope != null)
            {
                m_objCoordinates.Add(envelope.Min);
                m_objCoordinates.Add(envelope.Max);
            }
            else
            {
                throw new GeometryException("The envelope cannot be null.");
            }
        }

        public RoundedRectangle(Coordinate minPoint, Coordinate maxPoint, 
            double cornerRadius) : base(null) 
        {
            if (cornerRadius < 0)
            {
                throw new ArgumentException("The corner radius cannot be negative.");
            }

            m_objCoordinates   = new CoordinateCollection();
            m_dCornerRadius    = cornerRadius;

            if (minPoint != null && maxPoint != null)
            {
                m_objCoordinates.Add(minPoint);
                m_objCoordinates.Add(maxPoint);
            }
            else
            {
                throw new GeometryException("The coordinate list cannot be null.");
            }
        }

        public RoundedRectangle(Coordinate minPoint, Coordinate maxPoint, 
            double cornerRadius, GeometryFactory factory) : base(factory) 
        {
            if (cornerRadius < 0)
            {
                throw new ArgumentException("The corner radius cannot be negative.");
            }

            m_objCoordinates   = new CoordinateCollection();
            m_dCornerRadius    = cornerRadius;

            if (minPoint != null && maxPoint != null)
            {
                m_objCoordinates.Add(minPoint);
                m_objCoordinates.Add(maxPoint);
            }
            else
            {
                throw new GeometryException("The coordinate list cannot be null.");
            }
        }

        public RoundedRectangle(ICoordinateList coordinates, 
            double cornerRadius, GeometryFactory factory) : base(factory)
        {
            if (cornerRadius < 0)
            {
                throw new ArgumentException("The corner radius cannot be negative.");
            }

            m_dCornerRadius    = cornerRadius;
            
            if (coordinates == null)
            {
                throw new GeometryException("The coordinate list cannot be null.");
            }
                
            m_objCoordinates = coordinates;
        }
        
        #endregion

        #region Public Properties

        public double CornerRadius
        {
            get
            {
                return m_dCornerRadius;
            }

            set
            {
                m_dCornerRadius = value;
            }
        }

        public double Rotation
        {
            get
            {
                return m_dRotation;
            }

            set
            {
                m_dRotation = value;
            }
        }

        #endregion

        #region Public Methods

        public LinearRing ToRing() 
        {
            if (!IsEmpty)
            {
                Coordinate pt1 = m_objCoordinates[0];
                Coordinate pt3 = m_objCoordinates[1];
                Coordinate pt2 = new Coordinate(pt3.X, pt1.Y);
                Coordinate pt4 = new Coordinate(pt1.X, pt3.Y);

                return Factory.CreateLinearRing(
                    new Coordinate[] { pt1, pt2, pt3, pt4, pt1 });
            }

            return null;
        }

        public Polygon ToPolygon() 
        {
            if (!IsEmpty)
            {
                if (m_objCachedPolygon == null)
                {
                    Coordinate pt1 = m_objCoordinates[0];
                    Coordinate pt3 = m_objCoordinates[1];
                    Coordinate pt2 = new Coordinate(pt3.X, pt1.Y);
                    Coordinate pt4 = new Coordinate(pt1.X, pt3.Y);

                    m_objCachedPolygon = Factory.CreatePolygon(Factory.CreateLinearRing(
                        new Coordinate[] { pt1, pt2, pt3, pt4, pt1 }), null);
                }

                return m_objCachedPolygon;
            }

            return null;
        }

        public override string ToString()
        {
//            if (!IsEmpty)
//            {
//                Coordinate pt1 = m_objCoordinates[0];
//                Coordinate pt3 = m_objCoordinates[1];
//                return "ROUNDEDRECTANGLE (" + ToString(pt1) + ", " + 
//                    ToString(pt3) + "," + m_dCornerRadius.ToString() + ")";
//            }

            return "ROUNDEDRECTANGLE()";
        }

        #endregion

        #region Private Methods

        private String ToString(Coordinate c) 
        {
            return c.X + " " + c.Y;
        }

        #endregion

        #region IGeometry Members

        public override string Name
        {
            get
            {
                return "RoundedRectangle";
            }
        }

        public override GeometryType GeometryType
        {
            get
            {
                return GeometryType.RoundedRectangle;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                if (m_objCoordinates == null || m_objCoordinates.Count != 2)
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

        public override Geometry Boundary
        {
            get
            {
                return this.ToRing();
            }
        }

        public override DimensionType Dimension
        {
            get
            {
                return DimensionType.Surface;
            }
        }

        public override bool Equals(Geometry otherGeometry)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Equals(otherGeometry);
            }

            return false;
        }

        public override bool Disjoint(Geometry otherGeometry)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Disjoint(otherGeometry);
            }

            return false;
        }

        public override bool Intersects(Geometry otherGeometry)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Intersects(otherGeometry);
            }

            return false;
        }

        public override bool Touches(Geometry otherGeometry)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Touches(otherGeometry);
            }

            return false;
        }

        public override bool Crosses(Geometry otherGeometry)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Crosses(otherGeometry);
            }

            return false;
        }

        public override bool Within(Geometry otherGeometry)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Within(otherGeometry);
            }

            return false;
        }

        public override bool Contains(Geometry otherGeometry)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Contains(otherGeometry);
            }

            return false;
        }

        public override bool Overlaps(Geometry otherGeometry)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Overlaps(otherGeometry);
            }

            return false;
        }

        public override bool Relate(Geometry otherGeometry, string intersectionPattern)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Relate(otherGeometry, intersectionPattern);
            }

            return false;
        }

        public override IntersectionMatrix Relate(Geometry otherGeometry)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Relate(otherGeometry);
            }

            return null;
        }

        public override Geometry Buffer(double distance)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Buffer(distance);
            }

            return null;
        }

        public override Geometry ConvexHull()
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.ConvexHull();
            }

            return null;
        }

        public override Geometry Intersection(Geometry otherGeometry)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Intersection(otherGeometry);
            }

            return null;
        }

        public override Geometry Union(Geometry otherGeometry)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Union(otherGeometry);
            }

            return null;
        }

        public override Geometry Difference(Geometry otherGeometry)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.Difference(otherGeometry);
            }

            return null;
        }

        public override Geometry SymmetricDifference(Geometry otherGeometry)
        {
            if (m_objCachedPolygon == null)
            {
                m_objCachedPolygon = ToPolygon();
            }

            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon.SymmetricDifference(otherGeometry);
            }

            return null;
        }

        #endregion

        #region ICloneable Members

        public override Geometry Clone()
        {
            if (!IsEmpty)
            {
                ICoordinateList coordinates = m_objCoordinates.Clone();
                return new RoundedRectangle(coordinates, m_dCornerRadius, 
                    this.Factory);
            }

            return new RoundedRectangle(this.Factory);
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
                    return m_objCoordinates;
                }

                return null;
            }
        }

        public override int NumPoints
        {
            get
            {
                if (!IsEmpty)
                {
                    return 2;
                }

                return -1;
            }
        }

        public override double Area
        {
            get
            {
                return 0.0;
            }
        }

        public override Point Centroid
        {
            get
            {
                if (!IsEmpty)
                {
                    Coordinate pt1 = m_objCoordinates[0];
                    Coordinate pt3 = m_objCoordinates[1];

                    Coordinate pt  = new Coordinate((pt1.X + pt3.X)/2.0d, 
                        (pt1.Y + pt3.Y)/2.0d);

                    return new Point(pt, this.Factory);
                }

                return null;
            }
        }

        public override Point InteriorPoint
        {
            get
            {
                return this.Centroid;
            }
        }

        public override double Length
        {
            get
            {
                if (!IsEmpty)
                {
                    Normalize();

                    Coordinate pt1 = m_objCoordinates[0];
                    Coordinate pt3 = m_objCoordinates[1];

                    double width  = Math.Abs(pt3.X - pt1.X);
                    double height = Math.Abs(pt3.Y - pt1.Y);

                    return (width + height) * 2.0d;
                }

                return 0.0;
            }
        }

        public override void Apply(ICoordinateVisitor filter)
        {
            if (!IsEmpty && filter != null)
            {
                Coordinate pt1 = m_objCoordinates[0];
                filter.Visit(pt1);
                Coordinate pt3 = m_objCoordinates[1];
                filter.Visit(pt3);
            }
        }

        public override void Apply(IGeometryComponentVisitor filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            filter.Visit(this);
        }

        public override void Apply(IGeometryVisitor filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            filter.Visit(this);
        }

        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentType(other))
            {
                return false;
            }

            RoundedRectangle otherRectangle = (RoundedRectangle) other;
			
            if (this.IsEmpty && otherRectangle.IsEmpty)
            {
                return true;
            }
            else if (!this.IsEmpty && !otherRectangle.IsEmpty)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (!m_objCoordinates[i].Equals( 
                        otherRectangle.m_objCoordinates[i], tolerance))
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
			
            return true;
        }

        public override Coordinate Coordinate
        {
            get
            {
                if (!IsEmpty)
                {
                    Coordinate pt1 = m_objCoordinates[0];
                    Coordinate pt3 = m_objCoordinates[1];

                    return new Coordinate((pt1.X + pt3.X)/2.0d, (pt1.Y + pt3.Y)/2.0d);
                }

                return null;
            }
        }

        public override void Normalize()
        { 
            // Ensure the rectangle is of the forms ( lowerLeft, upperRight )
            if (!IsEmpty)
            {
                Coordinate pt1 = m_objCoordinates[0];
                Coordinate pt3 = m_objCoordinates[1];
                //the minimum x-coordinate
                double dMinX = pt1.X;         
                //the maximum x-coordinate
                double dMaxX = pt3.X;         
                //the minimum y-coordinate
                double dMinY = pt1.Y;         
                //the maximum y-coordinate
                double dMaxY = pt3.Y;

                bool bChanged = false;
                if (dMaxX < dMinX)
                {
                    double tmp = dMinX;
                    dMinX = dMaxX;
                    dMaxX = tmp;
                    bChanged = true;
                }

                if (dMinY > dMaxY)
                {
                    double tmp = dMinY;
                    dMinY = dMaxY;
                    dMaxY = tmp;
                    bChanged = true;
                }

                if (bChanged)
                {
                    pt1.X = dMinX;         
                    pt3.X = dMaxX;         
                    pt1.Y = dMinY;         
                    pt3.Y = dMaxY;

                    Changed();
                }
            }
        }

        public override void Changed()
        {
            m_objCachedPolygon = null; // invalidate the cached polygon

            base.Changed();
        }

        protected override int CompareToGeometry(Geometry o)
        {
            // TODO:  Add RoundedRectangle.CompareToGeometry implementation
            return 0;
        }

        protected override Envelope ComputeEnvelope()
        {
            if (!IsEmpty)
            {
                Coordinate pt1 = m_objCoordinates[0];
                Coordinate pt3 = m_objCoordinates[1];
                
                return new Envelope(pt1, pt3);
            }

            return null;
        }

        protected override void OnChanged()
        {
            m_objCachedPolygon = null;

            base.OnChanged();
        }

        #endregion
    }
}
