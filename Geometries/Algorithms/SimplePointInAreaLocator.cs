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

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary> 
	/// Computes whether a point lies in the interior of an 
	/// area <see cref="Geometry"/> or a <see cref="Surface"/>.
	/// </summary>
	/// <remarks>
	/// The algorithm used is only guaranteed to return correct results
	/// for points which are not on the boundary of the <see cref="Geometry"/>.
	/// </remarks>
	public sealed class SimplePointInAreaLocator
	{
        private SimplePointInAreaLocator()
        {
        }

		/// <summary> 
		/// Locate is the main location function.  It handles both single-element
		/// and multi-element Geometries.  The algorithm for multi-element Geometries
		/// is more complex, since it has to take into account the boundaryDetermination rule
		/// </summary>
		public static int Locate(Coordinate p, Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            if (geometry.IsEmpty)
				return LocationType.Exterior;
			
			if (ContainsPoint(p, geometry))
				return LocationType.Interior;

			return LocationType.Exterior;
		}
		
		private static bool ContainsPoint(Coordinate p, Geometry geom)
		{
            GeometryType geomType = geom.GeometryType;

			if (geomType == GeometryType.Polygon)
			{
				return ContainsPointInPolygon(p, (Polygon) geom);
			}
            else if(geomType == GeometryType.MultiPolygon)
            {
                MultiPolygon mpoly = (MultiPolygon)geom;
                int nCount = mpoly.NumGeometries;
                for (int i = 0; i < nCount; i++)
                {
                    Polygon poly = mpoly[i];
                    if (poly != geom)
                    {
                        if (ContainsPoint(p, poly))
                        {
                            return true;
                        }
                    }
                }
            }
			else 
			{
                if (geom.IsCollection)
                {
                    IGeometryEnumerator geomi = new GeometryIterator((GeometryCollection) geom);

                    while (geomi.MoveNext())
                    {
                        Geometry g2 = geomi.Current;
                        if (g2 != geom)
                        {
                            if (ContainsPoint(p, g2))
                            {
                                return true;
                            }
                        }
                    }
                }
			}

			return false;
		}
		
		public static bool ContainsPointInPolygon(Coordinate p, Polygon polygon)
		{
            if (polygon == null)
            {
                throw new ArgumentNullException("polygon");
            }

            if (polygon.IsEmpty)
				return false;

			LinearRing shell = (LinearRing) polygon.ExteriorRing;
			if (!CGAlgorithms.IsPointInRing(p, shell.Coordinates))
				return false;

			// now test if the point lies in or on the holes
			for (int i = 0; i < polygon.NumInteriorRings; i++)
			{
				LinearRing hole = (LinearRing) polygon.InteriorRing(i);
				if (CGAlgorithms.IsPointInRing(p, hole.Coordinates))
					return false;
			}

			return true;
		}
	}
}