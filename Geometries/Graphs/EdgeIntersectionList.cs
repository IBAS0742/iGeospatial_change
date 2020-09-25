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

namespace iGeospatial.Geometries.Graphs
{
	/// <summary> A list of edge intersections along an Edge</summary>
	[Serializable]
    internal class EdgeIntersectionList
	{
		// a List of EdgeIntersections
		private IDictionary nodeMap;

		internal Edge edge; // the parent edge

		public EdgeIntersectionList(Edge edge)
		{
			nodeMap = new SortedList();
			this.edge = edge;
		}
		
		/// <summary> Adds an intersection into the list, if it isn't already there.
		/// The input segmentIndex and dist are expected to be normalized.
		/// </summary>
		/// <returns> the EdgeIntersection found or added
		/// </returns>
		public EdgeIntersection Add(Coordinate intPt, int segmentIndex, double dist)
		{
			EdgeIntersection eiNew = new EdgeIntersection(intPt, segmentIndex, dist);

            EdgeIntersection ei = (EdgeIntersection)nodeMap[eiNew];
			if (ei != null)
			{
				return ei;
			}

            nodeMap[eiNew] = eiNew;

			return eiNew;
		}
		
        /// <summary> Returns an iterator of {@link EdgeIntersection}s
        /// 
        /// </summary>
        /// <returns> an Iterator of EdgeIntersections
        /// </returns>
        public IEnumerator Iterator()
		{
			return nodeMap.Values.GetEnumerator();
		}

        /// <summary> Tests if the given point is an edge intersection
        /// 
        /// </summary>
        /// <param name="pt">the point to test
        /// </param>
        /// <returns> true if the point is an intersection
        /// </returns>
        public bool IsIntersection(Coordinate pt)
		{
			for (IEnumerator it = Iterator(); it.MoveNext(); )
			{
				EdgeIntersection ei = (EdgeIntersection) it.Current;
				if (ei.coord.Equals(pt))
					return true;
			}
			return false;
		}
		
		/// <summary> 
		/// Adds entries for the first and last points of the edge to the 
		/// list.
		/// </summary>
		public void AddEndpoints()
		{
			int maxSegIndex = edge.pts.Count - 1;
			Add(edge.pts[0], 0, 0.0);
			Add(edge.pts[maxSegIndex], maxSegIndex, 0.0);
		}
		
		/// <summary> Creates new edges for all the edges that the intersections in this
		/// list split the parent edge into.
		/// Adds the edges to the input list (this is so a single list
		/// can be used to accumulate all split edges for a Geometry).
		/// </summary>
        /// <param name="edgeList">a list of EdgeIntersections
        /// </param>
        public void AddSplitEdges(EdgeCollection edgeList)
		{
			// ensure that the list has entries for the first and last point of the edge
			AddEndpoints();
			
			IEnumerator it = Iterator();

			// there should always be at least two entries in the list
			it.MoveNext();	  //TODO--PAUL
			EdgeIntersection eiPrev = (EdgeIntersection) it.Current;

            while (it.MoveNext())
			{
				EdgeIntersection ei = (EdgeIntersection) it.Current;
				Edge newEdge = CreateSplitEdge(eiPrev, ei);
				edgeList.Add(newEdge);
				
				eiPrev = ei;
			}
		}

		/// <summary> Create a new "split edge" with the section of points between
		/// (and including) the two intersections.
		/// The label for the new edge is the same as the label for the parent edge.
		/// </summary>
		internal Edge CreateSplitEdge(EdgeIntersection ei0, EdgeIntersection ei1)
		{
			//Debug.Print("\ncreateSplitEdge"); Debug.Print(ei0); Debug.Print(ei1);
			int npts = ei1.segmentIndex - ei0.segmentIndex + 2;
			
			Coordinate lastSegStartPt = edge.pts[ei1.segmentIndex];
			// if the last intersection point is not equal to the its segment start pt,
			// Add it to the points list as well.
			// (This check is needed because the Distance metric is not totally reliable!)
			// The check for point equality is 2D only - Z values are ignored
			bool useIntPt1 = ei1.dist > 0.0 || !ei1.coord.Equals(lastSegStartPt);
			if (!useIntPt1)
			{
				npts--;
			}
			
			Coordinate[] pts = new Coordinate[npts];
			int ipt = 0;
			pts[ipt++] = new Coordinate(ei0.coord);
			for (int i = ei0.segmentIndex + 1; i <= ei1.segmentIndex; i++)
			{
				pts[ipt++] = edge.pts[i];
			}
			if (useIntPt1)
				pts[ipt] = ei1.coord;

			return new Edge(new CoordinateCollection(pts), new Label(edge.m_objLabel));
		}
	}
}