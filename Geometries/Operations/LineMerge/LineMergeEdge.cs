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
using iGeospatial.Geometries;
using iGeospatial.Geometries.PlanarGraphs;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Operations.LineMerge
{
	/// <summary> 
	/// An edge of a <see cref="LineMergeGraph"/>. The marked field indicates
	/// whether this Edge has been logically deleted from the graph.
	/// </summary>
	internal sealed class LineMergeEdge : Edge
	{
        #region Private Fields

		private LineString line;
        
        #endregion

        #region Constructors and Destructor

		/// <summary> 
		/// Constructs a LineMergeEdge with vertices given by the 
		/// specified LineString.
		/// </summary>
		public LineMergeEdge(LineString line)
		{
			this.line = line;
		}

        #endregion
			
        #region Public Properties

		/// <summary> 
		/// Gets the LineString specifying the vertices of this edge.
		/// </summary>
		public LineString Line
		{
			get
			{
				return line;
			}
		}
        
        #endregion
	}
}