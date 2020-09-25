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
using System.Collections;
using System.Diagnostics;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Visitors;

namespace iGeospatial.Geometries
{
	/// <summary>
	/// Summary description for EllipticArc.
	/// </summary>
    [Serializable]
    public class EllipticArc : Arc
    {
        #region Private Fields

        /// <summary>  The Coordinate wrapped by this Point.</summary>
        private ICoordinateList m_objCoordinates;
		
        private Coordinate m_objCenter;
        private double     m_dRadius;

        #endregion

        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EllipticArc"/> geometry.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="GeometryFactory">geometry factory</see>, which 
        /// created this curve instance.
        /// </param>
        public EllipticArc(GeometryFactory factory) : base(factory)
        {
        }

        public EllipticArc(Coordinate center, double radius, GeometryFactory factory) 
            : base(factory)
        {
            m_objCenter = center;
            m_dRadius   = radius;
        }

        public EllipticArc(double centerX, double centerY, double radius, 
            GeometryFactory factory) : base(factory)
        {
            m_objCenter = new Coordinate(centerX, centerY);
            m_dRadius   = radius;
        }
		
        /// <param name="coordinates">     Contains the single coordinate on which to base this Point
        /// , or null to create the empty geometry.
        /// </param>
        public EllipticArc(ICoordinateList coordinates, double radius, 
            GeometryFactory factory) : base(factory)
        {
            if (coordinates == null)
            {
                coordinates = new CoordinateCollection(new Coordinate[]{});
            }
            else
            {
                Debug.Assert(coordinates.Count <= 1);

                m_objCoordinates = coordinates;

                m_objCenter = coordinates.Count != 0 ? coordinates[0] : null;
                m_dRadius   = radius;
            }
        }
        
        #endregion

        #region Public Properties

        public override Coordinate Center
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

        public double Radius 
        {
            get
            {
                return m_dRadius;
            }

            set
            {
                m_dRadius = value;
            }
        }

        #endregion

        #region Public Methods

        #endregion

        #region IGeometry Members

        public override string Name
        {
            get
            {
                return "EllipticArc";
            }
        }

        public override GeometryType GeometryType
        {
            get
            {
                return GeometryType.EllipticArc;
            }
        }

        public override Envelope Bounds
        {
            get
            {
                // TODO:  Add EllipticArc.EnvelopeInternal getter implementation
                return null;
            }
        }

        public override Geometry Envelope
        {
            get
            {
                // TODO:  Add EllipticArc.Envelope getter implementation
                return null;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                // TODO:  Add EllipticArc.IsEmpty getter implementation
                return false;
            }
        }

        public override bool IsSimple
        {
            get
            {
                // TODO:  Add EllipticArc.IsSimple getter implementation
                return false;
            }
        }

        public override Geometry Boundary
        {
            get
            {
                // TODO:  Add EllipticArc.Boundary getter implementation
                return null;
            }
        }

        public override DimensionType Dimension
        {
            get
            {
                return DimensionType.Curve;
            }
        }

        public override bool Equals(Geometry otherGeometry)
        {
            // TODO:  Add EllipticArc.Equals implementation
            return false;
        }

        public override bool Disjoint(Geometry otherGeometry)
        {
            // TODO:  Add EllipticArc.Disjoint implementation
            return false;
        }

        public override bool Intersects(Geometry otherGeometry)
        {
            // TODO:  Add EllipticArc.Intersects implementation
            return false;
        }

        public override bool Touches(Geometry otherGeometry)
        {
            // TODO:  Add EllipticArc.Touches implementation
            return false;
        }

        public override bool Crosses(Geometry otherGeometry)
        {
            // TODO:  Add EllipticArc.Crosses implementation
            return false;
        }

        public override bool Within(Geometry otherGeometry)
        {
            // TODO:  Add EllipticArc.Within implementation
            return false;
        }

        public override bool Contains(Geometry otherGeometry)
        {
            // TODO:  Add EllipticArc.Contains implementation
            return false;
        }

        public override bool Overlaps(Geometry otherGeometry)
        {
            // TODO:  Add EllipticArc.Overlaps implementation
            return false;
        }

        public override bool Relate(Geometry otherGeometry, String intersectionPattern)
        {
            // TODO:  Add EllipticArc.Relate implementation
            return false;
        }

        public override IntersectionMatrix Relate(Geometry otherGeometry)
        {
            // TODO:  Add EllipticArc.Open.Topology.Geometries.IGeometry.Relate implementation
            return null;
        }

        public override Geometry Buffer(double distance)
        {
            // TODO:  Add EllipticArc.Buffer implementation
            return null;
        }

        public override Geometry ConvexHull()
        {
            // TODO:  Add EllipticArc.ConvexHull implementation
            return null;
        }

        public override Geometry Intersection(Geometry otherGeometry)
        {
            // TODO:  Add EllipticArc.Intersection implementation
            return null;
        }

        public override Geometry Union(Geometry otherGeometry)
        {
            // TODO:  Add EllipticArc.Union implementation
            return null;
        }

        public override Geometry Difference(Geometry otherGeometry)
        {
            // TODO:  Add EllipticArc.Difference implementation
            return null;
        }

        public override Geometry SymmetricDifference(Geometry otherGeometry)
        {
            // TODO:  Add EllipticArc.SymmetricDifference implementation
            return null;
        }

        #endregion

        #region ICloneable Members

        public override Geometry Clone()
        {
            // TODO:  Add EllipticArc.Clone implementation
            return null;
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
                return Math.PI * m_dRadius * m_dRadius;
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
                return 2 * Math.PI * m_dRadius;
            }
        }

        public override void Apply(ICoordinateVisitor filter)
        {
            // TODO:  Add EllipticArc.Apply implementation
        }

        public override void Apply(IGeometryComponentVisitor filter)
        {
            // TODO:  Add EllipticArc.Apply implementation
        }

        public override void Apply(IGeometryVisitor filter)
        {
            // TODO:  Add EllipticArc.Apply implementation
        }

        public override bool EqualsExact(Geometry other, double tolerance)
        {
            // TODO:  Add EllipticArc.EqualsExact implementation
            return false;
        }

        public override bool EqualsExact(Geometry other)
        {
            // TODO:  Add EllipticArc.EqualsExact implementation
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
            // TODO:  Add EllipticArc.Normalize implementation
        }

        public override void Changed()
        {
            // TODO:  Add EllipticArc.GeometryChanged implementation
        }

        protected override int CompareToGeometry(Geometry o)
        {
            // TODO:  Add EllipticArc.CompareToGeometry implementation
            return 0;
        }

        protected override Envelope ComputeEnvelope()
        {
            // TODO:  Add EllipticArc.ComputeEnvelope implementation
            return null;
        }

        protected override void OnChanged()
        {
            // TODO:  Add EllipticArc.GeometryChangedAction implementation
        }

        protected override bool IsEquivalentType(Geometry other)
        {
            // TODO:  Add EllipticArc.IsEquivalentType implementation
            return false;
        }

        #endregion
    }
}
