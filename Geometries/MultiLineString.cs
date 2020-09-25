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
using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries.Operations;

namespace iGeospatial.Geometries
{
	/// <summary>  
	/// Basic implementation of MultiLineString.
	/// </summary>
    [Serializable]
    public class MultiLineString : GeometryCollection
	{
		/// <summary>  Constructs a MultiLineString.
		/// 
		/// </summary>
		/// <param name="">lineStrings
		/// the LineStrings for this MultiLineString,
		/// or null or an empty array to create the empty
		/// geometry. Elements may be empty LineStrings,
		/// but not nulls.
		/// </param>
		public MultiLineString(LineString[] lineStrings, GeometryFactory factory) 
            : base(lineStrings, factory)
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
        new public LineString this[int index] 
        {
            get 
            {
                return (LineString)base[index];
            }

            set 
            {
                base[index] = value;
            }
        }
		
        public virtual bool IsClosed
        {
            get
            {
                if (IsEmpty)
                {
                    return false;
                }
                for (int i = 0; i < m_arrGeometries.Length; i++)
                {
                    if (!((LineString) m_arrGeometries[i]).IsClosed)
                    {
                        return false;
                    }
                }
                return true;
            }			
        }
		
		public override DimensionType Dimension
		{
			get 
			{
				return DimensionType.Curve;
			}
		}
		
		public override DimensionType BoundaryDimension
		{
			get 
			{
				if (this.IsClosed)
				{
					return DimensionType.Empty;
				}

				return DimensionType.Point;
			}
		}
		
		public override GeometryType GeometryType
		{
			get 
			{
				return GeometryType.MultiLineString;
			}
		}
		
        public override string Name
        {
            get 
            {
                return "MultiLineString";
            }
        }
		
		public override bool IsSimple
		{
			get 
			{
				return (new IsSimpleOp()).IsSimple((MultiLineString)this);
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
				GeometryGraph g = new GeometryGraph(0, this);
				Coordinate[] pts = g.GetBoundaryPoints();
				return Factory.CreateMultiPoint(pts);
			}
		}
        
        /// <summary>
        /// Creates a <see cref="MultiLineString"/> in the reverse
        /// order to this object.
        /// </summary>
        /// <returns>
        /// A <see cref="MultiLineString"/> in the reverse order.
        /// </returns>
        /// <remarks>
        /// Both the order of the component <see cref="LineString"/>s
        /// and the order of their coordinate sequences are reversed.
        /// </remarks>
        public virtual MultiLineString ReverseAll()
        {
            int nLines            = this.Count;
            LineString[] revLines = new LineString[nLines];

            for (int i = 0; i < nLines; i++) 
            {
                revLines[nLines - 1 - i] = (this[i]).ReverseAll();
            }

            return this.Factory.CreateMultiLineString(revLines);
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
