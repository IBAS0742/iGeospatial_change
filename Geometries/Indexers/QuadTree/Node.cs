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
	
	/// <summary> Represents a node of a {@link Quadtree}.  Nodes contain
	/// items which have a spatial extent corresponding to the node's position
	/// in the quadtree.
	/// 
	/// </summary>
	[Serializable]
    internal class Node : NodeBase
	{
        private Envelope   env;
        private Coordinate centre;
        private int        level;
		
		public Node(Envelope env, int level)
		{
			//this.parent = parent;
			this.env = env;
			this.level = level;
			centre = new Coordinate();
			centre.X = (env.MinX + env.MaxX) / 2;
			centre.Y = (env.MinY + env.MaxY) / 2;
		}
		
		public Envelope Envelope
		{
			get
			{
				return env;
			}
		}

		public static Node CreateNode(Envelope env)
		{
			Key key   = new Key(env);
			Node node = new Node(key.Envelope, key.Level);

			return node;
		}
		
		public static Node CreateExpanded(Node node, Envelope addEnv)
		{
			Envelope expandEnv = new Envelope(addEnv);
			if (node != null)
				expandEnv.ExpandToInclude(node.env);
			
			Node largerNode = CreateNode(expandEnv);
			if (node != null)
				largerNode.InsertNode(node);

			return largerNode;
		}
		
		protected override bool IsSearchMatch(Envelope searchEnv)
		{
			return env.Intersects(searchEnv);
		}
		
		/// <summary> Returns the subquad containing the envelope.
		/// Creates the subquad if
		/// it does not already exist.
		/// </summary>
		public Node GetNode(Envelope searchEnv)
		{
			int subnodeIndex = GetSubnodeIndex(searchEnv, centre);
			// if subquadIndex is -1 searchEnv is not contained in a subquad
			if (subnodeIndex != - 1)
			{
				// create the quad if it does not exist
				Node node = GetSubnode(subnodeIndex);
				// recursively search the found/created quad
				return node.GetNode(searchEnv);
			}
			else
			{
				return this;
			}
		}
		
		/// <summary> Returns the smallest <i>existing</i>
		/// node containing the envelope.
		/// </summary>
		public NodeBase Find(Envelope searchEnv)
		{
			int subnodeIndex = GetSubnodeIndex(searchEnv, centre);
			if (subnodeIndex == - 1)
				return this;
			if (subnode[subnodeIndex] != null)
			{
				// query lies in subquad, so search it
				Node node = subnode[subnodeIndex];
				return node.Find(searchEnv);
			}

			// no existing subquad, so return this one anyway
			return this;
		}
		
		internal void InsertNode(Node node)
		{
			Debug.Assert(env == null || env.Contains(node.env));

            int index = GetSubnodeIndex(node.env, centre);

            if (node.level == level - 1)
			{
				subnode[index] = node;
			}
			else
			{
				// the quad is not a direct child, so make a new child quad to contain it
				// and recursively insert the quad
				Node childNode = CreateSubnode(index);
				childNode.InsertNode(node);
				subnode[index] = childNode;
			}
		}
		
		/// <summary> get the subquad for the index.
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
			// create a new subquad in the appropriate quadrant
			
			double minx = 0.0;
			double maxx = 0.0;
			double miny = 0.0;
			double maxy = 0.0;
			
			switch (index)
			{
				case 0: 
					minx = env.MinX;
					maxx = centre.X;
					miny = env.MinY;
					maxy = centre.Y;
					break;
				
				case 1: 
					minx = centre.X;
					maxx = env.MaxX;
					miny = env.MinY;
					maxy = centre.Y;
					break;
				
				case 2: 
					minx = env.MinX;
					maxx = centre.X;
					miny = centre.Y;
					maxy = env.MaxY;
					break;
				
				case 3: 
					minx = centre.X;
					maxx = env.MaxX;
					miny = centre.Y;
					maxy = env.MaxY;
					break;
			}

			Envelope sqEnv = new Envelope(minx, maxx, miny, maxy);
			Node node      = new Node(sqEnv, level - 1);

			return node;
		}
	}
}