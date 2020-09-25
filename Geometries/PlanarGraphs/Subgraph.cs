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
using iGeospatial.Collections.Sets;

namespace iGeospatial.Geometries.PlanarGraphs
{
	/// <summary> 
	/// A subgraph of a <see cref="PlanarGraph"/>.
	/// A subgraph may contain any subset of {@link Edges}
	/// from the parent graph.
	/// It will also automatically contain all {@link DirectedEdge}s
	/// and {@link Node}s associated with those edges.
	/// No new objects are created when edges are added -
	/// all associated components must already exist in the parent graph.
	/// </summary>
	internal class Subgraph
	{
		private PlanarGraph parentGraph;
		//TODO--PAUL 'java.util.HashSet' was converted to HashedSet
		private HashedSet edges;
		private IList     dirEdges;
		private NodeMap   nodeMap;
		
		/// <summary> 
		/// Creates a new subgraph of the given <see cref="PlanarGraph"/>
		/// </summary>
		/// <param name="parentGraph">the parent graph
		/// </param>
		public Subgraph(PlanarGraph parentGraph)
		{
			this.parentGraph = parentGraph;

            edges    = new HashedSet();
            dirEdges = new ArrayList();
            nodeMap  = new NodeMap();
		}

		/// <summary> Gets the <see cref="PlanarGraph"/> which this subgraph
		/// is part of.
		/// 
		/// </summary>
		/// <returns> the parent PlanarGraph
		/// </returns>
		public PlanarGraph Parent
		{
			get
			{
				return parentGraph;
			}
		}
			
		/// <summary> Adds an {@link Edge} to the subgraph.
		/// The associated {@link DirectedEdge}s and {@link Node}s
		/// are also added.
		/// 
		/// </summary>
		/// <param name="e">the edge to add
		/// </param>
		public void Add(Edge e)
		{
			if (edges.Contains(e))
				return ;
			
			edges.Add(e);
			dirEdges.Add(e.GetDirEdge(0));
			dirEdges.Add(e.GetDirEdge(1));
			nodeMap.Add(e.GetDirEdge(0).FromNode);
			nodeMap.Add(e.GetDirEdge(1).FromNode);
		}
		
		/// <summary> Returns an {@link Iterator} over the {@link DirectedEdge}s in this graph,
		/// in the order in which they were added.
		/// 
		/// </summary>
		/// <returns> an iterator over the directed edges
		/// 
		/// </returns>
		/// <seealso cref="Add(Edge)">
		/// </seealso>
		public IEnumerator DirEdgeIterator()
		{
			return dirEdges.GetEnumerator();
		}
		
		/// <summary> Returns an {@link Iterator} over the {@link Edge}s in this graph,
		/// in the order in which they were added.
		/// 
		/// </summary>
		/// <returns> an iterator over the edges
		/// 
		/// </returns>
		/// <seealso cref="Add(Edge)">
		/// </seealso>
		public IEnumerator EdgeIterator()
		{
			return edges.GetEnumerator();
		}
		
		/// <summary> 
		/// Returns an {@link Iterator} over the {@link Nodes} in this graph.</summary>
		/// <returns> an iterator over the nodes
		/// </returns>
		public IEnumerator NodeIterator()
		{
			return nodeMap.Iterator();
		}
		
		/// <summary> 
		/// Tests whether an {@link Edge} is contained in this subgraph.
		/// </summary>
		/// <param name="e">the edge to test
		/// </param>
		/// <returns> <see langword="true"/> if the edge is contained in this subgraph
		/// </returns>
		public bool Contains(Edge e)
		{
			return edges.Contains(e);
		}
	}
}