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

namespace iGeospatial.Geometries.Indexers.StrTree
{
	/// <summary> 
	/// A spatial object in an AbstractSTRTree.
	/// </summary>
	internal interface IBoundable
	{
		/// <summary> 
		/// Returns a representation of space that encloses this IBoundable, preferably
		/// not much bigger than this IBoundable's boundary yet fast to test for intersection
		/// with the bounds of other Boundables. The class of object returned depends
		/// on the subclass of AbstractSTRTree.
		/// </summary>
		/// <returns> an Envelope (for STRtrees), an Interval (for SIRtrees), or other object
		/// (for other subclasses of AbstractSTRTree)
		/// </returns>
		/// <seealso cref="AbstractSTRTree.IIntersectsOp">
		/// </seealso>
		object Bounds
		{
			get;
		}
	}
}