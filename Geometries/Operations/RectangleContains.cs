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
using iGeospatial.Geometries.Operations.Predicate;
	
namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// Optimized implementation of spatial predicate "contains"
	/// for cases where the first <see cref="Geometry"/> is a rectangle.
	/// </summary>
	/// <remarks>
	/// As a further optimization, this class can be used directly 
	/// to test many geometries against a single rectangle.
	/// </remarks>
	[Serializable]
    public class RectangleContains
	{
        #region Private Fields

        private Polygon rectangle;
        private Envelope rectEnv;
        
        #endregion
		
        #region Constructors and Destructor

		/// <summary> 
		/// Create a new contains computer for two geometries.
		/// </summary>
		/// <param name="rectangle">a rectangular geometry
		/// </param>
		public RectangleContains(Polygon rectangle)
		{
            if (rectangle == null)
            {
                throw new ArgumentNullException("rectangle");
            }

            this.rectangle = rectangle;
			rectEnv        = rectangle.Bounds;
		}
        
        #endregion
		
        #region Public Methods

		public bool Contains(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            if (!rectEnv.Contains(geometry.Bounds))
				return false;

			// check that geom is not contained entirely in the 
            // rectangle boundary
			if (IsContainedInBoundary(geometry))
				return false;

			return true;
		}
		
        public static bool Contains(Polygon rectangle, Geometry b)
        {
            RectangleContains rc = new RectangleContains(rectangle);
            if (rc != null)
            {
                return rc.Contains(b);
            }

            return false;
        }
        
        #endregion
		
        #region Private Methods

		private bool IsContainedInBoundary(Geometry geom)
		{
            GeometryType geomType = geom.GeometryType;
			// polygons can never be wholely contained in the boundary
			if (geomType == GeometryType.Polygon)
				return false;
			
            if (geomType == GeometryType.Point)
				return IsPointContainedInBoundary((Point) geom);

			if (geomType == GeometryType.LineString || 
                geomType == GeometryType.LinearRing)
				return IsLineStringContainedInBoundary((LineString)geom);
			
			for (int i = 0; i < geom.NumGeometries; i++)
			{
				Geometry comp = geom.GetGeometry(i);

				if (!IsContainedInBoundary(comp))
					return false;
			}
			return true;
		}
		
		private bool IsPointContainedInBoundary(Point point)
		{
			return IsPointContainedInBoundary(point.Coordinate);
		}
		
		private bool IsPointContainedInBoundary(Coordinate pt)
		{
			// we already know that the point is contained in the rectangle envelope
			
			if (!(pt.X == rectEnv.MinX || pt.X == rectEnv.MaxX))
				return false;
			if (!(pt.Y == rectEnv.MinY || pt.Y == rectEnv.MaxY))
				return false;
			
			return true;
		}
		
		private bool IsLineStringContainedInBoundary(LineString line)
		{
			ICoordinateList seq = line.Coordinates;
            int nCount          = seq.Count;
			for (int i = 0; i < nCount - 1; i++)
			{
				Coordinate p0 = seq[i];
				Coordinate p1 = seq[i + 1];
				
				if (!IsLineSegmentContainedInBoundary(p0, p1))
					return false;
			}

			return true;
		}
		
		private bool IsLineSegmentContainedInBoundary(Coordinate p0, Coordinate p1)
		{
			if (p0.Equals(p1))
				return IsPointContainedInBoundary(p0);
			
			// we already know that the segment is contained in the rectangle envelope
			if (p0.X == p1.X)
			{
				if (p0.X == rectEnv.MinX || p0.X == rectEnv.MaxX)
					return true;
			}
			else if (p0.Y == p1.Y)
			{
				if (p0.Y == rectEnv.MinY || p0.Y == rectEnv.MaxY)
					return true;
			}

			// Either
			// both x and y values are different
			// or
			// one of x and y are the same, but the other ordinate is not the 
            // same as a boundary ordinate
			// 
			// In either case, the segment is not wholely in the boundary
			return false;
		}
        
        #endregion
	}
}