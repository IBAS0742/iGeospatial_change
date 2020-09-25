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
using System.Diagnostics;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Visitors;
using iGeospatial.Geometries.Exports;

namespace iGeospatial.Geometries.Texts
{
	/// <summary>
	/// Summary description for Text.
	/// </summary>
    [Serializable]
    public class Text : Geometry
    {
        #region Private Fields

        /// <summary>  The Coordinate wrapped by this Point.</summary>
        private CoordinateCollection m_objCoordinates;
		
        private Coordinate m_objPosition;
        private Coordinate m_objAnchor;
        private string     m_strLabel;
        private bool       m_bIsVertical;
        private float      m_fRotation;
        private float      m_fHeight;
        private float      m_fWidth;
        private Encoding   m_objEncoding;

        #endregion

        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> geometry.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="GeometryFactory">geometry factory</see>, which 
        /// created this curve instance.
        /// </param>
        public Text(GeometryFactory factory) : base(factory)
        {
        }

        public Text(Coordinate position, GeometryFactory factory) 
            : base(factory)
        {
            m_objPosition = position;
            m_strLabel    = "";
        }

        public Text(Coordinate position, string text, 
            GeometryFactory factory) : base(factory)
        {
            m_objPosition = position;
            m_strLabel    = text;
        }

        public Text(double positionX, double positionY, 
            GeometryFactory factory) : base(factory)
        {
            m_objPosition = new Coordinate(positionX, positionY);
        }

        public Text(double positionX, double positionY, string text, 
            GeometryFactory factory) : base(factory)
        {
            m_objPosition = new Coordinate(positionX, positionY);
            m_strLabel    = text;
        }
		
        /// <param name="coordinates">     
        /// Contains the single coordinate on which to base this Point
        /// , or null to create the empty geometry.
        /// </param>
        public Text(CoordinateCollection coordinates, GeometryFactory factory) 
            : base(factory)
        {
            m_objCoordinates = coordinates;
            if (coordinates != null)
            {
                m_objPosition = coordinates.Count != 0 ? coordinates[0] : null;
            }
        }
		
        /// <param name="coordinates">     
        /// Contains the single coordinate on which to base this Point
        /// , or null to create the empty geometry.
        /// </param>
        public Text(CoordinateCollection coordinates, string text, 
            GeometryFactory factory) : base(factory)
        {
            m_objCoordinates = coordinates;
            
            if (coordinates != null)
            {
                m_objPosition = coordinates.Count != 0 ? coordinates[0] : null;
            }

            m_strLabel = text;
        }

        public Text(Coordinate position, string text, float rotation,
            bool isVertical, float width, float height, GeometryFactory factory)
            : base(factory)
        {
            m_objPosition = position;
            m_strLabel    = text;
            m_bIsVertical = isVertical;
            m_fRotation   = rotation;
            m_fHeight     = height;
            m_fWidth      = width;
        }

        public Text(Coordinate position, string text, float rotation,
            bool isVertical, float width, float height, Encoding encoding,
            GeometryFactory factory)
            : base(factory)
        {
            m_objPosition = position;
            m_strLabel    = text;
            m_bIsVertical = isVertical;
            m_fRotation   = rotation;
            m_fHeight     = height;
            m_fWidth      = width;
            m_objEncoding = encoding;
        }
        
        #endregion

        #region Public Properties

        public Coordinate Position
        {
            get
            {
                return m_objPosition;
            }

            set
            {
                m_objPosition = value;
            }
        }

        public Coordinate Anchor
        {
            get
            {
                return m_objAnchor;
            }

            set
            {
                m_objAnchor = value;
            }
        }

        public string Label
        {
            get
            {
                return m_strLabel;
            }

            set
            {
                m_strLabel = value;
            }
        }

        public bool IsVertical
        {
            get
            {
                return m_bIsVertical;
            }

            set
            {
                m_bIsVertical = value;
            }
        }

        public float Rotation
        {
            get
            {
                return m_fRotation;
            }

            set
            {
                m_fRotation = value;
            }
        }

        public float Height
        {
            get
            {
                return m_fHeight;
            }

            set
            {
                m_fHeight = value;
            }
        }

        public float Width
        {
            get
            {
                return m_fWidth;
            }

            set
            {
                m_fWidth = value;
            }
        }

        public Encoding CharEncoding
        {
            get
            {
                return m_objEncoding;
            }

            set
            {
                m_objEncoding = value;
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
                return "Text";
            }
        }

        public override GeometryType GeometryType
        {
            get
            {
                return GeometryType.Text;
            }
        }

        public override Envelope Bounds
        {
            get
            {
                // TODO:  Add Text.Bounds getter implementation
                return null;
            }
        }

        public override Geometry Envelope
        {
            get
            {
                // TODO:  Add Text.Envelope getter implementation
                return null;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                // TODO:  Add Text.IsEmpty getter implementation
                return false;
            }
        }

        public override bool IsSimple
        {
            get
            {
                // TODO:  Add Text.IsSimple getter implementation
                return false;
            }
        }

        public override Geometry Boundary
        {
            get
            {
                // TODO:  Add Text.Boundary getter implementation
                return null;
            }
        }

        public override DimensionType Dimension
        {
            get
            {
                return DimensionType.Point;
            }
        }

        public override bool Equals(Geometry otherGeometry)
        {
            // TODO:  Add Text.Equals implementation
            return false;
        }

        public override bool Disjoint(Geometry otherGeometry)
        {
            // TODO:  Add Text.Disjoint implementation
            return false;
        }

        public override bool Intersects(Geometry otherGeometry)
        {
            // TODO:  Add Text.Intersects implementation
            return false;
        }

        public override bool Touches(Geometry otherGeometry)
        {
            // TODO:  Add Text.Touches implementation
            return false;
        }

        public override bool Crosses(Geometry otherGeometry)
        {
            // TODO:  Add Text.Crosses implementation
            return false;
        }

        public override bool Within(Geometry otherGeometry)
        {
            // TODO:  Add Text.Within implementation
            return false;
        }

        public override bool Contains(Geometry otherGeometry)
        {
            // TODO:  Add Text.Contains implementation
            return false;
        }

        public override bool Overlaps(Geometry otherGeometry)
        {
            // TODO:  Add Text.Overlaps implementation
            return false;
        }

        public override bool Relate(Geometry otherGeometry, String intersectionPattern)
        {
            // TODO:  Add Text.Relate implementation
            return false;
        }

        public override IntersectionMatrix Relate(Geometry otherGeometry)
        {
            // TODO:  Add Text.MapQuest.Topology.Geometries.IGeometry.Relate implementation
            return null;
        }

        public override Geometry Buffer(double distance)
        {
            // TODO:  Add Text.Buffer implementation
            return null;
        }

        public override Geometry ConvexHull()
        {
            // TODO:  Add Text.ConvexHull implementation
            return null;
        }

        public override Geometry Intersection(Geometry otherGeometry)
        {
            // TODO:  Add Text.Intersection implementation
            return null;
        }

        public override Geometry Union(Geometry otherGeometry)
        {
            // TODO:  Add Text.Union implementation
            return null;
        }

        public override Geometry Difference(Geometry otherGeometry)
        {
            // TODO:  Add Text.Difference implementation
            return null;
        }

        public override Geometry SymmetricDifference(Geometry otherGeometry)
        {
            // TODO:  Add Text.SymmetricDifference implementation
            return null;
        }

        #endregion

        #region ICloneable Members

        public override Geometry Clone()
        {
            // TODO:  Add Text.Clone implementation
            return null;
        }

        #endregion

        #region IComparable Members

        public override int CompareTo(object obj)
        {
            // TODO:  Add Text.CompareTo implementation
            return 0;
        }

        #endregion

        #region IGeometryExtension Members

        public override DimensionType BoundaryDimension
        {
            get
            {
                return DimensionType.Empty;
            }
        }

        public override ICoordinateList Coordinates
        {
            get
            {
                if (m_objCoordinates == null)
                {
                    m_objCoordinates = new CoordinateCollection();
                    m_objCoordinates.Add(m_objPosition);

                    if (m_objAnchor != null)
                    {
                        m_objCoordinates.Add(m_objAnchor);
                    }

                    return m_objCoordinates;
                }

                if (m_objCoordinates.Count == 0)
                {
                    m_objCoordinates.Add(m_objPosition);

                    if (m_objAnchor != null)
                    {
                        m_objCoordinates.Add(m_objAnchor);
                    }
                }

                return m_objCoordinates;
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
                return 0.0;
            }
        }

        public override Point Centroid
        {
            get
            {
                return null;
            }
        }

        public override Point InteriorPoint
        {
            get
            {
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
            if (filter != null)
            {
                if (m_objPosition != null)
                {
                    filter.Visit(m_objPosition);
                }

                if (m_objAnchor != null)
                {
                    filter.Visit(m_objAnchor);
                }
            }
        }

        public override void Apply(IGeometryComponentVisitor filter)
        {
            if (filter != null)
            {
                filter.Visit(this);
            }
        }

        public override void Apply(IGeometryVisitor filter)
        {
            if (filter != null)
            {
                filter.Visit(this);
            }
        }

        public override bool EqualsExact(Geometry other, double tolerance)
        {
            // TODO:  Add Text.EqualsExact implementation
            return false;
        }

        public override bool EqualsExact(Geometry other)
        {
            // TODO:  Add Text.EqualsExact implementation
            return false;
        }

        public override Coordinate Coordinate
        {
            get
            {
                return m_objPosition;
            }
        }

        public override void Normalize()
        {
            // TODO:  Add Text.Normalize implementation
        }

        public override void Changed()
        {
            // TODO:  Add Text.GeometryChanged implementation
        }

        protected override int CompareToGeometry(Geometry o)
        {
            // TODO:  Add Text.CompareToGeometry implementation
            return 0;
        }

        protected override Envelope ComputeEnvelope()
        {
            // TODO:  Add Text.ComputeEnvelope implementation
            return null;
        }

        protected override void OnChanged()
        {
            base.OnChanged();
        }

        #endregion
    }
}
