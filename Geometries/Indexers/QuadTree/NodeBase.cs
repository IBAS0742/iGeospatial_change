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

namespace iGeospatial.Geometries.Indexers.QuadTree
{
	/// <summary> 
	/// The base class for nodes in a <see cref="Quadtree"/>.
	/// </summary>
	[Serializable]
    internal abstract class NodeBase
	{
		internal ArrayList m_arrItems;
		
		/// <summary> subquads are numbered as follows:
		/// <pre>
		/// 2 | 3
		/// --+--
		/// 0 | 1
		/// </pre>
		/// </summary>
		internal Node[] subnode;
		
        protected NodeBase()
        {
            subnode    = new Node[4];
            m_arrItems = new ArrayList();
        }
		
		public ArrayList Items
		{
			get
			{
				return m_arrItems;
			}
		}
		
        public bool HasItems
        {
            get 
            {
                return !(m_arrItems.Count == 0);
            }
        }
		
        public bool HasChildren
        {
            get 
            {
                for (int i = 0; i < 4; i++)
                {
                    if (subnode[i] != null)
                        return true;
                }
                return false;
            }
        }

        public bool Prunable
        {
            get
            {
                return !(this.HasChildren || this.HasItems);
            }
        }
		
		/// <summary> Returns the index of the subquad that wholly Contains the given envelope.
		/// If none does, returns -1.
		/// </summary>
		public static int GetSubnodeIndex(Envelope env, Coordinate centre)
		{
			int subnodeIndex = - 1;
			if (env.MinX >= centre.X)
			{
				if (env.MinY >= centre.Y)
					subnodeIndex = 3;
				if (env.MaxY <= centre.Y)
					subnodeIndex = 1;
			}

			if (env.MaxX <= centre.X)
			{
				if (env.MinY >= centre.Y)
					subnodeIndex = 2;
				if (env.MaxY <= centre.Y)
					subnodeIndex = 0;
			}

			return subnodeIndex;
		}
		
		public void Add(object item)
		{
			m_arrItems.Add(item);
		}
		
        /// <summary> Removes a single item from this subtree.
        /// 
        /// </summary>
        /// <param name="searchEnv">the envelope containing the item
        /// </param>
        /// <param name="item">the item to remove
        /// </param>
        /// <returns> <see langword="true"/> if the item was found and removed
        /// </returns>
        public bool Remove(Envelope itemEnv, object item)
        {
            // use envelope to restrict nodes scanned
            if (!IsSearchMatch(itemEnv))
                return false;
			
            bool found = false;
            for (int i = 0; i < 4; i++)
            {
                if (subnode[i] != null)
                {
                    found = subnode[i].Remove(itemEnv, item);
                    if (found)
                    {
                        // trim subtree if empty
                        if (subnode[i].Prunable)
                            subnode[i] = null;
                        break;
                    }
                }
            }

            // if item was found lower down, don't need to search for it here
            if (found)
                return found;

            // otherwise, try and remove the item from the list of items in this node
            bool tempBoolean;
            tempBoolean = m_arrItems.Contains(item);
            m_arrItems.Remove(item);
            found = tempBoolean;

            return found;
        }

		//<<TODO:RENAME?>> Sounds like this method adds resultItems to items
		//(like List#AddAll). Perhaps it should be renamed to "addAllItemsTo" [Jon Aquino]
		public ArrayList AddAllItems(ArrayList resultItems)
		{
            // this node may have items as well as subnodes (since items may not
            // be wholely contained in any single subnode
            resultItems.AddRange(this.m_arrItems);
			for (int i = 0; i < 4; i++)
			{
				if (subnode[i] != null)
				{
					subnode[i].AddAllItems(resultItems);
				}
			}
			return resultItems;
		}

		protected abstract bool IsSearchMatch(Envelope searchEnv);
		
		public void AddAllItemsFromOverlapping(Envelope searchEnv, 
            ArrayList resultItems)
		{
			if (!IsSearchMatch(searchEnv))
				return ;
			
            // this node may have items as well as subnodes (since items may not
            // be wholely contained in any single subnode
            resultItems.AddRange(m_arrItems);
			
			for (int i = 0; i < 4; i++)
			{
				if (subnode[i] != null)
				{
					subnode[i].AddAllItemsFromOverlapping(searchEnv, resultItems);
				}
			}
		}
		
        public void Visit(Envelope searchEnv, ISpatialIndexVisitor visitor)
        {
            if (!IsSearchMatch(searchEnv))
                return ;
			
            // this node may have items as well as subnodes (since items may not
            // be wholely contained in any single subnode
            VisitItems(searchEnv, visitor);
			
            for (int i = 0; i < 4; i++)
            {
                if (subnode[i] != null)
                {
                    subnode[i].Visit(searchEnv, visitor);
                }
            }
        }
		
        private void VisitItems(Envelope searchEnv, ISpatialIndexVisitor visitor)
        {
            // would be nice to filter items based on search envelope, but can't until they contain an envelope
            for (IEnumerator i = m_arrItems.GetEnumerator(); i.MoveNext(); )
            {
                visitor.VisitItem(i.Current);
            }
        }
		
		//<<TODO:RENAME?>> In Samet's terminology, I think what we're returning here is
		//actually level+1 rather than depth. (See p. 4 of his book) [Jon Aquino]
		internal int Depth()
		{
			int maxSubDepth = 0;
			for (int i = 0; i < 4; i++)
			{
				if (subnode[i] != null)
				{
					int sqd = subnode[i].Depth();
					if (sqd > maxSubDepth)
						maxSubDepth = sqd;
				}
			}
			return maxSubDepth + 1;
		}
		
		internal int Size()
		{
			int subSize = 0;
			for (int i = 0; i < 4; i++)
			{
				if (subnode[i] != null)
				{
					subSize += subnode[i].Size();
				}
			}

			return subSize + m_arrItems.Count;
		}
		
		internal int GetNodeCount()
		{
			int subSize = 0;
			for (int i = 0; i < 4; i++)
			{
				if (subnode[i] != null)
				{
					subSize += subnode[i].Size();
				}
			}

			return subSize + 1;
		}
	}
}