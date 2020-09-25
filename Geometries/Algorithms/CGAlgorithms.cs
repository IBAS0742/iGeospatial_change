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

using iGeospatial.Geometries;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary> 
	/// This class provides various fundamental computational geometric 
	/// algorithms (CGA) for working with the geometries.
	/// </summary>
    public sealed class CGAlgorithms
	{
        #region Constructors and Destructor
		
        private CGAlgorithms()
        {
        }
        
        #endregion
		
        #region Public Static Methods

        /// <summary> 
        /// This method returns the index of the direction of the point 
        /// <c>q</c> relative to a vector specified by <c>p1-p2</c>. 
        /// </summary>
        /// <param name="p1">The origin point of the vector.</param>
        /// <param name="p2">The final point of the vector.</param>
        /// <param name="q">The point to compute the direction to.</param>
        /// <returns>
        /// The returned value can be interpreted as follows:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// 1 if q is counter-clockwise (left) from p1-p2
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// -1 if q is clockwise (right) from p1-p2
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// 0 if q is collinear with p1-p2
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public static int OrientationIndex(Coordinate p1, 
            Coordinate p2, Coordinate q)
        {
            if (p1 == null)
            {
                throw new ArgumentNullException("p1");
            }
            if (p2 == null)
            {
                throw new ArgumentNullException("p2");
            }
            if (q == null)
            {
                throw new ArgumentNullException("q");
            }

            // travelling along p1->p2, turn counter clockwise to get to q return 1,
            // travelling along p1->p2, turn clockwise to get to q return -1,
            // p1, p2 and q are colinear return 0.
            double dx1 = p2.X - p1.X;
            double dy1 = p2.Y - p1.Y;
            double dx2 = q.X - p2.X;
            double dy2 = q.Y - p2.Y;

            return RobustDeterminant.SignOfDeterminant(dx1, dy1, dx2, dy2);
        }
		
        /// <summary>
        /// Test whether a point lies on the line segments defined by a
        /// list of coordinates.
        /// </summary>
        /// <param name="p">
        /// The coordinate of the point to be tested.
        /// </param>
        /// <param name="pts">
        /// The array of coordinates of points defining the line segments.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the point is a vertex of the line or lies in 
        /// the interior of a line segment in the linestring.
        /// </returns>
        public static bool IsOnLine(Coordinate p, ICoordinateList pts)
        {
            if (pts == null)
            {
                throw new ArgumentNullException("pts");
            }

            int nCount = pts.Count;
            LineIntersector lineIntersector = new RobustLineIntersector();

            for (int i = 1; i < nCount; i++)
            {
                Coordinate p0 = pts[i - 1];
                Coordinate p1 = pts[i];
                lineIntersector.ComputeIntersection(p, p0, p1);
                if (lineIntersector.HasIntersection)
                {
                    return true;
                }
            }
            return false;
        }
		
        /// <summary>
        /// The computes the orientation (clockwise, counter-clockwise or 
        /// collinear of a point <c>q</c> to the directed line segment <c>p1-p2</c>.
        /// The orientation of a point relative to a directed line segment indicates
        /// which way you turn to get to q after travelling from p1 to p2.
        /// </summary>
        /// <param name="p1">The coordinate start point.</param>
        /// <param name="p2">The coordinate end point.</param>
        /// <param name="q">The coordinate of the point to be tested.</param>
        /// <returns>
        /// An enumeration, <see cref="OrientationType"/>, specifying the
        /// orientation of given test point. 
        /// </returns>
        public static OrientationType ComputeOrientation(Coordinate p1, 
            Coordinate p2, Coordinate q)
        {
            return (OrientationType)OrientationIndex(p1, p2, q);
        }
		
        /// <summary> 
        /// Test whether a point lies inside a ring.
        /// </summary>
        /// <remarks>
        /// The ring may be oriented in either direction.
        /// <para>
        /// If the point lies on the ring boundary the result of this 
        /// method is unspecified.
        /// </para>
        /// This algorithm does not attempt to first check the point against 
        /// the envelope of the ring. 
        /// </remarks>
        /// <param name="p">
        /// The coordinate of the point to check for ring inclusion.
        /// </param>
        /// <param name="ring">
        /// The array of coordinates defining the ring, which is assumed to 
        /// have first point identical to last point.
        /// </param>
        public static bool IsPointInRing(Coordinate p, ICoordinateList ring) 
        {
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }
            if (ring == null)
            {
                throw new ArgumentNullException("ring");
            }

            // For each segment l = (i-1, i), see if it crosses ray from 
            // test point in positive x direction.
            int crossings = 0;  // number of segment/ray crossings
            int nCount    = ring.Count;

            for (int i = 1; i < nCount; i++) 
            {
                int i1 = i - 1;
                Coordinate p1 = ring[i];
                Coordinate p2 = ring[i1];

                if (((p1.Y > p.Y) && (p2.Y <= p.Y)) ||
                    ((p2.Y > p.Y) && (p1.Y <= p.Y))) 
                {
                    double x1 = p1.X - p.X;
                    double y1 = p1.Y - p.Y;
                    double x2 = p2.X - p.X;
                    double y2 = p2.Y - p.Y;
                    //  segment straddles x axis, so compute intersection with x-axis.
                    double xInt = RobustDeterminant.SignOfDeterminant(
                        x1, y1, x2, y2) / (y2 - y1);
                    //xsave = xInt;
                    //  crosses ray if strictly positive intersection.
                    if (xInt > 0.0) 
                    {
                        crossings++;
                    }
                }
            }
            
            //  p is inside if number of crossings is odd.
            return ((crossings % 2) == 1);
        }		
		
        /// <summary> 
        /// Computes whether a ring defined by an array of 
        /// <see cref="Coordinate"/> is oriented counter-clockwise.
        /// </summary>
        /// <remarks>
        /// When using this method, the following points must be noted:
        /// <list type="number">
        /// <item>
        /// <description>
        /// The list of points is assumed to have the first and last 
        /// points equal.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// This will handle coordinate lists which contain repeated points.
        /// </description>
        /// </item>
        /// </list>
        /// This algorithm is <b>only</b> guaranteed to work with valid rings.
        /// If the ring is invalid (e.g. self-crosses or touches),
        /// the computed result <b>may</b> not be correct.
        /// </remarks>
        /// <param name="ring">
        /// An array of coordinates forming a ring.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the ring is oriented counter-clockwise.
        /// </returns>
        public static bool IsCCW(ICoordinateList ring)
        {
            if (ring == null)
            {
                throw new ArgumentNullException("ring");
            }

            // number of points without closing endpoint
            int nPts = ring.Count - 1;

            // find highest point
            Coordinate hiPt = ring[0];
            int hiIndex = 0;
            for (int i = 1; i <= nPts; i++) 
            {
                Coordinate p = ring[i];
                if (p.Y > hiPt.Y) 
                {
                    hiPt = p;
                    hiIndex = i;
                }
            }

            // find distinct point before highest point
            int iPrev = hiIndex;
            do 
            {
                iPrev = iPrev - 1;
                if (iPrev < 0) iPrev = nPts;
            } while (ring[iPrev].Equals(hiPt) && iPrev != hiIndex);

            // find distinct point after highest point
            int iNext = hiIndex;
            do 
            {
                iNext = (iNext + 1) % nPts;
            } while (ring[iNext].Equals(hiPt) && iNext != hiIndex);

            Coordinate prev = ring[iPrev];
            Coordinate next = ring[iNext];

            // This check catches cases where the ring contains an A-B-A configuration of points.
            // This can happen if the ring does not contain 3 distinct points
            // (including the case where the input array has fewer than 4 elements),
            // or it contains coincident line segments.
            if (prev.Equals(hiPt) || next.Equals(hiPt) || prev.Equals(next))
            {
                return false;
            }

            int disc = (int)ComputeOrientation(prev, hiPt, next);

            // If disc is exactly 0, lines are collinear.  There are two possible cases:
            // (1) the lines lie along the x axis in opposite directions
            // (2) the lines lie on top of one another
            //
            // (1) is handled by checking if next is left of prev ==> CCW
            // (2) will never happen if the ring is valid, so don't check for it
            // (Might want to assert this)
            bool isCCW = false;
            if (disc == 0) 
            {
                // poly is CCW if prev x is right of next x
                isCCW = (prev.X > next.X);
            }
            else 
            {
                // if area is positive, points are ordered CCW
                isCCW = (disc > 0);
            }

            return isCCW;
        }
		
        /// <summary> 
        /// Computes whether a ring defined by an array of Coordinate is
        /// oriented counter-clockwise.
        /// </summary>
        /// <remarks>
        /// This algorithm is valid only for coordinate lists which do not 
        /// contain repeated points.
        /// </remarks>
        /// <param name="ring">
        /// An array of coordinates forming a ring.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the ring is oriented counter-clockwise.
        /// </returns>
        public static bool IsCCWUnique(ICoordinateList ring)
        {
            if (ring == null)
            {
                throw new ArgumentNullException("ring");
            }

            Coordinate hip;
            Coordinate p;
            Coordinate prev;
            Coordinate next;
            int hii;
            int i;
            int nPts = ring.Count;
			
            // check that this is a valid ring - if not, simply return a dummy value
            if (nPts < 4)
                return false;
			
            // algorithm to check if a Ring is stored in CCW order
            // find highest point
            hip = ring[0];
            hii = 0;
            for (i = 1; i < nPts; i++)
            {
                p = ring[i];
                if (p.Y > hip.Y)
                {
                    hip = p;
                    hii = i;
                }
            }

            // find points on either side of highest
            int iPrev = hii - 1;
            if (iPrev < 0)
            {
                iPrev = nPts - 2;
            }
            int iNext = hii + 1;
            if (iNext >= nPts)
            {
                iNext = 1;
            }

            prev = ring[iPrev];
            next = ring[iNext];
            int disc = (int)ComputeOrientation(prev, hip, next);

            //  If disc is exactly 0, lines are collinear.  There are two possible cases:
            //  (1) the lines lie along the x axis in opposite directions
            //  (2) the line lie on top of one another
            //  (2) should never happen, so we're going to ignore it!
            //  (Might want to assert this)
            //  (1) is handled by checking if next is left of prev ==> CCW
            //
            if (disc == 0)
            {
                // poly is CCW if prev x is right of next x
                return (prev.X > next.X);
            }
            else
            {
                // if area is positive, points are ordered CCW
                return (disc > 0);
            }
        }

		/// <summary> 
		/// Computes the Distance from a point p to a line segment AB.
		/// </summary>
		/// <param name="p">the point to compute the Distance for
		/// </param>
		/// <param name="A">one point of the line
		/// </param>
		/// <param name="B">another point of the line (must be different to A)
		/// </param>
		/// <returns> the Distance from p to line segment AB
		/// </returns>
		/// <remarks>
		/// <note>This is non-robust.</note>
		/// </remarks>
		public static double DistancePointLine(Coordinate p, 
            Coordinate A, Coordinate B)
		{          
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }
            if (A == null)
            {
                throw new ArgumentNullException("A");
            }
            if (B == null)
            {
                throw new ArgumentNullException("B");
            }

            // if start==end, then use pt Distance
			if (A.Equals(B))
				return p.Distance(A);
			
			// otherwise use comp.graphics.algorithms Frequently Asked Questions method
			/*(1)     	      AC dot AB
			r = ---------
			||AB||^2
			r has the following meaning:
			r=0 P = A
			r=1 P = B
			r<0 P is on the backward extension of AB
			r>1 P is on the forward extension of AB
			0<r<1 P is interior to AB
			*/
			
			double r = ((p.X - A.X) * (B.X - A.X) + (p.Y - A.Y) * (B.Y - A.Y)) / 
                ((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));
			
			if (r <= 0.0)
				return p.Distance(A);
			if (r >= 1.0)
				return p.Distance(B);
			
			
			/*(2)
			(Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
			s = -----------------------------
			L^2
			
			Then the Distance from C to P = |s|*L.
			*/
			
			double s = ((A.Y - p.Y) * (B.X - A.X) - (A.X - p.X) * (B.Y - A.Y)) / 
                ((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));
			
			return Math.Abs(s) * Math.Sqrt(((B.X - A.X) * (B.X - A.X) + 
                (B.Y - A.Y) * (B.Y - A.Y)));
		}

		/// <summary> 
		/// Computes the perpendicular distance from a point p
		/// to the (infinite) line containing the points AB. 
		/// </summary>
		/// <param name="p">The point to compute the Distance for.</param>
		/// <param name="A">One point of the line.</param>
		/// <param name="B">
		/// Another point of the line (must be different to A).
		/// </param>
		/// <returns>The Distance from p to line AB.</returns>
		public static double DistancePointLinePerpendicular(Coordinate p, 
            Coordinate A, Coordinate B)
		{
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }
            if (A == null)
            {
                throw new ArgumentNullException("A");
            }
            if (B == null)
            {
                throw new ArgumentNullException("B");
            }

            // use comp.graphics.algorithms Frequently Asked Questions method
			/*(2)
			(Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
			s = -----------------------------
			L^2
			
			Then the Distance from C to P = |s|*L.
			*/
			
			double s = ((A.Y - p.Y) * (B.X - A.X) - (A.X - p.X) * (B.Y - A.Y)) / 
                ((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));
			
			return Math.Abs(s) * Math.Sqrt(((B.X - A.X) * (B.X - A.X) + 
                (B.Y - A.Y) * (B.Y - A.Y)));
		}
		
		/// <summary> 
		/// Computes the distance from a line segment AB to a 
		/// line segment CD.
		/// </summary>
		/// <param name="A">A point of one line.
		/// </param>
		/// <param name="B">
		/// The second point of  (must be different to A).</param>
		/// <param name="C">One point of the line.</param>
		/// <param name="D">
		/// Another point of the line (must be different to A).
		/// </param>
        /// <remarks>
        /// <note>This is non-robust.</note>
        /// </remarks>
        public static double DistanceLineLine(Coordinate A, Coordinate B, 
            Coordinate C, Coordinate D)
		{
            if (A == null)
            {
                throw new ArgumentNullException("A");
            }
            if (B == null)
            {
                throw new ArgumentNullException("B");
            }
            if (C == null)
            {
                throw new ArgumentNullException("C");
            }
            if (D == null)
            {
                throw new ArgumentNullException("D");
            }

            // check for zero-length segments
			if (A.Equals(B))
				return DistancePointLine(A, C, D);
			if (C.Equals(D))
				return DistancePointLine(D, A, B);
			
			// AB and CD are line segments
			/* from comp.graphics.algo
			
			Solving the above for r and s yields
			(Ay-Cy)(Dx-Cx)-(Ax-Cx)(Dy-Cy)
			r = ----------------------------- (eqn 1)
			(Bx-Ax)(Dy-Cy)-(By-Ay)(Dx-Cx)
			
			(Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
			s = ----------------------------- (eqn 2)
			(Bx-Ax)(Dy-Cy)-(By-Ay)(Dx-Cx)
			Let P be the position vector of the intersection point, then
			P=A+r(B-A) or
			Px=Ax+r(Bx-Ax)
			Py=Ay+r(By-Ay)
			By examining the values of r & s, you can also determine some other
			limiting conditions:
			If 0<=r<=1 & 0<=s<=1, intersection exists
			r<0 or r>1 or s<0 or s>1 line segments do not intersect
			If the denominator in eqn 1 is zero, AB & CD are parallel
			If the numerator in eqn 1 is also zero, AB & CD are collinear.
			
			*/
			double r_top = (A.Y - C.Y) * (D.X - C.X) - (A.X - C.X) * (D.Y - C.Y);
			double r_bot = (B.X - A.X) * (D.Y - C.Y) - (B.Y - A.Y) * (D.X - C.X);
			
			double s_top = (A.Y - C.Y) * (B.X - A.X) - (A.X - C.X) * (B.Y - A.Y);
			double s_bot = (B.X - A.X) * (D.Y - C.Y) - (B.Y - A.Y) * (D.X - C.X);
			
			if ((r_bot == 0) || (s_bot == 0))
			{
				return Math.Min(DistancePointLine(A, C, D), 
                    Math.Min(DistancePointLine(B, C, D), 
                    Math.Min(DistancePointLine(C, A, B), 
                    DistancePointLine(D, A, B))));
			}
			double s = s_top / s_bot;
			double r = r_top / r_bot;
			
			if ((r < 0) || (r > 1) || (s < 0) || (s > 1))
			{
				//no intersection
				return Math.Min(DistancePointLine(A, C, D), 
                    Math.Min(DistancePointLine(B, C, D), 
                    Math.Min(DistancePointLine(C, A, B), 
                    DistancePointLine(D, A, B))));
			}

			return 0.0; //intersection exists
		}
		
		/// <summary> 
		/// Calculates the signed area for a ring.  
		/// </summary>
		/// <remarks>
		/// Note that the sign of the returned area can be determined using
		/// the <see cref="Math.Sign"/> method.
		/// </remarks>
		/// <param name="ring">
		/// The array of coordinates of the points defining the ring.
		/// </param>
		/// <returns>
		/// The signed area for a ring. The area is positive if the ring 
		/// is oriented clockwise.
		/// </returns>
		public static double SignedArea(ICoordinateList ring)
		{
            if (ring == null)
            {
                throw new ArgumentNullException("ring");
            }

            int nCount = ring.Count;
            if (nCount < 3)
				return 0.0;
			
            double sum = 0.0;

			for (int i = 0; i < nCount - 1; i++)
			{
				double bx = ring[i].X;
				double by = ring[i].Y;
				double cx = ring[i + 1].X;
				double cy = ring[i + 1].Y;
				sum += (bx + cx) * (cy - by);
			}

			return (-sum) / 2.0;
		}
		
		/// <summary> 
		/// Computes the length of a line string specified by a 
		/// sequence of points.
		/// </summary>
		/// <param name="pts">
		/// The points specifying the linestring.
		/// </param>
		/// <returns>The length of the linestring.</returns>
		public static double Length(ICoordinateList pts)
		{
            if (pts == null)
            {
                throw new ArgumentNullException("pts");
            }

            int nCount = pts.Count;
            if (nCount < 1)
				return 0.0;

			double sum = 0.0;
			
            for (int i = 1; i < nCount; i++)
			{
				sum += pts[i].Distance(pts[i - 1]);
			}
			
            return sum;
		}
        
        #endregion
	}
}