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

using iGeospatial.Geometries.Algorithms;
using iGeospatial.Coordinates;
using iGeospatial.Geometries.Visitors;
using iGeospatial.Geometries.Exports;

namespace iGeospatial.Geometries
{
	/// <summary> 
	/// Represents a line segment defined by two Coordinates.
	/// Provides methods to compute various geometric properties
	/// and relationships of line segments.
	/// </summary>
	/// <remarks>
	/// This class is designed to be easily mutable (to the extent of
	/// having its contained points public).
	/// This supports a common pattern of reusing a single LineSegment
	/// object as a way of computing segment properties on the
	/// segments defined by arrays or lists of Coordinates.
	/// </remarks>
	[Serializable]
	public class LineSegment : Curve
	{
        #region Private Fields

        private static readonly LineIntersector lineInt = 
            new RobustLineIntersector();

        internal Coordinate p0;
        internal Coordinate p1;

        #endregion
		
        #region Constructors and Destructor
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LineSegment"/> geometry.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="GeometryFactory">geometry factory</see>, which 
        /// created this geometry instance.
        /// </param>
        public LineSegment(GeometryFactory factory) 
            : this(factory, new Coordinate(), new Coordinate())
        {
        }
		
        public LineSegment(GeometryFactory factory, Coordinate p0, Coordinate p1)
            : base(factory)
        {
            this.p0 = p0;
            this.p1 = p1;
        }
		
        public LineSegment(LineSegment segment, GeometryFactory factory) 
            : this(factory)
        {
            if (segment == null)
            {
                throw new ArgumentNullException("segment");
            }

            this.p0 = segment.p0;
            this.p1 = segment.p1;
        }

        #endregion

        #region Public Properties
		
		/// <summary> Tests whether the segment is horizontal.
		/// 
		/// </summary>
		/// <returns> true if the segment is horizontal
		/// </returns>
		public bool IsHorizontal
		{
			get
			{
				return p0.Y == p1.Y;
			}
		}

		/// <summary> Tests whether the segment is vertical.
		/// 
		/// </summary>
		/// <returns> true if the segment is vertical
		/// </returns>
		public bool IsVertical
		{
			get
			{
				return p0.X == p1.X;
			}
		}

        public Coordinate StartPoint
        {
            get
            {
                return p0;
            }

            set
            {
                p0 = value;
            }
        }

        public Coordinate EndPoint
        {
            get
            {
                return p1;
            }

            set
            {
                p1 = value;
            }
        }

        #endregion
		
        #region Public Methods

		public Coordinate GetCoordinate(int i)
		{
			if (i == 0)
				return p0;

			return p1;
		}
		
		public void SetCoordinates(LineSegment segment)
		{
            if (segment == null)
            {
                throw new ArgumentNullException("segment");
            }

            SetCoordinates(segment.p0, segment.p1);
		}
		
		public void SetCoordinates(Coordinate p0, Coordinate p1)
		{
            if (p0 == null)
            {
                throw new ArgumentNullException("p0");
            }
            if (p1 == null)
            {
                throw new ArgumentNullException("p1");
            }

            this.p0.X = p0.X;
			this.p0.Y = p0.Y;
			this.p1.X = p1.X;
			this.p1.Y = p1.Y;
		}
		
		/// <summary> 
		/// Determines the orientation of a LineSegment relative to this segment.
		/// </summary>
		/// <param name="segment">The LineSegment to Compare.</param>
		/// <returns> 1 if seg is to the left of this segment
		/// </returns>
		/// <returns> -1 if seg is to the right of this segment
		/// </returns>
		/// <returns> 0 if seg has indeterminate orientation relative to this segment
		/// </returns>
		/// <remarks>
		/// The concept of orientation is specified as follows:
		/// Given two line segments A and L,
		/// <list type="number">
		/// <item>
		/// <description>
		/// A is to the left of a segment L if A lies wholly in the
		/// closed half-plane lying to the left of L.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// A is to the right of a segment L if A lies wholly in the
		/// closed half-plane lying to the right of L.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// Otherwise, A has indeterminate orientation relative to L. This
		/// happens if A is collinear with L or if A Crosses the line determined by L.
		/// </description>
		/// </item>
		/// </list>
		/// </remarks>
		public int OrientationIndex(LineSegment segment)
		{
            if (segment == null)
            {
                throw new ArgumentNullException("segment");
            }

            int orient0 = RobustCGAlgorithms.OrientationIndex(p0, p1, segment.p0);
 			int orient1 = RobustCGAlgorithms.OrientationIndex(p0, p1, segment.p1);

			// this handles the case where the points are L or collinear
			if (orient0 >= 0 && orient1 >= 0)
				return Math.Max(orient0, orient1);

			// this handles the case where the points are R or collinear
			if (orient0 <= 0 && orient1 <= 0)
				return Math.Max(orient0, orient1);

			// points lie on opposite sides ==> indeterminate orientation
			return 0;
		}

		/// <summary> 
		/// Reverses the direction of the line segment.
		/// </summary>
		public void Reverse()
		{
			Coordinate temp = p0;
			p0 = p1;
			p1 = temp;
		}

		/// <summary> 
		/// Puts the line segment into a normalized form.
		/// </summary>
		/// <remarks>
		/// This is useful for using line segments in maps and indexes when
		/// topological equality rather than exact equality is desired.
		/// <para>
        /// A segment in normalized form has the first point smaller than 
        /// the second (according to the standard ordering on 
        /// <see cref="Coordinate"/>).
        /// </para>
		/// </remarks>
		public override void Normalize()
		{
			if (p1.CompareTo(p0) < 0)
				Reverse();
		}
		
        /// <summary> 
        /// Computes the angle that the vector defined by this segment
        /// makes with the X-axis.
        /// The angle will be in the range 
        /// <c>[ -<see cref="Math.PI"/>, <see cref="Math.PI"/> ]</c> radians.
        /// </summary>
        /// <returns>
        /// The angle this segment makes with the x-axis (in radians).
        /// </returns>
        public double Angle()
		{
			return Math.Atan2(p1.Y - p0.Y, p1.X - p0.X);
		}
		
		/// <summary> 
		/// Computes the Distance between this line segment and another segment.
		/// </summary>
        /// <returns>
        /// The distance to the other segment.
        /// </returns>
        public double Distance(LineSegment segment)
		{
            if (segment == null)
            {
                throw new ArgumentNullException("segment");
            }

            return CGAlgorithms.DistanceLineLine(p0, p1, segment.p0, segment.p1);
		}
		
		/// <summary> 
		/// Computes the Distance between this line segment and a given point.
		/// </summary>
        /// <returns> 
        /// The distance from this segment to the given point.
        /// </returns>
        public double Distance(Coordinate p)
		{
			return CGAlgorithms.DistancePointLine(p, p0, p1);
		}
		
		/// <summary> 
		/// Computes the perpendicular Distance between the (infinite) line defined
		/// by this line segment and a point.
		/// </summary>
        /// <returns> 
        /// The perpendicular distance between the defined line and the 
        /// given point.
        /// </returns>
		public double DistancePerpendicular(Coordinate p)
		{
			return CGAlgorithms.DistancePointLinePerpendicular(p, p0, p1);
		}
		
        /// <summary> 
        /// Computes the <see cref="Coordinate"/> that lies a given
        /// fraction along the line defined by this segment.
        /// A fraction of <c>0.0</c> returns the start point of the segment;
        /// a fraction of <c>1.0</c> returns the end point of the segment.
        /// </summary>
        /// <param name="segmentLengthFraction">
        /// The fraction of the segment length along the line.
        /// </param>
        /// <returns>
        /// The point at that distance.
        /// </returns>
        public Coordinate PointAlong(double segmentLengthFraction)
        {
            Coordinate coord = new Coordinate();
            coord.X = p0.X + segmentLengthFraction * (p1.X - p0.X);
            coord.Y = p0.Y + segmentLengthFraction * (p1.Y - p0.Y);

            return coord;
        }
		
        /// <summary> 
        /// Computes the Projection Factor for the projection of the point p
        /// onto this LineSegment.  The Projection Factor is the constant r
        /// by which the vector for this segment must be multiplied to
        /// equal the vector for the projection of p on the line
        /// defined by this segment.
        /// </summary>
        public double ProjectionFactor(Coordinate p)
		{
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }

            if (p.Equals(p0))
				return 0.0;
			if (p.Equals(p1))
				return 1.0;
			// Otherwise, use comp.graphics.algorithms Frequently Asked Questions method
			/*     	      AC dot AB
			r = ---------
			||AB||^2
			r has the following meaning:
			r=0 P = A
			r=1 P = B
			r<0 P is on the backward extension of AB
			r>1 P is on the forward extension of AB
			0<r<1 P is interior to AB
			*/
			double dx = p1.X - p0.X;
			double dy = p1.Y - p0.Y;
			double len2 = dx * dx + dy * dy;
			double r = ((p.X - p0.X) * dx + (p.Y - p0.Y) * dy) / len2;

			return r;
		}
		
		/// <summary> 
		/// Compute the projection of a point onto the line determined by this line segment.
		/// </summary>
		/// <remarks>
		/// Note that the projected point may lie outside the line segment.  
		/// If this is the case, the projection factor will lie outside the range [0.0, 1.0].
		/// </remarks>
		public Coordinate Project(Coordinate p)
		{
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }

            if (p.Equals(p0) || p.Equals(p1))
            {
                return new Coordinate(p);
            }
			
			double r = ProjectionFactor(p);
			Coordinate coord = new Coordinate();
			coord.X = p0.X + r * (p1.X - p0.X);
			coord.Y = p0.Y + r * (p1.Y - p0.Y);

			return coord;
		}

		/// <summary> 
		/// Project a line segment onto this line segment and return the resulting
		/// line segment. 
		/// </summary>
		/// <param name="segment">the line segment to project
		/// </param>
		/// <returns> the projected line segment, or null if there is no overlap
		/// </returns>
		/// <remarks>
		/// The returned line segment will be a subset of
		/// the target line line segment.  This subset may be null, if
		/// the segments are oriented in such a way that there is no projection.
		/// <para>
		/// Note that the returned line may have zero length (i.e. the same endpoints).
		/// This can happen for instance if the lines are perpendicular to one another.
		/// </para>
		/// </remarks>
		public LineSegment Project(LineSegment segment)
		{
            if (segment == null)
            {
                throw new ArgumentNullException("segment");
            }

            double pf0 = ProjectionFactor(segment.p0);
			double pf1 = ProjectionFactor(segment.p1);
			// check if segment projects at all
			if (pf0 >= 1.0 && pf1 >= 1.0)
				return null;
			if (pf0 <= 0.0 && pf1 <= 0.0)
				return null;
			
			Coordinate newp0 = Project(segment.p0);
			if (pf0 < 0.0)
				newp0 = p0;
			if (pf0 > 1.0)
				newp0 = p1;
			
			Coordinate newp1 = Project(segment.p1);
			if (pf1 < 0.0)
				newp1 = p0;
			if (pf1 > 1.0)
				newp1 = p1;
			
			return new LineSegment(Factory, newp0, newp1);
		}

		/// <summary> 
		/// Computes the closest point on this line segment to another point.
		/// </summary>
		/// <param name="p">the point to find the closest point to
		/// </param>
		/// <returns> 
		/// a Coordinate which is the closest point on the line segment to the point p
		/// </returns>
		public Coordinate ClosestPoint(Coordinate p)
		{
			double factor = ProjectionFactor(p);
			if (factor > 0 && factor < 1)
			{
				return Project(p);
			}

			double dist0 = p0.Distance(p);
			double dist1 = p1.Distance(p);
			
            if (dist0 < dist1)
				return p0;
			
            return p1;
		}

		/// <summary> 
		/// Computes the closest points on two line segments.
		/// </summary>
		/// <param name="p">the point to find the closest point to
		/// </param>
		/// <returns> 
		/// a pair of Coordinates which are the closest points on the line segments
		/// </returns>
		public Coordinate[] ClosestPoints(LineSegment line)
		{
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            // test for intersection
			Coordinate intPt = Intersection(line);
			if (intPt != null)
			{
				return new Coordinate[]{intPt, intPt};
			}
			
			// if no intersection closest pair Contains at least one endpoint.
			// Test each endpoint in turn.
			Coordinate[] closestPt = new Coordinate[2];

            double minDistance     = Double.MaxValue;
			double dist;
			
			Coordinate close00 = ClosestPoint(line.p0);
			minDistance  = close00.Distance(line.p0);
			closestPt[0] = close00;
			closestPt[1] = line.p0;
			
			Coordinate close01 = ClosestPoint(line.p1);
			dist = close01.Distance(line.p1);
			if (dist < minDistance)
			{
				minDistance  = dist;
				closestPt[0] = close01;
				closestPt[1] = line.p1;
			}
			
			Coordinate close10 = line.ClosestPoint(p0);
			dist = close10.Distance(p0);
			if (dist < minDistance)
			{
				minDistance  = dist;
				closestPt[0] = p0;
				closestPt[1] = close10;
			}
			
			Coordinate close11 = line.ClosestPoint(p1);
			dist = close11.Distance(p1);
			if (dist < minDistance)
			{
				minDistance  = dist;
				closestPt[0] = p1;
				closestPt[1] = close11;
			}
			
			return closestPt;
		}
		
		/// <summary> 
		/// Computes an intersection point between two segments, if there is one.
		/// </summary>
		/// <param name="">line
		/// </param>
		/// <returns> an intersection point, or null if there is none
		/// </returns>
		/// <remarks>
		/// There may be 0, 1 or many intersection points between two segments.
		/// If there are 0, null is returned. If there is 1 or more, a single one
		/// is returned (chosen at the discretion of the algorithm).  If
		/// more information is required about the details of the intersection,
		/// the <see cref="RobustLineIntersector"/> class should be used.
		/// </remarks>
		public Coordinate Intersection(LineSegment line)
		{
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            LineIntersector li = new RobustLineIntersector();
			li.ComputeIntersection(p0, p1, line.p0, line.p1);
			if (li.HasIntersection)
				return li.GetIntersection(0);

			return null;
		}
		
		/// <summary>  
		/// Returns true if other has the same values for its points.
		/// </summary>
		/// <param name="other"> a LineSegment with which to do the comparison.
		/// </param>
		/// <returns>        true if other is a LineSegment
		/// with the same values for the x and y ordinates.
		/// </returns>
		public override bool Equals(object o)
		{
            LineSegment other = o as LineSegment;
			if (other == null)
			{
				return false;
			}

			return p0.Equals(other.p0) && p1.Equals(other.p1);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
		/// <summary>  
		/// Returns true if other is topologically equal to this <see cref="LineSegment"/> 
		/// (e.g. irrespective of orientation).
		/// </summary>
		/// <param name="other">A LineSegment with which to do the comparison.</param>
		/// <returns>
		/// true if other is a LineSegment with the same values for the x and y ordinates.
		/// </returns>
		public bool EqualsTopologically(LineSegment other)
		{
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return p0.Equals(other.p0) && p1.Equals(other.p1) || 
                p0.Equals(other.p1) && p1.Equals(other.p0);
		}
		
		public override string ToString()
		{
			return "LINESTRING(" + p0.X + " " + p0.Y + ", " + p1.X + " " + p1.Y + ")";
		}


        public bool Touches(LineSegment segment, Envelope env) 
        {
            if (segment == null)
            {
                throw new ArgumentNullException("segment");
            }

            return Touches(segment.p0, segment.p1, env);
        }

        public bool Touches(Coordinate p0, Coordinate p1, Envelope env) 
        {
            if (env == null)
            {
                throw new ArgumentNullException("env");
            }

            Envelope lineEnv = new Envelope(p0, p1);

            if (!lineEnv.Intersects(env)) 
            {
                return false;
            }

            // now test either endpoint is inside
            if (env.Contains(p0)) 
            {
                return true;
            }

            if (env.Contains(p1)) 
            {
                return true;
            }

            // test whether the segment intersects any of the envelope sides
            // the coordinates of the envelope, in CW order
            Coordinate env0 = new Coordinate(env.MinX, env.MinY);
            Coordinate env1 = new Coordinate(env.MinX, env.MaxY);
            Coordinate env2 = new Coordinate(env.MaxX, env.MaxY);
            Coordinate env3 = new Coordinate(env.MaxX, env.MinY);

            lineInt.ComputeIntersection(p0, p1, env0, env1);

            if (lineInt.HasIntersection) 
            {
                return true;
            }

            lineInt.ComputeIntersection(p0, p1, env1, env2);

            if (lineInt.HasIntersection) 
            {
                return true;
            }

            lineInt.ComputeIntersection(p0, p1, env2, env3);

            if (lineInt.HasIntersection) 
            {
                return true;
            }

            lineInt.ComputeIntersection(p0, p1, env3, env0);

            if (lineInt.HasIntersection) 
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Projects one line segment onto another and returns the resulting
        /// line segment.
        /// The returned line segment will be a subset of
        /// the target line line segment.  This subset may be null, if
        /// the segments are oriented in such a way that there is no projection.
        /// </summary>
        /// <param name="target">the line segment to be projected onto</param>
        /// <param name="segment">the line segment to project</param>
        /// <returns>
        /// The projected line segment, or <see langword="null"/> if there is 
        /// no overlap.
        /// </returns>
        public static LineSegment Project(LineSegment target, LineSegment segment) 
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (segment == null)
            {
                throw new ArgumentNullException("segment");
            }

            double pf0 = target.ProjectionFactor(segment.p0);
            double pf1 = target.ProjectionFactor(segment.p1);

            // check if segment projects at all
            if ((pf0 >= 1.0) && (pf1 >= 1.0)) 
            {
                return null;
            }

            if ((pf0 <= 0.0) && (pf1 <= 0.0)) 
            {
                return null;
            }

            Coordinate newp0 = target.Project(segment.p0);

            if (pf0 < 0.0) 
            {
                newp0 = target.p0;
            }

            if (pf0 > 1.0) 
            {
                newp0 = target.p1;
            }

            Coordinate newp1 = target.Project(segment.p1);

            if (pf1 < 0.0) 
            {
                newp1 = target.p0;
            }

            if (pf1 > 1.0) 
            {
                newp1 = target.p1;
            }

            return new LineSegment(target.Factory, newp0, newp1);
        }

        /// <summary>
        /// Computes the Hausdorff distance between two LineSegments.
        /// To compute the Hausdorff distance, it is sufficient to compute
        /// the distance from one segment's endpoints to the other segment
        /// and choose the maximum.
        /// </summary>
        /// <param name="segment0"></param>
        /// <param name="segment1"></param>
        /// <returns>the Hausdorff distance between the segments</returns>
        public static double HausdorffDistance(LineSegment segment0, 
            LineSegment segment1) 
        {
            if (segment0 == null)
            {
                throw new ArgumentNullException("segment0");
            }
            if (segment1 == null)
            {
                throw new ArgumentNullException("segment1");
            }

            double hausdorffDist = segment0.Distance(segment1.p0);
            double dist;
            dist = segment0.Distance(segment1.p1);

            if (dist > hausdorffDist) 
            {
                hausdorffDist = dist;
            }

            dist = segment1.Distance(segment0.p0);

            if (dist > hausdorffDist) 
            {
                hausdorffDist = dist;
            }

            dist = segment1.Distance(segment0.p1);

            if (dist > hausdorffDist) 
            {
                hausdorffDist = dist;
            }

            return hausdorffDist;
        }

        /// <summary>
        /// Converts a LineSegment to a LineString.
        /// </summary>
        /// <param name="factory">a factory used to create the LineString</param>
        /// <param name="segment">the LineSegment to convert</param>
        /// <returns>a new LineString based on the segment</returns>
        public static LineString ToLineString(GeometryFactory factory, 
            LineSegment segment) 
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (segment == null)
            {
                throw new ArgumentNullException("segment");
            }

            Coordinate[] coord = 
            { 
                new Coordinate(segment.p0), 
                new Coordinate(segment.p1) 
            };

            LineString line = factory.CreateLineString(coord);

            return line;
        }

        #endregion

        #region IGeometry Members

        public override GeometryType GeometryType
        {
            get
            {
                return GeometryType.LineSegment;
            }
        }

        public override string Name
        {
            get
            {
                return "LineSegment";
            }
        }

        public override bool IsEmpty
        {
            get
            {
                // TODO:  Add LineSegment.IsEmpty getter implementation
                return false;
            }
        }

        public override bool IsSimple
        {
            get
            {
                // TODO:  Add LineSegment.IsSimple getter implementation
                return false;
            }
        }

        public override Geometry Boundary
        {
            get
            {
                // TODO:  Add LineSegment.Boundary getter implementation
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
            // TODO:  Add LineSegment.iGeospatial.Geometries.IGeometry.Equals implementation
            return false;
        }

        public override bool Disjoint(Geometry otherGeometry)
        {
            // TODO:  Add LineSegment.Disjoint implementation
            return false;
        }

        public override bool Intersects(Geometry otherGeometry)
        {
            // TODO:  Add LineSegment.Intersects implementation
            return false;
        }

        public override bool Touches(Geometry otherGeometry)
        {
            // TODO:  Add LineSegment.iGeospatial.Geometries.IGeometry.Touches implementation
            return false;
        }

        public override bool Crosses(Geometry otherGeometry)
        {
            // TODO:  Add LineSegment.Crosses implementation
            return false;
        }

        public override bool Within(Geometry otherGeometry)
        {
            // TODO:  Add LineSegment.Within implementation
            return false;
        }

        public override bool Contains(Geometry otherGeometry)
        {
            // TODO:  Add LineSegment.Contains implementation
            return false;
        }

        public override bool Overlaps(Geometry otherGeometry)
        {
            // TODO:  Add LineSegment.Overlaps implementation
            return false;
        }

        public override bool Relate(Geometry otherGeometry, String intersectionPattern)
        {
            // TODO:  Add LineSegment.Relate implementation
            return false;
        }

        public override IntersectionMatrix Relate(Geometry otherGeometry)
        {
            // TODO:  Add LineSegment.Relate implementation
            return null;
        }

        public override Geometry Buffer(double distance)
        {
            // TODO:  Add LineSegment.Buffer implementation
            return null;
        }

        public override Geometry ConvexHull()
        {
            // TODO:  Add LineSegment.ConvexHull implementation
            return null;
        }

        public override Geometry Intersection(Geometry otherGeometry)
        {
            // TODO:  Add LineSegment.Intersection implementation
            return null;
        }

        public override Geometry Union(Geometry otherGeometry)
        {
            // TODO:  Add LineSegment.Union implementation
            return null;
        }

        public override Geometry Difference(Geometry otherGeometry)
        {
            // TODO:  Add LineSegment.Difference implementation
            return null;
        }

        public override Geometry SymmetricDifference(Geometry otherGeometry)
        {
            // TODO:  Add LineSegment.SymmetricDifference implementation
            return null;
        }

        public override bool IsWithinDistance(Geometry geom, double distance)
        {
            // TODO:  Add LineSegment.IsWithinDistance implementation
            return false;
        }

        #endregion

        #region ICloneable Members

        public override Geometry Clone()
        {
            // TODO:  Add LineSegment.Clone implementation
            return null;
        }

        #endregion

        #region IComparable Members

        /// <summary>  
        /// Compares this object with the specified object for order.
        /// Uses the standard lexicographic ordering for the points in the LineSegment.
        /// </summary>
        /// <param name="o">The LineSegment with which this LineSegment
        /// is being compared
        /// </param>
        /// <returns>A negative integer, zero, or a positive integer as this LineSegment
        /// is less than, equal to, or greater than the specified LineSegment
        /// </returns>
        public override int CompareTo(object o)
        {
            LineSegment other = (LineSegment) o;
            int comp0 = p0.CompareTo(other.p0);
            if (comp0 != 0)
                return comp0;

            return p1.CompareTo(other.p1);
        }

        #endregion

        #region IGeometryExtension Members

        /// <summary> Computes the length of the line segment.</summary>
        /// <returns> the length of the line segment
        /// </returns>
        public override double Length
        {
            get
            {
                return p0.Distance(p1);
            }
        }

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
                // TODO:  Add LineSegment.Coordinates getter implementation
                return null;
            }
        }

        public override int NumPoints
        {
            get
            {
                // TODO:  Add LineSegment.NumPoints getter implementation
                return 0;
            }
        }

        public override double Area
        {
            get
            {
                // TODO:  Add LineSegment.Area getter implementation
                return 0;
            }
        }

        public override Point Centroid
        {
            get
            {
                // TODO:  Add LineSegment.Centroid getter implementation
                return null;
            }
        }

        public override Point InteriorPoint
        {
            get
            {
                // TODO:  Add LineSegment.InteriorPoint getter implementation
                return null;
            }
        }

        public override void Apply(ICoordinateVisitor filter)
        {
            // TODO:  Add LineSegment.Apply implementation
        }

        public override void Apply(IGeometryComponentVisitor filter)
        {
            // TODO:  Add LineSegment.Apply implementation
        }

        public override void Apply(IGeometryVisitor filter)
        {
            // TODO:  Add LineSegment.Apply implementation
        }

        public override bool EqualsExact(Geometry other, double tolerance)
        {
            // TODO:  Add LineSegment.EqualsExact implementation
            return false;
        }

        public override Coordinate Coordinate
        {
            get
            {
                return p0;
            }
        }

        public override bool EqualsExact(Geometry other)
        {
            // TODO:  Add LineSegment.EqualsExact implementation
            return false;
        }

        public override void Changed()
        {
            // TODO:  Add LineSegment.GeometryChanged implementation
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

        #endregion
    }
}