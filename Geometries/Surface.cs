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
    /// A two-dimensional geometric object. It is the base interface of all
    /// the two-dimensional geometric objects, such as polygon.
    /// </summary>
    /// <remarks>
    /// The Open Geospatial Consortium Abstract Specification for Simple 
    /// Feature Geometry defines a simple surface as consisting of a 
    /// single 'patch' that is associated with one 'exterior boundary' 
    /// and 0 or more 'interior' boundaries. 
    /// <para>
    /// Simple surfaces in three-dimensional space are isomorphic to 
    /// planar surfaces. Polyhedral surfaces are formed by 'stitching' 
    /// together simple surfaces along their boundaries, polyhedral 
    /// surfaces in three-dimensional space may not be planar as a
    /// whole.
    /// </para>
    /// The boundary of a simple surface is the set of closed curves 
    /// corresponding to its 'exterior' and 'interior' boundaries.
    /// <para>
    /// For a discussion of the definition of surfaces, 
    /// <see href="http://www.opengis.org/techno/specs.htm">OpenGIS
    /// Simple Features Specification for SQL</see>.
    /// </para>
    /// In this implementation, we have two kinds of two-dimensional
    /// surfaces; those without interior bounds or rings, such are the
    /// rectangles, rounded rectangle circle, ellipse and those with
    /// the interior rings, the polygons.
    /// </remarks>
    /// <seealso cref="Polygon"/>
    [Serializable]
    public abstract class Surface : Geometry
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="Surface"/> geometry.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="GeometryFactory">geometry factory</see>, which 
        /// created this geometry instance.
        /// </param>
        protected Surface(GeometryFactory factory) : base(factory)
		{
		}

        /// <summary>
        /// 
        /// </summary>
        public override bool IsSurface
        {
            get
            {
                return true;
            }
        }
    }
}
