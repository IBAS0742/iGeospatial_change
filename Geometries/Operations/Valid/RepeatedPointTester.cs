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

namespace iGeospatial.Geometries.Operations.Valid
{
	/// <summary> Implements the appropriate checks for repeated points
	/// (consecutive identical coordinates) as defined in the OTS spec.
	/// </summary>
	internal class RepeatedPointTester
	{
        #region Private Fields

		// save the repeated coord found (if any)
		private Coordinate repeatedCoord;
        
        #endregion
		
        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="RepeatedPointTester"/> class.
        /// </summary>
		public RepeatedPointTester()
		{
		}

        #endregion
		
        #region Public Properties

		public Coordinate Coordinate
		{
			get
			{
				return repeatedCoord;
			}
		}
        
        #endregion
		
        #region Public Methods

		public bool HasRepeatedPoint(Geometry g)
		{
			if (g.IsEmpty)
				return false;

            GeometryType geomType = g.GeometryType;

            if (geomType == GeometryType.Point)
				return false;
			else if (geomType == GeometryType.MultiPoint)
				return false;
			// LineString also handles LinearRings
			else if (geomType == GeometryType.LineString ||
                geomType == GeometryType.LinearRing)
				return HasRepeatedPoint(((LineString) g).Coordinates);
			else if (geomType == GeometryType.Polygon)
				return HasRepeatedPoint((Polygon) g);
			else if (geomType == GeometryType.GeometryCollection)
				return HasRepeatedPoint((GeometryCollection) g);
			else
				throw new System.NotSupportedException(g.GetType().FullName);
		}
		
		public bool HasRepeatedPoint(ICoordinateList coord)
		{
            int nCount = coord.Count;
			for (int i = 1; i < nCount; i++)
			{
				if (coord[i - 1].Equals(coord[i]))
				{
					repeatedCoord = coord[i];
					return true;
				}
			}
			return false;
		}
        
        #endregion

        #region Private Methods

		private bool HasRepeatedPoint(Polygon p)
		{
			if (HasRepeatedPoint(p.ExteriorRing.Coordinates))
				return true;
			for (int i = 0; i < p.NumInteriorRings; i++)
			{
				if (HasRepeatedPoint(p.InteriorRing(i).Coordinates))
					return true;
			}
			return false;
		}

		private bool HasRepeatedPoint(GeometryCollection gc)
		{
			for (int i = 0; i < gc.NumGeometries; i++)
			{
				Geometry g = gc.GetGeometry(i);
				if (HasRepeatedPoint(g))
					return true;
			}
			return false;
		}
        
        #endregion
	}
}