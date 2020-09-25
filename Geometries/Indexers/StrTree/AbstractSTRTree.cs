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

namespace iGeospatial.Geometries.Indexers.StrTree
{
	
	/// <summary> 
	/// Base class for STRTree and SIRTree. 
	/// </summary>
	/// <remarks>
	/// STR-packed R-trees are described in:
	/// <para>
	/// P. Rigaux, Michel Scholl and Agnes Voisard. Spatial Databases With
	/// Application To GIS. Morgan Kaufmann, San Francisco, 2002.
	/// </para>
	/// <para>
	/// This implementation is based on Boundables rather than just AbstractNodes, 
	/// because the STR algorithm operates on both nodes and 
	/// data, both of which are treated here as Boundables.
	/// </para>
	/// <seealso cref="STRTree">
	/// </seealso>
	/// <seealso cref="SIRTree">
	/// </seealso>
	/// </remarks>
	internal abstract class AbstractSTRTree
	{
        internal AbstractNode m_objRoot;
		
        private bool built;

        private ArrayList itemBoundables;
        private int nodeCapacity;
		
		/// <summary> 
		/// Constructs an AbstractSTRTree with the specified maximum number of child
		/// nodes that a node may have
		/// </summary>
		protected AbstractSTRTree(int nodeCapacity)
		{
            itemBoundables = new ArrayList();

			Debug.Assert(nodeCapacity > 1, "Node capacity must be greater than 1");
			this.nodeCapacity = nodeCapacity;
		}
		
		protected AbstractNode Root
		{
			get
			{
				return m_objRoot;
			}
		}

		/// <summary> 
		/// Returns the maximum number of child nodes that a node may have
		/// </summary>
		public int NodeCapacity
		{
			get
			{
				return nodeCapacity;
			}
		}

		protected abstract IComparer Comparator
        {
            get;
        }
		
		/// <summary> 
		/// A test for intersection between two bounds, necessary because subclasses
		/// of AbstractSTRTree have different implementations of bounds. 
		/// </summary>
		protected interface IIntersectsOp
		{
			/// <summary> For STRtrees, the bounds will be Envelopes; for SIRtrees, Intervals;
			/// for other subclasses of AbstractSTRTree, some other class.
			/// </summary>
			/// <param name="aBounds">the bounds of one spatial object
			/// </param>
			/// <param name="bBounds">the bounds of another spatial object
			/// </param>
			/// <returns> whether the two bounds intersect
			/// </returns>
			bool Intersects(object aBounds, object bBounds);
		}
		
		/// <summary> Creates parent nodes, grandparent nodes, and so forth up to the root
		/// node, for the data that has been inserted into the tree. Can only be
		/// called once, and thus can be called only after all of the data has been
		/// inserted into the tree.
		/// </summary>
		public void Build()
		{
			Debug.Assert(!built);
			m_objRoot = (itemBoundables.Count == 0) ? CreateNode(0) : CreateHigherLevels(itemBoundables, - 1);
			built = true;
		}
		
		protected abstract AbstractNode CreateNode(int level);
		
		/// <summary> 
		/// Sorts the childBoundables then divides them into groups of size M, where
		/// M is the node capacity.
		/// </summary>
		protected virtual ArrayList CreateParentBoundables(ArrayList childBoundables, 
            int newLevel)
		{
			Debug.Assert(!(childBoundables.Count == 0));

            ArrayList parentBoundables = new ArrayList();

            parentBoundables.Add(CreateNode(newLevel));

            ArrayList sortedChildBoundables = new ArrayList(childBoundables);

            sortedChildBoundables.Sort(Comparator);

            for (IEnumerator i = sortedChildBoundables.GetEnumerator(); i.MoveNext(); )
			{
				IBoundable childBoundable = (IBoundable) i.Current;
				if (LastNode(parentBoundables).ChildBoundables.Count == NodeCapacity)
				{
					parentBoundables.Add(CreateNode(newLevel));
				}

				LastNode(parentBoundables).AddChildBoundable(childBoundable);
			}

			return parentBoundables;
		}
		
		protected AbstractNode LastNode(ArrayList nodes)
		{
			return (AbstractNode) nodes[nodes.Count - 1];
		}
		
		protected int CompareDoubles(double a, double b)
		{
			return a > b ? 1 : a < b ? -1 : 0;
		}
		
		/// <summary> 
		/// Creates the levels higher than the given level
		/// </summary>
		/// <param name="">boundablesOfALevel
		/// the level to build on
		/// </param>
		/// <param name="">level
		/// the level of the Boundables, or -1 if the boundables are item
		/// boundables (that is, below level 0)
		/// </param>
		/// <returns> the root, which may be a ParentNode or a LeafNode
		/// </returns>
		private AbstractNode CreateHigherLevels(ArrayList boundablesOfALevel, int level)
		{
			Debug.Assert(!(boundablesOfALevel.Count == 0));
			ArrayList parentBoundables = 
                CreateParentBoundables(boundablesOfALevel, level + 1);
			
            if (parentBoundables.Count == 1)
			{
				return (AbstractNode) parentBoundables[0];
			}
			
            return CreateHigherLevels(parentBoundables, level + 1);
		}
		
        public virtual int Size()
        {
            if (!built)
            {
                Build();
            }
            if ((itemBoundables.Count == 0))
            {
                return 0;
            }
            return Size(m_objRoot);
        }
		
        protected virtual int Size(AbstractNode node)
        {
            int size = 0;

            for (IEnumerator i = node.ChildBoundables.GetEnumerator(); i.MoveNext(); )
            {
                IBoundable childBoundable = (IBoundable) i.Current;
                AbstractNode abstractNode = childBoundable as AbstractNode;
                if (abstractNode != null)
                {
                    size += Size(abstractNode);
                }
                else if (childBoundable is ItemBoundable)
                {
                    size += 1;
                }
            }
            return size;
        }
		
        public virtual int Depth()
        {
            if (!built)
            {
                Build();
            }
            if ((itemBoundables.Count == 0))
            {
                return 0;
            }
            return Depth(m_objRoot);
        }
		
        protected int Depth(AbstractNode node)
        {
            int maxChildDepth = 0;
            for (IEnumerator i = node.ChildBoundables.GetEnumerator(); i.MoveNext(); )
            {
                AbstractNode childBoundable = i.Current as AbstractNode;
                if (childBoundable != null)
                {
                    int childDepth = Depth(childBoundable);
                    if (childDepth > maxChildDepth)
                        maxChildDepth = childDepth;
                }
            }

            return maxChildDepth + 1;
        }
		
        protected void Insert(object bounds, object item)
		{
			Debug.Assert(!built, "Cannot insert items into an STR packed R-tree after it has been built.");
			
            itemBoundables.Add(new ItemBoundable(bounds, item));
		}
		
		/// <summary>  Also builds the tree, if necessary.</summary>
		protected ArrayList Query(object searchBounds)
		{
			if (!built)
			{
				Build();
			}

			ArrayList matches = new ArrayList();
			
            if ((itemBoundables.Count == 0))
			{
				Debug.Assert(m_objRoot.Bounds == null);
				return matches;
			}
			
            if (GetIntersectsOp().Intersects(m_objRoot.Bounds, searchBounds))
			{
				Query(searchBounds, m_objRoot, matches);
			}

			return matches;
		}
		
        /// <summary>  Also builds the tree, if necessary.</summary>
        protected void Query(object searchBounds, ISpatialIndexVisitor visitor)
        {
            if (!built)
            {
                Build();
            }
            if ((itemBoundables.Count == 0))
            {
                Debug.Assert(m_objRoot.Bounds == null);
            }
            if (GetIntersectsOp().Intersects(m_objRoot.Bounds, searchBounds))
            {
                Query(searchBounds, m_objRoot, visitor);
            }
        }
		
		/// <returns> 
		/// A test for intersection between two bounds, necessary because subclasses
		/// of AbstractSTRTree have different implementations of bounds. 
		/// </returns>
		/// <seealso cref="IIntersectsOp">
		/// </seealso>
		protected abstract IIntersectsOp GetIntersectsOp();
		
		private void Query(object searchBounds, 
            AbstractNode node, ArrayList matches)
		{
			for (IEnumerator i = node.ChildBoundables.GetEnumerator(); i.MoveNext(); )
			{
				IBoundable childBoundable = (IBoundable) i.Current;
				if (!GetIntersectsOp().Intersects(childBoundable.Bounds, searchBounds))
				{
					continue;
				}
                AbstractNode abstractNode = childBoundable as AbstractNode;
				if (abstractNode != null)
				{
					Query(searchBounds, abstractNode, matches);
				}
				else 
                {   ItemBoundable objItem = childBoundable as ItemBoundable;
                    if (objItem != null)
                    {
                        matches.Add(objItem.Item);
                    }
                    else
                    {
                        Debug.Assert(false, "Should never reach here");
                    }
                }
			}
		}
		
        private void Query(object searchBounds, AbstractNode node, 
            ISpatialIndexVisitor visitor)
        {
            for (IEnumerator i = node.ChildBoundables.GetEnumerator(); 
                i.MoveNext(); )
            {
                IBoundable childBoundable = (IBoundable) i.Current;
                if (!GetIntersectsOp().Intersects(childBoundable.Bounds, searchBounds))
                {
                    continue;
                }
                if (childBoundable is AbstractNode)
                {
                    Query(searchBounds, (AbstractNode) childBoundable, visitor);
                }
                else if (childBoundable is ItemBoundable)
                {
                    visitor.VisitItem(((ItemBoundable) childBoundable).Item);
                }
                else
                {
                    Debug.Assert(true);
                }
            }
        }
		
        /// <summary>  Also builds the tree, if necessary.</summary>
        protected bool Remove(object searchBounds, object item)
        {
            if (!built)
            {
                Build();
            }

            if ((itemBoundables.Count == 0))
            {
                Debug.Assert(m_objRoot.Bounds == null);
            }

            if (GetIntersectsOp().Intersects(m_objRoot.Bounds, searchBounds))
            {
                return Remove(searchBounds, m_objRoot, item);
            }

            return false;
        }
		
        private bool RemoveItem(AbstractNode node, object item)
        {
            IBoundable childToRemove = null;

            for (IEnumerator i = node.ChildBoundables.GetEnumerator(); i.MoveNext(); )
            {
                ItemBoundable childBoundable = i.Current as ItemBoundable;
                if (childBoundable != null)
                {
                    if (childBoundable.Item == item)
                        childToRemove = childBoundable;
                }
            }
            if (childToRemove != null)
            {
                node.ChildBoundables.Remove(childToRemove);
                return true;
            }
            return false;
        }
		
        private bool Remove(object searchBounds, AbstractNode node, object item)
        {
            // first try removing item from this node
            bool found = RemoveItem(node, item);
            if (found)
                return true;
			
            AbstractNode childToPrune = null;
            // next try removing item from lower nodes
            for (IEnumerator i = node.ChildBoundables.GetEnumerator(); i.MoveNext(); )
            {
                IBoundable childBoundable = (IBoundable) i.Current;
                if (!GetIntersectsOp().Intersects(childBoundable.Bounds, searchBounds))
                {
                    continue;
                }
                AbstractNode childTemp = childBoundable as AbstractNode;
                if (childTemp != null)
                {
                    found = Remove(searchBounds, childTemp, item);
                    // if found, record child for pruning and exit
                    if (found)
                    {
                        childToPrune = childTemp;
                        break;
                    }
                }
            }
            // prune child if possible
            if (childToPrune != null)
            {
                if ((childToPrune.ChildBoundables.Count == 0))
                {
                    node.ChildBoundables.Remove(childToPrune);
                }
            }
            return found;
        }
		
		protected ArrayList BoundablesAtLevel(int level)
		{
			ArrayList boundables = new ArrayList();
			BoundablesAtLevel(level, m_objRoot, boundables);
			return boundables;
		}
		
		/// <param name="level">-1 to get items</param>
		private void BoundablesAtLevel(int level, AbstractNode top, ArrayList boundables)
		{
			Debug.Assert(level > - 2);
			if (top.Level == level)
			{
				boundables.Add(top);
				return ;
			}

            for (IEnumerator i = top.ChildBoundables.GetEnumerator(); i.MoveNext(); )
			{
				IBoundable boundable = (IBoundable) i.Current;
                AbstractNode abstractNode = boundable as AbstractNode;
				if (abstractNode != null)
				{
					BoundablesAtLevel(level, abstractNode, boundables);
				}
				else
				{
					Debug.Assert(boundable is ItemBoundable);
					if (level == - 1)
					{
						boundables.Add(boundable);
					}
				}
			}
			return ;
		}
	}
}