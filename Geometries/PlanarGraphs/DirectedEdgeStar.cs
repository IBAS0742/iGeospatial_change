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

namespace iGeospatial.Geometries.PlanarGraphs
{
	/// <summary> 
	/// A sorted collection of <see cref="DirectedEdge"/>s which leave a 
	/// <see cref="Node"/> in a <see cref="PlanarGraph"/>.
	/// </summary>
	internal class DirectedEdgeStar
	{
		/// <summary> 
		/// The underlying list of outgoing DirectedEdges
		/// </summary>
		private ArrayList outEdges;
		private bool sorted;
		
		/// <summary> Constructs a DirectedEdgeStar with no edges.</summary>
		public DirectedEdgeStar()
		{
            outEdges = new ArrayList();
        }

		/// <summary> 
		/// Returns the number of edges around the Node associated with this DirectedEdgeStar.
		/// </summary>
		public int Degree
		{
			get
			{
				return outEdges.Count;
			}
		}

		/// <summary> 
		/// Returns the coordinate for the node at wich this star is based.
		/// </summary>
		public Coordinate Coordinate
		{
			get
			{
				IEnumerator it = Iterator();

				if (!it.MoveNext())
					return null;

                DirectedEdge e = (DirectedEdge) it.Current;

				return e.Coordinate;
			}
		}

		/// <summary> 
		/// Returns the DirectedEdges, in ascending order by angle with the positive x-axis.
		/// </summary>
		public ArrayList Edges
		{
			get
			{
				SortEdges();

				return outEdges;
			}
		}
		
		/// <summary> Adds a new member to this DirectedEdgeStar.</summary>
		public void Add(DirectedEdge de)
		{
			outEdges.Add(de);
			sorted = false;
		}
		
        /// <summary> Drops a member of this DirectedEdgeStar.</summary>
		public void Remove(DirectedEdge de)
		{
			outEdges.Remove(de);
		}

		/// <summary> 
		/// Returns an Iterator over the DirectedEdges, in ascending order by angle 
		/// with the positive x-axis.
		/// </summary>
		public IEnumerator Iterator()
		{
			SortEdges();

			return outEdges.GetEnumerator();
		}
		
		private void SortEdges()
		{
			if (!sorted)
			{
                if (outEdges != null)
                {
                    outEdges.Sort();
                    sorted = true;
                }
			}
		}

		/// <summary> 
		/// Returns the zero-based index of the given Edge, after sorting in ascending order
		/// by angle with the positive x-axis.
		/// </summary>
		public int GetIndex(Edge edge)
		{
			SortEdges();
			for (int i = 0; i < outEdges.Count; i++)
			{
				DirectedEdge de = (DirectedEdge) outEdges[i];
				if (de.Edge == edge)
					return i;
			}

			return -1;
		}

		/// <summary> 
		/// Returns the zero-based index of the given DirectedEdge, after sorting 
		/// in ascending order by angle with the positive x-axis.
		/// </summary>
		public int GetIndex(DirectedEdge dirEdge)
		{
			SortEdges();
			for (int i = 0; i < outEdges.Count; i++)
			{
				DirectedEdge de = (DirectedEdge) outEdges[i];
				if (de == dirEdge)
					return i;
			}

			return -1;
		}

		/// <summary> 
		/// Returns the remainder when i is divided by the number of edges in this
		/// DirectedEdgeStar. 
		/// </summary>
		public int GetIndex(int i)
		{
			int modi = i % outEdges.Count;
			//I don't think modi can be 0 (assuming i is positive) [Jon Aquino 10/28/2003] 
			if (modi < 0)
				modi += outEdges.Count;
			return modi;
		}
		
		/// <summary> 
		/// Returns the DirectedEdge on the left-hand side of the given DirectedEdge (which
		/// must be a member of this DirectedEdgeStar). 
		/// </summary>
		public DirectedEdge GetNextEdge(DirectedEdge dirEdge)
		{
			int i = GetIndex(dirEdge);

			return (DirectedEdge) outEdges[GetIndex(i + 1)];
		}
	}
}