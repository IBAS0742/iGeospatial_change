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
using System.Diagnostics;
using System.Collections;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Noding
{
	/// <summary> 
	/// Implements a robust method of comparing the relative position of two
	/// points along the same segment.
	/// </summary>
	/// <remarks>
	/// The coordinates are assumed to lie "near" the segment.
	/// This means that this algorithm will only return correct results
	/// if the input coordinates
	/// have the same precision and correspond to rounded values
	/// of exact coordinates lying on the segment.
	/// </remarks>
	internal class SegmentPointComparator
	{
		/// <summary> Compares two <see cref="Coordinate"/>s for their relative position along a segment
		/// lying in the specified {@link Octant}.
		/// 
		/// </summary>
		/// <returns> -1 node0 occurs first
		/// </returns>
		/// <returns> 0 the two nodes are equal
		/// </returns>
		/// <returns> 1 node1 occurs first
		/// </returns>
		public static int Compare(int octant, Coordinate p0, Coordinate p1)
		{
			// nodes can only be equal if their coordinates are equal
			if (p0.Equals(p1))
				return 0;
			
			int xSign = RelativeSign(p0.X, p1.X);
			int ySign = RelativeSign(p0.Y, p1.Y);
			
			switch (octant)
			{
				
				case 0:  
                    return CompareValue(xSign, ySign);
				
				case 1:  
                    return CompareValue(ySign, xSign);
				
				case 2:  
                    return CompareValue(ySign, - xSign);
				
				case 3:  
                    return CompareValue(- xSign, ySign);
				
				case 4:  
                    return CompareValue(- xSign, - ySign);
				
				case 5:  
                    return CompareValue(- ySign, - xSign);
				
				case 6:  
                    return CompareValue(- ySign, xSign);
				
				case 7:  
                    return CompareValue(xSign, - ySign);
			}

			Debug.Assert(false, "invalid octant value");

			return 0;
		}
		
		public static int RelativeSign(double x0, double x1)
		{
			if (x0 < x1)
				return - 1;
			if (x0 > x1)
				return 1;
			return 0;
		}
		
		private static int CompareValue(int compareSign0, int compareSign1)
		{
			if (compareSign0 < 0)
				return - 1;
			if (compareSign0 > 0)
				return 1;
			if (compareSign1 < 0)
				return - 1;
			if (compareSign1 > 0)
				return 1;
			return 0;
		}
	}
}