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
	/// <summary> Methods for computing and working with octants of the Cartesian plane
	/// Octants are numbered as follows:
	/// <pre>
	/// \2|1/
	/// 3 \|/ 0
	/// ---+--
	/// 4 /|\ 7
	/// /5|6\
	/// <pre>
	/// If line segments lie along a coordinate axis, the octant is the lower of the two
	/// possible values.
	/// 
	/// </summary>
	internal class Octant
	{
        private Octant()
        {
        }
		
		/// <summary> Returns the octant of a directed line segment (specified as x and y
		/// displacements, which cannot both be 0).
		/// </summary>
		public static int GetOctant(double dx, double dy)
		{
			if (dx == 0.0 && dy == 0.0)
				throw new ArgumentException("Cannot compute the octant for point ( " + dx + ", " + dy + " )");
			
			double adx = Math.Abs(dx);
			double ady = Math.Abs(dy);
			
			if (dx >= 0)
			{
				if (dy >= 0)
				{
					if (adx >= ady)
						return 0;
					else
						return 1;
				}
				else
				{
					// dy < 0
					if (adx >= ady)
						return 7;
					else
						return 6;
				}
			}
			else
			{
				// dx < 0
				if (dy >= 0)
				{
					if (adx >= ady)
						return 3;
					else
						return 2;
				}
				else
				{
					// dy < 0
					if (adx >= ady)
						return 4;
					else
						return 5;
				}
			}
		}
		
		/// <summary> Returns the octant of a directed line segment from p0 to p1.</summary>
		public static int GetOctant(Coordinate p0, Coordinate p1)
		{
			double dx = p1.X - p0.X;
			double dy = p1.Y - p0.Y;
			if (dx == 0.0 && dy == 0.0)
			{
				throw new ArgumentException("Cannot compute the octant for two identical points " + p0);
			}

			return GetOctant(dx, dy);
		}
	}
}