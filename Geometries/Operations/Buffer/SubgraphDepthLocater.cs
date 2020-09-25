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
using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Operations.Buffer
{
	/// <summary> Locates a subgraph inside a set of subgraphs,
	/// in order to determine the outside depth of the subgraph.
	/// The input subgraphs are assumed to have had depths
	/// already calculated for their edges.
	/// </summary>
	internal class SubgraphDepthLocater
	{
		private ArrayList subgraphs;
		private LineSegment seg;
		
		public SubgraphDepthLocater(ArrayList subgraphs)
		{
            seg = new LineSegment((GeometryFactory)null);

			this.subgraphs = subgraphs;
		}
		
		public virtual int GetDepth(Coordinate p)
		{
			ArrayList stabbedSegments = FindStabbedSegments(p);
			// if no segments on stabbing line subgraph must be outside all others.
			if (stabbedSegments.Count == 0)
				return 0;

            stabbedSegments.Sort();
			DepthSegment ds = (DepthSegment) stabbedSegments[0];
			return ds.LeftDepth;
		}
		
		/// <summary> Finds all non-horizontal segments intersecting the stabbing line.
		/// The stabbing line is the ray to the right of stabbingRayLeftPt.
		/// 
		/// </summary>
		/// <param name="stabbingRayLeftPt">the left-hand origin of the stabbing line
		/// </param>
		/// <returns> a List of {@link DepthSegments} intersecting the stabbing line
		/// </returns>
		private ArrayList FindStabbedSegments(Coordinate stabbingRayLeftPt)
		{
			ArrayList stabbedSegments = new ArrayList();

            for (IEnumerator i = subgraphs.GetEnumerator(); i.MoveNext(); )
			{
				BufferSubgraph bsg = (BufferSubgraph) i.Current;
				
                // optimization - don't bother checking subgraphs which the ray does not intersect
                Envelope env = bsg.Envelope;
                if (stabbingRayLeftPt.Y < env.MinY || 
                    stabbingRayLeftPt.Y > env.MaxY)
                    continue;
				
                FindStabbedSegments(stabbingRayLeftPt, 
                    bsg.DirectedEdges, stabbedSegments);
			}

			return stabbedSegments;
		}
		
		/// <summary> Finds all non-horizontal segments intersecting the stabbing line
		/// in the list of dirEdges.
		/// The stabbing line is the ray to the right of stabbingRayLeftPt.
		/// 
		/// </summary>
		/// <param name="stabbingRayLeftPt">the left-hand origin of the stabbing line
		/// </param>
		/// <param name="stabbedSegments">the current list of {@link DepthSegments} intersecting the stabbing line
		/// </param>
		private void FindStabbedSegments(Coordinate stabbingRayLeftPt, 
            ArrayList dirEdges, ArrayList stabbedSegments)
		{
			// Check all forward DirectedEdges only.  This is still general,
			// because each Edge has a forward DirectedEdge.
			for (IEnumerator i = dirEdges.GetEnumerator(); i.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) i.Current;
				if (!de.Forward)
					continue;

				FindStabbedSegments(stabbingRayLeftPt, de, stabbedSegments);
			}
		}
		
		/// <summary> 
		/// Finds all non-horizontal segments intersecting the stabbing line
		/// in the input dirEdge.
		/// The stabbing line is the ray to the right of stabbingRayLeftPt.
		/// </summary>
		/// <param name="stabbingRayLeftPt">the left-hand origin of the stabbing line
		/// </param>
		/// <param name="stabbedSegments">the current list of {@link DepthSegments} intersecting the stabbing line
		/// </param>
		private void  FindStabbedSegments(Coordinate stabbingRayLeftPt, DirectedEdge dirEdge, ArrayList stabbedSegments)
		{
			ICoordinateList pts = dirEdge.Edge.Coordinates;
			for (int i = 0; i < pts.Count - 1; i++)
			{
				seg.p0 = pts[i];
				seg.p1 = pts[i + 1];
				// ensure segment always points upwards
				if (seg.p0.Y > seg.p1.Y)
					seg.Reverse();
				
				// skip segment if it is left of the stabbing line
				double maxx = Math.Max(seg.p0.X, seg.p1.X);
				if (maxx < stabbingRayLeftPt.X)
					continue;
				
				// skip horizontal segments (there will be a non-horizontal one carrying the same depth info
				if (seg.IsHorizontal)
					continue;
				
				// skip if segment is above or below stabbing line
				if (stabbingRayLeftPt.Y < seg.p0.Y || stabbingRayLeftPt.Y > seg.p1.Y)
					continue;
				
				// skip if stabbing ray is right of the segment
				if (CGAlgorithms.ComputeOrientation(seg.p0, seg.p1, stabbingRayLeftPt) 
                    == OrientationType.Clockwise)
					continue;
				
				// stabbing line cuts this segment, so record it
				int depth = dirEdge.GetDepth(Position.Left);
				// if segment direction was flipped, use RHS depth instead
				if (!seg.p0.Equals(pts[i]))
					depth = dirEdge.GetDepth(Position.Right);

				DepthSegment ds = new DepthSegment(this, seg, depth);

                stabbedSegments.Add(ds);
			}
		}
		
		/// <summary> 
		/// A segment from a directed edge which has been assigned a depth value
		/// for its sides.
		/// </summary>
		private sealed class DepthSegment : System.IComparable
		{
            private SubgraphDepthLocater m_objDepthLocater;
			
            private LineSegment upwardSeg;
            private int leftDepth;
			
			public DepthSegment(SubgraphDepthLocater depthLocater, 
                LineSegment segment, int depth)
			{
                this.m_objDepthLocater = depthLocater;

				// input seg is assumed to be normalized
				upwardSeg = new LineSegment(segment, segment.Factory);
				//upwardSeg.Normalize();
				this.leftDepth = depth;
			}
			
            public SubgraphDepthLocater DepthLocater
            {
                get
                {
                    return m_objDepthLocater;
                }
				
            }

            public int LeftDepth
            {
                get
                {
                    return leftDepth;
                }
            }

			/// <summary> Defines a comparision operation on DepthSegments
			/// which orders them left to right
			/// <code>
			/// DS1 less than DS2   if   DS1.seg is left of DS2.seg
			/// DS1 greater than DS2   if   DS1.seg is right of DS2.seg
			/// </code>
			/// </summary>
			/// <param name="">obj
			/// </param>
			/// <returns>
			/// </returns>
			public int CompareTo(object obj)
			{
				DepthSegment other = (DepthSegment) obj;
				// try and compute a determinate orientation for the segments.
				// Test returns 1 if other is left of this (i.e. this > other)
				int orientIndex = upwardSeg.OrientationIndex(other.upwardSeg);
				
				// If comparison between this and other is indeterminate,
				// try the opposite call order.
				// orientationIndex value is 1 if this is left of other,
				// so have to flip sign to get proper comparison value of
				// -1 if this is leftmost
				if (orientIndex == 0)
					orientIndex = (- 1) * other.upwardSeg.OrientationIndex(upwardSeg);
				
				// if orientation is determinate, return it
				if (orientIndex != 0)
					return orientIndex;
				
				// otherwise, segs must be collinear - sort based on minimum X value
				return CompareX(this.upwardSeg, other.upwardSeg);
			}
			
			/// <summary> Compare two collinear segments for left-most ordering.
			/// If segs are vertical, use vertical ordering for comparison.
			/// If segs are equal, return 0.
			/// Segments are assumed to be directed so that the second coordinate is >= to the first
			/// (e.g. up and to the right).
			/// 
			/// </summary>
			/// <param name="seg0">a segment to Compare
			/// </param>
			/// <param name="seg1">a segment to Compare
			/// </param>
			/// <returns>
			/// </returns>
			private int CompareX(LineSegment seg0, LineSegment seg1)
			{
				int compare0 = seg0.p0.CompareTo(seg1.p0);
				if (compare0 != 0)
					return compare0;
				return seg0.p1.CompareTo(seg1.p1);
			}
		}
	}
}