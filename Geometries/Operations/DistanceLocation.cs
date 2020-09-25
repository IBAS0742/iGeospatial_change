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

namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// Represents the location of a point on a Geometry.
	/// </summary>
	/// <remarks>
	/// Maintains both the actual point location (which of course
	/// may not be exact) as well as information about the component
	/// and segment index where the point occurs.
	/// <para>
	/// Locations inside area <see cref="Geometry"/> instances will 
	/// not have an associated segment index, so in this case the 
	/// segment index will have the sentinel value of 
	/// <see cref="DistanceLocation.InsideArea"/>.
	/// </para>
	/// </remarks>
	public sealed class DistanceLocation
	{
        #region Public Constants
		
        /// <summary> 
		/// Special value of segment-index for locations inside area 
		/// geometries. These locations do not have an associated 
		/// segment index.
		/// </summary>
		public const int InsideArea = -1;
        
        #endregion
		
        #region Private Fields

		private Geometry   component;
		private int        segIndex;
		private Coordinate pt;
        
        #endregion
		
        #region Constructors and Destructor
		
        /// <overloads>
		/// Initializes a new instance of the <see cref="DistanceLocation"/> class.
		/// </overloads>
		/// <summary> 
		/// Initializes a <see cref="DistanceLocation"/> specifying a point 
		/// on a geometry, as well as the segment that the point is on 
		/// (or <see cref="DistanceLocation.InsideArea"/> if the point is not on a segment).
		/// </summary>
		public DistanceLocation(Geometry component, 
            int segIndex, Coordinate pt)
		{
			this.component = component;
			this.segIndex = segIndex;
			this.pt = pt;
		}
		
		/// <summary> 
		/// Initializes a <see cref="DistanceLocation"/> specifying a point inside 
		/// an area geometry.
		/// </summary>
		public DistanceLocation(Geometry component, Coordinate pt) 
            : this(component, InsideArea, pt)
		{
		}
        
        #endregion
		
        #region Public Properties

		/// <summary> 
		/// Gets the geometry associated with this location.
		/// </summary>
		public Geometry GeometryComponent
		{
			get
			{
				return component;
			}
		}
			
		/// <summary> 
		/// Gets the segment index for this location. If the location 
		/// is inside an area, the index will have the value InsideArea.
		/// </summary>
		/// <value> 
		/// The segment index for the location, or InsideArea.
		/// </value>
		public int SegmentIndex
		{
			get
			{
				return segIndex;
			}
		}
		
        /// <summary> Gets the location.</summary>
		public Coordinate Coordinate
		{
			get
			{
				return pt;
			}
		}
		
        /// <summary> 
        /// Gets whether this DistanceLocation represents a point 
        /// inside an area geometry.
        /// </summary>
		public bool IsInsideArea
		{
			get
			{
				return (segIndex == InsideArea);
			}
		}
        
        #endregion
	}
}