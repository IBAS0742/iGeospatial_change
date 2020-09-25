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
    /// This is an <see langword="abstract"/> class defining the base interface 
    /// for all curves. A curve is one-dimensional geometric object. 
    /// <para>
    /// Curves may not be degenerate. That is, non-empty curves must have at 
    /// least 2 points, and no two consecutive points may be equal. 
    /// </para>
    /// </summary>
    /// <remarks>
    /// A curve is the basis for 1-dimensional geometry.
    /// It is usually stored as a sequence of points, with the subtype of
    /// curve specifying the form of the interpolation between points. 
    /// For instance, the <see cref="LineString"/> is a subclass of
    /// curve that uses linear interpolation between points.
    /// <para>
    /// A curve is simple if the it does not pass through the same 
    /// point more than once.
    /// </para>  
    /// A curve is closed if its start point is equal to its end point.
    /// <para>
    /// The boundary of a closed curve is the empty geometry. The boundary of a
    /// non-closed curve is the two endpoints.
    /// </para>
    /// <para>
    /// A curve that is simple and closed is a ring.
    /// </para>
    /// For a precise definition of a curve, see the 
    /// <see href="http://www.opengis.org/techno/specs.htm">OpenGIS Simple 
    /// Features Specification for SQL.</see>
    /// </remarks>
    /// <seealso cref="LineString"/>
    /// <seealso cref="LinearRing"/>
    [Serializable]
    public abstract class Curve : Geometry
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="Curve"/> geometry.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="GeometryFactory">geometry factory</see>, which 
        /// created this curve instance.
        /// </param>
		protected Curve(GeometryFactory factory) : base(factory)
		{
		}
	}
}
