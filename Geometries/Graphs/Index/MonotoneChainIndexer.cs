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

using iGeospatial.Collections;
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
	internal sealed class MonotoneChainIndexer
	{
        #region Constructors and Destructor

		private MonotoneChainIndexer()
		{
		}

        #endregion
		
        #region Public Methods

		public static int[] GetChainStartIndices(ICoordinateList pts)
		{
			// find the startpoint (and endpoints) of all monotone chains in this edge
			int start = 0;
			IntegerCollection startIndexList = new IntegerCollection();

            startIndexList.Add(start);
			do 
			{
				int last = FindChainEnd(pts, start);

                startIndexList.Add(last);
				start = last;
			}
			while (start < pts.Count - 1);

			// copy list to an array of ints, for efficiency
			int[] startIndex = startIndexList.ToArray();

			return startIndex;
		}
        
        #endregion
		
        #region Private Methods

		/// <returns>
		/// The index of the last point in the monotone chain
		/// </returns>
		private static int FindChainEnd(ICoordinateList pts, int start)
		{
			// determine quadrant for chain
			int chainQuad = Quadrant.GetQuadrant(pts[start], pts[start + 1]);
			int last = start + 1;
			while (last < pts.Count)
			{
				// compute quadrant for next possible segment in chain
				int quad = Quadrant.GetQuadrant(pts[last - 1], pts[last]);
				if (quad != chainQuad)
					break;
				last++;
			}
			return last - 1;
		}
        
        #endregion
	}
}