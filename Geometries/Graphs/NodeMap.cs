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
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Graphs
{
	/// <summary> 
	/// A map of nodes, indexed by the coordinate of the node.
	/// </summary>
	[Serializable]
    internal class NodeMap
	{
        #region Private Fields

		private IDictionary nodeMap;
		private NodeFactory nodeFact;
        
        #endregion
		
        #region Constructors and Destructor

		public NodeMap(NodeFactory nodeFact)
		{
            this.nodeMap  = new SortedList();
			this.nodeFact = nodeFact;
		}

        #endregion
		
        #region Public Methods

		/// <summary> This method expects that a node has a coordinate value.</summary>
		public Node AddNode(Coordinate coord)
		{
			Node node = (Node) nodeMap[coord];
			if (node == null)
			{
				node = nodeFact.CreateNode(coord);
				nodeMap[coord] = node;
			}
			return node;
		}
		
		public Node AddNode(Node n)
		{
			Node node = (Node) nodeMap[n.Coordinate];
			if (node == null)
			{
				nodeMap[n.Coordinate] = n;

				return n;
			}
			node.MergeLabel(n);

			return node;
		}
		
		/// <summary> Adds a node for the start point of this EdgeEnd
		/// (if one does not already exist in this map).
		/// Adds the EdgeEnd to the (possibly new) node.
		/// </summary>
		public void Add(EdgeEnd e)
		{
			Coordinate p = e.Coordinate;
			Node n = AddNode(p);
			n.Add(e);
		}

		/// <returns> the node if found; null otherwise
		/// </returns>
		public Node Find(Coordinate coord)
		{
			return (Node) nodeMap[coord];
		}
		
		public IEnumerator Iterator()
		{
			return nodeMap.Values.GetEnumerator();
		}

		public ICollection Values()
		{
			return nodeMap.Values;
		}
		
		public NodeCollection GetBoundaryNodes(int geomIndex)
		{
			NodeCollection bdyNodes = new NodeCollection();

            for (IEnumerator i = Iterator(); i.MoveNext(); )
			{
				Node node = (Node) i.Current;
				if (node.Label.GetLocation(geomIndex) == LocationType.Boundary)
					bdyNodes.Add(node);
			}

			return bdyNodes;
		}
        
        #endregion
	}
}