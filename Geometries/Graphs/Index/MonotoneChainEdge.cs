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
using iGeospatial.Geometries.Graphs;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Graphs.Index
{
	/// <summary> 
	/// MonotoneChains are a way of partitioning the segments of an edge to
	/// allow for fast searching of intersections.
	/// </summary>
	/// <remarks>
	/// They have the following properties:
	/// <list type="number">
	/// <item>
	/// the segments Within a monotone chain will never intersect each other
	/// </item>
	/// <item>
	/// the envelope of any contiguous subset of the segments in a monotone chain
	/// is simply the envelope of the endpoints of the subset.
	/// </item>
    /// </list>
	/// Property 1 means that there is no need to test pairs of segments from Within
	/// the same monotone chain for intersection.
	/// Property 2 allows
	/// binary search to be used to find the intersection points of two monotone chains.
	/// For many types of real-world data, these properties eliminate a large number of
	/// segment comparisons, producing substantial speed gains.
	/// </remarks>
	[Serializable]
    internal class MonotoneChainEdge
	{
        #region Private Fields

		private Edge e;
		private ICoordinateList pts; // cache a reference to the coord array, for efficiency
		// the lists of start/end indexes of the monotone chains.
		// Includes the end point of the edge as a sentinel
		private int[] startIndex;

		// these envelopes are created once and reused
        private Envelope env1;

        private Envelope env2;
        
        #endregion
		
        #region Constructors and Destructor

        public MonotoneChainEdge(Edge e)
        {
            env1 = new Envelope();
            env2 = new Envelope();

            this.e = e;
            pts    = e.Coordinates;

            startIndex = MonotoneChainIndexer.GetChainStartIndices(pts);
        }

        #endregion
		
        #region Public Properties

		public ICoordinateList Coordinates
		{
			get
			{
				return pts;
			}
		}

		public int[] StartIndexes
		{
			get
			{
				return startIndex;
			}
		}
        
        #endregion
		
        #region Public Methods

		public double GetMinX(int chainIndex)
		{
			double x1 = pts[startIndex[chainIndex]].X;
			double x2 = pts[startIndex[chainIndex + 1]].X;

			return x1 < x2 ? x1 : x2;
		}

		public double GetMaxX(int chainIndex)
		{
			double x1 = pts[startIndex[chainIndex]].X;
			double x2 = pts[startIndex[chainIndex + 1]].X;

			return x1 > x2 ? x1 : x2;
		}
		
		public void ComputeIntersects(MonotoneChainEdge mce, 
            SegmentIntersector si)
		{
			for (int i = 0; i < startIndex.Length - 1; i++)
			{
				for (int j = 0; j < mce.startIndex.Length - 1; j++)
				{
					ComputeIntersectsForChain(i, mce, j, si);
				}
			}
		}

		public void ComputeIntersectsForChain(int chainIndex0, 
            MonotoneChainEdge mce, int chainIndex1, SegmentIntersector si)
		{
			ComputeIntersectsForChain(startIndex[chainIndex0], 
                startIndex[chainIndex0 + 1], mce, mce.startIndex[chainIndex1], 
                mce.startIndex[chainIndex1 + 1], si);
		}
        
        #endregion
		
        #region Private Methods

		private void ComputeIntersectsForChain(int start0, int end0, 
            MonotoneChainEdge mce, int start1, int end1, SegmentIntersector ei)
		{
			Coordinate p00 = pts[start0];
			Coordinate p01 = pts[end0];
			Coordinate p10 = mce.pts[start1];
			Coordinate p11 = mce.pts[end1];

			// terminating condition for the recursion
			if (end0 - start0 == 1 && end1 - start1 == 1)
			{
				ei.AddIntersections(e, start0, mce.e, start1);
				return ;
			}
			// nothing to do if the envelopes of these chains don't overlap
			env1.Initialize(p00, p01);
			env2.Initialize(p10, p11);

			if (!env1.Intersects(env2))
				return ;
			
			// the chains overlap, so split each in half and iterate  (binary search)
			int mid0 = (start0 + end0) / 2;
			int mid1 = (start1 + end1) / 2;
			
			// Assert: mid != start or end (since we checked above for end - start <= 1)
			// check terminating conditions before recursing
			if (start0 < mid0)
			{
				if (start1 < mid1)
					ComputeIntersectsForChain(start0, mid0, mce, start1, mid1, ei);

				if (mid1 < end1)
					ComputeIntersectsForChain(start0, mid0, mce, mid1, end1, ei);
			}

			if (mid0 < end0)
			{
				if (start1 < mid1)
					ComputeIntersectsForChain(mid0, end0, mce, start1, mid1, ei);

				if (mid1 < end1)
					ComputeIntersectsForChain(mid0, end0, mce, mid1, end1, ei);
			}
		}
        
        #endregion
	}
}