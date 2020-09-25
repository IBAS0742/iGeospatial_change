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
using System.Diagnostics;
using System.Collections;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Graphs
{
    /// <summary> 
    /// A DirectedEdgeStar is an ordered list of outgoing DirectedEdges around a node.
    /// It supports labelling the edges as well as linking the edges to form both
    /// MaximalEdgeRings and MinimalEdgeRings.
    /// </summary>
    [Serializable]
    internal class DirectedEdgeStar : EdgeEndStar
    {
        /// <summary> 
        /// A list of all outgoing edges in the result, in CCW order
        /// </summary>
        private ArrayList resultAreaEdgeList;
        private Label label;
		
        private const int SCANNING_FOR_INCOMING = 1;
        private const int LINKING_TO_OUTGOING = 2;

        public DirectedEdgeStar()
        {
        }

        public Label Label
        {
            get
            {
                return label;
            }
        }

        public DirectedEdge GetRightmostEdge()
        {
            IList edges = this.Edges;
            int size = edges.Count;
            if (size < 1)
                return null;
            DirectedEdge de0 = (DirectedEdge) edges[0];
            if (size == 1)
                return de0;
            DirectedEdge deLast = (DirectedEdge) edges[size - 1];
			
            int quad0 = de0.Quadrant;
            int quad1 = deLast.Quadrant;
            if (Quadrant.IsNorthern(quad0) && Quadrant.IsNorthern(quad1))
            {
                return de0;
            }
            else if (!Quadrant.IsNorthern(quad0) && !Quadrant.IsNorthern(quad1))
            {
                return deLast;
            }
            else
            {
                // edges are in different hemispheres - make sure we return one that is non-horizontal
                if (de0.Dy != 0)
                    return de0;
                else if (deLast.Dy != 0)
                    return deLast;
            }

            Debug.Assert(false, "Should never reach here: found two horizontal edges incident on node");

            return null;
        }

        private ArrayList GetResultAreaEdges()
        {
            if (resultAreaEdgeList != null)
                return resultAreaEdgeList;

            resultAreaEdgeList = new ArrayList();

            for (IEnumerator it = Iterator(); it.MoveNext(); )
            {
                DirectedEdge de = (DirectedEdge) it.Current;
                if (de.InResult || de.Sym.InResult)
                {
                    resultAreaEdgeList.Add(de);
                }
            }

            return resultAreaEdgeList;
        }
		
        /// <summary> Insert a directed edge in the list</summary>
        public override void Insert(EdgeEnd ee)
        {
            DirectedEdge de = (DirectedEdge) ee;
            InsertEdgeEnd(de, de);
        }
		
        public int GetOutgoingDegree()
        {
            int degree = 0;

            for (IEnumerator it = Iterator(); it.MoveNext(); )
            {
                DirectedEdge de = (DirectedEdge) it.Current;
                if (de.InResult)
                    degree++;
            }

            return degree;
        }

        public int GetOutgoingDegree(EdgeRing er)
        {
            int degree = 0;

            for (IEnumerator it = Iterator(); it.MoveNext(); )
            {
                DirectedEdge de = (DirectedEdge) it.Current;
                if (de.EdgeRing == er)
                    degree++;
            }

            return degree;
        }

        /// <summary> 
        /// Compute the labelling for all dirEdges in this star, as well as the 
        /// overall labelling.
        /// </summary>
        public override void ComputeLabelling(GeometryGraph[] geom)
        {
            base.ComputeLabelling(geom);
			
            // determine the overall labelling for this DirectedEdgeStar
            // (i.e. for the node it is based at)
            label = new Label(LocationType.None);

            for (IEnumerator it = Iterator(); it.MoveNext(); )
            {
                EdgeEnd ee = (EdgeEnd) it.Current;
                Edge e = ee.Edge;
                Label eLabel = e.Label;
                for (int i = 0; i < 2; i++)
                {
                    int eLoc = eLabel.GetLocation(i);
                    if (eLoc == LocationType.Interior || eLoc == LocationType.Boundary)
                        label.SetLocation(i, LocationType.Interior);
                }
            }
        }
		
        /// <summary> 
        /// For each dirEdge in the star, merge the label from the sym dirEdge into the label
        /// </summary>
        public void MergeSymLabels()
        {
            for (IEnumerator it = Iterator(); it.MoveNext(); )
            {
                DirectedEdge de = (DirectedEdge) it.Current;
                Label label = de.Label;
                label.Merge(de.Sym.Label);
            }
        }
		
        /// <summary> 
        /// Update incomplete dirEdge labels from the labelling for the node.
        /// </summary>
        public void UpdateLabelling(Label nodeLabel)
        {
            for (IEnumerator it = Iterator(); it.MoveNext(); )
            {
                DirectedEdge de = (DirectedEdge) it.Current;
                Label label = de.Label;
                label.SetAllLocationsIfNull(0, nodeLabel.GetLocation(0));
                label.SetAllLocationsIfNull(1, nodeLabel.GetLocation(1));
            }
        }
		
        /// <summary> 
        /// Traverse the star of DirectedEdges, linking the included edges together.
        /// </summary>
        /// <remarks>
        /// <para>
        /// To link two DirectedEdges, the "next" pointer for an incoming DirectedEdge
        /// is set to the next outgoing edge.
        /// </para>
        /// DirectedEdges are only linked if:
        /// <list type="number">
        /// <item>
        /// <description>
        /// they belong to an area (i.e. they have sides)
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// they are marked as being in the result
        /// </description>
        /// </item>
        /// </list>
        /// <para>
        /// Edges are linked in CCW order (the order they are stored).
        /// This means that rings have their face on the Right
        /// (in other words, the topological location of the face is given by 
        /// the right hand side label of the DirectedEdge)
        /// </para>
        /// PRECONDITION: No pair of DirectedEdges are both marked as being in the result.
        /// </remarks>
        public void LinkResultDirectedEdges()
        {
            // make sure edges are copied to resultAreaEdges list
            IList generatedAux = this.GetResultAreaEdges();
            if (generatedAux == null)
            {
                return;
            }

            // find first area edge (if any) to start linking at
            DirectedEdge firstOut = null;
            DirectedEdge incoming = null;
            int state = SCANNING_FOR_INCOMING;
            // link edges in CCW order
            for (int i = 0; i < resultAreaEdgeList.Count; i++)
            {
                DirectedEdge nextOut = (DirectedEdge) resultAreaEdgeList[i];
                DirectedEdge nextIn = nextOut.Sym;
				
                // skip de's that we're not interested in
                if (!nextOut.Label.IsArea())
                    continue;
				
                // record first outgoing edge, in order to link the last incoming edge
                if (firstOut == null && nextOut.InResult)
                    firstOut = nextOut;
				
                switch (state)
                {
                    case SCANNING_FOR_INCOMING: 
                        if (!nextIn.InResult)
                            continue;
                        incoming = nextIn;
                        state = LINKING_TO_OUTGOING;
                        break;
					
                    case LINKING_TO_OUTGOING: 
                        if (!nextOut.InResult)
                            continue;
                        incoming.Next = nextOut;
                        state = SCANNING_FOR_INCOMING;
                        break;
                }
            }

            if (state == LINKING_TO_OUTGOING)
            {
                if (firstOut == null)
                    throw new GeometryException("no outgoing dirEdge found", Coordinate);

                Debug.Assert(firstOut.InResult, "unable to link last incoming dirEdge");
                incoming.Next = firstOut;
            }
        }

        public void LinkMinimalDirectedEdges(EdgeRing er)
        {
            // find first area edge (if any) to start linking at
            DirectedEdge firstOut = null;
            DirectedEdge incoming = null;
            int state             = SCANNING_FOR_INCOMING;

            // link edges in CW order
            for (int i = resultAreaEdgeList.Count - 1; i >= 0; i--)
            {
                DirectedEdge nextOut = (DirectedEdge) resultAreaEdgeList[i];
                DirectedEdge nextIn  = nextOut.Sym;
				
                // record first outgoing edge, in order to link the last incoming edge
                if (firstOut == null && nextOut.EdgeRing == er)
                    firstOut = nextOut;
				
                switch (state)
                {
                    case SCANNING_FOR_INCOMING: 
                        if (nextIn.EdgeRing != er)
                            continue;
                        incoming = nextIn;
                        state = LINKING_TO_OUTGOING;
                        break;
					
                    case LINKING_TO_OUTGOING: 
                        if (nextOut.EdgeRing != er)
                            continue;
                        incoming.NextMin = nextOut;
                        state = SCANNING_FOR_INCOMING;
                        break;
                }
            }

            if (state == LINKING_TO_OUTGOING)
            {
                Debug.Assert(firstOut != null, "found null for first outgoing dirEdge");
                Debug.Assert(firstOut.EdgeRing == er, "unable to link last incoming dirEdge");
                incoming.NextMin = firstOut;
            }
        }

        public void LinkAllDirectedEdges()
        {
            IList generatedAux = this.Edges;
            if (generatedAux == null)
            {
                return;
            }

            // find first area edge (if any) to start linking at
            DirectedEdge prevOut = null;
            DirectedEdge firstIn = null;

            // link edges in CW order
            for (int i = edgeList.Count - 1; i >= 0; i--)
            {
                DirectedEdge nextOut = (DirectedEdge) edgeList[i];
                DirectedEdge nextIn  = nextOut.Sym;
                if (firstIn == null)
                    firstIn = nextIn;
                if (prevOut != null)
                    nextIn.Next = prevOut;
                // record outgoing edge, in order to link the last incoming edge
                prevOut = nextOut;
            }
            firstIn.Next = prevOut;
        }
		
        /// <summary> 
        /// Traverse the star of edges, maintaing the current location in the result
        /// area at this node (if any).
        /// If any L edges are found in the interior of the result, mark them as covered.
        /// </summary>
        public void FindCoveredLineEdges()
        {
            // Since edges are stored in CCW order around the node,
            // as we move around the ring we move from the right to the left side of the edge
			
            // Find first DirectedEdge of result area (if any).
            // The interior of the result is on the RHS of the edge,
            // so the start location will be:
            // - INTERIOR if the edge is outgoing
            // - EXTERIOR if the edge is incoming
            int startLoc = LocationType.None;

            for (IEnumerator it = Iterator(); it.MoveNext(); )
            {
                DirectedEdge nextOut = (DirectedEdge) it.Current;
                DirectedEdge nextIn = nextOut.Sym;
                if (!nextOut.LineEdge)
                {
                    if (nextOut.InResult)
                    {
                        startLoc = LocationType.Interior;
                        break;
                    }
                    if (nextIn.InResult)
                    {
                        startLoc = LocationType.Exterior;
                        break;
                    }
                }
            }

            // no A edges found, so can't determine if L edges are covered or not
            if (startLoc == LocationType.None)
                return ;
			
            // move around ring, keeping track of the current location
            // (Interior or Exterior) for the result area.
            // If L edges are found, mark them as covered if they are in the interior
            int currLoc = startLoc;

            for (IEnumerator it = Iterator(); it.MoveNext(); )
            {
                DirectedEdge nextOut = (DirectedEdge) it.Current;
                DirectedEdge nextIn = nextOut.Sym;
                if (nextOut.LineEdge)
                {
                    nextOut.Edge.Covered = currLoc == LocationType.Interior;
                }
                else
                {
                    // edge is an Area edge
                    if (nextOut.InResult)
                        currLoc = LocationType.Exterior;
                    if (nextIn.InResult)
                        currLoc = LocationType.Interior;
                }
            }
        }
		
        public void ComputeDepths(DirectedEdge de)
        {
            int edgeIndex = FindIndex(de);
//            Label label = de.Label;
            int startDepth = de.GetDepth(Position.Left);
            int targetLastDepth = de.GetDepth(Position.Right);

            // compute the depths from this edge up to the end of the edge array
            int nextDepth = ComputeDepths(edgeIndex + 1, edgeList.Count, startDepth);
            
            // compute the depths for the initial part of the array
            int lastDepth = ComputeDepths(0, edgeIndex, nextDepth);
            
            if (lastDepth != targetLastDepth)
            {
                throw new GeometryException("depth mismatch at " + de.Coordinate);
            }
        }
		
        /// <summary> 
        /// Compute the DirectedEdge depths for a subsequence of the edge array.
        /// </summary>
        /// <returns> 
        /// The last depth assigned (from the R side of the last edge visited)
        /// </returns>
        private int ComputeDepths(int startIndex, int endIndex, int startDepth)
        {
            int currDepth = startDepth;
            for (int i = startIndex; i < endIndex; i++)
            {
                DirectedEdge nextDe = (DirectedEdge) edgeList[i];
//                Label label = nextDe.Label;
                nextDe.SetEdgeDepths(Position.Right, currDepth);
                currDepth = nextDe.GetDepth(Position.Left);
            }

            return currDepth;
        }
    }
}