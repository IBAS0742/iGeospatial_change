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

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Editors
{
    /// <summary> 
    /// A interface which specifies an edit operation for Geometries.
    /// </summary>
    public interface IGeometryEdit
    {
        /// <summary> 
        /// Edits a Geometry by returning a new Geometry with a modification.
        /// The returned Geometry might be the same as the Geometry passed in.
        /// </summary>
        /// <param name="geometry">the Geometry to modify
        /// </param>
        /// <param name="factory">the factory with which to construct the modified Geometry
        /// (may be different to the factory of the input geometry)
        /// </param>
        /// <returns> a new Geometry which is a modification of the input Geometry
        /// </returns>
        Geometry Edit(Geometry geometry, GeometryFactory factory);
    }
		
}
