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
using System.Collections;

using iGeospatial.Coordinates;
using iGeospatial.Coordinates.Visitors;

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary> 
	/// Computes the convex hull of a <see cref="Geometry"/>.
	/// </summary>
	/// <remarks>
	/// The convex hull is the smallest convex <see cref="Geometry"/> that 
	/// contains all the points in the input <see cref="Geometry"/>.
	/// <para>
	/// Uses the Graham Scan algorithm.
	/// </para>
	/// </remarks>
	public sealed class ConvexHull
	{
        private ICoordinateList m_arrInput;
		private GeometryFactory m_objFactory;
		
		/// <summary> 
		/// Create a new convex hull construction for the input Geometry.
		/// </summary>
		public ConvexHull(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            m_objFactory = geometry.Factory;
            
            UniqueCoordinateArrayVisitor filter = 
                new UniqueCoordinateArrayVisitor();
            geometry.Apply(filter);

            m_arrInput = filter.Coordinates;
        }

        public ConvexHull(Coordinate[] inputPoints, GeometryFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            } 
            if (inputPoints == null)
            {
                throw new ArgumentNullException("inputPoints");
            }

            m_arrInput   = new CoordinateCollection(inputPoints);
            m_objFactory = factory;
        }

        public ConvexHull(ICoordinateList inputPoints, 
            GeometryFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            } 
            if (inputPoints == null)
            {
                throw new ArgumentNullException("inputPoints");
            }

            m_arrInput   = inputPoints;
            m_objFactory = factory;
        }
		
		/// <summary> 
		/// Returns a Geometry that represents the convex hull of the input
		/// geometry.
		/// </summary>
		/// <returns> 
		/// If the convex hull contains 3 or more points, a <see cref="Polygon"/>;
		/// 2 points, a <see cref="LineString"/>; 1 point, a <see cref="Point"/>;
		/// 0 points, an empty <see cref="GeometryCollection"/>.
		/// </returns>
		/// <remarks>
		/// The geometry will contain the minimal number of points needed to
		/// represent the convex hull.  In particular, no more than two consecutive
		/// points will be collinear.
		/// </remarks>
		public Geometry ComputeConvexHull()
		{
			if (m_arrInput == null || m_arrInput.Count == 0)
			{
				return m_objFactory.CreateGeometryCollection(null);
			}

            int nCount = m_arrInput.Count;
			if (nCount == 1)
			{
				return m_objFactory.CreatePoint(m_arrInput[0]);
			}

			if (nCount == 2)
			{
				return m_objFactory.CreateLineString(m_arrInput);
			}
			
			// sort points for Graham scan.
            ICoordinateList reducedPts = m_arrInput;

            // use heuristic to reduce points, if large
			if (nCount > 50)
			{
                reducedPts = Reduce(m_arrInput);
			}
            // sort points for Graham scan.
            ICoordinateList sortedPts = PreSort(reducedPts);
			
			// Use Graham scan to find convex hull.
			Stack cHS = GrahamScan(sortedPts);
			
			// Convert stack to an array.
			ICoordinateList cH = ToCoordinateArray(cHS);
			
			// Convert array to linear ring.
			return LineOrPolygon(cH);
		}
		
		/// <summary> 
		/// An alternative to Stack.ToArray, which is not present in earlier versions
		/// of Java.
		/// </summary>
		private ICoordinateList ToCoordinateArray(Stack stack)
		{
            CoordinateCollection list = new CoordinateCollection();
			foreach (object obj in stack)
			{
                list.Add((Coordinate)obj);
			}

            list.Reverse();

            return list;
		}

        /**
         * Uses a heuristic to reduce the number of points scanned
         * to compute the hull.
         * The heuristic is to find a polygon guaranteed to
         * be in (or on) the hull, and eliminate all points inside it.
         * A quadrilateral defined by the extremal points
         * in the four orthogonal directions
         * can be used, but even more inclusive is
         * to use an octilateral defined by the points in the 8 cardinal directions.
         * <p>
         * Note that even if the method used to determine the polygon vertices
         * is not 100% robust, this does not affect the robustness of the convex hull.
         *
         * @param pts
         * @return
         */
        private ICoordinateList Reduce(ICoordinateList inputPts)
        {
            ICoordinateList polyPts = ComputeOctRing(inputPts);

            // unable to compute interior polygon for some reason
            if (polyPts == null)
                return inputPts;

            // add points defining polygon
            ICoordinateList reducedSet = new CoordinateCollection();
            int nCount = polyPts.Count;
            for (int i = 0; i < nCount; i++) 
            {
                if (!reducedSet.Contains(polyPts[i]))
                {
                    reducedSet.Add(polyPts[i]);
                }
            }

            // Add all unique points not in the interior poly.
            // CGAlgorithms.isPointInRing is not defined for points actually on the ring,
            // but this doesn't matter since the points of the interior polygon
            // are forced to be in the reduced set.
            nCount = inputPts.Count;

            for (int i = 0; i < nCount; i++) 
            {
                if (!CGAlgorithms.IsPointInRing(inputPts[i], polyPts)) 
                {
                    reducedSet.Add(inputPts[i]);
                }
            }

            return reducedSet;
        }
		
//		private ICoordinateList ReduceQuad(ICoordinateList pts)
//		{
//			ConvexHull.BigQuad bigQuad = this.GetBigQuad(pts);
//			
//			// Build a linear ring defining a big poly.
//			CoordinateCollection bigPoly = new CoordinateCollection();
//
//            bigPoly.Add(bigQuad.westmost);
//			if (!bigPoly.Contains(bigQuad.northmost))
//			{
//				bigPoly.Add(bigQuad.northmost);
//			}
//			if (!bigPoly.Contains(bigQuad.eastmost))
//			{
//				bigPoly.Add(bigQuad.eastmost);
//			}
//			if (!bigPoly.Contains(bigQuad.southmost))
//			{
//				bigPoly.Add(bigQuad.southmost);
//			}
//
//            if (bigPoly.Count < 3)
//			{
//				return pts;
//			}
//
//            bigPoly.Add(bigQuad.westmost);
//
//			LinearRing bQ = m_objFactory.CreateLinearRing(bigPoly);
//
//			// load an array with all points not in the big poly
//			// and the defining points.
//			CoordinateCollection reducedSet = new CoordinateCollection(bigPoly);
//			for (int i = 0; i < pts.Count; i++)
//			{
//				if (pointLocator.Locate(pts[i], bQ) == LocationType.Exterior)
//				{
//					reducedSet.Add(pts[i]);
//				}
//			}
//			
//			// Return this array as the reduced problem.
//			return reducedSet;
//		}
		
		private ICoordinateList PreSort(ICoordinateList pts)
		{
			Coordinate t;
			
			// find the lowest point in the set. If two or more points have
			// the same minimum y coordinate choose the one with the minimu x.
			// This focal point is put in array location pts[0].
			for (int i = 1; i < pts.Count; i++)
			{
				if ((pts[i].Y < pts[0].Y) || 
                    ((pts[i].Y == pts[0].Y) && (pts[i].X < pts[0].X)))
				{
					t = pts[0];
					pts[0] = pts[i];
					pts[i] = t;
				}
			}

            // sort the points radially around the focal point.
            CoordinateCollection coordPts = (CoordinateCollection)pts;

            coordPts.Sort(1, coordPts.Count - 1, new RadialComparator(coordPts[0]));
			
			// sort the points radially around the focal point.
			//RadialSort(pts);

			return pts;
		}
			
		private Stack GrahamScan(ICoordinateList c)
		{
			Coordinate p;
			Stack ps = new Stack();
			ps.Push(c[0]);
			ps.Push(c[1]);
			ps.Push(c[2]);

			for (int i = 3; i < c.Count; i++)
			{
				p = (Coordinate) ps.Pop();
				while (CGAlgorithms.ComputeOrientation(
                    (Coordinate)ps.Peek(), p, c[i]) > 0)
				{
					p = (Coordinate) ps.Pop();
				}
				ps.Push(p);
				ps.Push(c[i]);
                p = c[i];
			}

			ps.Push(c[0]);

			return ps;
		}
		
//		private void  RadialSort(ICoordinateList p)
//		{
//			// A selection sort routine, assumes the pivot point is
//			// the first point (i.e., p[0]).
//			Coordinate t;
//			for (int i = 1; i < (p.Count - 1); i++)
//			{
//				int min = i;
//				for (int j = i + 1; j < p.Count; j++)
//				{
//					if (PolarCompare(p[0], p[j], p[min]) < 0)
//					{
//						min = j;
//					}
//				}
//				t = p[i];
//				p[i] = p[min];
//				p[min] = t;
//			}
//		}
//		
//		private int PolarCompare(Coordinate o, Coordinate p, Coordinate q)
//		{
//			// Given two points p and q Compare them with respect to their radial
//			// ordering about point o. -1, 0 or 1 depending on whether p is less than,
//			// equal to or greater than q. First checks radial ordering then if both
//			// points lie on the same line, check Distance to o.
//			double dxp = p.X - o.X;
//			double dyp = p.Y - o.Y;
//			double dxq = q.X - o.X;
//			double dyq = q.Y - o.Y;
//			double alph = Math.Atan2(dxp, dyp);
//			double beta = Math.Atan2(dxq, dyq);
//			if (alph < beta)
//			{
//				return - 1;
//			}
//			if (alph > beta)
//			{
//				return 1;
//			}
//			double op = dxp * dxp + dyp * dyp;
//			double oq = dxq * dxq + dyq * dyq;
//			if (op < oq)
//			{
//				return -1;
//			}
//			if (op > oq)
//			{
//				return 1;
//			}
//			return 0;
//		}
		
		/// <returns>    whether the three coordinates are collinear and c2 lies between
		/// c1 and c3 inclusive
		/// </returns>
		private bool IsBetween(Coordinate c1, Coordinate c2, Coordinate c3)
		{
			if (CGAlgorithms.ComputeOrientation(c1, c2, c3) != 0)
			{
				return false;
			}

			if (c1.X != c3.X)
			{
				if (c1.X <= c2.X && c2.X <= c3.X)
				{
					return true;
				}
				if (c3.X <= c2.X && c2.X <= c1.X)
				{
					return true;
				}
			}
			
            if (c1.Y != c3.Y)
			{
				if (c1.Y <= c2.Y && c2.Y <= c3.Y)
				{
					return true;
				}
				if (c3.Y <= c2.Y && c2.Y <= c1.Y)
				{
					return true;
				}
			}

			return false;
		}

        private ICoordinateList ComputeOctRing(ICoordinateList inputPts) 
        {
            Coordinate[] octPts = ComputeOctPts(inputPts);
            CoordinateCollection coordList = new CoordinateCollection(octPts);

            // points must all lie in a line
            if (coordList.Count < 3) 
            {
                return null;
            }

            coordList.CloseRing();
            
            return coordList;
        }

        private Coordinate[] ComputeOctPts(ICoordinateList inputPts)
        {
            Coordinate[] pts = new Coordinate[8];

            for (int j = 0; j < pts.Length; j++) 
            {
                pts[j] = inputPts[0];
            }
            
            for (int i = 1; i < inputPts.Count; i++) 
            {
                if (inputPts[i].X < pts[0].X) 
                {
                    pts[0] = inputPts[i];
                }
                if (inputPts[i].X - inputPts[i].Y < pts[1].X - pts[1].Y) 
                {
                    pts[1] = inputPts[i];
                }
                if (inputPts[i].Y > pts[2].Y) 
                {
                    pts[2] = inputPts[i];
                }
                if (inputPts[i].X + inputPts[i].Y > pts[3].X + pts[3].Y) 
                {
                    pts[3] = inputPts[i];
                }
                if (inputPts[i].X > pts[4].X) 
                {
                    pts[4] = inputPts[i];
                }
                if (inputPts[i].X - inputPts[i].Y > pts[5].X - pts[5].Y) 
                {
                    pts[5] = inputPts[i];
                }
                if (inputPts[i].Y < pts[6].Y) 
                {
                    pts[6] = inputPts[i];
                }
                if (inputPts[i].X + inputPts[i].Y < pts[7].X + pts[7].Y) 
                {
                    pts[7] = inputPts[i];
                }
            }
            
            return pts; 
        }
		
//		private ConvexHull.BigQuad GetBigQuad(ICoordinateList pts)
//		{
//			ConvexHull.BigQuad bigQuad = new BigQuad();
//			bigQuad.northmost = pts[0];
//			bigQuad.southmost = pts[0];
//			bigQuad.westmost = pts[0];
//			bigQuad.eastmost = pts[0];
//
//			for (int i = 1; i < pts.Count; i++)
//			{
//				if (pts[i].X < bigQuad.westmost.X)
//				{
//					bigQuad.westmost = pts[i];
//				}
//				if (pts[i].X > bigQuad.eastmost.X)
//				{
//					bigQuad.eastmost = pts[i];
//				}
//				if (pts[i].Y < bigQuad.southmost.Y)
//				{
//					bigQuad.southmost = pts[i];
//				}
//				if (pts[i].Y > bigQuad.northmost.Y)
//				{
//					bigQuad.northmost = pts[i];
//				}
//			}
//			
//            return bigQuad;
//		}
		
		/// <param name="coordinates"> the vertices of a linear ring, which may or may not be
		/// flattened (i.e. vertices collinear)
		/// </param>
		/// <returns>           a 2-vertex LineString if the vertices are
		/// collinear; otherwise, a Polygon with unnecessary
		/// (collinear) vertices removed
		/// </returns>
		private Geometry LineOrPolygon(ICoordinateList coordinates)
		{
            coordinates = CleanRing(coordinates);

			if (coordinates.Count == 3)
			{
				return m_objFactory.CreateLineString(
                    new Coordinate[]{coordinates[0], coordinates[1]});
			}
			
            LinearRing linearRing = m_objFactory.CreateLinearRing(coordinates);

            return m_objFactory.CreatePolygon(linearRing, null);
		}
		
		/// <param name="original"> the vertices of a linear ring, which may or may not be
		/// flattened (i.e. vertices collinear)
		/// </param>
		/// <returns>           the coordinates with unnecessary (collinear) vertices
		/// removed
		/// </returns>
		private ICoordinateList CleanRing(ICoordinateList original)
		{
			CoordinateCollection cleanedRing = new CoordinateCollection();
			Coordinate previousDistinctCoordinate = null;
			for (int i = 0; i <= original.Count - 2; i++)
			{
				Coordinate currentCoordinate = original[i];
				Coordinate nextCoordinate = original[i + 1];
				if (currentCoordinate.Equals(nextCoordinate))
				{
					continue;
				}

				if (previousDistinctCoordinate != null && IsBetween(previousDistinctCoordinate, currentCoordinate, nextCoordinate))
				{
					continue;
				}

				cleanedRing.Add(currentCoordinate);
				previousDistinctCoordinate = currentCoordinate;
			}

            cleanedRing.Add(original[original.Count - 1]);

            return cleanedRing;
		}
		
//		private sealed class BigQuad
//		{
//			public Coordinate northmost;
//			public Coordinate southmost;
//			public Coordinate westmost;
//			public Coordinate eastmost;
//		}

        /// <summary>
        /// Compares <see cref="Coordinate"/>s for their angle and distance
        /// relative to an origin.
        /// </summary>
        private sealed class RadialComparator : IComparer
        {
            private Coordinate origin;

            public RadialComparator(Coordinate origin)
            {
                this.origin = origin;
            }

            public int Compare(Object o1, Object o2)
            {
                return PolarCompare(origin, (Coordinate)o1, (Coordinate) o2);
            }

            /**
             * Given two points p and q compare them with respect to their radial
             * ordering about point o.  First checks radial ordering.
             * If points are collinear, the comparison is based
             * on their distance to the origin.
             * <p>
             * p < q iff
             * <ul>
             * <li>ang(o-p) < ang(o-q) (e.g. o-p-q is CCW)
             * <li>or ang(o-p) == ang(o-q) && dist(o,p) < dist(o,q)
             * </ul>
             *
             * @param o the origin
             * @param p a point
             * @param q another point
             * @return -1, 0 or 1 depending on whether p is less than,
             * equal to or greater than q
             */
            private static int PolarCompare(Coordinate o, Coordinate p, Coordinate q)
            {
                double dxp = p.X - o.X;
                double dyp = p.Y - o.Y;
                double dxq = q.X - o.X;
                double dyq = q.Y - o.Y;

                /*
                      // MD - non-robust
                      int result = 0;
                      double alph = Math.atan2(dxp, dyp);
                      double beta = Math.atan2(dxq, dyq);
                      if (alph < beta) {
                        result = -1;
                      }
                      if (alph > beta) {
                        result = 1;
                      }
                      if (result !=  0) return result;
                      //*/

                OrientationType orient = 
                    CGAlgorithms.ComputeOrientation(o, p, q);

                if (orient == OrientationType.CounterClockwise) 
                    return 1;
                if (orient == OrientationType.Clockwise) 
                    return -1;

                // points are collinear - check distance
                double op = dxp * dxp + dyp * dyp;
                double oq = dxq * dxq + dyq * dyq;
                if (op < oq) 
                {
                    return -1;
                }
                if (op > oq) 
                {
                    return 1;
                }

                return 0;
            }   
        }     
	}
}