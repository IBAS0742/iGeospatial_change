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

namespace iGeospatial.Geometries.LinearReferencing
{
	/// <summary> 
	/// Represents a location along a <see cref="LineString"/> or 
	/// <see cref="MultiLineString"/>.
	/// </summary>
	/// <remarks>
	/// The referenced geometry is not maintained within this location, but must 
	/// be provided for operations which require it.
	/// <para>
	/// Various methods are provided to manipulate the location value and query 
	/// the geometry it references.
	/// </para>
	/// </remarks>
    [Serializable]
    public class LinearLocation : System.IComparable, System.ICloneable
	{
        #region Private Fields

        private int    componentIndex;
        private int    segmentIndex;
        private double segmentFraction;
        
        #endregion
		
        #region Constructors and Destructor

        /// <summary> 
        /// Creates a location referring to the start of a linear geometry.
        /// </summary>
        public LinearLocation()
        {
        }
		
        public LinearLocation(int segmentIndex, double segmentFraction)
            : this(0, segmentIndex, segmentFraction)
        {
        }
		
        public LinearLocation(int componentIndex, int segmentIndex, 
            double segmentFraction)
        {
            this.componentIndex  = componentIndex;
            this.segmentIndex    = segmentIndex;
            this.segmentFraction = segmentFraction;

            Normalize();
        }
        
        #endregion
		
        #region Public Properties

		/// <summary> 
		/// Gets the component index for this location.
		/// </summary>
		/// <value> A number specifying the component index.</value>
		public int ComponentIndex
		{
			get
			{
				return componentIndex;
			}
		}
			
		/// <summary> 
		/// Gets the segment index for this location
		/// </summary>
		/// <value> A number specifying the segment index.</value>
		public int SegmentIndex
		{
			get
			{
				return segmentIndex;
			}
		}
			
		/// <summary> 
		/// Gets the segment fraction for this location
		/// </summary>
		/// <value> A number specifying the segment fraction.</value>
		public double SegmentFraction
		{
			get
			{
				return segmentFraction;
			}
		}
			
		/// <summary> 
		/// Gets a value that indicates whether this location refers to 
		/// a vertex.
		/// </summary>
		/// <value> 
		/// This property returns <see langword="true"/> if the location is 
		/// a vertex; otherwise, <see langword="false"/>.
		/// </value>
		public bool Vertex
		{
			get
			{
				return segmentFraction <= 0.0 || segmentFraction >= 1.0;
			}
		}
        
        #endregion
			
        #region Public Methods

        /// <summary> 
		/// Gets or sets the value of this location to refer the end of a 
		/// linear geometry.
		/// </summary>
		/// <param name="geometry">The linear geometry to set.</param>
		public void SetToEnd(Geometry geometry)
		{
            componentIndex = geometry.NumGeometries - 1;

            LineString lastLine = 
                (LineString)geometry.GetGeometry(componentIndex);
            segmentIndex        = lastLine.NumPoints - 1;
				
            segmentFraction = 1.0;
		}
			
		/// <summary> Gets a location which refers to the end of a linear <see cref="Geometry"/>.</summary>
		/// <param name="linear">the linear geometry
		/// </param>
		/// <returns> a new <tt>LinearLocation</tt>
		/// </returns>
		public static LinearLocation GetEndLocation(Geometry linear)
		{
			// assert: linear is LineString or MultiLineString
			LinearLocation loc = new LinearLocation();
			loc.SetToEnd(linear);
			
            return loc;
		}
		
		/// <summary> Computes the <see cref="Coordinate"/> of a point a given fraction
		/// along the line segment <tt>(p0, p1)</tt>.
		/// If the fraction is greater than 1.0 the last
		/// point of the segment is returned.
		/// If the fraction is less than or equal to 0.0 the first point
		/// of the segment is returned.
		/// 
		/// </summary>
		/// <param name="p0">the first point of the line segment
		/// </param>
		/// <param name="p1">the last point of the line segment
		/// </param>
		/// <param name="frac">the length to the desired point
		/// </param>
		/// <returns> the <tt>Coordinate</tt> of the desired point
		/// </returns>
		public static Coordinate PointAlongSegmentByFraction(Coordinate p0, 
            Coordinate p1, double frac)
		{
			if (frac <= 0.0)
				return p0;
			if (frac >= 1.0)
				return p1;
			
			double x = (p1.X - p0.X) * frac + p0.X;
			double y = (p1.Y - p0.Y) * frac + p0.Y;

			return new Coordinate(x, y);
		}
		
		/// <summary> 
		/// Ensures the indexes are valid for a given linear <see cref="Geometry"/>.
		/// </summary>
		/// <param name="linear">A linear geometry.</param>
		public void Clamp(Geometry linear)
		{
			if (componentIndex >= linear.NumGeometries)
			{
				SetToEnd(linear);

				return;
			}

			if (segmentIndex >= linear.NumPoints)
			{
				LineString line = (LineString)linear.GetGeometry(componentIndex);
				segmentIndex    = line.NumPoints - 1;
				segmentFraction = 1.0;
			}
		}
		/// <summary> Snaps the value of this location to
		/// the nearest vertex on the given linear <see cref="Geometry"/>,
		/// if the vertex is closer than <tt>maxDistance</tt>.
		/// 
		/// </summary>
		/// <param name="linearGeom">a linear geometry
		/// </param>
		/// <param name="minDistance">the minimum allowable distance to a vertex
		/// </param>
		public void SnapToVertex(Geometry linearGeom, double minDistance)
		{
			if (segmentFraction <= 0.0 || segmentFraction >= 1.0)
				return;

			double segLen     = GetSegmentLength(linearGeom);
			double lenToStart = segmentFraction * segLen;
			double lenToEnd   = segLen - lenToStart;

			if (lenToStart <= lenToEnd && lenToStart < minDistance)
			{
				segmentFraction = 0.0;
			}
			else if (lenToEnd <= lenToStart && lenToEnd < minDistance)
			{
				segmentFraction = 1.0;
			}
		}
		
		/// <summary> 
		/// Gets the length of the segment in the given Geometry containing 
		/// this location.
		/// </summary>
		/// <param name="linearGeom">A linear geometry.</param>
		/// <returns>The length of the segment.</returns>
		public double GetSegmentLength(Geometry linearGeom)
		{
			LineString lineComp = 
                (LineString)linearGeom.GetGeometry(componentIndex);
			
			// ensure segment index is valid
			int segIndex = segmentIndex;
			if (segmentIndex >= lineComp.NumPoints - 1)
				segIndex = lineComp.NumPoints - 2;
			
			Coordinate p0 = lineComp.GetCoordinate(segIndex);
			Coordinate p1 = lineComp.GetCoordinate(segIndex + 1);

			return p0.Distance(p1);
		}
		
		/// <summary> 
		/// Gets the <see cref="Coordinate"/> along the
		/// given linear <see cref="Geometry"/> which is
		/// referenced by this location.
		/// 
		/// </summary>
		/// <param name="linearGeom">a linear geometry
		/// </param>
		/// <returns> the <tt>Coordinate</tt> at the location
		/// </returns>
		public Coordinate GetCoordinate(Geometry linearGeom)
		{
			LineString lineComp = 
                (LineString)linearGeom.GetGeometry(componentIndex);

			Coordinate p0 = lineComp.GetCoordinate(segmentIndex);
			if (segmentIndex >= lineComp.NumPoints - 1)
				return p0;
			Coordinate p1 = lineComp.GetCoordinate(segmentIndex + 1);

			return PointAlongSegmentByFraction(p0, p1, segmentFraction);
		}
		
		/// <summary> Tests whether this location refers to a valid
		/// location on the given linear <see cref="Geometry"/>.
		/// 
		/// </summary>
		/// <param name="linearGeom">a linear geometry
		/// </param>
		/// <returns> true if this location is valid
		/// </returns>
		public bool IsValid(Geometry linearGeom)
		{
			if (componentIndex < 0 || componentIndex >= linearGeom.NumGeometries)
				return false;
			
			LineString lineComp = 
                (LineString)linearGeom.GetGeometry(componentIndex);

			if (segmentIndex < 0 || segmentIndex > lineComp.NumGeometries)
				return false;
			if (segmentIndex == lineComp.NumGeometries && segmentFraction != 0.0)
				return false;
			
			if (segmentFraction < 0.0 || segmentFraction > 1.0)
				return false;

			return true;
		}
		
		/// <summary>  
		/// Compares this object with the specified index values for order.
		/// </summary>
		/// <param name="componentIndex1">a component index
		/// </param>
		/// <param name="segmentIndex1">a segment index
		/// </param>
		/// <param name="segmentFraction1">a segment fraction
		/// </param>
		/// <returns>    
		/// A negative integer, zero, or a positive integer as this 
		/// <see cref="LineStringLocation"/> is less than, equal to, or greater 
		/// than the specified locationValues.
		/// </returns>
		public int CompareLocationValues(int componentIndex1, 
            int segmentIndex1, double segmentFraction1)
		{
			// compare component indices
			if (componentIndex < componentIndex1)
				return - 1;
			if (componentIndex > componentIndex1)
				return 1;
			// compare segments
			if (segmentIndex < segmentIndex1)
				return - 1;
			if (segmentIndex > segmentIndex1)
				return 1;
			// same segment, so compare segment fraction
			if (segmentFraction < segmentFraction1)
				return - 1;
			if (segmentFraction > segmentFraction1)
				return 1;
			// same location
			return 0;
		}
		
		/// <summary>  
		/// Compares two sets of location values for order.
		/// </summary>
		/// <param name="componentIndex0">a component index
		/// </param>
		/// <param name="segmentIndex0">a segment index
		/// </param>
		/// <param name="segmentFraction0">a segment fraction
		/// </param>
		/// <param name="componentIndex1">another component index
		/// </param>
		/// <param name="segmentIndex1">another segment index
		/// </param>
		/// <param name="segmentFraction1">another segment fraction
		/// </param>
		/// <returns>    a negative integer, zero, or a positive integer
		/// as the first set of location values
		/// is less than, equal to, or greater than the second set of locationValues
		/// </returns>
		public static int CompareLocationValues(int componentIndex0, 
            int segmentIndex0, double segmentFraction0, int componentIndex1, 
            int segmentIndex1, double segmentFraction1)
		{
			// compare component indices
			if (componentIndex0 < componentIndex1)
				return - 1;
			if (componentIndex0 > componentIndex1)
				return 1;
			// compare segments
			if (segmentIndex0 < segmentIndex1)
				return - 1;
			if (segmentIndex0 > segmentIndex1)
				return 1;
			// same segment, so compare segment fraction
			if (segmentFraction0 < segmentFraction1)
				return - 1;
			if (segmentFraction0 > segmentFraction1)
				return 1;
			// same location
			return 0;
		}
        
        #endregion
		
        #region Private Methods

        /// <summary> Ensures the individual values are locally valid.
        /// Does <b>not</b> ensure that the indexes are valid for
        /// a particular linear geometry.
        /// 
        /// </summary>
        /// <seealso cref="clamp">
        /// </seealso>
        private void Normalize()
        {
            if (segmentFraction < 0.0)
            {
                segmentFraction = 0.0;
            }
            if (segmentFraction > 1.0)
            {
                segmentFraction = 1.0;
            }
			
            if (componentIndex < 0)
            {
                componentIndex = 0;
                segmentIndex = 0;
                segmentFraction = 0.0;
            }
            if (segmentIndex < 0)
            {
                segmentIndex = 0;
                segmentFraction = 0.0;
            }
            if (segmentFraction == 1.0)
            {
                segmentFraction = 0.0;
                segmentIndex += 1;
            }
        }
        
        #endregion
		
        #region IComparable Members

        /// <summary>
        /// Compares this object with the specified object for order.
        /// </summary>
        /// <param name="other">
        /// The <see cref="LineStringLocation"/> with which this 
        /// <see cref="Coordinate"/>
        /// is being compared
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this 
        /// <see cref="LineStringLocation"/>
        /// is less than, equal to, or greater than the specified 
        /// <see cref="LineStringLocation"/>
        /// </returns>
        public int CompareTo(LinearLocation other)
        {
            // compare component indices
            if (componentIndex < other.componentIndex)
                return - 1;
            if (componentIndex > other.componentIndex)
                return 1;
            // compare segments
            if (segmentIndex < other.segmentIndex)
                return - 1;
            if (segmentIndex > other.segmentIndex)
                return 1;
            // same segment, so compare segment fraction
            if (segmentFraction < other.segmentFraction)
                return - 1;
            if (segmentFraction > other.segmentFraction)
                return 1;

            // same location
            return 0;
        }

        /// <summary>
        /// Compares this object with the specified object for order.
        /// </summary>
        /// <param name="o">
        /// The <see cref="LineStringLocation"/> with which this 
        /// <see cref="Coordinate"/>
        /// is being compared
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this 
        /// <see cref="LineStringLocation"/>
        /// is less than, equal to, or greater than the specified 
        /// <see cref="LineStringLocation"/>
        /// </returns>
        public int CompareTo(object o)
        {
            LinearLocation other = (LinearLocation)o;

            // compare component indices
            if (componentIndex < other.componentIndex)
                return - 1;
            if (componentIndex > other.componentIndex)
                return 1;
            // compare segments
            if (segmentIndex < other.segmentIndex)
                return - 1;
            if (segmentIndex > other.segmentIndex)
                return 1;
            // same segment, so compare segment fraction
            if (segmentFraction < other.segmentFraction)
                return - 1;
            if (segmentFraction > other.segmentFraction)
                return 1;

            // same location
            return 0;
        }
        
        #endregion
		
        #region ICloneable Members

		/// <summary>
		/// Copies this location
		/// </summary>
		/// <returns>A copy of this location.</returns>
		public LinearLocation Clone()
		{
			return new LinearLocation(segmentIndex, segmentFraction);
		}

		/// <summary>
		/// Copies this location
		/// </summary>
		/// <returns>A copy of this location.</returns>
		object ICloneable.Clone()
		{
			return new LinearLocation(segmentIndex, segmentFraction);
		}
        
        #endregion
	}
}