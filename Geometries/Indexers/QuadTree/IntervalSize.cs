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

namespace iGeospatial.Geometries.Indexers.QuadTree
{
	/// <summary> 
	/// Provides a test for whether an interval is
	/// so small it should be considered as zero for the purposes of
	/// inserting it into a binary tree.
	/// The reason this check is necessary is that round-off error can
	/// cause the algorithm used to subdivide an interval to fail, by
	/// computing a midpoint value which does not lie strictly between the
	/// endpoints.
	/// 
	/// </summary>
	[Serializable]
    internal sealed class IntervalSize
	{
        private IntervalSize()
        {
        }

		/// <summary> 
		/// This value is chosen to be a few powers of 2 less than the
		/// number of bits available in the double representation (i.e. 53).
		/// This should allow enough extra precision for simple computations to be correct,
		/// at least for comparison purposes.
		/// </summary>
		public const int MinBinaryExponent = -50;
		
		/// <summary> Computes whether the interval [min, max] is effectively zero width.
		/// I.e. the width of the interval is so much less than the
		/// location of the interval that the midpoint of the interval cannot be
		/// represented precisely.
		/// </summary>
		public static bool IsZeroWidth(double min, double max)
		{
			double width = max - min;
			if (width == 0.0)
				return true;
			
			double maxAbs = Math.Max(Math.Abs(min), Math.Abs(max));
			double scaledInterval = width / maxAbs;
			int level = DoubleBits.Exponent(scaledInterval);

			return level <= MinBinaryExponent;
		}
	}
}