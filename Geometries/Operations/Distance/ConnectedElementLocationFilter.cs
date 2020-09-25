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
	/// A ConnectedElementPointFilter extracts a single point
	/// from each connected element in a Geometry
	/// (e.g. a polygon, linestring or point)
	/// and returns them in a list. The elements of the list are 
	/// <see cref="DistanceLocation"/>s.
	/// </summary>
	[Serializable]
    internal class ConnectedElementLocationFilter : IGeometryVisitor
	{
        #region Private Fields

        private IList locations;
        
        #endregion
		
        #region Constructors and Destructor

        public ConnectedElementLocationFilter(IList locations)
        {
            this.locations = locations;
        }

        #endregion
		
        #region Public Methods

		/// <summary> Returns a list containing a point from each Polygon, LineString, and Point
		/// found inside the specified geometry. Thus, if the specified geometry is
		/// not a GeometryCollection, an empty list will be returned. The elements of the list 
		/// are {@link iGeospatial.Geometries.Operations.Distance.DistanceLocation}s.
		/// </summary>
		public static IList GetLocations(Geometry geom)
		{
			ArrayList locations = new ArrayList();
			geom.Apply(new ConnectedElementLocationFilter(locations));

			return locations;
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
				locations.Add(new DistanceLocation(geometry, 0, 
                    geometry.Coordinate));
			}
		}
        
        #endregion
	}
}