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
using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.PlanarGraphs
{
	/// <summary> 
	/// Finds all connected {@link Subgraph}s of a <see cref="PlanarGraph"/>.
	/// <p>
	/// <b>Note:</b> uses the <code>isVisited</code> flag on the nodes.
	/// </summary>
	internal sealed class ConnectedSubgraphFinder
	{
        #region Private Fields

        private PlanarGraph graph;
        
        #endregion
		
        #region Constructors and Destructor

        public ConnectedSubgraphFinder(PlanarGraph graph)
        {
            this.graph = graph;
        }

        #endregion
		
        #region Public Properties

		public IList ConnectedSubgraphs
		{
			get
			{
				IList subgraphs = new ArrayList();
				
				PlanarGraphObject.SetVisited(graph.NodeIterator(), false);

                for (IEnumerator i = graph.EdgeIterator(); i.MoveNext(); )
				{
					Edge e    = (Edge)i.Current;
					Node node = e.GetDirEdge(0).FromNode;
					
                    if (!node.Visited)
					{
						subgraphs.Add(FindSubgraph(node));
					}
				}

				return subgraphs;
			}
		}
        
        #endregion
		
        #region Private Methods

		private Subgraph FindSubgraph(Node node)
		{
			Subgraph subgraph = new Subgraph(graph);
			AddReachable(node, subgraph);
			
            return subgraph;
		}
		
		/// <summary> Adds all nodes and edges reachable from this node to the subgraph.
		/// Uses an explicit stack to avoid a large depth of recursion.
		/// 
		/// </summary>
		/// <param name="node">a node known to be in the subgraph
		/// </param>
		private void AddReachable(Node startNode, Subgraph subgraph)
		{
			Stack nodeStack = new Stack();
			nodeStack.Push(startNode);
			while (!(nodeStack.Count == 0))
			{
				Node node = (Node)nodeStack.Pop();
				AddEdges(node, nodeStack, subgraph);
			}
		}
		
		/// <summary> Adds the argument node and all its out edges to the subgraph.</summary>
		/// <param name="node">the node to add
		/// </param>
		/// <param name="nodeStack">the current set of nodes being traversed
		/// </param>
		private void AddEdges(Node node, Stack nodeStack, Subgraph subgraph)
		{
			node.Visited = true;

            for (IEnumerator i = ((DirectedEdgeStar)node.OutEdges).Iterator(); 
                i.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) i.Current;
				
                subgraph.Add(de.Edge);
				Node toNode = de.ToNode;

				if (!toNode.Visited)
					nodeStack.Push(toNode);
			}
		}
        
        #endregion
	}
}