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
using System.Collections;

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Noding
{
	
	/// <summary> 
	/// Allows comparing <see cref="Coordinate"/> arrays in an 
	/// orientation-independent way.
	/// </summary>
	[Serializable]
    internal class OrientedCoordinateArray : IComparable
	{
		private ICoordinateList pts;
		private bool orientation;
		
		/// <summary> Creates a new {@link OrientedCoordinateArray}
		/// for the given <see cref="Coordinate"/> array.
		/// 
		/// </summary>
		/// <param name="pts">the coordinates to orient
		/// </param>
		public OrientedCoordinateArray(ICoordinateList pts)
		{
			this.pts    = pts;
			orientation = GetOrientation(pts);
		}
		
		/// <summary> Computes the canonical orientation for a coordinate array.
		/// 
		/// </summary>
		/// <param name="pts">the array to test
		/// </param>
		/// <returns> <see langword="true"/> if the points are oriented forwards
		/// </returns>
		/// <returns> <code>false</code if the points are oriented in reverse
		/// </returns>
		private static bool GetOrientation(ICoordinateList pts)
		{
			return IncreasingDirection(pts) == 1;
		}
		
		/// <summary> 
		/// Compares two {@link OrientedCoordinateArray}s for their relative order
		/// 
		/// </summary>
		/// <returns> -1 this one is smaller
		/// </returns>
		/// <returns> 0 the two objects are equal
		/// </returns>
		/// <returns> 1 this one is greater
		/// </returns>
		public int CompareTo(System.Object o1)
		{
			OrientedCoordinateArray oca = (OrientedCoordinateArray) o1;
			int comp = CompareOriented(pts, orientation, 
                oca.pts, oca.orientation);

			return comp;
		}
		
		private static int CompareOriented(ICoordinateList pts1, 
            bool orientation1, ICoordinateList pts2, bool orientation2)
		{
			int dir1   = orientation1 ? 1 :- 1;
			int dir2   = orientation2 ? 1 :- 1;
			int limit1 = orientation1 ? pts1.Count : -1;
			int limit2 = orientation2 ? pts2.Count : -1;
			
			int i1 = orientation1 ? 0 : pts1.Count - 1;
			int i2 = orientation2 ? 0 : pts2.Count - 1;
//			int comp = 0;
			while (true)
			{
				int compPt = pts1[i1].CompareTo(pts2[i2]);
				if (compPt != 0)
					return compPt;
				i1 += dir1;
				i2 += dir2;
				bool done1 = i1 == limit1;
				bool done2 = i2 == limit2;
				if (done1 && !done2)
					return - 1;
				if (!done1 && done2)
					return 1;
				if (done1 && done2)
					return 0;
			}
		}
		
        /// <summary> Determines which orientation of the {@link Coordinate} array
        /// is (overall) increasing.
        /// In other words, determines which end of the array is "smaller"
        /// (using the standard ordering on {@link Coordinate}).
        /// Returns an integer indicating the increasing direction.
        /// If the sequence is a palindrome, it is defined to be
        /// oriented in a positive direction.
        /// 
        /// </summary>
        /// <param name="pts">the array of Coordinates to test
        /// </param>
        /// <returns> <code>1</code> if the array is smaller at the start
        /// or is a palindrome,
        /// <code>-1</code> if smaller at the end
        /// </returns>
        private static int IncreasingDirection(ICoordinateList pts)
        {
            int nCount = pts.Count;

            for (int i = 0; i < nCount / 2; i++)
            {
                int j = nCount - 1 - i;
                // skip equal points on both ends
                int comp = pts[i].CompareTo(pts[j]);
                if (comp != 0)
                    return comp;
            }

            // array must be a palindrome - defined to be in positive direction
            return 1;
        }
    }
}