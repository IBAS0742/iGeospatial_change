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
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Noding
{
	/// <summary> 
	/// Computes the intersections between two line segments in 
	/// <see cref="SegmentString"/>s and adds them to each string.
	/// </summary>
	/// <remarks>
	/// The ISegmentIntersector is passed to a <see cref="INoder"/>. The 
	/// {@link addIntersections} method is called whenever the <see cref="INoder"/>
	/// detects that two SegmentStrings <i>might</i> intersect.
	/// This class is an example of the <i>Strategy</i> pattern.
	/// </remarks>
	internal interface ISegmentIntersector
	{
		/// <summary> 
		/// This method is called by clients of the <see cref="SegmentIntersector"/> 
		/// interface to process intersections for two segments of the 
		/// <see cref="SegmentString"/>s being intersected.
		/// </summary>
		void ProcessIntersections(SegmentString e0, int segIndex0, 
            SegmentString e1, int segIndex1);
	}
}