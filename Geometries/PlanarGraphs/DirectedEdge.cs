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
using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.PlanarGraphs
{
	/// <summary> 
	/// Represents a directed edge in a <see cref="PlanarGraph"/>. A DirectedEdge may or
	/// may not have a reference to a parent <see cref="Edge"/> (some applications of
	/// planar graphs may not require explicit Edge objects to be created). Usually
	/// a client using a PlanarGraph will subclass DirectedEdge
	/// to Add its own application-specific data and methods.
	/// </summary>
	internal class DirectedEdge : PlanarGraphObject, IComparable
	{
		private Edge parentEdge;
		private Node from;
		private Node to;
		private Coordinate p0, p1;
		private DirectedEdge m_objSym; // optional
		private bool m_bEdgeDirection;
		private int m_nQuadrant;
		private double m_dAngle;
		
        /// <summary> 
        /// Constructs a DirectedEdge connecting the <c>from</c> node to the
        /// <c>to</c> node.
        /// </summary>
        /// <param name="directionPt">
        /// Specifies this DirectedEdge's direction vector (determined by 
        /// the vector from the <c>from</c> node to <c>directionPt</c>)
        /// </param>
        /// <param name="edgeDirection">
        /// Whether this DirectedEdge's direction is the same as or
        /// opposite to that of the parent Edge (if any)
        /// </param>
        public DirectedEdge(Node from, Node to, Coordinate directionPt, 
            bool edgeDirection)
		{
			this.from          = from;
			this.to            = to;
			this.m_bEdgeDirection = edgeDirection;

			p0 = from.Coordinate;
			p1 = directionPt;

			double dx = p1.X - p0.X;
			double dy = p1.Y - p0.Y;

			m_nQuadrant = iGeospatial.Geometries.Graphs.Quadrant.GetQuadrant(dx, dy);
			m_dAngle    = Math.Atan2(dy, dx);
			//Assert.isTrue(! (dx == 0 && dy == 0), "EdgeEnd with identical endpoints found");
		}
		
		/// <summary> 
		/// Gets or sets this DirectedEdge's parent Edge, or <see langword="null"/> 
		/// if it has none.
		/// </summary>
		/// <value> The associated Edge with this DirectedEdge (possibly <see langword="null"/>, 
		/// indicating no associated Edge).
		/// </value>
		public Edge Edge
		{
			get
			{
				return parentEdge;
			}
			
			set
			{
				this.parentEdge = value;
			}
		}

		/// <summary> 
		/// Returns 0, 1, 2, or 3, indicating the quadrant in which this DirectedEdge's
		/// orientation lies.
		/// </summary>
		public int Quadrant
		{
			get
			{
				return m_nQuadrant;
			}
		}

		/// <summary> 
		/// Returns a point to which an imaginary line is drawn from the from-node to
		/// specify this DirectedEdge's orientation.
		/// </summary>
		public Coordinate DirectionPt
		{
			get
			{
				return p1;
			}
		}

		/// <summary> 
		/// Returns whether the direction of the parent Edge (if any) is the same as that
		/// of this Directed Edge.
		/// </summary>
		public bool EdgeDirection
		{
			get
			{
				return m_bEdgeDirection;
			}
		}

		/// <summary> 
		/// Gets the node from which this DirectedEdge leaves.
		/// </summary>
		public Node FromNode
		{
			get
			{
				return from;
			}
		}

		/// <summary> 
		/// Gets the node to which this DirectedEdge goes.
		/// </summary>
		public Node ToNode
		{
			get
			{
				return to;
			}
		}

		/// <summary> 
		/// Gets the coordinate of the from-node.
		/// </summary>
		public Coordinate Coordinate
		{
			get
			{
				return from.Coordinate;
			}
		}

		/// <summary> 
		/// Returns the angle that the start of this DirectedEdge makes with the
		/// positive x-axis, in radians.
		/// </summary>
		public double Angle
		{
			get
			{
				return m_dAngle;
			}
		}

		/// <summary> 
		/// Gets or sets the symmetric DirectedEdge -- the other DirectedEdge associated with
		/// this DirectedEdge's parent Edge.
		/// </summary>
		/// <value> 
		/// This current DirectedEdge's symmetric DirectedEdge, which runs in the opposite
		/// direction.
		/// </value>
		public DirectedEdge Sym
		{
			get
			{
				return m_objSym;
			}
			
			set
			{
				this.m_objSym = value;
			}
			
		}

        /// <summary> 
        /// Tests whether this directed edge has been removed from its 
        /// containing graph.
        /// </summary>
        /// <value> 
        /// <c>true</c> if this directed edge is removed.
        /// </value>
        public override bool IsRemoved
        {
            get
            {
                return (parentEdge == null);
            }
        }       

		/// <summary> 
		/// Returns a List containing the parent Edge (possibly null) for each of the given
		/// DirectedEdges.
		/// </summary>
		public static ArrayList ToEdges(ArrayList dirEdges)
		{
			ArrayList edges = new ArrayList();

            for (IEnumerator i = dirEdges.GetEnumerator(); i.MoveNext(); )
			{
				edges.Add(((DirectedEdge) i.Current).parentEdge);
			}

			return edges;
		}
		
        /// <summary> Removes this directed edge from its containing graph.</summary>
        public void Remove()
        {
            this.m_objSym = null;
            this.parentEdge = null;
        }
		
		/// <summary> 
		/// Returns 1 if this DirectedEdge has a greater angle with the
		/// positive x-axis than b", 0 if the DirectedEdges are collinear, and -1 otherwise.
		/// </summary>
		/// <remarks>
		/// Using the obvious algorithm of simply computing the angle is not robust,
		/// since the angle calculation is susceptible to roundoff. A robust algorithm
		/// is:
		/// <list type="number">
		/// <item>
		/// <description>
		/// first Compare the quadrants. If the quadrants are different, it it
		/// trivial to determine which vector is "greater".
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// if the vectors lie in the same quadrant, the robust
		/// <see cref="RobustCGAlgorithms.ComputeOrientation(Coordinate, Coordinate, Coordinate)"/>
		/// function can be used to decide the relative orientation of the vectors.
		/// </description>
		/// </item>
		/// </list>
		/// </remarks>
		public int CompareTo(object obj)
		{
			DirectedEdge de = (DirectedEdge)obj;

			return CompareDirection(de);
		}
		
		/// <summary> 
		/// Returns 1 if this DirectedEdge has a greater angle with the
		/// positive x-axis than b", 0 if the DirectedEdges are collinear, and -1 otherwise.
		/// </summary>
		/// <remarks>
		/// Using the obvious algorithm of simply computing the angle is not robust,
		/// since the angle calculation is susceptible to roundoff. A robust algorithm
		/// is:
		/// <list type="number">
		/// <item>
		/// <description>
		/// first Compare the quadrants. If the quadrants are different, it it
		/// trivial to determine which vector is "greater".
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// if the vectors lie in the same quadrant, the robust
		/// <see cref="RobustCGAlgorithms.ComputeOrientation(Coordinate, Coordinate, Coordinate)"/>
		/// function can be used to decide the relative orientation of the vectors.
		/// </description>
		/// </item>
		/// </list>
		/// </remarks>
		public int CompareDirection(DirectedEdge e)
		{
			// if the rays are in different quadrants, determining the ordering is trivial
			if (m_nQuadrant > e.m_nQuadrant)
				return 1;
			if (m_nQuadrant < e.m_nQuadrant)
				return -1;

			// vectors are in the same quadrant - check relative orientation of direction vectors
			// this is > e if it is CCW of e
			return (int)CGAlgorithms.ComputeOrientation(e.p0, e.p1, p1);
		}
	}
}