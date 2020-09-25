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
	/// Computes the minimum diameter of a Geometry.
	/// </summary>
	/// <remarks>
	/// The minimum diameter is defined to be the
	/// width of the smallest band that
	/// Contains the geometry,
	/// where a band is a strip of the plane defined
	/// by two parallel lines.
	/// This can be thought of as the smallest hole that the geometry can be
	/// moved through, with a single rotation.
	/// <para>
	/// The first step in the algorithm is computing the convex hull of the Geometry.
	/// If the input Geometry is known to be convex, a hint can be supplied to
	/// avoid this computation.
	/// </para>
	/// <seealso cref="ConvexHull"></seealso>
	/// </remarks>
	public class MinimumDiameter
	{
        private Geometry inputGeom;
		
        private bool isConvex;
		
        private LineSegment minBaseSeg;
        private Coordinate minWidthPt;
        private int minPtIndex;
        private double minWidth;
		
		/// <summary> 
		/// Compute a minimum diameter for a giver Geometry.
		/// </summary>
		/// <param name="inputGeometry">a Geometry
		/// </param>
		public MinimumDiameter(Geometry inputGeometry) 
            : this(inputGeometry, false)
		{
		}
		
		/// <summary> C
		/// ompute a minimum diameter for a giver Geometry,
		/// with a hint if the Geometry is convex
		/// (e.g. a convex Polygon or LinearRing,
		/// or a two-point LineString, or a Point).
		/// </summary>
		/// <param name="inputGeometry">a Geometry which is convex
		/// </param>
		/// <param name="isConvex">true if the input geometry is convex
		/// </param>
		public MinimumDiameter(Geometry inputGeometry, bool isConvex)
		{
            minBaseSeg = new LineSegment(inputGeom.Factory);
            this.inputGeom = inputGeom;
			this.isConvex = isConvex;
		}
		
		/// <summary> 
		/// Gets the length of the minimum diameter of the input Geometry.
		/// </summary>
		/// <value>The length of the minimum diameter.</value>
		public virtual double Length
		{
			get
			{
				ComputeMinimumDiameter();

				return minWidth;
			}
		}
			
		/// <summary> 
		/// Gets the Coordinate forming one end of the minimum diameter
		/// </summary>
		/// <value>
		/// A coordinate forming one end of the minimum diameter.
		/// </value>
		public virtual Coordinate WidthCoordinate
		{
			get
			{
				ComputeMinimumDiameter();
				return minWidthPt;
			}
		}
			
		/// <summary> 
		/// Gets the segment forming the base of the minimum diameter.
		/// </summary>
		/// <value> the segment forming the base of the minimum diameter
		/// </value>
		public virtual LineString SupportingSegment
		{
			get
			{
				ComputeMinimumDiameter();

				return inputGeom.Factory.CreateLineString(new Coordinate[]{minBaseSeg.p0, minBaseSeg.p1});
			}
		}

		/// <summary> Gets a LineString which is a minimum diameter.</summary>
		/// <value> Returns a LineString which is a minimum diameter.</value>
		public virtual LineString Diameter
		{
			get
			{
				ComputeMinimumDiameter();
				
				// return empty linestring if no minimum width calculated
				if (minWidthPt == null)
					return inputGeom.Factory.CreateLineString((Coordinate[]) null);
				
				Coordinate basePt = minBaseSeg.Project(minWidthPt);
				return inputGeom.Factory.CreateLineString(new Coordinate[]{basePt, minWidthPt});
			}
		}
		
		private void  ComputeMinimumDiameter()
		{
			// check if computation is cached
			if (minWidthPt != null)
				return;
			
			if (isConvex)
            {
                ComputeWidthConvex(inputGeom);
            }
			else
			{
				Geometry convexGeom = (new ConvexHull(inputGeom)).ComputeConvexHull();
				ComputeWidthConvex(convexGeom);
			}
		}
		
		private void  ComputeWidthConvex(Geometry geom)
		{
			//System.out.println("Input = " + geom);
			ICoordinateList pts = null;

            GeometryType geomType = geom.GeometryType;

            if (geomType == GeometryType.Polygon)
				pts = ((Polygon)geom).ExteriorRing.Coordinates;
			else
				pts = geom.Coordinates;
			
			// special cases for lines or points or degenerate rings
			if (pts.Count == 0)
			{
				minWidth = 0.0;
				minWidthPt = null;
				minBaseSeg = null;
			}
			else if (pts.Count == 1)
			{
				minWidth = 0.0;
				minWidthPt = pts[0];
				minBaseSeg.p0 = pts[0];
				minBaseSeg.p1 = pts[0];
			}
			else if (pts.Count == 2 || pts.Count == 3)
			{
				minWidth = 0.0;
				minWidthPt = pts[0];
				minBaseSeg.p0 = pts[0];
				minBaseSeg.p1 = pts[1];
			}
			else
            {
                ComputeConvexRingMinDiameter(pts);
            }
		}
		
		/// <summary> Compute the width information for a ring of Coordinates.
		/// Leaves the width information in the instance variables.
		/// 
		/// </summary>
		/// <param name="">pts
		/// </param>
		/// <returns>
		/// </returns>
		private void  ComputeConvexRingMinDiameter(ICoordinateList pts)
		{
			// for each segment in the ring
			minWidth = Double.MaxValue;
			int currMaxIndex = 1;
			
			LineSegment seg = new LineSegment(inputGeom.Factory);

			// compute the max Distance for all segments in the ring, and pick the minimum
			for (int i = 0; i < pts.Count - 1; i++)
			{
				seg.p0 = pts[i];
				seg.p1 = pts[i + 1];
				currMaxIndex = FindMaxPerpDistance(pts, seg, currMaxIndex);
			}
		}
		
		private int FindMaxPerpDistance(ICoordinateList pts, LineSegment seg, int startIndex)
		{
			double maxPerpDistance = seg.DistancePerpendicular(pts[startIndex]);
			double nextPerpDistance = maxPerpDistance;
			int maxIndex = startIndex;
			int NextIndex = maxIndex;
			while (nextPerpDistance >= maxPerpDistance)
			{
				maxPerpDistance = nextPerpDistance;
				maxIndex = NextIndex;
				
				NextIndex = MinimumDiameter.NextIndex(pts, maxIndex);
				nextPerpDistance = seg.DistancePerpendicular(pts[NextIndex]);
			}
			// found maximum width for this segment - update global min dist if appropriate
			if (maxPerpDistance < minWidth)
			{
				minPtIndex = maxIndex;
				minWidth = maxPerpDistance;
				minWidthPt = pts[minPtIndex];
				minBaseSeg = new LineSegment(seg, seg.Factory);
			}

			return maxIndex;
		}
		
		private static int NextIndex(ICoordinateList pts, int index)
		{
			index++;
			if (index >= pts.Count)
				index = 0;

			return index;
		}
	}
}