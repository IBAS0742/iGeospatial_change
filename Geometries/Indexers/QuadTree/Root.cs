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

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Indexers.QuadTree
{
	
	/// <summary> 
	/// QuadRoot is the root of a single Quadtree.  It is centred at the origin,
	/// and does not have a defined extent.
	/// </summary>
	[Serializable]
    internal class Root : NodeBase
	{
        #region Private Members

		// the singleton root quad is centred at the origin.
		private static readonly Coordinate origin = new Coordinate(0.0, 0.0);

        #endregion
		
		public Root()
		{
		}
		
		/// <summary> 
		/// Insert an item into the quadtree this is the root of.
		/// </summary>
		public virtual void  Insert(Envelope itemEnv, object item)
		{
			int index = GetSubnodeIndex(itemEnv, origin);
			// if index is -1, itemEnv must cross the X or Y axis.
			if (index == - 1)
			{
				Add(item);
				return ;
			}

			// the item must be contained in one quadrant, so insert it into the
			// tree for that quadrant (which may not yet exist)
			Node node = subnode[index];

			// If the subquad doesn't exist or this item is not contained in it,
			// have to expand the tree upward to contain the item.			
			if (node == null || !node.Envelope.Contains(itemEnv))
			{
				Node largerNode = Node.CreateExpanded(node, itemEnv);
				subnode[index] = largerNode;
			}

			// At this point we have a subquad which exists and must contain
			// Contains the env for the item.  Insert the item into the tree.
			InsertContained(subnode[index], itemEnv, item);
		}
		
		/// <summary> insert an item which is known to be contained in the tree rooted at
		/// the given QuadNode root.  Lower levels of the tree will be created
		/// if necessary to hold the item.
		/// </summary>
		private void  InsertContained(Node tree, Envelope itemEnv, object item)
		{
			Debug.Assert(tree.Envelope.Contains(itemEnv));
			// Do NOT create a new quad for zero-area envelopes - this would lead
			// to infinite recursion. Instead, use a heuristic of simply returning
			// the smallest existing quad containing the query
			bool isZeroX = IntervalSize.IsZeroWidth(itemEnv.MinX, itemEnv.MaxX);
			bool isZeroY = IntervalSize.IsZeroWidth(itemEnv.MinX, itemEnv.MaxX);

			NodeBase node;
			if (isZeroX || isZeroY)
				node = tree.Find(itemEnv);
			else
				node = tree.GetNode(itemEnv);
			node.Add(item);
		}
		
		protected override bool IsSearchMatch(Envelope searchEnv)
		{
			return true;
		}
	}
}