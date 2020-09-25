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
using iGeospatial.Geometries.Visitors;

namespace iGeospatial.Geometries.Operations.Distance
{
	/// <summary> 
	/// Extracts a single point from each connected element in a Geometry
	/// (e.g. a polygon, linestring or point) and returns them in a list
	/// </summary>
	internal class ConnectedElementPointFilter : IGeometryVisitor
	{
        #region Private Fields

        private ICoordinateList pts;
        
        #endregion
		
        #region Constructors and Destructor

        public ConnectedElementPointFilter(ICoordinateList pts)
        {
            this.pts = pts;
        }

        #endregion
		
        #region Public Methods

		/// <summary> 
		/// Returns a list containing a Coordinate from each Polygon, LineString, and Point
		/// found inside the specified geometry. Thus, if the specified geometry is
		/// not a GeometryCollection, an empty list will be returned.
		/// </summary>
		public static ICoordinateList GetCoordinates(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            ICoordinateList pts = new CoordinateCollection();
			geometry.Apply(new ConnectedElementPointFilter(pts));

			return pts;
		}
        
        #endregion
		
        #region IGeometryVisitor Members

		public void Visit(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            GeometryType geomType = geometry.GeometryType;

            if (geomType == GeometryType.Point      || 
                geomType == GeometryType.LineString ||
                geomType == GeometryType.LinearRing ||
                geomType == GeometryType.Polygon)
			{
				pts.Add(geometry.Coordinate);
			}
		}
        
        #endregion
	}
}