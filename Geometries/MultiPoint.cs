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
using iGeospatial.Geometries.Operations;

namespace iGeospatial.Geometries
{
	/// <summary>  
	/// Models a collection of Points.
	/// </summary>
    [Serializable]
    public class MultiPoint : GeometryCollection
	{
		/// <summary>  
		/// Constructs a MultiPoint.
		/// </summary>
		/// <param name="points">         the Points for this MultiPoint
		/// , or null or an empty array to create the empty geometry.
		/// Elements may be empty Points, but not nulls.
		/// </param>
		public MultiPoint(Point[] points, GeometryFactory factory) 
            : base(points, factory)
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
        new public Point this[int index] 
        {
            get 
            {
                return (Point)base[index];
            }

            set 
            {
                base[index] = value;
            }
        }
		
        public override bool IsValid
        {
            get
            {
                return true;
            }
        }
		
		public override DimensionType Dimension
		{
			get 
			{
				return DimensionType.Point;
			}
		}
		
		public override DimensionType BoundaryDimension
		{
			get 
			{
				return DimensionType.Empty;
			}
		}
		
		public override GeometryType GeometryType
		{
			get 
			{
				return GeometryType.MultiPoint;
			}
		}
		
        public override string Name
        {
            get 
            {
                return "MultiPoint";
            }
        }
		
		public override Geometry Boundary
		{
			get 
			{
				return Factory.CreateGeometryCollection(null);
			}
		}
		
		public override bool IsSimple
		{
			get 
			{
				return (new IsSimpleOp()).IsSimple((MultiPoint)this);
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
		
		/// <summary>  Returns the Coordinate at the given position.
		/// 
		/// </summary>
		/// <param name="n"> 
		/// the index of the Coordinate to retrieve, beginning at 0
		/// </param>
		/// <returns>    the nth Coordinate
		/// </returns>
		protected internal virtual Coordinate GetCoordinate(int n)
		{
			return ((Point)m_arrGeometries[n]).Coordinate;
		}
	}
}