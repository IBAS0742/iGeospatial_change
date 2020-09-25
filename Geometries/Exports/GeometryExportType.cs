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

namespace iGeospatial.Geometries.Exports
{
	/// <summary>
	/// Specifies the geometry export format types.
	/// </summary>
	[Serializable]
    public enum GeometryExportType
	{
        /// <summary>
        /// Not specified or unknown format.
        /// </summary>
        None   = 0,

        /// <summary>
        /// OpenGIS Well-Known Text (WKT) format.
        /// </summary>
        Wkt    = 1,

        /// <summary>
        /// OpenGIS Well-Known Binary (WKB) format.
        /// </summary>
        Wkb    = 2,

        /// <summary>
        /// OpenGIS Geography Markup Language (GML) format.
        /// </summary>
        Gml    = 3,

        /// <summary>
        /// Japanese Geospatial Extensible Markup Language (G-XML).
        /// </summary>
        GXml   = 4,

        /// <summary>
        /// User-defined formats.
        /// </summary>
        Custom = 20,
	}
}
