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
	/// Basic implementation of Point.
	/// </summary>
    [Serializable]
    public class Point : Geometry
	{
		/// <summary>  The Coordinate wrapped by this Point.</summary>
		private ICoordinateList coordinates;
		
        /// <param name="coordinates">     
        /// Contains the single coordinate on which to base this Point
        /// , or null to create the empty geometry.
        /// </param>
        public Point(Coordinate coordinate, GeometryFactory factory) 
            : base(factory)
        {
            coordinates = new CoordinateCollection();

            coordinates.Add(coordinate);
        }
		
		/// <param name="coordinates">     Contains the single coordinate on which to base this Point
		/// , or null to create the empty geometry.
		/// </param>
		public Point(ICoordinateList coordinates, GeometryFactory factory) 
            : base(factory)
		{
			if (coordinates == null)
			{
				coordinates = new CoordinateCollection();
			}

			Debug.Assert(coordinates.Count <= 1);
			this.coordinates = coordinates;
		}
		
		public override ICoordinateList Coordinates
		{
			get
			{
				return coordinates;
			}
		}
		
        public override int NumPoints
		{
			get
			{
				return IsEmpty ? 0 : 1;
			}
		}
		
        public override bool IsEmpty
		{
			get
			{
				return (this.Coordinate == null);
			}
		}
		
        public override bool IsSimple
		{
			get
			{
				return true;
			}
		}
		
        public override bool IsValid
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
				return DimensionType.Point;
			}
			
		}
		
        public override DimensionType BoundaryDimension
		{
			get
			{
				return DimensionType.Empty;
			}
		}

		public virtual double X
		{
			get
			{
				if (this.Coordinate == null)
				{
					throw new GeometryException("getX called on empty Point");
				}

				return this.Coordinate.X;
			}
		}
		
        public virtual double Y
		{
			get
			{
				if (this.Coordinate == null)
				{
					throw new GeometryException("getY called on empty Point");
				}

				return this.Coordinate.Y;
			}
		}
		
        public override GeometryType GeometryType
		{
			get
			{
				return GeometryType.Point;
			}
		}
		
        public override string Name
        {
            get
            {
                return "Point";
            }
        }
		
        public override Geometry Boundary
		{
			get
			{
				return Factory.CreateGeometryCollection(null);
			}
		}
		
		public override Coordinate Coordinate
		{
            get
            {
                return coordinates.Count != 0 ? coordinates[0] : null;
            }
		}
		
		protected override Envelope ComputeEnvelope()
		{
			if (IsEmpty)
			{
				return new Envelope();
			}

            Coordinate coord = this.Coordinate;

			return new Envelope(coord.X, coord.X, coord.Y, coord.Y);
		}
		
		public override bool EqualsExact(Geometry other, double tolerance)
		{
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (!IsEquivalentType(other))
			{
				return false;
			}
			
            if (IsEmpty && other.IsEmpty)
			{
				return true;
			}

			return other.Coordinate.Equals(this.Coordinate, tolerance);
		}
		
		public override void Apply(ICoordinateVisitor filter)
		{
			if (IsEmpty)
			{
				return;
			}
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

			filter.Visit(this.Coordinate);
		}
		
		public override void Apply(IGeometryVisitor filter)
		{
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            filter.Visit(this);
		}
		
		public override void Apply(IGeometryComponentVisitor filter)
		{
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            filter.Visit(this);
		}
		
		public override Geometry Clone()
		{
			Point p       = (Point) base.MemberwiseClone();
			p.coordinates = coordinates.Clone();

			return p; // return the clone
		}
		
		public override void Normalize()
		{
		}
		
		protected override int CompareToGeometry(Geometry other)
		{
			return this.Coordinate.CompareTo(other.Coordinate);
		}
	}
}