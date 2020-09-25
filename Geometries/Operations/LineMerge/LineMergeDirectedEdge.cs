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
using iGeospatial.Geometries.PlanarGraphs;

namespace iGeospatial.Geometries.Operations.LineMerge
{
	/// <summary> 
	/// A <see cref="DirectedEdge"/> of a <see cref="LineMergeGraph"/>.
	/// </summary>
	internal sealed class LineMergeDirectedEdge : DirectedEdge
	{
        #region Constructors and Destructor

		/// <summary> 
		/// Constructs a LineMergeDirectedEdge connecting the from node to the
		/// to node.
		/// </summary>
		/// <param name="">directionPt
		/// specifies this DirectedEdge's direction (given by an imaginary
		/// line from the from node to directionPt)
		/// </param>
		/// <param name="">edgeDirection
		/// whether this DirectedEdge's direction is the same as or
		/// opposite to that of the parent Edge (if any)
		/// </param>
		public LineMergeDirectedEdge(Node from, Node to, Coordinate directionPt, 
            bool edgeDirection) : base(from, to, directionPt, edgeDirection)
		{
		}

        #endregion

        #region Public Properties

		/// <summary> Returns the directed edge that starts at this directed edge's end point, or null
		/// if there are zero or multiple directed edges starting there.  
		/// </summary>
		/// <returns>
		/// </returns>
		public LineMergeDirectedEdge Next
		{
			get
			{
				if (ToNode.Degree != 2)
				{
					return null;
				}
				if (ToNode.OutEdges.Edges[0] == Sym)
				{
					return (LineMergeDirectedEdge) ToNode.OutEdges.Edges[1];
				}
				Debug.Assert(ToNode.OutEdges.Edges[1] == Sym);
				
				return (LineMergeDirectedEdge) ToNode.OutEdges.Edges[0];
			}
		}
        
        #endregion
	}
}