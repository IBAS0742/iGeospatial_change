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
using iGeospatial.Geometries.IO;

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary> 
	/// A LineIntersector is an algorithm that can both test whether
	/// two line segments intersect and compute the intersection point
	/// if they do.
	/// </summary>
	/// <remarks>
	/// The LineIntersector is an <see langword="abstract"/> base class
	/// providing helper methods for computing line intersections.
	/// <para>
	/// The intersection point may be computed in a precise or non-precise manner.
	/// Computing it precisely involves rounding it to an integer.  (This assumes
	/// that the input coordinates have been made precise by scaling them to
	/// an integer grid.)
	/// </para>
	/// </remarks>
	[Serializable]
    public abstract class LineIntersector   
    {
        #region Internal Fields
		
        internal int result;

        internal Coordinate[][] inputLines;
		
        internal Coordinate[] intPt;

		/// <summary> 
		/// The indexes of the endpoints of the intersection lines, in order along
		/// the corresponding line.
		/// </summary>
		internal int[][] intLineIndex;
		internal bool m_bIsProper;
		internal Coordinate pa;
		internal Coordinate pb;

		/// <summary> 
		/// If MakePrecise is true, computed intersection coordinates will be made precise
		/// using Coordinate#MakePrecise
		/// </summary>
		internal PrecisionModel m_objPrecisionModel;
		
        #endregion

        #region Constructors and Destructor
		
        protected LineIntersector()
		{
            intPt = new Coordinate[2];
            inputLines = new Coordinate[2][];
            for (int i = 0; i < 2; i++)
            {
                inputLines[i] = new Coordinate[2];
            }

			intPt[0] = new Coordinate();
			intPt[1] = new Coordinate();
			// alias the intersection points for ease of reference
			pa = intPt[0];
			pb = intPt[1];

            Initialize();
		}

        #endregion
		
        #region Public Properties

		/// <summary> 
		/// Force computed intersection to be rounded to a given precision model.
		/// No getter is provided, because the precision model is not required 
		/// to be specified.
		/// </summary>
		/// <value> A <see cref="iGeospatial.Coordinates.PrecisionModel"/> reference
		/// to the precision model with which computations are done.
		/// </value>
		public virtual PrecisionModel PrecisionModel
		{
            get
            {
                return this.m_objPrecisionModel;
            }

			set
			{
				this.m_objPrecisionModel = value;
			}
		}

        /// <summary> 
        /// Returns the number of intersection points found.  
        /// </summary>
        /// <value> This will be either 0, 1 or 2.</value>
		public virtual int IntersectionNum
		{
			get
			{
				return result;
			}
		}
		
        /// <summary> Tests whether an intersection is proper.
		/// </summary>
		/// <value> true if the intersection is proper. </value>
		/// <remarks>
		/// <para>
		/// The intersection between two line segments is considered proper if
		/// they intersect in a single point in the interior of both segments
		/// (e.g. the intersection is a single point and is not equal to any of the
		/// endpoints).
		/// </para>
		/// <para>
		/// The intersection between a point and a line segment is considered proper
		/// if the point lies in the interior of the segment (e.g. is not equal to
		/// either of the endpoints).
		/// </para>
		/// </remarks>
		public virtual bool Proper
		{
			get
			{
				return HasIntersection && m_bIsProper;
			}
		}
		
		/// <summary> Tests whether the input geometries intersect.</summary>
		/// <value> true if the input geometries intersect </value>
		public virtual bool HasIntersection
		{
            get
            {
                return result != (int)IntersectState.DonotIntersect;
            }
		}
		
        #endregion

        #region Internal Properties

        /// <summary>
        /// Indicates whether the intersection state of the lines is collinear or coaxial.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the intersection is collinear, <see langword="false"/> otherwise.
        /// </value>
		internal virtual bool Collinear
		{
			get
			{
				return result == (int)IntersectState.Collinear;
			}
		}
		
        /// <summary>
        /// Determines whether the intersection occurs at the end point or extreme points
        /// of the lines.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the intersection occurs at the end point, <see langword="false"/> otherwise.
        /// </value>
        internal virtual bool EndPoint
		{
			get
			{
				return HasIntersection && !m_bIsProper;
			}
		}

        #endregion
		
        #region Public Static Methods

		/// <summary> 
		/// Computes the "edge distance" of an intersection point along a segment.
		/// </summary>
		/// <remarks>
		/// The edge distance is a metric of the point along the edge.
		/// The metric used is a robust and easy to compute metric function.
		/// It is not equivalent to the usual Euclidean metric.
		/// It relies on the fact that either the x or the y ordinates of the
		/// points in the edge are unique, depending on whether the edge is longer in
		/// the horizontal or vertical direction.
		/// <para>
		/// NOTE: This function may produce incorrect distances
		/// for inputs where p is not precisely on p1-p2
		/// (E.g. p = (139,9) p1 = (139,10), p2 = (280,1) produces distanct 0.0, 
		/// which is incorrect.
		/// </para>
		/// My hypothesis is that the function is safe to use for points which are the
		/// result of rounding points which lie on the line,
		/// but not safe to use for truncated points.
		/// </remarks>
		public static double ComputeEdgeDistance(Coordinate p, 
            Coordinate p0, Coordinate p1)
		{
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }
            if (p0 == null)
            {
                throw new ArgumentNullException("p0");
            }
            if (p1 == null)
            {
                throw new ArgumentNullException("p1");
            }

            double dx = Math.Abs(p1.X - p0.X);
			double dy = Math.Abs(p1.Y - p0.Y);
			
			double dist = - 1.0; // sentinel value
			if (p.Equals(p0))
			{
				dist = 0.0;
			}
			else if (p.Equals(p1))
			{
				if (dx > dy)
					dist = dx;
				else
					dist = dy;
			}
			else
			{
				double pdx = Math.Abs(p.X - p0.X);
				double pdy = Math.Abs(p.Y - p0.Y);
				if (dx > dy)
					dist = pdx;
				else
					dist = pdy;
				// <FIX>
				// hack to ensure that non-endpoints always have a non-zero Distance
				if (dist == 0.0 && !p.Equals(p0))
				{
					dist = Math.Max(pdx, pdy);
				}
			}

			Debug.Assert(!(dist == 0.0 && !p.Equals(p0)), "Bad Distance calculation");
			return dist;
		}
		
		/// <summary> 
		/// This function is non-robust, since it may compute the square of large numbers.
		/// Currently not sure how to improve this.
		/// </summary>
		public static double NonRobustComputeEdgeDistance(Coordinate p, 
            Coordinate p1, Coordinate p2)
		{
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }
            if (p1 == null)
            {
                throw new ArgumentNullException("p1");
            }
            if (p2 == null)
            {
                throw new ArgumentNullException("p2");
            }

            double dx = p.X - p1.X;
			double dy = p.Y - p1.Y;
			double dist = Math.Sqrt(dx * dx + dy * dy); // dummy value
			Debug.Assert(!(dist == 0.0 && !p.Equals(p1)), "Invalid Distance calculation");

			return dist;
		}
		
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Compute the intersection of a point p and the line p1-p2.
        /// This function computes the boolean value of the HasIntersection test.
        /// The actual value of the intersection (if there is one)
        /// is equal to the value of p.
        /// </summary>
        /// <param name="p">The test point.</param>
        /// <param name="p1">The starting coordinate of the line.</param>
        /// <param name="p2">The ending coordinate of the line.</param>
 		/// <remarks>
		/// Notes to Inheritors:  You must implement this method in a derived class.
		/// </remarks>
        public abstract void ComputeIntersection(Coordinate p, Coordinate p1, Coordinate p2);
		
        /// <summary>
        /// Computes the intersection of the lines p1-p2 and p3-p4.
        /// This function computes both the boolean value of the HasIntersection test
        /// and the (approximate) value of the intersection point itself (if there is one).
        /// </summary>
        /// <param name="p1">The starting coordinate of first line.</param>
        /// <param name="p2">The ending coordinate of first line.</param>
        /// <param name="p3">The starting coordinate of seocnd line.</param>
        /// <param name="p4">The ending coordinate of second line.</param>
        public virtual void ComputeIntersection(Coordinate p1, Coordinate p2, 
            Coordinate p3, Coordinate p4)
		{
			inputLines[0][0] = p1;
			inputLines[0][1] = p2;
			inputLines[1][0] = p3;
			inputLines[1][1] = p4;

			result = ComputeIntersect(p1, p2, p3, p4);
			//numIntersects++;
		}
		
        /// <summary>
        /// A textual representation of the current instance of the 
        /// <see cref="LineIntersector"/> derived class.
        /// </summary>
        /// <returns>
        /// A string containing information on the current instance, 
        /// and its intersection state.
        /// </returns>
        public override string ToString() 
        {
            return GeometryWktWriter.ToLineString(inputLines[0][0], 
                inputLines[0][1]) + " - "
                + GeometryWktWriter.ToLineString(inputLines[1][0], 
                inputLines[1][1]) + GetTopologySummary();
        }

        private String GetTopologySummary()
        {
            StringBuilder catBuf = new StringBuilder();

            if (this.EndPoint) 
                catBuf.Append(" endpoint");

            if (this.Proper) 
                catBuf.Append(" proper");

            if (this.Collinear) 
                catBuf.Append(" collinear");

            return catBuf.ToString();
        }
		
//        public override string ToString()
//		{
//			string str = inputLines[0][0] + "-" + inputLines[0][1] + " " + 
//                inputLines[1][0] + "-" + inputLines[1][1] + " : ";
//
//			if (EndPoint)
//			{
//				str += " endpoint";
//			}
//
//			if (m_bIsProper)
//			{
//				str += " proper";
//			}
//			
//            if (Collinear)
//			{
//				str += " collinear";
//			}
//
//			return str;
//		}
		
		/// <summary> Returns the intIndex'th intersection point</summary>
		/// <param name="intIndex">is 0 or 1</param>
		/// <returns> the intIndex'th intersection point
		/// </returns>
		public virtual Coordinate GetIntersection(int intIndex)
		{
			return intPt[intIndex];
		}
		
		/// <summary> 
		/// Test whether a point is a intersection point of two line segments.
		/// Note that if the intersection is a line segment, this method only tests for
		/// equality with the endpoints of the intersection segment.
		/// It does not return true if the input point is internal to the 
		/// intersection segment.
		/// </summary>
		/// <returns> true if the input point is one of the intersection points.
		/// </returns>
		public virtual bool IsIntersection(Coordinate pt)
		{
			for (int i = 0; i < result; i++)
			{
				if (intPt[i].Equals(pt))
				{
					return true;
				}
			}
			return false;
		}
		
		/// <summary> 
		/// Tests whether either intersection point is an interior point of one of 
		/// the input segments.
		/// </summary>
		/// <returns> 
		/// true if either intersection point is in the interior of one of the 
		/// input segments.
		/// </returns>
		public virtual bool IsInteriorIntersection()
		{
			if (IsInteriorIntersection(0))
				return true;
			if (IsInteriorIntersection(1))
				return true;
			return false;
		}
		
		/// <summary> 
		/// Tests whether either intersection point is an interior point of the 
		/// specified input segment.
		/// </summary>
		/// <returns> 
		/// true if either intersection point is in the interior of the input segment.
		/// </returns>
		public virtual bool IsInteriorIntersection(int inputLineIndex)
		{
			for (int i = 0; i < result; i++)
			{
				if (!(intPt[i].Equals(inputLines[inputLineIndex][0]) || 
                    intPt[i].Equals(inputLines[inputLineIndex][1])))
				{
					return true;
				}
			}
			return false;
		}
		
		/// <summary> 
		/// Computes the intIndex'th intersection point in the direction of
		/// a specified input line segment
		/// </summary>
		/// <param name="segmentIndex">A segment index, which is either 0 or 1.</param>
		/// <param name="intIndex">A line index, which is either 0 or 1.</param>
		/// <returns> 
		/// The intIndex'th intersection point in the direction of the specified 
		/// input line segment.
		/// </returns>
		public virtual Coordinate GetIntersectionAlongSegment(int segmentIndex, int intIndex)
		{
			// lazily compute int line array
			ComputeIntLineIndex();

			return intPt[intLineIndex[segmentIndex][intIndex]];
		}
		
		/// <summary> 
		/// Computes the index of the intIndex'th intersection point in the direction of
		/// a specified input line segment
		/// </summary>
		/// <param name="segmentIndex">A line index, which is either is 0 or 1.</param>
		/// <param name="intIndex">A line index, which is either 0 or 1. </param>
		/// <returns> The index of the intersection point along the segment (0 or 1).
		/// </returns>
		public virtual int GetIndexAlongSegment(int segmentIndex, int intIndex)
		{
			ComputeIntLineIndex();

			return intLineIndex[segmentIndex][intIndex];
		}

		/// <summary> 
		/// Computes the "edge Distance" of an intersection point along the 
		/// specified input line segment. 
		/// </summary>
		/// <param name="segmentIndex">is 0 or 1
		/// </param>
		/// <param name="intIndex">is 0 or 1
		/// 
		/// </param>
		/// <returns> the edge Distance of the intersection point
		/// </returns>
		public virtual double GetEdgeDistance(int segmentIndex, int intIndex)
		{
			double dist = ComputeEdgeDistance(intPt[intIndex], 
                inputLines[segmentIndex][0], inputLines[segmentIndex][1]);

			return dist;
		}
		
        #endregion

        #region Protected Methods
		
        /// <summary>
        /// Computes the intersection of the lines p1-p2 and q1-q2.
        /// </summary>
        /// <param name="p1">The starting coordinate of first line.</param>
        /// <param name="p2">The ending coordinate of first line.</param>
        /// <param name="q1">The starting coordinate of seocnd line.</param>
        /// <param name="q2">The ending coordinate of second line.</param>
        /// <returns>
        /// A integer, which is one of the <see cref="IntersectState"/> values, 
        /// specifying the intersection state of the lines.
        /// </returns>
        /// <remarks>
        /// Notes to Inheritors:  You must implement this method in a derived class.
        /// </remarks>
        protected abstract int ComputeIntersect(Coordinate p1, Coordinate p2, 
            Coordinate q1, Coordinate q2);

        protected virtual void ComputeIntLineIndex()
        {
            if (intLineIndex == null)
            {
                intLineIndex = new int[2][];
                for (int i2 = 0; i2 < 2; i2++)
                {
                    intLineIndex[i2] = new int[2];
                }

                ComputeIntLineIndex(0);
                ComputeIntLineIndex(1);
            }
        }
		
		protected virtual void ComputeIntLineIndex(int segmentIndex)
		{
			double dist0 = GetEdgeDistance(segmentIndex, 0);
			double dist1 = GetEdgeDistance(segmentIndex, 1);

			if (dist0 > dist1)
			{
				intLineIndex[segmentIndex][0] = 0;
				intLineIndex[segmentIndex][1] = 1;
			}
			else
			{
				intLineIndex[segmentIndex][0] = 1;
				intLineIndex[segmentIndex][1] = 0;
			}
		}
		
        #endregion

        #region Private Methods

        private void Initialize()
        {
            result      = 0;
            m_bIsProper = false;
        }
        
        #endregion
	}
}