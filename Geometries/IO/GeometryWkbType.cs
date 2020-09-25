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

namespace iGeospatial.Geometries.IO
{
	/// <summary>
	/// Summary description for GeometryWkbType.
	/// </summary>
	[Serializable]
    public enum GeometryWkbType
	{
        /// <summary>
        /// Null or empty Geometry
        /// </summary>
        None               = 0,

        /// <summary>
        /// Point.
        /// </summary>
        Point              = 1,

        /// <summary>
        /// Line string.
        /// </summary>
        LineString         = 2,
        
        /// <summary>
        /// Polygon.
        /// </summary>
        Polygon            = 3,
        
        /// <summary>
        /// Multi point.
        /// </summary>
        MultiPoint         = 4,
        
        /// <summary>
        /// Multi line string.
        /// </summary>
        MultiLineString    = 5,
        
        /// <summary>
        /// Multi polygon.
        /// </summary>
        
        MultiPolygon       = 6,
        
        /// <summary>
        /// Geometry collection.
        /// </summary>
        GeometryCollection = 7
    }
}
