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

namespace iGeospatial.Geometries.Graphs.Index
{
	
	/// <summary> Finds all intersections in one or two sets of edges,
	/// using an x-axis sweepline algorithm in conjunction with Monotone Chains.
	/// While still O(n^2) in the worst case, this algorithm
	/// drastically improves the average-case time.
	/// The use of MonotoneChains as the items in the index
	/// seems to offer an improvement in performance over a sweep-line alone.
	/// 
	/// </summary>
	[Serializable]
    internal class SimpleMCSweepLineIntersector : EdgeSetIntersector
	{
        #region Private Fields

		private ArrayList events;
		// statistics information
		private int nOverlaps;
        
        #endregion
		
        #region Constructors and Destructor

		/// <summary> A SimpleMCSweepLineIntersector creates monotone chains from the edges
		/// and compares them using a simple sweep-line along the x-axis.
		/// </summary>
		public SimpleMCSweepLineIntersector()
		{
            events = new ArrayList();
		}

        #endregion
		
        #region Public Methods

		public override void ComputeIntersections(EdgeCollection edges, 
            SegmentIntersector si, bool testAllSegments)
		{
			if (testAllSegments)
				Add(edges, null);
			else
				Add(edges);

			ComputeIntersections(si);
		}
		
		public override void ComputeIntersections(EdgeCollection edges0, 
            EdgeCollection edges1, SegmentIntersector si)
		{
			Add(edges0, edges0);
			Add(edges1, edges1);
			ComputeIntersections(si);
		}
        
        #endregion
		
        #region Private Methods

		private void Add(EdgeCollection edges)
		{
			for (IEdgeEnumerator i = edges.GetEnumerator(); i.MoveNext(); )
			{
				Edge edge = i.Current;
				// edge is its own group
				Add(edge, edge);
			}
		}

		private void Add(EdgeCollection edges, object edgeSet)
		{
			for (IEdgeEnumerator i = edges.GetEnumerator(); i.MoveNext(); )
			{
				Edge edge = i.Current;
				Add(edge, edgeSet);
			}
		}
		
		private void Add(Edge edge, object edgeSet)
		{
			MonotoneChainEdge mce = edge.MonotoneChainEdge;
			int[] startIndex = mce.StartIndexes;
			for (int i = 0; i < startIndex.Length - 1; i++)
			{
				MonotoneChain mc = new MonotoneChain(mce, i);
				SweepLineEvent insertEvent = new SweepLineEvent(edgeSet, mce.GetMinX(i), null, mc);

                events.Add(insertEvent);

                events.Add(new SweepLineEvent(edgeSet, mce.GetMaxX(i), insertEvent, mc));
			}
		}
		
		/// <summary> 
		/// Because Delete Events have a link to their corresponding Insert event,
		/// it is possible to compute exactly the range of events which must be
		/// compared to a given Insert event object.
		/// </summary>
		private void PrepareEvents()
		{
            events.Sort();
			for (int i = 0; i < events.Count; i++)
			{
				SweepLineEvent ev = (SweepLineEvent) events[i];
				if (ev.IsDelete)
				{
					ev.InsertEvent.DeleteEventIndex = i;
				}
			}
		}
		
		private void ComputeIntersections(SegmentIntersector si)
		{
			nOverlaps = 0;
			PrepareEvents();
			
			for (int i = 0; i < events.Count; i++)
			{
				SweepLineEvent ev = (SweepLineEvent) events[i];
				if (ev.IsInsert)
				{
					ProcessOverlaps(i, ev.DeleteEventIndex, ev, si);
				}
			}
		}
		
		private void ProcessOverlaps(int start, int end, 
            SweepLineEvent ev0, SegmentIntersector si)
		{
			MonotoneChain mc0 = (MonotoneChain) ev0.Object;
			/// <summary> Since we might need to test for self-intersections,
			/// include current insert event object in list of event objects to test.
			/// Last index can be skipped, because it must be a Delete event.
			/// </summary>
			for (int i = start; i < end; i++)
			{
				SweepLineEvent ev1 = (SweepLineEvent) events[i];
				if (ev1.IsInsert)
				{
					MonotoneChain mc1 = (MonotoneChain) ev1.Object;
					// don't Compare edges in same group
					// null group indicates that edges should be compared
					if (ev0.EdgeSet == null || (ev0.EdgeSet != ev1.EdgeSet))
					{
						mc0.ComputeIntersections(mc1, si);
						nOverlaps++;
					}
				}
			}
		}
        
        #endregion
	}
}