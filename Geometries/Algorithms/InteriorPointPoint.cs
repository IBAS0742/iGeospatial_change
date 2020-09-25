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
	/// Computes a point in the interior of an point geometry.
	/// </summary>
	/// <remarks>
	/// Algorithm
	/// <para>
	/// Find a point which is closest to the centroid of the geometry.
	/// </para>
	/// </remarks>
	public sealed class InteriorPointPoint
	{
        #region Private Members
        
        private Coordinate centroid;

        private double minDistance;
		
        private Coordinate interiorPoint;
		
        #endregion

        #region Constructors and Destructor
        
        /// <summary>
        /// Initializes a new instance of the <see cref="InteriorPointPoint"/> class.
        /// </summary>
        /// <param name="g">
        /// The point <see cref="Geometry"/> to determine the interior point of.
        /// </param>
        public InteriorPointPoint(Geometry g)
        {
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }

            minDistance = System.Double.MaxValue;
            centroid    = g.Centroid.Coordinate;
            Add(g);
        }
		
        #endregion

        /// <summary>
        /// Gets a coordinate or point in the interior of a point geometry.
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
		/// Tests the point(s) defined by a Geometry for the best inside point.
		/// If a Geometry is not of dimension 0 it is not tested.
		/// </summary>
		/// <param name="geom">The geometry to add.</param>
		private void  Add(Geometry geom)
		{
            GeometryType geomType = geom.GeometryType;

            if (geomType == GeometryType.Point)
			{
				Add(geom.Coordinate);
			}
			else if (geomType == GeometryType.GeometryCollection ||
                geomType == GeometryType.MultiPoint)
			{
				GeometryCollection gc = (GeometryCollection) geom;
				for (int i = 0; i < gc.NumGeometries; i++)
				{
					Add(gc.GetGeometry(i));
				}
			}
		}

		private void  Add(Coordinate point)
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