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

namespace iGeospatial.Geometries
{
	/// <summary>    
	/// Constants representing the dimensions of a point, a curve and a surface.
	/// </summary>
	/// <remarks>
	/// Also, constants representing the dimensions of the empty geometry and
	/// non-empty geometries, and a wildcard dimension meaning "any dimension".
	/// </remarks>
    /// <seealso cref="IntersectionMatrix"/>
    [Serializable]
    public enum DimensionType
	{
        /// <summary>  
        /// Dimension value of a point (0).
        /// </summary>
		Point    = 0,
		
		/// <summary>  
		/// Dimension value of a curve (1).
		/// </summary>
		Curve    = 1,
		
		/// <summary>  
		/// Dimension value of a surface (2).
		/// </summary>
		Surface  = 2,
		
		/// <summary>  
		/// Dimension value of the empty geometry (-1).
		/// </summary>
		Empty    = -1,
		
		/// <summary>  
		/// Dimension value of non-empty geometries (= {Point, Curve, Surface}).
		/// </summary>
		NonEmpty = -2,
		
		/// <summary>  
		/// Dimension value for any dimension (= {Empty, NonEmpty}).
		/// </summary>
		DontCare = -3	
    }
}