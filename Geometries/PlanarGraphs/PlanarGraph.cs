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

namespace iGeospatial.Geometries.PlanarGraphs
{
	/// <summary> 
	/// Represents a directed graph which is embeddable in a planar surface.
	/// </summary>
	/// <remarks>
	/// This class and the other classes in this package serve as a framework for
	/// building planar graphs for specific algorithms. This class must be
	/// subclassed to expose appropriate methods to construct the graph. This allows
	/// controlling the types of graph components (<see cref="DirectedEdge"/>s,
	/// <see cref="Edge"/>s and <see cref="Node"/>s) which can be added to the graph. An
	/// application which uses the graph framework will almost always provide
	/// subclasses for one or more graph components, which hold application-specific
	/// data and graph algorithms.
	/// </remarks>
	internal abstract class PlanarGraph
	{
		internal ArrayList m_arrEdges;
		internal ArrayList dirEdges;
		internal NodeMap nodeMap;

		/// <summary> 
		/// Constructs a PlanarGraph without any Edges, DirectedEdges, or Nodes.
		/// </summary>
		public PlanarGraph()
		{
            m_arrEdges = new ArrayList();
            dirEdges   = new ArrayList();
            nodeMap    = new NodeMap();
        }

        /// <summary> 
        /// Returns the {@link Node} at the given location,
        /// or null if no {@link Node} was there.
        /// </summary>
        /// <param name="pt">the location to query
        /// </param>
        /// <returns> the node found
        /// </returns>
        /// <returns> 
        /// <c>null</c> if this graph contains no node at the location
        /// </returns>
        public Node FindNode(Coordinate pt)
		{
			return (Node) nodeMap.Find(pt);
		}
		
		public ICollection Nodes
		{
			get
			{
				return nodeMap.Values();
			}
		}

		/// <summary> 
		/// Returns the Edges that have been added to this PlanarGraph
		/// </summary>
		/// <seealso cref="Add(Edge)">
		/// </seealso>
		public ArrayList Edges
		{
			get
			{
				return m_arrEdges;
			}
		}
		
		/// <summary> 
		/// Adds a node to the map, replacing any that is already at that location.
		/// Only subclasses can Add Nodes, to ensure Nodes are of the right type.
		/// </summary>
		/// <returns> The added node.</returns>
		protected void Add(Node node)
		{
			nodeMap.Add(node);
		}
		
		/// <summary>
		/// Adds the Edge and its DirectedEdges with this PlanarGraph.
		/// Assumes that the Edge has already been created with its associated DirectEdges.
		/// Only subclasses can Add Edges, to ensure the edges added are of the right class.
		/// </summary>
		protected void  Add(Edge edge)
		{
			m_arrEdges.Add(edge);
			Add(edge.GetDirEdge(0));
			Add(edge.GetDirEdge(1));
		}
		
		/// <summary> 
		/// Adds the Edge to this PlanarGraph; only subclasses can Add DirectedEdges,
		/// to ensure the edges added are of the right class.
		/// </summary>
		protected void  Add(DirectedEdge dirEdge)
		{
			dirEdges.Add(dirEdge);
		}

		/// <summary> 
		/// Returns an Iterator over the Nodes in this PlanarGraph.
		/// </summary>
		public IEnumerator NodeIterator()
		{
			return nodeMap.Iterator();
		}

		/// <summary> Returns the Nodes in this PlanarGraph.</summary>
		/// <returns> 
		/// Returns an Iterator over the DirectedEdges in this PlanarGraph, in the 
		/// order in which they were added.
		/// </returns>
		/// <seealso cref="Add(Edge)">
		/// </seealso>
		/// <seealso cref="Add(DirectedEdge)">
		/// </seealso>
		public IEnumerator DirectedEdgeIterator()
		{
			return dirEdges.GetEnumerator();
		}

		/// <summary> 
		/// Returns an Iterator over the Edges in this PlanarGraph, in the order 
		/// in which they were added.
		/// </summary>
		/// <seealso cref="Add(Edge)">
		/// </seealso>
		public IEnumerator EdgeIterator()
		{
			return m_arrEdges.GetEnumerator();
		}
		
        /// <summary> Removes an {@link Edge} and its associated {@link DirectedEdge}s
        /// from their from-Nodes and from the graph.
        /// Note: This method does not remove the {@link Node}s associated
        /// with the {@link Edge}, even if the removal of the {@link Edge}
        /// reduces the degree of a {@link Node} to zero.
        /// </summary>
        public void Remove(Edge edge)
		{
			Remove(edge.GetDirEdge(0));
			Remove(edge.GetDirEdge(1));

            m_arrEdges.Remove(edge);
            edge.Remove();
		}
		
        /// <summary> Removes DirectedEdge from its from-Node and from this PlanarGraph.
        /// This method does not remove the Nodes associated with the DirectedEdge,
        /// even if the removal of the DirectedEdge reduces the degree of a Node to
        /// zero.
        /// </summary>
        public void Remove(DirectedEdge de)
		{
			DirectedEdge sym = de.Sym;
			if (sym != null)
				sym.Sym = null;
			de.FromNode.OutEdges.Remove(de);
            de.Remove();
			dirEdges.Remove(de);
		}

		/// <summary> 
		/// Removes a node from the graph, along with any associated DirectedEdges and Edges.
		/// </summary>
		public void Remove(Node node)
		{
			// unhook all directed edges
			ArrayList outEdges = node.OutEdges.Edges;

            for (IEnumerator i = outEdges.GetEnumerator(); i.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) i.Current;
				DirectedEdge sym = de.Sym;
				// remove the diredge that points to this node
				if (sym != null)
					Remove(sym);
				// remove this diredge from the graph collection
				dirEdges.Remove(de);
				
				Edge edge = de.Edge;
				if (edge != null)
				{
					m_arrEdges.Remove(edge);
				}
			}

			// remove the node from the graph
			nodeMap.Remove(node.Coordinate);
            node.Remove();
		}
		
        /// <summary> Tests whether this graph contains the given {@link Edge}
        /// 
        /// </summary>
        /// <param name="de">the edge to query
        /// </param>
        /// <returns> <see langword="true"/> if the graph contains the edge
        /// </returns>
        public bool Contains(Edge e)
        {
            return m_arrEdges.Contains(e);
        }
		
        /// <summary> Tests whether this graph contains the given {@link DirectedEdge}
        /// 
        /// </summary>
        /// <param name="de">the directed edge to query
        /// </param>
        /// <returns> <see langword="true"/> if the graph contains the directed edge
        /// </returns>
        public bool Contains(DirectedEdge de)
        {
            return dirEdges.Contains(de);
        }
		
		/// <summary> 
		/// Returns all Nodes with the given number of Edges around it.
		/// </summary>
		public ArrayList FindNodesOfDegree(int degree)
		{
			ArrayList nodesFound = new ArrayList();

            for (IEnumerator i = NodeIterator(); i.MoveNext(); )
			{
				Node node = (Node) i.Current;
				if (node.Degree == degree)
				{
					nodesFound.Add(node);
				}
			}

			return nodesFound;
		}
	}
}