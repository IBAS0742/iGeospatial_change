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
	/// Non-robust versions of various fundamental Computational Geometric algorithms.
	/// The non-robustness is due to rounding error in floating point computation.
	/// </summary>
	public sealed class NonRobustCGAlgorithms
	{
		public NonRobustCGAlgorithms()
		{
		}
		
		/// <summary> ring is expected to contain a closing point;
		/// i.e. ring[0] = ring[length - 1]
		/// </summary>
		public bool IsPointInRing(Coordinate p, ICoordinateList ring)
		{
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }
            if (ring == null)
            {
                throw new ArgumentNullException("ring");
            }

            int i, i1; // point index; i1 = i-1 mod n
			double xInt; // x intersection of e with ray
			int crossings = 0; // number of edge/ray crossings
			double x1, y1, x2, y2;
			int nPts = ring.Count;
			
			/* For each line edge l = (i-1, i), see if it Crosses ray from test point in positive x direction. */
			for (i = 1; i < nPts; i++)
			{
				i1 = i - 1;
				Coordinate p1 = ring[i];
				Coordinate p2 = ring[i1];
				x1 = p1.X - p.X;
				y1 = p1.Y - p.Y;
				x2 = p2.X - p.X;
				y2 = p2.Y - p.Y;
				
				if (((y1 > 0) && (y2 <= 0)) || ((y2 > 0) && (y1 <= 0)))
				{
					/* e straddles x axis, so compute intersection. */
					xInt = (x1 * y2 - x2 * y1) / (y2 - y1);
					//xsave = xInt;
					/* Crosses ray if strictly positive intersection. */
					if (0.0 < xInt)
						crossings++;
				}
			}
			/* p is inside if an odd number of crossings. */
			if ((crossings % 2) == 1)
				return true;
			else
				return false;
		}

		public bool IsOnLine(Coordinate p, ICoordinateList pts)
		{
            if (pts == null)
            {
                throw new ArgumentNullException("pts");
            }

            LineIntersector li = new RobustLineIntersector();
			for (int i = 1; i < pts.Count; i++)
			{
				Coordinate p0 = pts[i - 1];
				Coordinate p1 = pts[i];
				li.ComputeIntersection(p, p0, p1);
				if (li.HasIntersection)
					return true;
			}
			return false;
		}
		
		public bool IsCCW(ICoordinateList ring)
		{
            if (ring == null)
            {
                throw new ArgumentNullException("ring");
            }

            Coordinate hip, p, prev, next;
			int hii, i;
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
				iPrev = nPts - 2;
			int iNext = hii + 1;
			if (iNext >= nPts)
				iNext = 1;
			prev = ring[iPrev];
			next = ring[iNext];
			// translate so that hip is at the origin.
			// This will not affect the area calculation, and will avoid
			// finite-accuracy errors (i.e very small vectors with very large coordinates)
			// This also simplifies the discriminant calculation.
			double prev2x = prev.X - hip.X;
			double prev2y = prev.Y - hip.Y;
			double next2x = next.X - hip.X;
			double next2y = next.Y - hip.Y;
			// compute cross-product of vectors hip->next and hip->prev
			// (e.g. area of parallelogram they enclose)
			double disc = next2x * prev2y - next2y * prev2x;
			/* If disc is exactly 0, lines are collinear.  There are two possible cases:
			(1) the lines lie along the x axis in opposite directions
			(2) the line lie on top of one another
			(2) should never happen, so we're going to ignore it!
			(Might want to assert this)
			(1) is handled by checking if next is left of prev ==> CCW
			*/
			if (disc == 0.0)
			{
				// poly is CCW if prev x is right of next x
				return (prev.X > next.X);
			}
			else
			{
				// if area is positive, points are ordered CCW
				return (disc > 0.0);
			}
		}
		
		public OrientationType ComputeOrientation(Coordinate p1, 
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

            double dx1 = p2.X - p1.X;
			double dy1 = p2.Y - p1.Y;
			double dx2 = q.X - p2.X;
			double dy2 = q.Y - p2.Y;
			double det = dx1 * dy2 - dx2 * dy1;

			if (det > 0.0)
				return OrientationType.CounterClockwise;
			
            if (det < 0.0)
				return OrientationType.Clockwise;

			return OrientationType.Collinear;
		}
	}
}