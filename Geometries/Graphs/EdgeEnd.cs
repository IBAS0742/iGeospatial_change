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
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Graphs
{
	
	/// <summary> 
	/// Models the end of an edge incident on a node.
	/// </summary>
	/// <remarks>
	/// EdgeEnds have a direction determined by the direction of the ray from 
	/// the initial point to the next point.
	/// EdgeEnds are comparable under the ordering
	/// <para>
	/// "a has a greater angle with the x-axis than b".
	/// </para>
	/// This ordering is used to sort EdgeEnds around a node.
	/// </remarks>
	[Serializable]
    internal class EdgeEnd : IComparable
	{
        #region Protected Members
		
		internal Edge  m_objEdge; // the parent edge of this edge end
		internal Label m_objLabel;
		
        #endregion
		
        #region Private Fields

		private Node node; // the node this edge end originates at
		private Coordinate p0, p1; // points of initial line segment
		private double dx, dy; // the direction vector for this edge from its starting point
		private int quadrant;
		
        #endregion

        #region Constructors and Destructor

        public EdgeEnd(Edge edge, Coordinate p0, Coordinate p1) : this(edge, p0, p1, null)
        {
        }

        public EdgeEnd(Edge edge, Coordinate p0, Coordinate p1, Label label) : this(edge)
        {
            Initialize(p0, p1);
            this.m_objLabel = label;
        }
		
        internal EdgeEnd(Edge edge)
        {
            this.m_objEdge = edge;
        }

        #endregion

        #region Public Properties
		
        public virtual Edge Edge
		{
			get
			{
				return m_objEdge;
			}
		}

		public virtual Label Label
		{
			get
			{
				return m_objLabel;
			}
		}

		public Coordinate Coordinate
		{
			get
			{
				return p0;
			}
		}

		public Coordinate DirectedCoordinate
		{
			get
			{
				return p1;
			}
		}

		public int Quadrant
		{
			get
			{
				return quadrant;
			}
		}

		public double Dx
		{
			get
			{
				return dx;
			}
		}

		public double Dy
		{
			get
			{
				return dy;
			}
		}

		public Node Node
		{
			get
			{
				return node;
			}
			
			set
			{
				this.node = value;
			}
		}

        #endregion
		
		protected void Initialize(Coordinate p0, Coordinate p1)
		{
			this.p0 = p0;
			this.p1 = p1;
			dx = p1.X - p0.X;
			dy = p1.Y - p0.Y;
			quadrant = iGeospatial.Geometries.Graphs.Quadrant.GetQuadrant(dx, dy);
			Debug.Assert(!(dx == 0 && dy == 0), "EdgeEnd with identical endpoints found");
		}
		
		public int CompareTo(object obj)
		{
			EdgeEnd e = (EdgeEnd) obj;

			return CompareDirection(e);
		}

		/// <summary>
		/// Compares the directions of the specified <see cref="EdgeEnd"/> and the current
		/// EdgeEnd.
		/// </summary>
		/// <remarks>
		/// Implements the total order relation:
		/// <para>
		/// "a" has a greater angle with the positive x-axis than "b"
		/// </para>
		/// <para>
		/// Using the obvious algorithm of simply computing the angle is not robust,
		/// since the angle calculation is obviously susceptible to roundoff.
		/// </para>
		/// A robust algorithm is:
		/// <list type="number">
		/// <item>
		/// <description>
		/// first Compare the quadrant.  If the quadrants
		/// are different, it it trivial to determine which vector is "greater".
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// if the vectors lie in the same quadrant, the computeOrientation function
		/// can be used to decide the relative orientation of the vectors.
		/// </description>
		/// </item>
		/// </list>
		/// </remarks>
		public int CompareDirection(EdgeEnd e)
		{
			if (dx == e.dx && dy == e.dy)
				return 0;
			// if the rays are in different quadrants, determining the ordering is trivial
			if (quadrant > e.quadrant)
				return 1;
			if (quadrant < e.quadrant)
				return -1;
			// vectors are in the same quadrant - check relative 
            // orientation of direction vectors
			// this is > e if it is CCW of e

			return (int)CGAlgorithms.ComputeOrientation(e.p0, e.p1, p1);
		}
		
		public virtual void ComputeLabel()
		{
			// subclasses should override this if they are using labels
		}
	}
}