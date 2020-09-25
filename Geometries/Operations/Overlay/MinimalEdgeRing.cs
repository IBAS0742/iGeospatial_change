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

using iGeospatial.Geometries.Graphs;

namespace iGeospatial.Geometries.Operations.Overlay
{
	/// <summary> 
	/// A ring of <see cref="Edge"/>s with the property that no node
	/// has degree greater than 2.  These are the form of rings required
	/// to represent polygons under the OGC SFS spatial data model.
	/// </summary>
	/// <seealso cref="MaximalEdgeRing">
	/// </seealso>
	internal class MinimalEdgeRing : EdgeRing
	{            
        #region Constructors and Destructor

		public MinimalEdgeRing(DirectedEdge start, GeometryFactory geometryFactory)
            : base(start, geometryFactory)
		{
		}

        #endregion
		
        #region Public Methods

		public override DirectedEdge GetNext(DirectedEdge de)
		{
			return de.NextMin;
		}

		public override void SetEdgeRing(DirectedEdge de, EdgeRing er)
		{
			de.MinEdgeRing = er;
		}
        
        #endregion
	}
}