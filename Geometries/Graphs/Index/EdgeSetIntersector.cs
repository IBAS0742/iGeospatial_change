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

namespace iGeospatial.Geometries.Graphs.Index
{
	/// <summary> 
	/// An EdgeSetIntersector computes all the intersections between the
	/// edges in a set.  
	/// </summary>
	/// <remarks>
	/// It adds the computed intersections to each edge they are found on. 
	/// It may be used in two scenarios:
	/// <list type="number">
	/// <item>
	/// <description>
	/// determining the internal intersections between a single set of edges.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// determining the mutual intersections between two different sets of edges.
	/// </description>
	/// </item>
	/// </list>
	/// It uses a <see cref="SegmentIntersector"/> to compute the intersections between
	/// segments and to record statistics about what kinds of intersections were found.
	/// </remarks>
	internal abstract class EdgeSetIntersector
	{
        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeSetIntersector"/> derived class.
        /// </summary>
		protected EdgeSetIntersector()
		{
		}

        #endregion
		
        #region Public Methods

		/// <summary> Computes all self-intersections between edges in a set of edges,
		/// allowing client to choose whether self-intersections are computed.
		/// </summary>
		/// <param name="edges">A list of edges to test for intersections.</param>
		/// <param name="si">The SegmentIntersector to use.</param>
		/// <param name="testAllSegments">true if self-intersections are to be tested as well
		/// </param>
		public abstract void  ComputeIntersections(EdgeCollection edges, 
            SegmentIntersector si, bool testAllSegments);
		
		/// <summary> 
		/// Computes all mutual intersections between two sets of edges.
		/// </summary>
		/// <param name="edges0">A list of edges to test for intersections.</param>
		/// <param name="edges1">A list of edges to test for intersections.</param>
		/// <param name="si">The SegmentIntersector to use.</param>
		public abstract void  ComputeIntersections(EdgeCollection edges0, 
            EdgeCollection edges1, SegmentIntersector si);
        
        #endregion
	}
}