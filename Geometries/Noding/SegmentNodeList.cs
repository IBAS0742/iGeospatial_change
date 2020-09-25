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

namespace iGeospatial.Geometries.Noding
{
	/// <summary> 
	/// A list of the <see cref="SegmentNode"/>s present along a noded 
	/// <see cref="SegmentString"/>.
	/// </summary>
	[Serializable]
    internal class SegmentNodeList
	{
        #region Private Fields

		private IDictionary nodeMap;
		private SegmentString edge; // the parent edge
        
        #endregion
		
        #region Constructors and Destructor

		public SegmentNodeList(SegmentString edge)
		{
            this.nodeMap = new SortedList();
			this.edge    = edge;
		}
        
        #endregion
		
        #region Public Properties

        public SegmentString Edge
        {
            get
            {
                return this.edge;
            }
        }

        #endregion

		/// <summary> 
		/// Adds an intersection into the list, if it isn't already there.
		/// The input segmentIndex and dist are expected to be normalized.
		/// </summary>
		/// <returns> the SegmentIntersection found or added
		/// </returns>
		public virtual SegmentNode Add(Coordinate intPt, int segmentIndex)
		{
			SegmentNode eiNew = new SegmentNode(edge, intPt, segmentIndex, 
                edge.GetSegmentOctant(segmentIndex));

            SegmentNode ei = (SegmentNode)nodeMap[eiNew];
			if (ei != null)
			{
				return ei;
			}

            // node does not exist, so create it
            nodeMap[eiNew] = eiNew;

			return eiNew;
		}

		public IEnumerator Iterator()
		{
			return nodeMap.Values.GetEnumerator();
		}

		/// <summary> 
		/// Adds nodes for the first and last points of the edge.
		/// </summary>
		private void AddEndpoints()
		{
			int maxSegIndex = edge.Count - 1;

			Add(edge.GetCoordinate(0), 0);
			Add(edge.GetCoordinate(maxSegIndex), maxSegIndex);
		}
		
        /// <summary> Adds nodes for any collapsed edge pairs.
        /// Collapsed edge pairs can be caused by inserted nodes, or they can be
        /// pre-existing in the edge vertex list.
        /// In order to provide the correct fully noded semantics,
        /// the vertex at the base of a collapsed pair must also be added as a node.
        /// </summary>
        private void AddCollapsedNodes()
        {
            IList collapsedVertexIndexes = new ArrayList();
			
            FindCollapsesFromInsertedNodes(collapsedVertexIndexes);
            FindCollapsesFromExistingVertices(collapsedVertexIndexes);
			
            // node the collapses
            for (IEnumerator it = collapsedVertexIndexes.GetEnumerator(); it.MoveNext(); )
            {
                int vertexIndex = (int)it.Current;

                Add(edge.GetCoordinate(vertexIndex), vertexIndex);
            }
        }
		
        /// <summary> Adds nodes for any collapsed edge pairs
        /// which are pre-existing in the vertex list.
        /// </summary>
        private void FindCollapsesFromExistingVertices(IList collapsedVertexIndexes)
        {
            int nCount = edge.Count;

            for (int i = 0; i < nCount - 2; i++)
            {
                Coordinate p0 = edge.GetCoordinate(i);
                Coordinate p1 = edge.GetCoordinate(i + 1);
                Coordinate p2 = edge.GetCoordinate(i + 2);
                if (p0.Equals(p2))
                {
                    // add base of collapse as node
                    collapsedVertexIndexes.Add((int)(i + 1));
                }
            }
        }
		
        /// <summary> Adds nodes for any collapsed edge pairs caused by inserted nodes
        /// Collapsed edge pairs occur when the same coordinate is inserted as a node
        /// both before and after an existing edge vertex.
        /// To provide the correct fully noded semantics,
        /// the vertex must be added as a node as well.
        /// </summary>
        private void FindCollapsesFromInsertedNodes(
            IList collapsedVertexIndexes)
        {
            int[] collapsedVertexIndex = new int[1];
            IEnumerator it = Iterator();
            // there should always be at least two entries in the list, since the endpoints are nodes
            it.MoveNext(); //PAUL
            SegmentNode eiPrev = (SegmentNode) it.Current;
            while (it.MoveNext())
            {
                SegmentNode ei   = (SegmentNode) it.Current;
                bool isCollapsed = FindCollapseIndex(eiPrev, 
                    ei, collapsedVertexIndex);

                if (isCollapsed)
                    collapsedVertexIndexes.Add((int)collapsedVertexIndex[0]);
				
                eiPrev = ei;
            }
        }
		
        private bool FindCollapseIndex(SegmentNode ei0, 
            SegmentNode ei1, int[] collapsedVertexIndex)
        {
            // only looking for equal nodes
            if (!ei0.Coordinate.Equals(ei1.Coordinate))
                return false;
			
            int numVerticesBetween = ei1.SegmentIndex - ei0.SegmentIndex;
            if (!ei1.Interior)
            {
                numVerticesBetween--;
            }
			
            // if there is a single vertex between the two equal nodes, this is a collapse
            if (numVerticesBetween == 1)
            {
                collapsedVertexIndex[0] = ei0.SegmentIndex + 1;
                return true;
            }

            return false;
        }
				
		/// <summary> 
		/// Creates new edges for all the edges that the intersections in 
		/// this list split the parent edge into.
		/// Adds the edges to the input list (this is so a single list
		/// can be used to accumulate all split edges for a Geometry).
		/// </summary>
		public void AddSplitEdges(IList edgeList)
		{
			// ensure that the list has entries for the first and last point of the edge
			AddEndpoints();
            AddCollapsedNodes();
			
			IEnumerator it = Iterator();
			// there should always be at least two entries in the list,
            // since t he endpoints are nodes
			it.MoveNext(); //PAUL
			SegmentNode eiPrev = (SegmentNode) it.Current;

            while (it.MoveNext())
			{
				SegmentNode ei = (SegmentNode) it.Current;
				SegmentString newEdge = CreateSplitEdge(eiPrev, ei);
				edgeList.Add(newEdge);
				
				eiPrev = ei;
			}
		}
		
        /// <summary> 
        /// Checks the correctness of the set of split edges corresponding 
        /// to this edge.
        /// </summary>
        /// <param name="splitEdges">
        /// The split edges for this edge (in order).
        /// </param>
        private void CheckSplitEdgesCorrectness(IList splitEdges)
		{
			ICoordinateList edgePts = edge.Coordinates;
			
			// check that first and last points of split edges are same 
            //as endpoints of edge
			SegmentString split0 = (SegmentString)splitEdges[0];
			Coordinate pt0       = split0.GetCoordinate(0);
			if (!pt0.Equals(edgePts[0]))
			{
				throw new GeometryException("bad split edge start point at " + pt0);
			}
			
			SegmentString splitn      = (SegmentString)splitEdges[splitEdges.Count - 1];
			ICoordinateList splitnPts = splitn.Coordinates;
			Coordinate ptn            = splitnPts[splitnPts.Count - 1];
			if (!ptn.Equals(edgePts[edgePts.Count - 1]))
			{
				throw new GeometryException("bad split edge end point at " + ptn);
			}
		}

		/// <summary> Create a new "split edge" with the section of points between
		/// (and including) the two intersections.
		/// The label for the new edge is the same as the label for the parent edge.
		/// </summary>
		internal SegmentString CreateSplitEdge(SegmentNode ei0, SegmentNode ei1)
		{
			int npts = ei1.SegmentIndex - ei0.SegmentIndex + 2;
			
			Coordinate lastSegStartPt = edge.GetCoordinate(ei1.SegmentIndex);
			// if the last intersection point is not equal to the its segment start pt,
			// Add it to the points list as well.
			// (This check is needed because the Distance metric is not totally reliable!)
			// The check for point equality is 2D only - Z values are ignored
			bool useIntPt1 = ei1.Interior || !ei1.Coordinate.Equals(lastSegStartPt);
			if (!useIntPt1)
			{
				npts--;
			}
			
			Coordinate[] pts = new Coordinate[npts]; //TODO-PAUL -- rework
			int ipt = 0;
			pts[ipt++] = new Coordinate(ei0.Coordinate);
			for (int i = ei0.SegmentIndex + 1; i <= ei1.SegmentIndex; i++)
			{
				pts[ipt++] = edge.GetCoordinate(i);
			}
			
            if (useIntPt1)
				pts[ipt] = ei1.Coordinate;

			return new SegmentString(new CoordinateCollection(pts), edge.Data);
		}
	}
}