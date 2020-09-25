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

namespace iGeospatial.Geometries.Visitors
{
	/// <summary> 
	/// Extracts all the 2-dimensional (<see cref="Polygon"/>) components from a 
	/// <see cref="Geometry"/>.
	/// </summary>
	[Serializable]
    public class PolygonExtracter : IGeometryVisitor
	{
        private IGeometryList comps;

		/// <summary> 
		/// Constructs a PolygonExtracter filter with a list in which to store 
		/// <see cref="Polygon"/> class instances found.
		/// </summary>
		public PolygonExtracter(IGeometryList comps)
		{
			this.comps = comps;
		}
		
		/// <summary> 
		/// Returns the <see cref="Polygon"/> components from a single geometry.
		/// If more than one geometry is to be processed, it is more
		/// efficient to create a single <see cref="PolygonExtracter"/> instance
		/// and pass it to multiple geometries.
		/// </summary>
		public static IGeometryList GetPolygons(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            GeometryList comps = new GeometryList();
			geometry.Apply(new PolygonExtracter(comps));

			return comps;
		}
		
        /// <summary>
        /// The <see cref="IGeometryVisitor.Filter"/> implementation to filter 
        /// <see cref="Polygon"/> class instances in a geometry collection or list.
        /// </summary>
        /// <param name="geometry">
        /// The <see cref="Geometry"/> to perform the filtering operation on.
        /// </param>
		public virtual void Visit(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            GeometryType geomType = geometry.GeometryType;

            if (geomType == GeometryType.Polygon)
			{
				comps.Add(geometry);
			}
		}
	}
}