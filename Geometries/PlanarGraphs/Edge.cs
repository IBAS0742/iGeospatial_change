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

namespace iGeospatial.Geometries.PlanarGraphs
{
	/// <summary> 
	/// Represents an undirected edge of a <see cref="PlanarGraph"/>. An undirected edge
	/// in fact simply acts as a central point of reference for two opposite
	/// <see cref="DirectedEdge"/>s.
	/// <para>
	/// Usually a client using a PlanarGraph will subclass Edge
	/// to Add its own application-specific data and methods.
	/// </para>
	/// </summary>
	internal class Edge : PlanarGraphObject
	{
        /// <summary> The two DirectedEdges associated with this Edge.
        /// 0 is forward, 1 is reverse
        /// </summary>
        internal DirectedEdge[] dirEdge;
		
		/// <summary> 
		/// Constructs an Edge whose DirectedEdges are not yet set. Be sure to call
		/// <see cref="Edge.SetDirectedEdges(DirectedEdge, DirectedEdge)"/>
		/// </summary>
		public Edge()
		{
		}
		
		/// <summary> 
		/// Constructs an Edge initialized with the given DirectedEdges, and for each
		/// DirectedEdge: sets the Edge, sets the symmetric DirectedEdge, and adds
		/// this Edge to its from-Node.
		/// </summary>
		public Edge(DirectedEdge de0, DirectedEdge de1)
		{
			SetDirectedEdges(de0, de1);
		}

        /// <summary> 
        /// Tests whether this edge has been removed from its containing 
        /// graph.
        /// </summary>
        /// <value> 
        /// <c>true</c> if this edge is removed
        /// </value>
        public override bool IsRemoved
        {
            get
            {
                return (dirEdge == null);
            }
        }       
		
        /// <summary> 
        /// Removes this edge from its containing graph.
        /// </summary>
        public void Remove()
        {
            this.dirEdge = null;
        }
		
		/// <summary> 
		/// Initializes this Edge's two DirectedEdges, and for each DirectedEdge: sets the
		/// Edge, sets the symmetric DirectedEdge, and adds this Edge to its from-Node.
		/// </summary>
		public void SetDirectedEdges(DirectedEdge de0, DirectedEdge de1)
		{
			dirEdge = new DirectedEdge[]{de0, de1};
			de0.Edge = this;
			de1.Edge = this;
			de0.Sym = de1;
			de1.Sym = de0;
			de0.FromNode.AddOutEdge(de0);
			de1.FromNode.AddOutEdge(de1);
		}
		
		/// <summary> 
		/// Returns one of the DirectedEdges associated with this Edge.
		/// </summary>
        /// <param name="i">0 or 1.  0 returns the forward directed edge, 1 returns the reverse
        /// </param>
		public DirectedEdge GetDirEdge(int i)
		{
			return dirEdge[i];
		}
		
		/// <summary> 
		/// Returns the <see cref="DirectedEdge"/> that starts from the given node, 
		/// or <see langword="null"/> if the node is not one of the two nodes 
		/// associated with this Edge.
		/// </summary>
		public DirectedEdge GetDirEdge(Node fromNode)
		{
			if (dirEdge[0].FromNode == fromNode)
				return dirEdge[0];
			if (dirEdge[1].FromNode == fromNode)
				return dirEdge[1];
			// node not found
			// possibly should throw an exception here?
			return null;
		}
		
		/// <summary> 
		/// If node is one of the two nodes associated with this Edge,
		/// returns the other node; otherwise returns null.
		/// </summary>
		public Node GetOppositeNode(Node node)
		{
			if (dirEdge[0].FromNode == node)
				return dirEdge[0].ToNode;
			if (dirEdge[1].FromNode == node)
				return dirEdge[1].ToNode;
			// node not found
			// possibly should throw an exception here?
			return null;
		}
	}
}