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

namespace iGeospatial.Geometries.Algorithms
{
    /// <summary> 
	/// Represents a homogeneous coordinate for 2-D coordinate space.
	/// </summary>
	/// <remarks>
    /// In this library, the HCoordinate are used as a clean way
    /// of computing intersections between line segments.
	/// </remarks>
	internal class HCoordinate
	{
        private double x, y, w;
		
        public HCoordinate()
        {
            w = 1.0;
        }
		
        public HCoordinate(double _x, double _y, double _w)
        {
            x = _x;
            y = _y;
            w = _w;
        }
		
        public HCoordinate(double _x, double _y)
        {
            x = _x;
            y = _y;
            w = 1.0;
        }
		
        public HCoordinate(Coordinate p)
        {
            x = p.X;
            y = p.Y;
            w = 1.0;
        }
		
        public HCoordinate(HCoordinate p1, HCoordinate p2)
        {
            x = p1.y * p2.w - p2.y * p1.w;
            y = p2.x * p1.w - p1.x * p2.w;
            w = p1.x * p2.y - p2.x * p1.y;
        }
		
		public double X
		{
			get
			{
				double a = x / w;
				if ((System.Double.IsNaN(a)) || (System.Double.IsInfinity(a)))
				{
					throw new AlgorithmException("Projective point cannot be represented on the Cartesian plane.");
				}

				return a;
			}
		}

		public double Y
		{
			get
			{
				double a = y / w;
				if ((System.Double.IsNaN(a)) || (System.Double.IsInfinity(a)))
				{
					throw new AlgorithmException("Projective point cannot be represented on the Cartesian plane.");
				}

				return a;
			}
		}

		public Coordinate Coordinate
		{
			get
			{
				Coordinate p = new Coordinate();
				p.X = X;
				p.Y = Y;
			
				return p;
			}
		}
		
		/// <summary> 
		/// Computes the (approximate) intersection point between two line segments
		/// using homogeneous coordinates.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Note that this algorithm is
		/// not numerically stable; i.e. it can produce intersection points which
		/// lie outside the envelope of the line segments themselves.  In order
		/// to increase the precision of the calculation input points should be normalized
		/// before passing them to this routine.
		/// </para>
		/// </remarks>
		public static Coordinate Intersection(Coordinate p1, Coordinate p2, 
            Coordinate q1, Coordinate q2)
		{
			HCoordinate l1        = new HCoordinate(new HCoordinate(p1), new HCoordinate(p2));
			HCoordinate l2        = new HCoordinate(new HCoordinate(q1), new HCoordinate(q2));
			HCoordinate intHCoord = new HCoordinate(l1, l2);
			Coordinate intPt      = intHCoord.Coordinate;

			return intPt;
		}
	}
}