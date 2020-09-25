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

namespace iGeospatial.Geometries.Indexers.Chain
{
	/// <summary> 
	/// MonotoneChains are a way of partitioning the segments of a linestring to
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
	/// is equal to the envelope of the endpoints of the subset.
	/// </item>
	/// </list>
	/// <para>
	/// Property 1 means that there is no need to test pairs of segments from Within
	/// the same monotone chain for intersection.
	/// Property 2 allows
	/// binary search to be used to find the intersection points of two monotone chains.
	/// For many types of real-world data, these properties eliminate a large number of
	/// segment comparisons, producing substantial speed gains.
	/// </para>
	/// <para>
	/// One of the goals of this implementation of MonotoneChains is to be
	/// as space and time efficient as possible. One design choice that aids this
	/// is that a MonotoneChain is based on a subarray of a list of points.
	/// This means that new arrays of points (potentially very large) do not
	/// have to be allocated.
	/// </para>
	/// 
	/// MonotoneChains support the following kinds of queries:
	/// <list type="number">
	/// <item>
	/// <description>
	/// Envelope select: determine all the segments in the chain which intersect 
	/// a given envelope.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// Overlap: determine all the pairs of segments in two chains whose
	/// envelopes overlap.
	/// </description>
	/// </item>
	/// </list>
	/// <para>
	/// This implementation of MonotoneChains uses the concept of internal iterators
	/// to return the resultsets for the above queries.
	/// This has time and space advantages, since it
	/// is not necessary to build lists of instantiated objects to represent the segments
	/// returned by the query.
	/// However, it does mean that the queries are not thread-safe.
	/// </para>
	/// </remarks>
    [Serializable]
    internal class MonotoneChain
	{
        #region Private Members
		
        private ICoordinateList pts;
		private int start, end;
		private Envelope env;
		private object context; // user-defined information
		private int id; // useful for optimizing chain comparisons

        #endregion
		
        #region Constructors and Destructor
		
        public MonotoneChain(ICoordinateList pts, int start, 
            int end, object context)
		{
			this.pts = pts;
			this.start = start;
			this.end = end;
			this.context = context;
		}

        #endregion
		
        #region Public Properties
		
        public int Id
		{
			get
			{
				return id;
			}
			
			set
			{
				this.id = value;
			}
		}
		
        public object Context
		{
			get
			{
				return context;
			}
		}
		
        public Envelope Envelope
		{
			get
			{
				if (env == null)
				{
					Coordinate p0 = pts[start];
					Coordinate p1 = pts[end];
					env = new Envelope(p0, p1);
				}
				return env;
			}
		}
		
        public int StartIndex
		{
			get
			{
				return start;
			}
		}

		public int EndIndex
		{
			get
			{
				return end;
			}
		}

		/// <summary> 
		/// Return the subsequence of coordinates forming this chain.
		/// Allocates a new array to hold the Coordinates
		/// </summary>
		public ICoordinateList Coordinates
		{
			get
			{
				ICoordinateList coord = new CoordinateCollection(end - start + 1);
				for (int i = start; i <= end; i++)
				{
					coord.Add(pts[i]);
				}
				return coord;
			}
		}
		
        #endregion

        #region Public Methods
		
        public void GetLineSegment(int index, LineSegment ls)
		{
			ls.p0 = pts[index];
			ls.p1 = pts[index + 1];
		}
		
		/// <summary> 
		/// Determine all the line segments in the chain whose envelopes overlap
		/// the searchEnvelope, and process them
		/// </summary>
		public void Select(Envelope searchEnv, MonotoneChainSelectAction mcs)
		{
			ComputeSelect(searchEnv, start, end, mcs);
		}
		
		public void ComputeOverlaps(MonotoneChain mc, MonotoneChainOverlapAction mco)
		{
			ComputeOverlaps(start, end, mc, mc.start, mc.end, mco);
		}

        #endregion
		
        #region Private Members

		private void ComputeSelect(Envelope searchEnv, int start0, int end0, 
            MonotoneChainSelectAction mcs)
		{
			Coordinate p0 = pts[start0];
			Coordinate p1 = pts[end0];
			mcs.tempEnv1.Initialize(p0, p1);
			
			// terminating condition for the recursion
			if (end0 - start0 == 1)
			{
				mcs.Select(this, start0);
				return;
			}

			// nothing to do if the envelopes don't overlap
			if (!searchEnv.Intersects(mcs.tempEnv1))
				return;
			
			// the chains overlap, so split each in half and iterate  (binary search)
			int mid = (start0 + end0) / 2;
			
			// Assert: mid != start or end (since we checked above for end - start <= 1)
			// check terminating conditions before recursing
			if (start0 < mid)
			{
				ComputeSelect(searchEnv, start0, mid, mcs);
			}

			if (mid < end0)
			{
				ComputeSelect(searchEnv, mid, end0, mcs);
			}
		}
		
		private void ComputeOverlaps(int start0, int end0, MonotoneChain mc, 
            int start1, int end1, MonotoneChainOverlapAction mco)
		{
			Coordinate p00 = pts[start0];
			Coordinate p01 = pts[end0];
			Coordinate p10 = mc.pts[start1];
			Coordinate p11 = mc.pts[end1];

			// terminating condition for the recursion
			if (end0 - start0 == 1 && end1 - start1 == 1)
			{
				mco.Overlap(this, start0, mc, start1);
				return ;
			}

			// nothing to do if the envelopes of these chains don't overlap
			mco.tempEnv1.Initialize(p00, p01);
			mco.tempEnv2.Initialize(p10, p11);
			if (!mco.tempEnv1.Intersects(mco.tempEnv2))
				return ;
			
			// the chains overlap, so split each in half and iterate  (binary search)
			int mid0 = (start0 + end0) / 2;
			int mid1 = (start1 + end1) / 2;
			
			// Assert: mid != start or end (since we checked above for end - start <= 1)
			// check terminating conditions before recursing
			if (start0 < mid0)
			{
				if (start1 < mid1)
					ComputeOverlaps(start0, mid0, mc, start1, mid1, mco);
				if (mid1 < end1)
					ComputeOverlaps(start0, mid0, mc, mid1, end1, mco);
			}
			if (mid0 < end0)
			{
				if (start1 < mid1)
					ComputeOverlaps(mid0, end0, mc, start1, mid1, mco);
				if (mid1 < end1)
					ComputeOverlaps(mid0, end0, mc, mid1, end1, mco);
			}
		}

        #endregion
	}
}