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
using iGeospatial.Collections;
using iGeospatial.Collections.Sets;

using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Operations.Buffer
{
	/// <summary> 
	/// A connected subset of the graph of <see cref="DirectedEdge"/>s and 
	/// <see cref="Node"/>s.
	/// Its edges will generate either
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// a single polygon in the complete Buffer, with zero or more holes, or
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// one or more connected holes.
	/// </description>
	/// </item>
	/// </list>
	/// </summary>
	[Serializable]
    internal class BufferSubgraph : IComparable
	{
        #region Private Fields

        private RightmostEdgeFinder finder;
        private ArrayList dirEdgeList;
        private ArrayList nodes;
        private Coordinate rightMostCoord;
        private Envelope   env;
        
        #endregion

        #region Constructors and Destructor

        public BufferSubgraph()
        {
            dirEdgeList = new ArrayList();
            nodes       = new ArrayList();
            finder      = new RightmostEdgeFinder();
        }
        
        #endregion
		
        #region Public Properties

		public ArrayList DirectedEdges
		{
			get
			{
				return dirEdgeList;
			}
		}

		public ArrayList Nodes
		{
			get
			{
				return nodes;
			}
		}

		/// <summary> Gets the rightmost coordinate in the edges of the subgraph</summary>
		public Coordinate RightmostCoordinate
		{
			get
			{
				return rightMostCoord;
			}
		}
			
        /// <summary> Computes the envelope of the edges in the subgraph.
        /// The envelope is cached after being computed.
        /// 
        /// </summary>
        /// <returns> the envelope of the graph.
        /// </returns>
        public Envelope Envelope
        {
            get
            {
                if (env == null)
                {
                    Envelope edgeEnv = new Envelope();
                    for (IEnumerator it = dirEdgeList.GetEnumerator(); 
                        it.MoveNext(); )
                    {
                        DirectedEdge dirEdge = (DirectedEdge) it.Current;
                        ICoordinateList pts = dirEdge.Edge.Coordinates;

                        if (pts != null)
                        {
                            int nCount = pts.Count;

                            for (int i = 0; i < nCount - 1; i++)
                            {
                                edgeEnv.ExpandToInclude(pts[i]);
                            }
                        }
                    }

                    env = edgeEnv;
                }

                return env;
            }
        }
        
        #endregion
		
        #region Public Methods

		/// <summary> 
		/// Creates the subgraph consisting of all edges reachable from this node.
		/// Finds the edges in the graph and the rightmost coordinate.
		/// </summary>
		/// <param name="node">a node to start the graph traversal from
		/// </param>
		public void Create(Node node)
		{
			AddReachable(node);
			finder.FindEdge(dirEdgeList);
			rightMostCoord = finder.Coordinate;
		}
		
		public void ComputeDepth(int outsideDepth)
		{
			ClearVisitedEdges();
			// find an outside edge to assign depth to
			DirectedEdge de = finder.Edge;
//			Node n = de.Node;
//			Label label = de.Label;
			// right side of line returned by finder is on the outside
			de.SetEdgeDepths(Position.Right, outsideDepth);
			CopySymDepths(de);
			
			//ComputeNodeDepth(n, de);
			ComputeDepths(de);
		}
		
		/// <summary> 
		/// Find all edges whose depths indicates that they are in the result area(s).
		/// Since we want polygon shells to be
		/// oriented CW, choose DirectedEdges with the interior of the result on the right
		/// hand side.
		/// Mark them as being in the result.
		/// Interior area edges are the result of dimensional collapses.
		/// They do not form part of the result area boundary.
		/// </summary>
		public void FindResultEdges()
		{
			for (IEnumerator it = dirEdgeList.GetEnumerator(); it.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) it.Current;
				/// <summary> Select edges which have an interior depth on the RHS
				/// and an exterior depth on the LHS.
				/// Note that because of weird rounding effects there may be
				/// edges which have negative depths!  Negative depths
				/// count as "outside".
				/// </summary>
				// <FIX> - handle negative depths
				if (de.GetDepth(Position.Right) >= 1 && 
                    de.GetDepth(Position.Left) <= 0  && 
                    !de.InteriorAreaEdge)
				{
					de.InResult = true;
				}
			}
		}
		
		/// <summary> 
		/// <see cref="BufferSubgraphs"/> are compared on the x-value of their 
		/// rightmost Coordinate. This defines a partial ordering on the graphs such that:
		/// <para>
		/// g1 >= g2 &lt;==&gt; Ring(g2) does not contain Ring(g1)
		/// </para>
		/// <para>
		/// where Polygon(g) is the buffer polygon that is built from g.
		/// </para>
		/// This relationship is used to sort the BufferSubgraphs so that shells 
		/// are guaranteed to be built before holes.
		/// </summary>
		public int CompareTo(object o)
		{
			BufferSubgraph graph = (BufferSubgraph) o;
			if (this.rightMostCoord.X < graph.rightMostCoord.X)
			{
				return - 1;
			}
			if (this.rightMostCoord.X > graph.rightMostCoord.X)
			{
				return 1;
			}
			return 0;
		}
        
        #endregion
		
        #region Private Methods

		/// <summary> 
		/// Adds all nodes and edges reachable from this node to the subgraph.
		/// Uses an explicit stack to avoid a large depth of recursion.
		/// </summary>
		/// <param name="node">A node known to be in the subgraph.</param>
		private void AddReachable(Node startNode)
		{
			Stack nodeStack = new Stack();
			nodeStack.Push(startNode);
			while (!(nodeStack.Count == 0))
			{
				Node node = (Node) nodeStack.Pop();
				Add(node, nodeStack);
			}
		}
		
        /// <summary> 
        /// Adds the argument node and all its out edges to the subgraph</summary>
        /// <param name="node">the node to add.
        /// </param>
        /// <param name="nodeStack">
        /// The current set of nodes being traversed.
        /// </param>
        private void Add(Node node, Stack nodeStack)
		{
			node.Visited = true;
			nodes.Add(node);

            for (IEnumerator i = ((DirectedEdgeStar) node.Edges).Iterator(); i.MoveNext();)
			{
				DirectedEdge de = (DirectedEdge) i.Current;
				dirEdgeList.Add(de);
				DirectedEdge sym = de.Sym;
				Node symNode = sym.Node;
				
                // NOTE: this is a depth-first traversal of the graph.
				// This will cause a large depth of recursion.
				// It might be better to do a breadth-first traversal.
				if (!symNode.Visited)
                {
//                    CollectionsHelper.StackPush(nodeStack, symNode);
                    nodeStack.Push(symNode);
                }
			}
		}
		
		private void ClearVisitedEdges()
		{
			for (IEnumerator it = dirEdgeList.GetEnumerator(); it.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) it.Current;
				de.Visited = false;
			}
		}
		
		/// <summary> 
		/// Compute depths for all dirEdges via breadth-first traversal of nodes in graph
		/// </summary>
		/// <param name="startEdge">edge to start processing with
		/// </param>
		// <FIX> MD - use iteration & queue rather than recursion, for speed and robustness
		private void ComputeDepths(DirectedEdge startEdge)
		{
			ISet nodesVisited   = new HashedSet();
			ArrayList nodeQueue = new ArrayList();
			
			Node startNode = startEdge.Node;
			nodeQueue.Add(startNode);
			nodesVisited.Add(startNode);
			startEdge.Visited = true;
			
			while (nodeQueue.Count > 0)
			{
				Node n = (Node) nodeQueue[0];
				nodeQueue.RemoveAt(0);

				nodesVisited.Add(n);
				// compute depths around node, starting at this edge since it has depths assigned
				ComputeNodeDepth(n);
				
				// Add all adjacent nodes to process queue,
				// unless the node has been visited already
				for (IEnumerator i = ((DirectedEdgeStar) n.Edges).Iterator(); i.MoveNext(); )
				{
					DirectedEdge de = (DirectedEdge) i.Current;
					DirectedEdge sym = de.Sym;
					if (sym.Visited)
						continue;
					Node adjNode = sym.Node;
					if (!(nodesVisited.Contains(adjNode)))
					{
						nodeQueue.Add(adjNode);
						nodesVisited.Add(adjNode);
					}
				}
			}
		}
		
		private void ComputeNodeDepth(Node n)
		{
			// find a visited dirEdge to start at
			DirectedEdge startEdge = null;
			for (IEnumerator i = ((DirectedEdgeStar) n.Edges).Iterator(); i.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) i.Current;
				if (de.Visited || de.Sym.Visited)
				{
					startEdge = de;
					break;
				}
			}

			// MD - testing  Result: breaks algorithm
			//if (startEdge == null) return;
			Debug.Assert(startEdge != null, "unable to find edge to compute depths at " + n.Coordinate);
			
			((DirectedEdgeStar) n.Edges).ComputeDepths(startEdge);
			
			// copy depths to sym edges
			for (IEnumerator i = ((DirectedEdgeStar) n.Edges).Iterator(); i.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) i.Current;
				de.Visited = true;
				CopySymDepths(de);
			}
		}
		
		private void CopySymDepths(DirectedEdge de)
		{
			DirectedEdge sym = de.Sym;
			sym.SetDepth(Position.Left, de.GetDepth(Position.Right));
			sym.SetDepth(Position.Right, de.GetDepth(Position.Left));
		}
        
        #endregion
	}
}