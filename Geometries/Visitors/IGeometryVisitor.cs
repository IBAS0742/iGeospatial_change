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
using iGeospatial.Geometries;

namespace iGeospatial.Geometries.Visitors
{ 	
	/// <summary>  
	/// GeometryCollection classes support the concept of applying a 
	/// IGeometryVisitor to the Geometry.
	/// </summary>
	/// <remarks>
	/// The filter is applied to every element of the Geometry.
	/// A IGeometryVisitor can either record information about the Geometry
	/// or change the Geometry in some way.
	/// IGeometryVisitor is an example of the Visitor pattern.
	/// </remarks>
	public interface IGeometryVisitor
	{
		/// <summary>  
		/// Performs an operation with or on a geometry.
		/// </summary>
		/// <param name="geom"> 
		/// A Geometry to which the filter is applied.
		/// </param>
		void Visit(Geometry geom);
	}
}