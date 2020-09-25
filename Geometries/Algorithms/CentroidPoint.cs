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
	/// <summary> Computes the centroid of a point geometry.
	/// </summary>
	/// <remarks>
	/// Algorithm
	/// <para>
	/// Compute the average of all points.
	/// </para>
	/// </remarks>
	public sealed class CentroidPoint
	{
        private int ptCount;

        private Coordinate centSum;
		
        /// <summary>
        /// Initializes a new instance of the <see cref="CentroidPoint"/> class.
        /// </summary>
        public CentroidPoint()
        {
            centSum = new Coordinate();
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
				cent.X = centSum.X / ptCount;
				cent.Y = centSum.Y / ptCount;
				return cent;
			}
		}

		/// <summary> Adds the point(s) defined by a Geometry to the centroid total.
		/// If the geometry is not of dimension 0 it does not contribute to the centroid.
		/// </summary>
		/// <param name="geometry">the geometry to Add
		/// </param>
		public void Add(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            GeometryType geomType = geometry.GeometryType;

            if (geomType == GeometryType.Point)
			{
				Add(geometry.Coordinate);
			}
			else if (geomType == GeometryType.GeometryCollection ||
                geomType == GeometryType.MultiPoint)
			{
				GeometryCollection gc = (GeometryCollection)geometry;
				for (int i = 0; i < gc.NumGeometries; i++)
				{
					Add(gc.GetGeometry(i));
				}
			}
		}
		
		/// <summary> Adds the length defined by an array of coordinates.</summary>
		/// <param name="pts">an array of Coordinates
		/// </param>
		public void Add(Coordinate pt)
		{
            if (pt == null)
            {
                throw new ArgumentNullException("pt");
            }

            ptCount   += 1;
			centSum.X += pt.X;
			centSum.Y += pt.Y;
		}
	}
}