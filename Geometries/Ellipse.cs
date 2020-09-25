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
	/// Summary description for Ellipse.
	/// </summary>
    [Serializable]
    public class Ellipse : Surface
    {
        #region Private Fields

        private const int NUM_POINTS = 128;

        /// <summary>  The Coordinate wrapped by this Point.</summary>
        private CoordinateCollection m_objCoordinates;
		
        private Coordinate m_objCenter;
        private double     m_dMajorAxis;
        private double     m_dMinorAxis;
        private double     m_dRotation;

        private Polygon    m_objCachedPolygon;
        private LinearRing m_objCachedRing;

        #endregion

        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Ellipse"/> geometry.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="GeometryFactory">geometry factory</see>, which 
        /// created this curve instance.
        /// </param>
        public Ellipse(GeometryFactory factory) : base(factory)
        {
        }

        public Ellipse(Coordinate center, double majorAxis, double minorAxis, 
            GeometryFactory factory) : base(factory)
        {
            m_objCenter  = center;
            m_dMajorAxis = majorAxis;
            m_dMinorAxis = minorAxis;
        }

        public Ellipse(double centerX, double centerY, double majorAxis, 
            double minorAxis, GeometryFactory factory) : base(factory)
        {
            m_objCenter  = new Coordinate(centerX, centerY);
            m_dMajorAxis = majorAxis;
            m_dMinorAxis = minorAxis;
        }
		
        /// <param name="coordinates">     Contains the single coordinate on which to base this Point
        /// , or null to create the empty geometry.
        /// </param>
        public Ellipse(CoordinateCollection coordinates, double majorAxis, 
            double minorAxis, GeometryFactory factory) : base(factory)
        {
            if (coordinates == null)
            {
                coordinates = new CoordinateCollection();
            }

            Debug.Assert(coordinates.Count <= 1);

            m_objCoordinates = coordinates;

            m_objCenter = coordinates.Count != 0 ? coordinates[0] : null;
            m_dMajorAxis = majorAxis;
            m_dMinorAxis = minorAxis;
        }

        public Ellipse(Envelope envelope) : base(null) 
        {
            if (envelope == null)
            {
                throw new ArgumentNullException("envelope");
            }

            m_objCenter  = new Coordinate((envelope.MinX + envelope.MaxX)/2, 
                (envelope.MinY + envelope.MaxY)/2);

            m_dMajorAxis = envelope.Width;
            m_dMinorAxis = envelope.Height;
        }

        public Ellipse(Envelope envelope, GeometryFactory factory) 
            : base(factory) 
        {
            if (envelope == null)
            {
                throw new ArgumentNullException("envelope");
            }

            m_objCenter  = new Coordinate((envelope.MinX + envelope.MaxX)/2, 
                (envelope.MinY + envelope.MaxY)/2);

            m_dMajorAxis = envelope.Width;
            m_dMinorAxis = envelope.Height;
        }

        public Ellipse(Coordinate minPoint, Coordinate maxPoint) 
            : base(null) 
        {
            if (minPoint == null)
            {
                throw new ArgumentNullException("minPoint");
            }
            if (maxPoint == null)
            {
                throw new ArgumentNullException("maxPoint");
            }

            m_objCenter  = new Coordinate((minPoint.X + maxPoint.X)/2, 
                (minPoint.Y + maxPoint.Y)/2);

            m_dMajorAxis = Math.Abs(maxPoint.X - minPoint.X);
            m_dMinorAxis = Math.Abs(maxPoint.Y - minPoint.Y);
        }

        public Ellipse(Coordinate minPoint, Coordinate maxPoint, 
            GeometryFactory factory) : base(factory) 
        {
            if (minPoint == null)
            {
                throw new ArgumentNullException("minPoint");
            }
            if (maxPoint == null)
            {
                throw new ArgumentNullException("maxPoint");
            }

            m_objCenter  = new Coordinate((minPoint.X + maxPoint.X)/2, 
                (minPoint.Y + maxPoint.Y)/2);

            m_dMajorAxis = Math.Abs(maxPoint.X - minPoint.X);
            m_dMinorAxis = Math.Abs(maxPoint.Y - minPoint.Y);
        }
        
        #endregion

        #region Public Properties

        public Coordinate Center
        {
            get
            {
                return m_objCenter;
            }

            set
            {
                m_objCenter = value;
            }
        }

        public double MajorAxis 
        {
            get
            {
                return m_dMajorAxis;
            }

            set
            {
                m_dMajorAxis = value;
            }
        }

        public double MinorAxis 
        {
            get
            {
                return m_dMinorAxis;
            }

            set
            {
                m_dMinorAxis = value;
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

        public virtual Polygon ToPolygon()
        {
            if (m_objCachedPolygon != null)
            {
                return m_objCachedPolygon;
            }

            m_objCachedPolygon = ToPolygon(NUM_POINTS);

            return m_objCachedPolygon;
        }

        /// <summary> 
        /// Creates a circular Polygon.
        /// </summary>
        /// <returns> A circle. </returns>
        public virtual Polygon ToPolygon(int nPts)
        {
            if (IsEmpty)
            {
                return null;
            }

            double xRadius = m_dMajorAxis / 2.0;
            double yRadius = m_dMinorAxis / 2.0;

            double centreX = m_objCenter.X;
            double centreY = m_objCenter.Y;
			
            Coordinate[] pts = new Coordinate[nPts + 1];
            int iPt = 0;
            for (int i = 0; i < nPts; i++)
            {
                double ang = i * (2 * Math.PI / nPts);
                double x   = xRadius * Math.Cos(ang) + centreX;
                double y   = yRadius * Math.Sin(ang) + centreY;

                Coordinate pt = new Coordinate(x, y);
                pts[iPt++] = pt;
            }
            pts[iPt] = pts[0];
			
            LinearRing ring = Factory.CreateLinearRing(pts);
            Polygon poly    = Factory.CreatePolygon(ring, null);

            return poly;
        }

        public virtual LinearRing ToRing()
        {
            if (m_objCachedRing != null)
            {
                return m_objCachedRing;
            }

            m_objCachedRing = ToRing(NUM_POINTS);

            return m_objCachedRing;
        }

        /// <summary> 
        /// Creates a circular Polygon.
        /// </summary>
        /// <returns> A circle. </returns>
        public virtual LinearRing ToRing(int nPts)
        {
            if (IsEmpty)
            {
                return null;
            }

            double xRadius = m_dMajorAxis / 2.0;
            double yRadius = m_dMinorAxis / 2.0;

            double centreX = m_objCenter.X;
            double centreY = m_objCenter.Y;
			
            Coordinate[] pts = new Coordinate[nPts + 1];
            int iPt = 0;
            for (int i = 0; i < nPts; i++)
            {
                double ang = i * (2 * Math.PI / nPts);
                double x   = xRadius * Math.Cos(ang) + centreX;
                double y   = yRadius * Math.Sin(ang) + centreY;

                Coordinate pt = new Coordinate(x, y);
                pts[iPt++] = pt;
            }
            pts[iPt] = pts[0];
			
            LinearRing ring = Factory.CreateLinearRing(pts);

            return ring;
        }
		
        #endregion

        #region Public Static Methods
        
        public static Ellipse CircumEllipse(Coordinate p1, Coordinate p2, 
            Coordinate p3, GeometryFactory factory)
        {
            if (p1 != null && p2 != null && p3 != null)
            {
                Coordinate center = CircumCenter(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);

                double radius = center.Distance(p2);

                return new Ellipse(center.X - radius, center.Y - radius, 
                    2 * radius, 2 * radius, factory);
            }

            return null;
        }
        
        public static Ellipse CircumEllipse(Coordinate p1, 
            Coordinate p2, Coordinate p3)
        {
            if (p1 != null && p2 != null && p3 != null)
            {
                Coordinate center = CircumCenter(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);

                double radius = center.Distance(p2);

                return new Ellipse(center.X - radius, center.Y - radius, 
                    2 * radius, 2 * radius, null);
            }

            return null;
        }
		
        public static Coordinate CircumCenter(double x1, double y1, 
            double x2, double y2, double x3, double y3)
        {
            x2 -= x1;
            x3 -= x1;
            y2 -= y1;
            y3 -= y1;

            double sq2 = (x2 * x2 + y2 * y2);
            double sq3 = (x3 * x3 + y3 * y3);
            double x = (y2 * sq3 - y3 * sq2) / (y2 * x3 - y3 * x2);

            return new Coordinate(x1 + 0.5 * x, y1 + 0.5 * (sq2 - x * x2) / y2);
        }        

        #endregion

        #region IGeometry Members

        public override GeometryType GeometryType
        {
            get
            {
                return GeometryType.Ellipse;
            }
        }

        public override string Name
        {
            get
            {
                return "Ellipse";
            }
        }

        public override bool IsEmpty
        {
            get
            {
                if (m_objCenter == null)
                {
                    return true;
                }

                if (m_dMajorAxis == 0.0 || m_dMinorAxis == 0.0)
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
                // TODO:  Add Ellipse.Boundary getter implementation
                return null;
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
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.Equals(otherGeometry);
            }

            return false;
        }

        public override bool Disjoint(Geometry otherGeometry)
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.Disjoint(otherGeometry);
            }

            return false;
        }

        public override bool Intersects(Geometry otherGeometry)
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.Intersects(otherGeometry);
            }

            return false;
        }

        public override bool Touches(Geometry otherGeometry)
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.Touches(otherGeometry);
            }

            return false;
        }

        public override bool Crosses(Geometry otherGeometry)
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.Crosses(otherGeometry);
            }

            return false;
        }

        public override bool Within(Geometry otherGeometry)
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.Within(otherGeometry);
            }

            return false;
        }

        public override bool Contains(Geometry otherGeometry)
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.Contains(otherGeometry);
            }

            return false;
        }

        public override bool Overlaps(Geometry otherGeometry)
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.Overlaps(otherGeometry);
            }

            return false;
        }

        public override bool Relate(Geometry otherGeometry, string intersectionPattern)
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.Relate(otherGeometry, intersectionPattern);
            }

            return false;
        }

        public override IntersectionMatrix Relate(Geometry otherGeometry)
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.Relate(otherGeometry);
            }

            return null;
        }

        public override Geometry Buffer(double distance)
        {
            if (!IsEmpty)
            {
                double majorAxis = m_dMajorAxis + distance * 2;
                double minorAxis = m_dMinorAxis + distance * 2;

                return new Ellipse(new Coordinate(m_objCenter), 
                    majorAxis, minorAxis, Factory);
            }

            return null;
        }

        public override Geometry ConvexHull()
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.ConvexHull();
            }

            return null;
        }

        public override Geometry Intersection(Geometry otherGeometry)
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.Intersection(otherGeometry);
            }

            return null;
        }

        public override Geometry Union(Geometry otherGeometry)
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.Union(otherGeometry);
            }

            return null;
        }

        public override Geometry Difference(Geometry otherGeometry)
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.Difference(otherGeometry);
            }

            return null;
        }

        public override Geometry SymmetricDifference(Geometry otherGeometry)
        {
            if (m_objCachedRing == null)
            {
                m_objCachedRing = ToRing();
            }

            if (m_objCachedRing != null)
            {
                return m_objCachedRing.SymmetricDifference(otherGeometry);
            }

            return null;
        }

        #endregion

        #region ICloneable Members

        public override Geometry Clone()
        {
            if (IsEmpty)
            {
                return null;
            }

            return new Ellipse(new Coordinate(m_objCenter), 
                m_dMajorAxis, m_dMinorAxis, Factory);
        }

        #endregion

        #region IComparable Members

        public override int CompareTo(object obj)
        {
            // TODO:  Add Ellipse.CompareTo implementation
            return 0;
        }

        #endregion

        #region IGeometryExtension Members

        public override DimensionType BoundaryDimension
        {
            get
            {
                return DimensionType.Surface;
            }
        }

        public override ICoordinateList Coordinates
        {
            get
            {
                return null;
            }
        }

        public override int NumPoints
        {
            get
            {
                return 1;
            }
        }

        public override double Area
        {
            get
            {
                return Math.PI * m_dMajorAxis * m_dMinorAxis / 4;
            }
        }

        public override Point Centroid
        {
            get
            {
                if (Factory != null)
                {
                    return Factory.CreatePoint(m_objCenter);
                }

                return null;
            }
        }

        public override Point InteriorPoint
        {
            get
            {
                if (Factory != null)
                {
                    return Factory.CreatePoint(m_objCenter);
                }

                return null;
            }
        }

        public override double Length
        {
            get
            {
                return 0.0;
            }
        }

        public override void Apply(ICoordinateVisitor filter)
        {
            // TODO:  Add Ellipse.Apply implementation
        }

        public override void Apply(IGeometryComponentVisitor filter)
        {
            // TODO:  Add Ellipse.Apply implementation
        }

        public override void Apply(IGeometryVisitor filter)
        {
            // TODO:  Add Ellipse.Apply implementation
        }

        public override bool EqualsExact(Geometry other, double tolerance)
        {
            // TODO:  Add Ellipse.EqualsExact implementation
            return false;
        }

        public override bool EqualsExact(Geometry other)
        {
            // TODO:  Add Ellipse.EqualsExact implementation
            return false;
        }

        public override Coordinate Coordinate
        {
            get
            {
                return m_objCenter;
            }
        }

        public override void Normalize()
        {
            // TODO:  Add Ellipse.Normalize implementation
        }

        public override void Changed()
        {
            // TODO:  Add Ellipse.GeometryChanged implementation
        }

        protected override int CompareToGeometry(Geometry o)
        {
            // TODO:  Add Ellipse.CompareToGeometry implementation
            return 0;
        }

        protected override Envelope ComputeEnvelope()
        {
            if (IsEmpty)
            {
                return new Envelope();
            }

            return new Envelope(m_objCenter.X - m_dMajorAxis/2, 
                m_objCenter.X + m_dMajorAxis/2, 
                m_objCenter.Y - m_dMinorAxis/2, 
                m_objCenter.Y + m_dMinorAxis/2);
        }

        protected override void OnChanged()
        {
            // TODO:  Add Ellipse.GeometryChangedAction implementation
        }

        protected override bool IsEquivalentType(Geometry other)
        {
            // TODO:  Add Ellipse.IsEquivalentType implementation
            return false;
        }

        #endregion
    }
}
