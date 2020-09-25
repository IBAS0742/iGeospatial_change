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
	/// Computes the centroid of a linear geometry.
	/// </summary>
	/// <remarks>
	/// Algorithm
	/// <para>
	/// Compute the average of the midpoints of all line segments weighted by the 
	/// segment length.
	/// </para>
	/// </remarks>
	public sealed class CentroidLine
	{
        private Coordinate centSum;
        private double totalLength;
		
        /// <summary>
        /// Initializes a new instance of the <see cref="CentroidLine"/> class.
        /// </summary>
        public CentroidLine()
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
				cent.X = centSum.X / totalLength;
				cent.Y = centSum.Y / totalLength;
				return cent;
			}
			
		}
		
		/// <summary> 
		/// Adds the linestring(s) defined by a Geometry to the centroid total.
		/// If the geometry is not linear it does not contribute to the centroid
		/// </summary>
		/// <param name="geometry">The geometry to add.</param>
		public void Add(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            GeometryType geomType = geometry.GeometryType;

            if (geomType == GeometryType.LineString ||
                geomType == GeometryType.LinearRing)
			{
				Add(geometry.Coordinates);
			}
			else if (geomType == GeometryType.GeometryCollection ||
                geomType == GeometryType.MultiLineString)
			{
				GeometryCollection gc = (GeometryCollection)geometry;
				for (int i = 0; i < gc.NumGeometries; i++)
				{
					Add(gc.GetGeometry(i));
				}
			}
		}
		
		/// <summary> Adds the length defined by an array of coordinates.</summary>
		/// <param name="points">An array of Coordinates.</param>
		public void Add(ICoordinateList points)
		{
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            int nCount = points.Count - 1;
			for (int i = 0; i < nCount; i++)
			{
				double segmentLen = points[i].Distance(points[i + 1]);
				totalLength += segmentLen;
				
				double midx = (points[i].X + points[i + 1].X) / 2;
				centSum.X += segmentLen * midx;

				double midy = (points[i].Y + points[i + 1].Y) / 2;
				centSum.Y += segmentLen * midy;
			}
		}
	}
}