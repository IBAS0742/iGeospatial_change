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
	/// A map of <see cref="Node"/>s, indexed by the coordinate of the node.
	/// </summary>
	internal class NodeMap
	{
		private IDictionary nodeMap;
		
		/// <summary> Constructs a NodeMap without any Nodes.</summary>
		public NodeMap()
		{
            nodeMap = new SortedList();
        }
		
		/// <summary> 
		/// Adds a node to the map, replacing any that is already at that 
		/// location.
		/// </summary>
		/// <returns> the added node
		/// </returns>
		public Node Add(Node n)
		{
//			object tempObject;
//			tempObject = n;
//			nodeMap[n.Coordinate] = tempObject;
//			object generatedAux = tempObject;
			nodeMap[n.Coordinate] = n;
			return n;
		}
		
		/// <summary> Removes the Node at the given location, and returns it (or null if no Node was there).</summary>
		public Node Remove(Coordinate pt)
		{
			Node node = (Node)nodeMap[pt];  //TODO--PAUL
			nodeMap.Remove(pt);

			return node;
		}
		
		/// <summary> Returns the Node at the given location, or null if no Node was there.</summary>
		public Node Find(Coordinate coord)
		{
			return (Node) nodeMap[coord];
		}
		
		/// <summary> Returns an Iterator over the Nodes in this NodeMap, sorted in ascending order
		/// by angle with the positive x-axis.
		/// </summary>
		public IEnumerator Iterator()
		{
			return nodeMap.Values.GetEnumerator();
		}

		/// <summary> Returns the Nodes in this NodeMap, sorted in ascending order
		/// by angle with the positive x-axis.
		/// </summary>
		public ICollection Values()
		{
			return nodeMap.Values;
		}
	}
}