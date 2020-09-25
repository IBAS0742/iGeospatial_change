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

namespace iGeospatial.Geometries.Indexers.BinTree
{
	/// <summary> 
	/// A node of a <see cref="Bintree"/>.
	/// </summary>
    [Serializable]
    internal class Node : NodeBase
	{
        private Interval interval;
        private double centre;
        private int level;
		
        public Node(Interval interval, int level)
        {
            this.interval = interval;
            this.level = level;
            centre = (interval.Min + interval.Max) / 2;
        }
		
		public Interval Interval
		{
			get
			{
				return interval;
			}
			
		}
		public static Node CreateNode(Interval itemInterval)
		{
			Key key = new Key(itemInterval);
			
			Node node = new Node(key.Interval, key.Level);
			return node;
		}
		
		public static Node CreateExpanded(Node node, Interval addInterval)
		{
			Interval expandInt = new Interval(addInterval);
			if (node != null)
				expandInt.ExpandToInclude(node.interval);
			
			Node largerNode = CreateNode(expandInt);
			if (node != null)
				largerNode.Insert(node);
			return largerNode;
		}
		
		protected override bool IsSearchMatch(Interval itemInterval)
		{
			return itemInterval.Overlaps(interval);
		}
		
		/// <summary> Returns the subnode containing the envelope.
		/// Creates the node if
		/// it does not already exist.
		/// </summary>
		public Node GetNode(Interval searchInterval)
		{
			int subnodeIndex = GetSubnodeIndex(searchInterval, centre);
			// if index is -1 searchEnv is not contained in a subnode
			if (subnodeIndex != - 1)
			{
				// create the node if it does not exist
				Node node = GetSubnode(subnodeIndex);
				// recursively search the found/created node
				return node.GetNode(searchInterval);
			}
			else
			{
				return this;
			}
		}
		
		/// <summary> Returns the smallest <i>existing</i>
		/// node containing the envelope.
		/// </summary>
		public NodeBase Find(Interval searchInterval)
		{
			int subnodeIndex = GetSubnodeIndex(searchInterval, centre);
			if (subnodeIndex == - 1)
				return this;
			if (subnode[subnodeIndex] != null)
			{
				// query lies in subnode, so search it
				Node node = subnode[subnodeIndex];
				return node.Find(searchInterval);
			}
			// no existing subnode, so return this one anyway
			return this;
		}
		
		internal void Insert(Node node)
		{
			Debug.Assert(interval == null || interval.Contains(node.interval));
			int index = GetSubnodeIndex(node.interval, centre);
			if (node.level == level - 1)
			{
				subnode[index] = node;
			}
			else
			{
				// the node is not a direct child, so make a new child node to contain it
				// and recursively insert the node
				Node childNode = CreateSubnode(index);
				childNode.Insert(node);
				subnode[index] = childNode;
			}
		}
		
		/// <summary> get the subnode for the index.
		/// If it doesn't exist, create it
		/// </summary>
		private Node GetSubnode(int index)
		{
			if (subnode[index] == null)
			{
				subnode[index] = CreateSubnode(index);
			}
			return subnode[index];
		}
		
		private Node CreateSubnode(int index)
		{
			// create a new subnode in the appropriate interval
			
			double min = 0.0;
			double max = 0.0;
			
			switch (index)
			{
				
				case 0: 
					min = interval.Min;
					max = centre;
					break;
				
				case 1: 
					min = centre;
					max = interval.Max;
					break;
				}
			Interval subInt = new Interval(min, max);
			Node node = new Node(subInt, level - 1);
			return node;
		}
	}
}