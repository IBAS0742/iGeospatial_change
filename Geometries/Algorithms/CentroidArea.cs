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

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary> 
	/// Computes the centroid of an area geometry.
	/// </summary>
	/// <remarks>
	/// Algorithm
	/// <para>
	/// Based on the usual algorithm for calculating the centroid as a weighted 
	/// sum of the centroids of a decomposition of the area into (possibly overlapping) 
	/// triangles.
	/// </para>
	/// <para>
	/// The algorithm has been extended to handle holes and multi-polygons.
	/// See <see href="http://www.faqs.org/faqs/graphics/algorithms-faq/">
	/// Graphics Algorithm FAQ</see> for further details of the basic approach.
	/// </para>
	/// </remarks>
	public sealed class CentroidArea
	{
        #region Private Members
		
		private Coordinate basePt; // the point all triangles are based at

		private Coordinate triangleCent3; // temporary variable to hold centroid of triangle

		private double areasum2; /* Partial area sum */

		private Coordinate cg3; // partial centroid sum

        #endregion
		
        /// <summary>
        /// Initializes a new instance of the <see cref="CentroidArea"/> class.
        /// </summary>
        public CentroidArea()
        {
            triangleCent3 = new Coordinate();
            cg3 = new Coordinate();
        }
		
        /// <summary>
        /// Gets the coordinate of the centrod computed.
        /// </summary>
        /// <value>A Coordinate of the centroid point.</value>
		public Coordinate Centroid
		{
			get
			{
				Coordinate cent = new Coordinate();
				cent.X = cg3.X / 3 / areasum2;
				cent.Y = cg3.Y / 3 / areasum2;
				return cent;
			}
		}

		private Coordinate BasePoint
		{
			set
			{
				if (this.basePt == null)
					this.basePt = value;
			}
		}

		/// <summary> 
		/// Adds the area defined by a Geometry to the centroid total.
		/// If the geometry has no area it does not contribute to the centroid.
		/// </summary>
		/// <param name="geometry">The geometry to add</param>
		public void Add(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            GeometryType geomType = geometry.GeometryType;

            if (geomType == GeometryType.Polygon)
			{
				Polygon poly = (Polygon) geometry;
				BasePoint = poly.ExteriorRing.GetCoordinate(0);
				Add(poly);
			}
			else if (geomType == GeometryType.GeometryCollection ||
                geomType == GeometryType.MultiPolygon   ||
                geomType == GeometryType.MultiRectangle ||
                geomType == GeometryType.MultiCircle    ||
                geomType == GeometryType.MultiEllipse)
			{
				GeometryCollection gc = (GeometryCollection) geometry;
                int nCount            = gc.NumGeometries;
				for (int i = 0; i < nCount; i++)
				{
					Add(gc.GetGeometry(i));
				}
			}
		}
		
		/// <summary> 
		/// Adds the area defined by an array of coordinates.  The array must be a ring;
		/// i.e. end with the same coordinate as it starts with.
		/// </summary>
		/// <param name="ring">an array of Coordinates</param>
		public void Add(ICoordinateList ring)
		{
            if (ring == null)
            {
                throw new ArgumentNullException("ring");
            }

            BasePoint = ring[0];
			AddShell(ring);
		}
		
		private void Add(Polygon poly)
		{
			AddShell(poly.ExteriorRing.Coordinates);
            int nCount = poly.NumInteriorRings;
			for (int i = 0; i < nCount; i++)
			{
				AddHole(poly.InteriorRing(i).Coordinates);
			}
		}
		
		private void AddShell(ICoordinateList pts)
		{
			bool isPositiveArea = !CGAlgorithms.IsCCW(pts);
            int nCount          = pts.Count - 1;

			for (int i = 0; i < nCount; i++)
			{
				AddTriangle(basePt, pts[i], pts[i + 1], isPositiveArea);
			}
		}
		
		private void AddHole(ICoordinateList pts)
		{
			bool isPositiveArea = CGAlgorithms.IsCCW(pts);
            int nCount          = pts.Count - 1;

			for (int i = 0; i < nCount; i++)
			{
				AddTriangle(basePt, pts[i], pts[i + 1], isPositiveArea);
			}
		}

		private void AddTriangle(Coordinate p0, Coordinate p1, Coordinate p2, bool isPositiveArea)
		{
			double sign = (isPositiveArea) ? 1.0 : -1.0;

			Centroid3(p0, p1, p2, triangleCent3);
			double area2 = CentroidArea.Area2(p0, p1, p2);
			
            cg3.X += sign * area2 * triangleCent3.X;
			cg3.Y += sign * area2 * triangleCent3.Y;
			
            areasum2 += sign * area2;
		}
		
		/// <summary> Returns three times the centroid of the triangle p1-p2-p3.
		/// The factor of 3 is
		/// left in to permit division to be avoided until later.
		/// </summary>
		private static void Centroid3(Coordinate p1, Coordinate p2, 
            Coordinate p3, Coordinate c)
		{
			c.X = p1.X + p2.X + p3.X;
			c.Y = p1.Y + p2.Y + p3.Y;

			return;
		}
		
		/// <summary> Returns twice the signed area of the triangle p1-p2-p3,
		/// positive if a,b,c are oriented ccw, and negative if cw.
		/// </summary>
		private static double Area2(Coordinate p1, Coordinate p2, Coordinate p3)
		{
			return (p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.X) * (p2.Y - p1.Y);
		}
	}
}