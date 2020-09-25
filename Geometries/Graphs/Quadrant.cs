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

using iGeospatial.Geometries;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Graphs
{
	
	/// <summary> 
	/// Utility functions for working with quadrants, which are numbered as follows:
	/// <code>
	/// 1 | 0
	/// --+--
	/// 2 | 3
	/// </code>
	/// </summary>
	internal sealed class Quadrant
	{
        private Quadrant()
        {
        }

		/// <summary> 
		/// Returns the quadrant of a directed line segment (specified as x and y
		/// displacements, which cannot both be 0).
		/// </summary>
		public static int GetQuadrant(double dx, double dy)
		{
			if (dx == 0.0 && dy == 0.0)
            {
                throw new System.ArgumentException
                    ("Cannot compute the quadrant for point ( " + dx + ", " + dy + " )");
            }

			if (dx >= 0)
			{
				if (dy >= 0)
					return 0;
				else
					return 3;
			}
			else
			{
				if (dy >= 0)
					return 1;
				else
					return 2;
			}
		}
		
		/// <summary> 
		/// Returns the quadrant of a directed line segment from p0 to p1.
		/// </summary>
		public static int GetQuadrant(Coordinate p0, Coordinate p1)
		{
			double dx = p1.X - p0.X;
			double dy = p1.Y - p0.Y;
			if (dx == 0.0 && dy == 0.0)
			{
				throw new System.ArgumentException("Cannot compute the quadrant for two identical points " + p0);
			}

			return GetQuadrant(dx, dy);
		}
		
		/// <summary> 
		/// Returns true if the quadrants are 1 and 3, or 2 and 4.
		/// </summary>
		public static bool IsOpposite(int quad1, int quad2)
		{
			if (quad1 == quad2)
				return false;
			int diff = (quad1 - quad2 + 4) % 4;
			// if quadrants are not adjacent, they are opposite
			if (diff == 2)
				return true;

			return false;
		}
		
		/// <summary> 
		/// Returns the right-hand quadrant of the halfplane defined by the two quadrants,
		/// or -1 if the quadrants are opposite, or the quadrant if they are identical.
		/// </summary>
		public static int CommonHalfPlane(int quad1, int quad2)
		{
			// if quadrants are the same they do not determine a unique common halfplane.
			// Simply return one of the two possibilities
			if (quad1 == quad2)
				return quad1;

			int diff = (quad1 - quad2 + 4) % 4;
			// if quadrants are not adjacent, they do not share a common halfplane
			if (diff == 2)
				return -1;
			
			int min = (quad1 < quad2) ? quad1 : quad2;
			int max = (quad1 > quad2) ? quad1 : quad2;

			// for this one case, the righthand plane is NOT the minimum index;
			if (min == 0 && max == 3)
				return 3;

			// in general, the halfplane index is the minimum of the two adjacent quadrants
			return min;
		}
		
		/// <summary> 
		/// Returns whether the given quadrant lies Within the given halfplane 
		/// (specified by its right-hand quadrant).
		/// </summary>
		public static bool IsInHalfPlane(int quad, int halfPlane)
		{
			if (halfPlane == 3)
			{
				return quad == 3 || quad == 0;
			}
			return quad == halfPlane || quad == halfPlane + 1;
		}
		
		/// <summary> 
		/// Returns true if the given quadrant is 0 or 1.
		/// </summary>
		public static bool IsNorthern(int quad)
		{
			return quad == 0 || quad == 1;
		}
	}
}