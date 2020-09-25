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

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary> 
	/// A non-robust implementation of the <see cref="LineIntersector"/>, lines intersection
	/// determination abstract class.
	/// </summary>
    [Serializable]
    public sealed class NonRobustLineIntersector : LineIntersector
	{
        #region Constructors and Destructor
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NonRobustLineIntersector"/> class.
        /// </summary>
        public NonRobustLineIntersector()
        {
        }
        
        #endregion
		
        #region Public Methods

        /// <summary>
        /// Compute the intersection of a point p and the line p1-p2.
        /// </summary>
        /// <param name="p">The test point.</param>
        /// <param name="p1">The starting coordinate of the line.</param>
        /// <param name="p2">The ending coordinate of the line.</param>
        /// <remarks>
        /// This function computes the boolean value of the HasIntersection test.
        /// The actual value of the intersection (if there is one)
        /// is equal to the value of p.
        /// </remarks>
        public override void  ComputeIntersection(Coordinate p, 
            Coordinate p1, Coordinate p2)
		{
            if (p1 == null)
            {
                throw new ArgumentNullException("p1");
            }
            if (p2 == null)
            {
                throw new ArgumentNullException("p2");
            }
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }

            double a1;
			double b1;
			double c1;
			/*
			*  Coefficients of line eqns.
			*/
			double r;
			/*
			*  'Sign' values
			*/
			m_bIsProper = false;
			
			/*
			*  Compute a1, b1, c1, where line joining points 1 and 2
			*  is "a1 x  +  b1 y  +  c1  =  0".
			*/
			a1 = p2.Y - p1.Y;
			b1 = p1.X - p2.X;
			c1 = p2.X * p1.Y - p1.X * p2.Y;
			
			/*
			*  Compute r3 and r4.
			*/
			r = a1 * p.X + b1 * p.Y + c1;
			
			// if r != 0 the point does not lie on the line
			if (r != 0)
			{
				result = (int)IntersectState.DonotIntersect;
				return ;
			}
			
			// Point lies on line - check to see whether it lies in line segment.
			
			double dist = rParameter(p1, p2, p);
			if (dist < 0.0 || dist > 1.0)
			{
				result = (int)IntersectState.DonotIntersect;
				return ;
			}
			
			m_bIsProper = true;
			if (p.Equals(p1) || p.Equals(p2))
			{
				m_bIsProper = false;
			}
			result = (int)IntersectState.DoIntersect;
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
        protected override int ComputeIntersect(Coordinate p1, Coordinate p2, 
            Coordinate p3, Coordinate p4)
		{
            if (p1 == null)
            {
                throw new ArgumentNullException("p1");
            }
            if (p2 == null)
            {
                throw new ArgumentNullException("p2");
            }
            if (p3 == null)
            {
                throw new ArgumentNullException("p3");
            }
            if (p4 == null)
            {
                throw new ArgumentNullException("p4");
            }

            double a1;
			double b1;
			double c1;
			/*
			*  Coefficients of line eqns.
			*/
			double a2;
			/*
			*  Coefficients of line eqns.
			*/
			double b2;
			/*
			*  Coefficients of line eqns.
			*/
			double c2;
			double r1;
			double r2;
			double r3;
			double r4;
			/*
			*  'Sign' values
			*/
			//double denom, offset, num;     /* Intermediate values */
			
			m_bIsProper = false;
			
			/*
			*  Compute a1, b1, c1, where line joining points 1 and 2
			*  is "a1 x  +  b1 y  +  c1  =  0".
			*/
			a1 = p2.Y - p1.Y;
			b1 = p1.X - p2.X;
			c1 = p2.X * p1.Y - p1.X * p2.Y;
			
			/*
			*  Compute r3 and r4.
			*/
			r3 = a1 * p3.X + b1 * p3.Y + c1;
			r4 = a1 * p4.X + b1 * p4.Y + c1;
			
			/*
			*  Check signs of r3 and r4.  If both point 3 and point 4 lie on
			*  same side of line 1, the line segments do not intersect.
			*/
			if (r3 != 0 && r4 != 0 && IsSameSignAndNonZero(r3, r4))
			{
				return (int)IntersectState.DonotIntersect;
			}
			
			/*
			*  Compute a2, b2, c2
			*/
			a2 = p4.Y - p3.Y;
			b2 = p3.X - p4.X;
			c2 = p4.X * p3.Y - p3.X * p4.Y;
			
			/*
			*  Compute r1 and r2
			*/
			r1 = a2 * p1.X + b2 * p1.Y + c2;
			r2 = a2 * p2.X + b2 * p2.Y + c2;
			
			/*
			*  Check signs of r1 and r2.  If both point 1 and point 2 lie
			*  on same side of second line segment, the line segments do
			*  not intersect.
			*/
			if (r1 != 0 && r2 != 0 && IsSameSignAndNonZero(r1, r2))
			{
				return (int)IntersectState.DonotIntersect;
			}
			
			/// <summary>  Line segments intersect: compute intersection point.</summary>
			double denom = a1 * b2 - a2 * b1;
			if (denom == 0)
			{
				return ComputeCollinearIntersection(p1, p2, p3, p4);
			}
			double numX = b1 * c2 - b2 * c1;
			pa.X = numX / denom;
			/*
			*  TESTING ONLY
			*  double valX = (( num < 0 ? num - offset : num + offset ) / denom);
			*  double valXInt = (int) (( num < 0 ? num - offset : num + offset ) / denom);
			*  if (valXInt != pa.x)     // TESTING ONLY
			*  System.out.println(val + " - int: " + valInt + ", floor: " + pa.x);
			*/
			double numY = a2 * c1 - a1 * c2;
			pa.Y = numY / denom;
			
			// check if this is a proper intersection BEFORE truncating values,
			// to avoid spurious equality comparisons with endpoints
			m_bIsProper = true;
			if (pa.Equals(p1) || pa.Equals(p2) || pa.Equals(p3) || pa.Equals(p4))
			{
				m_bIsProper = false;
			}
			
			// truncate computed point to precision grid
			// TESTING - don't force coord to be precise
			if (m_objPrecisionModel != null)
			{
				pa.MakePrecise(m_objPrecisionModel);
			}

			return (int)IntersectState.DoIntersect;
		}
        
        #endregion
		
        #region Private Methods

		/// <returns> 
		/// true if both numbers are positive or if both numbers are negative.
		/// Returns false if both numbers are zero.
		/// </returns>
		private static bool IsSameSignAndNonZero(double a, double b)
		{
			if (a == 0 || b == 0)
			{
				return false;
			}

			return (a < 0 && b < 0) || (a > 0 && b > 0);
		}
		
		/*
		*  p1-p2  and p3-p4 are assumed to be collinear (although
		*  not necessarily intersecting). Returns:
		*  DONT_INTERSECT	: the two segments do not intersect
		*  COLLINEAR		: the segments intersect, in the
		*  line segment pa-pb.  pa-pb is in
		*  the same direction as p1-p2
		*  DO_INTERSECT		: the inputLines intersect in a single point
		*  only, pa
		*/
		private int ComputeCollinearIntersection(Coordinate p1, Coordinate p2, 
            Coordinate p3, Coordinate p4)
		{
			double r1;
			double r2;
			double r3;
			double r4;
			Coordinate q3;
			Coordinate q4;
			double t3;
			double t4;
			r1 = 0;
			r2 = 1;
			r3 = rParameter(p1, p2, p3);
			r4 = rParameter(p1, p2, p4);
			// make sure p3-p4 is in same direction as p1-p2
			if (r3 < r4)
			{
				q3 = p3;
				t3 = r3;
				q4 = p4;
				t4 = r4;
			}
			else
			{
				q3 = p4;
				t3 = r4;
				q4 = p3;
				t4 = r3;
			}
			// check for no intersection
			if (t3 > r2 || t4 < r1)
			{
				return (int)IntersectState.DonotIntersect;
			}
			
			// check for single point intersection
			if (q4 == p1)
			{
				pa.SetCoordinate(p1);
				return (int)IntersectState.DoIntersect;
			}
			if (q3 == p2)
			{
				pa.SetCoordinate(p2);
				return (int)IntersectState.DoIntersect;
			}
			
			// intersection MUST be a segment - compute endpoints
			pa.SetCoordinate(p1);
			if (t3 > r1)
			{
				pa.SetCoordinate(q3);
			}
			pb.SetCoordinate(p2);
			if (t4 < r2)
			{
				pb.SetCoordinate(q4);
			}
			return (int)IntersectState.Collinear;
		}
		
		/// <summary>  RParameter computes the parameter for the point p
		/// in the parameterized equation
		/// of the line from p1 to p2.
		/// This is equal to the 'Distance' of p along p1-p2
		/// </summary>
		private double rParameter(Coordinate p1, Coordinate p2, Coordinate p)
		{
			double r;
			// compute maximum delta, for numerical stability
			// also handle case of p1-p2 being vertical or horizontal
			double dx = Math.Abs(p2.X - p1.X);
			double dy = Math.Abs(p2.Y - p1.Y);
			if (dx > dy)
			{
				r = (p.X - p1.X) / (p2.X - p1.X);
			}
			else
			{
				r = (p.Y - p1.Y) / (p2.Y - p1.Y);
			}
			return r;
		}

        #endregion
	}
}