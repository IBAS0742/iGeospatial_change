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
using iGeospatial.Geometries.Graphs;

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary> 
	/// Computes the topological relationship (<see cref="Location"/>)
	/// of a single point to a <see cref="Geometry"/>.
	/// </summary>
	/// <remarks>
	/// The algorithm obeys the SFS Boundary Determination Rule to determine 
	/// whether the point lies on the boundary or not.
	/// <para>
	/// <see cref="LinearRing"/>s do not enclose any area - points inside 
	/// the ring are still in the EXTERIOR of the ring.
	/// </para>
	/// Note that instances of this class are not reentrant.
	/// </remarks>
	internal sealed class PointLocator
	{
		private bool isIn; // true if the point lies in or on any Geometry element
		private int numBoundaries; // the number of sub-elements whose boundaries the point lies in
		
		public PointLocator()
		{
        }
		
		/// <summary> 
		/// Convenience method to test a point for intersection with
		/// a <see cref="Geometry"/>.
		/// </summary>
		/// <param name="p">The coordinate to test.</param>
		/// <param name="geom">The <see cref="Geometry"/> to test.</param>
		/// <returns> 
		/// true, if the point is in the interior or boundary of 
		/// the <see cref="Geometry"/>, otherwise false.
		/// </returns>
		public bool Intersects(Coordinate p, Geometry geom)
		{
			return Locate(p, geom) != LocationType.Exterior;
		}
		
		/// <summary> 
		/// Computes the topological relationship (<see cref="LocationType"/>) 
		/// of a single point to a <see cref="Geometry"/>.
		/// </summary>
		/// <returns> 
		/// The <see cref="LocationType"/> of the point relative to the input Geometry
		/// </returns>
		/// <remarks>
		/// It handles both single-element and multi-element geometries.
		/// The algorithm for multi-part geometries takes into account 
		/// the Boundary Determination rule.
		/// </remarks>
		public int Locate(Coordinate p, Geometry geom)
		{
			if (geom.IsEmpty)
				return LocationType.Exterior;
			
            GeometryType geomType = geom.GeometryType;

            if (geomType == GeometryType.LineString)
			{
				return Locate(p, (LineString)geom);
			}
			else if (geomType == GeometryType.LinearRing)
			{
				return Locate(p, (LineString)geom);
			}
			else if (geomType == GeometryType.Polygon)
			{
				return Locate(p, (Polygon)geom);
			}
			
			isIn = false;
			numBoundaries = 0;
			ComputeLocation(p, geom);

			if (GeometryGraph.IsInBoundary(numBoundaries))
				return LocationType.Boundary;

			if (numBoundaries > 0 || isIn)
				return LocationType.Interior;

			return LocationType.Exterior;
		}
		
		private void ComputeLocation(Coordinate p, Geometry geom)
		{
            GeometryType geomType = geom.GeometryType;

            if (geomType == GeometryType.LineString)
			{
				UpdateLocationInfo(Locate(p, (LineString)geom));
			}
			else if (geomType == GeometryType.LinearRing)
			{
				UpdateLocationInfo(Locate(p, (LineString)geom));
			}
			else if (geomType == GeometryType.Polygon)
			{
				UpdateLocationInfo(Locate(p, (Polygon)geom));
			}
			else if (geomType == GeometryType.MultiLineString)
			{
				MultiLineString ml = (MultiLineString)geom;
                int nCount = ml.NumGeometries;
				for (int i = 0; i < nCount; i++)
				{
					LineString l = ml[i];
					UpdateLocationInfo(Locate(p, l));
				}
			}
			else if (geomType == GeometryType.MultiPolygon)
			{
				MultiPolygon mpoly = (MultiPolygon)geom;
                int nCount = mpoly.NumGeometries;
				for (int i = 0; i < nCount; i++)
				{
					Polygon poly = mpoly[i];
					UpdateLocationInfo(Locate(p, poly));
				}
			}
			else if (geomType == GeometryType.GeometryCollection)
			{
				IGeometryEnumerator geomi = new GeometryIterator((GeometryCollection)geom);

                while (geomi.MoveNext())
				{
					Geometry g2 = geomi.Current;
					if (g2 != geom)
                    {
                        ComputeLocation(p, g2);
                    }
				}
			}
		}
		
		private void UpdateLocationInfo(int loc)
		{
			if (loc == LocationType.Interior)
				isIn = true;

			if (loc == LocationType.Boundary)
				numBoundaries++;
		}
		
		private int Locate(Coordinate p, LineString l)
		{
			ICoordinateList pt = l.Coordinates;
			if (!l.IsClosed)
			{
				if (p.Equals(pt[0]) || p.Equals(pt[pt.Count - 1]))
				{
					return LocationType.Boundary;
				}
			}

			if (CGAlgorithms.IsOnLine(p, pt))
				return LocationType.Interior;
			
            return LocationType.Exterior;
		}
		
		private int Locate(Coordinate p, LinearRing ring)
		{
			// can this test be folded into IsPointInRing ?
			if (CGAlgorithms.IsOnLine(p, ring.Coordinates))
			{
				return LocationType.Boundary;
			}
			
            if (CGAlgorithms.IsPointInRing(p, ring.Coordinates))
				return LocationType.Interior;

			return LocationType.Exterior;
		}
		
		private int Locate(Coordinate p, Polygon poly)
		{
			if (poly.IsEmpty)
				return LocationType.Exterior;
			LinearRing shell = poly.ExteriorRing;
			
			int shellLoc = Locate(p, shell);
			if (shellLoc == LocationType.Exterior)
				return LocationType.Exterior;
			if (shellLoc == LocationType.Boundary)
				return LocationType.Boundary;

			// now test if the point lies in or on the holes
			for (int i = 0; i < poly.NumInteriorRings; i++)
			{
				LinearRing hole = poly.InteriorRing(i);
				int holeLoc = Locate(p, hole);
				if (holeLoc == LocationType.Interior)
					return LocationType.Exterior;
				if (holeLoc == LocationType.Boundary)
					return LocationType.Boundary;
			}

			return LocationType.Interior;
		}
	}
}