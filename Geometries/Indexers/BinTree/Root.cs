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

using iGeospatial.Geometries.Indexers.QuadTree;

namespace iGeospatial.Geometries.Indexers.BinTree
{
	/// <summary> 
	/// The root node of a single <see cref="Bintree"/>. It is centred at the origin,
	/// and does not have a defined extent. 
	/// </summary>
	[Serializable]
    internal class Root : NodeBase
	{
		// the singleton root node is centred at the origin.
		private const double origin = 0.0;
		
		public Root()
		{
		}
		
		/// <summary> Insert an item into the tree this is the root of.</summary>
		public virtual void  Insert(Interval itemInterval, object item)
		{
			int index = GetSubnodeIndex(itemInterval, origin);
			// if index is -1, itemEnv must contain the origin.
			if (index == - 1)
			{
				Add(item);
				return ;
			}

			// The item must be contained in one interval, so insert it into the
			// tree for that interval (which may not yet exist)
            Node node = subnode[index];

			// If the subnode doesn't exist or this item is not contained in it,
			// have to expand the tree upward to contain the item.
			if (node == null || !node.Interval.Contains(itemInterval))
			{
				Node largerNode = Node.CreateExpanded(node, itemInterval);
				subnode[index] = largerNode;
			}

			// At this point we have a subnode which exists and must contain
			// Contains the env for the item.  Insert the item into the tree.
			InsertContained(subnode[index], itemInterval, item);
		}
		
		/// <summary> 
		/// insert an item which is known to be contained in the tree rooted at the 
		/// given Node.  Lower levels of the tree will be created if necessary to hold 
		/// the item.
		/// </summary>
		private void  InsertContained(Node tree, Interval itemInterval, object item)
		{
			Debug.Assert(tree.Interval.Contains(itemInterval));
			/// <summary> Do NOT create a new node for zero-area intervals - this would lead
			/// to infinite recursion. Instead, use a heuristic of simply returning
			/// the smallest existing node containing the query
			/// </summary>
			bool isZeroArea = IntervalSize.IsZeroWidth(itemInterval.Min, itemInterval.Max);
			NodeBase node;
			if (isZeroArea)
				node = tree.Find(itemInterval);
			else
				node = tree.GetNode(itemInterval);
			node.Add(item);
		}
		
        /// <summary>
        /// The root node matches all searches.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        protected override bool IsSearchMatch(Interval interval)
		{
			return true;
		}
	}
}