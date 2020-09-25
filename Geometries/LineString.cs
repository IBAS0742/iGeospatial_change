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

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;
using iGeospatial.Geometries.Operations;
using iGeospatial.Geometries.Visitors;

namespace iGeospatial.Geometries
{
    /// <summary>  
    /// A <see cref="Curve">curve</see> with linear interpolation between points.
    /// Each consecutive pair of points defines a line segment.
    /// A LineString, line string, is usually refers to as a <c>polyline</c>  
    /// in most graphics packages.
    /// </summary>
    /// <remarks>
    /// We are using the definition of LineString given in the 
    /// <see href="http://www.opengis.org/techno/specs.htm">OpenGIS Simple 
    /// Features Specification for SQL</see>. This differs in an important 
    /// way from some other spatial models. The main difference is that
    /// a LineString may be non-simple. They may self-intersect in
    /// points or line segments.
    /// <para>
    /// In fact boundary points of a curve (e.g. the endpoints) may intersect the
    /// interior of the curve, resulting in a curve that is technically
    /// topologically closed but not closed according to the Simple Feature 
    /// Specification. 
    /// In this case, topologically the point of intersection would not be 
    /// on the boundary of the curve. 
    /// </para>
    /// <para>
    /// However, according to the Simple Feature Specification (SFS) 
    /// definition the point is considered to be on the boundary, and 
    /// this implementation follows this definition.
    /// </para>
    /// If the LineString is empty and closed, then the 
    /// <see cref="LineString.IsRing"/> (is ring) return <see langword="false"/>.
    /// <para>
    /// A LineString is simple if it does not pass through the same
    /// point twice (excepting the endpoints, which may be identical).
    /// </para>
    /// </remarks>
    [Serializable]
    public class LineString : Curve
	{
		/// <summary>  The points of this LineString.</summary>
		private ICoordinateList points;
		
		/// <summary>  Constructs a LineString with the given points.
		/// 
		/// </summary>
		/// <param name="points">         the points of the linestring, or null
		/// to create the empty geometry. This array must not contain null
		/// elements. Consecutive points may not be equal.
		/// </param>
		/// <param name="precisionModel"> the specification of the grid of allowable points
		/// for this LineString
		/// </param>
		/// <param name="SRID">           the ID of the Spatial Reference System used by this
		/// LineString
		/// </param>
		/// <deprecated> Use GeometryFactory instead 
		/// </deprecated>
		public LineString(Coordinate[] coords, GeometryFactory factory) 
            : base(factory)
        {
            if (coords == null)
            {
                this.points = new CoordinateCollection();
            }
            else 
            {
                if (coords.Length == 1)
                {
                    throw new ArgumentException("point array must contain 0 or >1 elements");
                }

                this.points = new CoordinateCollection(coords);
            }
        }
		
		/// <param name="points">         the points of the linestring, or null
		/// to create the empty geometry. Consecutive points may not be equal.
		/// </param>
		public LineString(ICoordinateList coords, GeometryFactory factory) 
            : base(factory)
		{
			if (coords == null)
			{
				coords = new CoordinateCollection();
			}
            else 
            {
                if (coords.Count == 1)
                {
                    throw new ArgumentException("point array must contain 0 or >1 elements");
                }
            }

			this.points = coords;
		}
		
		public override ICoordinateList Coordinates
		{
			get
			{
				return points;
			}
		}

		public override DimensionType Dimension
		{
			get
			{
				return DimensionType.Curve;
			}
		}

		public override DimensionType BoundaryDimension
		{
			get
			{
				if (this.IsClosed)
				{
					return DimensionType.Empty;
				}

				return DimensionType.Point;
			}
			
		}

		public override bool IsEmpty
		{
			get
			{
				return points.Count == 0;
			}
		}

        /// <summary>  
        /// Gets the number of points in this instance of the 
        /// <see cref="LineString"/>.
        /// </summary>
        /// <value>
        /// The number of points, <see cref="Point"/>, in this 
        /// <see cref="LineString"/>.
        /// </value>
        public override int NumPoints
		{
			get
			{
				return points.Count;
			}
		}

        /// <summary>  
        /// Gets the first point with which this <see cref="Curve"/> 
        /// object was constructed.
        /// </summary>
        /// <value> 
        /// The start point, or <see langword="null"/> if this 
        /// <see cref="Curve"/> is empty.
        /// </value>
        public virtual Point StartPoint
		{
			get
			{
				if (IsEmpty)
				{
					return null;
				}

				return GetPoint(0);
			}
		}

        /// <summary>  
        /// Gets the last point with which this <see cref="Curve"/> 
        /// object was constructed.
        /// </summary>
        /// <value>
        /// The end point, or <see langword="null"/> if this 
        /// <see cref="Curve"/> is empty.
        /// </value>
        public virtual Point EndPoint
		{
			get
			{
				if (IsEmpty)
				{
					return null;
				}

				return GetPoint(NumPoints - 1);
			}
		}

        /// <summary>  
        /// Gets a value indicating whether the start point and 
        /// the end point are equal or the curve is closed.
        /// </summary>
        /// <value>    
        /// <see langword="true"/>, if the start and end points are equal. 
        /// </value>
        /// <remarks>
        /// Classes implementing <see cref="Curve"/> should document what 
        /// <c>IsClosed</c> returns if the <see cref="Curve"/> is empty.
        /// </remarks>
        public virtual bool IsClosed
		{
			get
			{
				if (IsEmpty)
				{
					return false;
				}

				return GetCoordinate(0).Equals(GetCoordinate(NumPoints - 1));
			}
		}

        /// <summary>  
        /// Gets a value indicating whether this <see cref="Curve"/>
        /// is closed and simple.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this <see cref="Curve"/> is closed and does
        /// not pass through the same point more than once.
        /// </value>
        public virtual bool IsRing
		{
			get
			{
				return IsClosed && IsSimple;
			}
		}

		public override GeometryType GeometryType
		{
			get
			{
				return GeometryType.LineString;
			}
		}

        public override string Name
        {
            get
            {
                return "LineString";
            }
        }

        /// <summary>
        /// The length of the curve in its associated spatial reference.
        /// </summary>
        /// <value>
        /// A double precision value specifying the length of the curve.
        /// </value>
        /// <remarks>
        /// Implementations must document whether the computations handles
        /// coordinates expressed in latitude/longitude or geographical reference systems.
        /// </remarks>
        public override double Length
		{
			get
			{
				return CGAlgorithms.Length(points);
			}
		}

		public override bool IsSimple
		{
			get
			{
				return (new IsSimpleOp()).IsSimple((LineString)this);
			}
		}

		public override Geometry Boundary
		{
			get
			{
				if (IsEmpty)
				{
					return Factory.CreateGeometryCollection(null);
				}
				if (IsClosed)
				{
					return Factory.CreateMultiPoint((Coordinate[]) null);
				}
				return Factory.CreateMultiPoint(new Point[]{StartPoint, EndPoint});
			}
		}
		
        /// <summary>  
        /// Gets the <see cref="Coordinate"/> at the specified index.
        /// </summary>
        /// <param name="n"> 
        /// The zero-based index of the <see cref="Coordinate"/> to return 
        /// (n = 0, 1, 2, ...).
        /// </param>
        /// <returns>
        /// The n-th <see cref="Coordinate"/> in this <see cref="LineString"/>.
        /// </returns>
        public virtual Coordinate GetCoordinate(int n)
		{
			return points[n];
		}
		
		public override Coordinate Coordinate
		{
            get
            {
                if (IsEmpty)
                    return null;

                return points[0];
            }
		}
		
        /// <summary>  
        /// Gets the <see cref="Point"/> at the specified index.
        /// </summary>
        /// <param name="n"> 
        /// The zero-based index of the <see cref="Point"/> to return 
        /// (n = 0, 1, 2, ...).
        /// </param>
        /// <returns>
        /// The n-th <see cref="Point"/> in this <see cref="LineString"/>.
        /// </returns>
        public virtual Point GetPoint(int n)
		{
			return Factory.CreatePoint(points[n]);
		}
        
        /// <summary>
        /// Creates a <see cref="LineString"/> whose coordinates are in the 
        /// reverse order of this objects.
        /// </summary>
        /// <returns>
        /// A <see cref="LineString"/> with coordinates in the reverse order.
        /// </returns>
        public virtual LineString ReverseAll()
        {
            ICoordinateList seq = points.Clone();
            seq.Reverse();

            LineString revLine = this.Factory.CreateLineString(seq);
            
            return revLine;
        }
		
		/// <summary>  
		/// Returns true if the given point is a vertex of this LineString.
		/// </summary>
		/// <param name="pt"> the Coordinate to check
		/// </param>
		/// <returns>     true if pt is one of this LineString
		/// 's vertices
		/// </returns>
		public virtual bool IsCoordinate(Coordinate pt)
		{
			for (int i = 1; i < points.Count; i++)
			{
				if (points[i].Equals(pt))
				{
					return true;
				}
			}
			return false;
		}
		
		protected override Envelope ComputeEnvelope()
		{
			if (IsEmpty)
			{
				return new Envelope();
			}
			//Convert to array, then access array directly, to avoid the function-call overhead
			//of calling #get millions of times. #toArray may be inefficient for
			//non-BasicCoordinateSequence CoordinateSequences. [Jon Aquino]
			ICoordinateList coordinates = points;
			double minx = coordinates[0].X;
			double miny = coordinates[0].Y;
			double maxx = coordinates[0].X;
			double maxy = coordinates[0].Y;

            for (int i = 1; i < coordinates.Count; i++)
			{
				minx = minx < coordinates[i].X ? minx:coordinates[i].X;
				maxx = maxx > coordinates[i].X ? maxx:coordinates[i].X;
				miny = miny < coordinates[i].Y ? miny:coordinates[i].Y;
				maxy = maxy > coordinates[i].Y ? maxy:coordinates[i].Y;
			}
			return new Envelope(minx, maxx, miny, maxy);
		}
		
		public override bool EqualsExact(Geometry other, double tolerance)
		{
			if (!IsEquivalentType(other))
			{
				return false;
			}

			LineString otherLineString = (LineString) other;
			
            if (points.Count != otherLineString.points.Count)
			{
				return false;
			}
			
            for (int i = 0; i < points.Count; i++)
			{
				if (!points[i].Equals(otherLineString.points[i], tolerance))
				{
					return false;
				}
			}
			
            return true;
		}
		
		public override void  Apply(ICoordinateVisitor filter)
		{
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            for (int i = 0; i < points.Count; i++)
			{
				filter.Visit(points[i]);
			}
		}
		
		public override void  Apply(IGeometryVisitor filter)
		{
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            filter.Visit(this);
		}
		
		public override void  Apply(IGeometryComponentVisitor filter)
		{
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            filter.Visit(this);
		}
		
		public override Geometry Clone()
		{
			LineString ls = (LineString) base.MemberwiseClone();
			ls.points     = points.Clone();

			return ls;
		}
		
		/// <summary> Normalizes a LineString.  A normalized linestring
		/// has the first point which is not equal to it's reflected point
		/// less than the reflected point.
		/// </summary>
		public override void Normalize()
		{
			for (int i = 0; i < points.Count / 2; i++)
			{
				int j = points.Count - 1 - i;
				// skip equal points on both ends
				if (!points[i].Equals(points[j]))
				{
					if (points[i].CompareTo(points[j]) > 0)
					{
						Coordinates.Reverse();
					}
					return ;
				}
			}
		}
		
		protected override bool IsEquivalentType(Geometry other)
		{
			return (other.GeometryType == GeometryType.LineString);
		}
		
		protected override int CompareToGeometry(Geometry o)
		{
			LineString line = (LineString) o;
			// MD - optimized implementation
			int i = 0;
			int j = 0;
			while (i < points.Count && j < line.points.Count)
			{
				int comparison = points[i].CompareTo(line.points[j]);
				if (comparison != 0)
				{
					return comparison;
				}
				i++;
				j++;
			}
			
            if (i < points.Count)
			{
				return 1;
			}

			if (j < line.points.Count)
			{
				return - 1;
			}
			return 0;
		}
		
        internal void ValidateConstruction(bool isClosed)
        {
            int nPoints = points.Count;

            if (isClosed)
            {
                if (nPoints >= 1 && nPoints <= 3)
                {
                    throw new System.ArgumentException("Number of points must be 0 or >3");
                }

                if (nPoints != 0 && !points[0].Equals(points[nPoints - 1]))
                {
                    throw new System.ArgumentException("points must form a closed line string");
                }
            }
        }
    }
}