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

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Editors
{
    /// <summary> 
    /// A <see cref="IGeometryEdit"/> which modifies the coordinate list of a 
    /// <see cref="Geometry"/>. Operates on Geometry subclasses which 
    /// <c>Geometry.Contains</c> a single coordinate list.
    /// </summary>
    public abstract class CoordinateGeometryEdit : IGeometryEdit
    {
        public virtual Geometry Edit(Geometry geometry, GeometryFactory factory)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            GeometryType geomType = geometry.GeometryType;

            if (geomType == GeometryType.LinearRing)
            {
                return factory.CreateLinearRing(Edit(geometry.Coordinates, 
                    geometry));
            }
			
            if (geomType == GeometryType.LineString)
            {
                return factory.CreateLineString(Edit(geometry.Coordinates, 
                    geometry));
            }
			
            if (geomType == GeometryType.Point)
            {
                ICoordinateList newCoordinates = Edit(geometry.Coordinates, 
                    geometry);
				
                return factory.CreatePoint((newCoordinates.Count > 0) ? 
                    newCoordinates[0] : null);
            }
			
            return geometry;
        }
		
        /// <summary> 
        /// Edits the array of Coordinates from a Geometry.
        /// </summary>
        /// <param name="coordinates">the coordinate array to operate on
        /// </param>
        /// <param name="geometry">the geometry containing the coordinate list
        /// </param>
        /// <returns> an edited coordinate array (which may be the same as the input)
        /// </returns>
        public abstract ICoordinateList Edit(ICoordinateList coordinates, 
            Geometry geometry);
    }
}
