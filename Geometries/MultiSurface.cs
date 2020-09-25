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

namespace iGeospatial.Geometries
{
	/// <summary>
	/// A MultiSurface is an <see langword="abstract"/> class representing
	/// a two-dimensional geometric collection whose elements are surfaces.
	/// </summary>
	/// <remarks>
	/// By definition, the MultiSurface and therefore its subclasses must
	/// have the following attributes:
	/// <list type="number">
	/// <item>
	/// <description>
    /// The interiors of any two surfaces in a MultiSurface may not intersect.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
    /// The boundaries of any two elements in a MultiSurface may intersect 
    /// at most at a finite number of points.
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	[Serializable]
    public class MultiSurface : GeometryCollection
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSurface"/> geometry.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="GeometryFactory">geometry factory</see>, which 
        /// created this geometry instance.
        /// </param>
        public MultiSurface(GeometryFactory factory) : base(factory)
        {
        }
		
		public MultiSurface(Surface[] surfaces, GeometryFactory factory)
            : base(surfaces, factory)
		{
		}

        /// <summary>
        /// Gets a value specifying whether this <see cref="Geometry"/>
        /// instance is a <see cref="ISurface"/> or not.
        /// </summary>
        /// <value>
        /// This always return true, since <see cref="MultiSurface"/>
        /// is a surface.
        /// </value>
        /// <remarks>
        /// A <see cref="MultiSurface"/> is both a surface and a
        /// collection.
        /// </remarks>
        public override bool IsSurface
        {
            get
            {
                return true;
            }
        }
    }
}
