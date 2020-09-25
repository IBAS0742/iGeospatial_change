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
	/// Robust versions of various fundamental Computational Geometric 
	/// Algorithms.
	/// </summary>
	/// <remarks>
	/// The algorithms are made robust by use of the 
	/// <see cref="RobustDeterminant"/> class.
	/// </remarks>
	public sealed class RobustCGAlgorithms
	{
        public RobustCGAlgorithms()
        {
        }

		/// <summary> 
		/// Returns the index of the direction of the point q
		/// relative to a
		/// vector specified by p1-p2.
		/// 
		/// </summary>
		/// <param name="p1">the origin point of the vector
		/// </param>
		/// <param name="p2">the final point of the vector
		/// </param>
		/// <param name="q">the point to compute the direction to
		/// 
		/// </param>
		/// <returns> 1 if q is counter-clockwise (left) from p1-p2
		/// </returns>
		/// <returns> -1 if q is clockwise (right) from p1-p2
		/// </returns>
		/// <returns> 0 if q is collinear with p1-p2
		/// </returns>
		public static int OrientationIndex(Coordinate p1, Coordinate p2, Coordinate q)
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
		/// Computes whether a ring defined by an array of Coordinate is
		/// oriented counter-clockwise.
		/// <para>
		/// This algorithm is valid only for coordinate lists which do not contain
		/// repeated points.
		/// </para>
		/// </summary>
		/// <param name="ring">an array of coordinates forming a ring
		/// </param>
		/// <returns> true if the ring is oriented counter-clockwise.
		/// </returns>
		public bool IsCCW(ICoordinateList ring)
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
		/// This algorithm does not attempt to first check the point against the envelope
		/// of the ring.
		/// 
		/// </summary>
		/// <param name="ring">assumed to have first point identical to last point
		/// </param>
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

            /*
             *  For each segment l = (i-1, i), see if it crosses ray from test point in positive x direction.
             */
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
                    /*
                    *  segment straddles x axis, so compute intersection with x-axis.
                     */
                    double xInt = RobustDeterminant.SignOfDeterminant(x1, y1, x2, y2) / (y2 - y1);
                    //xsave = xInt;
                    /*
                    *  crosses ray if strictly positive intersection.
                     */
                    if (xInt > 0.0) 
                    {
                        crossings++;
                    }
                }
            }
            /*
             *  p is inside if number of crossings is odd.
             */
            if ((crossings % 2) == 1) 
            {
                return true;
            }
            else 
            {
                return false;
            }
        }		
		
		public bool IsOnLine(Coordinate p, ICoordinateList pts)
		{
            if (pts == null)
            {
                throw new ArgumentNullException("pts");
            }

            LineIntersector lineIntersector = new RobustLineIntersector();

			for (int i = 1; i < pts.Count; i++)
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
		
		public OrientationType ComputeOrientation(Coordinate p1, 
            Coordinate p2, Coordinate q)
		{
			return (OrientationType)OrientationIndex(p1, p2, q);
		}
		
		private bool IsInEnvelope(Coordinate p, ICoordinateList ring)
		{
			Envelope envelope = new Envelope();
			for (int i = 0; i < ring.Count; i++)
			{
				envelope.ExpandToInclude(ring[i]);
			}
			return envelope.Contains(p);
		}
	}
}