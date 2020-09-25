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
using iGeospatial.Collections;
using iGeospatial.Geometries.Graphs;

namespace iGeospatial.Geometries.Indexers.Chain
{
	/// <summary> 
	/// A MonotoneChainBuilder implements functions to determine the monotone chains
	/// in a sequence of points.
	/// </summary>
    [Serializable]
    internal class MonotoneChainBuilder
	{
        public MonotoneChainBuilder()
        {
        }
		
		public static IList GetChains(ICoordinateList pts)
		{
			return GetChains(pts, null);
		}
		
		/// <summary> 
		/// Return a list of the <see cref="MonotoneChain"/>s for the given 
		/// list of coordinates.
		/// </summary>
		public static IList GetChains(ICoordinateList pts, object context)
		{
			ArrayList mcList = new ArrayList();
			int[] startIndex = GetChainStartIndices(pts);
			for (int i = 0; i < startIndex.Length - 1; i++)
			{
				MonotoneChain mc = new MonotoneChain(pts, startIndex[i], 
                    startIndex[i + 1], context);

				mcList.Add(mc);
			}

			return mcList;
		}
		
		/// <summary> 
		/// Return an array containing lists of start/end indexes of the monotone chains
		/// for the given list of coordinates.
		/// The last entry in the array points to the end point of the point array,
		/// for use as a sentinel.
		/// </summary>
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
		
		/// <returns> 
		/// the index of the last point in the monotone chain starting at start.
		/// </returns>
		private static int FindChainEnd(ICoordinateList pts, int start)
		{
			// determine quadrant for chain
			int chainQuad = Quadrant.GetQuadrant(pts[start], pts[start + 1]);
			int last      = start + 1;
			while (last < pts.Count)
			{
				// compute quadrant for next possible segment in chain
				int quad = Quadrant.GetQuadrant(pts[last - 1], pts[last]);
				if (quad != chainQuad)
					break;

				last++;
			}

			return (last - 1);
		}
	}
}