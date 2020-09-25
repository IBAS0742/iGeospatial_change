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
	/// Defines the constants for specifying binary byte orders.
	/// </summary>
	[Serializable]
    public enum BytesOrder
	{
        /// <summary>
        /// Specifics a big-endian order. In this order, the bytes of a multibyte 
        /// value are ordered from the most significant to the least significant.
        /// </summary>
        BigEndian    = 0,

        /// <summary>
        /// Specifics a big-endian order. In this order, the bytes of a multibyte 
        /// value are ordered from the most significant to the least significant.
        /// </summary>
        LittleEndian = 1
	}
}
