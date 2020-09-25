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
using iGeospatial.Collections;
using iGeospatial.Collections.Sets;

namespace iGeospatial.Geometries.PlanarGraphs
{
	/// <summary> 
	/// A node in a <see cref="PlanarGraph"/> is a location where 0 or 
	/// more <see cref="Edge"/>s meet. 
	/// </summary>
	/// <remarks>
	/// A node is connected to each of its incident Edges via an outgoing
	/// DirectedEdge. Some clients using a PlanarGraph may want to
	/// subclass Node to add their own application-specific
	/// data and methods.
	/// </remarks>
	internal class Node : PlanarGraphObject
	{
		/// <summary>The location of this Node </summary>
		internal Coordinate pt;
		
		/// <summary>The collection of DirectedEdges that leave this Node </summary>
		internal DirectedEdgeStar deStar;
		
		/// <summary> Constructs a Node with the given location.</summary>
		public Node(Coordinate pt) : this(pt, new DirectedEdgeStar())
		{
		}
		
		/// <summary> Constructs a Node with the given location and collection of outgoing DirectedEdges.</summary>
		public Node(Coordinate pt, DirectedEdgeStar deStar)
		{
			this.pt = pt;
			this.deStar = deStar;
		}
		
		/// <summary> Returns the location of this Node.</summary>
		public Coordinate Coordinate
		{
			get
			{
				return pt;
			}
		}
			
		/// <summary> Returns the collection of DirectedEdges that leave this Node.</summary>
		public DirectedEdgeStar OutEdges
		{
			get
			{
				return deStar;
			}
		}

        /// <summary> 
        /// Tests whether this node has been removed from its 
        /// containing graph.
        /// </summary>
        /// <value> <c>true</c> if this node is removed
        /// </value>
        public override bool IsRemoved
        {
            get
            {
                return (pt == null);
            }
        }       
			
		/// <summary> Returns the number of edges around this Node.</summary>
		public int Degree
		{
			get
			{
				return deStar.Degree;
			}
		}
		
        /// <summary> 
        /// Removes this node from its containing graph.
        /// </summary>
        public void Remove()
        {
            pt = null;
        }

		/// <summary> 
		/// Returns all Edges that connect the two nodes (which are assumed to be different).
		/// </summary>
		public static ICollection GetEdgesBetween(Node node0, Node node1)
		{
			ArrayList edges0 = DirectedEdge.ToEdges(node0.OutEdges.Edges);
			ISet commonEdges = new HashedSet(edges0);

            ArrayList edges1 = DirectedEdge.ToEdges(node1.OutEdges.Edges);
			commonEdges.RetainAll(edges1);

			return commonEdges;
		}
		
		/// <summary> Adds an outgoing DirectedEdge to this Node.</summary>
		public void AddOutEdge(DirectedEdge de)
		{
			deStar.Add(de);
		}

		/// <summary> Returns the zero-based index of the given Edge, after sorting in ascending order
		/// by angle with the positive x-axis.
		/// </summary>
		public int GetIndex(Edge edge)
		{
			return deStar.GetIndex(edge);
		}
	}
}