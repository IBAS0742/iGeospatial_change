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
	/// Extracts all the 0-dimensional (<see cref="Point"/>) components 
	/// from a <see cref="Geometry"/>.
	/// </summary>
	[Serializable]
    public class PointExtracter : IGeometryVisitor
	{
        private IGeometryList pts;

		/// <summary> 
		/// Constructs a PointExtracter filter with a list in which to store 
		/// <see cref="Point"/> instances found.
		/// </summary>
		public PointExtracter(IGeometryList pts)
		{
			this.pts = pts;
		}
		
		/// <summary> Returns the <see cref="Point"/> components from a single geometry.
		/// If more than one geometry is to be processed, it is more
		/// efficient to create a single <see cref="PointExtracterFilter"/> instance
		/// and pass it to multiple geometries.
		/// </summary>
		public static IGeometryList GetPoints(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            GeometryList pts = new GeometryList();
			geometry.Apply(new PointExtracter(pts));

			return pts;
		}
		
        /// <summary>
        /// The <see cref="IGeometryVisitor.Filter"/> implementation to filter 
        /// <see cref="Point"/> class instances in a geometry collection or list.
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

            if (geometry.GeometryType == GeometryType.Point)
			{
				pts.Add(geometry);
			}
		}
	}
}