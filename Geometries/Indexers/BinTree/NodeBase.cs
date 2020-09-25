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

namespace iGeospatial.Geometries.Indexers.BinTree
{
	
	/// <summary> 
	/// The base class for nodes in a <see cref="Bintree"/>.
	/// </summary>
    [Serializable]
    internal abstract class NodeBase
	{
        protected internal ArrayList m_arrItems;
		
        protected NodeBase()
        {
            subnode = new Node[2];

            m_arrItems = new ArrayList();
        }
		
		/// <summary> subnodes are numbered as follows:
		/// 
		/// 0 | 1
		/// </summary>
        protected internal Node[] subnode;
		
		public ArrayList Items
		{
			get
			{
				return m_arrItems;
			}
		}
		
		/// <summary> 
		/// Returns the index of the subnode that wholely Contains the given interval.
		/// If none does, returns -1.
		/// </summary>
		public static int GetSubnodeIndex(Interval interval, double centre)
		{
			int subnodeIndex = - 1;
			if (interval.Min >= centre)
				subnodeIndex = 1;
			if (interval.Max <= centre)
				subnodeIndex = 0;
			return subnodeIndex;
		}

		public void Add(object item)
		{
			m_arrItems.Add(item);
		}

		public ArrayList AddAllItems(ArrayList items)
		{
			m_arrItems.AddRange(this.m_arrItems);
			for (int i = 0; i < 2; i++)
			{
				if (subnode[i] != null)
				{
					subnode[i].AddAllItems(items);
				}
			}

			return items;
		}

		protected abstract bool IsSearchMatch(Interval interval);
		
		public ArrayList AddAllItemsFromOverlapping(Interval interval, 
            ArrayList resultItems)
		{
			if (!IsSearchMatch(interval))
				return m_arrItems;
			
            // some of these may not actually overlap - this is allowed by the bintree contract
            resultItems.AddRange(m_arrItems);
			
			for (int i = 0; i < 2; i++)
			{
				if (subnode[i] != null)
				{
					subnode[i].AddAllItemsFromOverlapping(interval, resultItems);
				}
			}

			return m_arrItems;
		}
		
		internal int Depth()
		{
			int maxSubDepth = 0;
			for (int i = 0; i < 2; i++)
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
			for (int i = 0; i < 2; i++)
			{
				if (subnode[i] != null)
				{
					subSize += subnode[i].Size();
				}
			}

			return subSize + m_arrItems.Count;
		}
		
		internal int NodeSize()
		{
			int subSize = 0;
			for (int i = 0; i < 2; i++)
			{
				if (subnode[i] != null)
				{
					subSize += subnode[i].NodeSize();
				}
			}

			return subSize + 1;
		}
	}
}