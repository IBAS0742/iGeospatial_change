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
	/// using the straightforward method of
	/// comparing all segments.
	/// This algorithm is too slow for production use, but is useful for testing purposes.
	/// </summary>
	[Serializable]
    internal class SimpleEdgeSetIntersector : EdgeSetIntersector
	{
        #region Construtors and Destructor

		public SimpleEdgeSetIntersector()
		{
		}

        #endregion
		
        #region Public Methods

		public override void ComputeIntersections(EdgeCollection edges, SegmentIntersector si, bool testAllSegments)
		{
			for (IEdgeEnumerator i0 = edges.GetEnumerator(); i0.MoveNext(); )
			{
				Edge edge0 = i0.Current;
				for (IEdgeEnumerator i1 = edges.GetEnumerator(); i1.MoveNext(); )
				{
					Edge edge1 = i1.Current;
					if (testAllSegments || edge0 != edge1)
						ComputeIntersects(edge0, edge1, si);
				}
			}
		}
		
		public override void ComputeIntersections(EdgeCollection edges0, 
            EdgeCollection edges1, SegmentIntersector si)
		{
			for (IEdgeEnumerator i0 = edges0.GetEnumerator(); i0.MoveNext(); )
			{
				Edge edge0 = i0.Current;

                for (IEdgeEnumerator i1 = edges1.GetEnumerator(); i1.MoveNext(); )
				{
					Edge edge1 = i1.Current;
					ComputeIntersects(edge0, edge1, si);
				}
			}
		}
        
        #endregion
		
        #region Private Methods

		/// <summary> 
		/// Performs a brute-force comparison of every segment in each Edge.
		/// This has n^2 performance, and is about 100 times slower than using
		/// monotone chains.
		/// </summary>
		private void ComputeIntersects(Edge e0, Edge e1, SegmentIntersector si)
		{
			ICoordinateList pts0 = e0.Coordinates;
			ICoordinateList pts1 = e1.Coordinates;
			for (int i0 = 0; i0 < pts0.Count - 1; i0++)
			{
				for (int i1 = 0; i1 < pts1.Count - 1; i1++)
				{
					si.AddIntersections(e0, i0, e1, i1);
				}
			}
		}
        
        #endregion
	}
}