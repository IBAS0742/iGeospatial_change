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

namespace iGeospatial.Geometries.Operations
{
	/// <summary>
    /// These operations implement various boolean combinations of 
    /// the resultants of the overlay.
    /// </summary>
    /// <remarks>
    /// This is used with the <see cref="OverlayOp"/> class.
    /// </remarks>
    /// <seealso cref="OverlayOp"/>
	[Serializable]
    public enum OverlayType
	{
        /// <summary>
        /// None or undefined operation.
        /// </summary>
        None                = 0,

        /// <summary>
        /// Specifies geometry intersection overlap operation.
        /// </summary>
		Intersection        = 1,

        /// <summary>
        /// Specifies geometry unioning overlap operation.
        /// </summary>
		Union               = 2,

        /// <summary>
        /// Specifies geometry differencing overlap operation.
        /// </summary>
		Difference          = 3,

        /// <summary>
        /// Specifies geometry symmetric differencing overlap operation.
        /// </summary>
		SymmetricDifference = 4
	}
}
