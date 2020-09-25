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
using System.Diagnostics;

using iGeospatial.Coordinates;
using iGeospatial.Collections;
using iGeospatial.Collections.Sets;
using iGeospatial.Geometries.PlanarGraphs;

namespace iGeospatial.Geometries.Operations.Polygonize
{
	/// <summary> 
	/// Represents a planar graph of edges that can be used to compute a
	/// polygonization, and implements the algorithms to compute the
	/// <see cref="EdgeRing"/>s formed by the graph.
	/// <para>
	/// The marked flag on <see cref="DirectedEdge"/>s is used to indicate that 
	/// a directed edge has be logically deleted from the graph.
	/// </para>
	/// </summary>
	internal class PolygonizeGraph : PlanarGraph
	{
        #region Private Fields

		private GeometryFactory factory;
        
        #endregion
		
        #region Constructors and Destructor

		/// <summary> Create a new polygonization graph.</summary>
		public PolygonizeGraph(GeometryFactory factory)
		{
			this.factory = factory;
		}
        
        #endregion
		
        #region Public Properties

		/// <summary> Computes the EdgeRings formed by the edges in this graph.</summary>
		/// <returns> 
		/// A list of the <see cref="EdgeRing"/>s found by the polygonization process.
		/// </returns>
		public ArrayList EdgeRings
		{
			get
			{
				// maybe could optimize this, since most of these pointers 
                // should be set correctly already by DeleteCutEdges()
				ComputeNextCWEdges();
				// clear labels of all edges in graph
				Label(dirEdges, - 1);

                ArrayList maximalRings = FindLabeledEdgeRings(dirEdges);
				ConvertMaximalToMinimalEdgeRings(maximalRings);
				
				// find all edgerings
				ArrayList edgeRingList = new ArrayList();

                for (IEnumerator i = dirEdges.GetEnumerator(); i.MoveNext(); )
				{
					PolygonizeDirectedEdge de = (PolygonizeDirectedEdge) i.Current;
					if (de.Marked)
						continue;
					if (de.InRing)
						continue;
					
					EdgeRing er = FindEdgeRing(de);

					edgeRingList.Add(er);
				}
				return edgeRingList;
			}
			
		}
        
        #endregion
		
        #region Public Methods

		/// <summary> Deletes all edges at a node</summary>
		public static void  DeleteAllEdges(Node node)
		{
			ArrayList edges = node.OutEdges.Edges;

            for (IEnumerator i = edges.GetEnumerator(); i.MoveNext(); )
			{
				PolygonizeDirectedEdge de = (PolygonizeDirectedEdge) i.Current;
				de.Marked = true;
				PolygonizeDirectedEdge sym = (PolygonizeDirectedEdge) de.Sym;
				if (sym != null)
					sym.Marked = true;
			}
		}
		
		/// <summary> 
		/// Add a <see cref="LineString"/> forming an edge of the polygon graph.
		/// </summary>
		/// <param name="line">the line to Add
		/// </param>
		public void AddEdge(LineString line)
		{
			if (line.IsEmpty)
			{
				return ;
			}

			ICoordinateList linePts = 
                CoordinateCollection.RemoveRepeatedCoordinates(line.Coordinates);
			Coordinate startPt = linePts[0];
			Coordinate endPt = linePts[linePts.Count - 1];
			
			Node nStart = GetNode(startPt);
			Node nEnd = GetNode(endPt);
			
			DirectedEdge de0 = new PolygonizeDirectedEdge(nStart, nEnd, linePts[1], true);
			DirectedEdge de1 = new PolygonizeDirectedEdge(nEnd, nStart, 
                linePts[linePts.Count - 2], false);
			Edge edge = new PolygonizeEdge(line);
			edge.SetDirectedEdges(de0, de1);
			Add(edge);
		}
		
		/// <summary> Finds and removes all cut edges from the graph.</summary>
		/// <returns> a list of the <see cref="LineString"/>s forming the removed cut edges
		/// </returns>
		public ArrayList DeleteCutEdges()
		{
			ComputeNextCWEdges();
			// label the current set of edgerings
			FindLabeledEdgeRings(dirEdges);
			
			/// <summary> Cut Edges are edges where both dirEdges have the same label.
			/// Delete them, and record them
			/// </summary>
			ArrayList cutLines = new ArrayList();

			for (IEnumerator i = dirEdges.GetEnumerator(); i.MoveNext(); )
			{
				PolygonizeDirectedEdge de = (PolygonizeDirectedEdge) i.Current;
				if (de.Marked)
					continue;
				
				PolygonizeDirectedEdge sym = (PolygonizeDirectedEdge) de.Sym;
				
				if (de.Label == sym.Label)
				{
					de.Marked = true;
					sym.Marked = true;
					
					// save the line as a cut edge
					PolygonizeEdge e = (PolygonizeEdge) de.Edge;

                    cutLines.Add(e.Line);
				}
			}
			return cutLines;
		}
		
		/// <summary> Marks all edges from the graph which are "dangles".
		/// Dangles are which are incident on a node with degree 1.
		/// This process is recursive, since removing a dangling edge
		/// may result in another edge becoming a dangle.
		/// In order to handle large recursion depths efficiently,
		/// an explicit recursion stack is used
		/// 
		/// </summary>
		/// <returns> a List containing the {@link LineStrings} that formed dangles
		/// </returns>
		public ICollection DeleteDangles()
		{
			ArrayList nodesToRemove = FindNodesOfDegree(1);
			ISet dangleLines        = new HashedSet();
			
			Stack nodeStack = new Stack();
			for (IEnumerator i = nodesToRemove.GetEnumerator(); i.MoveNext(); )
			{
                nodeStack.Push(i.Current);
			}
			
			while (!(nodeStack.Count == 0))
			{
				Node node = (Node) nodeStack.Pop();
				
				DeleteAllEdges(node);

                ArrayList nodeOutEdges = node.OutEdges.Edges;

                for (IEnumerator i = nodeOutEdges.GetEnumerator(); i.MoveNext(); )
				{
					PolygonizeDirectedEdge de = (PolygonizeDirectedEdge) i.Current;
					// delete this edge and its sym
					de.Marked = true;
					PolygonizeDirectedEdge sym = (PolygonizeDirectedEdge) de.Sym;
					if (sym != null)
						sym.Marked = true;
					
					// save the line as a dangle
					PolygonizeEdge e = (PolygonizeEdge) de.Edge;
					dangleLines.Add(e.Line);
					
					Node toNode = de.ToNode;
					// Add the toNode to the list to be processed, if it is now a dangle
					if (GetDegreeNonDeleted(toNode) == 1)
                    {
                        nodeStack.Push(toNode);
                    }
				}
			}

			return dangleLines;
		}
        
        #endregion
		
        #region Private Methods

        private static int GetDegreeNonDeleted(Node node)
        {
            ArrayList edges = node.OutEdges.Edges;
            int degree = 0;

            for (IEnumerator i = edges.GetEnumerator(); i.MoveNext(); )
            {
                PolygonizeDirectedEdge de = (PolygonizeDirectedEdge) i.Current;
                if (!de.Marked)
                    degree++;
            }
            return degree;
        }
		
        private static int GetDegree(Node node, long label)
        {
            ArrayList edges = node.OutEdges.Edges;
            int degree = 0;

            for (IEnumerator i = edges.GetEnumerator(); i.MoveNext(); )
            {
                PolygonizeDirectedEdge de = (PolygonizeDirectedEdge) i.Current;
                if (de.Label == label)
                    degree++;
            }
            return degree;
        }
		
		private Node GetNode(Coordinate pt)
		{
			Node node = FindNode(pt);
			if (node == null)
			{
				node = new Node(pt);
				// ensure node is only added once to graph
				Add(node);
			}
			return node;
		}
		
		private void ComputeNextCWEdges()
		{
			// set the next pointers for the edges around each node
			for (IEnumerator iNode = NodeIterator(); iNode.MoveNext(); )
			{
				Node node = (Node) iNode.Current;
				ComputeNextCWEdges(node);
			}
		}
		
		/// <summary> 
		/// Convert the maximal edge rings found by the initial graph traversal
		/// into the minimal edge rings required by OTS polygon topology rules.
		/// </summary>
		/// <param name="ringEdges">the list of start edges for the edgeRings to convert.
		/// </param>
		private void ConvertMaximalToMinimalEdgeRings(ArrayList ringEdges)
		{
			for (IEnumerator i = ringEdges.GetEnumerator(); i.MoveNext(); )
			{
				PolygonizeDirectedEdge de = (PolygonizeDirectedEdge) i.Current;
				long label = de.Label;

                ArrayList intNodes = FindIntersectionNodes(de, label);
				
				if (intNodes == null)
					continue;

				// flip the next pointers on the intersection nodes to create minimal edge rings
				for (IEnumerator iNode = intNodes.GetEnumerator(); iNode.MoveNext(); )
				{
					Node node = (Node) iNode.Current;
					ComputeNextCCWEdges(node, label);
				}
			}
		}
		
		/// <summary> 
		/// Finds all nodes in a maximal edgering which are self-intersection nodes.
		/// </summary>
		/// <param name="">startDE
		/// </param>
		/// <param name="">label
		/// </param>
		/// <returns> the list of intersection nodes found,
		/// or null if no intersection nodes were found
		/// </returns>
		private static ArrayList FindIntersectionNodes(PolygonizeDirectedEdge startDE, 
            long label)
		{
			PolygonizeDirectedEdge de = startDE;

            ArrayList intNodes = null;
			do 
			{
				Node node = de.FromNode;
				if (GetDegree(node, label) > 1)
				{
					if (intNodes == null)
					{
						intNodes = new ArrayList();
					}

                    intNodes.Add(node);
				}
				
				de = de.Next;
				Debug.Assert(de != null, "found null DE in ring");
				Debug.Assert(de == startDE || !de.InRing, "found DE already in ring");
			}
			while (de != startDE);
			
			return intNodes;
		}
		
		/// <summary> </summary>
		/// <param name="dirEdges">a List of the DirectedEdges in the graph
		/// </param>
		/// <returns> a List of DirectedEdges, one for each edge ring found
		/// </returns>
		private static ArrayList FindLabeledEdgeRings(IList dirEdges)
		{
			ArrayList edgeRingStarts = new ArrayList();
			// label the edge rings formed
			long currLabel = 1;

			for (IEnumerator i = dirEdges.GetEnumerator(); i.MoveNext(); )
			{
				PolygonizeDirectedEdge de = (PolygonizeDirectedEdge) i.Current;
				if (de.Marked)
					continue;
				if (de.Label >= 0)
					continue;
				
				edgeRingStarts.Add(de);
				ArrayList edges = FindDirEdgesInRing(de);
				
				Label(edges, currLabel);
				currLabel++;
			}
			return edgeRingStarts;
		}
		
		private static void Label(IList dirEdges, long label)
		{
			for (IEnumerator i = dirEdges.GetEnumerator(); i.MoveNext(); )
			{
				PolygonizeDirectedEdge de = (PolygonizeDirectedEdge) i.Current;
				de.Label = label;
			}
		}

		private static void ComputeNextCWEdges(Node node)
		{
			DirectedEdgeStar deStar = node.OutEdges;
			PolygonizeDirectedEdge startDE = null;
			PolygonizeDirectedEdge prevDE = null;
			
			// the edges are stored in CCW order around the star
			for (IEnumerator i = deStar.Edges.GetEnumerator(); i.MoveNext(); )
			{
				PolygonizeDirectedEdge outDE = (PolygonizeDirectedEdge) i.Current;
				if (outDE.Marked)
					continue;
				
				if (startDE == null)
					startDE = outDE;
				if (prevDE != null)
				{
					PolygonizeDirectedEdge sym = (PolygonizeDirectedEdge) prevDE.Sym;
					sym.Next = outDE;
				}
				prevDE = outDE;
			}
			if (prevDE != null)
			{
				PolygonizeDirectedEdge sym = (PolygonizeDirectedEdge) prevDE.Sym;
				sym.Next = startDE;
			}
		}

		/// <summary> 
		/// Computes the next edge pointers going CCW around the given node, for the
		/// given edgering label.
		/// This algorithm has the effect of converting maximal edgerings into 
		/// minimal edgerings.
		/// </summary>
		private static void ComputeNextCCWEdges(Node node, long label)
		{
			DirectedEdgeStar deStar = node.OutEdges;
			//PolyDirectedEdge lastInDE = null;
			PolygonizeDirectedEdge firstOutDE = null;
			PolygonizeDirectedEdge prevInDE = null;
			
			// the edges are stored in CCW order around the star
			ArrayList edges = deStar.Edges;

			//for (Iterator i = deStar.getEdges().Iterator(); i.hasNext(); ) {
			for (int i = edges.Count - 1; i >= 0; i--)
			{
				PolygonizeDirectedEdge de = (PolygonizeDirectedEdge) edges[i];
				PolygonizeDirectedEdge sym = (PolygonizeDirectedEdge) de.Sym;
				
				PolygonizeDirectedEdge outDE = null;
				if (de.Label == label)
					outDE = de;
				PolygonizeDirectedEdge inDE = null;
				if (sym.Label == label)
					inDE = sym;
				
				if (outDE == null && inDE == null)
					continue; // this edge is not in edgering
				
				if (inDE != null)
				{
					prevInDE = inDE;
				}
				
				if (outDE != null)
				{
					if (prevInDE != null)
					{
						prevInDE.Next = outDE;
						prevInDE = null;
					}
					if (firstOutDE == null)
						firstOutDE = outDE;
				}
			}
			if (prevInDE != null)
			{
				Debug.Assert(firstOutDE != null);
				prevInDE.Next = firstOutDE;
			}
		}
		
		/// <summary> Traverse a ring of DirectedEdges, accumulating them into a list.
		/// This assumes that all dangling directed edges have been removed
		/// from the graph, so that there is always a next dirEdge.
		/// 
		/// </summary>
		/// <param name="startDE">the DirectedEdge to start traversing at
		/// </param>
		/// <returns> a List of DirectedEdges that form a ring
		/// </returns>
		private static ArrayList FindDirEdgesInRing(PolygonizeDirectedEdge startDE)
		{
			PolygonizeDirectedEdge de = startDE;
			ArrayList edges = new ArrayList();
			do 
			{
				edges.Add(de);
				de = de.Next;
				Debug.Assert(de != null, "found null DE in ring");
				Debug.Assert(de == startDE || !de.InRing, "found DE already in ring");
			}
			while (de != startDE);
			
			return edges;
		}
		
		private EdgeRing FindEdgeRing(PolygonizeDirectedEdge startDE)
		{
			PolygonizeDirectedEdge de = startDE;
			EdgeRing er = new EdgeRing(factory);
			do 
			{
				er.Add(de);
				de.Ring = er;
				de = de.Next;
				Debug.Assert(de != null, "found null DE in ring");
				Debug.Assert(de == startDE || !de.InRing, "found DE already in ring");
			}
			while (de != startDE);
			
			return er;
		}
        
        #endregion
	}
}