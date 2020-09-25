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

namespace iGeospatial.Geometries
{
	/// <summary>
	/// Summary description for MultiRectangle.
	/// </summary>
	[Serializable]
    public class MultiRectangle : MultiSurface
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiRectangle"/> geometry.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="GeometryFactory">geometry factory</see>, which 
        /// created this geometry instance.
        /// </param>
        public MultiRectangle(GeometryFactory factory) : base(factory)
        {
        }
		
        public MultiRectangle(Rectangle[] rectangles, GeometryFactory factory)
            : base(rectangles, factory)
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="Geometry"/> element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the
        /// <see cref="Geometry"/> element to get or set.</param>
        /// <value>
        /// The <see cref="Geometry"/> element at the specified <paramref name="index"/>.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException"><para>
        /// The property is set and the <see cref="GeometryCollection"/> is read-only.
        /// </para><para>-or-</para><para>
        /// The property is set, the <b>GeometryCollection</b> already contains the
        /// specified element at a different index, and the <b>GeometryCollection</b>
        /// ensures that all elements are unique.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.this"/> for details.</remarks>
        new public Rectangle this[int index] 
        {
            get 
            {
                return (Rectangle)base[index];
            }

            set 
            {
                base[index] = value;
            }
        }
		
        public override DimensionType Dimension
        {
            get 
            {
                return DimensionType.Surface;
            }
        }
		
        public override DimensionType BoundaryDimension
        {
            get 
            {
                return DimensionType.Curve;
            }
        }
		
        public override GeometryType GeometryType
        {
            get 
            {
                return GeometryType.MultiRectangle;
            }
        }
		
        public override string Name
        {
            get 
            {
                return "MultiRectangle";
            }
        }
		
        public override bool IsSimple
        {
            get 
            {
                return true;
            }
        }
		
        public override Geometry Boundary
        {
            get 
            {
                return null;
            }
        }
		
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentType(other))
            {
                return false;
            }

            return base.EqualsExact(other, tolerance);
        }
	}
}
