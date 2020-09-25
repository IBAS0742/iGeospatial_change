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
using System.Collections;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;
using iGeospatial.Geometries.Visitors;

using iGeospatial.Geometries.Operations.Distance;

namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// Computes the distance and closest points between two 
	/// <see cref="Geometry"/> instances.
	/// </summary>
	/// <remarks>
	/// <para>
    /// This find two points on two <see cref="Geometry"/>s which lie
    /// within a given distance, or else are the closest points
    /// on the geometries (in which case this also provides the distance 
    /// between the geometries).
    /// </para>
	/// The distance computation also finds a pair of points in the input 
	/// geometries which have the minimum distance between them.
	/// <para>
	/// If a point lies in the interior of a line segment, the coordinate 
	/// computed is a close approximation to the exact point.
	/// </para>
	/// The algorithms used are straightforward O(n^2) comparisons.  
	/// This worst-case performance could be improved on by using 
	/// Voronoi techniques or spatial indexes.
	/// </remarks>
	public sealed class DistanceOp
	{
        #region Private Fields

        // input fields
		private Geometry[] geom;
        private double terminateDistance;

        // working fields
        private PointLocator ptLocator;
		private DistanceLocation[] minDistanceLocation;
		private double minDistance;
        private GeometryFactory m_objFactory;
        
        #endregion
		
        #region Constructors and Destructor
		
        /// <summary> 
		/// Initializes a new instance of <see cref="DistanceOp"/> class 
		/// that computes the distance and closest points between the 
		/// two specified geometries.
		/// </summary>
		public DistanceOp(Geometry g0, Geometry g1)
		{
            if (g0 == null)
            {
                throw new ArgumentNullException("g0");
            }
            if (g1 == null)
            {
                throw new ArgumentNullException("g1");
            }

            ptLocator    = new PointLocator();
            minDistance  = Double.MaxValue;

			this.geom    = new Geometry[2];
			geom[0]      = g0;
			geom[1]      = g1;
            m_objFactory = g0.Factory;
            if (m_objFactory == null)
            {
                m_objFactory = g1.Factory;
            }
		}
		
        /// <summary> 
		/// Initializes a new instance of <see cref="DistanceOp"/> class 
		/// that computes the distance and closest points between the 
		/// two specified geometries.
		/// </summary>
		public DistanceOp(Geometry g0, Geometry g1, double 
            terminateDistance) : this(g0, g1)
		{
            this.terminateDistance = terminateDistance;
		}
        
        #endregion
		
        #region Public Methods

		/// <summary> 
		/// Compute the Distance between the closest points of two geometries.
		/// </summary>
		/// <param name="g0">a Geometry
		/// </param>
		/// <param name="g1">another Geometry
		/// </param>
		/// <returns> the Distance between the geometries
		/// </returns>
		public static double Distance(Geometry g0, Geometry g1)
		{
			DistanceOp distOp = new DistanceOp(g0, g1);

			return distOp.Distance();
		}
		
        /// <summary> 
        /// Test whether two geometries lie within a given distance of 
        /// each other.
        /// </summary>
        /// <param name="g0">a <see cref="Geometry"/>
        /// </param>
        /// <param name="g1">another <see cref="Geometry"/>
        /// </param>
        /// <param name="distance">the distance to test
        /// </param>
        /// <returns> true if g0.distance(g1) <= distance
        /// </returns>
        public static bool IsWithinDistance(Geometry g0, Geometry g1, 
            double distance)
        {
            DistanceOp distOp = new DistanceOp(g0, g1, distance);
            
            return (distOp.Distance() <= distance);
        }
		
		/// <summary> 
		/// Compute the the closest points of two geometries.
		/// The points are presented in the same order as the 
		/// input geometries.
		/// 
		/// </summary>
		/// <param name="g0">
		/// A Geometry.
		/// </param>
		/// <param name="g1">
		/// Another Geometry.
		/// </param>
		/// <returns> the closest points in the geometries
		/// </returns>
		public static Coordinate[] ClosestPoints(Geometry g0, Geometry g1)
		{
			DistanceOp distOp = new DistanceOp(g0, g1);
			return distOp.ClosestPoints();
		}
		
		/// <summary> 
		/// Report the distance between the closest points on the 
		/// input geometries. 
		/// </summary>
		/// <returns>
		/// The distance between the geometries.
		/// </returns>
		public double Distance()
		{
			ComputeMinDistance();
			return minDistance;
		}
		
		/// <summary> Report the coordinates of the closest points in the input geometries.
		/// The points are presented in the same order as the input Geometries.
		/// 
		/// </summary>
		/// <returns> a pair of Coordinates of the closest points
		/// </returns>
		public Coordinate[] ClosestPoints()
		{
			ComputeMinDistance();
			Coordinate[] closestPts = new Coordinate[]{minDistanceLocation[0].Coordinate, 
                                                          minDistanceLocation[1].Coordinate};

			return closestPts;
		}
		
		/// <summary> 
		/// Report the locations of the closest points in the input geometries.
		/// The locations are presented in the same order as the input Geometries.
		/// </summary>
		/// <returns> 
		/// A pair of <see cref="DistanceLocation"/>s for the closest points.
		/// </returns>
		public DistanceLocation[] ClosestLocations()
		{
			ComputeMinDistance();

			return minDistanceLocation;
		}
        
        #endregion
		
        #region Private Methods

		private void UpdateMinDistance(double dist)
		{
			if (dist < minDistance)
				minDistance = dist;
		}
		
		private void UpdateMinDistance(DistanceLocation[] locGeom, bool flip)
		{
			// if not set then don't update
			if (locGeom[0] == null)
				return ;
			
			if (flip)
			{
				minDistanceLocation[0] = locGeom[1];
				minDistanceLocation[1] = locGeom[0];
			}
			else
			{
				minDistanceLocation[0] = locGeom[0];
				minDistanceLocation[1] = locGeom[1];
			}
		}
		
		private void ComputeMinDistance()
		{
			if (minDistanceLocation != null)
				return ;
			
			minDistanceLocation = new DistanceLocation[2];
			ComputeContainmentDistance();
			if (minDistance <= terminateDistance)
				return;

			ComputeLineDistance();
		}
		
		private void ComputeContainmentDistance()
		{
			IGeometryList polys0 = PolygonExtracter.GetPolygons(geom[0]);
			IGeometryList polys1 = PolygonExtracter.GetPolygons(geom[1]);
			
			DistanceLocation[] locPtPoly = new DistanceLocation[2];
			// test if either geometry is wholely inside the other
			if (polys1.Count > 0)
			{
				IList insideLocs0 = 
                    ConnectedElementLocationFilter.GetLocations(geom[0]);
				ComputeInside(insideLocs0, polys1, locPtPoly);
				if (minDistance <= terminateDistance)
				{
					minDistanceLocation[0] = locPtPoly[0];
					minDistanceLocation[1] = locPtPoly[1];
					return ;
				}
			}

			if (polys0.Count > 0)
			{
				IList insideLocs1 = 
                    ConnectedElementLocationFilter.GetLocations(geom[1]);
				ComputeInside(insideLocs1, polys0, locPtPoly);
				if (minDistance <= terminateDistance)
				{
					// flip locations, since we are testing geom 1 VS geom 0
					minDistanceLocation[0] = locPtPoly[1];
					minDistanceLocation[1] = locPtPoly[0];
					return;
				}
			}
		}

		private void ComputeInside(IList locs, 
            IGeometryList polys, DistanceLocation[] locPtPoly)
		{
            int nLocs  = locs.Count;
            int nPolys = polys.Count;
			for (int i = 0; i < nLocs; i++)
			{
				DistanceLocation loc = (DistanceLocation)locs[i];
				for (int j = 0; j < nPolys; j++)
				{
					Polygon poly = (Polygon)polys[j];
					ComputeInside(loc, poly, locPtPoly);
					if (minDistance <= terminateDistance)
					{
						return;
					}
				}
			}
		}
		
		private void ComputeInside(DistanceLocation ptLoc, Polygon poly, 
            DistanceLocation[] locPtPoly)
		{
			Coordinate pt = ptLoc.Coordinate;
			if (LocationType.Exterior != ptLocator.Locate(pt, poly))
			{
				minDistance = 0.0;
				locPtPoly[0] = ptLoc;
				DistanceLocation locPoly = new DistanceLocation(poly, pt);
				locPtPoly[1] = locPoly;
				return ;
			}
		}
		
		private void ComputeLineDistance()
		{
			DistanceLocation[] locGeom = new DistanceLocation[2];
			
			/// <summary> Geometries are not wholely inside, so compute Distance from lines and points
			/// of one to lines and points of the other
			/// </summary>
			IGeometryList lines0 = LineStringExtracter.GetLines(geom[0]);
			IGeometryList lines1 = LineStringExtracter.GetLines(geom[1]);
			
			IGeometryList pts0 = PointExtracter.GetPoints(geom[0]);
			IGeometryList pts1 = PointExtracter.GetPoints(geom[1]);
			
			// bail whenever minDistance goes to zero, since it can't get any less
			ComputeMinDistanceLines(lines0, lines1, locGeom);
			UpdateMinDistance(locGeom, false);
			if (minDistance <= terminateDistance)
				return;
			
			locGeom[0] = null;
			locGeom[1] = null;
			ComputeMinDistanceLinesPoints(lines0, pts1, locGeom);
			UpdateMinDistance(locGeom, false);
			if (minDistance <= terminateDistance)
				return;
			
			locGeom[0] = null;
			locGeom[1] = null;
			ComputeMinDistanceLinesPoints(lines1, pts0, locGeom);
			UpdateMinDistance(locGeom, true);
			if (minDistance <= terminateDistance)
				return;
			
			locGeom[0] = null;
			locGeom[1] = null;
			ComputeMinDistancePoints(pts0, pts1, locGeom);
			UpdateMinDistance(locGeom, false);
		}
		
		private void ComputeMinDistanceLines(IGeometryList lines0, 
            IGeometryList lines1, DistanceLocation[] locGeom)
		{
            int nCount0 = lines0.Count;
            int nCount1 = lines1.Count;

            for (int i = 0; i < nCount0; i++)
			{
				LineString line0 = (LineString) lines0[i];
				for (int j = 0; j < nCount1; j++)
				{
					LineString line1 = (LineString) lines1[j];
					ComputeMinDistance(line0, line1, locGeom);
					if (minDistance <= terminateDistance)
						return;
				}
			}
		}
		
		private void ComputeMinDistancePoints(IGeometryList points0, 
            IGeometryList points1, DistanceLocation[] locGeom)
		{
            int nCount0 = points0.Count;
            int nCount1 = points1.Count;
			for (int i = 0; i < nCount0; i++)
			{
				Point pt0 = (Point) points0[i];
				for (int j = 0; j < nCount1; j++)
				{
					Point pt1 = (Point) points1[j];
					double dist = pt0.Coordinate.Distance(pt1.Coordinate);
					if (dist < minDistance)
					{
						minDistance = dist;
						// this is wrong - need to determine closest points on both segments!!!
						locGeom[0] = new DistanceLocation(pt0, 0, pt0.Coordinate);
						locGeom[1] = new DistanceLocation(pt1, 0, pt1.Coordinate);
					}
					if (minDistance <= terminateDistance)
						return;
				}
			}
		}
		
		private void ComputeMinDistanceLinesPoints(IGeometryList lines, 
            IGeometryList points, DistanceLocation[] locGeom)
		{
            int nLines  = lines.Count;
            int nPoints = points.Count;
			for (int i = 0; i < nLines; i++)
			{
				LineString line = (LineString)lines[i];
                if (line == null)
                {
                    continue;
                }

				for (int j = 0; j < nPoints; j++)
				{
					Point pt = (Point) points[j];
					ComputeMinDistance(line, pt, locGeom);
					if (minDistance <= terminateDistance)
						return;
				}
			}
		}
		
		private void ComputeMinDistance(LineString line0, LineString line1, DistanceLocation[] locGeom)
		{
			if (line0.Bounds.Distance(line1.Bounds) > minDistance)
				return;

			ICoordinateList coord0 = line0.Coordinates;
			ICoordinateList coord1 = line1.Coordinates;

			// brute force approach!
            int nCount0 = coord0.Count;
            int nCount1 = coord1.Count;
			for (int i = 0; i < nCount0 - 1; i++)
			{
				for (int j = 0; j < nCount1 - 1; j++)
				{
					double dist = CGAlgorithms.DistanceLineLine(coord0[i], 
                        coord0[i + 1], coord1[j], coord1[j + 1]);
					if (dist < minDistance)
					{
						minDistance = dist;
						LineSegment seg0 = 
                            new LineSegment(m_objFactory, coord0[i], coord0[i + 1]);

						LineSegment seg1 = 
                            new LineSegment(m_objFactory, coord1[j], coord1[j + 1]);
						
                        Coordinate[] closestPt = seg0.ClosestPoints(seg1);
						locGeom[0] = new DistanceLocation(line0, i, closestPt[0]);
						locGeom[1] = new DistanceLocation(line1, j, closestPt[1]);
					}
					if (minDistance <= terminateDistance)
						return ;
				}
			}
		}
		
		private void ComputeMinDistance(LineString line, Point pt, 
            DistanceLocation[] locGeom)
		{
			if (line.Bounds.Distance(pt.Bounds) > minDistance)
				return;

			ICoordinateList coord0 = line.Coordinates;
			Coordinate coord       = pt.Coordinate;

			// brute force approach!
            int nCount0 = coord0.Count;
			for (int i = 0; i < nCount0 - 1; i++)
			{
				double dist = CGAlgorithms.DistancePointLine(coord, coord0[i], coord0[i + 1]);
				if (dist < minDistance)
				{
					minDistance     = dist;
					LineSegment seg = 
                        new LineSegment(m_objFactory, coord0[i], coord0[i + 1]);
					Coordinate segClosestPoint = seg.ClosestPoint(coord);
					locGeom[0]      = new DistanceLocation(line, i, segClosestPoint);
					locGeom[1]      = new DistanceLocation(pt, 0, coord);
				}

				if (minDistance <= terminateDistance)
					return ;
			}
		}
        
        #endregion
	}
}