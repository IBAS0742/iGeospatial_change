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

namespace iGeospatial.Geometries.Indexers.StrTree
{
	
	/// <summary> 
	/// A node of the STR tree. The children of this node are either more nodes
	/// (AbstractNodes) or real data (ItemBoundables). If this node Contains real data
	/// (rather than nodes), then we say that this node is a "leaf node".  
	/// </summary>
	internal abstract class AbstractNode : IBoundable
	{
        private ArrayList childBoundables;
        private object    bounds;
        private int       level;
		
		/// <summary> 
		/// Constructs an AbstractNode at the given level in the tree
		/// </summary>
		/// <param name="level">
		/// 0 if this node is a leaf, 1 if a parent of a leaf, and so on; the
		/// root node will have the highest level
		/// </param>
		public AbstractNode(int level)
		{
            childBoundables = new ArrayList();

			this.level = level;
		}
		
		/// <summary> 
		/// Returns either child AbstractNodes, or if this is a leaf node, real data (wrapped
		/// in ItemBoundables).
		/// </summary>
		public ArrayList ChildBoundables
		{
			get
			{
				return childBoundables;
			}
		}

		public object Bounds
		{
			get
			{
				if (bounds == null)
				{
					bounds = ComputeBounds();
				}
				return bounds;
			}
		}

		/// <summary> 
		/// Returns 0 if this node is a leaf, 1 if a parent of a leaf, and so on; the
		/// root node will have the highest level
		/// </summary>
		public int Level
		{
			get
			{
				return level;
			}
		}

		/// <summary> Returns a representation of space that encloses this IBoundable,
		/// preferably not much bigger than this IBoundable's boundary yet fast to
		/// test for intersection with the bounds of other Boundables. The class of
		/// object returned depends on the subclass of AbstractSTRTree.
		/// </summary>
		/// <returns> an Envelope (for STRtrees), an Interval (for SIRtrees), or other
		/// object (for other subclasses of AbstractSTRTree)
		/// </returns>
		/// <seealso cref="AbstractSTRTree.IIntersectsOp">
		/// </seealso>
		protected abstract object ComputeBounds();
		
		/// <summary> 
		/// Adds either an AbstractNode, or if this is a leaf node, a data object
		/// (wrapped in an ItemBoundable)
		/// </summary>
		public void AddChildBoundable(IBoundable childBoundable)
		{
			Debug.Assert(bounds == null);

            childBoundables.Add(childBoundable);
		}
	}
}