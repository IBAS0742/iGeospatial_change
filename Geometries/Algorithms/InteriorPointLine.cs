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
	/// Computes a point in the interior of an linear geometry.
	/// </summary>
	/// <remarks>
	/// Algorithm
	/// <list type="number">
	/// <item>
	/// <description>
	/// Find an interior vertex which is closest to the centroid of the linestring.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// If there is no interior vertex, find the endpoint which is closest to the centroid.
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	public sealed class InteriorPointLine
	{
        #region Private Members

        private Coordinate centroid;

        private double minDistance;
		
        private Coordinate interiorPoint;

        #endregion
		
        /// <summary>
        /// Initializes a new instance of the <see cref="InteriorPointLine"/> class.
        /// </summary>
        /// <param name="geometry">The linear geometry to be examined.</param>
        public InteriorPointLine(Geometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            minDistance = Double.MaxValue;
            centroid = geometry.Centroid.Coordinate;
            AddInterior(geometry);

            if (interiorPoint == null)
            {
                AddEndpoints(geometry);
            }
        }
		
        /// <summary>
        /// Gets a coordinate or point in the interior of a linear geometry.
        /// </summary>
        /// <value>
        /// The <see cref="iGeospatial.Coordinates.Coordinate"/> of an interior point
        /// of the geometry.
        /// </value>
        public Coordinate InteriorPoint
		{
			get
			{
				return interiorPoint;
			}
		}
		
		/// <summary> 
		/// Tests the interior vertices (if any) defined by a linear 
		/// <see cref="Geometry"/> for the best inside point.
		/// If a Geometry is not of dimension 1 it is not tested.
		/// </summary>
		/// <param name="geom">The geometry to add.</param>
		private void AddInterior(Geometry geom)
		{
            GeometryType geomType = geom.GeometryType;

            if (geomType == GeometryType.LineString ||
                geomType == GeometryType.LinearRing)
			{
				AddInterior(geom.Coordinates);
			}
			else if (geomType == GeometryType.GeometryCollection ||
                geomType == GeometryType.MultiLineString)
			{
				GeometryCollection gc = (GeometryCollection) geom;
				for (int i = 0; i < gc.NumGeometries; i++)
				{
					AddInterior(gc.GetGeometry(i));
				}
			}
		}

		private void AddInterior(ICoordinateList pts)
		{
			for (int i = 1; i < pts.Count - 1; i++)
			{
				Add(pts[i]);
			}
		}

		/// <summary> 
		/// Tests the endpoint vertices defined by a linear <see cref="Geometry"/> 
		/// for the best inside point.
		/// If a Geometry is not of dimension 1 it is not tested.
		/// </summary>
		/// <param name="geom">The geometry to add.</param>
		private void AddEndpoints(Geometry geom)
		{
            GeometryType geomType = geom.GeometryType;

            if (geomType == GeometryType.LineString ||
                geomType == GeometryType.LinearRing)
			{
				AddEndpoints(geom.Coordinates);
			}
			else if (geomType == GeometryType.GeometryCollection ||
                geomType == GeometryType.MultiLineString)
			{
				GeometryCollection gc = (GeometryCollection) geom;
				for (int i = 0; i < gc.NumGeometries; i++)
				{
					AddEndpoints(gc.GetGeometry(i));
				}
			}
		}

		private void AddEndpoints(ICoordinateList pts)
		{
			Add(pts[0]);
			Add(pts[pts.Count - 1]);
		}
		
		private void Add(Coordinate point)
		{
			double dist = point.Distance(centroid);
			if (dist < minDistance)
			{
				interiorPoint = new Coordinate(point);
				minDistance = dist;
			}
		}
	}
}