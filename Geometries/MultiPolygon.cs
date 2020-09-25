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

namespace iGeospatial.Geometries
{
    /// <summary>  
    /// This is a <see cref="MultiSurface"/> of <see cref="Polygon"/>s. 
    /// </summary>
    /// <remarks>
    /// By definition of the Simple Feature Specifications, a MultiPolygon
    /// must have the following attributes:
    /// <list type="number">
    /// <item>
    /// <description>
    /// The MultiPolygon is defined as topologically closed.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The interiors of two Polygons that are elements of a 
    /// MultiPolygon may not intersect.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The boundaries of any two Polygons that are elements of a 
    /// MultiPolygon may not ‘cross’ and may touch at only a 
    /// finite number of points.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The MultiPolygon may not have cut lines, spikes or punctures, 
    /// a MultiPolygon is a regular, closed point set.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The interior of a MultiPolygon with more than one Polygon is 
    /// not connected, the number of connected components of the 
    /// interior of a MultiPolygon is equal to the number of Polygons 
    /// in the MultiPolygon.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// MultiPolygons are simple.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// The boundary of a MultiPolygon is a set of closed curves 
    /// corresponding to the boundaries of its element Polygons. 
    /// Each curve in the boundary of the MultiPolygon is in the 
    /// boundary of exactly one element Polygon, and every curve 
    /// in the boundary of an element Polygon is in the boundary 
    /// of the MultiPolygon.
    /// </para>
    /// For a precise definition of MultiPolygons with illustrations, see the 
    /// <see href="http://www.opengis.org/techno/specs.htm">
    /// OpenGIS Simple Features Specification for SQL</see>.
    /// </remarks>
    [Serializable]
    public class MultiPolygon : MultiSurface
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSurface"/> geometry.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="GeometryFactory">geometry factory</see>, which 
        /// created this geometry instance.
        /// </param>
        public MultiPolygon(GeometryFactory factory) : base(factory)
        {
        }
		
		/// <summary>  Constructs a MultiPolygon.
		/// </summary>
		/// <param name="">polygons
		/// the Polygons for this MultiPolygon,
		/// or null or an empty array to create the empty
		/// geometry. Elements may be empty Polygons, but
		/// not nulls. The polygons must conform to the
		/// assertions specified in the <A
		/// HREF="http://www.opengis.org/techno/specs.htm">OpenGIS Simple
		/// Features Specification for SQL</A>.
		/// </param>
		public MultiPolygon(Polygon[] polygons, GeometryFactory factory)
            : base(polygons, factory)
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
        new public Polygon this[int index] 
        {
            get 
            {
                return (Polygon)base[index];
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
				return GeometryType.MultiPolygon;
			}
		}
		
        public override string Name
        {
            get 
            {
                return "MultiPolygon";
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
				if (IsEmpty)
				{
					return Factory.CreateGeometryCollection(null);
				}

                GeometryCollection allRings = new GeometryCollection(Factory);
				for (int i = 0; i < m_arrGeometries.Length; i++)
				{
					Polygon polygon = (Polygon)m_arrGeometries[i];
					Geometry rings  = polygon.Boundary;
					for (int j = 0; j < rings.NumGeometries; j++)
					{
                        allRings.Add(rings.GetGeometry(j));
					}
				}

                LineString[] allRingsArray = new LineString[allRings.Count];

                allRings.CopyTo(allRingsArray);

                return Factory.CreateMultiLineString(allRingsArray);
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