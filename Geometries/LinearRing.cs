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
	/// Basic implementation of LinearRing.
	/// The first and last point in the coordinate sequence must be equal.
	/// Either orientation of the ring is allowed.
	/// A valid ring must not self-intersect. 
	/// </summary>
    [Serializable]
    public class LinearRing : LineString
	{
//		/// <summary>  
//		/// Constructs a LinearRing with the given points.
//		/// </summary>
//		/// <param name="points"> points forming a closed and simple linestring, or
//		/// null or an empty array to create the empty geometry.
//		/// This array must not contain null elements.
//		/// </param>
//		/// <param name="precisionModel"> the specification of the grid of allowable points
//		/// for this LinearRing
//		/// </param>
//		/// <param name="SRID"> the ID of the Spatial Reference System used by this
//		/// LinearRing
//		/// </param>
//		/// <deprecated> Use GeometryFactory instead
//		/// </deprecated>
//		public LinearRing(Coordinate[] points, 
//            PrecisionModel precisionModel, int SRID) 
//            : this(CoordinateSequenceFactory.Instance().Create(points == null ? 
//            new Coordinate[]{} : points), 
//            new GeometryFactory(precisionModel, SRID, CoordinateSequenceFactory.Instance()))
//		{
//		}
		
		
		/// <summary>  Constructs a LinearRing with the given points.
		/// 
		/// </summary>
		/// <param name="points">         points forming a closed and simple linestring, or
		/// null or an empty array to create the empty geometry.
		/// This array must not contain null elements.
		/// 
		/// </param>
		public LinearRing(ICoordinateList points, GeometryFactory factory) 
            : base(points, factory)
		{
			ValidateConstruction(true);
		}
		
        public override bool IsClosed
        {
            get
            {
                return true;
            }
        }
		
		public override bool IsSimple
		{
			get 
			{
				return true;
			}
		}
		
		public override GeometryType GeometryType
		{
			get 
			{
				return GeometryType.LinearRing;
			}
		}
		
        public override string Name
        {
            get 
            {
                return "LinearRing";
            }
        }
		
        protected override bool IsEquivalentType(Geometry other)
        {
            return (other.GeometryType == GeometryType.LinearRing);
        }
	}
}