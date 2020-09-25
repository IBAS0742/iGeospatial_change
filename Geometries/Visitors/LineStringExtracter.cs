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
	/// Extracts all the 1-dimensional (<see cref="LineString"/>) components 
	/// from a <see cref="Geometry"/>.
	/// </summary>
	[Serializable]
    public class LineStringExtracter : IGeometryComponentVisitor
	{
        private IGeometryList lines;
		
		/// <summary> 
		/// Constructs a LineStringExtracter filter with a list in which to 
		/// store <see cref="LineString"/> class instances found.
		/// </summary>
		public LineStringExtracter(IGeometryList lines)
		{
			this.lines = lines;
		}
		
		
		/// <summary> 
		/// Extracts the linear components from a single geometry.
		/// If more than one geometry is to be processed, it is more
		/// efficient to create a single <see cref="LineStringExtracter"/> instance
		/// and pass it to multiple geometries.
		/// </summary>
		/// <param name="geometry">the geometry from which to extract linear components
		/// </param>
		/// <returns> the list of linear components
		/// </returns>
		public static IGeometryList GetLines(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            GeometryList lines = new GeometryList();
			geometry.Apply(new LineStringExtracter(lines));

			return lines;
		}
		
        /// <summary>
        /// The <see cref="IGeometryComponentVisitor.Filter"/> implementation to filter 
        /// <see cref="LineString"/> class instances in a geometry collection or list.
        /// </summary>
        /// <param name="geom">
        /// The <see cref="Geometry"/> to perform the filtering operation on.
        /// </param>
        public virtual void Visit(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            GeometryType geomType = geometry.GeometryType;

            if (geomType == GeometryType.LineString ||
                geomType == GeometryType.LinearRing)
			{
				lines.Add(geometry);
			}
		}
	}
}