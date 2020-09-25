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

using iGeospatial.Geometries.Operations;

namespace iGeospatial.Geometries
{
	/// <summary>
	/// Specifies the end cap style of the generated buffer.
	/// </summary>
	/// <remarks>
	/// This is used with the <see cref="BufferOp"/> class.
	/// </remarks>
	/// <seealso cref="BufferOp"/>
	[Serializable]
    public enum BufferCapType
	{               
        /// <summary>
        /// No buffer operation.
        /// </summary>
        None   = 0,

		/// <summary> 
		/// Specifies a round line buffer end cap style.
		/// </summary>
		Round  = 1,

		/// <summary> 
		/// Specifies a butt (or flat) line buffer end cap style.
		/// </summary>
		Flat   = 2,

		/// <summary> 
		/// Specifies a square line buffer end cap style.
		/// </summary>
		Square = 3
	}
}
